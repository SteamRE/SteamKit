using System.Net;
using SteamKit2.Networking.Steam3;
using Xunit;

namespace Tests
{
    public class SmartCMServerListFacts
    {
        public SmartCMServerListFacts()
        {
            serverList = new SmartCMServerList();
        }

        readonly SmartCMServerList serverList;

        [Fact]
        public void TryAdd_ReturnsTrue_WhenAdded()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            var added = serverList.TryAdd(endPoint);
            Assert.True(added, "TryAdd should have added the IPEndPoint to the list.");

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal(1, addresses.Length);
            Assert.Equal(endPoint, addresses[0]);
        }

        [Fact]
        public void TryAdd_ReturnsFalse_WhenEqualEndPointAlreadyAdded()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            serverList.TryAdd(endPoint);

            var added = serverList.TryAdd(new IPEndPoint(IPAddress.Loopback, 27015));
            Assert.False(added, "TryAdd should not have added the equal IPEndPoint to the list.");

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal(1, addresses.Length);
            Assert.Equal(endPoint, addresses[0]);
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingEndPointWithDifferentAddress()
        {
            serverList.TryAdd(new IPEndPoint(IPAddress.Loopback, 27015));

            var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 27015);

            var added = serverList.TryAdd(endPoint);
            Assert.True(added, "TryAdd should have added the IPEndPoint to the list.");

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal(2, addresses.Length);
            Assert.Equal(endPoint, addresses[1]);
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingEndPointWithDifferentPort()
        {
            serverList.TryAdd(new IPEndPoint(IPAddress.Loopback, 27015));

            var endPoint = new IPEndPoint(IPAddress.Loopback, 27016);

            var added = serverList.TryAdd(endPoint);
            Assert.True(added, "TryAdd should have added the IPEndPoint to the list.");

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal(2, addresses.Length);
            Assert.Equal(endPoint, addresses[1]);
        }

        [Fact]
        public void TryAdd_ReturnsTrue_AddingEndPointWithDifferentAddressAndPort()
        {
            serverList.TryAdd(new IPEndPoint(IPAddress.Loopback, 27015));

            var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 27016);

            var added = serverList.TryAdd(endPoint);
            Assert.True(added, "TryAdd should have added the IPEndPoint to the list.");

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal(2, addresses.Length);
            Assert.Equal(endPoint, addresses[1]);
        }

        [Fact]
        public void GetNextServer_ReturnsNull_IfListIsEmpty()
        {
            var endPoint = serverList.GetNextServer();
            Assert.Null(endPoint);
        }

        [Fact]
        public void GetNextServer_ReturnsServer_IfListHasServers()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            serverList.TryAdd(endPoint);

            var nextEndPoint = serverList.GetNextServer();
            Assert.Equal(endPoint, nextEndPoint);
        }

        [Fact]
        public void GetNextServer_ReturnsServer_IfListHasServers_EvenIfAllServersAreBad()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            serverList.TryAdd(endPoint);
            serverList.Mark(endPoint, ServerQuality.Bad);

            var nextEndPoint = serverList.GetNextServer();
            Assert.Equal(endPoint, nextEndPoint);
        }

        // Warning: This test is dependent on random values from the system and may, if you are unlucky enough, falsely report a failure.
        [Fact]
        public void GetNextServer_IsBiasedTowardsGoodServers()
        {
            var goodEndPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            var neutralEndPoint = new IPEndPoint(IPAddress.Loopback, 27016);
            var badEndPoint = new IPEndPoint(IPAddress.Loopback, 27017);

            serverList.TryAdd(goodEndPoint);
            serverList.TryAdd(neutralEndPoint);
            serverList.TryAdd(badEndPoint);

            const int numTimesToMark = 5;

            for (int i = 0; i < numTimesToMark; i++)
            {
                serverList.Mark(goodEndPoint, ServerQuality.Good);
                serverList.Mark(badEndPoint, ServerQuality.Bad);
            }

            var numTimesGotGoodServer = 0;
            var numTimesGotNeutralServer = 0;
            var numTimesGotBadServer = 0;

            const int numTimesToGetServer = 1000000;

            for (int i = 0; i < numTimesToGetServer; i++)
            {
                var nextServer = serverList.GetNextServer();

                if (nextServer == goodEndPoint)
                {
                    numTimesGotGoodServer++;
                }
                else if (nextServer == neutralEndPoint)
                {
                    numTimesGotNeutralServer++;
                }
                else if (nextServer == badEndPoint)
                {
                    numTimesGotBadServer++;
                }
                else
                {
                    Assert.True(false, "Got server that was not added to the server list.");
                }
            }

            Assert.True(numTimesGotGoodServer > numTimesGotNeutralServer, "Should get good servers more times than neutral servers");
            Assert.True(numTimesGotGoodServer > numTimesGotBadServer, "Should get good servers more times than bad servers");
            Assert.True(numTimesGotNeutralServer > numTimesGotBadServer, "Should get neutral servers more times than bad servers");
        }

        [Fact]
        public void TryMark_ReturnsTrue_IfServerInList()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            serverList.TryAdd(endPoint);

            var marked = serverList.TryMark(endPoint, ServerQuality.Good);
            Assert.True(marked);
        }

        [Fact]
        public void TryMark_ReturnsFalse_IfServerNotInList()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            serverList.TryAdd(endPoint);

            var marked = serverList.TryMark(new IPEndPoint(IPAddress.Loopback, 27016), ServerQuality.Good);
            Assert.False(marked);
        }
    }
}
