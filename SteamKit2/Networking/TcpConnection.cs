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

namespace SteamKit2
{
    class TcpConnection : IConnection
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

<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
<<<<<<< HEAD
        private ILogContext log;
        private Socket? socket;
        private Thread? netThread;
        private NetworkStream? netStream;
        private BinaryReader? netReader;
        private BinaryWriter? netWriter;

        private CancellationTokenSource? cancellationToken;
        private object netLock;
=======
        private Socket socket;
        private Thread netThread;
        private NetworkStream netStream;
        private BinaryReader netReader;
        private BinaryWriter netWriter;

        private CancellationTokenSource cancellationToken;
        private readonly object netLock;
>>>>>>> upstream/some-sonarqujbe

        public TcpConnection(ILogContext log)
        {
            this.log = log ?? throw new ArgumentNullException( nameof( log ) );
            netLock = new object();
        }

        public event EventHandler<NetMsgEventArgs>? NetMsgReceived;

        public event EventHandler? Connected;

        public event EventHandler<DisconnectedEventArgs>? Disconnected;

        public EndPoint? CurrentEndPoint { get; private set; }

        public ProtocolTypes ProtocolTypes => ProtocolTypes.Tcp;
=======
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
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs

            internal void SetNetFilter( INetFilterEncryption filter )
            {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect( true );
                }
=======
                NetFilter = filter;
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
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
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
=======
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
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            }
        }

        private void Release( ConnectionState connectionState, bool userRequestedDisconnect )
        {
            connectionState.CleanupSocket();

<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
            Disconnected?.Invoke( this, new DisconnectedEventArgs( userRequestedDisconnect ) );
=======
            OnDisconnected( new DisconnectedEventArgs( userRequestedDisconnect ) );

            connectionState.PostRelease();
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
        }

        private void ConnectCompleted( ConnectionState connectionState, bool success )
        {
            // Always discard result if our request was cancelled
            if ( connectionState.CancellationToken.IsCancellationRequested )
            {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                log.LogDebug("TcpConnection", "Connection request to {0} was cancelled", CurrentEndPoint);
                if (success) Shutdown();
                Release( userRequestedDisconnect: true );
=======
                DebugLog.WriteLine( "TcpConnection", "Connection request to {0} was cancelled", connectionState.Destination );
                if ( success ) connectionState.Disconnect();
                Release( connectionState, userRequestedDisconnect: true );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                return;
            }
            else if ( !success )
            {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                log.LogDebug( "TcpConnection", "Timed out while connecting to {0}", CurrentEndPoint);
                Release( userRequestedDisconnect: false );
                return;
            }

            log.LogDebug( "TcpConnection", "Connected to {0}", CurrentEndPoint);

            try
            {
                lock (netLock)
                {
                    netStream = new NetworkStream(socket, false);
                    netReader = new BinaryReader(netStream);
                    netWriter = new BinaryWriter(netStream);

                    netThread = new Thread(NetLoop);
                    netThread.Name = "TcpConnection Thread";

                    CurrentEndPoint = socket!.RemoteEndPoint;
                }

                netThread.Start();

                Connected?.Invoke( this, EventArgs.Empty );
=======
                DebugLog.WriteLine( "TcpConnection", "Timed out while connecting to {0}", connectionState.Destination );
                Release( connectionState, userRequestedDisconnect: false );
                return;
            }

            DebugLog.WriteLine( "TcpConnection", "Connected to {0}", connectionState.Destination );

            try
            {
                connectionState.StartWorker( NetLoop );

                OnConnected( EventArgs.Empty );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            }
            catch ( Exception ex )
            {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                log.LogDebug( "TcpConnection", "Exception while setting up connection to {0}: {1}", CurrentEndPoint, ex);
                Release( userRequestedDisconnect: false );
=======
                DebugLog.WriteLine( "TcpConnection", "Exception while setting up connection to {0}: {1}", connectionState.Destination, ex );
                Release( connectionState, userRequestedDisconnect: false );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            }
        }

        private void TryConnect( ConnectionState connectionState, int timeout )
        {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
            DebugLog.Assert( cancellationToken != null, nameof( TcpConnection ), "null CancellationToken in TryConnect" );

            int timeout = (int)sender;
            if (cancellationToken.IsCancellationRequested)
            {
                log.LogDebug( "TcpConnection", "Connection to {0} cancelled by user", CurrentEndPoint);
                Release( userRequestedDisconnect: true );
                return;
            }
            
            var asyncResult = socket!.BeginConnect(CurrentEndPoint, null, null );
            if ( WaitHandle.WaitAny( new WaitHandle[] { asyncResult.AsyncWaitHandle, cancellationToken.Token.WaitHandle }, timeout ) == 0 )
            {
                try
                {
                    socket.EndConnect( asyncResult );
                    ConnectCompleted( true );
                }
                catch ( Exception ex )
                {
                    log.LogDebug( "TcpConnection", "Socket exception while completing connection request to {0}: {1}", CurrentEndPoint, ex );
                    ConnectCompleted( false );
                }
            }
            else
=======
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
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            {
                ConnectCompleted( false );
            }
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point to connect to.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
        public void Connect(EndPoint endPoint, int timeout)
        {
            lock ( netLock )
            {
                Debug.Assert( cancellationToken == null );
                cancellationToken = new CancellationTokenSource();

                socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                socket.ReceiveTimeout = timeout;
                socket.SendTimeout = timeout;

                CurrentEndPoint = endPoint;
                log.LogDebug( "TcpConnection", "Connecting to {0}...", CurrentEndPoint);
                TryConnect( timeout );
=======
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
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            }

        }

        public void Disconnect( bool userInitiated )
        {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
            lock ( netLock )
            {
                cancellationToken?.Cancel();

                Disconnected?.Invoke( this, new DisconnectedEventArgs( userInitiated ) );
=======
            // acquire and hold connectLock such that the winning thread must complete this work before anyone else can Connect (or Disconnect no-op)
            lock ( connectLock )
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
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
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

<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
            DebugLog.Assert( cancellationToken != null, nameof( TcpConnection ), "null cancellationToken in NetLoop" );

            while (!cancellationToken.IsCancellationRequested)
=======
            while ( !connectionState.CancellationToken.IsCancellationRequested )
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            {
                bool canRead = false;

                try
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    canRead = socket!.Poll(POLL_MS * 1000, SelectMode.SelectRead);
=======
                    canRead = connectionState.Socket.Poll( POLL_MS * 1000, SelectMode.SelectRead );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                }
                catch ( SocketException ex )
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    log.LogDebug( "TcpConnection", "Socket exception while polling: {0}", ex);
=======
                    DebugLog.WriteLine( "TcpConnection", "Socket exception while polling: {0}", ex );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                    break;
                }

                if ( !canRead )
                {
                    // nothing to read yet
                    continue;
                }

                byte[] packData;

                try
                {
                    // read the packet off the network
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    packData = ReadPacket();
=======
                    packData = ReadPacket( connectionState );

                    // decrypt the data off the wire if needed
                    if ( connectionState.NetFilter != null )
                    {
                        packData = connectionState.NetFilter.ProcessIncoming( packData );
                    }
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                }
                catch ( IOException ex )
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    log.LogDebug("TcpConnection", "Socket exception occurred while reading packet: {0}", ex);
=======
                    DebugLog.WriteLine( "TcpConnection", "Socket exception occurred while reading packet: {0}", ex );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                    break;
                }

                try
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    NetMsgReceived?.Invoke( this, new NetMsgEventArgs( packData, CurrentEndPoint! ) );
=======
                    OnNetMsgReceived( new NetMsgEventArgs( packData, connectionState.Destination ) );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                }
                catch ( Exception ex )
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    log.LogDebug( "TcpConnection", "Unexpected exception propogated back to NetLoop: {0}", ex);
=======
                    DebugLog.WriteLine( "TcpConnection", "Unexpected exception propogated back to NetLoop: {0}", ex );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
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
                packetLen = netReader!.ReadUInt32();
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

