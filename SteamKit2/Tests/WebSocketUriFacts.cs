using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class WebSocketUriFacts
    {
        [TestMethod]
        public void DnsEndPoint()
        {
            var endpoint = new DnsEndPoint( "example.com", 1337 );
            Assert.AreEqual( "wss://example.com:1337/cmsocket/", WebSocketConnection.WebSocketContext.ConstructUri( endpoint ).ToString() );
        }

        [TestMethod]
        public void IPEndPointV4()
        {
            var endpoint = new IPEndPoint( IPAddress.Loopback, 1337 );
            Assert.AreEqual( "wss://127.0.0.1:1337/cmsocket/", WebSocketConnection.WebSocketContext.ConstructUri( endpoint ).ToString() );
        }

        [TestMethod]
        public void IPEndPointV6()
        {
            var endpoint = new IPEndPoint( IPAddress.IPv6Loopback, 1337 );
            Assert.AreEqual( "wss://[::1]:1337/cmsocket/", WebSocketConnection.WebSocketContext.ConstructUri( endpoint ).ToString() );
        }

        [TestMethod]
        public void ThrowsWrongEndPoint()
        {
            Assert.ThrowsException<InvalidOperationException>( () => WebSocketConnection.WebSocketContext.ConstructUri( new DummyEndPoint() ) );
        }

        class DummyEndPoint : EndPoint
        {
        }
    }
}
