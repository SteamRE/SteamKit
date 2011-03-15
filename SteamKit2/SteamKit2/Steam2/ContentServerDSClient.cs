using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit2
{
    public class ContentServerDSClient : DSClient
    {
        public IPEndPoint[] GetContentServerList( uint depotId, uint depotVersion, uint cellId )
        {
            TcpPacket packet = base.GetRawServerList(
                ( EServerType )0, // command 0
                ( ushort )1, // unknown
                depotId,
                depotVersion,
                ( ushort )10, // num servers
                cellId,
                unchecked( ( ulong )-1 )
            );

            DataStream ds = new DataStream( packet.GetPayload(), true );

            ushort numAddrs = ds.ReadUInt16();

            IPEndPoint[] serverList = new IPEndPoint[ numAddrs ];
            for ( int x = 0 ; x < numAddrs ; ++x )
            {

                uint contentServerId = ds.ReadUInt32();

                IPAddrPort ipAddr = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );
                IPAddrPort ipAddr2 = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );

                serverList[ x ] = ipAddr2;

            }

            return serverList;
        }
    }
}
