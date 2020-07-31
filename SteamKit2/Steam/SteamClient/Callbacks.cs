/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using SteamKit2.Discovery;
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
            internal ConnectedCallback()
            {
            }
        }


        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
            /// <summary>
            /// If true, the disconnection was initiated by calling <see cref="CMClient.Disconnect()"/>.
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
            public ReadOnlyCollection<ServerRecord> Servers { get; private set; }


            internal CMListCallback( CMsgClientCMList cmMsg )
            {
                var cmList = cmMsg.cm_addresses
                    .Zip( cmMsg.cm_ports, ( addr, port ) => ServerRecord.CreateSocketServer( new IPEndPoint( NetHelpers.GetIPAddress( addr ), ( int )port ) ) );

                var websocketList = cmMsg.cm_websocket_addresses.Select( ( addr ) => ServerRecord.CreateWebSocketServer( addr ) );

                Servers = new ReadOnlyCollection<ServerRecord>( cmList.Concat( websocketList ).ToList() );
            }
        }
    }
}
