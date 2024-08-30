/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using SteamKit2.Internal;

namespace SteamKit2
{
    static class NetHelpers
    {
        public static IPAddress GetLocalIP( Socket activeSocket )
        {
            var ipEndPoint = activeSocket.LocalEndPoint as IPEndPoint;

            if ( ipEndPoint == null || ipEndPoint.Address == IPAddress.Any )
                throw new InvalidOperationException( "Socket not connected" );

            return ipEndPoint.Address;
        }

        public static IPAddress GetIPAddress( uint ipAddr )
        {
            return new IPAddress(
                ( ( ipAddr & 0xFF000000 ) >> 24 ) |
                ( ( ipAddr & 0x00FF0000 ) >> 8 ) |
                ( ( ipAddr & 0x0000FF00 ) << 8 ) |
                ( ( ipAddr & 0x000000FF ) << 24 )
            );
        }

        public static uint GetIPAddressAsUInt( IPAddress ipAddr )
        {
            DebugLog.Assert( ipAddr.AddressFamily == AddressFamily.InterNetwork, nameof( NetHelpers ), "GetIPAddressAsUInt only works with IPv4 addresses." );

            Span<byte> addrBytes = stackalloc byte[ 4 ];
            ipAddr.TryWriteBytes( addrBytes, out _ );

            return Unsafe.BitCast<int, uint>( IPAddress.NetworkToHostOrder( BitConverter.ToInt32( addrBytes ) ) );
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

            if ( ipAddr.AddressFamily == AddressFamily.InterNetworkV6 )
            {
                msgIpAddress.v6 = ipAddr.GetAddressBytes();
            }
            else
            {
                msgIpAddress.v4 = GetIPAddressAsUInt( ipAddr );
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
            if ( !IPEndPoint.TryParse( stringValue, out endPoint ) )
            {
                return false;
            }

            if ( endPoint.Port == 0 )
            {
                endPoint = null;
                return false;
            }

            return true;
        }

        public static string ExtractEndpointHost( EndPoint endPoint )
        {
            return endPoint switch
            {
                IPEndPoint ipep => ipep.Address.ToString(),
                DnsEndPoint dns => dns.Host,
                _ => throw new InvalidOperationException( "Unknown endpoint type." ),
            };
        }

        public static int ExtractEndpointPort( EndPoint endPoint )
        {
            return endPoint switch
            {
                IPEndPoint ipep => ipep.Port,
                DnsEndPoint dns => dns.Port,
                _ => throw new InvalidOperationException( "Unknown endpoint type." ),
            };
        }
    }
}
