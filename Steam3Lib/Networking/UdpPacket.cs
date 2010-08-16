using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamLib
{
    class UdpPacket
    {

        public UdpHeader Header { get; private set; }
        public byte[] Payload { get; private set; }

        public bool IsValid { get; private set; }


        public UdpPacket( byte[] data )
        {
            this.IsValid = false;

            int headerSize = Marshal.SizeOf( typeof( UdpHeader ) );

            if ( data.Length < headerSize )
                return;

            this.Header = UdpHeader.Deserialize( data );

            if ( this.Header == null )
                return;

            if ( this.Header.Magic != UdpHeader.PACKET_MAGIC )
                return;

            this.Payload = new byte[ Header.PayloadSize ];

            Array.Copy( data, headerSize, Payload, 0, Payload.Length );

            this.IsValid = true;

        }

        public UdpPacket( EUdpPacketType type )
        {
            this.Header = new UdpHeader();
            this.Payload = new byte[ 0 ];

            this.Header.PacketType = type;

            this.IsValid = true;
        }


        public void SetPayload( byte[] data )
        {
            Payload = data;
        }

        public byte[] GetData()
        {
            Header.PayloadSize = ( ushort )Payload.Length;
            Header.MsgSize = ( uint )Payload.Length;

            ByteBuffer bb = new ByteBuffer();

            bb.Append( Header.Serialize() );
            bb.Append( Payload );

            return bb.ToArray();
        }

    }
}
