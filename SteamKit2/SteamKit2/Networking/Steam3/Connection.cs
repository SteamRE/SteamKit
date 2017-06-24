/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SteamKit2
{
    /// <summary>
    /// Represents data that has been received over the network.
    /// </summary>
    class NetMsgEventArgs : EventArgs
    {
        public byte[] Data { get; }
        public EndPoint EndPoint { get; }

        public NetMsgEventArgs( byte[] data, EndPoint endPoint )
        {
            this.Data = data;
            this.EndPoint = endPoint;
        }
    }

    class ConnectedEventArgs : EventArgs
    {
        public bool SecureChannel { get; }
        public EUniverse Universe { get; }

        public ConnectedEventArgs( bool secureChannel )
            : this( secureChannel, EUniverse.Invalid )
        {
        }

        public ConnectedEventArgs( bool secureChannel, EUniverse universe )
        {
            this.SecureChannel = secureChannel;
            this.Universe = universe;
        }
    }

    class DisconnectedEventArgs : EventArgs
    {
        public bool UserInitiated { get; }

        public DisconnectedEventArgs( bool userInitiated )
        {
            this.UserInitiated = userInitiated;
        }
    }

    interface INetFilterEncryption
    {
        byte[] ProcessIncoming( byte[] data );
        byte[] ProcessOutgoing( byte[] data );
    }

    class NetFilterEncryption : INetFilterEncryption
    {
        readonly byte[] sessionKey;

        public NetFilterEncryption( byte[] sessionKey )
        {
            DebugLog.Assert( sessionKey.Length == 32, nameof(NetFilterEncryption), "AES session key was not 32 bytes!" );

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
                DebugLog.WriteLine( nameof(NetFilterEncryption), "Unable to decrypt incoming packet: " + ex.Message );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
        }

        public byte[] ProcessOutgoing( byte[] data )
        {
            return CryptoHelper.SymmetricEncrypt( data, sessionKey );
        }
    }

    class NetFilterEncryptionWithHMAC : INetFilterEncryption
    {
        readonly byte[] sessionKey;
        readonly byte[] hmacSecret;

        public NetFilterEncryptionWithHMAC( byte[] sessionKey )
        {
            DebugLog.Assert( sessionKey.Length == 32, nameof(NetFilterEncryption), "AES session key was not 32 bytes!" );

            this.sessionKey = sessionKey;
            this.hmacSecret = new byte[ 16 ];
            Array.Copy( sessionKey, 0, hmacSecret, 0, hmacSecret.Length );
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            try
            {
                return CryptoHelper.SymmetricDecryptHMACIV( data, sessionKey, hmacSecret );
            }
            catch ( CryptographicException ex )
            {
                DebugLog.WriteLine( nameof(NetFilterEncryptionWithHMAC), "Unable to decrypt incoming packet: " + ex.Message );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
        }

        public byte[] ProcessOutgoing( byte[] data )
        {
            return CryptoHelper.SymmetricEncryptWithHMACIV( data, sessionKey, hmacSecret );
        }
    }

    abstract class Connection
    {
        const int DEFAULT_TIMEOUT = 5000;

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
            NetMsgReceived?.Invoke(this, e);
        }

        /// <summary>
        /// The <see cref="System.Net.EndPoint" /> of the current connection.
        /// This is non-null between <see cref="E:Connected"/> and <see cref="E:Disconnected"/>, inclusive.
        /// </summary>
        public abstract EndPoint CurrentEndPoint { get; }

        /// <summary>
        /// Occurs when the physical connection is established.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;
        protected void OnConnected( ConnectedEventArgs e )
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when the physical connection is broken.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        protected void OnDisconnected( DisconnectedEventArgs e )
        {
            Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPointTask">Task returning the end point.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        public abstract void Connect( Task<EndPoint> endPointTask, int timeout = DEFAULT_TIMEOUT );
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

        /// <summary>
        /// Sets the network encryption filter for this connection
        /// </summary>
        /// <param name="filter">filter implementing <see cref="INetFilterEncryption"/></param>
        public abstract void SetNetEncryptionFilter( INetFilterEncryption filter );


        /// <summary>
        /// The type of connection method that this connection uses.
        /// </summary>
        public abstract CMConnectionType Kind { get; }
    }

}
