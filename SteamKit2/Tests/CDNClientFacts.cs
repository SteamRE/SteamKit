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
#if DEBUG
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
                ChunkID = [ 0xFF ],
            };

            var ex = await Assert.ThrowsAsync<SteamKitWebRequestException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server, [] ) );
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

            var ex = await Assert.ThrowsAsync<ArgumentException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server, [] ) );
            Assert.Equal( "chunk", ex.ParamName );
        }

        [Fact]
        public async Task ThrowsWhenDestinationBufferSmaller()
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
                ChunkID = [ 0xFF ],
                UncompressedLength = 64,
                CompressedLength = 32,
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server, new byte[ 4 ] ) );
            Assert.Equal( "destination", ex.ParamName );
        }

        [Fact]
        public async Task ThrowsWhenDestinationBufferSmallerWithDepotKey()
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
                ChunkID = [ 0xFF ],
                UncompressedLength = 64,
                CompressedLength = 32,
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>( () => client.DownloadDepotChunkAsync( depotId: 0, chunk, server, new byte[ 4 ], depotKey: [] ) );
            Assert.Equal( "destination", ex.ParamName );
        }

        [Theory]
        [InlineData( "10.0.0.1", true )]       // Private IPv4 (10.0.0.0/8)
        [InlineData( "172.16.0.1", true )]     // Private IPv4 (172.16.0.0/12)
        [InlineData( "192.168.0.1", true )]    // Private IPv4 (192.168.0.0/16)
        [InlineData( "8.8.8.8", false )]       // Public IPv4
        [InlineData( "127.0.0.1", true )]      // Loopback IPv4
        public void IsPrivateAddress_IPv4Tests( string ipAddress, bool expected )
        {
            IPAddress address = IPAddress.Parse( ipAddress );
            bool result = Client.IsPrivateAddress( address );

            Assert.Equal( expected, result );
        }

        [Theory]
        [InlineData( "fc00::1", true )]         // Private IPv6 (Unique Local Address)
        [InlineData( "fe80::1", true )]         // Link-local IPv6
        [InlineData( "2001:db8::1", false )]    // Public IPv6
        [InlineData( "::1", true )]             // Loopback IPv6
        public void IsPrivateAddress_IPv6Tests( string ipAddress, bool expected )
        {
            IPAddress address = IPAddress.Parse( ipAddress );
            bool result = Client.IsPrivateAddress( address );

            Assert.Equal( expected, result );
        }

        sealed class TeapotHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
                => Task.FromResult( new HttpResponseMessage( ( HttpStatusCode )418 ) );
        }
    }
#endif
}
