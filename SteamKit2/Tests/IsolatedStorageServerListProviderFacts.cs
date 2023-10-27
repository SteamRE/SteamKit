using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using SteamKit2.Discovery;

namespace Tests
{
    [TestClass]
    public class IsolatedStorageServerListProviderFacts
    {
        public IsolatedStorageServerListProviderFacts()
        {
            isolatedStorageProvider = new IsolatedStorageServerListProvider();
        }

        readonly IsolatedStorageServerListProvider isolatedStorageProvider;

        [TestMethod]
        public async Task ReadsUpdatedServerList()
        {
            await isolatedStorageProvider.UpdateServerListAsync(new List<ServerRecord>()
            {
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Any, 1234)),
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Loopback, 4321))
            });

            var servers = await isolatedStorageProvider.FetchServerListAsync();

            Assert.AreEqual(2, servers.Count());
            Assert.AreEqual(IPAddress.Any.ToString(), servers.First().GetHost());
            Assert.AreEqual(1234, servers.First().GetPort());
            Assert.AreEqual(ProtocolTypes.Tcp | ProtocolTypes.Udp, servers.First().ProtocolTypes);

            await isolatedStorageProvider.UpdateServerListAsync(new List<ServerRecord>());
        }
    }
}
