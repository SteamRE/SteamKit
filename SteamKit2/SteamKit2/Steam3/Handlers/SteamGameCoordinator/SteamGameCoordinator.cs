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

            clientMsg.Body.msgtype = MsgUtil.MakeGCMsg( msg.MsgType, msg.IsProto );
            clientMsg.Body.appid = appId;

            clientMsg.Body.payload = msg.Serialize();

            this.Client.Send( clientMsg );
        }

        /// <summary>
        /// Sends a game coordinator message for a specific appid.
        /// </summary>
        /// <param name="data">The data to send. This should be a serialized GC message.</param>
        /// <param name="appId">The app id of the game coordinator to send to.</param>
        [Obsolete( "This function is obsolete and the IClientGCMsg override should be used!" )]
        public void Send( byte[] data, uint appId )
        {
            var clientMsg = new ClientMsgProtobuf<CMsgGCClient>( EMsg.ClientToGC );

            clientMsg.Body.msgtype = BitConverter.ToUInt32( data, 0 );
            clientMsg.Body.appid = appId;

            clientMsg.Body.payload = data;

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

#if STATIC_CALLBACKS
                var callback = new MessageCallback( Client, msg.Body );
                SteamClient.PostCallback( callback );
#else
                var callback = new MessageCallback( msg.Body );
                this.Client.PostCallback( callback );
#endif
            }
        }
    }
}
