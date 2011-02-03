using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SteamKit2
{

    public class ClientMsgEventArgs : NetMsgEventArgs
    {
        EMsg _eMsg;
        public EMsg EMsg
        {
            get { return MsgUtil.GetMsg( _eMsg ); }
        }

        public bool IsProto
        {
            get { return MsgUtil.IsProtoBuf( _eMsg ); }
        }

        public ClientMsgEventArgs()
            : base()
        {
            _eMsg = EMsg.Invalid;
        }

        public ClientMsgEventArgs( EMsg eMsg, byte[] data )
            : base( data )
        {
            _eMsg = eMsg;
        }
    }

    // this base client handles everything required to keep the connection to steam active
    public abstract class CMClient
    {

        public EUniverse ConnectedUniverse { get; private set; }
        public int SessionID { get; private set; }
        public ulong SteamID { get; private set; }


        Connection Connection { get; set; }
        byte[] tempSessionKey;

        ScheduledFunction heartBeatFunc;


        public CMClient()
        {
            SessionID = default( int );
            SteamID = default( ulong );

            // todo: UdpConnection needs an implementation
            Connection = new TcpConnection();
            Connection.NetMsgReceived += NetMsgReceived;
        }


        public void Connect()
        {
            // we'll just connect to the first CM server for now
            // not sure how the client challenges servers when using tcp
            Connection.Connect( Connection.CMServers[ 0 ] );
        }
        public void Disconnect()
        {
            if ( heartBeatFunc != null )
            {
                heartBeatFunc.Stop();
            }

            Connection.Disconnect();
        }

        
        public void Send<MsgType>( ClientMsgProtobuf<MsgType> clientMsg )
            where MsgType : ISteamSerializableMessage, new()
        {
            if ( this.SessionID != default( int ) )
                clientMsg.ProtoHeader.client_session_id = this.SessionID;

            if ( this.SteamID != default( ulong ) )
                clientMsg.ProtoHeader.client_steam_id = this.SteamID;

#if DEBUG
            Trace.WriteLine( string.Format( "CMClient Sent -> EMsg: {0} (Proto: True)", clientMsg.GetEMsg() ), "Steam3" );
#endif

            Connection.Send( clientMsg );
        }
        public void Send<MsgType>( ClientMsg<MsgType, ExtendedClientMsgHdr> clientMsg )
            where MsgType : ISteamSerializableMessage, new()
        {
            if ( this.SessionID != default( int ) )
                clientMsg.Header.SessionID = this.SessionID;

            if ( this.SteamID != default( ulong ) )
                clientMsg.Header.SteamID = this.SteamID;

#if DEBUG
            Trace.WriteLine( string.Format( "CMClient Sent -> EMsg: {0} (Proto: False)", clientMsg.GetEMsg() ), "Steam3" );
#endif

            Connection.Send( clientMsg );
        }
        public void Send<MsgType>( ClientMsg<MsgType, MsgHdr> clientMsg )
            where MsgType : ISteamSerializableMessage, new()
        {

#if DEBUG
            Trace.WriteLine( string.Format( "CMClient Sent -> EMsg: {0} (Proto: False)", clientMsg.GetEMsg() ), "Steam3" );
#endif

            Connection.Send( clientMsg );
        }


        protected virtual void OnClientMsgReceived( ClientMsgEventArgs e )
        {
            // nop, SteamClient handles this
        }


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            byte[] data = e.Data;

            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

            ClientMsgEventArgs cliEvent = new ClientMsgEventArgs( ( EMsg )rawEMsg, e.Data );

#if DEBUG
            Trace.WriteLine( string.Format( "CMClient <- Recv'd EMsg: {0} (Proto: {1})", eMsg, MsgUtil.IsProtoBuf( rawEMsg ) ), "Steam3" );
#endif

            switch ( eMsg )
            {
                case EMsg.ChannelEncryptRequest:
                    HandleEncryptRequest( cliEvent );
                    break;

                case EMsg.ChannelEncryptResult:
                    HandleEncryptResult( cliEvent );
                    break;

                case EMsg.Multi:
                    HandleMulti( cliEvent );
                    break;

                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( cliEvent );
                    break;
            }

            OnClientMsgReceived( cliEvent );
        }


        void SendHeartbeat()
        {
            var beat = new ClientMsgProtobuf<MsgClientHeartBeat>();
            Send( beat );
        }


        void HandleMulti( ClientMsgEventArgs e )
        {
            if ( !e.IsProto )
            {
#if DEBUG
                Trace.WriteLine( "CMClient HandleMulti got non-proto MsgMulti!!", "Steam3" );
                return;
#endif
            }

            var msgMulti = new ClientMsgProtobuf<MsgMulti>( e.Data );

            byte[] payload = msgMulti.Msg.Proto.message_body;

            if ( msgMulti.Msg.Proto.size_unzipped > 0 )
            {
                try
                {
                    payload = ZipUtil.Decompress( payload );
                }
                catch ( Exception ex )
                {
#if DEBUG
                    Trace.WriteLine("CMClient HandleMulti encountered an exception.\n" + ex.ToString(), "Steam3");
#endif

                    return;
                }
            }

            DataStream ds = new DataStream( payload );

            while ( ds.SizeRemaining() != 0 )
            {
                uint subSize = ds.ReadUInt32();
                byte[] subData = ds.ReadBytes( subSize );

                NetMsgReceived( this, new NetMsgEventArgs( subData ) );
            }

        }
        void HandleLogOnResponse( ClientMsgEventArgs e )
        {

            if ( !e.IsProto )
            {
#if DEBUG
                Trace.WriteLine( "CMClient HandleLogOnResponse got non-proto msg!!", "Steam3" );
                return;
#endif
            }

            var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( e.Data );

            if ( logonResp.Msg.Proto.eresult == ( int )EResult.OK )
            {
                SessionID = logonResp.ProtoHeader.client_session_id;
                SteamID = logonResp.ProtoHeader.client_steam_id;

                int hbDelay = logonResp.Msg.Proto.out_of_game_heartbeat_seconds;

                if ( heartBeatFunc != null )
                {
                    heartBeatFunc.Stop();
                }

                heartBeatFunc = new ScheduledFunction(
                    SendHeartbeat,
                    TimeSpan.FromSeconds( hbDelay ),
                    "HeartBeatFunc"
                );
            }
        }
        void HandleEncryptRequest( ClientMsgEventArgs e )
        {
            var encRequest = new ClientMsg<MsgChannelEncryptRequest, MsgHdr>( e.Data );

            EUniverse eUniv = encRequest.Msg.Universe;
            uint protoVersion = encRequest.Msg.ProtocolVersion;

            Console.WriteLine( "Got Encryption Request for {0} universe. Proto version: {1}", eUniv, protoVersion );

            ConnectedUniverse = eUniv;

            tempSessionKey = CryptoHelper.GenerateRandomBlock( 32 );
            byte[] pubKey = KeyDictionary.GetPublicKey( eUniv );

            if ( pubKey == null )
            {
#if DEBUG
                Trace.WriteLine( string.Format( "CMClient HandleEncryptRequest got request for invalid universe! eUniv: {0} Proto: {1}", eUniv, protoVersion ), "Steam3" );
#endif

                return;
            }

            var encResp = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>();

            byte[] cryptedSessKey = CryptoHelper.RSAEncrypt( tempSessionKey, pubKey );
            byte[] keyCrc = CryptoHelper.CRCHash( cryptedSessKey );

            encResp.Payload.Append( cryptedSessKey );
            encResp.Payload.Append( keyCrc );
            encResp.Payload.Append<uint>( 0 );

            Connection.Send( encResp );
        }
        void HandleEncryptResult( ClientMsgEventArgs e )
        {
            var encResult = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( e.Data );

            Console.WriteLine( "Got Encryption Result: {0}", encResult.Msg.Result );

            Connection.NetFilter = new NetFilterEncryption( tempSessionKey );
        }

    }
}
