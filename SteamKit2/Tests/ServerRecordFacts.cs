using SteamKit2;
using SteamKit2.Discovery;
using Xunit;

namespace Tests
{
    public class ServerRecordFacts
    {
        [Fact]
        public void NullsAreEqualOperator()
        {
            ServerRecord l = null;
            ServerRecord r = null;

            Assert.True(l == r);
        }

        [Fact]
        public void NullIsNotEqual()
        {
            var s = ServerRecord.CreateWebSocketServer("host:1");

            Assert.True(s != null);
            Assert.True(null != s);
            Assert.False(s.Equals(null));
            Assert.False(s == null);
            Assert.False(null == s);
        }

        [Fact]
        public void DifferentProtocolsAreNotEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 1, ProtocolTypes.WebSocket);

            Assert.True(l != r);
            Assert.True(r != l);
            Assert.False(l == r);
            Assert.False(r == l);
            Assert.False(l.Equals(r));
            Assert.False(r.Equals(l));
        }

        [Fact]
        public void DifferentEndPointsAreNotEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 2, ProtocolTypes.Tcp);

            Assert.True(l != r);
            Assert.True(r != l);
            Assert.False(l == r);
            Assert.False(r == l);
            Assert.False(l.Equals(r));
            Assert.False(r.Equals(l));
        }

        [Fact]
        public void DifferentEndPointsAndProtocolsAreNotEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 2, ProtocolTypes.WebSocket);

            Assert.True(l != r);
            Assert.True(r != l);
            Assert.False(l == r);
            Assert.False(r == l);
            Assert.False(l.Equals(r));
            Assert.False(r.Equals(l));
        }

        [Fact]
        public void SameEndPointsAndProtocolsAreEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);

            Assert.False(l != r);
            Assert.False(r != l);
            Assert.True(l == r);
            Assert.True(r == l);
            Assert.True(l.Equals(r));
            Assert.True(r.Equals(l));

            Assert.Equal(l.GetHashCode(), r.GetHashCode());
        }
    }
}
