/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Net;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// Represents a client that is capable of connecting to the Steam2 General Directory Server.
    /// </summary>
    public sealed class GeneralDSClient : DSClient
    {
        /// <summary>
        /// This is the boostrap list of General Directory Servers.
        /// </summary>
        public static readonly IPEndPoint[] GDServers = 
        {
            new IPEndPoint( IPAddress.Parse( "72.165.61.189" ), 27030 ), // gds1.steampowered.com
            new IPEndPoint( IPAddress.Parse( "72.165.61.190" ), 27030 ), // gds2.steampowered.com
            /*
            new IPEndPoint( IPAddress.Parse( "69.28.151.178" ), 27038 ),
            new IPEndPoint( IPAddress.Parse( "69.28.153.82" ), 27038 ),
            new IPEndPoint( IPAddress.Parse( "87.248.196.194" ), 27038 ),
            new IPEndPoint( IPAddress.Parse( "68.142.72.250" ), 27038 ),*/
        };
    }
}
