using System;
using System.Collections.Generic;
using SteamKit2.GC;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all game coordinator messaging.
    /// </summary>
    public sealed partial class SteamGameCoordinator : ClientMsgHandler
    {
        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamGameCoordinator()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientFromGC, HandleFromGC },
            };
        }


        /// <summary>
        /// Sends a game coordinator message for a specific appid.
        /// </summary>
        /// <param name="msg">The GC message to send.</param>
        /// <param name="appId">The app id of the game coordinator to send to.</param>
        public void Send( IClientGCMsg msg, uint appId )
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( nameof(msg) );
            }

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
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof(packetMsg) );
            }

            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleFromGC( IPacketMsg packetMsg )
        {
            var msg = new ClientMsgProtobuf<CMsgGCClient>( packetMsg );

            var callback = new MessageCallback( msg.Body );
            this.Client.PostCallback( callback );
        }
        #endregion
    }
}
