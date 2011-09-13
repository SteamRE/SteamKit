/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit3
{
    public abstract class CMClient
    {
        public enum ConnectionType
        {
            Tcp,
            Udp,
        }

        public EUniverse ConnectedUniverse { get; internal set; }

        internal Connection Connection { get; private set; }


        public CMClient( ConnectionType connType = ConnectionType.Tcp )
        {
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
        }


        public void Connect()
        {
            this.Disconnect();

            Connection.Connect( Connection.CMServers[ 0 ] );
        }
        public void Disconnect()
        {
            Connection.Disconnect();
        }

        public void Send( IClientMsg msg )
        {
            Connection.Send( msg );
        }


        protected abstract void OnClientConnected();
        protected abstract void OnClientDisconnected();
        protected abstract void OnClientMsgReceived( NetMsgEventArgs e );

        void NetMsgReceived( object sender, NetMsgEventArgs e )
        {
            OnClientMsgReceived( e );
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
    }
}
