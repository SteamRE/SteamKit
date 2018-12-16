/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteamKit2
{
    public sealed partial class CDNClient : IDisposable
    {
        const uint MAGIC_PATCH_HEADER = 0x502F15E5;
        const uint MAGIC_PATCH_FOOTER = 0xF3DEA982;

        /// <summary>
        /// Downloads the depot delta patch specified by the given manifest IDs.
        /// </summary>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="sourceManifestId">The unique identifier of the manifest to diff from.</param>
        /// <param name="targetManifestId">The unique identifier of the manifest to diff to.</param>
        /// <returns>A <see cref="DepotDeltaChunks"/> instance that contains information about the delta chunk patches present</returns>
        public async Task<DepotDeltaChunks> DownloadPatchAsync( uint depotId, ulong sourceManifestId, ulong targetManifestId )
        {
            depotCdnAuthKeys.TryGetValue( depotId, out var cdnToken );
            depotKeys.TryGetValue( depotId, out var depotKey );

            return await DownloadPatchCoreAsync( depotId, sourceManifestId, targetManifestId, connectedServer, cdnToken, depotKey ).ConfigureAwait( false );
        }

        /// <summary>
        /// Downloads the depot delta patch specified by the given manifest IDs.
        /// </summary>
        /// <param name="depotId">The id of the depot being accessed.</param>
        /// <param name="sourceManifestId">The unique identifier of the manifest to diff from.</param>
        /// <param name="targetManifestId">The unique identifier of the manifest to diff to.</param>
        /// <param name="host">CDN hostname.</param>
        /// <param name="cdnAuthToken">CDN auth token for CDN content server endpoints.</param>
        /// <param name="depotKey">
        /// The depot decryption key for the depot that will be downloaded.
        /// This is used for decrypting filenames (if needed) in depot manifests, and processing depot chunks.
        /// </param>
        /// <returns>A <see cref="DepotDeltaChunks"/> instance that contains information about the delta chunk patches present</returns>
        public async Task<DepotDeltaChunks> DownloadPatchAsync( uint depotId, ulong sourceManifestId, ulong targetManifestId, string host, string cdnAuthToken, byte[] depotKey )
        {
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = host,
                VHost = host,
                Port = 80
            };

            return await DownloadPatchCoreAsync( depotId, sourceManifestId, targetManifestId, server, cdnAuthToken, depotKey ).ConfigureAwait( false );
        }

        async Task<DepotDeltaChunks> DownloadPatchCoreAsync( uint depotId, ulong sourceManifestId, ulong targetManifestId, Server server, string cdnAuthToken, byte[] depotKey )
        {
            if ( depotKey == null )
            {
                throw new ArgumentNullException( nameof( depotKey ) );
            }

            if ( sourceManifestId == targetManifestId )
            {
                throw new ArgumentException( "Source manifest cannot equal target manifest" );
            }

            // TODO: This request can return 404 with html content, need to account for that
            var patchData = await DoRawCommandAsync( server, HttpMethod.Get, "depot", doAuth: true, args: $"{depotId}/patch/{sourceManifestId}/{targetManifestId}", authtoken: cdnAuthToken ).ConfigureAwait( false );
            
            // Steam client reference is CContentDeltaChunks::BDeserializeProtobuf
            patchData = CryptoHelper.SymmetricDecrypt( patchData, depotKey );

            using ( var ms = new MemoryStream( patchData ) )
            using ( var br = new BinaryReader( ms ) )
            {
                var headerMagic = br.ReadUInt32();

                if ( headerMagic != MAGIC_PATCH_HEADER )
                {
                    throw new InvalidDataException( "Didn't recognize header of content delta patch" );
                }

                var payloadLength = br.ReadUInt32();
                var payload = br.ReadBytes( ( int )payloadLength );

                var footerMagic = br.ReadUInt32();

                if ( footerMagic != MAGIC_PATCH_FOOTER )
                {
                    throw new InvalidDataException( "Didn't recognize trailer of content delta patch" );
                }

                return new DepotDeltaChunks( payload );
            }
        }
    }
}
