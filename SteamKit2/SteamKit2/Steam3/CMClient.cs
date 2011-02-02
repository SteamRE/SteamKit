using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SteamKit2
{

    public class ClientMsgEventArgs : NetMsgEventArgs
    {
        public EMsg EMsg { get; private set; }

        public ClientMsgEventArgs()
            : base()
        {
            EMsg = EMsg.Invalid;
        }

        public ClientMsgEventArgs( EMsg eMsg, byte[] data )
            : base( data )
        {
            this.EMsg = eMsg;
        }
    }

    public abstract class CMClient
    {

        public EUniverse ConnectedUniverse { get; private set; }
        public int SessionID { get; private set; }
        public ulong SteamID { get; private set; }


        public event EventHandler<ClientMsgEventArgs> ClientMsgReceived;
        protected virtual void OnClientMsgReceived( ClientMsgEventArgs e )
        {
            if ( ClientMsgReceived != null )
                ClientMsgReceived( this, e );
        }

        public event EventHandler ChannelEncrypted;
        protected virtual void OnChannelEncrypted( EventArgs e )
        {
            if ( ChannelEncrypted != null )
                ChannelEncrypted( this, e );
        }


        Connection Connection { get; set; }
        byte[] tempSessionKey;

        ScheduledFunction heartBeatFunc;


        public CMClient()
        {
            SessionID = 0;
            SteamID = 0;

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


        public void Send( IClientMsg clientMsg )
        {
#if DEBUG
            byte[] data = clientMsg.Serialize();

            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

            Trace.WriteLine( string.Format( "CMClient Sent -> EMsg: {0} (Proto: {1})", eMsg, MsgUtil.IsProtoBuf( rawEMsg ) ), "Steam3" );
#endif

            Connection.Send( clientMsg );
        }


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            byte[] data = e.Data;

            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

#if DEBUG
            Trace.WriteLine( string.Format( "CMClient <- Recv'd EMsg: {0} (Proto: {1})", eMsg, MsgUtil.IsProtoBuf( rawEMsg ) ), "Steam3" );
#endif

            switch ( eMsg )
            {
                case EMsg.ChannelEncryptRequest:
                    HandleEncryptRequest( data );
                    break;

                case EMsg.ChannelEncryptResult:
                    HandleEncryptResult( data );
                    break;

                case EMsg.Multi:
                    HandleMulti( data );
                    break;

                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( data );
                    break;
            }

            OnClientMsgReceived( new ClientMsgEventArgs( eMsg, data ) );
        }


        void SendHeartbeat()
        {
            var beat = new ClientMsgProtobuf<MsgClientHeartBeat>();

            beat.ProtoHeader.client_session_id = SessionID;
            beat.ProtoHeader.client_steam_id = SteamID;

            Send( beat );
        }


        void HandleMulti( byte[] data )
        {
            var msgMulti = new ClientMsgProtobuf<MsgMulti>( data );

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
        void HandleLogOnResponse( byte[] data )
        {
            var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( data );

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
        void HandleEncryptRequest( byte[] data )
        {
            var encRequest = new ClientMsg<MsgChannelEncryptRequest, MsgHdr>( data );

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
        void HandleEncryptResult( byte[] data )
        {
            var encResult = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( data );

            Console.WriteLine( "Got Encryption Result: {0}", encResult.Msg.Result );

            Connection.NetFilter = new NetFilterEncryption( tempSessionKey );

            if ( encResult.Msg.Result == EResult.OK )
                OnChannelEncrypted( EventArgs.Empty );
        }

    }
}
