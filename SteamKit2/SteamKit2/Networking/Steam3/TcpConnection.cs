/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SteamKit2
{

    class TcpConnection : Connection
    {
        const uint MAGIC = 0x31305456; // "VT01"

        bool isConnected;

        Socket sock;

        NetworkStream netStream;

        BinaryReader netReader;
        BinaryWriter netWriter;

        Thread netThread;


        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public override void Connect( IPEndPoint endPoint, int timeout )
        {
            // if we're connected, disconnect
            Disconnect();

            Socket socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            DebugLog.WriteLine( "TcpConnection", "Connecting to {0}...", endPoint );

            ThreadPool.QueueUserWorkItem( sender =>
            {
                var asyncResult = socket.BeginConnect( endPoint, null, null );

                if ( asyncResult.AsyncWaitHandle.WaitOne( timeout ) )
                {
                    sock = socket;
                    ConnectCompleted( socket );
                }
                else
                {
                    socket.Close();
                    ConnectCompleted( null );
                }
            });
        }

        void ConnectCompleted( Socket sock )
        {
            if ( sock == null )
            {
                OnDisconnected( EventArgs.Empty );
                return;
            }

            if ( !sock.Connected )
            {
                DebugLog.WriteLine( "TcpConnection", "Unable to connect" );
                OnDisconnected( EventArgs.Empty );
                return;
            }

            DebugLog.WriteLine( "TcpConnection", "Connected!" );

            isConnected = true;

            netStream = new NetworkStream( sock, false );

            netReader = new BinaryReader( netStream );
            netWriter = new BinaryWriter( netStream );

            // initialize our network thread
            netThread = new Thread( NetLoop );
            netThread.Name = "TcpConnection Thread";
            netThread.Start();

            OnConnected( EventArgs.Empty );
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public override void Disconnect()
        {
            if ( !isConnected )
                return;

            isConnected = false;

            // wait for our network thread to terminate
            netThread.Join();
            netThread = null;

            Cleanup();

            OnDisconnected( EventArgs.Empty );
        }

        /// <summary>
        /// Sends the specified client net message.
        /// </summary>
        /// <param name="clientMsg">The client net message.</param>
        public override void Send( IClientMsg clientMsg )
        {
            if ( !isConnected )
            {
                DebugLog.WriteLine( "TcpConnection", "Attempting to send client message when not connected: {0}", clientMsg.MsgType );
                return;
            }

            byte[] data = clientMsg.Serialize();

            // encrypt outgoing traffic if we need to
            if ( NetFilter != null )
            {
                data = NetFilter.ProcessOutgoing( data );
            }

            lock ( sock )
            {
                // write header
                netWriter.Write( ( uint )data.Length );
                netWriter.Write( TcpConnection.MAGIC );

                netWriter.Write( data );
            }

        }

        // this is now a steamkit meme
        /// <summary>
        /// Nets the loop.
        /// </summary>
        void NetLoop()
        {
            // poll for readable data every 100ms
            const int POLL_MS = 100; 

            while ( true )
            {
                if ( !isConnected )
                {
                    break;
                }

                bool canRead = sock.Poll( POLL_MS * 1000, SelectMode.SelectRead );

                if ( !canRead )
                {
                    // nothing to read yet
                    continue;
                }

                // read the packet off the network
                ReadPacket();
            }
        }


        void ReadPacket()
        {
            // the tcp packet header is considerably less complex than the udp one
            // it only consists of the packet length, followed by the "VT01" magic
            uint packetLen = 0;
            uint packetMagic = 0;

            byte[] packData = null;

            try
            {
                try
                {
                    packetLen = netReader.ReadUInt32();
                    packetMagic = netReader.ReadUInt32();
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
                packData = netReader.ReadBytes( ( int )packetLen );

                if ( packData.Length != packetLen )
                {
                    throw new IOException( "Connection lost while reading packet payload" );
                }

                // decrypt the data off the wire if needed
                if ( NetFilter != null )
                {
                    packData = NetFilter.ProcessIncoming( packData );
                }
            }
            catch ( IOException ex )
            {
                DebugLog.WriteLine( "TcpConnection", "Socket exception occurred while reading packet: {0}", ex );

                // signal that our connection is dead
                isConnected = false;

                Cleanup();

                OnDisconnected( EventArgs.Empty );
                return;
            }

            OnNetMsgReceived( new NetMsgEventArgs( packData, sock.RemoteEndPoint as IPEndPoint ) );
        }

        void Cleanup()
        {
            // cleanup streams
            if ( netReader != null )
            {
                netReader.Dispose();
                netReader = null;
            }

            if ( netWriter != null )
            {
                netWriter.Dispose();
                netWriter = null;
            }

            if ( netStream != null )
            {
                netStream.Dispose();
                netStream = null;
            }

            if ( sock != null )
            {
                try
                {
                    // cleanup socket
                    if ( sock.Connected )
                    {
                        sock.Shutdown( SocketShutdown.Both );
                        sock.Disconnect( true );
                    }
                    sock.Close();
                }
                catch
                {
                    // Shutdown is throwing when the remote end closes the connection before SteamKit attempts to
                    // so this should be safe as a no-op
                    // see: https://bitbucket.org/VoiDeD/steamre/issue/41/socketexception-thrown-when-closing
                }

                sock = null;
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