using System;
using System.Net.Http;

namespace SteamKit2
{
    /// <summary>
    /// Provides helper extensions to make WebAPI interfaces from existing SteamConfiguration.
    /// </summary>
    public static class SteamConfigurationWebAPIExtensions
    {
        /// <summary>
        /// Retrieves a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="config">The configuration to use for this Web API interface.</param>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <returns>A dynamic <see cref="WebAPI.Interface"/> object to interact with the Web API.</returns>
        public static WebAPI.Interface GetWebAPIInterface(this SteamConfiguration config, string iface)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return new WebAPI.Interface(config.GetHttpClientForWebAPI(), iface, config.WebAPIKey);
        }

        /// <summary>
        /// Retrieves a dynamic handler capable of interacting with the specified interface on the Web API.
        /// </summary>
        /// <param name="config">The configuration to use for this Web API interface.</param>
        /// <param name="iface">The interface to retrieve a handler for.</param>
        /// <returns>A dynamic <see cref="WebAPI.AsyncInterface"/> object to interact with the Web API.</returns>
        public static WebAPI.AsyncInterface GetAsyncWebAPIInterface(this SteamConfiguration config, string iface)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return new WebAPI.AsyncInterface(config.GetHttpClientForWebAPI(), iface, config.WebAPIKey);
        }

        internal static HttpClient GetHttpClientForWebAPI(this SteamConfiguration config)
        {
            var client = config.HttpClientFactory();

            client.BaseAddress = config.WebAPIBaseAddress;
            client.Timeout = WebAPI.DefaultTimeout;

            return client;
        }
    }
}
