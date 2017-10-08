using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Discovery;
using Xunit;

namespace Tests
{
    public class IsolatedStorageServerListProviderFacts
    {
        public IsolatedStorageServerListProviderFacts()
        {
            isolatedStorageProvider = new IsolatedStorageServerListProvider();
        }

        readonly IsolatedStorageServerListProvider isolatedStorageProvider;

        [Fact]
        public async Task ReadsUpdatedServerList()
        {
            await isolatedStorageProvider.UpdateServerListAsync(new List<ServerRecord>()
            {
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Any, 1234)),
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Loopback, 4321))
            });

            var servers = await isolatedStorageProvider.FetchServerListAsync();

            Assert.Equal(2, servers.Count());
            Assert.Equal(IPAddress.Any.ToString(), servers.First().GetHost());
            Assert.Equal(1234, servers.First().GetPort());
            Assert.Equal(ProtocolTypes.Tcp | ProtocolTypes.Udp, servers.First().ProtocolTypes);

            await isolatedStorageProvider.UpdateServerListAsync(new List<ServerRecord>());
        }
    }
}
