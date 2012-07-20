using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace SteamKit2.Blob
{
    /// <summary>
    /// BlobReader that sequentially reads a blob
    /// </summary>
    public class BlobReader : IDisposable
    {
        /// <summary>
        /// Exception for a Blob that cannot be parsed
        /// </summary>
        public class InvalidBlobException : Exception
        {
            internal InvalidBlobException(string message) : base(message) { }
        }

        private bool ownsSource;
        private Stream source;
        private Stack<Stream> sourceStack;

        private ECacheState cacheState;
        private EAutoPreprocessCode processCode;
        private int bytesAvailable, spareAvailable;

        /// <summary>
        /// Cache state of the blob being read
        /// </summary>
        public ECacheState CacheState { get { return cacheState; } }
        /// <summary>
        /// Process code of the blob being read
        /// </summary>
        public EAutoPreprocessCode ProcessCode { get { return processCode; } }

        /// <summary>
        /// Current bytes available to be read
        /// </summary>
        public int BytesAvailable { get { return bytesAvailable; } }
        /// <summary>
        /// Current spare bytes available to be read
        /// </summary>
        public int SpareAvailable { get { return spareAvailable; } }

        private int keyBytes, dataBytes;

        /// <summary>
        /// Size of the Field Key currently being processed
        /// </summary>
        public int FieldKeyBytes { get { return keyBytes; } }
        /// <summary>
        /// Size of the Field Data currently being processed
        /// </summary>
        public int FieldDataBytes { get { return dataBytes; } }

        private byte[] keyBuffer;
        /// <summary>
        /// Byte buffer of the Field Key
        /// </summary>
        public byte[] ByteKey { get { return keyBuffer; } }

        private const int BlobHeaderLength = 10;
        internal const int FieldHeaderLength = 6;
        private const int CompressedHeaderLength = 10;
        private const int EncryptedHeaderLength = 20;

        /// <summary>
        /// Create a new BlobReader from a file path
        /// </summary>
        /// <param name="fileName">Path to blob</param>
        /// <returns></returns>
        public static BlobReader CreateFrom(string fileName)
        {
            return new BlobReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 0x10000, FileOptions.SequentialScan), true);
        }

        /// <summary>
        /// Create a BlobReader from a Stream. Does not take ownership of the Stream.
        /// </summary>
        /// <param name="inputSteam">Source</param>
        /// <returns></returns>
        public static BlobReader CreateFrom(Stream inputSteam)
        {
            return new BlobReader(inputSteam, false);
        }


        private BlobReader(Stream blobSource, bool ownsSource = true)
        {
            this.sourceStack = null;
            this.source = blobSource;
            this.ownsSource = ownsSource;
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

        /// <summary>
        /// Dispose the BlobReader, releasing any Streams allocated.
        /// </summary>
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

        /// <summary>
        /// Read the next Field in the blob
        /// </summary>
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

        /// <summary>
        /// Reads a field as a Blob
        /// </summary>
        /// <returns>Blob Reader</returns>
        public BlobReader ReadFieldBlob()
        {
            return new BlobReader(source, false);
        }

        // magic method to make serializers easier
        internal Stream ReadFieldStream()
        {
            return source;
        }

        /// <summary>
        /// Skip over a field, discarding the contents of a field
        /// </summary>
        public void SkipField()
        {
            source.ReadAndDiscard(dataBytes);
        }

        /// <summary>
        /// Skip over the spare, discarding the contents
        /// </summary>
        public void SkipSpare()
        {
#if DEBUG
            Debug.Assert(bytesAvailable == 0);
#endif
            source.ReadAndDiscard(spareAvailable);
        }
    }
}
