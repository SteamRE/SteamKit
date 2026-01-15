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
    /// This handler is used for interacting with remote storage and user generated content.
    /// </summary>
    public sealed partial class SteamCloud : ClientMsgHandler
    {
        /// <summary>
        /// Requests details for a specific item of user generated content from the Steam servers.
        /// Results are returned in a <see cref="UGCDetailsCallback"/>.
        /// </summary>
        /// <param name="ugcId">The unique user generated content id.</param>
        /// <returns>The UGC details.</returns>
        public async Task<UGCDetailsCallback> RequestUGCDetails( UGCHandle ugcId )
        {
            ArgumentNullException.ThrowIfNull( ugcId );

            var request = new CCloud_GetFileDetails_Request
            {
                ugcid = ugcId,
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var cloudService = unifiedMessages.CreateService<Cloud>();
            var response = await cloudService.GetFileDetails( request );

            return new UGCDetailsCallback( response );
        }

        /// <summary>
        /// Requests details for a specific file in the user's Cloud storage.
        /// Results are returned in a <see cref="SingleFileInfoCallback"/>.
        /// </summary>
        /// <param name="appid">The app id of the game.</param>
        /// <param name="filename">The path to the file being requested.</param>
        /// <returns>The file info.</returns>
        public async Task<SingleFileInfoCallback> GetSingleFileInfo( uint appid, string filename )
        {
            var request = new CCloud_GetSingleFileInfo_Request
            {
                app_id = appid,
                file_name = filename,
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var cloudService = unifiedMessages.CreateService<Cloud>();
            var response = await cloudService.GetSingleFileInfo( request );

            return new SingleFileInfoCallback( response );
        }

        /// <summary>
        /// Commit a Cloud file at the given path to make its UGC handle publicly visible.
        /// Results are returned in a <see cref="ShareFileCallback"/>.
        /// </summary>
        /// <param name="appid">The app id of the game.</param>
        /// <param name="filename">The path to the file being requested.</param>
        /// <returns>The share file result.</returns>
        public async Task<ShareFileCallback> ShareFile( uint appid, string filename )
        {
            var request = new CCloud_ShareFile_Request
            {
                app_id = appid,
                file_name = filename,
            };

            var unifiedMessages = Client.GetHandler<SteamUnifiedMessages>()!;
            var cloudService = unifiedMessages.CreateService<Cloud>();
            var response = await cloudService.ShareFile( request );

            return new ShareFileCallback( response );
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
