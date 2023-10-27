using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using SteamKit2.Discovery;

namespace Tests
{
    [TestClass]
    public class SteamConfigurationDefaultFacts
    {
        public SteamConfigurationDefaultFacts()
        {
            configuration = SteamConfiguration.Create(_ => { });
        }

        readonly SteamConfiguration configuration;

        [TestMethod]
        public void AllowsDirectoryFetch()
        {
            Assert.IsTrue(configuration.AllowDirectoryFetch);
        }

        [TestMethod]
        public void CellIDIsZero()
        {
            Assert.AreEqual(0u, configuration.CellID);
        }

        [TestMethod]
        public void ConnectionTimeoutIsFiveSeconds()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(5), configuration.ConnectionTimeout);
        }

        [TestMethod]
        public void DefaultPersonaStateFlags()
        {
            var expected = EClientPersonaStateFlag.PlayerName | EClientPersonaStateFlag.Presence |
                    EClientPersonaStateFlag.SourceID | EClientPersonaStateFlag.GameExtraInfo |
                    EClientPersonaStateFlag.LastSeen;

            Assert.AreEqual(expected, configuration.DefaultPersonaStateFlags);
        }

        [TestMethod]
        public void DefaultHttpClientFactory()
        {
            using (var client = configuration.HttpClientFactory())
            {
                Assert.IsNotNull(client);
                Assert.IsInstanceOfType<HttpClient>(client);

                var steamKitAssemblyVersion = typeof( SteamClient ).Assembly.GetName().Version;
                Assert.AreEqual("SteamKit/" + steamKitAssemblyVersion.ToString(fieldCount: 3), client.DefaultRequestHeaders.UserAgent.ToString());
            }
        }

        [TestMethod]
        public void DefaultMachineInfoProvider()
        {
            Assert.IsNotNull(configuration.MachineInfoProvider);
            Assert.IsNotInstanceOfType<DefaultMachineInfoProvider>(configuration.MachineInfoProvider);
        }

        [TestMethod]
        public void ServerListProviderIsNothingFancy()
        {
            Assert.IsInstanceOfType<MemoryServerListProvider>(configuration.ServerListProvider);
        }

        [TestMethod]
        public void ServerListIsNotNull()
        {
            Assert.IsNotNull(configuration.ServerList);
        }

        [TestMethod]
        public void DefaultProtocols()
        {
            Assert.AreEqual(ProtocolTypes.Tcp, configuration.ProtocolTypes);
        }

        [TestMethod]
        public void PublicUniverse()
        {
            Assert.AreEqual(EUniverse.Public, configuration.Universe);
        }

        [TestMethod]
        public void WebAPIAddress()
        {
            Assert.AreEqual("https://api.steampowered.com/", configuration.WebAPIBaseAddress?.AbsoluteUri);
        }

        [TestMethod]
        public void NoWebAPIKey()
        {
            Assert.IsNull(configuration.WebAPIKey);
        }
    }

    [TestClass]
    public class SteamConfigurationConfiguredObjectFacts
    {
        public SteamConfigurationConfiguredObjectFacts()
        {
            configuration = SteamConfiguration.Create(b =>
                b.WithDirectoryFetch(false)
                 .WithCellID(123)
                 .WithConnectionTimeout(TimeSpan.FromMinutes(1))
                 .WithDefaultPersonaStateFlags(EClientPersonaStateFlag.SourceID)
                 .WithHttpClientFactory(() => { var c = new HttpClient(); c.DefaultRequestHeaders.Add("X-SteamKit-Tests", "true"); return c; })
                 .WithMachineInfoProvider(new CustomMachineInfoProvider())
                 .WithProtocolTypes(ProtocolTypes.WebSocket | ProtocolTypes.Udp)
                 .WithServerListProvider(new CustomServerListProvider())
                 .WithUniverse(EUniverse.Internal)
                 .WithWebAPIBaseAddress(new Uri("http://foo.bar.com/api/"))
                 .WithWebAPIKey("T0PS3kR1t"));
        }

        readonly SteamConfiguration configuration;

        [TestMethod]
        public void DirectoryFetchIsConfigured()
        {
            Assert.IsFalse(configuration.AllowDirectoryFetch);
        }

        [TestMethod]
        public void CellIDIsConfigured()
        {
            Assert.AreEqual(123u, configuration.CellID);
        }

        [TestMethod]
        public void ConnectionTimeoutIsConfigured()
        {
            Assert.AreEqual(TimeSpan.FromMinutes(1), configuration.ConnectionTimeout);
        }

        [TestMethod]
        public void HttpClientFactoryIsConfigured()
        {
            using (var client = configuration.HttpClientFactory())
            {
                Assert.AreEqual("true", client.DefaultRequestHeaders.GetValues("X-SteamKit-Tests").FirstOrDefault());
            }
        }

        [TestMethod]
        public void MachineInfoProviderIsConfigured()
        {
            Assert.IsInstanceOfType<CustomMachineInfoProvider>(configuration.MachineInfoProvider);
            Assert.AreSame(configuration.MachineInfoProvider, configuration.MachineInfoProvider);
        }

        [TestMethod]
        public void PersonaStateFlagsIsConfigured()
        {
            Assert.AreEqual(EClientPersonaStateFlag.SourceID, configuration.DefaultPersonaStateFlags);
        }

        [TestMethod]
        public void ServerListProviderIsConfigured()
        {
            Assert.IsInstanceOfType<CustomServerListProvider>(configuration.ServerListProvider);
        }

        [TestMethod]
        public void ServerListIsNotNull()
        {
            Assert.IsNotNull(configuration.ServerList);
        }

        [TestMethod]
        public void ProtocolsAreConfigured()
        {
            Assert.AreEqual(ProtocolTypes.WebSocket | ProtocolTypes.Udp, configuration.ProtocolTypes);
        }

        [TestMethod]
        public void UniverseIsConfigured()
        {
            Assert.AreEqual(EUniverse.Internal, configuration.Universe);
        }

        [TestMethod]
        public void WebAPIAddress()
        {
            Assert.AreEqual("http://foo.bar.com/api/", configuration.WebAPIBaseAddress?.AbsoluteUri);
        }

        [TestMethod]
        public void NoWebAPIKey()
        {
            Assert.AreEqual("T0PS3kR1t", configuration.WebAPIKey);
        }

        class CustomMachineInfoProvider : IMachineInfoProvider
        {
            byte[] IMachineInfoProvider.GetDiskId()
                => throw new NotImplementedException();
            byte[] IMachineInfoProvider.GetMacAddress()
                => throw new NotImplementedException();
            byte[] IMachineInfoProvider.GetMachineGuid()
                => throw new NotImplementedException();
        }

        class CustomServerListProvider : IServerListProvider
        {
            Task<IEnumerable<ServerRecord>> IServerListProvider.FetchServerListAsync()
                => throw new NotImplementedException();

            Task IServerListProvider.UpdateServerListAsync(IEnumerable<ServerRecord> endpoints)
                => throw new NotImplementedException();
        }
    }
}
