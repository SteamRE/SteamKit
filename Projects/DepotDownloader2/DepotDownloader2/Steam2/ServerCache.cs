/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SteamKit2;

namespace DepotDownloader2
{
    class ServerCache
    {
        public static ServerList ConfigServers { get; private set; }
        public static ServerList CSDSServers { get; private set; }


        static ServerCache()
        {
            ConfigServers = new ServerList();
            CSDSServers = new ServerList();
        }


        public static void Build()
        {
            Log.WriteLine( "Building Steam2 server cache..." );

            foreach ( var gdServer in GeneralDSClient.GDServers )
            {
                BuildServer( gdServer, EServerType.ConfigServer, ConfigServers );
                BuildServer( gdServer, EServerType.CSDS, CSDSServers );
            }

            Log.WriteLine( "Done! {0} Config Servers, {1} CSDS Servers.\n", ConfigServers.Count, CSDSServers.Count );
        }

        static void BuildServer( IPEndPoint gdServer, EServerType serverType, ServerList listToBuild )
        {
            try
            {
                GeneralDSClient gdsClient = new GeneralDSClient();
                gdsClient.Connect( gdServer );

                IPEndPoint[] servers = gdsClient.GetServerList( serverType );

                listToBuild.AddRange( servers );

                gdsClient.Disconnect();
            }
            catch ( Exception ex )
            {
                Log.WriteVerbose( "Warning: Unable to connect to GDS {0} to get a list of {1} servers: {2}", gdServer, serverType, ex.Message );
            }
        }
    }


    class ServerList : List<IPEndPoint>
    {
        public new void AddRange( IEnumerable<IPEndPoint> endPoints )
        {
            foreach ( var endPoint in endPoints )
            {
                Add( endPoint );
            }
        }

        public new void Add( IPEndPoint endPoint )
        {
            if ( base.Contains( endPoint ) )
                return;

            base.Add( endPoint );
        }
    }
}
