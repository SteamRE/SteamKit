/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Linq;

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
        public static ReadOnlyCollection<IPEndPoint> Servers { get; private set; }

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
        /// This value will be <see cref="EUniverse.Invalid"/> if the client is logged off of Steam.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse ConnectedUniverse { get; private set; }

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
            Servers = new ReadOnlyCollection<IPEndPoint>( new List<IPEndPoint>
            {
                // Qwest, Seattle
                new IPEndPoint( IPAddress.Parse( "72.165.61.174" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.174" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.175" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.175" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.176" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.176" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.185" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.185" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.187" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.187" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.188" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "72.165.61.188" ), 27018 ),
                // Inteliquent, Luxembourg, cm-[01-04].lux.valve.net
                new IPEndPoint( IPAddress.Parse( "146.66.152.12" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.12" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.12" ), 27019 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.13" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.13" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.13" ), 27019 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.14" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.14" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.14" ), 27019 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.15" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.15" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "146.66.152.15" ), 27019 ),
                /* Highwinds, Netherlands (not live)
                new IPEndPoint( IPAddress.Parse( "81.171.115.5" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.5" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.5" ), 27019 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.6" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.6" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.6" ), 27019 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.7" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.7" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.7" ), 27019 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.8" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.8" ), 27018 ),
                new IPEndPoint( IPAddress.Parse( "81.171.115.8" ), 27019 ),*/
                // Highwinds, Kaysville
                new IPEndPoint( IPAddress.Parse( "209.197.29.196" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "209.197.29.197" ), 27017 ),
                /* Starhub, Singapore (non-optimal route)
                new IPEndPoint( IPAddress.Parse( "103.28.54.10" ), 27017 ),
                new IPEndPoint( IPAddress.Parse( "103.28.54.11" ), 27017 )*/
            } );
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
                Random random = new Random();
                cmServer = Servers[ random.Next( Servers.Count ) ];
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

                case EMsg.ClientCMList:
                    HandleCMList( packetMsg );
                    break;
            }
        }
        /// <summary>
        /// Called when the client is physically disconnected from Steam3.
        /// </summary>
        protected abstract void OnClientDisconnected();


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            OnClientMsgReceived( GetPacketMsg( e.Data ) );
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
        void HandleCMList( IPacketMsg packetMsg )
        {
            var cmMsg = new ClientMsgProtobuf<CMsgClientCMList>( packetMsg );
            DebugLog.Assert( cmMsg.Body.cm_addresses.Count == cmMsg.Body.cm_ports.Count, "CMClient", "HandleCMList received malformed message" );

            var cmList = cmMsg.Body.cm_addresses
                .Zip( cmMsg.Body.cm_ports, ( addr, port ) => new IPEndPoint( NetHelpers.GetIPAddress( addr ), ( int )port ) );

            // update our bootstrap list with steam's list of CMs
            Servers = new ReadOnlyCollection<IPEndPoint>( cmList.ToList() );
        }
        #endregion
    }
}
