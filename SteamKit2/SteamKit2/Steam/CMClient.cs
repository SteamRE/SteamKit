/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.Discovery;

namespace SteamKit2.Internal
{
    /// <summary>
    /// This base client handles the underlying connection to a CM server. This class should not be use directly, but through the <see cref="SteamClient"/> class.
    /// </summary>
    public abstract class CMClient
    {
        /// <summary>
        /// The configuration for this client.
        /// </summary>
        public SteamConfiguration Configuration { get; }

        /// <summary>
        /// Bootstrap list of CM servers.
        /// </summary>
        public SmartCMServerList Servers => Configuration.ServerList;

        /// <summary>
        /// Returns the the local IP of this client.
        /// </summary>
        /// <returns>The local IP.</returns>
        public IPAddress LocalIP => connection?.GetLocalIP();

        /// <summary>
        /// Gets the public IP address of this client. This value is assigned after a logon attempt has succeeded.
        /// This value will be <c>null</c> if the client is logged off of Steam.
        /// </summary>
        /// <value>The SteamID.</value>
        public IPAddress PublicIP { get; private set; }

        /// <summary>
        /// Gets the country code of our public IP address according to Steam. This value is assigned after a logon attempt has succeeded.
        /// This value will be <c>null</c> if the client is logged off of Steam.
        /// </summary>
        /// <value>The SteamID.</value>
        public string IPCountryCode { get; private set; }

        /// <summary>
        /// Gets the universe of this client.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse Universe => Configuration.Universe;

        /// <summary>
        /// Gets a value indicating whether this instance is connected to the remote CM server.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the session token assigned to this client from the AM.
        /// </summary>
        public ulong SessionToken { get; private set; }

        /// <summary>
        /// Gets the Steam recommended Cell ID of this client. This value is assigned after a logon attempt has succeeded.
        /// This value will be <c>null</c> if the client is logged off of Steam.
        /// </summary>
        public uint? CellID { get; private set; }

        /// <summary>
        /// Gets the session ID of this client. This value is assigned after a logon attempt has succeeded.
        /// This value will be <c>null</c> if the client is logged off of Steam.
        /// </summary>
        /// <value>The session ID.</value>
        public int? SessionID { get; private set; }
        /// <summary>
        /// Gets the SteamID of this client. This value is assigned after a logon attempt has succeeded.
        /// This value will be <c>null</c> if the client is logged off of Steam.
        /// </summary>
        /// <value>The SteamID.</value>
        public SteamID SteamID { get; private set; }

        /// <summary>
        /// Gets or sets the connection timeout used when connecting to the Steam server.
        /// </summary>
        /// <value>
        /// The connection timeout.
        /// </value>
        public TimeSpan ConnectionTimeout => Configuration.ConnectionTimeout;

        /// <summary>
        /// Gets or sets the network listening interface. Use this for debugging only.
        /// For your convenience, you can use <see cref="NetHookNetworkListener"/> class.
        /// </summary>
        public IDebugNetworkListener DebugNetworkListener { get; set; }

        internal bool ExpectDisconnection { get; set; }

        // connection lock around the setup and tear down of the connection task
        object connectionLock = new object();
        CancellationTokenSource connectionCancellation;
        Task connectionSetupTask;
        volatile IConnection connection;

        ScheduledFunction heartBeatFunc;

        Dictionary<EServerType, HashSet<IPEndPoint>> serverMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="CMClient"/> class with a specific configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use for this client.</param>
        /// <exception cref="ArgumentNullException">The configuration object is <c>null</c></exception>
        public CMClient( SteamConfiguration configuration )
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            serverMap = new Dictionary<EServerType, HashSet<IPEndPoint>>();

            heartBeatFunc = new ScheduledFunction( () =>
            {
                Send( new ClientMsgProtobuf<CMsgClientHeartBeat>( EMsg.ClientHeartBeat ) );
            } );
        }

