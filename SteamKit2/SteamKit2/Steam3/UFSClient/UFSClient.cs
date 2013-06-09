/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// Represents a single client that connects to a UFS server.
    /// </summary>
    public partial class UFSClient
    {
        /// <summary>
        /// Gets the connected universe of this client.
        /// This value will be <see cref="EUniverse.Invalid"/> if the client is logged off of Steam.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse ConnectedUniverse { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected to the remote UFS server.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get { return ConnectedUniverse != EUniverse.Invalid; } }

        /// <summary>
        /// Gets or sets the connection timeout used when connecting to the UFS server.
        /// The default value is 5 seconds.
        /// </summary>
        /// <value>
        /// The connection timeout.
        /// </value>
        public TimeSpan ConnectionTimeout { get; set; }


        SteamClient steamClient;

        Connection connection;
        byte[] tempSessionKey;


        /// <summary>
        /// Initializes a new instance of the <see cref="UFSClient"/> class.
        /// </summary>
        /// <param name="steamClient">
        /// The parent <see cref="SteamClient"/> instance that the UFS connection is for.
        /// Callbacks will also be posted through this instance.
        /// </param>
        public UFSClient( SteamClient steamClient )
        {
            this.steamClient = steamClient;

            // our default timeout
            ConnectionTimeout = TimeSpan.FromSeconds( 5 );

            // steamclient has the connection type hardcoded as TCP
            // todo: determine if UFS supports UDP and if we want to support it
            connection = new TcpConnection();

            connection.NetMsgReceived += NetMsgReceived;
            connection.Disconnected += Disconnected;
        }


        /// <summary>
        /// Connects this client to a UFS server.
        /// This begins the process of connecting and encrypting the data channel between the client and the UFS server.
        /// Results are returned asynchronously in a <see cref="ConnectedCallback"/>.
        /// If the UFS server that this client attempts to connect to is down, a <see cref="DisconnectedCallback"/> will be posted instead.
        /// <see cref="UFSClient"/> will not attempt to reconnect to Steam, you must handle this callback and call <see cref="Connect"/> again, preferrably after a short delay.
        /// In order to connect to the UFS server, the parent <see cref="SteamClient"/> must be connected to the CM server.
        /// </summary>
        /// <param name="ufsServer">
        /// The <see cref="IPEndPoint"/> of the UFS server to connect to.
        /// If <c>null</c>, <see cref="UFSClient"/> will randomly select a UFS server from the <see cref="SteamClient"/>'s list of servers.
        /// </param>
        public void Connect( IPEndPoint ufsServer = null )
        {
            DebugLog.Assert( steamClient.IsConnected, "UFSClient", "CMClient is not connected!" );

            this.Disconnect();

            if ( ufsServer == null )
            {
                var serverList = steamClient.GetServersOfType( EServerType.UFS );

                Random random = new Random();
                ufsServer = serverList[ random.Next( serverList.Count ) ];
            }

            connection.Connect( ufsServer, ( int )ConnectionTimeout.TotalMilliseconds );
        }

        /// <summary>
        /// Disconnects this client from the UFS server.
        /// a <see cref="DisconnectedCallback"/> will be posted upon disconnection.
        /// </summary>
        public void Disconnect()
        {
            connection.Disconnect();
        }


        /// <summary>
        /// Attempt to logon to the UFS and authorize the client for the given AppIDs.
        /// The <see cref="UFSClient"/> should be connected before this point.
        /// Results are returned in a <see cref="LoggedOnCallback"/>.
        /// </summary>
        /// <param name="appIds">The AppIDs to authorize when connecting to the UFS.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID Logon( IEnumerable<uint> appIds )
        {
            var loginReq = new ClientMsgProtobuf<CMsgClientUFSLoginRequest>( EMsg.ClientUFSLoginRequest );
            loginReq.SourceJobID = steamClient.GetNextJobID();

            loginReq.Body.apps.AddRange( appIds );
            loginReq.Body.protocol_version = MsgClientLogon.CurrentProtocol;
            loginReq.Body.am_session_token = steamClient.SessionToken;

            Send( loginReq );

            return loginReq.SourceJobID;
        }

        /// <summary>
        /// Begins a request to upload a file to the UFS.
        /// The <see cref="UFSClient"/> should be logged on before this point.
        /// Results are returned in a <see cref="UploadFileResponseCallback"/>.
        /// </summary>
        /// <param name="gameID">The game to upload this file to</param>
        /// <param name="fileName">The filepath of the file as it exists in Steam Cloud</param>
        /// <param name="fileData">The binary data of the file</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID UploadFile( GameID gameID, string fileName, byte[] fileData )
        {
            var compressedData = ZipUtil.Compress( fileData );

            var msg = new ClientMsgProtobuf<CMsgClientUFSUploadFileRequest>( EMsg.ClientUFSUploadFileRequest );
            msg.SourceJobID = steamClient.GetNextJobID();

            msg.Body.app_id = gameID.AppID;
            msg.Body.can_encrypt = false;
            msg.Body.file_name = fileName;
            msg.Body.file_size = (uint)compressedData.Length;
            msg.Body.raw_file_size = (uint)fileData.Length;
            msg.Body.sha_file = CryptoHelper.SHAHash( fileData );
            msg.Body.time_stamp = MicroTime.UtcNow.ToUnixTime();

            Send( msg );

            return msg.SourceJobID;
        }

        /// <summary>
        /// Uploads the actual contents of a file to the UFS.
        /// The <see cref="UFSClient"/> should be logged on before this point.
        /// Results are returned in a <see cref="UploadFileFinishedCallback"/>.
        /// </summary>
        /// <param name="response">The response callback as a result of calling 'UploadFile'</param>
        /// <param name="fileData">The binary data of the file</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public void UploadFileChunks( UploadFileResponseCallback response, byte[] fileData )
        {
            var pkzippedData = ZipUtil.Compress( fileData );
            var buffer = new byte[10240];

            using (var ms = new MemoryStream( pkzippedData ))
            {
                for ( long i = 0; i < ms.Length; i += buffer.Length )
                {
                    var msg = new ClientMsgProtobuf<CMsgClientUFSFileChunk>( EMsg.ClientUFSUploadFileChunk );
                    msg.TargetJobID = response.FileUploadJobID;

                    var bytesRead = ms.Read( buffer, 0, buffer.Length );

                    if ( bytesRead < buffer.Length )
                    {
                        msg.Body.data = buffer.Take( bytesRead ).ToArray();
                    }
                    else
                    {
                        msg.Body.data = buffer;
                    }

                    msg.Body.file_start = (uint)i;
                    msg.Body.sha_file = response.ShaHash;

                    Send( msg );
                }
            }
        }


        /// <summary>
        /// Sends the specified client message to the UFS server.
        /// This method will automatically assign the correct <see cref="IClientMsg.SteamID"/> of the message, as given by the parent <see cref="SteamClient"/>.
        /// </summary>
        /// <param name="msg">The client message to send.</param>
        public void Send( IClientMsg msg )
        {
            msg.SteamID = steamClient.SteamID;

            DebugLog.WriteLine( "UFSClient", "Sent -> EMsg: {0} {1}", msg.MsgType, msg.IsProto ? "(Proto)" : "" );

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



        void Disconnected( object sender, EventArgs e )
        {
            ConnectedUniverse = EUniverse.Invalid;

            connection.NetFilter = null;

            steamClient.PostCallback( new DisconnectedCallback() );
        }

        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            var packetMsg = CMClient.GetPacketMsg( e.Data );

            DebugLog.WriteLine( "UFSClient", "<- Recv'd EMsg: {0} ({1}) {2}", packetMsg.MsgType, (int)packetMsg.MsgType, packetMsg.IsProto ? "(Proto)" : "" );

            var msgDispatch = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ChannelEncryptRequest, HandleEncryptRequest },
                { EMsg.ChannelEncryptResult, HandleEncryptResult },

                { EMsg.ClientUFSLoginResponse, HandleLoginResponse },
                { EMsg.ClientUFSUploadFileResponse, HandleUploadFileResponse },
                { EMsg.ClientUFSUploadFileFinished, HandleUploadFileFinished },
            };

            Action<IPacketMsg> handlerFunc;
            if ( !msgDispatch.TryGetValue( packetMsg.MsgType, out handlerFunc ) )
                return;

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleEncryptRequest( IPacketMsg packetMsg )
        {
            var encRequest = new Msg<MsgChannelEncryptRequest>( packetMsg );

            EUniverse eUniv = encRequest.Body.Universe;
            uint protoVersion = encRequest.Body.ProtocolVersion;

            DebugLog.WriteLine( "UFSClient", "Got encryption request. Universe: {0} Protocol ver: {1}", eUniv, protoVersion );
            DebugLog.Assert( protoVersion == 1, "UFSClient", "Encryption handshake protocol version mismatch!" );

            byte[] pubKey = KeyDictionary.GetPublicKey( eUniv );

            if ( pubKey == null )
            {
                DebugLog.WriteLine( "UFSClient", "HandleEncryptionRequest got request for invalid universe! Universe: {0} Protocol ver: {1}", eUniv, protoVersion );
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

            DebugLog.WriteLine( "UFSClient", "Encryption result: {0}", encResult.Body.Result );

            if ( encResult.Body.Result == EResult.OK )
            {
                connection.NetFilter = new NetFilterEncryption( tempSessionKey );
            }

            steamClient.PostCallback( new ConnectedCallback( encResult.Body ) );
        }
        void HandleLoginResponse( IPacketMsg packetMsg )
        {
            var loginResp = new ClientMsgProtobuf<CMsgClientUFSLoginResponse>( packetMsg );

            var innerCallback = new LoggedOnCallback( loginResp.Body );
            var callback = new SteamClient.JobCallback<LoggedOnCallback>( loginResp.TargetJobID, innerCallback );
            steamClient.PostCallback( callback );
        }
        void HandleUploadFileResponse( IPacketMsg packetMsg )
        {
            var uploadResp = new ClientMsgProtobuf<CMsgClientUFSUploadFileResponse>( packetMsg );

            var innerCallback = new UploadFileResponseCallback( uploadResp.Body, uploadResp.SourceJobID );
            var callback = new SteamClient.JobCallback<UploadFileResponseCallback>( uploadResp.TargetJobID, innerCallback );
            steamClient.PostCallback( callback );
        }
        void HandleUploadFileFinished( IPacketMsg packetMsg )
        {
            var uploadFin = new ClientMsgProtobuf<CMsgClientUFSUploadFileFinished>( packetMsg );

            var innerCallback = new UploadFileFinishedCallback(uploadFin.Body);
            var callback = new SteamClient.JobCallback<UploadFileFinishedCallback>( uploadFin.TargetJobID, innerCallback );
            steamClient.PostCallback( callback );
        }
        #endregion
    }
}
