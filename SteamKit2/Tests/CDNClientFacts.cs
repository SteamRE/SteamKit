using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.CDN;
using Xunit;

namespace Tests
{
    public class CDNClientFacts
    {
        [Fact]
        public async Task ThrowsSteamKitWebExceptionOnUnsuccessfulWebResponseForManifest()
        {
            var configuration = SteamConfiguration.Create( x => x.WithHttpClientFactory( () => new HttpClient( new TeapotHttpMessageHandler() ) ) );
            var steam = new SteamClient( configuration );
            using var client = new Client( steam );
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = "localhost",
                VHost = "localhost",
                Port = 80
            };

            var ex = await Assert.ThrowsAsync<SteamKitWebRequestException>( () => client.DownloadManifestAsync( depotId: 0, manifestId: 0, manifestRequestCode: 0, server ) );
            Assert.Equal( ( HttpStatusCode )418, ex.StatusCode );
        }

        [Fact]
        public async Task ThrowsSteamKitWebExceptionOnUnsuccessfulWebResponseForChunk()
        {
            var configuration = SteamConfiguration.Create( x => x.WithHttpClientFactory( () => new HttpClient( new TeapotHttpMessageHandler() ) ) );
            var steam = new SteamClient( configuration );
            using var client = new Client( steam );
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = "localhost",
                VHost = "localhost",
                Port = 80
            };
            var chunk = new DepotManifest.ChunkData
            {
                ChunkID = [0xFF],
            };

            var ex = await Assert.ThrowsAsync<SteamKitWebRequestException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server ) );
            Assert.Equal( ( HttpStatusCode )418, ex.StatusCode );
        }

        [Fact]
        public async Task ThrowsWhenNoChunkIDIsSet()
        {
            var configuration = SteamConfiguration.Create( x => x.WithHttpClientFactory( () => new HttpClient( new TeapotHttpMessageHandler() ) ) );
            var steam = new SteamClient( configuration );
            using var client = new Client( steam );
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = "localhost",
                VHost = "localhost",
                Port = 80
            };
            var chunk = new DepotManifest.ChunkData();

            var ex = await Assert.ThrowsAsync<ArgumentException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server ) );
            Assert.Equal( "chunk", ex.ParamName );
        }

        sealed class TeapotHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
                => Task.FromResult( new HttpResponseMessage( ( HttpStatusCode )418 ) );
        }
    }
}
