/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
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
            if ( string.Equals( branch, "public", StringComparison.InvariantCultureIgnoreCase ) )
            {
                branch = null;
                branchPasswordHash = null;
            }

            if ( branchPasswordHash != null && branch == null )
            {
                throw new ArgumentNullException( "Branch name may not be null if password is provided.", nameof( branch ) );
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
            var contentService = unifiedMessages.CreateService<IContentServerDirectory>();
            var message = await contentService.SendMessage( api => api.GetManifestRequestCode( request ) );
            var response = message.GetDeserializedResponse<CContentServerDirectory_GetManifestRequestCode_Response>();

            return response.manifest_request_code;
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
