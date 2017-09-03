/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Threading;
using SteamKit2.Discovery;

namespace SteamKit2
{
    /// <summary>
    /// Configuration object to use.
    /// This object should not be mutated after it is passed to one or more <see cref="SteamClient"/> objects.
    /// </summary>
    public sealed class SteamConfiguration
    {
        /// <summary>
        /// Creates a <see cref="SteamConfiguration"/> object.
        /// </summary>
        public SteamConfiguration()
        {
            serverListProvider = new NullServerListProvider();
            webAPIBaseAddress = WebAPI.DefaultBaseAddress;
            ServerList = new SmartCMServerList(this);
        }

        IServerListProvider serverListProvider;
        Uri webAPIBaseAddress;

        /// <summary>
        /// Whether or not to use the Steam Directory to discover available servers.
        /// </summary>
        public bool AllowDirectoryFetch { get; set; } = true;

        /// <summary>
        /// The Steam Cell ID to prioritize when connecting.
        /// </summary>
        public uint CellID { get; set; } = 0;

        /// <summary>
        /// The connection timeout used when connecting to Steam serves.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The default persona state flags used when requesting information for a new friend, or
        /// when calling <c>SteamFriends.RequestFriendInfo</c> without specifying flags.
        /// </summary>
        public EClientPersonaStateFlag DefaultPersonaStateFlags { get; set; } =
            EClientPersonaStateFlag.PlayerName | EClientPersonaStateFlag.Presence |
            EClientPersonaStateFlag.SourceID | EClientPersonaStateFlag.GameExtraInfo |
            EClientPersonaStateFlag.LastSeen;

        /// <summary>
        /// The supported protocol types to use when attempting to connect to Steam.
        /// </summary>
        public ProtocolTypes ProtocolTypes { get; set; } = ProtocolTypes.Tcp;

        /// <summary>
        /// The server list provider to use.
        /// </summary>
        public IServerListProvider ServerListProvider
        {
            get => serverListProvider;
            set => serverListProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The Universe to connect to. This should always be <see cref="EUniverse.Public"/> unless
        /// you work at Valve and are using this internally. If this is you, hello there.
        /// </summary>
        public EUniverse Universe { get; set; } = EUniverse.Public;

        /// <summary>
        /// The base address of the Steam Web API to connect to.
        /// </summary>
        public Uri WebAPIBaseAddress
        {
            get => webAPIBaseAddress;
            set => webAPIBaseAddress = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The server list used for this configuration.
        /// If this configuration is used by multiple <see cref="SteamClient"/> instances, they all share the server list.
        /// </summary>
        public SmartCMServerList ServerList { get; }

    }
}
