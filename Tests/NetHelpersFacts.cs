using System;
using System.Net;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class NetHelpersFacts
    {
        [Fact]
        public void GetMsgIPAddress()
        {
            Assert.Equal( 2130706433u, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).v4 );
            Assert.Equal( new byte[] {
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 1
            }, NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).v6 );
        }

        [Fact]
        public void GetIPAddressFromMsg()
        {
            Assert.Equal( IPAddress.Loopback, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).GetIPAddress() );
            Assert.Equal( IPAddress.IPv6Loopback, NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).GetIPAddress() );
        }

        [Fact]
        public void GetIPAddress()
        {
            Assert.Equal( IPAddress.Loopback, NetHelpers.GetIPAddress( 2130706433 ) );
            Assert.Equal( 2130706433u, NetHelpers.GetIPAddressAsUInt( IPAddress.Loopback ) );
        }

        [Fact]
        public void ObfuscatePrivateIP()
        {
            Assert.Equal( 3316510732u, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).ObfuscatePrivateIP().v4 );
            Assert.Equal( new byte[] {
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 1 ^ 0xBA
            }, NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).ObfuscatePrivateIP().v6 );
        }

        [Fact]
        public void TryParseIPEndPoint()
        {
            Assert.True( NetHelpers.TryParseIPEndPoint( "127.0.0.1:1337", out var parsedIp ) );
            Assert.Equal( new IPEndPoint( IPAddress.Loopback, 1337 ), parsedIp );

            Assert.True( NetHelpers.TryParseIPEndPoint( "[::1]:1337", out var parsedIpv6 ) );
            Assert.Equal( new IPEndPoint( IPAddress.IPv6Loopback, 1337 ), parsedIpv6 );
        }
    }
}
