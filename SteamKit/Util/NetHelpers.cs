using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SteamKit
{
    static class NetHelpers
    {
        public static IPAddress GetLocalIP()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry( hostName );

            foreach ( var ipAddr in hostEntry.AddressList )
            {
                if ( ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                    return ipAddr;
            }

            return null;
        }

        public static IPAddress GetIPAddress( uint ipAddr )
        {
            return new IPAddress( BitConverter.GetBytes( ipAddr ) );
        }
        public static uint GetIPAddress( IPAddress ipAddr )
        {
            return BitConverter.ToUInt32( ipAddr.GetAddressBytes(), 0 );
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
