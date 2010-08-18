using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Classless.Hasher;

namespace SteamLib
{
    public enum EServerType : uint
    {
        ProxyASClientAuthentication = 0,
        ContentServer = 1,
        GeneralDirectoryServer = 2,
        ConfigServer = 3,
        CSDS = 6,
        VCDSValidateNewValveCDKey = 7,
        HLMasterServer = 15,
        FriendsServer = 16,
        AllMasterASClientAuthentication = 18,
        CSER = 20,
        HL2MasterServer = 24,
        MasterASClientAuthentication = 26,
        SlaveASClientAuthentication = 28,
    }

    public class Client
    {
        protected TcpSocket socket;

        public Client()
        {
            socket = new TcpSocket();
        }

        public bool ConnectToServer( IPEndPoint endPoint )
        {
            try
            {
                socket.Connect( endPoint );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HandshakeServer( EServerType type )
        {
            try
            {
                socket.Writer.Write( ( uint )type );
                return socket.Reader.ReadByte() == 1;
            }
            catch
            {
                return false;
            }
        }

        public bool SendCommand( byte command, params object[] args )
        {
            try
            {
                TcpPacket packet = new TcpPacket();

                packet.Append( command );

                foreach ( object arg in args )
                {
                    if ( arg is byte[] )
                        packet.Append( ( byte[] )arg );
                    else if ( arg is string )
                        packet.Append( ( string )arg, Encoding.ASCII );
                    else
                        packet.Append( arg.GetType(), arg );
                }

                socket.Send( packet );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public TcpPacket RecvPacket()
        {
            try
            {
                return socket.Receive();
            }
            catch
            {
                return null;
            }
        }

        public void Disconnect()
        {
            try
            {
                socket.Disconnect();
            }
            catch { }
        }
    }

    public class ASClient : Client
    {
    }

    public class ContentServerClient : Client
    {
    }



    public class ConfigServerClient : Client
    {

        public byte[] GetClientConfigRecord( IPEndPoint configServer )
        {
            if ( !this.ConnectToServer( configServer ) )
                return null;

            if ( !this.HandshakeServer( EServerType.ConfigServer ) )
            {
                this.Disconnect();
                return null;
            }

            uint externalIp = socket.Reader.ReadUInt32();

            if ( !this.SendCommand( 1 ) ) // command: Get CCR
            {
                this.Disconnect();
                return null;
            }

            TcpPacket pack = this.RecvPacket();
            this.Disconnect();

            if ( pack == null )
                return null;

            return pack.GetPayload();
        }

        public byte[] GetContentDescriptionRecord( IPEndPoint endPoint, byte[] oldCDRHash )
        {
            if ( !this.ConnectToServer( endPoint ) )
                return null;

            if ( !this.HandshakeServer( EServerType.ConfigServer ) )
            {
                this.Disconnect();
                return null;
            }

            uint externalIp = socket.Reader.ReadUInt32();

            if ( oldCDRHash == null )
                oldCDRHash = new byte[ 20 ];

            if ( !SendCommand( 9, oldCDRHash ) )
            {
                this.Disconnect();
                return null;
            }

            byte[] unknown = socket.Reader.ReadBytes( 11 );

            TcpPacket packet = this.RecvPacket();
            this.Disconnect();

            if ( packet == null )
                return null;

            return packet.GetPayload();
        }
    }

    public class DSClient : Client
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

    public class CSDSClient : DSClient
    {
        public IPEndPoint[] GetContentServerList( IPEndPoint endPoint )
        {
            return this.GetServerList( endPoint, EServerType.ConfigServer, null );
        }
    }

    public class GDSClient : DSClient
    {
        public static string[] GDServers =
        {
            "72.165.61.189:27030", // gds1.steampowered.com
            "72.165.61.190:27030", // gds2.steampowered.com
            "69.28.151.178:27038",
            "69.28.153.82:27038",
            "87.248.196.194:27038",
            "68.142.72.250:27038",
        };



    }
}
