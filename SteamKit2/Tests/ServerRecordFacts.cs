using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using SteamKit2.Discovery;

namespace Tests
{
    [TestClass]
    public class ServerRecordFacts
    {
        [TestMethod]
        public void NullsAreEqualOperator()
        {
            ServerRecord l = null;
            ServerRecord r = null;

            Assert.IsTrue(l == r);
        }

        [TestMethod]
        public void NullIsNotEqual()
        {
            var s = ServerRecord.CreateWebSocketServer("host:1");

            Assert.IsTrue(s != null);
            Assert.IsTrue(null != s);
            Assert.IsFalse(s.Equals(null));
            Assert.IsFalse(s == null);
            Assert.IsFalse(null == s);
        }

        [TestMethod]
        public void DifferentProtocolsAreNotEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 1, ProtocolTypes.WebSocket);

            Assert.IsTrue(l != r);
            Assert.IsTrue(r != l);
            Assert.IsFalse(l == r);
            Assert.IsFalse(r == l);
            Assert.IsFalse(l.Equals(r));
            Assert.IsFalse(r.Equals(l));
        }

        [TestMethod]
        public void DifferentEndPointsAreNotEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 2, ProtocolTypes.Tcp);

            Assert.IsTrue(l != r);
            Assert.IsTrue(r != l);
            Assert.IsFalse(l == r);
            Assert.IsFalse(r == l);
            Assert.IsFalse(l.Equals(r));
            Assert.IsFalse(r.Equals(l));
        }

        [TestMethod]
        public void DifferentEndPointsAndProtocolsAreNotEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 2, ProtocolTypes.WebSocket);

            Assert.IsTrue(l != r);
            Assert.IsTrue(r != l);
            Assert.IsFalse(l == r);
            Assert.IsFalse(r == l);
            Assert.IsFalse(l.Equals(r));
            Assert.IsFalse(r.Equals(l));
        }

        [TestMethod]
        public void SameEndPointsAndProtocolsAreEqual()
        {
            var l = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);
            var r = ServerRecord.CreateServer("host", 1, ProtocolTypes.Tcp);

            Assert.IsFalse(l != r);
            Assert.IsFalse(r != l);
            Assert.IsTrue(l == r);
            Assert.IsTrue(r == l);
            Assert.IsTrue(l.Equals(r));
            Assert.IsTrue(r.Equals(l));

            Assert.AreEqual(l.GetHashCode(), r.GetHashCode());
        }

        [TestMethod]
        public void CanTryCreateSocketServer()
        {
            Assert.IsTrue(ServerRecord.TryCreateSocketServer("127.0.0.1:1234", out var record));
            Assert.IsNotNull(record);
            Assert.AreEqual(new IPEndPoint(IPAddress.Loopback, 1234), record.EndPoint);
            Assert.AreEqual(ProtocolTypes.Tcp | ProtocolTypes.Udp, record.ProtocolTypes);

            Assert.IsTrue(ServerRecord.TryCreateSocketServer("192.168.0.1:5678", out record));
            Assert.IsNotNull(record);
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("192.168.0.1"), 5678), record.EndPoint);
            Assert.AreEqual(ProtocolTypes.Tcp | ProtocolTypes.Udp, record.ProtocolTypes);
        }

        [TestMethod]
        public void CannotTryCreateSocketServer()
        {
            Assert.IsFalse(ServerRecord.TryCreateSocketServer("127.0.0.1", out var record));
            Assert.IsNull(record);

            Assert.IsFalse(ServerRecord.TryCreateSocketServer("127.0.0.1:123456789", out record));
            Assert.IsNull(record);

            Assert.IsFalse(ServerRecord.TryCreateSocketServer("127.0.0.1:-1234", out record));
            Assert.IsNull(record);

            Assert.IsFalse(ServerRecord.TryCreateSocketServer("127.0.0.1:notanint", out record));
            Assert.IsNull(record);

            Assert.IsFalse(ServerRecord.TryCreateSocketServer("volvopls.valvesoftware.com:1234", out record));
            Assert.IsNull(record);
        }
    }
}
