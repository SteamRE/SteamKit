/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
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


        public ulong RequestUGCDetails( ulong ugcId )
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

#if STATIC_CALLBACKS
            var innerCallback = new UGCDetailsCallback( Client, infoResponse.Body );
            var callback = new SteamClient.JobCallback<UGCDetailsCallback>( Client, infoResponse.TargetJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new UGCDetailsCallback( infoResponse.Body );
            var callback = new SteamClient.JobCallback<UGCDetailsCallback>( infoResponse.TargetJobID, innerCallback );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion

    }
}
