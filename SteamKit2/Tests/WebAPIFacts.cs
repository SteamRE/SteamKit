using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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
    }
}
