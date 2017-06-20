namespace SteamKit2
{
    /// <summary>
    /// The type of communications protocol to use when communicating with the Steam backend
    /// </summary>
    public enum ProtocolType
    {
        /// <summary>
        /// TCP
        /// </summary>
        Tcp,

        /// <summary>
        /// UDP
        /// </summary>
        Udp,

        /// <summary>
        /// WebSockets (HTTP / TLS)
        /// </summary>
        WebSocket
    }
}
