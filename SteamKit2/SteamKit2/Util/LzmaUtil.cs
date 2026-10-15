using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SteamKit2.Internal
{
    /// <summary>
    /// Internal helper to decode data blobs with the 'LZMA' magic header.
    /// </summary>
    internal static class LzmaUtil
    {
        /// <summary>
        /// Check to see if some data starts with the LZMA header magic.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <returns><c>true</c> if the data begins with the magic</returns>
        public static bool HasLzmaHeader( ReadOnlySpan<byte> data )
        {
            if ( data.Length < sizeof( int ) )
            {
                return false;
            }

            var magic = BinaryPrimitives.ReadInt32BigEndian( data[ ..sizeof( int ) ] );
            return ( magic == 0x4C5A4D41 );
        }

        /// <summary>
        /// Decompress a LZMA stream into another stream.
        /// </summary>
        /// <param name="input">The LZMA stream to decode.</param>
        /// <param name="output">The destination stream to decode to.</param>
        /// <param name="compressedLength">The (expected) length of compressed data.</param>
        /// <param name="uncompressedLength">The (expected) length of data after decompression.</param>
        public static void Decompress( Stream input, Stream output, int compressedLength, int uncompressedLength )
        {
            Span<byte> properties = stackalloc byte[ LzmaDecoder.PropertiesSize ];
            input.ReadExactly( properties );

            var compressed = ArrayPool<byte>.Shared.Rent( compressedLength );
            var decompressed = ArrayPool<byte>.Shared.Rent( uncompressedLength );

            try
            {
                input.ReadExactly( compressed, 0, compressedLength );
                LzmaDecoder.Decode( properties, compressed.AsSpan( 0, compressedLength ), decompressed.AsSpan( 0, uncompressedLength ) );
                output.Write( decompressed, 0, uncompressedLength );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( compressed );
                ArrayPool<byte>.Shared.Return( decompressed );
            }
        }

        /// <summary>
        /// Decomrpesses a LZMA stream into another stream.
        /// </summary>
        /// <param name="input">The LZMA stream to decode.</param>
        /// <param name="outputFactory">A function to create or acquire an output stream, given the (expected) length of
        /// data after decompression.</param>
        /// <param name="output">The output stream containing decompressed data.</param>
        /// <returns><c>true</c> if decompression was successful, <c>false</c> otherwise.</returns>
        public static bool TryDecompress( Stream input, Func<int, Stream> outputFactory, [NotNullWhen( true )] out Stream? output )
        {
            Span<byte> buffer = stackalloc byte[ sizeof( int ) ];
            
            if ( !TryReadExactly( input, buffer ) )
            {
                output = null;
                return false;
            }

            var magic = BinaryPrimitives.ReadInt32BigEndian( buffer );
            if ( magic != 0x4C5A4D41 /* LZMA */)
            {
                output = null;
                return false;
            }

            if ( !TryReadExactly( input, buffer ) )
            {
                output = null;
                return false;
            }

            var uncompressedLength = BinaryPrimitives.ReadInt32LittleEndian( buffer );

            if ( !TryReadExactly( input, buffer ) )
            {
                output = null;
                return false;
            }

            var compressedLength = BinaryPrimitives.ReadInt32LittleEndian( buffer );

            if ( compressedLength < 0 || uncompressedLength < 0 )
            {
                throw new InvalidDataException( $"LZMA lengths must not be negative (compressed {compressedLength}, uncompressed {uncompressedLength})." );
            }

            output = outputFactory( uncompressedLength );
            try
            {
                Decompress( input, output, compressedLength, uncompressedLength );

                if ( output.CanSeek )
                {
                    output.Seek( 0, SeekOrigin.Begin );
                }

                return true;
            }
            catch
            {
                output.Dispose();
                output = null;
                throw;
            }
        }

        static bool TryReadExactly( Stream stream, Span<byte> buffer)
        {
            var read = 0;
            while ( read < buffer.Length )
            {
                var count = stream.Read( buffer.Slice( read ) );
                if (count == 0)
                {
                    return false;
                }

                read += count;
            }

            return true;
        }
    }
}
