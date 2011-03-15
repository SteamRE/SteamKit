/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// Represents a client that is capable of connecting to a Steam2 content server.
    /// </summary>
    public sealed class ContentServerClient : ServerClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentServerClient"/> class.
        /// </summary>
        public ContentServerClient()
        {
        }

        /// <summary>
        /// Requests the cell ID of the currently connected content server.
        /// </summary>
        /// <returns>A valid cellid on success, or 0 on failure.</returns>
        public uint GetCellID()
        {
            if ( !this.HandshakeServer( ( EServerType )3 ) )
                return 0; // 0 is the global or error cellid

            TcpPacket packet = new TcpPacket();
            packet.Write( ( uint )2 );

            try
            {
                this.Socket.Send( packet );

                uint cellID = NetHelpers.EndianSwap( this.Socket.Reader.ReadUInt32() );

                return cellID;
            }
            catch
            {
                return 0;
            }
        }
    }
}
