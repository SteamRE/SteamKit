using System;
using System.IO;
using System.IO.Hashing;

namespace SteamKit2
{
    class VZipUtil
    {
        private static ushort VZipHeader = 0x5A56;
        private static ushort VZipFooter = 0x767A;
        private static int HeaderLength = 7;
        private static int FooterLength = 10;

        private static char Version = 'a';


        public static byte[] Decompress(byte[] buffer)
        {
            using MemoryStream ms = new MemoryStream( buffer );
            using BinaryReader reader = new BinaryReader( ms );
            if ( reader.ReadUInt16() != VZipHeader )
            {
                throw new Exception( "Expecting VZipHeader at start of stream" );
            }

            if ( reader.ReadChar() != Version )
            {
                throw new Exception( "Expecting VZip version 'a'" );
            }

            // Sometimes this is a creation timestamp (e.g. for Steam Client VZips).
            // Sometimes this is a CRC32 (e.g. for depot chunks).
            /* uint creationTimestampOrSecondaryCRC = */ reader.ReadUInt32();

            byte[] properties = reader.ReadBytes( 5 );
            var compressedPosition = ms.Position;
            var compressedLength = ( int )ms.Length - HeaderLength - FooterLength - 5;
            //byte[] compressedBuffer = reader.ReadBytes( ( int )ms.Length - HeaderLength - FooterLength - 5 );
            ms.Position += compressedLength;

            uint outputCRC = reader.ReadUInt32();
            uint sizeDecompressed = reader.ReadUInt32();

            if ( reader.ReadUInt16() != VZipFooter )
            {
                throw new Exception( "Expecting VZipFooter at end of stream" );
            }

            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            decoder.SetDecoderProperties( properties );

            //using MemoryStream inputStream = new MemoryStream( compressedBuffer );
            using MemoryStream outStream = new MemoryStream( ( int )sizeDecompressed );
            ms.Position = compressedPosition; // Redirect the location of compressed data,  decoder.Code does not read the last 10 bytes
            decoder.Code( ms, outStream, compressedLength, sizeDecompressed, null );

            byte[] outData;
            if ( sizeDecompressed == outStream.Position && sizeDecompressed == outStream.Capacity && sizeDecompressed  == outStream.Length)
            {
                outData = outStream.GetBuffer(); // After specifying sizeDecompressed, MemoryStream will not be expanded. Use GetBuffer to reduce copying.
                                                 // At this time, sizeDecompressed == Position == Length == Capacity
            }
            else
            {
                outData = outStream.ToArray();
            }
            if ( Crc32.HashToUInt32( outData ) != outputCRC )
            {
                throw new InvalidDataException( "CRC does not match decompressed data. VZip data may be corrupted." );
            }

            return outData;
        }

        public static byte[] Compress(byte[] buffer)
        {
            using MemoryStream ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter( ms );
            byte[] crc = Crc32.Hash( buffer );

            writer.Write( VZipHeader );
            writer.Write( ( byte )Version );
            writer.Write( crc );

            int dictionary = 1 << 23;
            int posStateBits = 2;
            int litContextBits = 3;
            int litPosBits = 0;
            int algorithm = 2;
            int numFastBytes = 128;

            SevenZip.CoderPropID[] propIDs =
            [
                SevenZip.CoderPropID.DictionarySize,
                SevenZip.CoderPropID.PosStateBits,
                SevenZip.CoderPropID.LitContextBits,
                SevenZip.CoderPropID.LitPosBits,
                SevenZip.CoderPropID.Algorithm,
                SevenZip.CoderPropID.NumFastBytes,
                SevenZip.CoderPropID.MatchFinder,
                SevenZip.CoderPropID.EndMarker
            ];

            object[] properties =
            [
                dictionary,
                posStateBits,
                litContextBits,
                litPosBits,
                algorithm,
                numFastBytes,
                "bt4",
                false
            ];

            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.SetCoderProperties( propIDs, properties );
            encoder.WriteCoderProperties( ms );

            using ( MemoryStream input = new MemoryStream( buffer ) )
            {
                encoder.Code( input, ms, -1, -1, null );
            }

            writer.Write( crc );
            writer.Write( ( uint )buffer.Length );
            writer.Write( VZipFooter );

            return ms.ToArray();
        }
    }
}
