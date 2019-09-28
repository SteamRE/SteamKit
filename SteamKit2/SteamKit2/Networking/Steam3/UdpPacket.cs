/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SteamKit2.Internal;

namespace SteamKit2
{
    class UdpPacket
    {
        public const uint MAX_PAYLOAD = 0x4DC;

        public UdpHeader Header { get; private set; }

        [DisallowNull, NotNull]
        public MemoryStream? Payload { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                return Header.Magic == UdpHeader.MAGIC
                    && Header.PayloadSize <= MAX_PAYLOAD
                    && Payload != null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPacket"/> class with
        /// information from the memory stream.
        /// 
        /// Header is populated from the MemoryStream
        /// </summary>
        /// <param name="ms">The stream containing the packet and it's payload data.</param>
        public UdpPacket(MemoryStream ms)
        {
            Header = new UdpHeader();

            try
            {
                Header.Deserialize(ms);
            }
            catch ( Exception )
            {
                return;
            }

            if ( this.Header.Magic != UdpHeader.MAGIC )
                return;

            SetPayload(ms, Header.PayloadSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPacket"/> class, with
        /// no payload.
        /// 
        /// Header must be populated manually.
        /// </summary>
        /// <param name="type">The type.</param>
        public UdpPacket(EUdpPacketType type)
        {
            this.Header = new UdpHeader();
            this.Payload = new MemoryStream();

            this.Header.PacketType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPacket"/> class, of the
        /// specified type containing the specified payload.
        /// 
        /// Header must be populated manually.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="payload">The payload.</param>
        public UdpPacket(EUdpPacketType type, MemoryStream payload)
            : this(type)
        {
            SetPayload(payload);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPacket"/> class, of the
        /// specified type containing the first 'length' bytes of specified payload.
        /// 
        /// Header must be populated manually.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="length">The length.</param>
        public UdpPacket(EUdpPacketType type, MemoryStream payload, long length)
            : this(type)
        {
            SetPayload(payload, length);
        }

        /// <summary>
        /// Sets the payload
        /// </summary>
        /// <param name="ms">The payload to copy.</param>
        public void SetPayload(MemoryStream ms)
        {
            SetPayload(ms, ms.Length - ms.Position);
        }

        /// <summary>
        /// Sets the payload.
        /// </summary>
        /// <param name="ms">The payload.</param>
        /// <param name="length">The length.</param>
        public void SetPayload(MemoryStream ms, long length)
        {
            if ( length > MAX_PAYLOAD )
                throw new ArgumentException("Payload length exceeds 0x4DC maximum");

            byte[] buf = new byte[length];
            ms.Read(buf, 0, buf.Length);

            Payload = new MemoryStream(buf);
            Header.PayloadSize = (ushort) Payload.Length;
            Header.MsgSize = (uint) Payload.Length;
        }

        /// <summary>
        /// Serializes the UdpPacket.
        /// </summary>
        /// <returns>The serialized packet.</returns>
        public byte[] GetData()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );

                Payload.Seek( 0, SeekOrigin.Begin );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }

    }
}
