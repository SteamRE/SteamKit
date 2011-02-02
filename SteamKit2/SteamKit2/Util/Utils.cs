using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace SteamKit2
{
    public static class Utils
    {
        public static DateTime DateTimeFromUnixTime( uint unixTime )
        {
            DateTime origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return origin.AddSeconds( unixTime );
        }
    }

    public static class MsgUtil
    {
        private static readonly uint ProtoMask = 0x80000000;
        private static readonly uint EMsgMask = ~ProtoMask;

        public static EMsg GetMsg( uint integer )
        {
            return ( EMsg )( integer & EMsgMask );
        }

        public static EMsg GetMsg( EMsg msg )
        {
            return GetMsg( ( uint )msg );
        }

        public static bool IsProtoBuf( uint integer )
        {
            return ( integer & ProtoMask ) > 0;
        }

        public static bool IsProtoBuf( EMsg msg )
        {
            return IsProtoBuf( ( uint )msg );
        }

        public static EMsg MakeMsg( EMsg msg )
        {
            return msg;
        }

        public static EMsg MakeMsg( EMsg msg, bool protobuf )
        {
            if ( protobuf )
                return ( EMsg )( ( uint )msg | ProtoMask );

            return msg;
        }
    }

    static class NetHelpers
    {
        public static IPAddress GetLocalIP()
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
            foreach ( NetworkInterface nw in networks )
            {
                if ( nw.Description.IndexOf( "Virtual", 0, StringComparison.OrdinalIgnoreCase ) > 0 )
                    continue;

                IPInterfaceProperties ipProps = nw.GetIPProperties();
                foreach ( UnicastIPAddressInformation ucip in ipProps.UnicastAddresses )
                {
                    if ( ucip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ucip.IsDnsEligible )
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
