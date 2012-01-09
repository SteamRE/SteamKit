/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.ObjectModel;

namespace SteamKit2
{
    public sealed partial class SteamMasterServer
    {
        public sealed class QueryCallback : CallbackMsg
        {
            public sealed class Server
            {
                public IPAddress Address { get; private set; }
                public ushort Port { get; private set; }

                public uint AuthedPlayers { get; private set; }


                internal Server( CMsgGMSClientServerQueryResponse.Server server )
                {
                    Address = NetHelpers.GetIPAddress( server.server_ip );
                    Port = ( ushort )server.server_port;

                    AuthedPlayers = server.auth_players;
                }
            }

            public ReadOnlyCollection<Server> Servers { get; private set; }

#if STATIC_CALLBACKS
            internal QueryCallback( SteamClient client, CMsgGMSClientServerQueryResponse msg )
                : base( client )
#else
            internal QueryCallback( CMsgGMSClientServerQueryResponse msg )
#endif
            {
                var serverList = msg.servers
                    .Select( s => new Server( s ) )
                    .ToList();

                this.Servers = new ReadOnlyCollection<Server>( serverList );
            }
        }
    }
}
