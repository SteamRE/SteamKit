using System.Linq;
using System.Net.Http;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class HttpClientPurposeConfiguredFacts
    {
        public HttpClientPurposeConfiguredFacts()
        {
            configuration = SteamConfiguration.Create( b =>
                b.WithHttpClientFactory( purpose =>
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add( "X-Purpose", purpose.ToString() );
                    return client;
                } ) );
        }

        readonly SteamConfiguration configuration;

        [Fact]
        public void WebAPIPurposeReceivesCorrectParameter()
        {
            using var client = configuration.HttpClientFactory( HttpClientPurpose.WebAPI );
            Assert.Equal( nameof( HttpClientPurpose.WebAPI ), client.DefaultRequestHeaders.GetValues( "X-Purpose" ).FirstOrDefault() );
        }

        [Fact]
        public void CMWebSocketPurposeReceivesCorrectParameter()
        {
            using var client = configuration.HttpClientFactory( HttpClientPurpose.CMWebSocket );
            Assert.Equal( nameof( HttpClientPurpose.CMWebSocket ), client.DefaultRequestHeaders.GetValues( "X-Purpose" ).FirstOrDefault() );
        }

        [Fact]
        public void CDNPurposeReceivesCorrectParameter()
        {
            using var client = configuration.HttpClientFactory( HttpClientPurpose.CDN );
            Assert.Equal( nameof( HttpClientPurpose.CDN ), client.DefaultRequestHeaders.GetValues( "X-Purpose" ).FirstOrDefault() );
        }

        [Fact]
        public void DifferentPurposesReceiveDifferentConfigurations()
        {
            using var webApiClient = configuration.HttpClientFactory( HttpClientPurpose.WebAPI );
            using var cdnClient = configuration.HttpClientFactory( HttpClientPurpose.CDN );

            Assert.Equal( nameof( HttpClientPurpose.WebAPI ), webApiClient.DefaultRequestHeaders.GetValues( "X-Purpose" ).FirstOrDefault() );
            Assert.Equal( nameof( HttpClientPurpose.CDN ), cdnClient.DefaultRequestHeaders.GetValues( "X-Purpose" ).FirstOrDefault() );
        }
    }
}
