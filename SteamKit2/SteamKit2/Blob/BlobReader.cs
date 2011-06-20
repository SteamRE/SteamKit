using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SteamKit2
{
    public enum FieldKeyType
    {
        StringType,
        IntType
    }

    public class BlobReader : IDisposable
    {
        private const int BlobHeaderLength = 10;
        private const int FieldHeaderLength = 6;
        private const int CompressedHeaderLength = 10;
        private const int EncryptedHeaderLength = 20;

        private PeekableStream input;

        private long length;
        private long mark;

        private byte[] aesKey;


        public delegate void BlobDelegate(EAutoPreprocessCode processCode, ECacheState cacheState);
        public event BlobDelegate Blob;

        public delegate void EndBlobDelegate();
        public event EndBlobDelegate EndBlob;

        public delegate void FieldDelegate(FieldKeyType type, byte[] key, int fieldSize);
        public event FieldDelegate Field;

        public delegate void EndFiendDelegate();
        public event EndFiendDelegate EndField;


        public delegate void FieldValueDelegate(byte[] data);
        public event FieldValueDelegate FieldValue;

        public delegate void SpareDelegate(byte[] spare);
        public event SpareDelegate Spare;


        private BlobReader()
        {
        }

        protected BlobReader(Stream input)
            : this()
        {
            Debug.Assert(input.CanRead, "Stream must be readable");
            Debug.Assert(input.CanSeek, "Stream must be seekable");

            this.input = new PeekableStream(input);
            this.length = input.Length;
        }

        public virtual void Dispose()
        {
            input.Close();
            input.Dispose();
        }


        public static BlobReader Create(Stream inputStream)
        {
            return new BlobReader(inputStream);
        }

        public static BlobReader Create(string fileName)
        {
            return Create(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 0x1000, FileOptions.SequentialScan));
        }


        public void SetKey(byte[] key)
        {
            this.aesKey = key;
        }

        public bool Process()
        {
            return TryReadBlob();
        }
        

        private void Mark(int size)
        {
            if (input.CanSeek)
                mark = input.Position;

            input.Mark(size);
        }

        private void Reset()
        {
            input.Reset();
        }

        private bool CanRead(int toRead)
        {
            if (input.CanSeek)
                return input.Length - input.Position >= toRead;

            return true;
        }

        private bool CanReadMarked(int toRead)
        {
            if (input.CanSeek)
                return input.Length - mark >= toRead;

            return true;
        }


        private bool TryReadBlob()
        {
            if (!CanRead(BlobHeaderLength))
                return false;

            ECacheState cachestate;
            EAutoPreprocessCode process;
            Int32 serialized, spare;

            Mark(BlobHeaderLength);

            try
            {
                cachestate = (ECacheState)input.ReadByte();
                process = (EAutoPreprocessCode)input.ReadByte();

                serialized = input.ReadInt32();
                spare = input.ReadInt32();
            }
            catch (Exception)
            {
                Reset();
                throw new Exception("Corrupted blob");
            }

            if (!BlobUtil.IsValidCacheState(cachestate) || !BlobUtil.IsValidProcess(process) ||
                    serialized < 0 || spare < 0 ||
                    !CanReadMarked(serialized))
            {
                Reset();
                return false;
            }

            serialized -= BlobHeaderLength;

            if (process != EAutoPreprocessCode.eAutoPreprocessCodePlaintext)
            {
                PeekableStream originalStream = input;
                object extendedData = null;

                input = HandleProcessCode(process, ref serialized, out extendedData);
                bool result = TryReadBlob();

                // todo: deal with extra HMAC data, since it's now expected

                input.Close();
                input = originalStream;

                return result;
            }

            if(Blob != null)
                Blob(process, cachestate);

            ProcessFields(serialized);

            if (spare > 0)
            {
                byte[] spareData = new byte[spare];
                input.Read(spareData, 0, spare);

                if(Spare != null)
                    Spare(spareData);
            }

            if(EndBlob != null)
                EndBlob();

            return true;
        }

        private void ProcessFields(int serializedSize)
        {
            while (serializedSize > FieldHeaderLength && CanRead(FieldHeaderLength))
            {
                Int16 descriptorLength;
                Int32 dataLength;

                byte[] descriptor;

                Mark(FieldHeaderLength);

                try
                {
                    descriptorLength = input.ReadInt16();
                    dataLength = input.ReadInt32();

                    if ( descriptorLength <= 0 || !CanRead( descriptorLength + dataLength ) )
                    {
                        Reset();
                        throw new Exception("Corrupted field lengths");
                    }

                    serializedSize -= FieldHeaderLength;

                    descriptor = new byte[descriptorLength];
                    input.Read(descriptor, 0, descriptorLength);

                    serializedSize -= descriptorLength;
                }
                catch (Exception)
                {
                    Reset();
                    throw new Exception("Corrupted field");
                }

                if(Field != null)
                    Field((BlobUtil.IsIntDescriptor(descriptor) ? FieldKeyType.IntType : FieldKeyType.StringType),
                            descriptor, dataLength);

                if (!TryReadBlob())
                {
                    byte[] data = new byte[dataLength];
                    input.Read(data, 0, dataLength);

                    if(FieldValue != null)
                        FieldValue(data);
                }

                if(EndField != null)
                    EndField();

                serializedSize -= dataLength;
            }

            if(serializedSize != 0)
                throw new Exception( "Data left untouched in Field");
        }

        private PeekableStream HandleProcessCode(EAutoPreprocessCode process, ref int serialized, out object extended)
        {
            try
            {
                switch (process)
                {
                    case EAutoPreprocessCode.eAutoPreprocessCodeCompressed:
                        {
                            Mark(CompressedHeaderLength);

                            Int32 decompressedSize, unknown;
                            Int16 level;

                            decompressedSize = input.ReadInt32();
                            unknown = input.ReadInt32();
                            level = input.ReadInt16();

                            input.ReadInt16(); // skip zlib header
                            serialized -= CompressedHeaderLength + 2 /* for zlib header */;

                            extended = level;

                            DeflateStream deflate = new DeflateStream(new FramingStream(input, serialized), CompressionMode.Decompress, true);

                            return new PeekableStream(new BufferedStream(deflate, 0x1000));
                        }
                    case EAutoPreprocessCode.eAutoPreprocessCodeEncrypted:
                        {
                            Mark( EncryptedHeaderLength );

                            int decryptedSize;
                            byte[] aesIv;

                            decryptedSize = input.ReadInt32();
                            aesIv = input.ReadBytes( 16 );

                            serialized -= EncryptedHeaderLength;
                            // account for one hmac
                            // (but there's actually two!)
                            // how does this work? blatently lucky code!!
                            serialized -= 20;

                            extended = aesIv;

                            RijndaelManaged aes = new RijndaelManaged();
                            aes.BlockSize = aes.KeySize = 128;
                            aes.Mode = CipherMode.CBC;

                            ICryptoTransform aesTransform = aes.CreateDecryptor( aesKey, aesIv );
                            CryptoStream decryptStream = new CryptoStream( new FramingStream( input, serialized ), aesTransform, CryptoStreamMode.Read );

                            return new PeekableStream( new BufferedStream( decryptStream, 0x1000 ) );
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception processing code: " + ex);

                Reset();
                throw new Exception("Corrupted blob while handling process code");
            }

            throw new Exception("Process code went unhandled");
        }

    }
}
