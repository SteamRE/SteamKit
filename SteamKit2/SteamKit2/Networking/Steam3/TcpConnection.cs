/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SteamKit2
{

    class TcpConnection : Connection
    {
        const uint MAGIC = 0x31305456; // "VT01"

        Socket sock;

        Thread netThread;

        NetworkStream stream;
        BinaryReader reader;
        BinaryWriter writer;

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public override void Connect( IPEndPoint endPoint )
        {
            Disconnect();

            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            sock.Connect( endPoint );

            stream = new NetworkStream( sock, true );
            reader = new BinaryReader( stream );
            writer = new BinaryWriter( stream );

            netThread = new Thread( NetLoop );
            netThread.Name = "TcpConnection Thread";
            netThread.Start();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public override void Disconnect()
        {
            if ( sock == null || !sock.Connected )
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
        }

        /// <summary>
        /// Sends the specified client net message.
        /// </summary>
        /// <param name="clientMsg">The client net message.</param>
        public override void Send( IClientMsg clientMsg )
        {
            if ( sock == null || !sock.Connected )
                return;

            //TODO: Change this
            byte[] data = clientMsg.Serialize();

            if ( NetFilter != null )
            {
                data = NetFilter.ProcessOutgoing( data );
            }

            writer.Write( ( uint )data.Length );
            writer.Write( TcpConnection.MAGIC );
            writer.Write( data );
        }

        // this is now a steamkit meme
        /// <summary>
        /// Nets the loop.
        /// </summary>
        void NetLoop()
        {
            try
            {
                while ( sock.Connected )
                {                    
                    // the tcp packet header is considerably less complex than the udp one
                    // it only consists of the packet length, followed by the "VT01" magic
                    uint packetLen = 0;
                    uint packetMagic = 0;

                    try
                    {
                        packetLen = reader.ReadUInt32();
                        packetMagic = reader.ReadUInt32();
                    }
                    catch ( IOException ex )
                    {
                        throw new IOException( "Connection lost while reading packet header.", ex );
                    }

                    if ( packetMagic != TcpConnection.MAGIC )
                    {
                        throw new IOException( "Got a packet with invalid magic!" );
                    }

                    // rest of the packet is the physical data
                    byte[] packData = reader.ReadBytes( ( int )packetLen );

                    if ( packData.Length != packetLen )
                    {
                        throw new IOException( "Connection lost while reading packet payload" );
                    }

                    if ( NetFilter != null )
                    {
                        packData = NetFilter.ProcessIncoming( packData );
                    }

                    OnNetMsgReceived( new NetMsgEventArgs( packData, sock.RemoteEndPoint as IPEndPoint ) );
                }
            }
            catch ( IOException e )
            {
                DebugLog.WriteLine( "TcpConnection SocketException", e.ToString() );
                OnDisconnected( EventArgs.Empty );
            }
        }

        /// <summary>
        /// Gets the local IP.
        /// </summary>
        /// <returns>The local IP.</returns>
        public override IPAddress GetLocalIP()
        {
            return NetHelpers.GetLocalIP( sock );
        }
    }
}