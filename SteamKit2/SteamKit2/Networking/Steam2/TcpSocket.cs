/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Represents a Tcp socket.
    /// </summary>
    sealed class TcpSocket
    {
        Socket sock;
        bool bConnected;

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
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket()
        {
        }


        /// <summary>
        /// Disconnects (if needed) and connects the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public void Connect( IPEndPoint endPoint )
        {
            Disconnect();

            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            sock.Connect( endPoint );

            bConnected = true;

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

            if ( !bConnected )
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

            bConnected = false;
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
