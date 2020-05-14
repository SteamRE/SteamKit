/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    static class Utils
    {
        public static string EncodeHexString(byte[] input)
        {
            return input.Aggregate(new StringBuilder(),
                       (sb, v) => sb.Append(v.ToString("x2"))
                      ).ToString();
        }

        [return: NotNullIfNotNull("hex")]
        public static byte[]? DecodeHexString(string? hex)
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

                            case 5:
                                switch ( ver.Minor )
                                {
                                    case 0:
                                        return EOSType.Win2000;

                                    case 1:
                                        return EOSType.WinXP;

                                    case 2:
                                        // Assume nobody runs Windows XP Professional x64 Edition
                                        // It's an edition of Windows Server 2003 anyway.
                                        return EOSType.Win2003;
                                }

                                goto default;

                            case 6:
                                switch ( ver.Minor )
                                {
                                    case 0:
                                        return EOSType.WinVista; // Also Server 2008

                                    case 1:
                                        return EOSType.Windows7; // Also Server 2008 R2

                                    case 2:
                                        return EOSType.Windows8; // Also Server 2012

                                    // Note: The OSVersion property reports the same version number (6.2.0.0) for both Windows 8 and Windows 8.1.- http://msdn.microsoft.com/en-us/library/system.environment.osversion(v=vs.110).aspx
                                    // In practice, this will only get hit if the application targets Windows 8.1 in the app manifest.
                                    // See http://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx for more info.
                                    case 3:
                                        return EOSType.Windows81; // Also Server 2012 R2
                                }

                                goto default;

                            case 10:
                                return EOSType.Windows10;

                            default:
                                return EOSType.WinUnknown;
                        }
                    }

                case PlatformID.Unix:
                    {
                        if ( IsMacOS() )
                        {
                            switch ( ver.Major )
                            {
                                case 11:
                                    return EOSType.MacOS107; // "Lion"

                                case 12:
                                    return EOSType.MacOS108; // "Mountain Lion"

                                case 13:
                                    return EOSType.MacOS109; // "Mavericks"

                                case 14:
                                   return EOSType.MacOS1010; // "Yosemite"

                                case 15:
                                    return EOSType.MacOS1011; // El Capitan

                                case 16:
                                    return EOSType.MacOS1012; // Sierra

                                default:
                                    return EOSType.MacOSUnknown;
                            }
                        }
                        else
                        {
                            return EOSType.LinuxUnknown;
                        }
                    }

                default:
                    return EOSType.Unknown;
            }
        }

        public static bool IsMacOS()
            => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static T[] GetAttributes<T>( this Type type, bool inherit = false )
            where T : Attribute
        {
            return (T[])type.GetTypeInfo().GetCustomAttributes( typeof( T ), inherit );
        }
    }

    /// <summary>
    /// Contains various utility functions for dealing with dates.
    /// </summary>
    public static class DateUtils
    {
        /// <summary>
        /// Converts a given unix timestamp to a DateTime
        /// </summary>
        /// <param name="unixTime">A unix timestamp expressed as seconds since the unix epoch</param>
        /// <returns>DateTime representation</returns>
        public static DateTime DateTimeFromUnixTime(ulong unixTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(unixTime);
        }
        /// <summary>
        /// Converts a given DateTime into a unix timestamp representing seconds since the unix epoch.
        /// </summary>
        /// <param name="time">DateTime to be expressed</param>
        /// <returns>64-bit wide representation</returns>
        public static ulong DateTimeToUnixTime(DateTime time)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (ulong)(time - origin).TotalSeconds;
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
            var ipEndPoint = activeSocket.LocalEndPoint as IPEndPoint;

            if ( ipEndPoint == null || ipEndPoint.Address == IPAddress.Any )
                throw new InvalidOperationException( "Socket not connected" );

            return ipEndPoint.Address;
        }

        public static IPAddress GetIPAddress( uint ipAddr )
        {
            byte[] addrBytes = BitConverter.GetBytes( ipAddr );
            Array.Reverse( addrBytes );

            return new IPAddress( addrBytes );
        }

        public static uint GetIPAddressAsUInt( IPAddress ipAddr )
        {
            byte[] addrBytes = ipAddr.GetAddressBytes();
            Array.Reverse( addrBytes );

            return BitConverter.ToUInt32( addrBytes, 0 );
        }

        public static IPAddress GetIPAddress( this CMsgIPAddress ipAddr )
        {
            if ( ipAddr.ShouldSerializev6() )
            {
                return new IPAddress( ipAddr.v6 );
            }
            else
            {
                return GetIPAddress( ipAddr.v4 );
            }
        }

        public static CMsgIPAddress GetMsgIPAddress( IPAddress ipAddr )
        {
            var msgIpAddress = new CMsgIPAddress();
            byte[] addrBytes = ipAddr.GetAddressBytes();

            if ( ipAddr.AddressFamily == AddressFamily.InterNetworkV6 )
            {
                msgIpAddress.v6 = addrBytes;
            }
            else
            {
                Array.Reverse( addrBytes );

                msgIpAddress.v4 = BitConverter.ToUInt32( addrBytes, 0 );
            }

            return msgIpAddress;
        }

        public static CMsgIPAddress ObfuscatePrivateIP( this CMsgIPAddress msgIpAddress )
        {
            var localIp = msgIpAddress;

            if ( localIp.ShouldSerializev6() )
            {
                localIp.v6[ 0 ] ^= 0x0D;
                localIp.v6[ 1 ] ^= 0xF0;
                localIp.v6[ 2 ] ^= 0xAD;
                localIp.v6[ 3 ] ^= 0xBA;

                localIp.v6[ 4 ] ^= 0x0D;
                localIp.v6[ 5 ] ^= 0xF0;
                localIp.v6[ 6 ] ^= 0xAD;
                localIp.v6[ 7 ] ^= 0xBA;

                localIp.v6[ 8 ] ^= 0x0D;
                localIp.v6[ 9 ] ^= 0xF0;
                localIp.v6[ 10 ] ^= 0xAD;
                localIp.v6[ 11 ] ^= 0xBA;

                localIp.v6[ 12 ] ^= 0x0D;
                localIp.v6[ 13 ] ^= 0xF0;
                localIp.v6[ 14 ] ^= 0xAD;
                localIp.v6[ 15 ] ^= 0xBA;
            }
            else
            {
                localIp.v4 ^= MsgClientLogon.ObfuscationMask;
            }

            return localIp;
        }

        public static bool TryParseIPEndPoint( string stringValue, [NotNullWhen( true )] out IPEndPoint? endPoint )
        {
            var colonPosition = stringValue.LastIndexOf( ':' );

            if ( colonPosition == -1 )
            {
                endPoint = null;
                return false;
            }

            if ( !IPAddress.TryParse( stringValue.Substring( 0, colonPosition ), out var address ) )
            {
                endPoint = null;
                return false;
            }

            if ( !ushort.TryParse( stringValue.Substring( colonPosition + 1 ), out var port ) )
            {
                endPoint = null;
                return false;
            }

            endPoint = new IPEndPoint( address, port );
            return true;
        }
    }
}
