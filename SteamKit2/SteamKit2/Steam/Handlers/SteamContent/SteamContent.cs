/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with content server directory on the Steam network.
    /// </summary>
    public sealed class SteamContent : ClientMsgHandler
    {
        /// <summary>
        /// This is received when a CDN auth token is received
        /// </summary>
        public sealed class CDNAuthToken
        {
            /// <summary>
            /// Result of the operation
            /// </summary>
            public EResult Result { get; set; }
            /// <summary>
            /// CDN auth token
            /// </summary>
            public string Token { get; set; }
            /// <summary>
            /// Token expiration date
            /// </summary>
            public DateTime Expiration { get; set; }

            internal CDNAuthToken( SteamUnifiedMessages.ServiceMethodResponse<CContentServerDirectory_GetCDNAuthToken_Response> message )
            {
                Result = message.Result;
                Token = message.Body.token;
                Expiration = DateUtils.DateTimeFromUnixTime( message.Body.expiration_time );
            }
        }

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// This is an alternative to <see cref="o:ContentServerDirectoryService.LoadAsync"></see>.
        /// </summary>
        /// <param name="cellId">Preferred steam cell id</param>
        /// <param name="maxNumServers">Max number of servers to return.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDN.Server"/>s.</returns>
        public async Task<IReadOnlyCollection<CDN.Server>> GetServersForSteamPipe( uint? cellId = null, uint? maxNumServers = null )
        {
            var request = new CContentServerDirectory_GetServersForSteamPipe_Request();

            if ( cellId.HasValue )
            {
                request.cell_id = cellId.Value;
            }
            else
            {
                request.cell_id = this.Client.CellID ?? 0;
            }

            if ( maxNumServers.HasValue )
            {
                request.max_servers = maxNumServers.Value;
            }

            // SendMessage is an AsyncJob, but we want to deserialize it
            // can't really do HandleMsg because it requires parsing the service like its done in HandleServiceMethod
            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<ContentServerDirectory>();
            var response = await contentService.GetServersForSteamPipe( request );
            return ContentServerDirectoryService.ConvertServerList( response.Body );
        }

        /// <summary>
        /// Request the manifest request code for the specified arguments.
        /// </summary>
        /// <param name="depotId">The DepotID to request a manifest request code for.</param>
        /// <param name="appId">The AppID parent of the DepotID.</param>
        /// <param name="manifestId">The ManifestID that will be downloaded.</param>
        /// <param name="branch">The branch name this manifest belongs to.</param>
        /// <param name="branchPasswordHash">The branch password. TODO: how is it hashed?</param>
        /// <returns>Returns the manifest request code, it may be zero if it was not granted.</returns>
        public async Task<ulong> GetManifestRequestCode( uint depotId, uint appId, ulong manifestId, string? branch = null, string? branchPasswordHash = null )
        {
            if ( string.Equals( branch, "public", StringComparison.OrdinalIgnoreCase ) )
            {
                branch = null;
                branchPasswordHash = null;
            }

            if ( branchPasswordHash != null && branch == null )
            {
                throw new ArgumentNullException( nameof( branch ), "Branch name may not be null if password is provided." );
            }

            var request = new CContentServerDirectory_GetManifestRequestCode_Request
            {
                app_id = appId,
                depot_id = depotId,
                manifest_id = manifestId,
                app_branch = branch,
                branch_password_hash = branchPasswordHash,
            };

            // SendMessage is an AsyncJob, but we want to deserialize it
            // can't really do HandleMsg because it requires parsing the service like its done in HandleServiceMethod
            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<ContentServerDirectory>();
            var response = await contentService.GetManifestRequestCode( request );
            return response.Body.manifest_request_code;
        }

        /// <summary>
        /// Request product information for an app or package
        /// Results are returned in a <see cref="CDNAuthToken"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the result.
        /// </summary>
        /// <param name="app">App id requested.</param>
        /// <param name="depot">Depot id requested.</param>
        /// <param name="host_name">CDN host name being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="CDNAuthToken"/>.</returns>
        public async Task<CDNAuthToken> GetCDNAuthToken(uint app, uint depot, string host_name)
        {
            var request = new CContentServerDirectory_GetCDNAuthToken_Request
            {
                app_id = app,
                depot_id = depot,
                host_name = host_name,
            };

            // SendMessage is an AsyncJob, but we want to deserialize it
            // can't really do HandleMsg because it requires parsing the service like its done in HandleServiceMethod
            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var contentService = unifiedMessages.CreateService<ContentServerDirectory>();
            var result = await contentService.GetCDNAuthToken( request );

            return new CDNAuthToken( result );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            // not used
        }
    }
}
