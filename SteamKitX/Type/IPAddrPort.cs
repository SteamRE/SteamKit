using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace SteamKit
{
    public class IPAddrPort
    {
        public static readonly int Size = 6;

        public uint IPAddress;
        public ushort Port;

        public IPAddrPort(uint ipaddr, ushort port)
        {
            IPAddress = ipaddr;
            Port = port;
        }

        public static implicit operator IPEndPoint(IPAddrPort addr)
        {
            return addr.ToEndPoint();
        }


        public IPAddress ToIPAddress()
        {
            return NetHelper.GetIPAddress(IPAddress);
        }

        public IPEndPoint ToEndPoint()
        {
            return new IPEndPoint(ToIPAddress(), Port);
        }


        public override string ToString()
        {
            return ToEndPoint().ToString();
        }

        public static IPAddrPort Deserialize(byte[] buffer)
        {
            return new IPAddrPort(
                                BitConverter.ToUInt32(buffer, 0),
                                BitConverter.ToUInt16(buffer, 4)
                                );
        }
    }
}
