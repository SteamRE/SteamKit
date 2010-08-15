using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Steam3Lib
{
    public class UdpPacket
    {
        UDPPktHdr UdpHeader;


        public void GetData( MemoryStream ms )
        {
            byte[] udpHdr = UdpHeader.Serialize();

            ms.Write( udpHdr, 0, udpHdr.Length );
        }
        public void SetData( MemoryStream ms )
        {
            int hdrSize = Marshal.SizeOf( typeof( UDPPktHdr ) );
            byte[] udpHdr = new byte[ hdrSize ];

            ms.Read( udpHdr, 0, udpHdr.Length );

            UdpHeader = UDPPktHdr.Deserialize( udpHdr );
        }
    }
}
