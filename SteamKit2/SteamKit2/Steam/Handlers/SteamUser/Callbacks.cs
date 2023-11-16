/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using ProtoBuf;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamUser
    {
        /// <summary>
        /// This callback is returned in response to an attempt to log on to the Steam3 network through <see cref="SteamUser"/>.
        /// </summary>
        public sealed class LoggedOnCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the logon.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the extended result of the logon.
            /// </summary>
            public EResult ExtendedResult { get; private set; }

            /// <summary>
            /// Gets the out of game secs per heartbeat value.
            /// This is used internally by SteamKit to initialize heartbeating.
            /// </summary>
            public int OutOfGameSecsPerHeartbeat { get; private set; }
            /// <summary>
            /// Gets the in game secs per heartbeat value.
            /// This is used internally by SteamKit to initialize heartbeating.
            /// </summary>
            public int InGameSecsPerHeartbeat { get; private set; }

            /// <summary>
            /// Gets or sets the public IP of the client
            /// </summary>
            public IPAddress? PublicIP { get; private set; }

            /// <summary>
            /// Gets the Steam3 server time.
            /// </summary>
            public DateTime ServerTime { get; private set; }

            /// <summary>
            /// Gets the account flags assigned by the server.
            /// </summary>
            public EAccountFlags AccountFlags { get; private set; }

            /// <summary>
            /// Gets the client steam ID.
            /// </summary>
            public SteamID? ClientSteamID { get; private set; }

            /// <summary>
            /// Gets the email domain.
            /// </summary>
            public string? EmailDomain { get; private set; }

            /// <summary>
            /// Gets the Steam2 CellID.
            /// </summary>
            public uint CellID { get; private set; }

            /// <summary>
            /// Gets the Steam2 CellID ping threshold.
            /// </summary>
            public uint CellIDPingThreshold { get; private set; }

            /// <summary>
            /// Gets the Steam2 ticket.
            /// This is used for authenticated content downloads in Steam2.
            /// This field will only be set when <see cref="LogOnDetails.RequestSteam2Ticket"/> has been set to <c>true</c>.
            /// </summary>
            public byte[]? Steam2Ticket { get; private set; }

            /// <summary>
            /// Gets the IP country code.
            /// </summary>
            public string? IPCountryCode { get; private set; }

            /// <summary>
            /// Gets the vanity URL.
            /// </summary>
            public string? VanityURL { get; private set; }

            /// <summary>
            /// Gets the threshold for login failures before Steam wants the client to migrate to a new CM.
            /// </summary>
            public int NumLoginFailuresToMigrate { get; private set; }
            /// <summary>
            /// Gets the threshold for disconnects before Steam wants the client to migrate to a new CM.
            /// </summary>
            public int NumDisconnectsToMigrate { get; private set; }

            /// <summary>
            /// Gets the Steam parental settings.
            /// </summary>
            public ParentalSettings? ParentalSettings { get; private set; }

            internal LoggedOnCallback( CMsgClientLogonResponse resp )
            {
                this.Result = ( EResult )resp.eresult;
                this.ExtendedResult = ( EResult )resp.eresult_extended;

                this.OutOfGameSecsPerHeartbeat = resp.legacy_out_of_game_heartbeat_seconds;
                this.InGameSecsPerHeartbeat = resp.heartbeat_seconds;

                this.PublicIP = resp.public_ip?.GetIPAddress();

                this.ServerTime = DateUtils.DateTimeFromUnixTime( resp.rtime32_server_time );

                this.AccountFlags = ( EAccountFlags )resp.account_flags;

                this.ClientSteamID = new SteamID( resp.client_supplied_steamid );

                this.EmailDomain = resp.email_domain;

                this.CellID = resp.cell_id;
                this.CellIDPingThreshold = resp.cell_id_ping_threshold;

                this.Steam2Ticket = resp.steam2_ticket;

                this.IPCountryCode = resp.ip_country_code;

                this.VanityURL = resp.vanity_url;

                this.NumLoginFailuresToMigrate = resp.count_loginfailures_to_migrate;
                this.NumDisconnectsToMigrate = resp.count_disconnects_to_migrate;

                if ( resp.parental_settings != null )
                {
                    using var ms = new MemoryStream( resp.parental_settings );
                    this.ParentalSettings = Serializer.Deserialize<ParentalSettings>( ms );
                }
            }


            internal LoggedOnCallback( MsgClientLogOnResponse resp )
            {
                this.Result = resp.Result;

                this.OutOfGameSecsPerHeartbeat = resp.OutOfGameHeartbeatRateSec;
                this.InGameSecsPerHeartbeat = resp.InGameHeartbeatRateSec;

                this.PublicIP = NetHelpers.GetIPAddress( resp.IpPublic );

                this.ServerTime = DateUtils.DateTimeFromUnixTime( resp.ServerRealTime );

                this.ClientSteamID = resp.ClientSuppliedSteamId;
            }


            internal LoggedOnCallback( EResult result )
            {
                this.Result = result;
            }
        }

        /// <summary>
        /// This callback is returned when the client is told to log off by the server.
        /// </summary>
        public sealed class LoggedOffCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the log off.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }


            internal LoggedOffCallback( EResult result )
            {
                this.Result = result;
            }
        }

        /// <summary>
        /// This callback is fired when the client recieves it's unique Steam3 session token. This token is used for authenticated content downloading in Steam2.
        /// </summary>
        public sealed class SessionTokenCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the Steam3 session token used for authenticating to various other services.
            /// </summary>
            public ulong SessionToken { get; private set; }


            internal SessionTokenCallback( CMsgClientSessionToken msg )
            {
                this.SessionToken = msg.token;
            }
        }

        /// <summary>
        /// This callback is received when account information is recieved from the network.
        /// This generally happens after logon.
        /// </summary>
        public sealed class AccountInfoCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the last recorded persona name used by this account.
            /// </summary>
            public string PersonaName { get; private set; }
            /// <summary>
            /// Gets the country this account is connected from.
            /// </summary>
            public string Country { get; private set; }

            /// <summary>
            /// Gets the count of SteamGuard authenticated computers.
            /// </summary>
            public int CountAuthedComputers { get; private set; }

            /// <summary>
            /// Gets the account flags for this account.
            /// </summary>
            public EAccountFlags AccountFlags { get; private set; }

            /// <summary>
            /// Gets the facebook ID of this account if it is linked with facebook.
            /// </summary>
            public ulong FacebookID { get; private set; }
            /// <summary>
            /// Gets the facebook name if this account is linked with facebook.
            /// </summary>
            public string FacebookName { get; private set; }


            internal AccountInfoCallback( CMsgClientAccountInfo msg )
            {
                PersonaName = msg.persona_name;
                Country = msg.ip_country;

                CountAuthedComputers = msg.count_authed_computers;

                AccountFlags = ( EAccountFlags )msg.account_flags;

                FacebookID = msg.facebook_id;
                FacebookName = msg.facebook_name;
            }
        }

        /// <summary>
        /// This callback is received when email information is recieved from the network.
        /// </summary>
        public sealed class EmailAddrInfoCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the email address of this account.
            /// </summary>
            public string EmailAddress { get; private set; }
            /// <summary>
            /// Gets a value indicating validated email or not.
            /// </summary>
            public bool IsValidated { get; private set; }

            internal EmailAddrInfoCallback( CMsgClientEmailAddrInfo msg )
            {
                EmailAddress = msg.email_address;
                IsValidated = msg.email_is_validated;
            }
        }

        /// <summary>
        /// This callback is received when wallet info is recieved from the network.
        /// </summary>
        public sealed class WalletInfoCallback : CallbackMsg
        {
            /// <summary>
            /// Gets a value indicating whether this instance has wallet data.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if this instance has wallet data; otherwise, <c>false</c>.
            /// </value>
            public bool HasWallet { get; private set; }

            /// <summary>
            /// Gets the currency code for this wallet.
            /// </summary>
            public ECurrencyCode Currency { get; private set; }

            /// <summary>
            /// Gets the balance of the wallet as a 32-bit integer, in cents.
            /// </summary>
            public int Balance { get; private set; }

            /// <summary>
            /// Gets the delayed (pending) balance of the wallet as a 32-bit integer, in cents.
            /// </summary>
            public int BalanceDelayed { get; private set; }

            /// <summary>
            /// Gets the balance of the wallet as a 64-bit integer, in cents.
            /// </summary>
            public long LongBalance { get; private set; }

            /// <summary>
            /// Gets the delayed (pending) balance of the wallet as a 64-bit integer, in cents.
            /// </summary>
            public long LongBalanceDelayed { get; private set; }

            internal WalletInfoCallback( CMsgClientWalletInfoUpdate wallet )
            {
                HasWallet = wallet.has_wallet;

                Currency = ( ECurrencyCode )wallet.currency;
                Balance = wallet.balance;
                BalanceDelayed = wallet.balance_delayed;
                LongBalance = wallet.balance64;
                LongBalanceDelayed = wallet.balance64_delayed;
            }
        }

        /// <summary>
        /// This callback is received when requesting a new WebAPI authentication user nonce.
        /// </summary>
        public sealed class WebAPIUserNonceCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the authentication nonce.
            /// </summary>
            public string Nonce { get; private set; }


            internal WebAPIUserNonceCallback( JobID jobID, CMsgClientRequestWebAPIAuthenticateUserNonceResponse body )
            {
                this.JobID = jobID;

                this.Result = ( EResult )body.eresult;
                this.Nonce = body.webapi_authenticate_user_nonce;
            }
        }

        /// <summary>
        /// This callback is received when users' vanity url changes.
        /// </summary>
        public sealed class VanityURLChangedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the new vanity url.
            /// </summary>
            public string VanityURL { get; private set; }


            internal VanityURLChangedCallback( JobID jobID, CMsgClientVanityURLChangedNotification body )
            {
                this.JobID = jobID;
                this.VanityURL = body.vanity_url;
            }
        }

        /// <summary>
        /// This callback is fired when the client receives a marketing message update.
        /// </summary>
        public sealed class MarketingMessageCallback : CallbackMsg
        {
            /// <summary>
            /// Represents a single marketing message.
            /// </summary>
            public sealed class Message
            {
                /// <summary>
                /// Gets the unique identifier for this marketing message.
                /// </summary>
                public GlobalID ID { get; private set; }

                /// <summary>
                /// Gets the URL for this marketing message.
                /// </summary>
                public string URL { get; private set; }

                /// <summary>
                /// Gets the marketing message flags.
                /// </summary>
                public EMarketingMessageFlags Flags { get; private set; }


                internal Message( byte[] data )
                {
                    using var ms = new MemoryStream( data );
                    using var br = new BinaryReader( ms );
                    ID = br.ReadUInt64();
                    URL = br.BaseStream.ReadNullTermString( Encoding.UTF8 );
                    Flags = ( EMarketingMessageFlags )br.ReadUInt32();
                }
            }


            /// <summary>
            /// Gets the time of this marketing message update.
            /// </summary>
            public DateTime UpdateTime { get; private set; }

            /// <summary>
            /// Gets the messages.
            /// </summary>
            public ReadOnlyCollection<Message> Messages { get; private set; }


            internal MarketingMessageCallback( MsgClientMarketingMessageUpdate2 body, byte[] payload )
            {
                UpdateTime = DateUtils.DateTimeFromUnixTime( body.MarketingMessageUpdateTime );

                var msgList = new List<Message>();

                using ( var ms = new MemoryStream( payload ) )
                using ( var br = new BinaryReader( ms ) )
                {
                    for ( int x = 0; x < body.Count; ++x )
                    {
                        int dataLen = br.ReadInt32() - 4; // total length includes the 4 byte length
                        byte[] messageData = br.ReadBytes( dataLen );

                        msgList.Add( new Message( messageData ) );
                    }
                }

                Messages = new ReadOnlyCollection<Message>( msgList );
            }
        }

        /// <summary>
        /// This callback is received when another client starts or stops playing a game.
        /// While blocked, sending ClientGamesPlayed message will log you off with LoggedInElsewhere result.
        /// </summary>
        public sealed class PlayingSessionStateCallback : CallbackMsg
        {
            /// <summary>
            /// Indicates whether playing is currently blocked by another client.
            /// </summary>
            public bool PlayingBlocked { get; private set; }
            /// <summary>
            /// When blocked, gets the appid which is currently being played.
            /// </summary>
            public uint PlayingAppID { get; private set; }

            internal PlayingSessionStateCallback( JobID jobID, CMsgClientPlayingSessionState msg )
            {
                JobID = jobID;
                PlayingBlocked = msg.playing_blocked;
                PlayingAppID = msg.playing_app;
            }
        }
    }
}
