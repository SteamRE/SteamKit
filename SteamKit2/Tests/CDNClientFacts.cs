using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class CDNClientFacts
    {
        [Fact]
        public async Task ThrowsSteamKitWebExceptionOnUnsuccessfulWebResponse()
        {
            var configuration = SteamConfiguration.Create(x => x.WithHttpClientFactory(() => new HttpClient(new TeapotHttpMessageHandler())));
            var steam = new SteamClient(configuration);
            var client = new CDNClient(steam);

            try
            {
                await client.DownloadManifestAsync(0, 0, "localhost", "12345");
                throw new InvalidOperationException("This should be unreachable.");
            }
            catch (SteamKitWebRequestException ex)
            {
#if NET5_0_OR_GREATER
                Assert.Equal((HttpStatusCode)418, ((HttpRequestException)ex).StatusCode);
#endif

#pragma warning disable CS0618 // Type or member is obsolete
                Assert.Equal((HttpStatusCode)418, ex.StatusCode);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        sealed class TeapotHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(new HttpResponseMessage((HttpStatusCode)418));
        }
    }
}
