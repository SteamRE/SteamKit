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
        /// <exception cref="InvalidDataException">Thrown if the processed data does not match the expected checksum given in it's chunk information.</exception>
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
                var decryptedStream = new MemoryStream( buffer, 0, written );

                if ( buffer.Length > 1 && buffer[ 0 ] == 'V' && buffer[ 1 ] == 'Z' )
                {
                    writtenDecompressed = VZipUtil.Decompress( decryptedStream, destination, verifyChecksum: false );
                }
                else
                {
                    writtenDecompressed = ZipUtil.Decompress( decryptedStream, destination, verifyChecksum: false );
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

            var dataCrc = Utils.AdlerHash( destination.AsSpan()[ ..writtenDecompressed ] );

            if ( dataCrc != info.Checksum )
            {
                throw new InvalidDataException( "Processed data checksum is incorrect! Downloaded depot chunk is corrupt or invalid/wrong depot key?" );
            }

            return writtenDecompressed;
        }
    }
}
