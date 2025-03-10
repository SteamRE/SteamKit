using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    static class VZstdUtil
    {
        private const uint VZstdHeader = 0x56535A61;

        public static int Decompress( ReadOnlySpan<byte> buffer, byte[] destination )
        {
            if ( MemoryMarshal.Read<uint>( buffer ) != VZstdHeader )
            {
                throw new InvalidDataException( "Expecting VZstdHeader at start of stream" );
            }

            var sizeCompressed = MemoryMarshal.Read<int>( buffer[ ^15.. ] ); // TODO: I am not convinced this is correct -- maybe its the frame size
            var sizeDecompressed = MemoryMarshal.Read<int>( buffer[ ^11.. ] );

            if ( buffer[ ^3 ] != 'z' || buffer[ ^2 ] != 's' || buffer[ ^1 ] != 'v' )
            {
                throw new InvalidDataException( "Expecting VZstdFooter at end of stream" );
            }

            if ( destination.Length < sizeDecompressed )
            {
                throw new ArgumentException( "The destination buffer is smaller than the decompressed data size.", nameof( destination ) );
            }

            using var zstdDecompressor = new ZstdSharp.Decompressor();

            var input = buffer[ 4..^12 ];

            if ( !zstdDecompressor.TryUnwrap( input, destination, out var sizeWritten ) || sizeDecompressed != sizeWritten )
            {
                throw new InvalidDataException( $"Failed to decompress Zstd (expected {sizeDecompressed} bytes, got {sizeWritten})." );
            }

            return sizeDecompressed;
        }
    }
}
