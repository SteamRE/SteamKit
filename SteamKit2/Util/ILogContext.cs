namespace SteamKit2
{
    /// <summary>
    /// A handle to write to the debug log in the context of a particular <see cref="SteamKit2.Internal.CMClient" />
    /// </summary>
    public interface ILogContext
    {
        /// <summary>
        /// Writes a line to the debug log, informing all listeners.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="message">A composite format string.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        void LogDebug( string category, string message, params object?[]? args );
    }

    sealed class DebugLogContext : ILogContext
    {
        public static ILogContext Instance { get; } = new DebugLogContext();

        public void LogDebug( string category, string message, params object?[]? args )
            => DebugLog.WriteLine( category, message, args );
    }
}
