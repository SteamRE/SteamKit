using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Discovery;
using Xunit;

namespace Tests
{
    public class SteamConfigurationDefaultFacts
    {
        public SteamConfigurationDefaultFacts()
        {
            configuration = SteamConfiguration.Create(_ => { });
        }

        readonly SteamConfiguration configuration;

        [Fact]
        public void AllowsDirectoryFetch()
        {
            Assert.Equal(true, configuration.AllowDirectoryFetch);
        }

        [Fact]
        public void CellIDIsZero()
        {
            Assert.Equal(0u, configuration.CellID);
        }

        [Fact]
        public void ConnectionTimeoutIsFiveSeconds()
        {
            Assert.Equal(TimeSpan.FromSeconds(5), configuration.ConnectionTimeout);
        }

        [Fact]
        public void DefaultPersonaStateFlags()
        {
            var expected = EClientPersonaStateFlag.PlayerName | EClientPersonaStateFlag.Presence |
                    EClientPersonaStateFlag.SourceID | EClientPersonaStateFlag.GameExtraInfo |
                    EClientPersonaStateFlag.LastSeen;

            Assert.Equal(expected, configuration.DefaultPersonaStateFlags);
        }

        [Fact]
        public void DefaultHttpMessageHandler()
        {
            Assert.NotNull(configuration.HttpMessageHandlerFactory);

            using (var handler = configuration.HttpMessageHandlerFactory())
            {
                Assert.NotNull(handler);
                Assert.IsType<HttpClientHandler>(handler);

                using (var secondHandler = configuration.HttpMessageHandlerFactory())
                {
                    Assert.NotNull(secondHandler);
                    Assert.IsType<HttpClientHandler>(secondHandler);

                    Assert.NotSame(handler, secondHandler);
                }
            }
        }

        [Fact]
        public async Task DefaultHttpClientFactory()
        {
            using (var handler = new StubHttpMessageHandler())
            {
                using (var client = configuration.HttpClientFactory(handler))
                {
                    var task = client.GetAsync("http://example.com/");

                    Assert.Equal("http://example.com/", handler.LastRequest?.RequestUri.AbsoluteUri);
                    Assert.False(task.IsCompleted);

                    handler.Completion.SetResult(new HttpResponseMessage(HttpStatusCode.OK));

                    var result = await task;
                    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                }
            }
        }

        [Fact]
        public void ServerListProviderIsNothingFancy()
        {
            Assert.IsType<NullServerListProvider>(configuration.ServerListProvider);
        }

        [Fact]
        public void ServerListIsNotNull()
        {
            Assert.NotNull(configuration.ServerList);
        }

        [Fact]
        public void DefaultProtocols()
        {
            Assert.Equal(ProtocolTypes.Tcp, configuration.ProtocolTypes);
        }

        [Fact]
        public void PublicUniverse()
        {
            Assert.Equal(EUniverse.Public, configuration.Universe);
        }

        [Fact]
        public void WebAPIAddress()
        {
            Assert.Equal("https://api.steampowered.com/", configuration.WebAPIBaseAddress?.AbsoluteUri);
        }

        [Fact]
        public void NoWebAPIKey()
        {
            Assert.Null(configuration.WebAPIKey);
        }

        class StubHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage LastRequest { get; private set; }

            public TaskCompletionSource<HttpResponseMessage> Completion { get; set; } = new TaskCompletionSource<HttpResponseMessage>();

            public bool IsDisposed { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Completion.Task;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }
    }

    public class SteamConfigurationConfiguredObjectFacts
    {
        public SteamConfigurationConfiguredObjectFacts()
        {
            configuration = SteamConfiguration.Create(b =>
                b.WithDirectoryFetch(false)
                 .WithCellID(123)
                 .WithConnectionTimeout(TimeSpan.FromMinutes(1))
                 .WithDefaultPersonaStateFlags(EClientPersonaStateFlag.SourceID)
                 .WithHttpClientFactory(h => { var c = new HttpClient(h); c.DefaultRequestHeaders.Add("X-SteamKit-Tests", "true"); return c; })
                 .WithHttpMessageHandlerFactory(() => new HttpClientHandler() { Properties = { ["SteamKit2-Tests"] = "true" } })
                 .WithProtocolTypes(ProtocolTypes.WebSocket | ProtocolTypes.Udp)
                 .WithServerListProvider(new CustomServerListProvider())
                 .WithUniverse(EUniverse.Internal)
                 .WithWebAPIBaseAddress(new Uri("http://foo.bar.com/api/"))
                 .WithWebAPIKey("T0PS3kR1t"));
        }

        readonly SteamConfiguration configuration;

        [Fact]
        public void DirectoryFetchIsConfigured()
        {
            Assert.Equal(false, configuration.AllowDirectoryFetch);
        }

        [Fact]
        public void CellIDIsConfigured()
        {
            Assert.Equal(123u, configuration.CellID);
        }

        [Fact]
        public void ConnectionTimeoutIsConfigured()
        {
            Assert.Equal(TimeSpan.FromMinutes(1), configuration.ConnectionTimeout);
        }

        [Fact]
        public void HttpClientFactoryIsConfigured()
        {
            using (var handler = new HttpClientHandler())
            {
                var client = configuration.HttpClientFactory(handler);
                Assert.Equal("true", client.DefaultRequestHeaders.GetValues("X-SteamKit-Tests").FirstOrDefault());
            }
        }

        [Fact]
        public void HttpMessageHandlerFactoryIsConfigured()
        {
            using (var client = configuration.HttpMessageHandlerFactory() as HttpClientHandler)
            {
                Assert.Equal("true", client.Properties["SteamKit2-Tests"]);
            }
        }

        [Fact]
        public void PersonaStateFlagsIsConfigured()
        {
            Assert.Equal(EClientPersonaStateFlag.SourceID, configuration.DefaultPersonaStateFlags);
        }

        [Fact]
        public void ServerListProviderIsConfigured()
        {
            Assert.IsType<CustomServerListProvider>(configuration.ServerListProvider);
        }

        [Fact]
        public void ServerListIsNotNull()
        {
            Assert.NotNull(configuration.ServerList);
        }

        [Fact]
        public void ProtocolsAreConfigured()
        {
            Assert.Equal(ProtocolTypes.WebSocket | ProtocolTypes.Udp, configuration.ProtocolTypes);
        }

        [Fact]
        public void UniverseIsConfigured()
        {
            Assert.Equal(EUniverse.Internal, configuration.Universe);
        }

        [Fact]
        public void WebAPIAddress()
        {
            Assert.Equal("http://foo.bar.com/api/", configuration.WebAPIBaseAddress?.AbsoluteUri);
        }

        [Fact]
        public void NoWebAPIKey()
        {
            Assert.Equal("T0PS3kR1t", configuration.WebAPIKey);
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
