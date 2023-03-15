using System;

namespace SteamKit2
{
    /// <summary>
    /// Thrown when <see cref="SteamAuthentication"/> fails to authenticate.
    /// </summary>
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// Gets the result of the authentication request.
        /// </summary>
        public EResult Result { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="result">The result code that describes the error.</param>
        public AuthenticationException( string message, EResult result )
            : base( $"{message} with result {result}." )
        {
            Result = result;
        }
    }
}
