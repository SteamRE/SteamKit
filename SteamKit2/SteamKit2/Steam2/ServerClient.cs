/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// This is a list of known Steam2 server types.
    /// </summary>
    public enum ESteam2ServerType : uint
    {
        /// <summary>
        /// This is the auth server that all clients should connect to.
        /// Implemented as a reverse proxy for load balancing.
        /// </summary>
        ProxyASClientAuthentication = 0,
        /// <summary>
        /// Represents a server that serves game content to clients.
        /// </summary>
        ContentServer = 1,
        /// <summary>
        /// The general directory server which returns a list of other servers.
        /// </summary>
        GeneralDirectoryServer = 2,
        /// <summary>
        /// Represents a server that serves config data (such as the CDR) to clients.
        /// </summary>
        ConfigServer = 3,
        /// <summary>
        /// Content Server?/Config Server? directory server
        /// </summary>
        CSDS = 6,
        /// <summary>
        /// Unknown.
        /// </summary>
        VCDSValidateNewValveCDKey = 7,
        /// <summary>
        /// Half-Life master server.
        /// </summary>
        HLMasterServer = 15,
        /// <summary>
        /// Friends server. Most likely obsolete.
        /// </summary>
        FriendsServer = 16,
        /// <summary>
        /// Unknown.
        /// </summary>
        AllMasterASClientAuthentication = 18,
        /// <summary>
        /// Reporting server for source games.
        /// </summary>
        CSER = 20,
        /// <summary>
        /// Half-Life 2 master server.
        /// </summary>
        HL2MasterServer = 24,
        /// <summary>
        /// Unknown.
        /// </summary>
        MasterASClientAuthentication = 26,
        /// <summary>
        /// Unknown.
        /// </summary>
        SlaveASClientAuthentication = 28,
        /// <summary>
        /// Rag doll kung fu master server.
        /// </summary>
        RDKFMasterServer = 30
    }

    /// <summary>
    /// This is the root client class used to provide all the functionality required to connect to Steam2 servers.
    /// </summary>
    public class ServerClient
    {
        /// <summary>
        /// Gets the socket of the client.
        /// </summary>
        /// <value>The socket.</value>
        internal TcpSocket Socket { get; private set; }
        /// <summary>
        /// Gets the remote endpoint of the client.
        /// </summary>
        /// <value>The end point.</value>
        protected IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerClient"/> class.
        /// </summary>
        public ServerClient()
        {
            Socket = new TcpSocket();
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public void Connect( IPEndPoint endPoint )
        {
            try
            {
                Socket.Connect( endPoint );
                EndPoint = endPoint;
            }
            catch
            {
                throw;
            }

        }
        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            Socket.Disconnect();
        }


        /// <summary>
        /// Sends a command to the connected server.
        /// The return value of this function does not signify command success, only if the command was sent.
        /// </summary>
        /// <param name="command">The command type to send.</param>
        /// <param name="args">The arguments to send.</param>
        /// <returns>True if the command was sent; otherwise, false.</returns>
        protected bool SendCommand( byte command, params object[] args )
        {
            try
            {
                using ( TcpPacket packet = new TcpPacket() )
                {
                    packet.Write( command );

                    foreach ( object arg in args )
                    {
                        if ( arg is byte[] )
                            packet.Write( ( byte[] )arg );
                        else if ( arg is string )
                            packet.Write( ( string )arg, Encoding.ASCII );
                        else
                            packet.Write( arg.GetType(), arg );
                    }

                    Socket.Send( packet );
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Performs a handshake with the server.
        /// </summary>
        /// <param name="type">The expected server type the client is handshaking with.</param>
        /// <returns>True if the handshake succeeded; otherwise false.</returns>
        protected bool HandshakeServer( ESteam2ServerType type )
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
