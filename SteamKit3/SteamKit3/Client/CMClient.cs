/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using log4net.Config;
using log4net;
using ProtoBuf;
using System.Threading.Tasks;
using System.Threading;

namespace SteamKit3
{
    /// <summary>
    /// This base client handles the underlying connection to a CM server. This class should not be use directly, but through the <see cref="SteamClient"/> class.
    /// </summary>
    public abstract class CMClient
    {
        /// <summary>
        /// The connection type to use when connecting to the Steam3 network.
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// Tcp.
            /// </summary>
            Tcp,
            /// <summary>
            /// Udp.
            /// </summary>
            Udp,
        }

        internal Connection Connection { get; private set; }

        /// <summary>
        /// Gets the steam universe this client is connected to.
        /// </summary>
        /// <value>The universe.</value>
        public EUniverse ConnectedUniverse { get; internal set; }

        /// <summary>
        /// Gets the session ID of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The session ID.</value>
        public int SessionID { get; internal set; }
        /// <summary>
        /// Gets the <see cref="SteamID"/> of this client. This value is assigned after a logon attempt has succeeded.
        /// </summary>
        /// <value>The <see cref="SteamID"/>.</value>
        public SteamID SteamID { get; internal set; }


        /// <summary>
        /// Gets the log4net log context for this <see cref="CMClient"/>.
        /// </summary>
        protected ILog Log = LogManager.GetLogger( typeof( CMClient ) );

        Timer heartbeatTimer;


        /// <summary>
        /// Initializes a new instance of the <see cref="CMClient"/> class.
        /// </summary>
        /// <param name="connType">The connection type to use.</param>
        public CMClient( ConnectionType connType = ConnectionType.Tcp )
        {
            // initialize our session members to some easy defaults
            SessionID = -1;
            SteamID = new SteamID();

            switch ( connType )
            {
                case ConnectionType.Tcp:
                    Connection = new TcpConnection();
                    break;

                case ConnectionType.Udp:
                    Connection = new UdpConnection();
                    break;
            }

            Connection.Connected += Connected;
            Connection.Disconnected += Disconnected;
            Connection.NetMsgReceived += NetMsgReceived;

            heartbeatTimer = new Timer( HeartbeatFunc );
        }


        /// <summary>
        /// Connects this client to a Steam3 server.
        /// This begins the process of connecting and encrypting the data channel between the client and the server.
        /// Results are returned in a <see cref="SteamClient.ConnectedCallback"/>.
        /// </summary>
        public void Connect()
        {
            this.Disconnect();

            Connection.Connect( Connection.CMServers[ 0 ] );
        }
        /// <summary>
        /// Disconnects this client. Results are given with the <see cref="SteamClient.DisconnectedCallback"/>.
        /// </summary>
        public void Disconnect()
        {
            Connection.Disconnect();
        }

        /// <summary>
        /// Sends the specified client message to the server we're connected to.
        /// </summary>
        /// <param name="msg">The client message.</param>
        public void Send( IClientMsg msg )
        {
            if ( this.SessionID != -1 ) // if we have a session id assigned, all of the messages we send need it
                msg.SessionID = this.SessionID;

            if ( this.SteamID.IsValid() ) // if we have a session steamid, all of our messages we send need it
                msg.SteamID = this.SteamID;

            Log.DebugFormat( "Sent -> EMsg: {0} (Proto: {1})", msg.MsgType, msg.IsProto );

            Connection.Send( msg );
        }


        /// <summary>
        /// Called when the client successfully connects and the channel is encrypted.
        /// </summary>
        protected abstract void OnClientConnected();
        /// <summary>
        /// Called when the client is disconnected.
        /// </summary>
        protected abstract void OnClientDisconnected();
        /// <summary>
        /// Called when the client receieves a network message.
        /// </summary>
        /// <param name="msg">The packet message.</param>
        protected abstract void OnClientMsgReceived( IPacketMsg msg );


        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            var msg = GetPacketMsg( e.Data );

            Log.DebugFormat( "<- Recv'd EMsg: {0} (Proto: {1})", msg.MsgType, msg.IsProto );

            OnClientMsgReceived( msg );
        }
        void Disconnected( object sender, EventArgs e )
        {
            Connection.NetFilter = null;

            OnClientDisconnected();
        }
        void Connected( object sender, EventArgs e )
        {
            OnClientConnected();
        }

        void HeartbeatFunc( object state )
        {
            Send( new ClientMsgProtobuf<CMsgClientHeartBeat>( EMsg.ClientHeartBeat ) );
        }


        /// <summary>
        /// Gets a <see cref="IPacketMsg"/> implementation for the data that represents a client message from the server.
        /// </summary>
        /// <param name="data">Data representing a client message received from the server.</param>
        /// <returns>A <see cref="IPacketMsg"/> that can be used to create a client message.</returns>
        protected IPacketMsg GetPacketMsg( byte[] data )
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


        internal void StartHeartbeat( int seconds )
        {
            int delayMs = ( int )TimeSpan.FromSeconds( seconds ).TotalMilliseconds;

            heartbeatTimer.Change( delayMs, delayMs );
        }
        internal void StopHeartbeat()
        {
            heartbeatTimer.Change( Timeout.Infinite, Timeout.Infinite );
        }
    }
}
