/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all user log on/log off related actions and callbacks.
    /// </summary>
    public sealed partial class SteamUser : ClientMsgHandler
    {
        /// <summary>
        /// The unique name of this hadler.
        /// </summary>
        public const string NAME = "SteamUser";


        internal SteamUser()
            : base( SteamUser.NAME )
        {
        }


        /// <summary>
        /// Represents the details required to log into Steam3.
        /// </summary>
        public class LogOnDetails
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>The username.</value>
            public string Username { get; set; }
            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>The password.</value>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the client Ticket Granting Ticket.
            /// </summary>
            /// <value>The client TGT.</value>
            public ClientTGT ClientTGT { get; set; }
            /// <summary>
            /// Gets or sets the server Ticket Granting Ticket.
            /// </summary>
            /// <value>The server TGT.</value>
            public byte[] ServerTGT { get; set; }
            /// <summary>
            /// Gets or sets the account record.
            /// </summary>
            /// <value>The account record.</value>
            public Blob AccRecord { get; set; }
        }
        /// <summary>
        /// Logs the client into the Steam3 network. The client should already have been connected at this point.
        /// Results are returned in a <see cref="LogOnCallback"/>.
        /// </summary>
        /// <param name="details">The details.</param>
        public void LogOn( LogOnDetails details )
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID steamId = new SteamID();
            steamId.SetFromSteam2( details.ClientTGT.UserID, this.Client.ConnectedUniverse );

            uint localIp = NetHelpers.GetIPAddress( this.Client.GetLocalIP() );

            MicroTime creationTime = MicroTime.Deserialize( details.AccRecord.GetDescriptor( AuthFields.eFieldTimestampCreation ) );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = steamId.ConvertToUint64();

            logon.Msg.Proto.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Msg.Proto.account_name = details.Username;
            logon.Msg.Proto.password = details.Password;

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Msg.Proto.client_os_type = 10; // windows
            logon.Msg.Proto.client_language = "english";
            logon.Msg.Proto.rtime32_account_creation = creationTime.ToUnixTime();

            logon.Msg.Proto.cell_id = 10; // todo: figure out how to grab a cell id
            logon.Msg.Proto.client_package_version = 1385;

            logon.Msg.Proto.machine_id = Utils.GenerateMachineID();

            logon.Msg.Proto.email_address = details.AccRecord.GetStringDescriptor( AuthFields.eFieldEmail );

            byte[] serverTgt = new byte[ details.ServerTGT.Length + 4 ];

            Array.Copy( BitConverter.GetBytes( localIp ), serverTgt, 4 );
            Array.Copy( details.ServerTGT, 0, serverTgt, 4, details.ServerTGT.Length );

            logon.Msg.Proto.steam2_auth_ticket = serverTgt;

            this.Client.Send( logon );
        }

        /// <summary>
        /// Logs the client off of the Steam3 network. This method does not disconnect the client.
        /// Results are returned in a <see cref="LogOffCallback"/>.
        /// </summary>
        public void LogOff()
        {
            var logOff = new ClientMsgProtobuf<MsgClientLogOff>();
            this.Client.Send( logOff );

            this.Client.PostCallback( new LogOffCallback() );
        }

        /// <summary>
        /// Gets the SteamID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The SteamID.</value>
        public SteamID GetSteamID()
        {
            return this.Client.SteamID;
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.ClientMsgEventArgs"/> instance containing the event data.</param>
        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( e );
                    break;

                case EMsg.ClientNewLoginKey:
                    HandleLoginKey( e );
                    break;
            }
        }


        void HandleLoginKey( ClientMsgEventArgs e )
        {
            var loginKey = new ClientMsg<MsgClientNewLoginKey, ExtendedClientMsgHdr>( e.Data );

            var resp = new ClientMsg<MsgClientNewLoginKeyAccepted, ExtendedClientMsgHdr>();
            resp.Msg.UniqueID = loginKey.Msg.UniqueID;

            this.Client.Send( resp );

            var callback = new LoginKeyCallback( loginKey.Msg );
            this.Client.PostCallback( callback );
        }
        void HandleLogOnResponse( ClientMsgEventArgs e )
        {
            if ( e.IsProto )
            {
                var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( e.Data );

                var callback = new LogOnCallback( logonResp.Msg.Proto );
                this.Client.PostCallback( callback );
            }
        }
    }
}
