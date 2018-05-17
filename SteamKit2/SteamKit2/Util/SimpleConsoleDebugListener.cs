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
        /// <param name="category">The category of the message.</param>
        /// <param name="msg">The message to log.</param>
        public void WriteLine(string category, string msg)
        {
            Console.WriteLine("[{0}]: {1}", category, msg);
        }
    }
}
