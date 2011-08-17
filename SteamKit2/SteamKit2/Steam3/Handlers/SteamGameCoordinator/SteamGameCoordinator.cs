using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all game coordinator messaging.
    /// </summary>
    public sealed partial class SteamGameCoordinator : ClientMsgHandler
    {

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
            var clientMsg = new ClientMsg<MsgClientToGC, ExtendedClientMsgHdr>();

            clientMsg.Msg.GCEMsg = ( uint )msg.GetEMsg();
            clientMsg.Msg.AppId = appId;

            msg.Serialize( clientMsg.Payload.BaseStream );

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
            var clientMsg = new ClientMsg<MsgClientToGC, ExtendedClientMsgHdr>();

            clientMsg.Msg.GCEMsg = ( uint )MsgUtil.MakeGCMsg( msg.GetEMsg(), true );
            clientMsg.Msg.AppId = appId;

            msg.Serialize( clientMsg.Payload.BaseStream );

            this.Client.Send( clientMsg );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.ClientMsgEventArgs"/> instance containing the event data.</param>
        public override void HandleMsg( ClientMsgEventArgs e )
        {
            if ( e.EMsg == EMsg.ClientFromGC )
            {
                var msg = new ClientMsg<MsgClientFromGC, ExtendedClientMsgHdr>();

                try
                {
                    msg.SetData( e.Data );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "SteamGameCoordinator", "HandleMsg encountered an exception while reading client msg.\n{0}", ex.ToString() );
                    return;
                }

#if STATIC_CALLBACKS
                var callback = new MessageCallback( Client, msg.Msg, msg.Payload.ToArray() );
                SteamClient.PostCallback( callback );
#else
                var callback = new MessageCallback( msg.Msg, msg.Payload.ToArray() );
                this.Client.PostCallback( callback );
#endif
            }
        }
    }
}
