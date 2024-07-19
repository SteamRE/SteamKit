/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using SteamKit2.Internal;

namespace SteamKit2
{
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
