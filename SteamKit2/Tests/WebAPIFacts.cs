using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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
            var listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 28123));
            listener.Start();
            try
            {
                AcceptAndAutoReplyNextSocket(listener);

                var baseUri = "http://localhost:28123";
                dynamic iface = WebAPI.GetAsyncInterface(new Uri(baseUri), "IFooService");

                await Assert.ThrowsAsync<WebAPIRequestException>(() => (Task)iface.PerformFooOperation());
            }
            finally
            {
                listener.Stop();
            }
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

        // Primitive HTTP listener function that always returns HTTP 503.
        static void AcceptAndAutoReplyNextSocket(TcpListener listener)
        {
            void OnSocketAccepted(IAsyncResult result)
            {
                try
                {
                    using (var socket = listener.EndAcceptSocket(result))
                    using (var stream = new NetworkStream(socket))
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        string line;
                        do
                        {
                            line = reader.ReadLine();
                        }
                        while (!string.IsNullOrEmpty(line));

                        writer.WriteLine("HTTP/1.1 503 Service Unavailable");
                        writer.WriteLine("X-Response-Source: Unit Test");
                        writer.WriteLine();
                    }
                }
                catch
                {
                }
            }

            var ar = listener.BeginAcceptSocket(OnSocketAccepted, null);
            if (ar.CompletedSynchronously)
            {
                OnSocketAccepted(ar);
            }
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
