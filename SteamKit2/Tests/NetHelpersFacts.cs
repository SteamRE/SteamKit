using System.Net;
using SteamKit2;
using Xunit;

namespace Tests
{
#if DEBUG
    public class NetHelpersFacts
    {
        [Fact]
        public void GetMsgIPAddress()
        {
            Assert.Equal( 2130706433u, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).v4 );
            Assert.Equal( [
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 1
            ], NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).v6 );
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
            Assert.Equal( IPAddress.Parse( "0.0.0.1" ), NetHelpers.GetIPAddress( 1 ) );
            Assert.Equal( IPAddress.Parse( "255.255.255.255" ), NetHelpers.GetIPAddress( uint.MaxValue ) );
            Assert.Equal( IPAddress.Any, NetHelpers.GetIPAddress( 0 ) );
        }

        [Fact]
        public void GetIPAddressAsUInt()
        {
            Assert.Equal( 2130706433u, NetHelpers.GetIPAddressAsUInt( IPAddress.Loopback ) );
            Assert.Equal( 1u, NetHelpers.GetIPAddressAsUInt( IPAddress.Parse( "0.0.0.1" ) ) );
            Assert.Equal( uint.MaxValue, NetHelpers.GetIPAddressAsUInt( IPAddress.Parse( "255.255.255.255" ) ) );
            Assert.Equal( 3232235521u, NetHelpers.GetIPAddressAsUInt( IPAddress.Parse( "192.168.0.1" ) ) );
            Assert.Equal( 167772161u, NetHelpers.GetIPAddressAsUInt( IPAddress.Parse( "10.0.0.1" ) ) );
            Assert.Equal( 2886729729u, NetHelpers.GetIPAddressAsUInt( IPAddress.Parse( "172.16.0.1" ) ) );
        }

        [Fact]
        public void ObfuscatePrivateIP()
        {
            Assert.Equal( 3316510732u, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).ObfuscatePrivateIP().v4 );
            Assert.Equal( [
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 1 ^ 0xBA
            ], NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).ObfuscatePrivateIP().v6 );
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
#endif
}
