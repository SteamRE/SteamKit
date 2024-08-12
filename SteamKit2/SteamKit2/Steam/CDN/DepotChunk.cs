/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SteamKit2.CDN
{
    /// <summary>
    /// Represents a single downloaded chunk from a file in a depot.
    /// </summary>
    public sealed class DepotChunk
    {
        /// <summary>
        /// Gets the depot manifest chunk information associated with this chunk.
        /// </summary>
        public DepotManifest.ChunkData ChunkInfo { get; }

        /// <summary>
        /// Gets a value indicating whether this chunk has been processed. A chunk is processed when the data has been decrypted and decompressed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this chunk has been processed; otherwise, <c>false</c>.
        /// </value>
        public bool IsProcessed { get; internal set; }

        /// <summary>
        /// Gets the underlying data for this chunk.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepotChunk"/> class.
        /// </summary>
        /// <param name="info">The manifest chunk information associated with this chunk.</param>
        /// <param name="data">The underlying data for this chunk.</param>
        public DepotChunk( DepotManifest.ChunkData info, byte[] data )
        {
            ArgumentNullException.ThrowIfNull( info );

            ArgumentNullException.ThrowIfNull( data );

            ChunkInfo = info;
            Data = data;
        }

        /// <summary>
        /// Processes the specified depot key by decrypting the data with the given depot encryption key, and then by decompressing the data.
        /// If the chunk has already been processed, this function does nothing.
        /// </summary>
        /// <param name="depotKey">The depot decryption key.</param>
        /// <exception cref="System.IO.InvalidDataException">Thrown if the processed data does not match the expected checksum given in it's chunk information.</exception>
        public void Process( byte[] depotKey )
        {
            ArgumentNullException.ThrowIfNull( depotKey );

            if ( IsProcessed )
            {
                return;
            }

            Data = ProcessCore( ChunkInfo, Data, depotKey );
            IsProcessed = true;
        }

        static byte[] ProcessCore( DepotManifest.ChunkData info, Span<byte> data, byte[] depotKey )
        {
            DebugLog.Assert( depotKey.Length == 32, nameof( DepotChunk ), $"Tried to decrypt depot chunk with non 32 byte key!" );

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = depotKey;

            // first 16 bytes of input is the ECB encrypted IV
            Span<byte> iv = stackalloc byte[ 16 ];
            aes.DecryptEcb( data[ ..iv.Length ], iv, PaddingMode.None );

            byte[] processedData;

            // With CBC and padding, the decrypted size will always be smaller
            var buffer = ArrayPool<byte>.Shared.Rent( data.Length - iv.Length );

            try
            {
                var written = aes.DecryptCbc( data[ iv.Length.. ], iv, buffer, PaddingMode.PKCS7 );
                var decryptedStream = new MemoryStream( buffer, 0, written );

                if ( buffer.Length > 1 && buffer[ 0 ] == 'V' && buffer[ 1 ] == 'Z' )
                {
                    processedData = VZipUtil.Decompress( decryptedStream );
                }
                else
                {
                    processedData = ZipUtil.Decompress( decryptedStream );
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( buffer );
            }

            var dataCrc = Utils.AdlerHash( processedData );

            if ( dataCrc != info.Checksum )
            {
                throw new InvalidDataException( "Processed data checksum is incorrect! Downloaded depot chunk is corrupt or invalid/wrong depot key?" );
            }

            return processedData;
        }

        internal static DepotChunk ProcessFromData( DepotManifest.ChunkData info, Span<byte> data, byte[] depotKey )
        {
            var processedData = ProcessCore( info, data, depotKey );

            return new DepotChunk( info, processedData )
            {
                IsProcessed = true
            };
        }
    }
}
