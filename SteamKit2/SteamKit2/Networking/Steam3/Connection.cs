/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Represents data that has been received over the network.
    /// </summary>
    class NetMsgEventArgs : EventArgs
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
            DebugLog.Assert( sessionKey.Length == 32, "NetFilterEncryption", "AES session key was not 32 bytes!" );

            this.sessionKey = sessionKey;
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            try
            {
                return CryptoHelper.SymmetricDecrypt( data, sessionKey );
            }
            catch ( CryptographicException ex )
            {
                DebugLog.WriteLine( "NetFilterEncryption", "Unable to decrypt incoming packet: " + ex.Message );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
        }

        public byte[] ProcessOutgoing( byte[] ms )
        {
            return CryptoHelper.SymmetricEncrypt( ms, sessionKey );
        }
    }

    abstract class Connection
    {
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
        /// Occurs when the physical connection is established.
        /// </summary>
        public event EventHandler Connected;
        protected void OnConnected(EventArgs e)
        {
            if (Connected != null)
                Connected(this, e);
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

        /// <summary>
        /// Gets the local IP.
        /// </summary>
        /// <returns>The local IP.</returns>
        public abstract IPAddress GetLocalIP();
    }

}
