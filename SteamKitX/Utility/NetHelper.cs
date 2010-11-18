using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace SteamKit
{
    class NetHelper
    {
        // before we were trying all devices, but the proper solution is to get the localendpoint
        // after an i/o operation has been performed (connect/tcp send/udp)
        // but it doesn't seem to work
        public static IPAddress GetLocalIP(Socket socket)
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nw in networks)
            {
                if (nw.Description.IndexOf("Virtual", 0, StringComparison.OrdinalIgnoreCase) > 0)
                    continue;

                IPInterfaceProperties ipProps = nw.GetIPProperties();
                foreach (UnicastIPAddressInformation ucip in ipProps.UnicastAddresses)
                {
                    if (ucip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ucip.IsDnsEligible)
                       return ucip.Address;
                }
            }

            return null;
        }

        public static IPAddress GetIPAddress(uint ipAddr)
        {
            return new IPAddress(BitConverter.GetBytes(ipAddr));
        }
        public static uint GetIPAddress(IPAddress ipAddr)
        {
            return EndianSwap(BitConverter.ToUInt32(ipAddr.GetAddressBytes(), 0));
        }


        public static uint EndianSwap(uint input)
        {
            return (uint)IPAddress.NetworkToHostOrder((int)input);
        }
        public static ulong EndianSwap(ulong input)
        {
            return (ulong)IPAddress.NetworkToHostOrder((long)input);
        }
        public static ushort EndianSwap(ushort input)
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)input);
        }
    }
}
