using System.Collections.Generic;
using System.Linq;
using System.Net;
using SteamKit2.Discovery;
using Xunit;

namespace Tests
{
    public class FileStorageServerListProviderFacts
    {
        public FileStorageServerListProviderFacts()
        {
            fileStorageProvider = new FileStorageServerListProvider("servertest.bin");
        }

        readonly FileStorageServerListProvider fileStorageProvider;

        [Fact]
        public async void ReadsUpdatedServerList()
        {
            await fileStorageProvider.UpdateServerListAsync(new List<CMServerRecord>()
            {
                CMServerRecord.SocketServer(new IPEndPoint(IPAddress.Any, 1234)),
                CMServerRecord.SocketServer(new IPEndPoint(IPAddress.Loopback, 4321))
            });

            var servers = await fileStorageProvider.FetchServerListAsync();

            Assert.Equal(2, servers.Count());
            Assert.Equal(IPAddress.Any, servers.First().GetIPAddress());
            Assert.Equal(1234, servers.First().GetPort());

            await fileStorageProvider.UpdateServerListAsync(new List<CMServerRecord>());
        }
    }
}
