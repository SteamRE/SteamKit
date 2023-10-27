using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using SteamKit2.CDN;

namespace Tests
{
    [TestClass]
    public class CDNClientFacts
    {
        [TestMethod]
        public async Task ThrowsSteamKitWebExceptionOnUnsuccessfulWebResponseForManifest()
        {
            var configuration = SteamConfiguration.Create( x => x.WithHttpClientFactory( () => new HttpClient( new TeapotHttpMessageHandler() ) ) );
            var steam = new SteamClient( configuration );
            var client = new Client( steam );
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = "localhost",
                VHost = "localhost",
                Port = 80
            };

            var ex = await Assert.ThrowsExceptionAsync<SteamKitWebRequestException>( () => client.DownloadManifestAsync( depotId: 0, manifestId: 0, manifestRequestCode: 0, server ) );
            Assert.AreEqual( ( HttpStatusCode )418, ex.StatusCode );
        }

        [TestMethod]
        public async Task ThrowsSteamKitWebExceptionOnUnsuccessfulWebResponseForChunk()
        {
            var configuration = SteamConfiguration.Create( x => x.WithHttpClientFactory( () => new HttpClient( new TeapotHttpMessageHandler() ) ) );
            var steam = new SteamClient( configuration );
            var client = new Client( steam );
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = "localhost",
                VHost = "localhost",
                Port = 80
            };
            var chunk = new DepotManifest.ChunkData
            {
                ChunkID = new byte[] { 0xFF },
            };

            var ex = await Assert.ThrowsExceptionAsync<SteamKitWebRequestException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server ) );
            Assert.AreEqual( ( HttpStatusCode )418, ex.StatusCode );
        }

        [TestMethod]
        public async Task ThrowsWhenNoChunkIDIsSet()
        {
            var configuration = SteamConfiguration.Create( x => x.WithHttpClientFactory( () => new HttpClient( new TeapotHttpMessageHandler() ) ) );
            var steam = new SteamClient( configuration );
            var client = new Client( steam );
            var server = new Server
            {
                Protocol = Server.ConnectionProtocol.HTTP,
                Host = "localhost",
                VHost = "localhost",
                Port = 80
            };
            var chunk = new DepotManifest.ChunkData();

            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server ) );
            Assert.AreEqual( "chunk", ex.ParamName );
        }

        sealed class TeapotHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
                => Task.FromResult( new HttpResponseMessage( ( HttpStatusCode )418 ) );
        }
    }
}
