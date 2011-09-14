/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit3
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
            /// Gets the extended result.
            /// </summary>
            public EResult ExtendedResult { get; private set; }

            /// <summary>
            /// Gets the out of game secs per heartbeat value. This is used internally.
            /// </summary>
            public int OutOfGameSecsPerHeartbeat { get; private set; }
            /// <summary>
            /// Gets the in game secs per heartbeat value. This is used internally.
            /// </summary>
            public int InGameSecsPerHeartbeat { get; private set; }

            /// <summary>
            /// Gets or sets the public IP of the client
            /// </summary>
            public IPAddress PublicIP { get; private set; }

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
            public SteamID ClientSteamID { get; private set; }

            /// <summary>
            /// Gets the email domain.
            /// </summary>
            public string EmailDomain { get; private set; }

            /// <summary>
            /// Gets the Steam2 CellID.
            /// </summary>
            public uint CellID { get; private set; }
            /// <summary>
            /// Gets the Steam2 client ticket.
            /// </summary>
            public byte[] Steam2Ticket { get; private set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="LoggedOnCallback"/> callback.
            /// </summary>
            /// <param name="msg">The logon response to initialize this callback with.</param>
            internal LoggedOnCallback( CMsgClientLogonResponse msg )
            {
                this.Result = ( EResult )msg.eresult;

                this.ExtendedResult = (EResult)msg.eresult_extended;

                this.OutOfGameSecsPerHeartbeat = msg.out_of_game_heartbeat_seconds;
                this.InGameSecsPerHeartbeat = msg.in_game_heartbeat_seconds;

                this.PublicIP = new IPAddress( BitConverter.GetBytes( msg.public_ip ) );

                this.ServerTime = Utils.DateTimeFromUnix( msg.rtime32_server_time );

                this.AccountFlags = ( EAccountFlags )msg.account_flags;

                this.ClientSteamID = new SteamID( msg.client_supplied_steamid );

                this.EmailDomain = msg.email_domain;

                this.CellID = msg.cell_id;
                this.Steam2Ticket = msg.steam2_ticket;
            }
        }
    }
}
