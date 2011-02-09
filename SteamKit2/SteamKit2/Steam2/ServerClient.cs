/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Classless.Hasher;
using System.Diagnostics;

namespace SteamKit2
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
        protected TcpSocket Socket { get; private set; }
        protected IPEndPoint EndPoint { get; private set; }

        public ServerClient()
        {
            Socket = new TcpSocket();
        }

        public void Connect( IPEndPoint endPoint )
        {
            EndPoint = endPoint;

            try
            {
                Socket.Connect( endPoint );
            }
            catch
            {
                throw;
            }

        }
        public void Disconnect()
        {
            Socket.Disconnect();
        }

        protected bool SendCommand( byte command, params object[] args )
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

                Socket.Send( packet );

                return true;
            }
            catch
            {
                return false;
            }

        }
        protected bool HandshakeServer( EServerType type )
        {
            try
            {
                Socket.Writer.Write( NetHelpers.EndianSwap( ( uint )type ) );
                return Socket.Reader.ReadByte() == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
