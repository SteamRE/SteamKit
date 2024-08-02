/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with remote storage and user generated content.
    /// </summary>
    public sealed partial class SteamCloud : ClientMsgHandler
    {
        private static CallbackMsg? GetCallback( IPacketMsg packetMsg ) => packetMsg.MsgType switch
        {
            EMsg.ClientUFSGetUGCDetailsResponse => new UGCDetailsCallback( packetMsg ),
            EMsg.ClientUFSGetSingleFileInfoResponse => new SingleFileInfoCallback( packetMsg ),
            EMsg.ClientUFSShareFileResponse => new ShareFileCallback( packetMsg ),
            _ => null,
        };

        /// <summary>
        /// Requests details for a specific item of user generated content from the Steam servers.
        /// Results are returned in a <see cref="UGCDetailsCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="ugcId">The unique user generated content id.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="UGCDetailsCallback"/>.</returns>
        public AsyncJob<UGCDetailsCallback> RequestUGCDetails( UGCHandle ugcId )
        {
            ArgumentNullException.ThrowIfNull( ugcId );

            var request = new ClientMsgProtobuf<CMsgClientUFSGetUGCDetails>( EMsg.ClientUFSGetUGCDetails );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.hcontent = ugcId;

            this.Client.Send( request );

            return new AsyncJob<UGCDetailsCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Requests details for a specific file in the user's Cloud storage.
        /// Results are returned in a <see cref="SingleFileInfoCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appid">The app id of the game.</param>
        /// <param name="filename">The path to the file being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SingleFileInfoCallback"/>.</returns>
        public AsyncJob<SingleFileInfoCallback> GetSingleFileInfo( uint appid, string filename )
        {
            var request = new ClientMsgProtobuf<CMsgClientUFSGetSingleFileInfo> (EMsg.ClientUFSGetSingleFileInfo );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = appid;
            request.Body.file_name = filename;

            this.Client.Send(request);

            return new AsyncJob<SingleFileInfoCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Commit a Cloud file at the given path to make its UGC handle publicly visible.
        /// Results are returned in a <see cref="ShareFileCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appid">The app id of the game.</param>
        /// <param name="filename">The path to the file being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="ShareFileCallback"/>.</returns>
        public AsyncJob<ShareFileCallback> ShareFile( uint appid, string filename )
        {
            var request = new ClientMsgProtobuf<CMsgClientUFSShareFile>(EMsg.ClientUFSShareFile);
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = appid;
            request.Body.file_name = filename;

            this.Client.Send(request);

            return new AsyncJob<ShareFileCallback>( this.Client, request.SourceJobID );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            var callback = GetCallback( packetMsg );

            if ( callback == null )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            this.Client.PostCallback( callback );
        }
    }
}
