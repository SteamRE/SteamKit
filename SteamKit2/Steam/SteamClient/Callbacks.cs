/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamClient
    {
        /// <summary>
        /// This callback is received after attempting to connect to the Steam network.
        /// </summary>
        public sealed class ConnectedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }


            internal ConnectedCallback( MsgChannelEncryptResult result )
                : this( result.Result )
            {
            }

            internal ConnectedCallback( EResult result )
            {
                this.Result = result;
            }
        }


        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
            /// <summary>
            /// If true, the disconnection was initiated by calling <see cref="CMClient.Disconnect"/>.
            /// If false, the disconnection was the cause of something not user-controlled, such as a network failure or
            /// a forcible disconnection by the remote server.
            /// </summary>
            public bool UserInitiated { get; private set; }

            internal DisconnectedCallback( bool userInitiated )
            {
                this.UserInitiated = userInitiated;
            }
        }


        /// <summary>
        /// This callback is received when the client has received the CM list from Steam.
        /// </summary>
        public sealed class CMListCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the CM server list.
            /// </summary>
            public ReadOnlyCollection<IPEndPoint> Servers { get; private set; }


            internal CMListCallback( CMsgClientCMList cmMsg )
            {
                var cmList = cmMsg.cm_addresses
                    .Zip( cmMsg.cm_ports, ( addr, port ) => new IPEndPoint( NetHelpers.GetIPAddress( addr ), ( int )port ) );

                Servers = new ReadOnlyCollection<IPEndPoint>( cmList.ToList() );
            }
        }

        /// <summary>
        /// This callback is fired when the client receives a list of all publically available Steam3 servers.
        /// This callback may be fired multiple times for different server lists.
        /// </summary>
        public sealed class ServerListCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the server list.
            /// </summary>
            public Dictionary<EServerType, ReadOnlyCollection<IPEndPoint>> Servers { get; private set; }


            internal ServerListCallback( CMsgClientServerList serverList )
            {
                Servers = serverList.servers
                    .Select( s => new { Type = ( EServerType )s.server_type, EndPoint = new IPEndPoint( NetHelpers.GetIPAddress( s.server_ip ), ( int )s.server_port ) } )
                    .GroupBy( s => s.Type )
                    .ToDictionary( grp => grp.Key, grp => new ReadOnlyCollection<IPEndPoint>( grp.Select( s => s.EndPoint ).ToList() ) );
            }
        }
    }
}