using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SteamKit2
{
    /// <summary>
    /// Thrown when a HTTP request fails.
    /// </summary>
    public class SteamKitWebRequestException : HttpRequestException
    {
        /// <summary>
        /// Represents the status code of the HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Represents the collection of HTTP response headers.
        /// </summary>
        public HttpResponseHeaders Headers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamKitWebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">HTTP response message including the status code and data.</param>
        public SteamKitWebRequestException(string message, HttpResponseMessage response)
            : base(message)
        {
            this.StatusCode = response.StatusCode;
            this.Headers = response.Headers;
        }
    }
}
