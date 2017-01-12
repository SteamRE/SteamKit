using System;
using System.Collections.Generic;
using SteamKit2.GC;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all game coordinator messaging.
    /// </summary>
    public sealed partial class SteamGameCoordinator : ClientMsgMappingHandler
    {
        /// <inheritdoc />
        protected override Dictionary<EMsg, Action<IPacketMsg>> DispatchMap { get; }

        internal SteamGameCoordinator()
        {
            DispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
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
            var clientMsg = new ClientMsgProtobuf<CMsgGCClient>( EMsg.ClientToGC );

            clientMsg.ProtoHeader.routing_appid = appId;
            clientMsg.Body.msgtype = MsgUtil.MakeGCMsg( msg.MsgType, msg.IsProto );
            clientMsg.Body.appid = appId;

            clientMsg.Body.payload = msg.Serialize();

            this.Client.Send( clientMsg );
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
