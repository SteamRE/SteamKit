using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SteamLib
{
    class GeneralDSClient : DSClient
    {
        public static IPEndPoint[] GDServers = 
        {
            new IPEndPoint( IPAddress.Parse( "72.165.61.189" ), 27030 ), // gds1.steampowered.com
            new IPEndPoint( IPAddress.Parse( "72.165.61.190" ), 27030 ), // gds2.steampowered.com

            new IPEndPoint( IPAddress.Parse( "69.28.151.178" ), 27038 ),
            new IPEndPoint( IPAddress.Parse( "69.28.153.82" ), 27038 ),
            new IPEndPoint( IPAddress.Parse( "87.248.196.194" ), 27038 ),
            new IPEndPoint( IPAddress.Parse( "68.142.72.250" ), 27038 ),
        };

        /*
        public static string[] GDServers =
        {
            "72.165.61.189:27030", // gds1.steampowered.com
            "72.165.61.190:27030", // gds2.steampowered.com
            "69.28.151.178:27038",
            "69.28.153.82:27038",
            "87.248.196.194:27038",
            "68.142.72.250:27038",
        };*/
    }
}