        /// <summary>
        /// Connects this client to a Steam3 server.
        /// This begins the process of connecting and encrypting the data channel between the client and the server.
        /// Results are returned asynchronously in a <see cref="SteamClient.ConnectedCallback"/>.
        /// If the server that SteamKit attempts to connect to is down, a <see cref="SteamClient.DisconnectedCallback"/>
        /// will be posted instead.
        /// SteamKit will not attempt to reconnect to Steam, you must handle this callback and call Connect again
        /// preferrably after a short delay.
        /// </summary>
        /// <param name="cmServer">
        /// The <see cref="IPEndPoint"/> of the CM server to connect to.
        /// If <c>null</c>, SteamKit will randomly select a CM server from its internal list.
        /// </param>
        public void Connect( ServerRecord cmServer = null )
        {
            lock ( connectionLock )
            {
                this.Disconnect();
                Debug.Assert( connection == null );

                Debug.Assert( connectionCancellation == null );

                connectionCancellation = new CancellationTokenSource();
                var token = connectionCancellation.Token;

                ExpectDisconnection = false;

                Task<ServerRecord> recordTask = null;

                if ( cmServer == null )
                {
                    recordTask = Servers.GetNextServerCandidateAsync( Configuration.ProtocolTypes );
                }
                else
                {
                    recordTask = Task.FromResult( cmServer );
                }

                connectionSetupTask = recordTask.ContinueWith( t =>
                {
                    if ( token.IsCancellationRequested )
                    {
                        DebugLog.WriteLine( nameof( CMClient ), "Connection cancelled before a server could be chosen." );
                        OnClientDisconnected( userInitiated: true );
                        return;
                    }
                    else if ( t.IsFaulted || t.IsCanceled )
                    {
                        DebugLog.WriteLine( nameof( CMClient ), "Server record task threw exception: {0}", t.Exception );
                        OnClientDisconnected( userInitiated: false );
                        return;
                    }

                    var record = t.Result;

                    connection = CreateConnection( record.ProtocolTypes & Configuration.ProtocolTypes );
                    connection.NetMsgReceived += NetMsgReceived;
                    connection.Connected += Connected;
                    connection.Disconnected += Disconnected;
                    connection.Connect( record.EndPoint, ( int )ConnectionTimeout.TotalMilliseconds );
                }, TaskContinuationOptions.ExecuteSynchronously ).ContinueWith( t =>
              {
                    if ( t.IsFaulted )
                    {
                        DebugLog.WriteLine( nameof( CMClient ), "Unhandled exception when attempting to connect to Steam: {0}", t.Exception );
                        OnClientDisconnected( userInitiated: false );
                    }
                }, TaskContinuationOptions.ExecuteSynchronously );
            }
        }

        /// <summary>
        /// Disconnects this client.
        /// </summary>
        public void Disconnect() => Disconnect( userInitiated: true ); 

        void Disconnect( bool userInitiated)
        {
            lock ( connectionLock )
            {
                heartBeatFunc.Stop();

                if ( connectionCancellation != null )
                {
                    connectionCancellation.Cancel();
                    connectionCancellation.Dispose();
                    connectionCancellation = null;
                }

                if ( connectionSetupTask != null )
                {
                    // though it's ugly, we want to wait for the completion of this task and keep hold of the lock
                    connectionSetupTask.GetAwaiter().GetResult();
                    connectionSetupTask = null;
                }

                // Connection implementations are required to issue the Disconnected callback before Disconnect() returns
                connection?.Disconnect( userInitiated );
                Debug.Assert( connection == null );
            }
        }

