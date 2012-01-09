using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all game coordinator messaging.
    /// </summary>
    public sealed partial class SteamGameCoordinator : ClientMsgHandler
    {

        /*
        /// <summary>
        /// Sends the specified game coordinator message.
        /// The message header should not be a protobuf header, use the overload function.
        /// </summary>
        /// <typeparam name="MsgType">The message body type of the message.</typeparam>
        /// <typeparam name="Hdr">The message header type of the message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="appId">The AppID of the game coordinator to send to.</param>
        public void Send<MsgType, Hdr>( GCMsg<MsgType, Hdr> msg, uint appId )
            where Hdr : IGCSerializableHeader, new()
            where MsgType : IGCSerializableMessage, new()
        {
            var clientMsg = new ClientMsgProtobuf<CMsgAMGCClientRelay>( EMsg.ClientToGC );

            clientMsg.Body.msgtype = ( uint )msg.GetEMsg();
            clientMsg.Body.appid = appId;

            using ( var ms = new MemoryStream() )
            {
                msg.Serialize( ms );
                clientMsg.Body.payload = ms.ToArray();
            }

            this.Client.Send( clientMsg );
        }

        /// <summary>
        /// Sends the specific game coordinator message. This function is for protobuf messages.
        /// </summary>
        /// <typeparam name="MsgType">The message body type of the message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="appId">The AppID of the game coordinator to send to.</param>
        public void Send<MsgType>( GCMsgProtobuf<MsgType> msg, uint appId )
            where MsgType : IGCSerializableMessage, new()
        {
            var clientMsg = new ClientMsgProtobuf<CMsgAMGCClientRelay>( EMsg.ClientToGC );

            clientMsg.Body.msgtype = ( uint )MsgUtil.MakeGCMsg( msg.GetEMsg(), true );
            clientMsg.Body.appid = appId;

            using ( var ms = new MemoryStream() )
            {
                msg.Serialize( ms );
                clientMsg.Body.payload = ms.ToArray();
            }

            this.Client.Send( clientMsg );
        }*/


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.ClientMsgEventArgs"/> instance containing the event data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg.MsgType == EMsg.ClientFromGC )
            {
                var msg = new ClientMsgProtobuf<CMsgAMGCClientRelay>( packetMsg );

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
