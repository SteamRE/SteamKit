using System;

namespace SteamKit2
{
    /// <summary>
    /// Thrown when Steam encounters a remote error with a pending <see cref="AsyncJob"/>.
    /// </summary>
    public class AsyncJobFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJobFailedException"/> class.
        /// </summary>
        public AsyncJobFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJobFailedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AsyncJobFailedException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJobFailedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AsyncJobFailedException( string message, Exception innerException )
            : base( message, innerException )
        {
        }
    }
}
