using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class WebAPIFacts
    {
        [Fact]
        public void WebAPIHasDefaultTimeout()
        {
            var iface = WebAPI.GetInterface( new Uri("https://whatever/"), "ISteamWhatever" );

            Assert.Equal( iface.Timeout, TimeSpan.FromSeconds( 100 ) );
        }

        [Fact]
        public void WebAPIAsyncHasDefaultTimeout()
        {
            var iface = WebAPI.GetAsyncInterface( new Uri("https://whatever/"), "ISteamWhatever" );

            Assert.Equal( iface.Timeout, TimeSpan.FromSeconds( 100 ) );
        }

        [Fact]
        public void SteamConfigWebAPIInterface()
        {
            var config = SteamConfiguration.Create(b =>
                b.WithWebAPIBaseAddress(new Uri("http://example.com"))
                 .WithWebAPIKey("hello world"));

            var iface = config.GetAsyncWebAPIInterface("TestInterface");

            Assert.Equal("TestInterface", iface.iface);
            Assert.Equal("hello world", iface.apiKey);
            Assert.Equal(new Uri("http://example.com"), iface.httpClient.BaseAddress);
        }

        [Fact]
        public async Task ThrowsWebAPIRequestExceptionIfRequestUnsuccessful()
        {
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( new ServiceUnavailableHttpMessageHandler() ) ) );
            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" ); 

           await Assert.ThrowsAsync<WebAPIRequestException>(() => (Task)iface.PerformFooOperation());
        }

        [Fact]
        public async Task UsesSingleParameterArgumentsDictionary()
        {
            var capturingHandler = new CaturingHttpMessageHandler();
            var configuration = SteamConfiguration.Create( c => c.WithHttpClientFactory( () => new HttpClient( capturingHandler ) ) );

            dynamic iface = configuration.GetAsyncWebAPIInterface( "IFooService" );

            var args = new Dictionary<string, object>
            {
                [ "f" ] = "foo",
                [ "b" ] = "bar",
                [ "method" ] = "PUT"
            };

            var response = await iface.PerformFooOperation2( args );

            var request = capturingHandler.MostRecentRequest;
            Assert.NotNull( request );
            Assert.Equal( "/IFooService/PerformFooOperation/v2", request.RequestUri.AbsolutePath );
            Assert.Equal( HttpMethod.Put, request.Method );

            var formData = await request.Content.ReadAsFormDataAsync();
            Assert.Equal( 3, formData.Count );
            Assert.Equal( "foo", formData[ "f" ] );
            Assert.Equal( "bar", formData[ "b" ] );
            Assert.Equal( "vdf", formData[ "format" ] );
        }

        sealed class ServiceUnavailableHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
                => Task.FromResult( new HttpResponseMessage( HttpStatusCode.ServiceUnavailable ) );
        }

        sealed class CaturingHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage MostRecentRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                MostRecentRequest = request;

                return Task.FromResult( new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new ByteArrayContent( Array.Empty<byte>() )
                });
            }
        }
    }
}
