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
    public partial class SteamUser
    {
        /// <summary>
        /// This callback is returned in response to an attempt to log on to the Steam3 network through <see cref="SteamUser"/>.
        /// </summary>
        public sealed class LogOnCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the logon.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the extended result of the logon.
            /// </summary>
            /// <vlaue>The result.</vlaue>
            public EResult ExtendedResult { get; private set; }

            /// <summary>
            /// Gets the out of game secs per heartbeat value. This is used internally.
            /// </summary>
            /// <value>The out of game secs per heartbeat.</value>
            public int OutOfGameSecsPerHeartbeat { get; private set; }
            /// <summary>
            /// Gets the in game secs per heartbeat value. This is used internally.
            /// </summary>
            /// <value>The in game secs per heartbeat.</value>
            public int InGameSecsPerHeartbeat { get; private set; }

            /// <summary>
            /// Gets or sets the public IP of the client
            /// </summary>
            /// <value>The public IP.</value>
            public IPAddress PublicIP { get; private set; }

            /// <summary>
            /// Gets the Steam3 server time.
            /// </summary>
            /// <value>The server time.</value>
            public DateTime ServerTime { get; private set; }

            /// <summary>
            /// Gets the account flags assigned by the server.
            /// </summary>
            /// <value>The account flags.</value>
            public EAccountFlags AccountFlags { get; private set; }

            /// <summary>
            /// Gets the client steam ID.
            /// </summary>
            /// <value>The client steam ID.</value>
            public SteamID ClientSteamID { get; private set; }

            /// <summary>
            /// Gets the email domain.
            /// </summary>
            /// <value>The email domain.</value>
            public string EmailDomain { get; private set; }

            /// <summary>
            /// Gets the Steam2 CellID.
            /// </summary>
            public uint CellID { get; private set; }

            /// <summary>
            /// Gets the Steam2 ticket.
            /// </summary>
            public byte[] Steam2Ticket { get; private set; }


#if STATIC_CALLBACKS
            internal LogOnCallback( SteamClient client, CMsgClientLogonResponse resp )
                : base( client )
#else
            internal LogOnCallback( CMsgClientLogonResponse resp )
#endif
            {
                this.Result = ( EResult )resp.eresult;
                this.ExtendedResult =(EResult)resp.eresult_extended;

                this.OutOfGameSecsPerHeartbeat = resp.out_of_game_heartbeat_seconds;
                this.InGameSecsPerHeartbeat = resp.in_game_heartbeat_seconds;

                this.PublicIP = NetHelpers.GetIPAddress( resp.public_ip );

                this.ServerTime = Utils.DateTimeFromUnixTime( resp.rtime32_server_time );

                this.AccountFlags = ( EAccountFlags )resp.account_flags;

                this.ClientSteamID = new SteamID( resp.client_supplied_steamid );

                this.EmailDomain = resp.email_domain;

                this.CellID = resp.cell_id;

                this.Steam2Ticket = resp.steam2_ticket;
            }
        }

        /// <summary>
        /// This callback is returned in response to a log off attempt, or when the client is told to log off by the server.
        /// </summary>
        public sealed class LoggedOffCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the log off.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

#if STATIC_CALLBACKS
            internal LoggedOffCallback( SteamClient client, CMsgClientLoggedOff resp )
                : base( client )
#else
            internal LoggedOffCallback( CMsgClientLoggedOff resp )
#endif
            {
                this.Result = ( EResult )resp.eresult;
            }
        }

        /// <summary>
        /// This callback is returned in response to a log on attempt. After this callback it is safe to change the client persona state to online.
        /// </summary>
        public sealed class LoginKeyCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the login key.
            /// </summary>
            /// <value>The login key.</value>
            public byte[] LoginKey { get; private set; }
            /// <summary>
            /// Gets the unique ID.
            /// </summary>
            /// <value>The unique ID.</value>
            public uint UniqueID { get; private set; }

#if STATIC_CALLBACKS
            internal LoginKeyCallback( SteamClient client, MsgClientNewLoginKey logKey )
                : base( client )
#else
            internal LoginKeyCallback( MsgClientNewLoginKey logKey )
#endif
            {
                this.LoginKey = logKey.LoginKey;
                this.UniqueID = logKey.UniqueID;
            }
        }

        /// <summary>
        /// This callback is fired when the client recieves it's unique Steam3 session token. This token is used for authenticated content downloading in Steam2.
        /// </summary>
        public sealed class SessionTokenCallback : CallbackMsg
        {
            public ulong SessionToken { get; private set; }

#if STATIC_CALLBACKS
            internal SessionTokenCallback( SteamClient client, CMsgClientSessionToken msg )
                : base( client )
#else
            internal SessionTokenCallback( CMsgClientSessionToken msg )
#endif
            {
                this.SessionToken = msg.token;
            }
        }

        public sealed class AccountInfoCallback : CallbackMsg
        {
            public string PersonaName { get; private set; }
            public string Country { get; private set; }

            public byte[] PasswordSalt { get; private set; }
            public byte[] PasswordSHADisgest { get; private set; }

            public int CountAuthedComputers { get; private set; }
            public bool LockedWithIPT { get; private set; }

            public EAccountFlags AccountFlags { get; private set; }

            public ulong FacebookID { get; private set; }
            public string FacebookName { get; private set ;}

#if STATIC_CALLBACKS
            internal AccountInfoCallback( SteamClient client, CMsgClientAccountInfo msg )
                : base( client )
#else
            internal AccountInfoCallback( CMsgClientAccountInfo msg )
#endif
            {
                PersonaName = msg.persona_name;
                Country = msg.ip_country;

                PasswordSalt = msg.salt_password;
                PasswordSHADisgest = msg.sha_digest_Password;

                CountAuthedComputers = msg.count_authed_computers;
                LockedWithIPT = msg.locked_with_ipt;

                AccountFlags = ( EAccountFlags )msg.account_flags;

                FacebookID = msg.facebook_id;
                FacebookName = msg.facebook_name;
            }
        }

        public sealed class WalletInfoCallback : CallbackMsg
        {
            public bool HasWallet { get; private set; }

            public int Currency { get; private set; }
            public int Balance { get; private set; }


#if STATIC_CALLBACKS
            internal WalletInfoCallback( SteamClient client, CMsgClientWalletInfoUpdate wallet )
                : base( client )
#else
            internal WalletInfoCallback( CMsgClientWalletInfoUpdate wallet )
#endif
            {
                HasWallet = wallet.has_wallet;

                Currency = wallet.currency;
                Balance = wallet.balance;
            }
        }
    }
}
