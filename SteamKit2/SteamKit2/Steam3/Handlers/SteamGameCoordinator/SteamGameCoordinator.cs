using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        /// <param name="data">The data to send. This should be a serialized GC message.</param>
        /// <param name="appId">The app id of the game coordinator to send to.</param>
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
