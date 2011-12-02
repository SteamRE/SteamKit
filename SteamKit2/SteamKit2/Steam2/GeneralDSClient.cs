/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

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


        /// <summary>
        /// Gets an auth server list for a specific username.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <returns>A list of servers on success; otherwise, <c>null</c>.</returns>
        public IPEndPoint[] GetAuthServerList( string userName )
        {
            userName = userName.ToLower();

            byte[] userHash = CryptoHelper.JenkinsHash( Encoding.ASCII.GetBytes( userName ) );
            uint userData = BitConverter.ToUInt32( userHash, 0 ) & 1;

            TcpPacket packet = base.GetRawServerList( EServerType.ProxyASClientAuthentication, NetHelpers.EndianSwap( userData ) );

            if ( packet == null )
                return null;

            DataStream ds = new DataStream( packet.GetPayload(), true );

            ushort numAddrs = ds.ReadUInt16();

            IPEndPoint[] serverList = new IPEndPoint[ numAddrs ];
            for ( int x = 0 ; x < numAddrs ; ++x )
            {
                IPAddrPort ipAddr = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );

                serverList[ x ] = ipAddr;
            }

            return serverList;
        }
        
    }
}
