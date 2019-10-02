using System;

namespace SteamKit2
{
    /// <summary>
    /// A debug listener that writes debug output to the system console.
    /// </summary>
    public sealed class SimpleConsoleDebugListener : IDebugListener
    {
        /// <summary>
        /// Called when the DebugLog wishes to inform listeners of debug spew.
        /// </summary>
        /// <param name="token">A token to uniquely identify the source of the message.</param>
        /// <param name="category">The category of the message.</param>
        /// <param name="msg">The message to log.</param>
        public void WriteLine(LoggerToken token, string category, string msg)
        {
            if ( token.IsDefault )
            {
                Console.WriteLine( "[{0}]: {1}", category, msg );
            }
            else
            {
                Console.WriteLine( "[{0}/{1}]: {2}", token.Identifier, category, msg );
            }
        }
    }
}
