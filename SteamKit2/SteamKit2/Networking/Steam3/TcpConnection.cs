/*
* This file is subject to the terms and conditions defined in
* file 'license.txt', which is part of this source code package.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2
{
    class TcpConnection : Connection
    {
        internal class ConnectionState
        {
            public IPEndPoint Destination { get; private set; }
            public Socket Socket { get; private set; }
            public INetFilterEncryption NetFilter { get; private set; }
            public Thread NetThread { get; private set; }
            public NetworkStream NetStream { get; private set; }
            public BinaryReader NetReader { get; private set; }
            public BinaryWriter NetWriter { get; private set; }

            public bool Released { get; private set; }
            public CancellationTokenSource CancellationToken { get; private set; }
            public ManualResetEvent ConnectionReleased { get; private set; }
            public ManualResetEvent IssuedDisconnectResult { get; private set; }

            // netlock guards concurrent access to socket and network streams
            public object NetLock { get; private set; }
            // releaselock guards the actual disposal of the events and token source until anything consuming them releases it
            public object ReleaseLock { get; private set; }

            public ConnectionState( int timeout )
            {
                Destination = null;

                Socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                Socket.ReceiveTimeout = timeout;
                Socket.SendTimeout = timeout;

                CancellationToken = new CancellationTokenSource();
                ConnectionReleased = new ManualResetEvent( initialState: false );
                IssuedDisconnectResult = new ManualResetEvent( initialState: false );

                NetLock = new object();
                ReleaseLock = new object();
            }

            internal void SetDestination( IPEndPoint destination )
            {
                Destination = destination;
            }

            internal void SetNetFilter( INetFilterEncryption filter )
            {
                NetFilter = filter;
            }

            public void StartWorker( ParameterizedThreadStart workFunction )
            {
                NetStream = new NetworkStream( Socket, false );
                NetReader = new BinaryReader( NetStream );
                NetWriter = new BinaryWriter( NetStream );
                NetFilter = null;

                NetThread = new Thread( workFunction );
                NetThread.Name = "TcpConnection Network Thread";

                NetThread.Start( this );
            }

            public void Disconnect()
            {
                try
                {
                    if ( Socket.Connected )
                    {
                        Socket.Shutdown( SocketShutdown.Both );
#if NET46
                        Socket.Disconnect( true );
#endif
                    }
                }
                catch
                {
                    // Shutdown is throwing when the remote end closes the connection before SteamKit attempts to
                    // so this should be safe as a no-op
                    // see: https://bitbucket.org/VoiDeD/steamre/issue/41/socketexception-thrown-when-closing
                }
            }

            public void CleanupSocket()
            {
                lock ( NetLock )
                {
                    if ( NetWriter != null )
                    {
                        NetWriter.Dispose();
                        NetWriter = null;
                    }

                    if ( NetReader != null )
                    {
                        NetReader.Dispose();
                        NetReader = null;
                    }

                    if ( NetStream != null )
                    {
                        NetStream.Dispose();
                        NetStream = null;
                    }

                    if ( Socket != null )
                    {
                        Socket.Dispose();
                        Socket = null;
                    }
                }

                NetFilter = null;

                ConnectionReleased.Set();
            }

            public void PostRelease()
            {
                IssuedDisconnectResult.Set();

                lock ( ReleaseLock )
                {
                    CancellationToken.Dispose();

                    ConnectionReleased.Dispose();
                    IssuedDisconnectResult.Dispose();

                    Released = true;
                }
            }
        }

        const uint MAGIC = 0x31305456; // "VT01"

        private ConnectionState activeConnectionState;
        private object connectLock;

        public TcpConnection()
        {
            activeConnectionState = null;
            connectLock = new object();
        }

        public override IPEndPoint CurrentEndPoint
        {
            get
            {
                var connectionState = activeConnectionState;
                return connectionState == null ? null : connectionState.Destination;
            }
        }

        private void Release( ConnectionState connectionState, bool userRequestedDisconnect )
        {
            connectionState.CleanupSocket();

            OnDisconnected( new DisconnectedEventArgs( userRequestedDisconnect ) );

            connectionState.PostRelease();
        }

        private void ConnectCompleted( ConnectionState connectionState, bool success )
        {
            // Always discard result if our request was cancelled
            if ( connectionState.CancellationToken.IsCancellationRequested )
            {
                DebugLog.WriteLine( "TcpConnection", "Connection request to {0} was cancelled", connectionState.Destination );
                if ( success ) connectionState.Disconnect();
                Release( connectionState, userRequestedDisconnect: true );
                return;
            }
            else if ( !success )
            {
                DebugLog.WriteLine( "TcpConnection", "Timed out while connecting to {0}", connectionState.Destination );
                Release( connectionState, userRequestedDisconnect: false );
                return;
            }

            DebugLog.WriteLine( "TcpConnection", "Connected to {0}", connectionState.Destination );

            try
            {
                connectionState.StartWorker( NetLoop );

                OnConnected( EventArgs.Empty );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "TcpConnection", "Exception while setting up connection to {0}: {1}", connectionState.Destination, ex );
                Release( connectionState, userRequestedDisconnect: false );
            }
        }

        private void TryConnect( ConnectionState connectionState, int timeout )
        {
            if ( connectionState.CancellationToken.IsCancellationRequested )
            {
                DebugLog.WriteLine( "TcpConnection", "Connection to {0} cancelled by user", connectionState.Destination );
                Release( connectionState, userRequestedDisconnect: true );
                return;
            }

            var connectEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = connectionState.Destination, UserToken = connectionState };
            var asyncWaitHandle = new ManualResetEvent( initialState: false );
            EventHandler<SocketAsyncEventArgs> completionHandler = ( s, e ) =>
            {
                asyncWaitHandle.Set();

                var connected = e.ConnectSocket != null;
                ConnectCompleted( e.UserToken as ConnectionState, connected );
                ( e as IDisposable )?.Dispose();
            };
            connectEventArgs.Completed += completionHandler;

            if ( !connectionState.Socket.ConnectAsync( connectEventArgs ) )
            {
                completionHandler( connectionState.Socket, connectEventArgs );
            }

            if ( WaitHandle.WaitAny( new WaitHandle[] { asyncWaitHandle, connectionState.CancellationToken.Token.WaitHandle }, timeout ) != 0 )
            {
                Socket.CancelConnectAsync( connectEventArgs );
            }
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPointTask">Task returning the end point.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public override void Connect( Task<IPEndPoint> endPointTask, int timeout )
        {
            // First, capture the lock around this flow to ensure that no two competing Connect calls can setup connections only to get immediately torn down
            lock ( connectLock )
            {
                Disconnect();

                Debug.Assert( activeConnectionState == null );

                // now that we own the connection lock and we've waited for the connection state to be released, we can setup another connection
                activeConnectionState = new ConnectionState( timeout );

                // now we wait for the result from the endpoint task. We will be leaving our connectLock guard if it's not already resolved
                endPointTask.ContinueWith( ( t, sender ) =>
                 {
                     ConnectionState connectionState = ( ConnectionState )sender;

                     if ( t.IsFaulted || t.IsCanceled )
                     {
                         if ( t.Exception != null )
                         {
                             foreach ( var ex in t.Exception.Flatten().InnerExceptions )
                             {
                                 DebugLog.WriteLine( "TcpConnection", "Endpoint task threw exception: {0}", ex );
                             }
                         }

                         Release( connectionState, userRequestedDisconnect: false );
                         return;
                     }

                     var destination = t.Result;
                     connectionState.SetDestination( destination );

                     if ( destination != null )
                     {
                         DebugLog.WriteLine( "TcpConnection", "Connecting to {0}...", destination );
                         TryConnect( connectionState, timeout );
                     }
                     else
                     {
                         DebugLog.WriteLine( "TcpConnection", "No destination supplied from endpoint task" );
                         Release( connectionState, userRequestedDisconnect: false );
                         return;
                     }
                 }, activeConnectionState, TaskContinuationOptions.LongRunning );
            }
        }

        public override void Disconnect()
        {
            ConnectionState currentConnectionState = Interlocked.Exchange( ref activeConnectionState, null );

            if ( currentConnectionState != null )
            {
                lock ( currentConnectionState.ReleaseLock )
                {
                    if ( currentConnectionState.Released )
                    {
                        // nothing to do, it was already released
                        return;
                    }

                    // signal the current state to cancel regardless of where it is in the process. Wait for the post-release disconnect callback result to ensure it is queued
                    currentConnectionState.CancellationToken.Cancel();
                    currentConnectionState.IssuedDisconnectResult.WaitOne();
                }
            }
        }

        // this is now a steamkit meme
        /// <summary>
        /// Nets the loop.
        /// </summary>
        void NetLoop( object sender )
        {
            ConnectionState connectionState = ( ConnectionState )sender;

            // poll for readable data every 100ms
            const int POLL_MS = 100;

            while ( !connectionState.CancellationToken.IsCancellationRequested )
            {
                bool canRead = false;

                try
                {
                    canRead = connectionState.Socket.Poll( POLL_MS * 1000, SelectMode.SelectRead );
                }
                catch ( SocketException ex )
                {
                    DebugLog.WriteLine( "TcpConnection", "Socket exception while polling: {0}", ex );
                    break;
                }

                if ( !canRead )
                {
                    // nothing to read yet
                    continue;
                }

                byte[] packData = null;

                try
                {
                    // read the packet off the network
                    packData = ReadPacket( connectionState );

                    // decrypt the data off the wire if needed
                    if ( connectionState.NetFilter != null )
                    {
                        packData = connectionState.NetFilter.ProcessIncoming( packData );
                    }
                }
                catch ( IOException ex )
                {
                    DebugLog.WriteLine( "TcpConnection", "Socket exception occurred while reading packet: {0}", ex );
                    break;
                }

                try
                {
                    OnNetMsgReceived( new NetMsgEventArgs( packData, connectionState.Destination ) );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "TcpConnection", "Unexpected exception propogated back to NetLoop: {0}", ex );
                }
            }

            // Thread is shutting down, ensure socket is shut down and disposed
            bool userShutdown = connectionState.CancellationToken.IsCancellationRequested;

            if ( userShutdown )
            {
                connectionState.Disconnect();
            }

            Release( connectionState, userShutdown );
        }

        byte[] ReadPacket( ConnectionState connectionState )
        {
            var socket = connectionState.Socket;
            var netReader = connectionState.NetReader;

            // the tcp packet header is considerably less complex than the udp one
            // it only consists of the packet length, followed by the "VT01" magic
            uint packetLen = 0;
            uint packetMagic = 0;

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
            byte[] packData = netReader.ReadBytes( ( int )packetLen );

            if ( packData.Length != packetLen )
            {
                throw new IOException( "Connection lost while reading packet payload" );
            }

            return packData;
        }

        public override void Send( IClientMsg clientMsg )
        {
            var connectionState = activeConnectionState;

            if ( connectionState == null )
            {
                DebugLog.WriteLine( "TcpConnection", "Attempting to send client message when not connected: {0}", clientMsg.MsgType );
                return;
            }

            lock ( connectionState.NetLock )
            {
                if ( connectionState.Socket == null || connectionState.NetStream == null )
                {
                    DebugLog.WriteLine( "TcpConnection", "Attempting to send client message when not connected: {0}", clientMsg.MsgType );
                    return;
                }

                var data = clientMsg.Serialize();

                if ( connectionState.NetFilter != null )
                {
                    data = connectionState.NetFilter.ProcessOutgoing( data );
                }

                try
                {
                    connectionState.NetWriter.Write( ( uint )data.Length );
                    connectionState.NetWriter.Write( TcpConnection.MAGIC );
                    connectionState.NetWriter.Write( data );
                }
                catch ( IOException ex )
                {
                    DebugLog.WriteLine( "TcpConnection", "Socket exception while writing data: {0}", ex );
                }
            }
        }

        public override IPAddress GetLocalIP()
        {
            var connectionState = activeConnectionState;

            if ( connectionState == null ) return IPAddress.None;

            lock ( connectionState.NetLock )
            {
                if ( connectionState.Socket != null )
                {
                    try
                    {
                        return NetHelpers.GetLocalIP( connectionState.Socket );
                    }
                    catch ( Exception ex )
                    {
                        DebugLog.WriteLine( "TcpConnection", "Socket exception trying to read bound IP: {0}", ex );
                        return IPAddress.None;
                    }
                }
            }

            return IPAddress.None;
        }

        public override void SetNetEncryptionFilter( INetFilterEncryption filter )
        {
            var connectionState = activeConnectionState;

            if ( connectionState == null ) return;
            connectionState.SetNetFilter( filter );
        }
    }
}
