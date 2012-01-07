/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID gsId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.AnonGameServer );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = gsId.ConvertToUint64();

            uint localIp = NetHelpers.GetIPAddress( this.Client.GetLocalIP() );
            logon.Msg.Proto.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol;

            logon.Msg.Proto.client_os_type = ( uint )Utils.GetOSType();
            logon.Msg.Proto.game_server_app_id = ( int )appId;
            logon.Msg.Proto.machine_id = Utils.GenerateMachineID();

            this.Client.Send( logon );
        }


        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.GSStatusReply:
                    HandleStatusReply( e );
                    break;

                case EMsg.ClientTicketAuthComplete:
                    HandleAuthComplete( e );
                    break;
            }
        }


        #region Handlers
        void HandleStatusReply( ClientMsgEventArgs e )
        {
            var statusReply = new ClientMsgProtobuf<MsgGSStatusReply>( e.Data );

#if STATIC_CALLBACKS
            var callback = new StatusReplyCallback( Client, statusReply.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new StatusReplyCallback( statusReply.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAuthComplete( ClientMsgEventArgs e )
        {
            var authComplete = new ClientMsgProtobuf<MsgClientTicketAuthComplete>( e.Data );

#if STATIC_CALLBACKS
            var callback = new TicketAuthCallback( Client, authComplete.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new TicketAuthCallback( authComplete.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion
    }
}
