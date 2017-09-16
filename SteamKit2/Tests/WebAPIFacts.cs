using System;
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
            var config = new SteamConfiguration
            {
                WebAPIBaseAddress = new Uri("http://example.com"),
                WebAPIKey = "hello world"
            };

            var iface = config.GetAsyncWebAPIInterface("TestInterface");

            Assert.Equal(iface.iface, "TestInterface");
            Assert.Equal(iface.apiKey, "hello world");
            Assert.Equal(iface.httpClient.BaseAddress, new Uri("http://example.com"));
        }
    }
}
