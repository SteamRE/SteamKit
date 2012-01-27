/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace SteamKit2
{
    /// <summary>
    /// Represents a client capable of connecting to a generic Directory Server.
    /// </summary>
    public class DSClient : ServerClient
    {
        /// <summary>
        /// Gets the server list for a specific server type.
        /// </summary>
        /// <param name="type">The server type.</param>
        /// <returns>A list of servers on success; otherwise, <c>null</c>.</returns>
        public IPEndPoint[] GetServerList( ESteam2ServerType type )
        {
            TcpPacket packet = this.GetRawServerList( ( byte )type );

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


        internal TcpPacket GetRawServerList( ESteam2ServerType type, params object[] args )
        {
            return this.GetRawServerList( ( byte )type, args );
        }

        internal TcpPacket GetRawServerList( byte commandOrType, params object[] args )
        {

            try
            {

                if ( !this.HandshakeServer( ESteam2ServerType.GeneralDirectoryServer ) )
                {
                    DebugLog.WriteLine( "DSClient", "GetServerList failed handshake." );

                    Socket.Disconnect();
                    return null;
                }

                bool bRet = this.SendCommand( commandOrType, args );

                if ( !bRet )
                {
                    DebugLog.WriteLine( "DSClient", "GetServerList failed sending EServerType command." );

                    Socket.Disconnect();
                    return null;
                }

                TcpPacket packet = Socket.ReceivePacket();
                return packet;

            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "DSClient", "GetServerList threw an exception.\n{0}", ex.ToString() );

                Socket.Disconnect();
                return null;
            }

        }
    }
}
