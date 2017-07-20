/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;

namespace SteamKit2
{
    /// <summary>
    /// The type of communications protocol to use when communicating with the Steam backend
    /// </summary>
    [Flags]
    public enum ProtocolTypes
    {
        /// <summary>
        /// TCP
        /// </summary>
        Tcp = 1 << 0,

        /// <summary>
        /// UDP
        /// </summary>
        Udp = 1 << 1,

        /// <summary>
        /// WebSockets (HTTP / TLS)
        /// </summary>
        WebSocket = 1 << 2,
        
        /// <summary>
        /// All available protocol types
        /// </summary>
        All = Tcp | Udp | WebSocket
    }

    static class ProtocolTypesExtensions
    {
        public static bool HasFlagsFast(this ProtocolTypes self, ProtocolTypes flags)
            => (self & flags) > 0;
    }
}
