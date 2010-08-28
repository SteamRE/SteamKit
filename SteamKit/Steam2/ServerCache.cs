using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SteamLib
{

    class ServerList
    {
        List<IPEndPoint> servers;

        public EServerType Type { get; private set; }

        public ServerList( EServerType type )
        {
            this.Type = type;
            this.servers = new List<IPEndPoint>();
        }

        public void AddServer( IPEndPoint endPoint )
        {
            foreach ( IPEndPoint server in servers )
            {
                if ( server.Address.Equals( endPoint.Address ) && server.Port == endPoint.Port )
                    return;
            }

            servers.Add( endPoint );
        }

        public IPEndPoint[] GetServers()
        {
            return servers.ToArray();
        }
    }

    static class ServerCache
    {
        static Dictionary<EServerType, ServerList> serverMap;

        static ServerCache()
        {
            serverMap = new Dictionary<EServerType, ServerList>();
        }

        public static void AddServer( EServerType type, IPEndPoint server )
        {
            if ( !serverMap.ContainsKey( type ) )
                serverMap.Add( type, new ServerList( type ) );

            serverMap[ type ].AddServer( server );
        }

        public static void AddServers( EServerType type, IPEndPoint[] servers )
        {
            if ( servers == null )
                return;

            foreach ( IPEndPoint endPoint in servers )
                AddServer( type, endPoint );
        }

        public static ServerList GetServers( EServerType type )
        {
            if ( !serverMap.ContainsKey( type ) )
                return null;

            return serverMap[ type ];
        }

        public static ServerList[] GetServerLists()
        {
            List<ServerList> serverLists = new List<ServerList>();

            foreach ( var kvp in serverMap )
                serverLists.Add( kvp.Value );

            return serverLists.ToArray();
        }


    }
}
