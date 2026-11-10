/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;

namespace SteamKit2.CDN
{
    /// <summary>
    /// Provides a helper function to decrypt and decompress a single depot chunk.
    /// </summary>
    public static class DepotChunk
    {
        /// <summary>
        /// Processes the specified depot key by decrypting the data with the given depot encryption key, and then by decompressing the data.
        /// If the chunk has already been processed, this function does nothing.
        /// </summary>
        /// <param name="info">The depot chunk data representing.</param>
        /// <param name="data">The encrypted chunk data.</param>
        /// <param name="destination">The buffer to receive the decrypted chunk data.</param>
        /// <param name="depotKey">The depot decryption key.</param>
        /// <exception cref="InvalidDataException">Thrown if the processed data does not match the expected checksum given in its chunk information.</exception>
        public static int Process( DepotManifest.ChunkData info, ReadOnlySpan<byte> data, byte[] destination, byte[] depotKey )
        {
            ArgumentNullException.ThrowIfNull( info );
            ArgumentNullException.ThrowIfNull( depotKey );

            if ( destination.Length < info.UncompressedLength )
            {
                throw new ArgumentException( $"The destination buffer must be longer than the chunk {nameof( DepotManifest.ChunkData.UncompressedLength )}.", nameof( destination ) );
            }

            DebugLog.Assert( depotKey.Length == 32, nameof( DepotChunk ), $"Tried to decrypt depot chunk with non 32 byte key!" );

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = depotKey;

            // first 16 bytes of input is the ECB encrypted IV
            Span<byte> iv = stackalloc byte[ 16 ];
            aes.DecryptEcb( data[ ..iv.Length ], iv, PaddingMode.None );

            // With CBC and padding, the decrypted size will always be smaller
            var buffer = ArrayPool<byte>.Shared.Rent( data.Length - iv.Length );

            var writtenDecompressed = 0;

            try
            {
                var written = aes.DecryptCbc( data[ iv.Length.. ], iv, buffer, PaddingMode.PKCS7 );

                if ( written < 16 )
                {
                    throw new InvalidDataException( $"Not enough data in the decrypted depot chunk (was {written} bytes)." );
                }

                var decrypted = buffer.AsSpan( 0, written );

                if ( decrypted.StartsWith( "VSZa"u8 ) ) // Zstd
                {
                    writtenDecompressed = VZstdUtil.Decompress( decrypted, destination, verifyChecksum: false );
                }
                else if ( decrypted.StartsWith( "VZa"u8 ) ) // LZMA
                {
                    writtenDecompressed = VZipUtil.Decompress( decrypted, destination, verifyChecksum: false );
                }
                else if ( decrypted.StartsWith( "PK\x03\x04"u8 ) ) // PKzip
                {
                    using var decryptedStream = new MemoryStream( buffer, 0, written );
                    writtenDecompressed = ZipUtil.Decompress( decryptedStream, destination, verifyChecksum: false );
                }
                else
                {
                    throw new InvalidDataException( $"Unexpected depot chunk compression (first four bytes are {Convert.ToHexString( buffer.AsSpan( 0, 4 ) )})." );
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( buffer );
            }

            if ( info.UncompressedLength != writtenDecompressed )
            {
                throw new InvalidDataException( $"Processed data checksum failed to decompressed to the expected chunk uncompressed length. (was {writtenDecompressed}, should be {info.UncompressedLength})" );
            }

            var dataCrc = AdlerHash( destination.AsSpan( 0, writtenDecompressed ) );

            if ( dataCrc != info.Checksum )
            {
                throw new InvalidDataException( "Processed data checksum is incorrect! Downloaded depot chunk is corrupt or invalid/wrong depot key?" );
            }

            return writtenDecompressed;
        }

        /// <summary>
        /// Calculates the Adler32 checksum with the bytes taken from the span using zero as the initial seed.
        /// </summary>
        public static uint AdlerHash( ReadOnlySpan<byte> input )
            => Adler32.Calculate( 0, input );
    }
}
