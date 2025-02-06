using System;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    static class VZstdUtil
    {
        private const uint VZstdHeader = 0x615A5356;

        public static int Decompress( ReadOnlySpan<byte> buffer, byte[] destination, bool verifyChecksum = true )
        {
            if ( MemoryMarshal.Read<uint>( buffer ) != VZstdHeader )
            {
                throw new InvalidDataException( "Expecting VZstdHeader at start of stream" );
            }

            var crc32 = MemoryMarshal.Read<int>( buffer[ 4.. ] );
            var crc32_footer = MemoryMarshal.Read<int>( buffer[ ^15.. ] );
            var sizeDecompressed = MemoryMarshal.Read<int>( buffer[ ^11.. ] );

            Debug.Assert( crc32 == crc32_footer ); // They write CRC32 twice?

            if ( buffer[ ^3 ] != 'z' || buffer[ ^2 ] != 's' || buffer[ ^1 ] != 'v' )
            {
                throw new InvalidDataException( "Expecting VZstdFooter at end of stream" );
            }

            if ( destination.Length < sizeDecompressed )
            {
                throw new ArgumentException( "The destination buffer is smaller than the decompressed data size.", nameof( destination ) );
            }

            using var zstdDecompressor = new ZstdSharp.Decompressor();

            var input = buffer[ 8..^15 ];

            if ( !zstdDecompressor.TryUnwrap( input, destination, out var sizeWritten ) || sizeDecompressed != sizeWritten )
            {
                throw new InvalidDataException( $"Failed to decompress Zstd (expected {sizeDecompressed} bytes, got {sizeWritten})." );
            }

            if ( verifyChecksum && Crc32.HashToUInt32( destination.AsSpan()[ ..sizeDecompressed ] ) != crc32_footer )
            {
                throw new InvalidDataException( "CRC does not match decompressed data. VZstd data may be corrupted." );
            }

            return sizeDecompressed;
        }
    }
}
