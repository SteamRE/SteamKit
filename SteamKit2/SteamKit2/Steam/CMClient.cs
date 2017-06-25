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
        /// Bootstrap list of CM servers.
        /// </summary>
        public static SmartCMServerList Servers { get; private set; }

        /// <summary>
        /// Returns the the local IP of this client.
        /// </summary>
        /// <returns>The local IP.</returns>
        public IPAddress LocalIP
        {
            get { return connection.GetLocalIP(); }
        }

        /// <summary>
        /// Gets the universe of this client.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse Universe { get; }

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
        /// The default value is 5 seconds.
        /// </summary>
        /// <value>
        /// The connection timeout.
        /// </value>
        public TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the network listening interface. Use this for debugging only.
        /// For your convenience, you can use <see cref="NetHookNetworkListener"/> class.
        /// </summary>
        public IDebugNetworkListener DebugNetworkListener { get; set; }

        internal bool ExpectDisconnection { get; set; }

        IConnection connection;

        ScheduledFunction heartBeatFunc;

        Dictionary<EServerType, HashSet<IPEndPoint>> serverMap;


        static CMClient()
        {
            Servers = new SmartCMServerList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMClient"/> class with a specific connection type.
        /// </summary>
        /// <param name="type">The connection types to use.</param>
        /// <param name="universe">The universe to connect to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public CMClient( ProtocolTypes type = ProtocolTypes.Tcp, EUniverse universe = EUniverse.Public )
        {
            serverMap = new Dictionary<EServerType, HashSet<IPEndPoint>>();

            // our default timeout
            ConnectionTimeout = TimeSpan.FromSeconds( 5 );

            switch ( type )
            {
                case ProtocolTypes.Tcp:
                    connection = new EnvelopeEncryptedConnection( new TcpConnection(), universe );
                    break;

                case ProtocolTypes.Udp:
                    connection = new EnvelopeEncryptedConnection( new UdpConnection(), universe );
                    break;

                case ProtocolTypes.WebSocket:
                    connection = new WebSocketConnection();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "The provided protocol type is not a valid enum member." );
            }

            Universe = universe;

            connection.NetMsgReceived += NetMsgReceived;
            connection.Connected += Connected;
            connection.Disconnected += Disconnected;

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
        public void Connect( CMServerRecord cmServer = null  )
        {
            this.Disconnect();

            ExpectDisconnection = false;

            Task<EndPoint> epTask = null;

            if ( cmServer == null )
            {
                epTask = Servers.GetNextServerCandidateAsync( connection.ProtocolTypes )
                    .ContinueWith(r => r.Result.EndPoint, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            else
            {
                epTask = Task.FromResult( cmServer.EndPoint );
            }

            connection.Connect( epTask, ( int )ConnectionTimeout.TotalMilliseconds );
        }

        /// <summary>
        /// Disconnects this client.
        /// </summary>
        public void Disconnect()
        {
            heartBeatFunc.Stop();

            connection.Disconnect();
        }

        /// <summary>
        /// Sends the specified client message to the server.
        /// This method automatically assigns the correct SessionID and SteamID of the message.
        /// </summary>
        /// <param name="msg">The client message to send.</param>
        public void Send( IClientMsg msg )
        {
            if ( msg == null )
                throw new ArgumentException( "A value for 'msg' must be supplied" );

            if ( this.SessionID.HasValue )
                msg.SessionID = this.SessionID.Value;

            if ( this.SteamID != null )
                msg.SteamID = this.SteamID;

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
                connection.Send( msg.Serialize() );
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


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            OnClientMsgReceived( GetPacketMsg( e.Data ) );
        }

        void Connected( object sender, EventArgs e )
        {
            Servers.TryMark( connection.CurrentEndPoint, ServerQuality.Good );

            IsConnected = true;
            OnClientConnected();
        }

        void Disconnected( object sender, DisconnectedEventArgs e )
        {
            IsConnected = false;

            if ( !e.UserInitiated )
            {
                Servers.TryMark( connection.CurrentEndPoint, ServerQuality.Bad );
            }

            SessionID = null;
            SteamID = null;

            heartBeatFunc.Stop();

            OnClientDisconnected( userInitiated: e.UserInitiated || ExpectDisconnection );
        }

        internal static IPacketMsg GetPacketMsg( byte[] data )
        {
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

            heartBeatFunc.Stop();

            if ( packetMsg.IsProto )
            {
                var logoffMsg = new ClientMsgProtobuf<CMsgClientLoggedOff>( packetMsg );
                var logoffResult = (EResult)logoffMsg.Body.eresult;

                if ( logoffResult == EResult.TryAnotherCM || logoffResult == EResult.ServiceUnavailable )
                {
                    Servers.TryMark( connection.CurrentEndPoint, ServerQuality.Bad );
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
                .Zip( cmMsg.Body.cm_ports, ( addr, port ) => CMServerRecord.SocketServer( new IPEndPoint( NetHelpers.GetIPAddress( addr ) , ( int )port ) ) );

            var webSocketList = cmMsg.Body.cm_websocket_addresses.Select( addr => CMServerRecord.WebSocketServer( addr ) );

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
