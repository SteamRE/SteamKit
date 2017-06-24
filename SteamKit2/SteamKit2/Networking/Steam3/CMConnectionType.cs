namespace SteamKit2
{
    /// <summary>
    /// The connection method to use when communicating with this CM server
    /// </summary>
    public enum CMConnectionType
    {
        /// <summary>
        /// This CM server communicates over TCP or UDP. This is an IPv4 endpoint.
        /// </summary>
        Socket,

        /// <summary>
        /// This CM server that communicates of WebSocket (HTTP w/ TLS). This is typically a DNS endpoint, but could also contain an IP address.
        /// </summary>
        WebSocket
    }
}
