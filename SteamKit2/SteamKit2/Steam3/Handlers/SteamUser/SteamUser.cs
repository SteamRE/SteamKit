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
                public uint Type { get; set; }
                public string Identifier { get; set; }
                public uint Value { get; set; }
            }

            /// <summary>
            /// Gets or sets the target Job ID for the request.
            /// This is provided in the <see cref="JobCallback"/> for a <see cref="UpdateMachineAuthCallback"/>.
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
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID steamId = new SteamID( 0, details.AccountInstance, Client.ConnectedUniverse, EAccountType.Individual );

            if ( string.IsNullOrEmpty( details.Username ) || string.IsNullOrEmpty( details.Password ) )
            {
                throw new ArgumentException( "LogOn requires a username and password to be set in LogOnDetails." );
            }

            uint localIp = NetHelpers.GetIPAddress( this.Client.GetLocalIP() );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = steamId.ConvertToUint64();

            logon.Msg.Proto.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Msg.Proto.account_name = details.Username;
            logon.Msg.Proto.password = details.Password;

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Msg.Proto.client_os_type = ( uint )Utils.GetOSType();
            logon.Msg.Proto.client_language = "english";

            logon.Msg.Proto.steam2_ticket_request = details.RequestSteam2Ticket;


            // because steamkit doesn't attempt to find the best cellid
            // we'll just use the default one
            // this is really only relevant for steam2, so it's a mystery as to why steam3 wants to know
            // logon.Msg.Proto.cell_id = 0;

            // we're now using the latest steamclient package version, this is required to get a proper sentry file for steam guard
            logon.Msg.Proto.client_package_version = 1634;

            // this is not a proper machine id that Steam accepts
            // but it's good enough for identifying a machine
            logon.Msg.Proto.machine_id = Utils.GenerateMachineID();


            // steam guard 
            logon.Msg.Proto.auth_code = details.AuthCode;

            logon.Msg.Proto.sha_sentryfile = details.SentryFileHash;
            logon.Msg.Proto.eresult_sentryfile = ( int )( details.SentryFileHash != null ? EResult.OK : EResult.FileNotFound );


            this.Client.Send( logon );
        }
        /// <summary>
        /// Logs the client into the Steam3 network as an anonymous user. The client should already have been connected at this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        public void LogOnAnonUser()
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID auId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.AnonUser );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = auId.ConvertToUint64();

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Msg.Proto.client_os_type = ( uint )Utils.GetOSType();

            // this is not a proper machine id that Steam accepts
            // but it's good enough for identifying a machine
            logon.Msg.Proto.machine_id = Utils.GenerateMachineID();

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
        }

        /// <summary>
        /// Sends a machine auth response.
        /// This should normally be used in response to a <see cref="UpdateMachineAuthCallback"/>.
        /// </summary>
        /// <param name="details">The details pertaining to the response.</param>
        public void SendMachineAuthResponse( MachineAuthDetails details )
        {
            var response = new ClientMsgProtobuf<MsgClientUpdateMachineAuthResponse>();

            // so we respond to the correct message
            response.ProtoHeader.job_id_target = ( ulong )details.JobID;

            response.Msg.Proto.cubwrote = ( uint )details.BytesWritten;
            response.Msg.Proto.eresult = ( uint )details.Result;

            response.Msg.Proto.filename = details.FileName;
            response.Msg.Proto.filesize = ( uint )details.FileSize;

            response.Msg.Proto.getlasterror = ( uint )details.LastError;
            response.Msg.Proto.offset = ( uint )details.Offset;

            response.Msg.Proto.sha_file = details.SentryFileHash;

            response.Msg.Proto.otp_identifier = details.OneTimePassword.Identifier;
            response.Msg.Proto.otp_type = ( int )details.OneTimePassword.Type;
            response.Msg.Proto.otp_value = details.OneTimePassword.Value;

            this.Client.Send( response );

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

                case EMsg.ClientSessionToken:
                    HandleSessionToken( e );
                    break;

                case EMsg.ClientLoggedOff:
                    HandleLoggedOff( e );
                    break;

                case EMsg.ClientUpdateMachineAuth:
                    HandleUpdateMachineAuth( e );
                    break;

                case EMsg.ClientAccountInfo:
                    HandleAccountInfo( e );
                    break;

                case EMsg.ClientWalletInfoUpdate:
                    HandleWalletInfo( e );
                    break;
            }
        }

        
        #region ClientMsg Handlers
        void HandleLoggedOff( ClientMsgEventArgs e )
        {
            var loggedOff = new ClientMsgProtobuf<MsgClientLoggedOff>();

            try
            {
                loggedOff.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamUser", "HandleLoggedOff encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            SteamClient.PostCallback( new LoggedOffCallback( Client, loggedOff.Msg.Proto ) );
#else
            this.Client.PostCallback( new LoggedOffCallback( loggedOff.Msg.Proto ) );
#endif
        }
        void HandleUpdateMachineAuth( ClientMsgEventArgs e )
        {
            var machineAuth = new ClientMsgProtobuf<MsgClientUpdateMachineAuth>();

            try
            {
                machineAuth.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamUser", "HandleUpdateMachineAuth encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var innerCallback = new UpdateMachineAuthCallback( Client, machineAuth.Msg.Proto );
            var callback = new SteamClient.JobCallback<UpdateMachineAuthCallback>( Client, innerCallback );
            SteamClient.PostCallback( callback );
#else
            var innerCallback = new UpdateMachineAuthCallback( machineAuth.Msg.Proto );
            var callback = new SteamClient.JobCallback<UpdateMachineAuthCallback>( ( long )machineAuth.ProtoHeader.job_id_source, innerCallback );
            Client.PostCallback( callback );
#endif
        }
        void HandleSessionToken( ClientMsgEventArgs e )
        {
            var sessToken = new ClientMsgProtobuf<MsgClientSessionToken>();

            try
            {
                sessToken.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamUser", "HandleSessionToken encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var callback = new SessionTokenCallback( Client, sessToken.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new SessionTokenCallback( sessToken.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLoginKey( ClientMsgEventArgs e )
        {
            var loginKey = new ClientMsg<MsgClientNewLoginKey, ExtendedClientMsgHdr>();

            try
            {
                loginKey.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamUser", "HandleLoginKey encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

            var resp = new ClientMsg<MsgClientNewLoginKeyAccepted, ExtendedClientMsgHdr>();
            resp.Msg.UniqueID = loginKey.Msg.UniqueID;

            this.Client.Send( resp );

#if STATIC_CALLBACKS
            var callback = new LoginKeyCallback( Client, loginKey.Msg );
            SteamClient.PostCallback( callback );
#else
            var callback = new LoginKeyCallback( loginKey.Msg );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLogOnResponse( ClientMsgEventArgs e )
        {
            if ( e.IsProto )
            {
                var logonResp = new ClientMsgProtobuf<MsgClientLogOnResponse>();

                try
                {
                    logonResp.SetData( e.Data );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "SteamUser", "HandleLogOnResponse encountered an exception while reading client msg.\n{0}", ex.ToString() );
                    return;
                }

#if STATIC_CALLBACKS
                var callback = new LogOnCallback( Client, logonResp.Msg.Proto );
                SteamClient.PostCallback( callback );
#else
                var callback = new LoggedOnCallback( logonResp.Msg.Proto );
                this.Client.PostCallback( callback );
#endif
            }
        }
        void HandleAccountInfo( ClientMsgEventArgs e )
        {
            var accInfo = new ClientMsgProtobuf<MsgClientAccountInfo>( e.Data );

#if STATIC_CALLBACKS
            var callback = new AccountInfoCallback( Client, accInfo.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new AccountInfoCallback( accInfo.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleWalletInfo( ClientMsgEventArgs e )
        {
            var walletInfo = new ClientMsgProtobuf<MsgClientWalletInfoUpdate>( e.Data );

#if STATIC_CALLBACKS
            var callback = new WalletInfoCallback( Client, walletInfo.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new WalletInfoCallback( walletInfo.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion
    }
}
