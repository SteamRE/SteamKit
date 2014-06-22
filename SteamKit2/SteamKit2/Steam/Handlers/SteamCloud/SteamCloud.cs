/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with remote storage and user generated content.
    /// </summary>
    public sealed partial class SteamCloud : ClientMsgHandler
    {
 
        internal SteamCloud()
        {
        }


        /// <summary>
        /// Requests details for a specific item of user generated content from the Steam servers.
        /// Results are returned in a <see cref="UGCDetailsCallback"/>.
        /// </summary>
        /// <param name="ugcId">The unique user generated content id.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="Callback&lt;T&gt;"/>.</returns>
        public JobID RequestUGCDetails( UGCHandle ugcId )
        {
            var request = new ClientMsgProtobuf<CMsgClientUFSGetUGCDetails>( EMsg.ClientUFSGetUGCDetails );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.hcontent = ugcId;

            this.Client.Send( request );

            return request.SourceJobID;
        }

        /// <summary>
        /// Requests details for a specific file in the user's Cloud storage.
        /// Results are returned in a <see cref="SingleFileInfoCallback"/>.
        /// </summary>
        /// <param name="appid">The app id of the game.</param>
        /// <param name="filename">The path to the file being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="Callback&lt;T&gt;"/>.</returns>
        public JobID GetSingleFileInfo(uint appid, string filename)
        {
            var request = new ClientMsgProtobuf<CMsgClientUFSGetSingleFileInfo>(EMsg.ClientUFSGetSingleFileInfo);
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = appid;
            request.Body.file_name = filename;

            this.Client.Send(request);

            return request.SourceJobID;
        }

        /// <summary>
        /// Commit a Cloud file at the given path to make its UGC handle publicly visible.
        /// Results are returned in a <see cref="ShareFileCallback"/>.
        /// </summary>
        /// <param name="appid">The app id of the game.</param>
        /// <param name="filename">The path to the file being requested.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="Callback&lt;T&gt;"/>.</returns>
        public JobID ShareFile(uint appid, string filename)
        {
            var request = new ClientMsgProtobuf<CMsgClientUFSShareFile>(EMsg.ClientUFSShareFile);
            request.SourceJobID = Client.GetNextJobID();

            request.Body.app_id = appid;
            request.Body.file_name = filename;

            this.Client.Send(request);

            return request.SourceJobID;
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            switch ( packetMsg.MsgType )
            {
                case EMsg.ClientUFSGetUGCDetailsResponse:
                    HandleUGCDetailsResponse( packetMsg );
                    break;
                case EMsg.ClientUFSGetSingleFileInfoResponse:
                    HandleSingleFileInfoResponse( packetMsg );
                    break;
                case EMsg.ClientUFSShareFileResponse:
                    HandleShareFileResponse( packetMsg );
                    break;
            }
        }


        #region ClientMsg Handlers
        void HandleUGCDetailsResponse( IPacketMsg packetMsg )
        {
            var infoResponse = new ClientMsgProtobuf<CMsgClientUFSGetUGCDetailsResponse>( packetMsg );

            var callback = new UGCDetailsCallback(infoResponse.TargetJobID, infoResponse.Body);
            this.Client.PostCallback( callback );
        }

        void HandleSingleFileInfoResponse(IPacketMsg packetMsg)
        {
            var infoResponse = new ClientMsgProtobuf<CMsgClientUFSGetSingleFileInfoResponse>( packetMsg );

            var callback = new SingleFileInfoCallback(infoResponse.TargetJobID, infoResponse.Body);
            this.Client.PostCallback(callback);
        }

        void HandleShareFileResponse(IPacketMsg packetMsg)
        {
            var shareResponse = new ClientMsgProtobuf<CMsgClientUFSShareFileResponse>(packetMsg);

            var callback = new ShareFileCallback(shareResponse.TargetJobID, shareResponse.Body);
            this.Client.PostCallback(callback);
        }
        #endregion

    }
}
