/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SteamKit2
{
    /// <summary>
    /// Represents a Tcp socket.
    /// </summary>
    sealed class TcpSocket
    {
        Socket sock;
        NetworkStream sockStream;

        /// <summary>
        /// Gets the network binary reader.
        /// </summary>
        /// <value>The binary reader.</value>
        public BinaryReader Reader { get; private set; }
        /// <summary>
        /// Gets the network binary writer.
        /// </summary>
        /// <value>The binary writer.</value>
        public BinaryWriter Writer { get; private set; }

        /// <summary>
        /// Gets whether or not the client is connected.
        /// </summary>
        /// <value>True if connected, otherwise false.</value>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the length of time a connection will attempt to establish before timing out. The default timeout is 30 seconds.
        /// </summary>
        /// <value>The connection timeout.</value>
        public TimeSpan ConnectionTimeout { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket()
        {
            ConnectionTimeout = TimeSpan.FromSeconds( 30 );
        }


        /// <summary>
        /// Disconnects (if needed) and connects the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public void Connect( IPEndPoint endPoint )
        {
            Disconnect();

            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            var asyncResult = sock.BeginConnect( endPoint, null, null );

            IsConnected = asyncResult.AsyncWaitHandle.WaitOne( ConnectionTimeout );

            if ( !IsConnected )
            {
                sock.Close();
                return;
            }

            sock.EndConnect( asyncResult );

            sockStream = new NetworkStream( sock, true );

            Reader = new BinaryReader( sockStream );
            Writer = new BinaryWriter( sockStream );
        }

        /// <summary>
        /// Disconnects this socket.
        /// </summary>
        public void Disconnect()
        {
            // mono doesn't like calling Shutdown if we're not connected
            if ( sock == null || !sock.Connected )
                return;

            if ( !IsConnected )
                return;

            if ( sock != null )
            {
                try
                {
                    sock.Shutdown( SocketShutdown.Both );
                    sock.Disconnect( true );
                    sock.Close();

                    sock = null;
                }
                catch { }
            }

            IsConnected = false;
        }

        /// <summary>
        /// Sends the specified data on the socket.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send( byte[] data )
        {
            sock.Send( data );
        }

        /// <summary>
        /// Sends the specified packet on the socket.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void Send( TcpPacket packet )
        {
            this.Send( packet.GetData() );
        }

        /// <summary>
        /// Attempts to receive a tcp packet from the socket.
        /// </summary>
        /// <returns>The packet.</returns>
        public TcpPacket ReceivePacket()
        {
            TcpPacket pack = new TcpPacket();

            uint size = NetHelpers.EndianSwap( this.Reader.ReadUInt32() );
            byte[] payload = Reader.ReadBytes( ( int )size );

            pack.SetPayload( payload );

            return pack;
        }

        /// <summary>
        /// Gets the local IP.
        /// </summary>
        /// <returns>The local IP.</returns>
        public IPAddress GetLocalIP()
        {
            return NetHelpers.GetLocalIP(sock);
        }
    }
}
