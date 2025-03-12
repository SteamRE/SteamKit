using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SteamKitten
{
    /// <summary>
    /// Thrown when a HTTP request fails.
    /// </summary>
    public class SteamKittenWebRequestException : HttpRequestException
    {
        /// <summary>
        /// Represents the status code of the HTTP response.
        /// </summary>
        public new HttpStatusCode StatusCode => base.StatusCode ?? default;

        /// <summary>
        /// Represents the collection of HTTP response headers.
        /// </summary>
        public HttpResponseHeaders Headers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamKittenWebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">HTTP response message including the status code and data.</param>
        public SteamKittenWebRequestException(string message, HttpResponseMessage response)
            : base(message, null, response.StatusCode)
        {
            this.Headers = response.Headers;
        }

        /// <inheritdoc/>
        public SteamKittenWebRequestException()
        {
            using var response = new HttpResponseMessage();
            Headers = response.Headers;
        }

        /// <inheritdoc/>
        public SteamKittenWebRequestException( string message ) : base( message )
        {
            using var response = new HttpResponseMessage();
            Headers = response.Headers;
        }

        /// <inheritdoc/>
        public SteamKittenWebRequestException( string message, System.Exception innerException ) : base( message, innerException )
        {
            using var response = new HttpResponseMessage();
            Headers = response.Headers;
        }
    }
}
