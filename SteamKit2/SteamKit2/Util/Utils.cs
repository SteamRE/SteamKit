/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;

namespace SteamKit2
{
    static class Utils
    {
        public static DateTime DateTimeFromUnixTime( uint unixTime )
        {
            DateTime origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return origin.AddSeconds( unixTime );
        }
        public static uint DateTimeToUnixTime( DateTime time )
        {
            DateTime origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return ( uint )( time - origin ).TotalSeconds;
        }

        public static string EncodeHexString(byte[] input)
        {
            return input.Aggregate(new StringBuilder(),
                       (sb, v) => sb.Append(v.ToString("x2"))
                      ).ToString();
        }

        public static byte[] DecodeHexString(string hex)
        {
            if (hex == null)
                return null;

            int chars = hex.Length;
            byte[] bytes = new byte[chars / 2];

            for (int i = 0; i < chars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        public static EOSType GetOSType()
        {
            var osVer = Environment.OSVersion;
            var ver = osVer.Version;

            switch ( osVer.Platform )
            {
                case PlatformID.Win32Windows:
                    {
                        switch ( ver.Minor )
                        {
                            case 0:
                                return EOSType.Win95;

                            case 10:
                                return EOSType.Win98;

                            case 90:
                                return EOSType.WinME;

                            default:
                                return EOSType.WinUnknown;
                        }
                    }

                case PlatformID.Win32NT:
                    {
                        switch ( ver.Major )
                        {
                            case 4:
                                return EOSType.WinNT;

                            case 5: // XP family
                                if ( ver.Minor == 0 )
                                    return EOSType.Win200;

                                if ( ver.Minor == 1 || ver.Minor == 2 )
                                    return EOSType.WinXP;

                                goto default;

                            case 6: // Vista & 7
                                if ( ver.Minor == 0 )
                                    return EOSType.WinVista;

                                if ( ver.Minor == 1 )
                                    return EOSType.Windows7;

                                goto default;

                            default:
                                return EOSType.WinUnknown;
                        }
                    }

                case PlatformID.Unix:
                    return EOSType.LinuxUnknown; // this _could_ be mac, but we're gonna just go with linux for now

                default:
                    return EOSType.Unknown;
            }
        }


        public static byte[] GenerateMachineID()
        {
            // this is steamkit's own implementation, it doesn't match what steamclient does
            // but this should make it so individual systems can be identified

            PlatformID platform = Environment.OSVersion.Platform;

            if ( platform == PlatformID.Win32NT )
            {
                string hwString = "foobar";

                try
                {
                    RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                    localKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                    if (localKey != null)
                    {
                        hwString = localKey.GetValue("MachineGuid").ToString();
                    }
                }
                catch { }

                try
                {
                    return CryptoHelper.SHAHash( Encoding.ASCII.GetBytes( hwString.ToString() ) );
                }
                catch { return null; }
            }
            else
            {
                // todo: implement me!
                return null;
            }
        }

        public static T[] GetAttributes<T>( this Type type, bool inherit = false )
            where T : Attribute
        {
            return type.GetCustomAttributes( typeof( T ), inherit ) as T[];
        }
    }

    /// <summary>
    /// Contains various utility functions for handling EMsgs.
    /// </summary>
    public static class MsgUtil
    {
        private const uint ProtoMask = 0x80000000;
        private const uint EMsgMask = ~ProtoMask;

        /// <summary>
        /// Strips off the protobuf message flag and returns an EMsg.
        /// </summary>
        /// <param name="msg">The message number.</param>
        /// <returns>The underlying EMsg.</returns>
        public static EMsg GetMsg( uint msg )
        {
            return ( EMsg )( msg & EMsgMask );
        }

        /// <summary>
        /// Strips off the protobuf message flag and returns an EMsg.
        /// </summary>
        /// <param name="msg">The message number.</param>
        /// <returns>The underlying EMsg.</returns>
        public static uint GetGCMsg( uint msg )
        {
            return ( msg & EMsgMask );
        }

        /// <summary>
        /// Strips off the protobuf message flag and returns an EMsg.
        /// </summary>
        /// <param name="msg">The message number.</param>
        /// <returns>The underlying EMsg.</returns>
        public static EMsg GetMsg( EMsg msg )
        {
            return GetMsg( ( uint )msg );
        }

        /// <summary>
        /// Determines whether message is protobuf flagged.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>
        ///   <c>true</c> if this message is protobuf flagged; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProtoBuf( uint msg )
        {
            return ( msg & ProtoMask ) > 0;
        }

        /// <summary>
        /// Determines whether message is protobuf flagged.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>
        ///   <c>true</c> if this message is protobuf flagged; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProtoBuf( EMsg msg )
        {
            return IsProtoBuf( ( uint )msg );
        }

        /// <summary>
        /// Crafts an EMsg, flagging it if required.
        /// </summary>
        /// <param name="msg">The EMsg to flag.</param>
        /// <param name="protobuf">if set to <c>true</c>, the message is protobuf flagged.</param>
        /// <returns>A crafted EMsg, flagged if requested.</returns>
        public static EMsg MakeMsg( EMsg msg, bool protobuf = false )
        {
            if ( protobuf )
                return ( EMsg )( ( uint )msg | ProtoMask );

            return msg;
        }
        /// <summary>
        /// Crafts an EMsg, flagging it if required.
        /// </summary>
        /// <param name="msg">The EMsg to flag.</param>
        /// <param name="protobuf">if set to <c>true</c>, the message is protobuf flagged.</param>
        /// <returns>A crafted EMsg, flagged if requested.</returns>
        public static uint MakeGCMsg( uint msg, bool protobuf = false )
        {
            if ( protobuf )
                return msg | ProtoMask;

            return msg;
        }
    }

    static class WebHelpers
    {
        static bool IsUrlSafeChar( char ch )
        {
            if ( ( ( ( ch >= 'a' ) && ( ch <= 'z' ) ) || ( ( ch >= 'A' ) && ( ch <= 'Z' ) ) ) || ( ( ch >= '0' ) && ( ch <= '9' ) ) )
            {
                return true;
            }

            switch ( ch )
            {
                case '-':
                case '.':
                case '_':
                    return true;
            }

            return false;
        }

        public static string UrlEncode( string input )
        {
            return UrlEncode( Encoding.UTF8.GetBytes( input ) );
        }


        public static string UrlEncode( byte[] input )
        {
            StringBuilder encoded = new StringBuilder( input.Length * 2 );

            for ( int i = 0 ; i < input.Length ; i++ )
            {
                char inch = ( char )input[ i ];

                if ( IsUrlSafeChar( inch ) )
                {
                    encoded.Append( inch );
                }
                else if ( inch == ' ' )
                {
                    encoded.Append( '+' );
                }
                else
                {
                    encoded.AppendFormat( "%{0:X2}", input[ i ] );
                }
            }

            return encoded.ToString();
        }
    }

    static class NetHelpers
    {
        public static IPAddress GetLocalIP(Socket activeSocket)
        {
            IPEndPoint ipEndPoint = activeSocket.LocalEndPoint as IPEndPoint;

            if ( ipEndPoint == null || ipEndPoint.Address == IPAddress.Any )
                throw new Exception( "Socket not connected" );

            return ipEndPoint.Address;
        }

        public static IPAddress GetIPAddress( uint ipAddr )
        {
            byte[] addrBytes = BitConverter.GetBytes( ipAddr );
            Array.Reverse( addrBytes );

            return new IPAddress( addrBytes );
        }
        public static uint GetIPAddress( IPAddress ipAddr )
        {
            byte[] addrBytes = ipAddr.GetAddressBytes();
            Array.Reverse( addrBytes );

            return BitConverter.ToUInt32( addrBytes, 0 );
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

    class TimeoutableWebClient : WebClient
    {
        public int Timeout { get; set; }


        public TimeoutableWebClient()
        {
            // msdn docs state that the default timeout of a HttpWebRequest is 100,000 milliseconds (100 sec)
            Timeout = 100000;
        }


        protected override WebRequest GetWebRequest( Uri address )
        {
            var webReq = base.GetWebRequest( address );

            if ( webReq != null )
                webReq.Timeout = Timeout;

            return webReq;
        }
    }
}
