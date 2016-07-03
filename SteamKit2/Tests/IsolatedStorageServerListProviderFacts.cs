using SteamKit2.Discovery;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public void ReadsUpdatedServerList()
        {
            isolatedStorageProvider.UpdateServerListAsync(new List<IPEndPoint>()
            {
                new IPEndPoint(IPAddress.Any, 1234),
                new IPEndPoint(IPAddress.Loopback, 4321)
            }).Wait();

            var serverListTask = isolatedStorageProvider.FetchServerListAsync();
            serverListTask.Wait();

            var servers = serverListTask.Result;

            Assert.Equal(2, servers.Count);
            Assert.Equal(IPAddress.Any, servers.First().Address);
            Assert.Equal(1234, servers.First().Port);

            isolatedStorageProvider.UpdateServerListAsync(new List<IPEndPoint>()).Wait();
        }
    }
}
