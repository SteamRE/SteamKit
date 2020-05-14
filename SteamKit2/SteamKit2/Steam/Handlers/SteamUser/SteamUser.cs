/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all user log on/log off related actions and callbacks.
    /// </summary>
    public sealed partial class SteamUser : ClientMsgHandler
    {

        /// <summary>
        /// Represents the details required to log into Steam3 as a user.
        /// </summary>
        public sealed class LogOnDetails
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>The username.</value>
            public string? Username { get; set; }
            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>The password.</value>
            public string? Password { get; set; }

            /// <summary>
            /// Gets or sets the CellID.
            /// </summary>
            /// <value>The CellID.</value>
            public uint? CellID { get; set; }

            /// <summary>
            /// Gets or sets the LoginID. This number is used for identifying logon session.
            /// The purpose of this field is to allow multiple sessions to the same steam account from the same machine.
            /// This is because Steam Network doesn't allow more than one session with the same LoginID to access given account at the same time from the same public IP.
            /// If you want to establish more than one active session to given account, you must make sure that every session (to that account) from the same public IP has a unique LoginID.
            /// By default LoginID is automatically generated based on machine's primary bind address, which is the same for all sessions.
            /// Null value will cause this property to be automatically generated based on default behaviour.
            /// If in doubt, set this property to null.
            /// </summary>
            /// <value>The LoginID.</value>
            public uint? LoginID { get; set; }

            /// <summary>
            /// Gets or sets the Steam Guard auth code used to login. This is the code sent to the user's email.
            /// </summary>
            /// <value>The auth code.</value>
            public string? AuthCode { get; set; }
            /// <summary>
            /// Gets or sets the 2-factor auth code used to login. This is the code that can be received from the authenticator apps.
            /// </summary>
            /// <value>The two factor auth code.</value>
            public string? TwoFactorCode { get; set; }
            /// <summary>
            /// Gets or sets the login key used to login. This is a key that has been recieved in a previous Steam sesson by a <see cref="LoginKeyCallback"/>.
            /// </summary>
            /// <value>The login key.</value>
            public string? LoginKey { get; set; }
            /// <summary>
            /// Gets or sets the 'Should Remember Password' flag. This is used in combination with the login key and <see cref="LoginKeyCallback"/> for password-less login.
            /// </summary>
            /// <value>The 'Should Remember Password' flag.</value>
            public bool ShouldRememberPassword { get; set; }
            /// <summary>
            /// Gets or sets the sentry file hash for this logon attempt, or null if no sentry file is available.
            /// </summary>
            /// <value>The sentry file hash.</value>
            public byte[]? SentryFileHash { get; set; }

            /// <summary>
            /// Gets or sets the account instance. 1 for the PC instance or 2 for the Console (PS3) instance.
            /// </summary>
            /// <value>The account instance.</value>
            /// <seealso cref="SteamKit2.SteamID.DesktopInstance"/>
            /// <seealso cref="SteamKit2.SteamID.ConsoleInstance"/>
            public uint AccountInstance { get; set; }
            /// <summary>
            /// Gets or sets the account ID used for connecting clients when using the Console instance.
            /// </summary>
            /// <value>
            /// The account ID.
            /// </value>
            /// <seealso cref="LogOnDetails.AccountInstance"/>
            public uint AccountID { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to request the Steam2 ticket.
            /// This is an optional request only needed for Steam2 content downloads.
            /// </summary>
            /// <value>
            ///   <c>true</c> if the Steam2 ticket should be requested; otherwise, <c>false</c>.
            /// </value>
            public bool RequestSteam2Ticket { get; set; }

            /// <summary>
            /// Gets or sets the client operating system type.
            /// </summary>
            /// <value>The client operating system type.</value>
            public EOSType ClientOSType { get; set; }
            /// <summary>
            /// Gets or sets the client language.
            /// </summary>
            /// <value>The client language.</value>
            public string ClientLanguage { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LogOnDetails"/> class.
            /// </summary>
            public LogOnDetails()
            {
                AccountInstance = SteamID.DesktopInstance; // use the default pc steam instance
                AccountID = 0;

                ClientOSType = Utils.GetOSType();
                ClientLanguage = "english";
            }
        }

        /// <summary>
        /// Represents the details required to log into Steam3 as an anonymous user.
        /// </summary>
        public sealed class AnonymousLogOnDetails
        {
            /// <summary>
            /// Gets or sets the CellID.
            /// </summary>
            /// <value>The CellID.</value>
            public uint? CellID { get; set; }

            /// <summary>
            /// Gets or sets the client operating system type.
            /// </summary>
            /// <value>The client operating system type.</value>
            public EOSType ClientOSType { get; set; }
            /// <summary>
            /// Gets or sets the client language.
            /// </summary>
            /// <value>The client language.</value>
            public string ClientLanguage { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="AnonymousLogOnDetails"/> class.
            /// </summary>
            public AnonymousLogOnDetails()
            {
                ClientOSType = Utils.GetOSType();
                ClientLanguage = "english";
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
                public string? Identifier { get; set; }
                /// <summary>
                /// Gets or sets the one-time-password value.
                /// </summary>
                public uint Value { get; set; }
            }

            /// <summary>
            /// Gets or sets the target Job ID for the request.
            /// This is provided in the <see cref="Callback&lt;T&gt;"/> for a <see cref="UpdateMachineAuthCallback"/>.
            /// </summary>
            /// <value>The Job ID.</value>
            public JobID? JobID { get; set; }

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
            public string? FileName { get; set; }
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
            public byte[]? SentryFileHash { get; set; }

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
        public SteamID? SteamID
        {
            get { return this.Client.SteamID; }
        }


        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamUser()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientLogOnResponse, HandleLogOnResponse },
                { EMsg.ClientLoggedOff, HandleLoggedOff },
                { EMsg.ClientNewLoginKey, HandleLoginKey },
                { EMsg.ClientSessionToken, HandleSessionToken },
                { EMsg.ClientUpdateMachineAuth, HandleUpdateMachineAuth },
                { EMsg.ClientAccountInfo, HandleAccountInfo },
                { EMsg.ClientEmailAddrInfo, HandleEmailAddrInfo },
                { EMsg.ClientWalletInfoUpdate, HandleWalletInfo },
                { EMsg.ClientRequestWebAPIAuthenticateUserNonceResponse, HandleWebAPIUserNonce },
                { EMsg.ClientMarketingMessageUpdate2, HandleMarketingMessageUpdate },
            };
        }

        static SteamUser()
        {
            HardwareUtils.Init();
        }


        /// <summary>
        /// Logs the client into the Steam3 network.
        /// The client should already have been connected at this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        /// <exception cref="ArgumentNullException">No logon details were provided.</exception>
        /// <exception cref="ArgumentException">Username or password are not set within <paramref name="details"/>.</exception>
        public void LogOn( LogOnDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( nameof(details) );
            }
            if ( string.IsNullOrEmpty( details.Username ) || ( string.IsNullOrEmpty( details.Password ) && string.IsNullOrEmpty( details.LoginKey ) ) )
            {
                throw new ArgumentException( "LogOn requires a username and password to be set in 'details'." );
            }
            if ( !string.IsNullOrEmpty( details.LoginKey ) && !details.ShouldRememberPassword )
            {
                // Prevent consumers from screwing this up.
                // If should_remember_password is false, the login_key is ignored server-side.
                // The inverse is not applicable (you can log in with should_remember_password and no login_key).
                throw new ArgumentException( "ShouldRememberPassword is required to be set to true in order to use LoginKey." );
            }
            if ( !this.Client.IsConnected )
            {
                this.Client.PostCallback( new LoggedOnCallback( EResult.NoConnection ) );
                return;
            }

            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID steamId = new SteamID( details.AccountID, details.AccountInstance, Client.Universe, EAccountType.Individual );

            if ( details.LoginID.HasValue )
            {
                // TODO: Support IPv6 login ids?
                logon.Body.obfuscated_private_ip = new CMsgIPAddress
                {
                    v4 = details.LoginID.Value
                };
            }
            else
            {
                logon.Body.obfuscated_private_ip = NetHelpers.GetMsgIPAddress( this.Client.LocalIP! ).ObfuscatePrivateIP();
            }

            // Legacy field, Steam client still sets it
            if ( logon.Body.obfuscated_private_ip.ShouldSerializev4() )
            {
                logon.Body.deprecated_obfustucated_private_ip = logon.Body.obfuscated_private_ip.v4;
            }

            logon.ProtoHeader.client_sessionid = 0;
            logon.ProtoHeader.steamid = steamId.ConvertToUInt64();

            logon.Body.account_name = details.Username;
            logon.Body.password = details.Password;
            logon.Body.should_remember_password = details.ShouldRememberPassword;

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Body.client_os_type = ( uint )details.ClientOSType;
            logon.Body.client_language = details.ClientLanguage;
            logon.Body.cell_id = details.CellID ?? Client.Configuration.CellID;

            logon.Body.steam2_ticket_request = details.RequestSteam2Ticket;

            // we're now using the latest steamclient package version, this is required to get a proper sentry file for steam guard
            logon.Body.client_package_version = 1771; // todo: determine if this is still required
            logon.Body.supports_rate_limit_response = true;
            logon.Body.machine_id = HardwareUtils.GetMachineID();

            // steam guard 
            logon.Body.auth_code = details.AuthCode;
            logon.Body.two_factor_code = details.TwoFactorCode;

            logon.Body.login_key = details.LoginKey;

            logon.Body.sha_sentryfile = details.SentryFileHash;
            logon.Body.eresult_sentryfile = ( int )( details.SentryFileHash != null ? EResult.OK : EResult.FileNotFound );


            this.Client.Send( logon );
        }

        /// <summary>
        /// Logs the client into the Steam3 network as an anonymous user.
        /// The client should already have been connected at this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        public void LogOnAnonymous()
        {
            LogOnAnonymous( new AnonymousLogOnDetails() );
        }
        /// <summary>
        /// Logs the client into the Steam3 network as an anonymous user.
        /// The client should already have been connected at this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        /// <param name="details">The details to use for logging on.</param>
        public void LogOnAnonymous( AnonymousLogOnDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( nameof(details) );
            }

            if ( !this.Client.IsConnected )
            {
                this.Client.PostCallback( new LoggedOnCallback( EResult.NoConnection ) );
                return;
            }

            var logon = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

            SteamID auId = new SteamID( 0, 0, Client.Universe, EAccountType.AnonUser );

            logon.ProtoHeader.client_sessionid = 0;
            logon.ProtoHeader.steamid = auId.ConvertToUInt64();

            logon.Body.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Body.client_os_type = ( uint )details.ClientOSType;
            logon.Body.client_language = details.ClientLanguage;
            logon.Body.cell_id = details.CellID ?? Client.Configuration.CellID;

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
        /// Sends a machine auth response.
        /// This should normally be used in response to a <see cref="UpdateMachineAuthCallback"/>.
        /// </summary>
        /// <param name="details">The details pertaining to the response.</param>
        public void SendMachineAuthResponse( MachineAuthDetails details )
        {
            if ( details == null )
            {
                throw new ArgumentNullException( nameof(details) );
            }

            var response = new ClientMsgProtobuf<CMsgClientUpdateMachineAuthResponse>( EMsg.ClientUpdateMachineAuthResponse );

            // so we respond to the correct message
            if ( details.JobID != null )
            {
                response.ProtoHeader.jobid_target = details.JobID;
            }

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
        /// Requests a new WebAPI authentication user nonce.
        /// Results are returned in a <see cref="WebAPIUserNonceCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="WebAPIUserNonceCallback"/>.</returns>
        public AsyncJob<WebAPIUserNonceCallback> RequestWebAPIUserNonce()
        {
            var reqMsg = new ClientMsgProtobuf<CMsgClientRequestWebAPIAuthenticateUserNonce>( EMsg.ClientRequestWebAPIAuthenticateUserNonce );
            reqMsg.SourceJobID = Client.GetNextJobID();

            this.Client.Send( reqMsg );

            return new AsyncJob<WebAPIUserNonceCallback>( this.Client, reqMsg.SourceJobID );
        }

        /// <summary>
        /// Accepts the new Login Key provided by a <see cref="LoginKeyCallback"/>.
        /// </summary>
        /// <param name="callback">The callback containing the new Login Key.</param>
        public void AcceptNewLoginKey( LoginKeyCallback callback )
        {
            if ( callback == null )
            {
                throw new ArgumentNullException( nameof(callback) );
            }

            var acceptance = new ClientMsgProtobuf<CMsgClientNewLoginKeyAccepted>( EMsg.ClientNewLoginKeyAccepted );
            acceptance.Body.unique_id = callback.UniqueID;

            this.Client.Send( acceptance );
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
        void HandleLoggedOff( IPacketMsg packetMsg )
        {
            EResult result = EResult.Invalid;

            if ( packetMsg.IsProto )
            {
                var loggedOff = new ClientMsgProtobuf<CMsgClientLoggedOff>( packetMsg );
                result = ( EResult )loggedOff.Body.eresult;
            }
            else
            {
                var loggedOff = new ClientMsg<MsgClientLoggedOff>( packetMsg );
                result = loggedOff.Body.Result;
            }

            this.Client.PostCallback( new LoggedOffCallback( result ) );
        }
        void HandleUpdateMachineAuth( IPacketMsg packetMsg )
        {
            var machineAuth = new ClientMsgProtobuf<CMsgClientUpdateMachineAuth>( packetMsg );

            var callback = new UpdateMachineAuthCallback(packetMsg.SourceJobID, machineAuth.Body);
            Client.PostCallback( callback );
        }
        void HandleSessionToken( IPacketMsg packetMsg )
        {
            var sessToken = new ClientMsgProtobuf<CMsgClientSessionToken>( packetMsg );

            var callback = new SessionTokenCallback( sessToken.Body );
            this.Client.PostCallback( callback );
        }
        void HandleLoginKey( IPacketMsg packetMsg )
        {
            var loginKey = new ClientMsgProtobuf<CMsgClientNewLoginKey>( packetMsg );

            var callback = new LoginKeyCallback( loginKey.Body );
            this.Client.PostCallback( callback );
        }
        void HandleLogOnResponse( IPacketMsg packetMsg )
        {
            if ( packetMsg.IsProto )
            {
                var logonResp = new ClientMsgProtobuf<CMsgClientLogonResponse>( packetMsg );

                var callback = new LoggedOnCallback( logonResp.Body );
                this.Client.PostCallback( callback );
            }
            else
            {
                var logonResp = new ClientMsg<MsgClientLogOnResponse>( packetMsg );

                var callback = new LoggedOnCallback( logonResp.Body );
                this.Client.PostCallback( callback );
            }
        }
        void HandleAccountInfo( IPacketMsg packetMsg )
        {
            var accInfo = new ClientMsgProtobuf<CMsgClientAccountInfo>( packetMsg );

            var callback = new AccountInfoCallback( accInfo.Body );
            this.Client.PostCallback( callback );
        }
        void HandleEmailAddrInfo(IPacketMsg packetMsg)
        {
            var emailAddrInfo = new ClientMsgProtobuf<CMsgClientEmailAddrInfo>(packetMsg);
            var callback = new EmailAddrInfoCallback(emailAddrInfo.Body);
            this.Client.PostCallback(callback);
        }
        void HandleWalletInfo( IPacketMsg packetMsg )
        {
            var walletInfo = new ClientMsgProtobuf<CMsgClientWalletInfoUpdate>( packetMsg );

            var callback = new WalletInfoCallback( walletInfo.Body );
            this.Client.PostCallback( callback );
        }
        void HandleWebAPIUserNonce( IPacketMsg packetMsg )
        {
            var userNonce = new ClientMsgProtobuf<CMsgClientRequestWebAPIAuthenticateUserNonceResponse>( packetMsg );
            var callback = new WebAPIUserNonceCallback(userNonce.TargetJobID, userNonce.Body);
            this.Client.PostCallback( callback );
        }
        void HandleMarketingMessageUpdate( IPacketMsg packetMsg )
        {
            var marketingMessage = new ClientMsg<MsgClientMarketingMessageUpdate2>( packetMsg );

            byte[] payload = marketingMessage.Payload.ToArray();

            var callback = new MarketingMessageCallback( marketingMessage.Body, payload );
            this.Client.PostCallback( callback );
        }
        #endregion
    }
}
