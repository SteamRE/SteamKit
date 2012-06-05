using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SteamKit2.Blob
{

    /// <summary>
    /// Represents the base blob reader that can process a binary blob.
    /// </summary>
    public class BlobReader : IDisposable
    {
        /// <summary>
        /// The field key type.
        /// </summary>
        public enum FieldKeyType
        {
            /// <summary>
            /// String key type.
            /// </summary>
            StringType,
            /// <summary>
            /// Int key type.
            /// </summary>
            IntType
        }

        private const int BlobHeaderLength = 10;
        private const int FieldHeaderLength = 6;
        private const int CompressedHeaderLength = 10;
        private const int EncryptedHeaderLength = 20;

        private PeekableStream input;

        private long length;
        private long mark;

        private byte[] aesKey;


        /// <summary>
        /// Occurs when a blob begins.
        /// </summary>
        public event Action<EAutoPreprocessCode, ECacheState> Blob;
        /// <summary>
        /// Occurs when a blob ends.
        /// </summary>
        public event Action EndBlob;

        /// <summary>
        /// Occurs when a blob field begins.
        /// </summary>
        public event Action<FieldKeyType, byte[], int> Field;
        /// <summary>
        /// Occurs when a blob field ends.
        /// </summary>
        public event Action EndField;

        /// <summary>
        /// Occurs when a blob field value is parsed.
        /// </summary>
        public event Action<byte[]> FieldValue;

        /// <summary>
        /// Occurs when blob spare data is parsed.
        /// </summary>
        public event Action<byte[]> Spare;


        private BlobReader()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobReader"/> class.
        /// </summary>
        /// <param name="input">The input stream to process.</param>
        protected BlobReader(Stream input)
            : this()
        {
            if ( !input.CanRead || !input.CanSeek )
                throw new ArgumentException( "Input stream must be readable and seekable." );

            this.input = new PeekableStream(input);
            this.length = input.Length;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            input.Close();
            input.Dispose();
        }


        /// <summary>
        /// Creates a new instance of a <see cref="BlobReader"/> for the given input stream.
        /// </summary>
        /// <param name="inputStream">The input stream to process.</param>
        /// <returns>A new <see cref="BlobReader"/> instance.</returns>
        public static BlobReader Create(Stream inputStream)
        {
            return new BlobReader(inputStream);
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BlobReader"/> for the given input file.
        /// </summary>
        /// <param name="fileName">The input file to process.</param>
        /// <returns>A new <see cref="BlobReader"/> instance.</returns>
        public static BlobReader Create(string fileName)
        {
            return Create(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 0x1000, FileOptions.SequentialScan));
        }


        /// <summary>
        /// Sets the AES encryption key for this blob.
        /// </summary>
        /// <param name="key">The key.</param>
        public void SetKey(byte[] key)
        {
            this.aesKey = key;
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <returns><c>true</c> if this blob could be processed; otherwise, file.</returns>
        public bool Process()
        {
            return TryReadBlob(UInt32.MaxValue);
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


        private bool TryReadBlob(UInt32 lengthHint)
        {
            if (!CanRead(BlobHeaderLength) || lengthHint < BlobHeaderLength)
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
                throw new InvalidDataException( "Corrupted blob" );
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

                bool result = TryReadBlob((uint)serialized);

                input.ReadBytes( 40 ); // read and dispose HMACs
                // todo: validate them?

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

                if (!TryReadBlob((uint)dataLength))
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
