/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Net;

namespace SteamKit2
{
    /// <summary>
    /// Represents the public facing information of a Steam2 content server.
    /// </summary>
    public sealed class ContentServer
    {
        /// <summary>
        /// Gets the load value of the content server.
        /// </summary>
        public uint Load { get; internal set; }

        /// <summary>
        /// Gets the <see cref="IPEndPoint"/> of the package server.
        /// </summary>
        public IPEndPoint PackageServer { get; internal set; } // this server supports querying cellid (handshake 3)
        /// <summary>
        /// Gets the <see cref="IPEndPoint"/> of the storage server.
        /// </summary>
        public IPEndPoint StorageServer { get; internal set; } // this server is used for heavy content bandwidth? (handshake 7)
    }

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
            TcpPacket packet = base.GetRawServerList( 3 ); // command 3

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
        /// <param name="cellid">A cell ID of the preferred server.</param>
        /// <param name="maxServers">The maximum number of servers to request.</param>
        /// <returns>
        /// A list of servers on success; otherwise, <c>null</c>.
        /// </returns>
        public ContentServer[] GetContentServerList( uint depotId, uint depotVersion, uint cellid = 0, ushort maxServers = 20 )
        {
            TcpPacket packet = null;

            if ( cellid != 0 )
            {
                packet = base.GetRawServerList(
                    ( byte )0, // command 0
                    ( ushort )0, // no cellid specified
                    depotId,
                    depotVersion,
                    maxServers, // num servers
                    UInt64.MaxValue
                );
            }
            else
            {
                packet = base.GetRawServerList(
                    ( byte )0, // command 0
                    ( ushort )1, // cellid is specified
                    depotId,
                    depotVersion,
                    maxServers, // num servers
                    cellid,
                    UInt64.MaxValue
                );
            }

            if ( packet == null )
                return null;

            return GetServersFromPacket( packet );

        }

        static ContentServer[] GetServersFromPacket( TcpPacket packet )
        {
            DataStream ds = new DataStream( packet.GetPayload(), true );

            ushort numAddrs = ds.ReadUInt16();

            ContentServer[] serverList = new ContentServer[ numAddrs ];
            for ( int x = 0 ; x < numAddrs ; ++x )
            {
                uint weighedLoad = ds.ReadUInt32();

                IPAddrPort ipAddr = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );
                IPAddrPort ipAddr2 = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );

                serverList[ x ] = new ContentServer()
                {
                    Load = weighedLoad,
                    PackageServer = ipAddr,
                    StorageServer = ipAddr2,
                };

            }

            return serverList;
        }
    }
}
