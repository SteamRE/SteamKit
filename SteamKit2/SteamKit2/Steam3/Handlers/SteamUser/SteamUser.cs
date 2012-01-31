/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all user log on/log off related actions and callbacks.
    /// </summary>
    public sealed partial class SteamUser : ClientMsgHandler
    {

        /// <summary>
        /// Represents the details required to log into Steam3.
        /// </summary>
        public sealed class LogOnDetails
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
            /// Gets or sets the Steam Guard auth code used to login. This is the code sent to the user's email.
            /// </summary>
            /// <value>The auth code.</value>
            public string AuthCode { get; set; }
            /// <summary>
            /// Gets or sets the sentry file hash for this logon attempt, or null if no sentry file is available.
            /// </summary>
            /// <value>The sentry file hash.</value>
            public byte[] SentryFileHash { get; set; }

            /// <summary>
            /// Gets or sets the account instance. 1 for the PC instance or 2 for the Console (PS3) instance.
            /// </summary>
            /// <value>The account instance.</value>
            public uint AccountInstance { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to request the Steam2 ticket.
            /// This is an optional request only needed for Steam2 content downloads.
            /// </summary>
            /// <value>
            ///   <c>true</c> if the Steam2 ticket should be requested; otherwise, <c>false</c>.
            /// </value>
            public bool RequestSteam2Ticket { get; set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="LogOnDetails"/> class.
            /// </summary>
            public LogOnDetails()
            {
                AccountInstance = 1; // use the default pc steam instance
            }
        }

        /// <summary>
        /// Represents details required to complete a machine auth request.
        /// </summary>
        public sealed class MachineAuthDetails
        {
            /// <summary>
            /// The One-Time-Password details for this response.
            /// </summary>
            public sealed class OTPDetails
            {
                /// <summary>
                /// Gets or sets the one-time-password type.
                /// </summary>
                public uint Type { get; set; }
                /// <summary>
                /// Gets or sets the one-time-password identifier.
                /// </summary>
                public string Identifier { get; set; }
                /// <summary>
                /// Gets or sets the one-time-password value.
                /// </summary>
                public uint Value { get; set; }
            }

            /// <summary>
            /// Gets or sets the target Job ID for the request.
            /// This is provided in the <see cref="SteamClient.JobCallback&lt;T&gt;"/> for a <see cref="UpdateMachineAuthCallback"/>.
            /// </summary>
            /// <value>The Job ID.</value>
            public long JobID { get; set; }

            /// <summary>
            /// Gets or sets the result of updating the machine auth.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; set; }

            /// <summary>
            /// Gets or sets the number of bytes written for the sentry file.
            /// </summary>
            /// <value>The number of bytes written.</value>
            public int BytesWritten { get; set; }
            /// <summary>
            /// Gets or sets the offset within the sentry file that was written.
            /// </summary>
            /// <value>The offset.</value>
            public int Offset { get; set; }

            /// <summary>
            /// Gets or sets the filename of the sentry file that was written.
            /// </summary>
            /// <value>The name of the sentry file.</value>
            public string FileName { get; set; }
            /// <summary>
            /// Gets or sets the size of the sentry file.
            /// </summary>
            /// <value>/ The size of the sentry file.</value>
            public int FileSize { get; set; }

            /// <summary>
            /// Gets or sets the last error that occurred while writing the sentry file, or 0 if no error occurred.
            /// </summary>
            /// <value>The last error.</value>
            public int LastError { get; set; }

            /// <summary>
            /// Gets or sets the SHA-1 hash of the sentry file.
            /// </summary>
            /// <value>The sentry file hash.</value>
            public byte[] SentryFileHash { get; set; }

            /// <summary>
            /// Gets or sets the one-time-password details.
            /// </summary>
            /// <value>The one time password details.</value>
            public OTPDetails OneTimePassword { get; set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="MachineAuthDetails"/> class.
            /// </summary>
            public MachineAuthDetails()
            {
                OneTimePassword = new OTPDetails();
            }
        }


        /// <summary>
        /// Gets the SteamID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The SteamID.</value>
        public SteamID SteamID
        {
            get { return this.Client.SteamID; }
        }


        internal SteamUser()
        {
        }


        /// <summary>
        /// Logs the client into the Steam3 network. The client should already have been connected at this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        /// <param name="details">The details.</param>
        public void LogOn( LogOnDetails details )
        {
            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID steamId = new SteamID( 0, details.AccountInstance, Client.ConnectedUniverse, EAccountType.Individual );

            if ( string.IsNullOrEmpty( details.Username ) || string.IsNullOrEmpty( details.Password ) )
            {
                throw new ArgumentException( "LogOn requires a username and password to be set in LogOnDetails." );
            }

            uint localIp = NetHelpers.GetIPAddress( this.Client.LocalIP );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = steamId.ConvertToUInt64();

            logon.Body.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Body.account_name = details.Username;
            logon.Body.password = details.Password;

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Body.client_os_type = ( uint )Utils.GetOSType();
            logon.Body.client_language = "english";

            logon.Body.steam2_ticket_request = details.RequestSteam2Ticket;

            // we're now using the latest steamclient package version, this is required to get a proper sentry file for steam guard
            logon.Body.client_package_version = 1771; // todo: determine if this is still required

            // this is not a proper machine id that Steam accepts
            // but it's good enough for identifying a machine
            logon.Body.machine_id = Utils.GenerateMachineID();


            // steam guard 
            logon.Body.auth_code = details.AuthCode;

            logon.Body.sha_sentryfile = details.SentryFileHash;
            logon.Body.eresult_sentryfile = ( int )( details.SentryFileHash != null ? EResult.OK : EResult.FileNotFound );


            this.Client.Send( logon );
        }
        /// <summary>
        /// Logs the client into the Steam3 network as an anonymous user. The client should already have been connected at this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        public void LogOnAnonUser()
        {
            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID auId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.AnonUser );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = auId.ConvertToUInt64();

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Body.client_os_type = ( uint )Utils.GetOSType();

            // this is not a proper machine id that Steam accepts
            // but it's good enough for identifying a machine
            logon.Body.machine_id = Utils.GenerateMachineID();

            this.Client.Send( logon );
        }

        /// <summary>
        /// Logs the client off of the Steam3 network. This method does not disconnect the client.
        /// Results are returned in a <see cref="LoggedOffCallback"/>.
        /// </summary>
        public void LogOff()
        {
            var logOff = new ClientMsgProtobuf<CMsgClientLogOff>( EMsg.ClientLogOff );
            this.Client.Send( logOff );
        }

        /// <summary>
        /// Sends a machine auth response.
        /// This should normally be used in response to a <see cref="UpdateMachineAuthCallback"/>.
        /// </summary>
        /// <param name="details">The details pertaining to the response.</param>
        public void SendMachineAuthResponse( MachineAuthDetails details )
        {
            var response = new ClientMsgProtobuf<CMsgClientUpdateMachineAuthResponse>( EMsg.ClientUpdateMachineAuthResponse );

            // so we respond to the correct message
            response.ProtoHeader.job_id_target = ( ulong )details.JobID;

            response.Body.cubwrote = ( uint )details.BytesWritten;
            response.Body.eresult = ( uint )details.Result;

            response.Body.filename = details.FileName;
            response.Body.filesize = ( uint )details.FileSize;

            response.Body.getlasterror = ( uint )details.LastError;
            response.Body.offset = ( uint )details.Offset;

            response.Body.sha_file = details.SentryFileHash;

            response.Body.otp_identifier = details.OneTimePassword.Identifier;
            response.Body.otp_type = ( int )details.OneTimePassword.Type;
            response.Body.otp_value = details.OneTimePassword.Value;

            this.Client.Send( response );

        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            switch ( packetMsg.MsgType )
            {
                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( packetMsg );
                    break;

                case EMsg.ClientNewLoginKey:
                    HandleLoginKey( packetMsg );
                    break;

                case EMsg.ClientSessionToken:
                    HandleSessionToken( packetMsg );
                    break;

                case EMsg.ClientLoggedOff:
                    HandleLoggedOff( packetMsg );
                    break;

                case EMsg.ClientUpdateMachineAuth:
                    HandleUpdateMachineAuth( packetMsg );
                    break;

                case EMsg.ClientAccountInfo:
                    HandleAccountInfo( packetMsg );
                    break;

                case EMsg.ClientWalletInfoUpdate:
                    HandleWalletInfo( packetMsg );
                    break;
            }
        }

        
        #region ClientMsg Handlers
        void HandleLoggedOff( IPacketMsg packetMsg )
        {
            var loggedOff = new ClientMsgProtobuf<CMsgClientLoggedOff>( packetMsg );

#if STATIC_CALLBACKS
            SteamClient.PostCallback( new LoggedOffCallback( Client, loggedOff.Body ) );
#else
            this.Client.PostCallback( new LoggedOffCallback( loggedOff.Body ) );
#endif
        }
        void HandleUpdateMachineAuth( IPacketMsg packetMsg )
        {
            var machineAuth = new ClientMsgProtobuf<CMsgClientUpdateMachineAuth>( packetMsg );

#if STATIC_CALLBACKS
            var innerCallback = new UpdateMachineAuthCallback( Client, machineAuth.Body );
            var callback = new SteamClient.JobCallback<UpdateMachineAuthCallback>( Client, packetMsg.SourceJobID, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new UpdateMachineAuthCallback( machineAuth.Body );
            var callback = new SteamClient.JobCallback<UpdateMachineAuthCallback>( packetMsg.SourceJobID, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        void HandleSessionToken( IPacketMsg packetMsg )
        {
            var sessToken = new ClientMsgProtobuf<CMsgClientSessionToken>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new SessionTokenCallback( Client, sessToken.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new SessionTokenCallback( sessToken.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLoginKey( IPacketMsg packetMsg )
        {
            var loginKey = new ClientMsg<MsgClientNewLoginKey>( packetMsg );

            var resp = new ClientMsg<MsgClientNewLoginKeyAccepted>();
            resp.Body.UniqueID = loginKey.Body.UniqueID;

            this.Client.Send( resp );

#if STATIC_CALLBACKS
            var callback = new LoginKeyCallback( Client, loginKey.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new LoginKeyCallback( loginKey.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLogOnResponse( IPacketMsg packetMsg )
        {
            if ( packetMsg.IsProto )
            {
                var logonResp = new ClientMsgProtobuf<CMsgClientLogonResponse>( packetMsg );

#if STATIC_CALLBACKS
                var callback = new LoggedOnCallback( Client, logonResp.Body );
                SteamClient.PostCallback( callback );
#else
                var callback = new LoggedOnCallback( logonResp.Body );
                this.Client.PostCallback( callback );
#endif
            }
        }
        void HandleAccountInfo( IPacketMsg packetMsg )
        {
            var accInfo = new ClientMsgProtobuf<CMsgClientAccountInfo>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new AccountInfoCallback( Client, accInfo.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new AccountInfoCallback( accInfo.Body );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleWalletInfo( IPacketMsg packetMsg )
        {
            var walletInfo = new ClientMsgProtobuf<CMsgClientWalletInfoUpdate>( packetMsg );

#if STATIC_CALLBACKS
            var callback = new WalletInfoCallback( Client, walletInfo.Body );
            SteamClient.PostCallback( callback );
#else
            var callback = new WalletInfoCallback( walletInfo.Body );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion
    }
}
