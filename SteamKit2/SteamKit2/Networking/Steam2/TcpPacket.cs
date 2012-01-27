/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// Represents a data packet sent over tcp. Contains a length and payload.
    /// </summary>
    class TcpPacket : BinaryWriterEx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpPacket"/> class.
        /// </summary>
        public TcpPacket()
            : base( true )
        {
        }

        /// <summary>
        /// Gets the payload of this packet.
        /// </summary>
        /// <returns>The payload.</returns>
        public byte[] GetPayload()
        {
            return this.ToArray();
        }

        /// <summary>
        /// Sets the payload of this packet.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public void SetPayload( byte[] payload )
        {
            this.Clear();
            this.Write( payload );
        }


        /// <summary>
        /// Gets the full packet data with a big-endian length prepended onto the payload.
        /// </summary>
        /// <returns>The full packet data.</returns>
        public byte[] GetData()
        {
            BinaryWriterEx bb = new BinaryWriterEx( true );

            byte[] payload = this.GetPayload();

            bb.Write( ( uint )payload.Length );
            bb.Write( payload );

            return bb.ToArray();
        }

    }
}
