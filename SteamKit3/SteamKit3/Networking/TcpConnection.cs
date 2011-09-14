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

namespace SteamKit3
{

    class TcpConnection : Connection
    {
        const uint MAGIC = 0x31305456; // "VT01"

        Socket sock;

        Thread netThread;

        NetworkStream stream;
        BinaryReader reader;

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public override void Connect( IPEndPoint endPoint )
        {
            Disconnect();

            sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

            var socketConnect = new SocketAsyncEventArgs();

            socketConnect.RemoteEndPoint = endPoint;
            socketConnect.Completed += ( sender, e ) =>
                {
                    if ( e.SocketError == SocketError.Success )
                    {
                        stream = new NetworkStream( sock, true );
                        reader = new BinaryReader( stream );

                        netThread = new Thread( NetLoop );
                        netThread.Name = "TcpConnection Thread";
                        netThread.Start();

                        OnConnected( EventArgs.Empty );

                        return;
                    }

                    OnDisconnected( EventArgs.Empty );
                };

            sock.ConnectAsync( socketConnect );
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

            byte[] data = clientMsg.Serialize();

            if ( NetFilter != null )
            {
                data = NetFilter.ProcessOutgoing( data );
            }

            using ( MemoryStream ms = new MemoryStream() )
            using ( BinaryWriter bw = new BinaryWriter( ms ) )
            {
                bw.Write( data.Length );
                bw.Write( TcpConnection.MAGIC );
                bw.Write( data );

                data = ms.ToArray();
            }

            var sockSend = new SocketAsyncEventArgs();

            sockSend.SetBuffer( data, 0, data.Length );
            sockSend.Completed += ( sender, e ) =>
            {
                if ( e.SocketError == SocketError.Success )
                    return;

                // report send failure?
            };

            sock.SendAsync( sockSend );
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
                    byte[] packetHeader = reader.ReadBytes( 8 );

                    if ( packetHeader.Length != 8 )
                        throw new IOException( "Connection lost while reading packet header" );

                    using ( MemoryStream ms = new MemoryStream( packetHeader ) )
                    using ( BinaryReader br = new BinaryReader( ms ) )
                    {
                        uint packetLen = br.ReadUInt32();
                        uint packetMagic = br.ReadUInt32();

                        if ( packetMagic != TcpConnection.MAGIC )
                            throw new IOException( "RecvCompleted got a packet with invalid magic!" );

                        // rest of the packet is the physical data
                        byte[] packData = reader.ReadBytes( ( int )packetLen );
                        if ( packData.Length != packetLen )
                            throw new IOException( "Connection lost while reading packet payload" );

                        if ( NetFilter != null )
                            packData = NetFilter.ProcessIncoming( packData );

                        OnNetMsgReceived( new NetMsgEventArgs( packData, sock.RemoteEndPoint as IPEndPoint ) );
                    }
                }
            }
            catch ( IOException e )
            {
                Log.Error( "Socket exception", e );

                OnDisconnected( EventArgs.Empty );
            }
        }
    }
}