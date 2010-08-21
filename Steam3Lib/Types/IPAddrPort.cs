using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace SteamLib
{
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class IPAddrPort : Serializable<IPAddrPort>
    {
        public uint IPAddress;
        public ushort Port;



        public static implicit operator IPEndPoint( IPAddrPort addr )
        {
            return addr.ToEndPoint();
        }


        public IPAddress ToIPAddress()
        {
            return NetHelpers.GetIPAddress( IPAddress );
        }

        public IPEndPoint ToEndPoint()
        {
            return new IPEndPoint( ToIPAddress(), Port );
        }


        public override string ToString()
        {
            return ToEndPoint().ToString();
        }
    }
}
