/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with the Steam network as a game server.
    /// </summary>
    public sealed partial class SteamGameServer : ClientMsgHandler
    {
        internal SteamGameServer()
        {
        }


        /// <summary>
        /// Logs onto the Steam network as a game server.
        /// </summary>
        public void LogOn( uint appId = 0 )
        {
            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID gsId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.AnonGameServer );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = gsId.ConvertToUInt64();

            uint localIp = NetHelpers.GetIPAddress( this.Client.LocalIP );
            logon.Body.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;

            logon.Body.client_os_type = ( uint )Utils.GetOSType();
            logon.Body.game_server_app_id = ( int )appId;
            logon.Body.machine_id = Utils.GenerateMachineID();

            this.Client.Send( logon );
        }


        public override void HandleMsg( IPacketMsg packetMsg )
        {
            switch ( packetMsg.MsgType )
            {
                case EMsg.GSStatusReply:
                    HandleStatusReply( packetMsg );
                    break;

                case EMsg.ClientTicketAuthComplete:
                    HandleAuthComplete( packetMsg );
                    break;
            }
        }


        #region Handlers
        void HandleStatusReply( IPacketMsg packetMsg )
        {
            var statusReply = new ClientMsgProtobuf<CMsgGSStatusReply>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new StatusReplyCallback( Client, statusReply.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new StatusReplyCallback( statusReply.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAuthComplete( IPacketMsg packetMsg )
        {
            var authComplete = new ClientMsgProtobuf<CMsgClientTicketAuthComplete>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new TicketAuthCallback( Client, authComplete.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new TicketAuthCallback( authComplete.Body );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion
    }
}
