/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace SteamKit2
{

    /// <summary>
    /// Represents a client message that has been received over the network.
    /// </summary>
    public class ClientMsgEventArgs : NetMsgEventArgs
    {
        EMsg _eMsg;
        /// <summary>
        /// Gets EMsg type of the data.
        /// </summary>
        /// <value>The EMsg the data represents.</value>
        public EMsg EMsg
        {
            get { return MsgUtil.GetMsg( _eMsg ); }
        }

        /// <summary>
        /// Gets a value indicating whether this data is a protobuf message.
        /// </summary>
        /// <value><c>true</c> if the data is protobuf'd; otherwise, <c>false</c>.</value>
        public bool IsProto
        {
            get { return MsgUtil.IsProtoBuf( _eMsg ); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgEventArgs"/> class.
        /// </summary>
        /// <param name="eMsg">The EMsg the data represents.</param>
        /// <param name="data">The data.</param>
        /// <param name="endPoint">The end point the data was received from.</param>
        public ClientMsgEventArgs( EMsg eMsg, byte[] data, IPEndPoint endPoint )
            : base( data, endPoint )
        {
            _eMsg = eMsg;
        }
    }

    /// <summary>
    /// This base client handles the underlying connection to a CM server. This class should not be use directly, but through the <see cref="SteamClient"/> class.
    /// </summary>
    public abstract class CMClient
    {

        /// <summary>
        /// Gets the connected universe of this client.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse ConnectedUniverse { get; private set; }
        /// <summary>
        /// Gets the session ID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The session ID.</value>
        public int SessionID { get; private set; }
        /// <summary>
        /// Gets the SteamID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The SteamID.</value>
        public SteamID SteamID { get; private set; }


        Connection Connection { get; set; }
        byte[] tempSessionKey;

        ScheduledFunction heartBeatFunc;


        /// <summary>
        /// Initializes a new instance of the <see cref="CMClient"/> class.
        /// </summary>
        public CMClient()
        {
            SessionID = default( int );
            SteamID = default( ulong );

            Connection = new UdpConnection();
            Connection.NetMsgReceived += NetMsgReceived;
            Connection.Disconnected += Disconnected;
        }


        /// <summary>
        /// Connects this client to a Steam3 server.
        /// This begins the process of connecting and encrypting the data channel between the client and the server.
        /// Results are returned in a <see cref="ConnectCallback"/>.
        /// </summary>
        public void Connect()
        {
            this.Disconnect();

            // we'll just connect to the first CM server for now
            // not sure how the client challenges servers when using tcp
            // todo: determine if we should try other servers
            this.Connection.Connect( Connection.CMServers[ 0 ] );
        }
        /// <summary>
        /// Disconnects this client.
        /// </summary>
        public void Disconnect()
        {
            if ( heartBeatFunc != null )
            {
                heartBeatFunc.Stop();
            }

            Connection.Disconnect();
        }


        /// <summary>
        /// Sends the specified client message to the server. This method automatically sets the SessionID and SteamID of the message.
        /// </summary>
        /// <typeparam name="MsgType">The MsgType of the client message.</typeparam>
        /// <param name="clientMsg">The client message to send.</param>
        public void Send<MsgType>( ClientMsgProtobuf<MsgType> clientMsg )
            where MsgType : ISteamSerializableMessage, new()
        {
            if ( this.SessionID != default( int ) )
                clientMsg.ProtoHeader.client_session_id = this.SessionID;

            if ( this.SteamID != default( ulong ) )
                clientMsg.ProtoHeader.client_steam_id = this.SteamID;

            DebugLog.WriteLine( "CMClient", "Sent -> EMsg: {0} (Proto: True)", clientMsg.GetEMsg() );

            this.Connection.Send( clientMsg );
        }
        /// <summary>
        /// Sends the specified client message to the server. This method automatically sets the SessionID and SteamID of the message.
        /// </summary>
        /// <typeparam name="MsgType">The MsgType of the client message.</typeparam>
        /// <param name="clientMsg">The client message to send.</param>
        public void Send<MsgType>( ClientMsg<MsgType, ExtendedClientMsgHdr> clientMsg )
            where MsgType : ISteamSerializableMessage, new()
        {
            if ( this.SessionID != default( int ) )
                clientMsg.Header.SessionID = this.SessionID;

            if ( this.SteamID != default( ulong ) )
                clientMsg.Header.SteamID = this.SteamID;

            DebugLog.WriteLine( "CMClient", "Sent -> EMsg: {0} (Proto: False)", clientMsg.GetEMsg() );

            this.Connection.Send( clientMsg );
        }
        /// <summary>
        /// Sends the specified client message to the server. This method automatically sets the SessionID and SteamID of the message.
        /// </summary>
        /// <typeparam name="MsgType">The MsgType of the client message.</typeparam>
        /// <param name="clientMsg">The client message to send.</param>
        public void Send<MsgType>( ClientMsg<MsgType, MsgHdr> clientMsg )
            where MsgType : ISteamSerializableMessage, new()
        {
            DebugLog.WriteLine( "CMClient", "Sent -> EMsg: {0} (Proto: False)", clientMsg.GetEMsg() );

            Connection.Send( clientMsg );
        }

        /// <summary>
        /// Returns the the local IP of this client.
        /// </summary>
        /// <returns>The local IP.</returns>
        public IPAddress GetLocalIP()
        {
            return Connection.GetLocalIP();
        }


        protected abstract void OnClientMsgReceived( ClientMsgEventArgs e );
        protected abstract void OnClientDisconnected();


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            byte[] data = e.Data;

            uint rawEMsg = BitConverter.ToUInt32( data, 0 );
            EMsg eMsg = MsgUtil.GetMsg( rawEMsg );

            ClientMsgEventArgs cliEvent = new ClientMsgEventArgs( ( EMsg )rawEMsg, e.Data, e.EndPoint );

            DebugLog.WriteLine( "CMClient", "<- Recv'd EMsg: {0} ({1}) (Proto: {2})", cliEvent.EMsg, (int)eMsg, cliEvent.IsProto );

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

                case EMsg.ClientLogOnResponse: // we handle this to get the SteamID/SessionID and to setup heartbeating
                    HandleLogOnResponse( cliEvent );
                    break;
            }

            OnClientMsgReceived( cliEvent );
        }
        void Disconnected( object sender, EventArgs e )
        {
            Connection.NetFilter = null;

            OnClientDisconnected();
        }


        void SendHeartbeat()
        {
            var beat = new ClientMsgProtobuf<MsgClientHeartBeat>();
            Send( beat );
        }


        #region ClientMsg Handlers
        void HandleMulti( ClientMsgEventArgs e )
        {
            if ( !e.IsProto )
            {
                DebugLog.WriteLine( "CMClient", "HandleMulti got non-proto MsgMulti!!" );
                return;
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
                    DebugLog.WriteLine( "CMClient", "HandleMulti encountered an exception when decompressing.\n{0}", ex.ToString() );
                    return;
                }
            }

            DataStream ds = new DataStream( payload );

            while ( ds.SizeRemaining() != 0 )
            {
                uint subSize = ds.ReadUInt32();
                byte[] subData = ds.ReadBytes( subSize );

                NetMsgReceived( this, new NetMsgEventArgs( subData, e.EndPoint ) );
            }

        }
        void HandleLogOnResponse( ClientMsgEventArgs e )
        {
            if ( !e.IsProto )
            {
                DebugLog.WriteLine( "CMClient", "HandleLogOnResponse got non-proto MsgClientLogonResponse!!" );
                return;
            }

            var logonResp = new ClientMsgProtobuf<MsgClientLogOnResponse>( e.Data );

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

            DebugLog.WriteLine( "CMClient", "Got encryption request. Universe: {0} Protocol ver: {1}", eUniv, protoVersion );

            ConnectedUniverse = eUniv;

            tempSessionKey = CryptoHelper.GenerateRandomBlock( 32 );
            byte[] pubKey = KeyDictionary.GetPublicKey( eUniv );

            if ( pubKey == null )
            {
                DebugLog.WriteLine( "CMClient", "HandleEncryptionRequest got request for invalid universe! Universe: {0} Protocol ver: {1}", eUniv, protoVersion );
                return;
            }

            var encResp = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>();

            byte[] cryptedSessKey = CryptoHelper.RSAEncrypt( tempSessionKey, pubKey );
            byte[] keyCrc = CryptoHelper.CRCHash( cryptedSessKey );

            encResp.Payload.Write( cryptedSessKey );
            encResp.Payload.Write( keyCrc );
            encResp.Payload.WriteType<uint>( 0 );

            Connection.Send( encResp );
        }
        void HandleEncryptResult( ClientMsgEventArgs e )
        {
            var encResult = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( e.Data );

            DebugLog.WriteLine( "CMClient", "Encryption result: {0}", encResult.Msg.Result );

            if ( encResult.Msg.Result == EResult.OK )
                Connection.NetFilter = new NetFilterEncryption( tempSessionKey );
        }
        #endregion
    }
}
