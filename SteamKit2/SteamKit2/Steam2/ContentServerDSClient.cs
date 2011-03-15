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
    /// <summary>
    /// This client is capable of connecting to the content server directory server.
    /// This directory server is used for the sole purpose of getting a list of content servers.
    /// </summary>
    public sealed class ContentServerDSClient : DSClient
    {
        /// <summary>
        /// Gets a list of all currently active content servers.
        /// </summary>
        /// <returns>A list of servers on success; otherwise, <c>null</c>.</returns>
        public IPEndPoint[] GetContentServerList()
        {
            TcpPacket packet = base.GetRawServerList( ( EServerType )3 );

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

        /// <summary>
        /// Gets a list of content servers that provide specific content.
        /// </summary>
        /// <param name="depotId">The depot ID of the content you wish to request.</param>
        /// <param name="depotVersion">The version ID of the content you wish to request.</param>
        /// <param name="cellId">A cell ID of the preferred server.</param>
        /// <returns>A list of servers on success; otherwise, <c>null</c>.</returns>
        public IPEndPoint[] GetContentServerList( uint depotId, uint depotVersion, uint cellId )
        {
            TcpPacket packet = base.GetRawServerList(
                ( EServerType )0, // command 0
                ( ushort )1, // unknown
                depotId,
                depotVersion,
                ( ushort )10, // num servers, 10 should be a good enough amount
                cellId,
                unchecked( ( ulong )-1 )
            );

            if ( packet == null )
                return null;

            DataStream ds = new DataStream( packet.GetPayload(), true );

            ushort numAddrs = ds.ReadUInt16();

            IPEndPoint[] serverList = new IPEndPoint[ numAddrs ];
            for ( int x = 0 ; x < numAddrs ; ++x )
            {

                uint contentServerId = ds.ReadUInt32();

                IPAddrPort ipAddr = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );
                IPAddrPort ipAddr2 = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );

                serverList[ x ] = ipAddr;

            }

            return serverList;
        }
    }
}
