using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Classless.Hasher;

namespace SteamLib
{

    public class DSClient : ServerClient
    {
        public IPEndPoint[] GetServerList( IPEndPoint directoryServer, EServerType type, string userName )
        {
            List<IPEndPoint> serverList = new List<IPEndPoint>();

            if ( !this.ConnectToServer( directoryServer ) )
                return null;

            if ( !this.HandshakeServer( EServerType.GeneralDirectoryServer ) )
            {
                this.Disconnect();
                return null;
            }

            bool bRet = false;

            if ( userName != null )
            {
                JenkinsHash jh = new JenkinsHash();
                byte[] userHash = jh.ComputeHash( Encoding.ASCII.GetBytes( userName.ToLower() ) );
                Array.Reverse( userHash );
                jh.Clear();

                bRet = this.SendCommand( ( byte )type, userHash );
            }
            else
            {
                bRet = this.SendCommand( ( byte )type );
            }

            if ( !bRet )
            {
                this.Disconnect();
                return null;
            }

            TcpPacket packet = this.RecvPacket();

            if ( packet == null )
                return null;


            DataStream ds = new DataStream( packet.GetPayload(), true );

            ushort numAddrs = ds.ReadUInt16();

            for ( int x = 0 ; x < numAddrs ; ++x )
            {
                IPAddress ipAddr = new IPAddress( ds.ReadBytes( 4 ) );
                int port = IPAddress.NetworkToHostOrder( ds.ReadInt16() );

                serverList.Add( new IPEndPoint( ipAddr, port ) );
            }

            return serverList.ToArray();
        }
    }
}
