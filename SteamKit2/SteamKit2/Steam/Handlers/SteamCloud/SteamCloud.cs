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
        /// Results are returned in a <see cref="UGCDetailsCallback"/> from a <see cref="SteamClient.JobCallback&lt;T&gt;"/>.
        /// </summary>
        /// <param name="ugcId">The unique user generated content id.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID RequestUGCDetails( UGCHandle ugcId )
        {
            var request = new ClientMsgProtobuf<CMsgClientUFSGetUGCDetails>( EMsg.ClientUFSGetUGCDetails );
            request.SourceJobID = Client.GetNextJobID();

            request.Body.hcontent = ugcId;

            this.Client.Send( request );

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
            }
        }


        #region ClientMsg Handlers
        void HandleUGCDetailsResponse( IPacketMsg packetMsg )
        {
            var infoResponse = new ClientMsgProtobuf<CMsgClientUFSGetUGCDetailsResponse>( packetMsg );

            var innerCallback = new UGCDetailsCallback( infoResponse.Body );
            var callback = new SteamClient.JobCallback<UGCDetailsCallback>( infoResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
        }
        #endregion

    }
}