        /// <summary>
        /// Sends the specified client message to the server.
        /// This method automatically assigns the correct SessionID and SteamID of the message.
        /// </summary>
        /// <param name="msg">The client message to send.</param>
        public void Send( IClientMsg msg )
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( nameof(msg), "A value for 'msg' must be supplied" );
            }

            if ( this.SessionID.HasValue )
            {
                msg.SessionID = this.SessionID.Value;
            }

            if ( this.SteamID != null )
            {
                msg.SteamID = this.SteamID;
            }

            DebugLog.WriteLine( "CMClient", "Sent -> EMsg: {0} (Proto: {1})", msg.MsgType, msg.IsProto );

            try
            {
                DebugNetworkListener?.OnOutgoingNetworkMessage(msg.MsgType, msg.Serialize());
            }
            catch ( Exception e )
            {
                DebugLog.WriteLine( "CMClient", "DebugNetworkListener threw an exception: {0}", e );
            }

            // we'll swallow any network failures here because they will be thrown later
            // on the network thread, and that will lead to a disconnect callback
            // down the line

            try
            {
                connection?.Send( msg.Serialize() );
            }
            catch ( IOException )
            {
            }
            catch ( SocketException )
            {
            }
        }


        /// <summary>
        /// Returns the list of servers matching the given type
        /// </summary>
        /// <param name="type">Server type requested</param>
        /// <returns>List of server endpoints</returns>
        public List<IPEndPoint> GetServersOfType( EServerType type )
        {
            if ( !serverMap.TryGetValue( type, out var set ) )
                return new List<IPEndPoint>();

            return set.ToList();
        }


        /// <summary>
        /// Called when a client message is received from the network.
        /// </summary>
        /// <param name="packetMsg">The packet message.</param>
        protected virtual bool OnClientMsgReceived( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                DebugLog.WriteLine( "CMClient", "Packet message failed to parse, shutting down connection" );
                Disconnect();
                return false;
            }

            DebugLog.WriteLine( "CMClient", "<- Recv'd EMsg: {0} ({1}) (Proto: {2})", packetMsg.MsgType, ( int )packetMsg.MsgType, packetMsg.IsProto );

            // Multi message gets logged down the line after it's decompressed
            if ( packetMsg.MsgType != EMsg.Multi )
            {
                try
                {
                    DebugNetworkListener?.OnIncomingNetworkMessage( packetMsg.MsgType, packetMsg.GetData() );
                }
                catch ( Exception e )
                {
                    DebugLog.WriteLine( "CMClient", "DebugNetworkListener threw an exception: {0}", e );
                }
            }

            switch ( packetMsg.MsgType )
            {
                case EMsg.Multi:
                    HandleMulti( packetMsg );
                    break;

                case EMsg.ClientLogOnResponse: // we handle this to get the SteamID/SessionID and to setup heartbeating
                    HandleLogOnResponse( packetMsg );
                    break;

                case EMsg.ClientLoggedOff: // to stop heartbeating when we get logged off
                    HandleLoggedOff( packetMsg );
                    break;

                case EMsg.ClientServerList: // Steam server list
                    HandleServerList( packetMsg );
                    break;

                case EMsg.ClientCMList:
                    HandleCMList( packetMsg );
                    break;

                case EMsg.ClientSessionToken: // am session token
                    HandleSessionToken( packetMsg );
                    break;
            }

            return true;
        }
        /// <summary>
        /// Called when the client is securely connected to Steam3.
        /// </summary>
        protected virtual void OnClientConnected()
        {
        }
        /// <summary>
        /// Called when the client is physically disconnected from Steam3.
        /// </summary>
        protected virtual void OnClientDisconnected( bool userInitiated )
        {
            foreach ( var set in serverMap.Values )
            {
                set.Clear();
            }
        }

        IConnection CreateConnection( ProtocolTypes protocol )
        {
            if ( protocol.HasFlagsFast( ProtocolTypes.WebSocket ) )
            {
                return new WebSocketConnection();
            }
            else if ( protocol.HasFlagsFast( ProtocolTypes.Tcp ) )
            {
                return new EnvelopeEncryptedConnection( new TcpConnection(), Universe );
            }
            else if ( protocol.HasFlagsFast( ProtocolTypes.Udp ) )
            {
                return new EnvelopeEncryptedConnection( new UdpConnection(), Universe );
            }

            throw new ArgumentOutOfRangeException( nameof(protocol), protocol, "Protocol bitmask has no supported protocols set." );
        }


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            OnClientMsgReceived( GetPacketMsg( e.Data ) );
        }

        void Connected( object sender, EventArgs e )
        {
            Servers.TryMark( connection.CurrentEndPoint, connection.ProtocolTypes, ServerQuality.Good );

            IsConnected = true;
            OnClientConnected();
        }

        void Disconnected( object sender, DisconnectedEventArgs e )
        {
            var connectionRelease = Interlocked.Exchange( ref connection, null );
            if ( connectionRelease == null ) return;

            IsConnected = false;

            if ( !e.UserInitiated && !ExpectDisconnection )
            {
                Servers.TryMark( connectionRelease.CurrentEndPoint, connectionRelease.ProtocolTypes, ServerQuality.Bad );
            }

            SessionID = null;
            SteamID = null;

            connectionRelease.NetMsgReceived -= NetMsgReceived;
            connectionRelease.Connected -= Connected;
            connectionRelease.Disconnected -= Disconnected;

            heartBeatFunc.Stop();

            OnClientDisconnected( userInitiated: e.UserInitiated || ExpectDisconnection );
        }

        internal static IPacketMsg GetPacketMsg( byte[] data )
        {
            if ( data.Length < sizeof( uint ) )
            {
                DebugLog.WriteLine( nameof(CMClient), "PacketMsg too small to contain a message, was only {0} bytes. Message: 0x{1}", data.Length, BitConverter.ToString( data ).Replace( "-", string.Empty ) );
                return null;
            }

            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

            switch ( eMsg )
            {
                // certain message types are always MsgHdr
                case EMsg.ChannelEncryptRequest:
                case EMsg.ChannelEncryptResponse:
                case EMsg.ChannelEncryptResult:
                    return new PacketMsg( eMsg, data );
            }

            try
            {
                if (MsgUtil.IsProtoBuf(rawEMsg))
                {
                    // if the emsg is flagged, we're a proto message
                    return new PacketClientMsgProtobuf(eMsg, data);
                }
                else
                {
                    // otherwise we're a struct message
                    return new PacketClientMsg(eMsg, data);
                }
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine( "CMClient", "Exception deserializing emsg {0} ({1}).\n{2}", eMsg, MsgUtil.IsProtoBuf( rawEMsg ), ex.ToString() );
                return null;
            }
        }


        #region ClientMsg Handlers
        void HandleMulti( IPacketMsg packetMsg )
        {
            if ( !packetMsg.IsProto )
            {
                DebugLog.WriteLine( "CMClient", "HandleMulti got non-proto MsgMulti!!" );
                return;
            }

            var msgMulti = new ClientMsgProtobuf<CMsgMulti>( packetMsg );

            byte[] payload = msgMulti.Body.message_body;

            if ( msgMulti.Body.size_unzipped > 0 )
            {
                try
                {
                    using ( var compressedStream = new MemoryStream( payload ) )
                    using ( var gzipStream = new GZipStream( compressedStream, CompressionMode.Decompress ) )
                    using ( var decompressedStream = new MemoryStream() )
                    {
                        gzipStream.CopyTo( decompressedStream );
                        payload = decompressedStream.ToArray();
                    }
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "CMClient", "HandleMulti encountered an exception when decompressing.\n{0}", ex.ToString() );
                    return;
                }
            }

            using ( var ms = new MemoryStream( payload ) )
            using ( var br = new BinaryReader( ms ) )
            {
                while ( ( ms.Length - ms.Position ) != 0 )
                {
                    int subSize = br.ReadInt32();
                    byte[] subData = br.ReadBytes( subSize );

                    if ( !OnClientMsgReceived( GetPacketMsg( subData ) ) )
                    {
                        break;
                    }
                }
            }

        }
        void HandleLogOnResponse( IPacketMsg packetMsg )
        {
            if ( !packetMsg.IsProto )
            {
                // a non proto ClientLogonResponse can come in as a result of connecting but never sending a ClientLogon
                // in this case, it always fails, so we don't need to do anything special here
                DebugLog.WriteLine( "CMClient", "Got non-proto logon response, this is indicative of no logon attempt after connecting." );
                return;
            }

            var logonResp = new ClientMsgProtobuf<CMsgClientLogonResponse>( packetMsg );

            if ( logonResp.Body.eresult == ( int )EResult.OK )
            {
                SessionID = logonResp.ProtoHeader.client_sessionid;
                SteamID = logonResp.ProtoHeader.steamid;

                CellID = logonResp.Body.cell_id;
                PublicIP = NetHelpers.GetIPAddress(logonResp.Body.public_ip);
                IPCountryCode = logonResp.Body.ip_country_code;

                int hbDelay = logonResp.Body.out_of_game_heartbeat_seconds;

                // restart heartbeat
                heartBeatFunc.Stop();
                heartBeatFunc.Delay = TimeSpan.FromSeconds( hbDelay );
                heartBeatFunc.Start();
            }
        }
        void HandleLoggedOff( IPacketMsg packetMsg )
        {
            SessionID = null;
            SteamID = null;

            CellID = null;
            PublicIP = null;
            IPCountryCode = null;

            heartBeatFunc.Stop();

            if ( packetMsg.IsProto )
            {
                var logoffMsg = new ClientMsgProtobuf<CMsgClientLoggedOff>( packetMsg );
                var logoffResult = (EResult)logoffMsg.Body.eresult;

                if ( logoffResult == EResult.TryAnotherCM || logoffResult == EResult.ServiceUnavailable )
                {
                    Servers.TryMark( connection.CurrentEndPoint, connection.ProtocolTypes, ServerQuality.Bad );
                }
            }
        }
        void HandleServerList( IPacketMsg packetMsg )
        {
            var listMsg = new ClientMsgProtobuf<CMsgClientServerList>( packetMsg );

            foreach ( var server in listMsg.Body.servers )
            {
                var type = ( EServerType )server.server_type;

                if ( !serverMap.TryGetValue( type, out var endpointSet ) )
                {
                    serverMap[type] = endpointSet = new HashSet<IPEndPoint>();
                }

                endpointSet.Add( new IPEndPoint( NetHelpers.GetIPAddress( server.server_ip ), ( int )server.server_port ) );
            }
        }
        void HandleCMList( IPacketMsg packetMsg )
        {
            var cmMsg = new ClientMsgProtobuf<CMsgClientCMList>( packetMsg );
            DebugLog.Assert( cmMsg.Body.cm_addresses.Count == cmMsg.Body.cm_ports.Count, "CMClient", "HandleCMList received malformed message" );

            var cmList = cmMsg.Body.cm_addresses
                .Zip( cmMsg.Body.cm_ports, ( addr, port ) => ServerRecord.CreateSocketServer( new IPEndPoint( NetHelpers.GetIPAddress( addr ) , ( int )port ) ) );

            var webSocketList = cmMsg.Body.cm_websocket_addresses.Select( addr => ServerRecord.CreateWebSocketServer( addr ) );

            // update our list with steam's list of CMs
            Servers.ReplaceList( cmList.Concat( webSocketList ) );
        }
        void HandleSessionToken( IPacketMsg packetMsg )
        {
            var sessToken = new ClientMsgProtobuf<CMsgClientSessionToken>( packetMsg );

            SessionToken = sessToken.Body.token;
        }
        #endregion
    }
}
