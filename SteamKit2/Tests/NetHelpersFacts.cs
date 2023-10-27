using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class NetHelpersFacts
    {
        [TestMethod]
        public void GetMsgIPAddress()
        {
            Assert.AreEqual( 2130706433u, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).v4 );
            Assert.IsTrue( NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).v6.SequenceEqual( new byte[] {
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 1
            } ) );
        }

        [TestMethod]
        public void GetIPAddressFromMsg()
        {
            Assert.AreEqual( IPAddress.Loopback, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).GetIPAddress() );
            Assert.AreEqual( IPAddress.IPv6Loopback, NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).GetIPAddress() );
        }

        [TestMethod]
        public void GetIPAddress()
        {
            Assert.AreEqual( IPAddress.Loopback, NetHelpers.GetIPAddress( 2130706433 ) );
            Assert.AreEqual( 2130706433u, NetHelpers.GetIPAddressAsUInt( IPAddress.Loopback ) );
        }

        [TestMethod]
        public void ObfuscatePrivateIP()
        {
            Assert.AreEqual( 3316510732u, NetHelpers.GetMsgIPAddress( IPAddress.Loopback ).ObfuscatePrivateIP().v4 );
            Assert.IsTrue( NetHelpers.GetMsgIPAddress( IPAddress.IPv6Loopback ).ObfuscatePrivateIP().v6.SequenceEqual( new byte[] {
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 0xBA,
                0x0D, 0xF0, 0xAD, 1 ^ 0xBA
            } ) );
        }

        [TestMethod]
        public void TryParseIPEndPoint()
        {
            Assert.IsTrue( NetHelpers.TryParseIPEndPoint( "127.0.0.1:1337", out var parsedIp ) );
            Assert.AreEqual( new IPEndPoint( IPAddress.Loopback, 1337 ), parsedIp );

            Assert.IsTrue( NetHelpers.TryParseIPEndPoint( "[::1]:1337", out var parsedIpv6 ) );
            Assert.AreEqual( new IPEndPoint( IPAddress.IPv6Loopback, 1337 ), parsedIpv6 );
        }
    }
}
