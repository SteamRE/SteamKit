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
            return Convert.ToHexString(input).ToLower();
        }

        [return: NotNullIfNotNull( nameof( hex ) )]
        public static byte[]? DecodeHexString(string? hex)
        {
            if (hex == null)
                return null;

            return Convert.FromHexString( hex );
        }

        public static EOSType GetOSType()
        {
            var osVer = Environment.OSVersion;
            var ver = osVer.Version;

            return osVer.Platform switch
            {
                PlatformID.Win32Windows => ver.Minor switch
                {
                    0 => EOSType.Win95,
                    10 => EOSType.Win98,
                    90 => EOSType.WinME,
                    _ => EOSType.WinUnknown,
                },

                PlatformID.Win32NT => ver.Major switch
                {
                    4 => EOSType.WinNT,
                    5 => ver.Minor switch
                    {
                        0 => EOSType.Win2000,
                        1 => EOSType.WinXP,
                        // Assume nobody runs Windows XP Professional x64 Edition
                        // It's an edition of Windows Server 2003 anyway.
                        2 => EOSType.Win2003,
                        _ => EOSType.WinUnknown,
                    },
                    6 => ver.Minor switch
                    {
                        0 => EOSType.WinVista, // Also Server 2008
                        1 => EOSType.Windows7, // Also Server 2008 R2
                        2 => EOSType.Windows8, // Also Server 2012
                        // Note: The OSVersion property reports the same version number (6.2.0.0) for both Windows 8 and Windows 8.1.- http://msdn.microsoft.com/en-us/library/system.environment.osversion(v=vs.110).aspx
                        // In practice, this will only get hit if the application targets Windows 8.1 in the app manifest.
                        // See http://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx for more info.
                        3 => EOSType.Windows81, // Also Server 2012 R2
                        _ => EOSType.WinUnknown,
                    },
                    10 when ver.Build >= 22000 => EOSType.Win11,
                    10 => EOSType.Windows10,// Also Server 2016, Server 2019, Server 2022
                    _ => EOSType.WinUnknown,
                },

                // The specific minor versions only exist in Valve's enum for LTS versions
                PlatformID.Unix when RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) => ver.Major switch
                {
                    2 => ver.Minor switch
                    {
                        2 => EOSType.Linux22,
                        4 => EOSType.Linux24,
                        6 => EOSType.Linux26,
                        _ => EOSType.LinuxUnknown,
                    },
                    3 => ver.Minor switch
                    {
                        2 => EOSType.Linux32,
                        5 => EOSType.Linux35,
                        6 => EOSType.Linux36,
                        10 => EOSType.Linux310,
                        16 => EOSType.Linux316,
                        18 => EOSType.Linux318,
                        _ => EOSType.Linux3x,
                    },
                    4 => ver.Minor switch
                    {
                        1 => EOSType.Linux41,
                        4 => EOSType.Linux44,
                        9 => EOSType.Linux49,
                        14 => EOSType.Linux414,
                        19 => EOSType.Linux419,
                        _ => EOSType.Linux4x,
                    },
                    5 => ver.Minor switch
                    {
                        4 => EOSType.Linux54,
                        10 => EOSType.Linux510,
                        _ => EOSType.Linux5x,
                    },
                    6 => EOSType.Linux6x,
                    7 => EOSType.Linux7x,
                    _ => EOSType.LinuxUnknown,
                },

                PlatformID.Unix when RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) => ver.Major switch
                {
                    11 => EOSType.MacOS107, // "Lion"
                    12 => EOSType.MacOS108, // "Mountain Lion"
                    13 => EOSType.MacOS109, // "Mavericks"
                    14 => EOSType.MacOS1010, // "Yosemite"
                    15 => EOSType.MacOS1011, // El Capitan
                    16 => EOSType.MacOS1012, // Sierra
                    17 => EOSType.Macos1013, // High Sierra
                    18 => EOSType.Macos1014, // Mojave
                    19 => EOSType.Macos1015, // Catalina
                    20 => EOSType.MacOS11, // Big Sur
                    21 => EOSType.MacOS12, // Monterey
                    22 => EOSType.MacOS13, // Ventura
                    23 => EOSType.MacOS14, // Sonoma
                    _ => EOSType.MacOSUnknown,
                },

                _ => EOSType.Unknown,
            };
        }

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
            return DateTimeOffset.FromUnixTimeSeconds( (long)unixTime ).DateTime;
        }
        /// <summary>
        /// Converts a given DateTime into a unix timestamp representing seconds since the unix epoch.
        /// </summary>
        /// <param name="time">DateTime to be expressed</param>
        /// <returns>64-bit wide representation</returns>
        public static ulong DateTimeToUnixTime(DateTime time)
        {
            return (ulong)new DateTimeOffset( time ).ToUnixTimeSeconds();
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

            return ch switch
            {
                '-' or '.' or '_' => true,
                _ => false,
            };
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

            if ( !IPAddress.TryParse( stringValue.AsSpan( 0, colonPosition ), out var address ) )
            {
                endPoint = null;
                return false;
            }

            if ( !ushort.TryParse( stringValue.AsSpan( colonPosition + 1 ), out var port ) )
            {
                endPoint = null;
                return false;
            }

            endPoint = new IPEndPoint( address, port );
            return true;
        }

        public static (string host, int port) ExtractEndpointHost( EndPoint endPoint )
        {
            return endPoint switch
            {
                IPEndPoint ipep => (ipep.Address.ToString(), ipep.Port),
                DnsEndPoint dns => (dns.Host, dns.Port),
                _ => throw new InvalidOperationException( "Unknown endpoint type." ),
            };
        }
    }
}
