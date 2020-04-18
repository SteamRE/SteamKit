/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
            /// Gets or sets the authentication token used to log in as a game server.
            /// </summary>
            public string? Token { get; set; }

            /// <summary>
            /// Gets or sets the AppID this gameserver will serve.
            /// </summary>
            public uint AppID { get; set; }
        }

        /// <summary>
        /// Represents the details of the game server's current status.
        /// </summary>
        public sealed class StatusDetails
        {
            /// <summary>
            /// Gets or sets the AppID this game server is serving.
            /// </summary>
            public uint AppID { get; set; }

            /// <summary>
            /// Gets or sets the server's basic state as flags.
            /// </summary>
            public EServerFlags ServerFlags { get; set; }

            /// <summary>
            /// Gets or sets the directory the game data is in.
            /// </summary>
            public string? GameDirectory { get; set; }

            /// <summary>
            /// Gets or sets the IP address the game server listens on.
            /// </summary>
            public IPAddress? Address { get; set; }

            /// <summary>
            /// Gets or sets the port the game server listens on.
            /// </summary>
            public uint Port { get; set; }

            /// <summary>
            /// Gets or sets the port the game server responds to queries on.
            /// </summary>
            public uint QueryPort { get; set; }

            /// <summary>
            /// Gets or sets the current version of the game server.
            /// </summary>
            public string? Version { get; set; }
        }


        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamGameServer()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.GSStatusReply, HandleStatusReply },
                { EMsg.ClientTicketAuthComplete, HandleAuthComplete },
            };
        }


        /// <summary>
        /// Logs onto the Steam network as a persistent game server.
        /// The client should already have been connected at this point.
        /// Results are return in a <see cref="SteamUser.LoggedOnCallback"/>.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="System.ArgumentNullException">No logon details were provided.</exception>
        /// <exception cref="System.ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public void LogOn( LogOnDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( "details" );
            }

            if ( string.IsNullOrEmpty( details.Token ) )
            {
                throw new ArgumentException( "LogOn requires a game server token to be set in 'details'." );
            }
            
            if ( !this.Client.IsConnected )
            {
                this.Client.PostCallback( new SteamUser.LoggedOnCallback( EResult.NoConnection ) );
                return;
            }

            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogonGameServer );

            SteamID gsId = new SteamID( 0, 0, Client.Universe, EAccountType.GameServer );

            logon.ProtoHeader.client_sessionid = 0;
            logon.ProtoHeader.steamid = gsId.ConvertToUInt64();

            logon.Body.obfuscated_private_ip = NetHelpers.GetMsgIPAddress( this.Client.LocalIP! ).ObfuscatePrivateIP();

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;

            logon.Body.client_os_type = ( uint )Utils.GetOSType();
            logon.Body.game_server_app_id = ( int )details.AppID;
            logon.Body.machine_id = HardwareUtils.GetMachineID();

            logon.Body.game_server_token = details.Token;

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
            if ( !this.Client.IsConnected )
            {
                this.Client.PostCallback( new SteamUser.LoggedOnCallback( EResult.NoConnection ) );
                return;
            }

            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID gsId = new SteamID( 0, 0, Client.Universe, EAccountType.AnonGameServer );

            logon.ProtoHeader.client_sessionid = 0;
            logon.ProtoHeader.steamid = gsId.ConvertToUInt64();

            logon.Body.obfuscated_private_ip = NetHelpers.GetMsgIPAddress( this.Client.LocalIP! ).ObfuscatePrivateIP();

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;

            logon.Body.client_os_type = ( uint )Utils.GetOSType();
            logon.Body.game_server_app_id = ( int )appId;
            logon.Body.machine_id = HardwareUtils.GetMachineID();

            this.Client.Send( logon );
        }

        /// <summary>
        /// Informs the Steam servers that this client wishes to log off from the network.
        /// The Steam server will disconnect the client, and a <see cref="SteamClient.DisconnectedCallback"/> will be posted.
        /// </summary>
        public void LogOff()
        {
            ExpectDisconnection = true;

            var logOff = new ClientMsgProtobuf<CMsgClientLogOff>( EMsg.ClientLogOff );
            this.Client.Send( logOff );
        }

        /// <summary>
        /// Sends the server's status to the Steam network.
        /// Results are returned in a <see cref="StatusReplyCallback"/> callback.
        /// </summary>
        /// <param name="details">A <see cref="SteamGameServer.StatusDetails"/> object containing the server's status.</param>
        public void SendStatus(StatusDetails details)
        {
            if (details == null)
            {
                throw new ArgumentNullException( nameof(details) );
            }

            if (details.Address != null && details.Address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supported.");
            }

            var status = new ClientMsgProtobuf<CMsgGSServerType>(EMsg.GSServerType);
            status.Body.app_id_served = details.AppID;
            status.Body.flags = (uint)details.ServerFlags;
            status.Body.game_dir = details.GameDirectory;
            status.Body.game_port = details.Port;
            status.Body.game_query_port = details.QueryPort;
            status.Body.game_version = details.Version;

            if (details.Address != null)
            {
                status.Body.deprecated_game_ip_address = NetHelpers.GetIPAddressAsUInt( details.Address );
            }

            this.Client.Send( status );
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


        #region Handlers
        void HandleStatusReply( IPacketMsg packetMsg )
        {
            var statusReply = new ClientMsgProtobuf<CMsgGSStatusReply>( packetMsg );

            var callback = new StatusReplyCallback( statusReply.Body );
            this.Client.PostCallback( callback );
        }
        void HandleAuthComplete( IPacketMsg packetMsg )
        {
            var authComplete = new ClientMsgProtobuf<CMsgClientTicketAuthComplete>( packetMsg );

            var callback = new TicketAuthCallback( authComplete.Body );
            this.Client.PostCallback( callback );
        }
        #endregion
    }
}
