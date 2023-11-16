/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.IO;
using System.Linq;

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

            byte[] processedData = CryptoHelper.SymmetricDecrypt( Data, depotKey );

            if ( processedData.Length > 1 && processedData[ 0 ] == 'V' && processedData[ 1 ] == 'Z' )
            {
                processedData = VZipUtil.Decompress( processedData );
            }
            else
            {
                processedData = ZipUtil.Decompress( processedData );
            }

            DebugLog.Assert( ChunkInfo.Checksum != null, nameof( DepotChunk ), "Expected data chunk to have a checksum." );

            byte[] dataCrc = CryptoHelper.AdlerHash( processedData );

            if ( !dataCrc.SequenceEqual( ChunkInfo.Checksum ) )
            {
                throw new InvalidDataException( "Processed data checksum is incorrect! Downloaded depot chunk is corrupt or invalid/wrong depot key?" );
            }

            Data = processedData;
            IsProcessed = true;
        }
    }
}
