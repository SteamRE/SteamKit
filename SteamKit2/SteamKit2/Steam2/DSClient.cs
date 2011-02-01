using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace SteamKit2
{
    public class DSClient : ServerClient
    {

        public IPEndPoint[] GetServerList( EServerType type, string userName )
        {
            List<IPEndPoint> serverList = new List<IPEndPoint>();

            try
            {

                if ( !this.HandshakeServer( EServerType.GeneralDirectoryServer ) )
                {
#if DEBUG
                    Trace.WriteLine( "DSClient GetServerList failed handshake.", "Steam2" );
#endif

                    Socket.Disconnect();
                    return null;
                }

                bool bRet = false;

                if ( userName != null )
                {
                    byte[] userHash = CryptoHelper.JenkinsHash( Encoding.ASCII.GetBytes( userName ) );

                    bRet = this.SendCommand( ( byte )type, userHash );
                }
                else
                {
                    bRet = this.SendCommand( ( byte )type );
                }

                if ( !bRet )
                {
#if DEBUG
                    Trace.WriteLine( "DSClient GetServerList failed sending EServerType command.", "Steam2" );
#endif

                    Socket.Disconnect();
                    return null;
                }

                TcpPacket packet = Socket.ReceivePacket();
                DataStream ds = new DataStream( packet.GetPayload(), true );

                ushort numAddrs = ds.ReadUInt16();

                for ( int x = 0 ; x < numAddrs ; ++x )
                {
                    IPAddrPort ipAddr = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );

                    serverList.Add( ipAddr );
                }
            }
            catch ( Exception ex )
            {
#if DEBUG
                Trace.WriteLine( string.Format( "DSClient GetServerList threw an exception.\n{0}", ex.ToString() ), "Steam2" );
#endif

                Socket.Disconnect();
                return null;
            }

            return serverList.ToArray();
        }
    }
}
