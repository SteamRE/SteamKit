using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Hashing;

namespace SteamKit2
{
    static class VZstdUtil
    {
        private const uint VZstdHeader = 0x615A5356;
        private const int HeaderLength = 8; // magic + crc
        private const int FooterLength = 15; // crc + decompressed size + unknown + magic

        public static int Decompress( ReadOnlySpan<byte> buffer, byte[] destination, bool verifyChecksum = true )
        {
            if ( buffer.Length < HeaderLength + FooterLength )
            {
                throw new InvalidDataException( $"VZstd data is too short (was {buffer.Length} bytes)." );
            }

            if ( BinaryPrimitives.ReadUInt32LittleEndian( buffer ) != VZstdHeader )
            {
                throw new InvalidDataException( "Expecting VZstdHeader at start of stream" );
            }

            var crc32 = BinaryPrimitives.ReadUInt32LittleEndian( buffer[ 4.. ] );
            var crc32_footer = BinaryPrimitives.ReadUInt32LittleEndian( buffer[ ^FooterLength.. ] );
            var sizeDecompressed = BinaryPrimitives.ReadInt32LittleEndian( buffer[ ^11.. ] );

            Debug.Assert( crc32 == crc32_footer ); // They write CRC32 twice?

            if ( buffer[ ^3 ] != 'z' || buffer[ ^2 ] != 's' || buffer[ ^1 ] != 'v' )
            {
                throw new InvalidDataException( "Expecting VZstdFooter at end of stream" );
            }

            if ( destination.Length < sizeDecompressed )
            {
                throw new ArgumentException( "The destination buffer is smaller than the decompressed data size.", nameof( destination ) );
            }

            var input = buffer[ HeaderLength..^FooterLength ];

#if NET11_0_OR_GREATER
            if ( !ZstandardDecoder.TryDecompress( input, destination, out var sizeWritten ) || sizeDecompressed != sizeWritten )
            {
                throw new InvalidDataException( $"Failed to decompress Zstd (expected {sizeDecompressed} bytes, got {sizeWritten})." );
            }
#else
            using var zstdDecompressor = new ZstdSharp.Decompressor();

            try
            {
                if ( !zstdDecompressor.TryUnwrap( input, destination, out var sizeWritten ) || sizeDecompressed != sizeWritten )
                {
                    throw new InvalidDataException( $"Failed to decompress Zstd (expected {sizeDecompressed} bytes, got {sizeWritten})." );
                }
            }
            catch ( ZstdSharp.ZstdException ex )
            {
                throw new InvalidDataException( "Failed to decompress Zstd.", ex );
            }
#endif

            if ( verifyChecksum && Crc32.HashToUInt32( destination.AsSpan( 0, sizeDecompressed ) ) != crc32_footer )
            {
                throw new InvalidDataException( "CRC does not match decompressed data. VZstd data may be corrupted." );
            }

            return sizeDecompressed;
        }
    }
}
