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


        public IPAddress ToIPAddress()
        {
            return new IPAddress( BitConverter.GetBytes( IPAddress ) );
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
