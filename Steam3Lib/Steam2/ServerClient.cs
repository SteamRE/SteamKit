using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

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

    public class ServerClient
    {

        public TcpSocket socket;


        public ServerClient()
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
}
