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

    public class CMClient
    {
        protected Connection Connection { get; private set; }

        byte[] tempSessionKey;

        public EUniverse ConnectedUniverse { get; private set; }

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


        public CMClient()
        {
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
            Connection.Disconnect();
        }

        public void Send( IClientMsg clientMsg )
        {
            Connection.Send( clientMsg );
        }

        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            byte[] data = e.Data;

            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

#if DEBUG
            Trace.WriteLine( string.Format( "CMClient Recv'd EMsg: {0} (Proto: {1})", eMsg, MsgUtil.IsProtoBuf( rawEMsg ) ), "Steam3" );
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
            }

            OnClientMsgReceived( new ClientMsgEventArgs( eMsg, data ) );
        }

        void MultiplexMulti( byte[] payload )
        {
            DataStream ds = new DataStream( payload );

            while ( ds.SizeRemaining() != 0 )
            {
                uint subSize = ds.ReadUInt32();
                byte[] subData = ds.ReadBytes( subSize );

                NetMsgReceived( this, new NetMsgEventArgs( subData ) );
            }
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

            MultiplexMulti( payload );
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
