using System;
using System.IO.Hashing;
using System.Net;
using SteamKit2.Internal;

namespace SteamKit2
{
    class EnvelopeEncryptedConnection : IConnection
    {
        public EnvelopeEncryptedConnection( IConnection inner, EUniverse universe, ILogContext log, IDebugNetworkListener? debugNetworkListener )
        {
            this.inner = inner ?? throw new ArgumentNullException( nameof(inner) );
            this.universe = universe;
            this.log = log ?? throw new ArgumentNullException( nameof( log ) );
            this.debugNetworkListener = debugNetworkListener;

            inner.NetMsgReceived += OnNetMsgReceived;
            inner.Connected += OnConnected;
            inner.Disconnected += OnDisconnected;
        }

        readonly IConnection inner;
        readonly EUniverse universe;
        readonly ILogContext log;
        EncryptionState state;
        INetFilterEncryption? encryption;
        IDebugNetworkListener? debugNetworkListener;

        public EndPoint? CurrentEndPoint => inner.CurrentEndPoint;

        public ProtocolTypes ProtocolTypes => inner.ProtocolTypes;

        public event EventHandler<NetMsgEventArgs>? NetMsgReceived;

        public event EventHandler? Connected;

        public event EventHandler<DisconnectedEventArgs>? Disconnected;

        public void Connect( EndPoint endPoint, int timeout = 5000 )
            => inner.Connect( endPoint, timeout );

        public void Disconnect( bool userInitiated )
        {
            inner.Disconnect( userInitiated );
        }

        public IPAddress? GetLocalIP() => inner.GetLocalIP();

        public void Send( byte[] data )
        {
            if ( state == EncryptionState.Encrypted )
            {
                data = encryption!.ProcessOutgoing( data );
            }

            inner.Send( data );
        }

        void OnConnected( object? sender, EventArgs e )
        {
            state = EncryptionState.Connected;
        }

        void OnDisconnected( object? sender, DisconnectedEventArgs e )
        {
            state = EncryptionState.Disconnected;
            encryption = null;

            Disconnected?.Invoke( this, e );
        }

        void OnNetMsgReceived( object? sender, NetMsgEventArgs e )
        {
            if (state == EncryptionState.Encrypted)
            {
                var plaintextData = encryption!.ProcessIncoming( e.Data );
                NetMsgReceived?.Invoke( this, e.WithData( plaintextData ) );
                return;
            }
            
            var packetMsg = CMClient.GetPacketMsg( e.Data, log );

            if ( packetMsg == null )
            {
                log.LogDebug( nameof(EnvelopeEncryptedConnection), "Failed to parse message during channel setup, shutting down connection" );
                Disconnect( userInitiated: false );
                return;
            }

            try
            {
                debugNetworkListener?.OnIncomingNetworkMessage( packetMsg.MsgType, packetMsg.GetData() );
            }
            catch ( Exception ex )
            {
                log.LogDebug( nameof( EnvelopeEncryptedConnection ), "DebugNetworkListener threw an exception: {0}", ex );
            }

            if ( !IsExpectedEMsg( packetMsg.MsgType ) )
            {
                log.LogDebug( nameof(EnvelopeEncryptedConnection), "Rejected EMsg: {0} during channel setup", packetMsg.MsgType );
                return;
            }

            switch ( packetMsg.MsgType )
            {
                case EMsg.ChannelEncryptRequest:
                    HandleEncryptRequest( packetMsg );
                    break;

                case EMsg.ChannelEncryptResult:
                    HandleEncryptResult( packetMsg );
                    break;
            }
        }
        
