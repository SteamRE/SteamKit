using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace SteamKit
{
    public static class NetHelpers
    {
        public static IPAddress GetLocalIP()
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

        public static IPAddress GetIPAddress( uint ipAddr )
        {
            return new IPAddress( BitConverter.GetBytes( ipAddr ) );
        }
        public static uint GetIPAddress( IPAddress ipAddr )
        {
            return EndianSwap( BitConverter.ToUInt32( ipAddr.GetAddressBytes(), 0 ) );
        }


        public static uint EndianSwap( uint input ) 
        {
            return ( uint )IPAddress.NetworkToHostOrder( ( int )input );
        }
        public static ulong EndianSwap( ulong input )
        {
            return ( ulong )IPAddress.NetworkToHostOrder( ( long )input );
        }
        public static ushort EndianSwap( ushort input )
        {
            return ( ushort )IPAddress.NetworkToHostOrder( ( short )input );
        }
    }
}
