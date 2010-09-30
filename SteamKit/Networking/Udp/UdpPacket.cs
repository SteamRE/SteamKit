using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SteamKit
{
    class UdpPacket
    {

        public UdpHeader Header { get; private set; }
        public MemoryStream Payload { get; private set; }

        public bool IsValid { get; private set; }


        public UdpPacket( MemoryStream ms )
        {
            this.IsValid = false;

            Header = new UdpHeader();

            try
            {
                Header.deserialize(ms);
            }
            catch (Exception)
            {
                return;
            }

            if ( this.Header.Magic != UdpHeader.MAGIC )
                return;

            Payload = new MemoryStream(Header.PayloadSize);

            if(Header.PayloadSize > 0)
                ms.CopyTo(Payload, Header.PayloadSize);
    
            Payload.Seek(0, SeekOrigin.Begin);

            this.IsValid = true;

        }

        public UdpPacket( EUdpPacketType type )
        {
            this.Header = new UdpHeader();
            this.Payload = new MemoryStream();

            this.Header.PacketType = type;

            this.IsValid = true;
        }


        public void SetPayload( MemoryStream ms )
        {
            Payload = ms;
        }

        public MemoryStream GetData()
        {
            Header.PayloadSize = (ushort)Payload.Length;
            Header.MsgSize = (uint)Payload.Length;

            MemoryStream header = Header.serialize();

            MemoryStream ms = new MemoryStream( (int)header.Length + (int)Payload.Length );
            header.CopyTo(ms);
            Payload.CopyTo(ms);

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

    }
}
