using System;
using SteamKit2.GC;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all game coordinator messaging.
    /// </summary>
    public sealed partial class SteamGameCoordinator : ClientMsgHandler
    {

        /// <summary>
        /// Sends a game coordinator message for a specific appid.
        /// </summary>
        /// <param name="msg">The GC message to send.</param>
        /// <param name="appId">The app id of the game coordinator to send to.</param>
        public void Send( IClientGCMsg msg, uint appId )
        {
            var clientMsg = new ClientMsgProtobuf<CMsgGCClient>( EMsg.ClientToGC );

            clientMsg.ProtoHeader.routing_appid = appId;
            clientMsg.Body.msgtype = MsgUtil.MakeGCMsg( msg.MsgType, msg.IsProto );
            clientMsg.Body.appid = appId;

            clientMsg.Body.payload = msg.Serialize();

            this.Client.Send( clientMsg );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg.MsgType == EMsg.ClientFromGC )
            {
                var msg = new ClientMsgProtobuf<CMsgGCClient>( packetMsg );

                var callback = new MessageCallback( msg.Body );
                this.Client.PostCallback( callback );
            }
        }
    }
}
