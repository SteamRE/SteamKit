using SteamKit2.Discovery;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            await fileStorageProvider.UpdateServerListAsync(new List<IPEndPoint>()
            {
                new IPEndPoint(IPAddress.Any, 1234),
                new IPEndPoint(IPAddress.Loopback, 4321)
            });

            var servers = await fileStorageProvider.FetchServerListAsync();

            Assert.Equal(2, servers.Count());
            Assert.Equal(IPAddress.Any, servers.First().Address);
            Assert.Equal(1234, servers.First().Port);

            await fileStorageProvider.UpdateServerListAsync(new List<IPEndPoint>());
        }
    }
}
