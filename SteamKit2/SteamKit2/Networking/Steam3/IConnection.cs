/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Net;

namespace SteamKit2
{
    interface IConnection
    {
        /// <summary>
        /// Occurs when a net message is recieved over the network.
        /// </summary>
        event EventHandler<NetMsgEventArgs> NetMsgReceived;

        /// <summary>
        /// The remote <see cref="System.Net.EndPoint" /> of the current connection.
        /// This is non-null between <see cref="E:Connected"/> and <see cref="E:Disconnected"/>, inclusive.
        /// </summary>
        EndPoint? CurrentEndPoint { get; }

        /// <summary>
        /// Occurs when the physical connection is established.
        /// </summary>
        event EventHandler? Connected;

        /// <summary>
        /// Occurs when the physical connection is broken.
        /// </summary>
        event EventHandler<DisconnectedEventArgs>? Disconnected;

        /// <summary>
        /// Connects to the specified end point.
        /// </summary>
        /// <param name="endPoint">The end point to connect to.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
       void Connect( EndPoint endPoint, int timeout = 5000 );
        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        /// <param name="userInitiated">If true, this disconnection attempt was initated by a consumer.</param>
        void Disconnect( bool userInitiated );

        /// <summary>
        /// Sends the specified data packet.
        /// </summary>
        /// <param name="data">The data packet to send.</param>
        void Send( byte[] data );

        /// <summary>
        /// Gets the local IP.
        /// </summary>
        /// <returns>The local IP.</returns>
        IPAddress? GetLocalIP();

        /// <summary>
        /// The type of communication protocol that this connection uses.
        /// </summary>
        ProtocolTypes ProtocolTypes { get; }
    }
}
