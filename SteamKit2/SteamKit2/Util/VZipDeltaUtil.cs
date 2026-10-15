using System;
using System.Buffers;
using System.IO;
using System.IO.Hashing;

namespace SteamKit2
{
    static class VZipDeltaUtil
    {
        private const byte Version = ( byte )'d';

        public static byte[] Decompress( byte[] buffer, byte[] sourceChunkData )
        {
            // The CRC in the header is also the CRC of the chunk
            var frame = VZipUtil.ReadFrame( buffer, Version );

            // The source chunk is the preset dictionary, and positions in the delta continue after it
            var totalLength = sourceChunkData.Length + frame.DecompressedSize;
            var window = ArrayPool<byte>.Shared.Rent( totalLength );

            try
            {
                sourceChunkData.CopyTo( window, 0 );
                LzmaDecoder.Decode( frame.Properties, frame.Compressed, window.AsSpan( 0, totalLength ), sourceChunkData.Length );

                var outData = window.AsSpan( sourceChunkData.Length, frame.DecompressedSize ).ToArray();

                if ( Crc32.HashToUInt32( outData ) != frame.OutputCRC )
                {
                    throw new InvalidDataException( "CRC does not match decompressed data. VZip data may be corrupted." );
                }

                return outData;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( window );
            }
        }
    }
}
