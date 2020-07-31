/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;

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

        internal static IEnumerable<ProtocolTypes> GetFlags(this ProtocolTypes self)
        {
            if (self.HasFlagsFast(ProtocolTypes.Tcp))
            {
                yield return ProtocolTypes.Tcp;
            }

            if (self.HasFlagsFast(ProtocolTypes.Udp))
            {
                yield return ProtocolTypes.Udp;
            }

            if (self.HasFlagsFast(ProtocolTypes.WebSocket))
            {
                yield return ProtocolTypes.WebSocket;
            }

        }
    }
}