<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
        public void Send( byte[] data )
        {
            lock ( netLock )
=======
        public override void Send( IClientMsg clientMsg )
        {
            var connectionState = activeConnectionState;

            if ( connectionState == null )
            {
                DebugLog.WriteLine( "TcpConnection", "Attempting to send client message when not connected: {0}", clientMsg.MsgType );
                return;
            }

            lock ( connectionState.NetLock )
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
            {
                if ( connectionState.Socket == null || connectionState.NetStream == null )
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    log.LogDebug( "TcpConnection", "Attempting to send client data when not connected.");
                    return;
                }

                try
                {
                    netWriter!.Write((uint)data.Length);
                    netWriter.Write(MAGIC);
                    netWriter.Write(data);
=======
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
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                }
                catch ( IOException ex )
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    log.LogDebug( "TcpConnection", "Socket exception while writing data: {0}", ex);
=======
                    DebugLog.WriteLine( "TcpConnection", "Socket exception while writing data: {0}", ex );
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                }
            }
        }

        public IPAddress GetLocalIP()
        {
            var connectionState = activeConnectionState;

            if ( connectionState == null ) return IPAddress.None;

            lock ( connectionState.NetLock )
            {
                if ( connectionState.Socket != null )
                {
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
                    log.LogDebug( "TcpConnection", "Socket exception trying to read bound IP: {0}", ex);
                    return IPAddress.None;
=======
                    try
                    {
                        return NetHelpers.GetLocalIP( connectionState.Socket );
                    }
                    catch ( Exception ex )
                    {
                        DebugLog.WriteLine( "TcpConnection", "Socket exception trying to read bound IP: {0}", ex );
                        return IPAddress.None;
                    }
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
                }
            }

            return IPAddress.None;
        }
<<<<<<< HEAD:SteamKit2/Networking/TcpConnection.cs
=======

        public override void SetNetEncryptionFilter( INetFilterEncryption filter )
        {
            var connectionState = activeConnectionState;

            if ( connectionState == null ) return;
            connectionState.SetNetFilter( filter );
        }
>>>>>>> upstream/cleanup-tcp-connection:SteamKit2/SteamKit2/Networking/Steam3/TcpConnection.cs
    }
}
