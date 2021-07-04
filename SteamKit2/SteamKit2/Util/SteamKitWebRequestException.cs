using System;
using System.ComponentModel;
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
#if NET5_0_OR_GREATER
        [Obsolete("Use HttpRequestException.StatusCode instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new HttpStatusCode StatusCode { get; private set; }
#else
        public HttpStatusCode StatusCode { get; private set; }
#endif

        /// <summary>
        /// Represents the collection of HTTP response headers.
        /// </summary>
        public HttpResponseHeaders Headers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamKitWebRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="response">HTTP response message including the status code and data.</param>
#if NET5_0_OR_GREATER
        public SteamKitWebRequestException(string message, HttpResponseMessage response)
            : base(message, inner: null, statusCode: response.StatusCode)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            this.StatusCode = response.StatusCode;
#pragma warning restore CS0618 // Type or member is obsolete
            this.Headers = response.Headers;
        }
#else
        public SteamKitWebRequestException(string message, HttpResponseMessage response)
            : base(message)
        {
            this.StatusCode = response.StatusCode;
            this.Headers = response.Headers;
        }
#endif
    }
}
