﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using SteamKit2;
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
            var initialServers = await fileStorageProvider.FetchServerListAsync();

            await fileStorageProvider.UpdateServerListAsync(new List<ServerRecord>()
            {
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Any, 1234)),
                ServerRecord.CreateSocketServer(new IPEndPoint(IPAddress.Loopback, 4321))
            });

            var servers = await fileStorageProvider.FetchServerListAsync();

            Assert.Equal(2, servers.Count());
            Assert.Equal(IPAddress.Any.ToString(), servers.First().GetHost());
            Assert.Equal(1234, servers.First().GetPort());
            Assert.Equal(ProtocolTypes.Tcp, servers.First().ProtocolTypes);

            await fileStorageProvider.UpdateServerListAsync(new List<ServerRecord>());
        }
    }
}
