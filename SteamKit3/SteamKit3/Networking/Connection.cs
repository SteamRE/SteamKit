/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace SteamKit3
{
    /// <summary>
    /// Represents data that has been received over the network.
    /// </summary>
    public class NetMsgEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public NetMsgEventArgs( byte[] data, IPEndPoint endPoint )
        {
            this.Data = data;
            this.EndPoint = endPoint;
        }
    }

    class NetFilterEncryption
    {
        byte[] sessionKey;

        public NetFilterEncryption( byte[] sessionKey )
        {
            this.sessionKey = sessionKey;
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            return CryptoHelper.SymmetricDecrypt( data, sessionKey );
        }

        public byte[] ProcessOutgoing( byte[] ms )
        {
            return CryptoHelper.SymmetricEncrypt( ms, sessionKey );
        }
    }

    abstract class Connection
    {
        public static readonly IPEndPoint[] CMServers =
        {
            new IPEndPoint( IPAddress.Parse( "68.142.64.164" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.64.165" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.91.34" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.91.35" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.91.36" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.116.178" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "68.142.116.179" ), 27017 ),

            new IPEndPoint( IPAddress.Parse( "69.28.145.170" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "69.28.145.171" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "69.28.145.172" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "69.28.156.250" ), 27017 ),

            new IPEndPoint( IPAddress.Parse( "72.165.61.185" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "72.165.61.186" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "72.165.61.187" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "72.165.61.188" ), 27017 ),

            new IPEndPoint( IPAddress.Parse( "208.111.133.84" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.133.85" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.158.52" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.158.53" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.171.82" ), 27017 ),
            new IPEndPoint( IPAddress.Parse( "208.111.171.83" ), 27017 ),
        };

        /// <summary>
        /// Gets or sets the net filter for this connection.
        /// </summary>
        /// <value>The net filter.</value>
        public NetFilterEncryption NetFilter { get; set; }


        /// <summary>
        /// Occurs when a net message is recieved over the network.
        /// </summary>
        public event EventHandler<NetMsgEventArgs> NetMsgReceived;
        /// <summary>
        /// Raises the <see cref="E:NetMsgReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.NetMsgEventArgs"/> instance containing the event data.</param>
        protected void OnNetMsgReceived( NetMsgEventArgs e )
        {
            if ( NetMsgReceived != null )
                NetMsgReceived( this, e );
        }

        /// <summary>
        /// Occurs when the physical connection is broken.
        /// </summary>
        public event EventHandler Disconnected;
        protected void OnDisconnected( EventArgs e )
        {
            if ( Disconnected != null )
                Disconnected( this, e );
        }

        public event EventHandler Connected;
        protected void OnConnected( EventArgs e )
        {
            if ( Connected != null )
                Connected( this, e );
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public abstract void Connect( IPEndPoint endPoint );
        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Sends the specified client net message.
        /// </summary>
        /// <param name="clientMsg">The client net message.</param>
        public abstract void Send( IClientMsg clientMsg );
    }

}