        void HandleEncryptRequest( IPacketMsg packetMsg )
        {
            var request = new Msg<MsgChannelEncryptRequest>( packetMsg );

            var connectedUniverse = request.Body.Universe;
            var protoVersion = request.Body.ProtocolVersion;

            log.LogDebug( nameof(EnvelopeEncryptedConnection), "Got encryption request. Universe: {0} Protocol ver: {1}", connectedUniverse, protoVersion );
            DebugLog.Assert( protoVersion == 1, nameof(EnvelopeEncryptedConnection), "Encryption handshake protocol version mismatch!" );
            DebugLog.Assert( connectedUniverse == universe, nameof(EnvelopeEncryptedConnection), FormattableString.Invariant( $"Expected universe {universe} but server reported universe {connectedUniverse}" ) );

            byte[]? randomChallenge;
            if ( request.Payload.Length >= 16 )
            {
                randomChallenge = request.Payload.ToArray();
            }
            else
            {
                randomChallenge = null;
            }

            var publicKey = KeyDictionary.GetPublicKey( connectedUniverse );

            if ( publicKey == null )
            {
                log.LogDebug( nameof(EnvelopeEncryptedConnection), "HandleEncryptRequest got request for invalid universe! Universe: {0} Protocol ver: {1}", connectedUniverse, protoVersion );

                Disconnect( userInitiated: false );
                return;
            }

            var response = new Msg<MsgChannelEncryptResponse>();
            
            var tempSessionKey = CryptoHelper.GenerateRandomBlock( 32 );
            byte[] encryptedHandshakeBlob;
            
            using ( var rsa = new RSACrypto( publicKey ) )
            {
                if ( randomChallenge != null )
                {
                    var blobToEncrypt = new byte[ tempSessionKey.Length + randomChallenge.Length ];
                    Array.Copy( tempSessionKey, blobToEncrypt, tempSessionKey.Length );
                    Array.Copy( randomChallenge, 0, blobToEncrypt, tempSessionKey.Length, randomChallenge.Length );

                    encryptedHandshakeBlob = rsa.Encrypt( blobToEncrypt );
                }
                else
                {
                    encryptedHandshakeBlob = rsa.Encrypt( tempSessionKey );
                }
            }

            var keyCrc = Crc32.Hash( encryptedHandshakeBlob );

            response.Write( encryptedHandshakeBlob );
            response.Write( keyCrc );
            response.Write( ( uint )0 );
            
            if (randomChallenge != null)
            {
                encryption = new NetFilterEncryptionWithHMAC( tempSessionKey, log );
            }
            else
            {
                encryption = new NetFilterEncryption( tempSessionKey, log );
            }

            var serialized = response.Serialize();

            try
            {
                debugNetworkListener?.OnOutgoingNetworkMessage( response.MsgType, serialized );
            }
            catch ( Exception e )
            {
                log.LogDebug( nameof( EnvelopeEncryptedConnection ), "DebugNetworkListener threw an exception: {0}", e );
            }

            state = EncryptionState.Challenged;
            Send( serialized );
        }

        void HandleEncryptResult( IPacketMsg packetMsg )
        {
            var result = new Msg<MsgChannelEncryptResult>( packetMsg );

            log.LogDebug( nameof(EnvelopeEncryptedConnection), "Encryption result: {0}", result.Body.Result );
            DebugLog.Assert( encryption != null, nameof( EnvelopeEncryptedConnection ), "Encryption is null" );

            if ( result.Body.Result == EResult.OK && encryption != null )
            {
                state = EncryptionState.Encrypted;
                Connected?.Invoke( this, EventArgs.Empty );
            }
            else
            {
                log.LogDebug( nameof(EnvelopeEncryptedConnection), "Encryption channel setup failed" );
                Disconnect( userInitiated: false );
            }
        }

        bool IsExpectedEMsg( EMsg msg )
        {
            return state switch
            {
                EncryptionState.Disconnected => false,
                EncryptionState.Connected => msg == EMsg.ChannelEncryptRequest,
                EncryptionState.Challenged => msg == EMsg.ChannelEncryptResult,
                EncryptionState.Encrypted => true,
                _ => throw new InvalidOperationException( "Unreachable - landed up in undefined state." ),
            };
        }

        enum EncryptionState
        {
            Disconnected,
            Connected,
            Challenged,
            Encrypted
        }
    }
}
