using System;
using System.Net;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class WebSocketUriFacts
    {
        [Fact]
        public void DnsEndPoint()
        {
            var endpoint = new DnsEndPoint( "example.com", 1337 );
            Assert.Equal( "wss://example.com:1337/cmsocket/", WebSocketConnection.WebSocketContext.ConstructUri( endpoint ).ToString() );
        }

        [Fact]
        public void IPEndPointV4()
        {
            var endpoint = new IPEndPoint( IPAddress.Loopback, 1337 );
            Assert.Equal( "wss://127.0.0.1:1337/cmsocket/", WebSocketConnection.WebSocketContext.ConstructUri( endpoint ).ToString() );
        }

        [Fact]
        public void IPEndPointV6()
        {
            var endpoint = new IPEndPoint( IPAddress.IPv6Loopback, 1337 );
            Assert.Equal( "wss://[::1]:1337/cmsocket/", WebSocketConnection.WebSocketContext.ConstructUri( endpoint ).ToString() );
        }

        [Fact]
        public void ThrowsWrongEndPoint()
        {
            Assert.Throws<InvalidOperationException>( () => WebSocketConnection.WebSocketContext.ConstructUri( new DummyEndPoint() ) );
        }

        class DummyEndPoint : EndPoint
        {
        }
    }
}
