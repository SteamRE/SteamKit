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
            /// Gets or sets the 'Should Remember Password' flag. This is used in combination with the <see cref="AccessToken"/> for password-less login.
            /// Set this to true when <see cref="Authentication.AuthSessionDetails.IsPersistentSession"/> is set to true.
            /// </summary>
            /// <value>The 'Should Remember Password' flag.</value>
            public bool ShouldRememberPassword { get; set; }
            /// <summary>
            /// Gets or sets the access token used to login. This a token that has been provided after a successful login using <see cref="Authentication"/>.
            /// </summary>
            /// <value>The access token.</value>
            public string? AccessToken { get; set; }

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
            /// Gets or sets the machine name.
            /// </summary>
            /// <value>The machine name.</value>
            public string? MachineName { get; set; } = $"{Environment.MachineName} (SteamKit2)";

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
                { EMsg.ClientSessionToken, HandleSessionToken },
                { EMsg.ClientAccountInfo, HandleAccountInfo },
                { EMsg.ClientEmailAddrInfo, HandleEmailAddrInfo },
                { EMsg.ClientWalletInfoUpdate, HandleWalletInfo },
                { EMsg.ClientRequestWebAPIAuthenticateUserNonceResponse, HandleWebAPIUserNonce },
                { EMsg.ClientVanityURLChangedNotification, HandleVanityURLChangedNotification },
                { EMsg.ClientMarketingMessageUpdate2, HandleMarketingMessageUpdate },
                { EMsg.ClientPlayingSessionState, HandlePlayingSessionState },
            };
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
            ArgumentNullException.ThrowIfNull( details );

            if ( string.IsNullOrEmpty( details.Username ) || ( string.IsNullOrEmpty( details.Password ) && string.IsNullOrEmpty( details.AccessToken ) ) )
            {
                throw new ArgumentException( "LogOn requires a username and password or access token to be set in 'details'." );
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
            logon.Body.machine_name = details.MachineName;
            logon.Body.machine_id = HardwareUtils.GetMachineID( Client.Configuration.MachineInfoProvider );

            // steam guard 
            logon.Body.auth_code = details.AuthCode;
            logon.Body.two_factor_code = details.TwoFactorCode;

            logon.Body.access_token = details.AccessToken;

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
            ArgumentNullException.ThrowIfNull( details );

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

            logon.Body.machine_id = HardwareUtils.GetMachineID( Client.Configuration.MachineInfoProvider );

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
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            ArgumentNullException.ThrowIfNull( packetMsg );

            if ( !dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc ) )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleLoggedOff( IPacketMsg packetMsg )
        {
            EResult result;

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
        void HandleSessionToken( IPacketMsg packetMsg )
        {
            var sessToken = new ClientMsgProtobuf<CMsgClientSessionToken>( packetMsg );

            var callback = new SessionTokenCallback( sessToken.Body );
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
        void HandleEmailAddrInfo( IPacketMsg packetMsg )
        {
            var emailAddrInfo = new ClientMsgProtobuf<CMsgClientEmailAddrInfo>( packetMsg );
            var callback = new EmailAddrInfoCallback( emailAddrInfo.Body );
            this.Client.PostCallback( callback );
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
            var callback = new WebAPIUserNonceCallback( userNonce.TargetJobID, userNonce.Body );
            this.Client.PostCallback( callback );
        }
        void HandleVanityURLChangedNotification( IPacketMsg packetMsg )
        {
            var vanityUrl = new ClientMsgProtobuf<CMsgClientVanityURLChangedNotification>( packetMsg );
            var callback = new VanityURLChangedCallback( vanityUrl.TargetJobID, vanityUrl.Body );
            this.Client.PostCallback( callback );
        }
        void HandleMarketingMessageUpdate( IPacketMsg packetMsg )
        {
            var marketingMessage = new ClientMsg<MsgClientMarketingMessageUpdate2>( packetMsg );

            byte[] payload = marketingMessage.Payload.ToArray();

            var callback = new MarketingMessageCallback( marketingMessage.Body, payload );
            this.Client.PostCallback( callback );
        }
        void HandlePlayingSessionState( IPacketMsg packetMsg )
        {
            var playingSessionState = new ClientMsgProtobuf<CMsgClientPlayingSessionState>( packetMsg );

            this.Client.PostCallback( new PlayingSessionStateCallback( packetMsg.TargetJobID, playingSessionState.Body ) );
        }
        #endregion
    }
}
