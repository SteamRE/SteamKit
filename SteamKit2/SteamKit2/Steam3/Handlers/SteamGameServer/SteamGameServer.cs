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
        /// <summary>
        /// Represents the details required to log into Steam3 as a game server.
        /// </summary>
        public sealed class LogOnDetails
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the AppID this gameserver will serve.
            /// </summary>
            public uint AppID { get; set; }
        }


        internal SteamGameServer()
        {
        }


        /// <summary>
        /// Logs onto the Steam network as a persistent game server.
        /// The client should already have been connected at this point.
        /// Results are return in a <see cref="SteamUser.LoggedOnCallback"/>.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="ArgumentNullException">No logon details were provided.</exception>
        /// <exception cref="ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public void LogOn( LogOnDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( "details" );
            }

            if ( string.IsNullOrEmpty( details.Username ) || string.IsNullOrEmpty( details.Password ) )
            {
                throw new ArgumentException( "LogOn requires a username and password to be set in 'details'." );
            }

            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID gsId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.GameServer );

            logon.ProtoHeader.client_sessionid = 0;
            logon.ProtoHeader.steamid = gsId.ConvertToUInt64();

            uint localIp = NetHelpers.GetIPAddress( this.Client.LocalIP );
            logon.Body.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;

            logon.Body.client_os_type = ( uint )Utils.GetOSType();
            logon.Body.game_server_app_id = ( int )details.AppID;
            logon.Body.machine_id = Utils.GenerateMachineID();

            logon.Body.account_name = details.Username;
            logon.Body.password = details.Password;

            this.Client.Send( logon );
        }

        /// <summary>
        /// Logs the client into the Steam3 network as an anonymous game server.
        /// The client should already have been connected at this point.
        /// Results are returned in a <see cref="SteamUser.LoggedOnCallback"/>.
        /// </summary>
        /// <param name="appId">The AppID served by this game server, or 0 for the default.</param>
        public void LogOnAnonymous( uint appId = 0 )
        {
            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID gsId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.AnonGameServer );

            logon.ProtoHeader.client_sessionid = 0;
            logon.ProtoHeader.steamid = 0; gsId.ConvertToUInt64();

            uint localIp = NetHelpers.GetIPAddress( this.Client.LocalIP );
            logon.Body.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;

            logon.Body.client_os_type = ( uint )Utils.GetOSType();
            logon.Body.game_server_app_id = ( int )appId;
            logon.Body.machine_id = Utils.GenerateMachineID();

            this.Client.Send( logon );
        }

        /// <summary>
        /// Logs the game server off of the Steam3 network.
        /// This method does not disconnect the client.
        /// Results are returned in a <see cref="SteamUser.LoggedOffCallback"/>.
        /// </summary>
        public void LogOff()
        {
            var logOff = new ClientMsgProtobuf<CMsgClientLogOff>( EMsg.ClientLogOff );
            this.Client.Send( logOff );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
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
