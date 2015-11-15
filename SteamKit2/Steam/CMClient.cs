/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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
        /// Gets the connected universe of this client.
        /// This value will be <see cref="EUniverse.Invalid"/> if the client is not connected to Steam.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse ConnectedUniverse { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected to the remote CM server.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get { return ConnectedUniverse != EUniverse.Invalid; } }

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


        Connection connection;
        byte[] tempSessionKey;

        ScheduledFunction heartBeatFunc;

        Dictionary<EServerType, List<IPEndPoint>> serverMap;


        static CMClient()
        {
            Servers = new SmartCMServerList();
            Servers.UseInbuiltList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMClient"/> class with a specific connection type.
        /// </summary>
        /// <param name="type">The connection type to use.</param>
        /// <exception cref="NotSupportedException">
        /// The provided <see cref="ProtocolType"/> is not supported.
        /// Only Tcp and Udp are available.
        /// </exception>
        public CMClient( ProtocolType type = ProtocolType.Tcp )
        {
            serverMap = new Dictionary<EServerType, List<IPEndPoint>>();

            // our default timeout
            ConnectionTimeout = TimeSpan.FromSeconds( 5 );

            switch ( type )
            {
                case ProtocolType.Tcp:
                    connection = new TcpConnection();
                    break;

                case ProtocolType.Udp:
                    connection = new UdpConnection();
                    break;

                default:
                    throw new NotSupportedException( "The provided protocol type is not supported. Only Tcp and Udp are available." );
            }

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
        public void Connect( IPEndPoint cmServer = null  )
        {
            this.Disconnect();

            if ( cmServer == null )
            {
                cmServer = Servers.GetNextServerCandidate();
            }

            connection.Connect( cmServer, ( int )ConnectionTimeout.TotalMilliseconds );
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


            // we'll swallow any network failures here because they will be thrown later
            // on the network thread, and that will lead to a disconnect callback
            // down the line

            try
            {
                connection.Send( msg );
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
            List<IPEndPoint> list;
            if ( !serverMap.TryGetValue( type, out list ) )
                return new List<IPEndPoint>();

            return list;
        }


        /// <summary>
        /// Called when a client message is received from the network.
        /// </summary>
        /// <param name="packetMsg">The packet message.</param>
        protected virtual void OnClientMsgReceived( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                DebugLog.WriteLine( "CMClient", "Packet message failed to parse, shutting down connection" );
                Disconnect();
                return;
            }

            DebugLog.WriteLine( "CMClient", "<- Recv'd EMsg: {0} ({1}) (Proto: {2})", packetMsg.MsgType, ( int )packetMsg.MsgType, packetMsg.IsProto );

            switch ( packetMsg.MsgType )
            {
                case EMsg.ChannelEncryptRequest:
                    HandleEncryptRequest( packetMsg );
                    break;

                case EMsg.ChannelEncryptResult:
                    HandleEncryptResult( packetMsg );
                    break;

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
        }
        /// <summary>
        /// Called when the client is physically disconnected from Steam3.
        /// </summary>
        protected abstract void OnClientDisconnected( bool userInitiated );


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            OnClientMsgReceived( GetPacketMsg( e.Data ) );
        }

        void Connected( object sender, EventArgs e )
        {
            Servers.TryMark( connection.CurrentEndPoint, ServerQuality.Good );
        }

        void Disconnected( object sender, DisconnectedEventArgs e )
        {
            if ( !e.UserInitiated )
            {
                Servers.TryMark( connection.CurrentEndPoint, ServerQuality.Bad );
            }

            SessionID = null;
            SteamID = null;

            ConnectedUniverse = EUniverse.Invalid;

            heartBeatFunc.Stop();

            OnClientDisconnected( userInitiated: e.UserInitiated );
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

                    OnClientMsgReceived( GetPacketMsg( subData ) );
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
        void HandleEncryptRequest( IPacketMsg packetMsg )
        {
            var encRequest = new Msg<MsgChannelEncryptRequest>( packetMsg );

            EUniverse eUniv = encRequest.Body.Universe;
            uint protoVersion = encRequest.Body.ProtocolVersion;

            DebugLog.WriteLine( "CMClient", "Got encryption request. Universe: {0} Protocol ver: {1}", eUniv, protoVersion );
            DebugLog.Assert( protoVersion == 1, "CMClient", "Encryption handshake protocol version mismatch!" );

            byte[] pubKey = KeyDictionary.GetPublicKey( eUniv );

            if ( pubKey == null )
            {
                DebugLog.WriteLine( "CMClient", "HandleEncryptionRequest got request for invalid universe! Universe: {0} Protocol ver: {1}", eUniv, protoVersion );
                return;
            }

            ConnectedUniverse = eUniv;

            var encResp = new Msg<MsgChannelEncryptResponse>();

            tempSessionKey = CryptoHelper.GenerateRandomBlock( 32 );
            byte[] cryptedSessKey = null;

            using ( var rsa = new RSACrypto( pubKey ) )
            {
                cryptedSessKey = rsa.Encrypt( tempSessionKey );
            }

            byte[] keyCrc = CryptoHelper.CRCHash( cryptedSessKey );

            encResp.Write( cryptedSessKey );
            encResp.Write( keyCrc );
            encResp.Write( ( uint )0 );

            this.Send( encResp );
        }
        void HandleEncryptResult( IPacketMsg packetMsg )
        {
            var encResult = new Msg<MsgChannelEncryptResult>( packetMsg );

            DebugLog.WriteLine( "CMClient", "Encryption result: {0}", encResult.Body.Result );

            if ( encResult.Body.Result == EResult.OK )
            {
                connection.SetNetEncryptionFilter( new NetFilterEncryption( tempSessionKey ) );
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
                EServerType type = ( EServerType )server.server_type;

                List<IPEndPoint> endpointList;
                if ( !serverMap.TryGetValue( type, out endpointList ) )
                {
                    serverMap[ type ] = endpointList = new List<IPEndPoint>();
                }

                endpointList.Add( new IPEndPoint( NetHelpers.GetIPAddress( server.server_ip ), ( int )server.server_port ) );
            }
        }
        void HandleCMList( IPacketMsg packetMsg )
        {
            var cmMsg = new ClientMsgProtobuf<CMsgClientCMList>( packetMsg );
            DebugLog.Assert( cmMsg.Body.cm_addresses.Count == cmMsg.Body.cm_ports.Count, "CMClient", "HandleCMList received malformed message" );

            var cmList = cmMsg.Body.cm_addresses
                .Zip( cmMsg.Body.cm_ports, ( addr, port ) => new IPEndPoint( NetHelpers.GetIPAddress( addr ), ( int )port ) );

            // update our bootstrap list with steam's list of CMs
            foreach ( var cm in cmList )
            {
                Servers.TryAdd( cm );
            }
        }
        void HandleSessionToken( IPacketMsg packetMsg )
        {
            var sessToken = new ClientMsgProtobuf<CMsgClientSessionToken>( packetMsg );

            SessionToken = sessToken.Body.token;
        }
        #endregion
    }
}
