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


            internal LogOnCallback( CMsgClientLogonResponse resp )
            {
                this.Result = ( EResult )resp.eresult;

                this.OutOfGameSecsPerHeartbeat = resp.out_of_game_heartbeat_seconds;
                this.InGameSecsPerHeartbeat = resp.in_game_heartbeat_seconds;

                this.PublicIP = NetHelpers.GetIPAddress( resp.public_ip );

                this.ServerTime = Utils.DateTimeFromUnixTime( resp.rtime32_server_time );

                this.AccountFlags = ( EAccountFlags )resp.account_flags;

                this.ClientSteamID = new SteamID( resp.client_supplied_steamid );

                this.EmailDomain = resp.email_domain;
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

            internal LoggedOffCallback( CMsgClientLoggedOff resp )
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

            internal LoginKeyCallback( MsgClientNewLoginKey logKey )
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

            internal SessionTokenCallback( CMsgClientSessionToken msg )
            {
                this.SessionToken = msg.token;
            }
        }
    }
}
