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
    public class FileStorageServerListProviderFacts
    {
        public FileStorageServerListProviderFacts()
        {
            fileStorageProvider = new FileStorageServerListProvider("servertest.bin");
        }

        readonly FileStorageServerListProvider fileStorageProvider;

        [TestMethod]
        public async Task ReadsUpdatedServerList()
        {
            var initialServers = await fileStorageProvider.FetchServerListAsync();

            await fileStorageProvider.UpdateServerListAsync(new List<ServerRecord>()
            {
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Any, 1234)),
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Loopback, 4321))
            });

            var servers = await fileStorageProvider.FetchServerListAsync();

            Assert.AreEqual(2, servers.Count());
            Assert.AreEqual(IPAddress.Any.ToString(), servers.First().GetHost());
            Assert.AreEqual(1234, servers.First().GetPort());
            Assert.AreEqual(ProtocolTypes.Tcp | ProtocolTypes.Udp, servers.First().ProtocolTypes);

            await fileStorageProvider.UpdateServerListAsync(new List<ServerRecord>());
        }
    }
}
