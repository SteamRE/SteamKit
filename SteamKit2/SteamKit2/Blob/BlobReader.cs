using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SteamKit2.Blob
{
    // BlobReader that reads sequentially
    // ReadBlobHeader() from constructor, call while CanReadField() ReadFieldHeader() ...
    public class BlobReader : IDisposable
    {
        public class InvalidBlobException : Exception
        {
            public InvalidBlobException(string message) : base(message) { }
        }

        private bool ownsSource;
        private Stream source;
        private Stack<Stream> sourceStack;

        private ECacheState cacheState;
        private EAutoPreprocessCode processCode;
        private int bytesAvailable, spareAvailable;

        public ECacheState CacheState { get { return cacheState; } }
        public EAutoPreprocessCode ProcessCode { get { return processCode; } }

        public int BytesAvailable { get { return bytesAvailable; } }
        public int SpareAvailable { get { return spareAvailable; } }

        private int keyBytes, dataBytes;

        public int FieldKeyBytes { get { return keyBytes; } }
        public int FieldDataBytes { get { return dataBytes; } }

        private byte[] keyBuffer;
        public byte[] ByteKey { get { return keyBuffer; } }

        private const int BlobHeaderLength = 10;
        internal const int FieldHeaderLength = 6;
        private const int CompressedHeaderLength = 10;
        private const int EncryptedHeaderLength = 20;


        public static BlobReader CreateFrom(string fileName)
        {
            return new BlobReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 0x10000, FileOptions.SequentialScan), true);
        }

        public static BlobReader CreateFrom(Stream inputSteam)
        {
            return new BlobReader(inputSteam, false);
        }


        private BlobReader(Stream blobSource, bool ownsSource = true)
        {
            this.sourceStack = null;
            this.source = blobSource;
            this.keyBuffer = new byte[4];

            ReadBlobHeader();
        }

        private void ReadBlobHeader()
        {
            cacheState = (ECacheState)source.ReadByte();
            processCode = (EAutoPreprocessCode)source.ReadByte();

            bytesAvailable = source.ReadInt32();
            spareAvailable = source.ReadInt32();

            if (!BlobUtil.IsValidCacheState(cacheState) || !BlobUtil.IsValidProcess(processCode))
            {
                throw new InvalidBlobException("Invalid blob header");
            }

            TakeBytes(BlobHeaderLength);
            UnpackBlobIfNeeded();
        }

        public void Dispose()
        {
            if(ownsSource)
                this.source.Close();

            if (sourceStack != null)
                foreach (var stream in sourceStack)
                    stream.Close();
        }

        private void UnpackBlobIfNeeded()
        {
            if (processCode == EAutoPreprocessCode.eAutoPreprocessCodeCompressed)
            {
                Int32 decompressedSize, unknown;
                Int16 level;

                decompressedSize = source.ReadInt32();
                unknown = source.ReadInt32();
                level = source.ReadInt16();

                if (sourceStack == null)
                    sourceStack = new Stack<Stream>(1);

                sourceStack.Push(source);

                source.ReadInt16(); // skip zlib header

                if (sourceStack == null)
                    sourceStack = new Stack<Stream>(1);

                sourceStack.Push(source);
                source = new BufferedStream(new DeflateStream(source, CompressionMode.Decompress, true), 0x10000);

                ReadBlobHeader();
            }
            else if (processCode == EAutoPreprocessCode.eAutoPreprocessCodeEncrypted)
            {
                // cryptostream
            }
        }

        private void TakeBytes(int size)
        {
#if DEBUG
            Debug.Assert(CanTakeBytes(size));
#endif
            bytesAvailable -= size;
        }

        internal bool CanTakeBytes(int size)
        {
            return bytesAvailable >= size;
        }

        public void ReadFieldHeader()
        {
            keyBytes = source.ReadUInt16();
            dataBytes = source.ReadInt32();

            TakeBytes(FieldHeaderLength);

            if (!CanTakeBytes(keyBytes + dataBytes))
            {
                throw new InvalidBlobException("Invalid field lengths");
            }

            if (keyBuffer.Length < keyBytes)
            {
                keyBuffer = new byte[keyBytes];
            }

            source.Read(keyBuffer, 0, keyBytes);

            TakeBytes(keyBytes + dataBytes);
        }

        // read a field containing a blob, return a BlobReader
        public BlobReader ReadFieldBlob()
        {
            return new BlobReader(source);
        }

        // magic method to make serializers easier
        internal Stream ReadFieldStream()
        {
            return source;
        }

        public void SkipField()
        {
            source.ReadAndDiscard(dataBytes);
        }

        public void SkipSpare()
        {
#if DEBUG
            Debug.Assert(bytesAvailable == 0);
#endif
            source.ReadAndDiscard(spareAvailable);
        }
    }
}
