/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SteamKit2.Internal
{
    /// <summary>
    /// This base client handles the underlying connection to a CM server. This class should not be use directly, but through the <see cref="SteamClient"/> class.
    /// </summary>
    public abstract class CMClient
    {
        const ushort PortCM_PublicEncrypted = 27017;
        const ushort PortCM_Public = 27014;

        /// <summary>
        /// Bootstrap list of CM servers.
        /// </summary>
        public static readonly IPAddress[] Servers =
        {
            // Limelight, New York
            IPAddress.Parse( "68.142.91.34" ),
            IPAddress.Parse( "68.142.91.35" ),
            IPAddress.Parse( "68.142.91.36" ),

            // Limelight, San Jose
            IPAddress.Parse( "68.142.116.178" ),
            IPAddress.Parse( "68.142.116.179" ),

            // Limelight, Los Angeles
            IPAddress.Parse( "69.28.145.170" ),
            IPAddress.Parse( "69.28.145.171" ),
            IPAddress.Parse( "69.28.145.172" ),

            // CenturyLink/Qwest, Seattle
            IPAddress.Parse( "72.165.61.174" ),
            IPAddress.Parse( "72.165.61.175" ),
            IPAddress.Parse( "72.165.61.176" ),
            IPAddress.Parse( "72.165.61.185" ),
            IPAddress.Parse( "72.165.61.186" ),
            IPAddress.Parse( "72.165.61.187" ),
            IPAddress.Parse( "72.165.61.188" ),

            // Eweka, Netherlands
            IPAddress.Parse( "81.171.115.5" ),
            IPAddress.Parse( "81.171.115.6" ),
            IPAddress.Parse( "81.171.115.7" ),
            IPAddress.Parse( "81.171.115.8" ),

            // Limelight, Japan
            IPAddress.Parse( "203.77.185.4" ),
            IPAddress.Parse( "203.77.185.5" ),

            // Limelight, Seattle
            IPAddress.Parse( "208.111.133.84" ),
            IPAddress.Parse( "208.111.133.85" ),

            // Limelight, Chicago
            IPAddress.Parse( "208.111.158.52" ),
            IPAddress.Parse( "208.111.158.53" ),
            IPAddress.Parse( "208.111.171.82" ),
            IPAddress.Parse( "208.111.171.83" ),
        };

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
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse ConnectedUniverse { get; private set; }
        /// <summary>
        /// Gets the session ID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The session ID.</value>
        public int? SessionID { get; private set; }
        /// <summary>
        /// Gets the SteamID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The SteamID.</value>
        public SteamID SteamID { get; private set; }

        Connection connection;
        byte[] tempSessionKey;
        bool encrypted;

        ScheduledFunction heartBeatFunc;

        Dictionary<EServerType, List<IPEndPoint>> serverMap;


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
            connection.Disconnected += Disconnected;
            connection.Connected += Connected;
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
        /// <param name="bEncrypted">
        /// If set to <c>true</c> the underlying connection to Steam will be encrypted. This is the default mode of communication.
        /// Previous versions of SteamKit always used encryption.
        /// </param>
        public void Connect( bool bEncrypted = true )
        {
            this.Disconnect();
            encrypted = bEncrypted;

            Random random = new Random();

            var server = Servers[ random.Next( Servers.Length ) ];
            var endPoint = new IPEndPoint( server, bEncrypted ? PortCM_PublicEncrypted : PortCM_Public );

            connection.Connect( endPoint );
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
            }
        }
        /// <summary>
        /// Called when the client is physically disconnected from Steam3.
        /// </summary>
        protected abstract void OnClientDisconnected();
        /// <summary>
        /// Called when the client is connected to Steam3 and is ready to send messages.
        /// </summary>
        protected abstract void OnClientConnected();


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            OnClientMsgReceived( GetPacketMsg( e.Data ) );
        }

        void Connected(object sender, EventArgs e)
        {
            // If we're on an encrypted connection, we wait for the handshake to complete
            if ( encrypted )
                return;

            // we only connect to the public universe
            ConnectedUniverse = EUniverse.Public;

            // since there is no encryption handshake, we're 'connected' after the underlying connection is established
            OnClientConnected();
        }

        void Disconnected( object sender, EventArgs e )
        {
            ConnectedUniverse = EUniverse.Invalid;

            heartBeatFunc.Stop();
            connection.NetFilter = null;

            OnClientDisconnected();
        }

        static IPacketMsg GetPacketMsg( byte[] data )
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

            if ( MsgUtil.IsProtoBuf( rawEMsg ) )
            {
                // if the emsg is flagged, we're a proto message
                return new PacketClientMsgProtobuf( eMsg, data );
            }
            else
            {
                // otherwise we're a struct message
                return new PacketClientMsg( eMsg, data );
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
                    payload = ZipUtil.Decompress( payload );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "CMClient", "HandleMulti encountered an exception when decompressing.\n{0}", ex.ToString() );
                    return;
                }
            }

            DataStream ds = new DataStream( payload );

            while ( ds.SizeRemaining() != 0 )
            {
                uint subSize = ds.ReadUInt32();
                byte[] subData = ds.ReadBytes( subSize );

                OnClientMsgReceived( GetPacketMsg( subData ) );
            }

        }
        void HandleLogOnResponse( IPacketMsg packetMsg )
        {
            if ( !packetMsg.IsProto )
            {
                DebugLog.WriteLine( "CMClient", "HandleLogOnResponse got non-proto MsgClientLogonResponse!!" );
                return;
            }


            var logonResp = new ClientMsgProtobuf<CMsgClientLogonResponse>( packetMsg );

            if ( logonResp.Body.eresult == ( int )EResult.OK )
            {
                SessionID = logonResp.ProtoHeader.client_sessionid;
                SteamID = logonResp.ProtoHeader.steamid;

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
                connection.NetFilter = new NetFilterEncryption( tempSessionKey );
            }
        }
        void HandleLoggedOff( IPacketMsg packetMsg )
        {
            SessionID = null;
            SteamID = null;

            heartBeatFunc.Stop();
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
        #endregion
    }
}
