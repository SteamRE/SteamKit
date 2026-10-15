using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Hashing;

namespace SteamKit2
{
    static class VZipUtil
    {
        private const ushort VZipHeader = 0x5A56;
        private const ushort VZipFooter = 0x767A;
        private const int HeaderLength = 7; // magic + version + timestamp/crc
        private const int FooterLength = 10; // crc + decompressed size + magic

        private const byte Version = (byte)'a';

        /// <summary>
        /// The parts of a VZip buffer: the LZMA properties and stream, and the footer's CRC and size of the decompressed data.
        /// </summary>
        internal readonly ref struct Frame
        {
            public ReadOnlySpan<byte> Properties { get; init; }
            public ReadOnlySpan<byte> Compressed { get; init; }
            public uint OutputCRC { get; init; }
            public int DecompressedSize { get; init; }
        }

        internal static Frame ReadFrame( ReadOnlySpan<byte> buffer, byte version )
        {
            if ( buffer.Length < HeaderLength + LzmaDecoder.PropertiesSize + FooterLength )
            {
                throw new InvalidDataException( $"VZip data is too short (was {buffer.Length} bytes)." );
            }

            if ( BinaryPrimitives.ReadUInt16LittleEndian( buffer ) != VZipHeader )
            {
                throw new InvalidDataException( "Expecting VZipHeader at start of stream" );
            }

            if ( buffer[ 2 ] != version )
            {
                throw new InvalidDataException( $"Expecting VZip version '{( char )version}'" );
            }

            // The next four bytes are sometimes a creation timestamp (e.g. for Steam Client VZips),
            // and sometimes a CRC32 (e.g. for depot chunks).

            var footer = buffer[ ^FooterLength.. ];
            var outputCRC = BinaryPrimitives.ReadUInt32LittleEndian( footer );
            var sizeDecompressed = BinaryPrimitives.ReadInt32LittleEndian( footer[ 4.. ] );

            if ( BinaryPrimitives.ReadUInt16LittleEndian( footer[ 8.. ] ) != VZipFooter )
            {
                throw new InvalidDataException( "Expecting VZipFooter at end of stream" );
            }

            if ( sizeDecompressed < 0 )
            {
                throw new InvalidDataException( $"VZip decompressed size is negative (was {sizeDecompressed})." );
            }

            return new Frame
            {
                Properties = buffer.Slice( HeaderLength, LzmaDecoder.PropertiesSize ),
                Compressed = buffer[ ( HeaderLength + LzmaDecoder.PropertiesSize )..^FooterLength ],
                OutputCRC = outputCRC,
                DecompressedSize = sizeDecompressed,
            };
        }

        public static int Decompress( ReadOnlySpan<byte> buffer, byte[] destination, bool verifyChecksum = true )
        {
            var frame = ReadFrame( buffer, Version );

            if ( destination.Length < frame.DecompressedSize )
            {
                throw new ArgumentException( "The destination buffer is smaller than the decompressed data size.", nameof( destination ) );
            }

            var output = destination.AsSpan( 0, frame.DecompressedSize );
            LzmaDecoder.Decode( frame.Properties, frame.Compressed, output );

            if ( verifyChecksum && Crc32.HashToUInt32( output ) != frame.OutputCRC )
            {
                throw new InvalidDataException( "CRC does not match decompressed data. VZip data may be corrupted." );
            }

            return frame.DecompressedSize;
        }

        public static byte[] Compress( byte[] buffer )
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
