/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using SteamKit2.Discovery;

namespace SteamKit2
{
    sealed class SteamConfigurationBuilder : ISteamConfigurationBuilder
    {
        public SteamConfigurationBuilder()
        {
            state = CreateDefaultState();
        }

        public static SteamConfigurationState CreateDefaultState()
        {
            return new SteamConfigurationState
            {
                AllowDirectoryFetch = true,

                ConnectionTimeout = TimeSpan.FromSeconds(5),

                DefaultPersonaStateFlags =
                    EClientPersonaStateFlag.PlayerName | EClientPersonaStateFlag.Presence |
                    EClientPersonaStateFlag.SourceID | EClientPersonaStateFlag.GameExtraInfo |
                    EClientPersonaStateFlag.LastSeen,

                ProtocolTypes = ProtocolTypes.Tcp,

                ServerListProvider = new NullServerListProvider(),

                Universe = EUniverse.Public,

                WebAPIBaseAddress = WebAPI.DefaultBaseAddress
            };
        }

        SteamConfigurationState state;

        public SteamConfiguration Build()
            => new SteamConfiguration(state);

        public ISteamConfigurationBuilder WithCellID(uint cellID)
        {
            state.CellID = cellID;
            return this;
        }

        public ISteamConfigurationBuilder WithConnectionTimeout(TimeSpan connectionTimeout)
        {
            state.ConnectionTimeout = connectionTimeout;
            return this;
        }

        public ISteamConfigurationBuilder WithDefaultPersonaStateFlags(EClientPersonaStateFlag personaStateFlags)
        {
            state.DefaultPersonaStateFlags = personaStateFlags;
            return this;
        }

        public ISteamConfigurationBuilder WithDirectoryFetch(bool allowDirectoryFetch)
        {
            state.AllowDirectoryFetch = allowDirectoryFetch;
            return this;
        }

        public ISteamConfigurationBuilder WithProtocolTypes(ProtocolTypes protocolTypes)
        {
            state.ProtocolTypes = protocolTypes;
            return this;
        }

        public ISteamConfigurationBuilder WithServerListProvider(IServerListProvider provider)
        {
            state.ServerListProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            return this;
        }

        public ISteamConfigurationBuilder WithUniverse(EUniverse universe)
        {
            state.Universe = universe;
            return this;
        }

        public ISteamConfigurationBuilder WithWebAPIBaseAddress(Uri baseAddress)
        {
            state.WebAPIBaseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
            return this;
        }

        public ISteamConfigurationBuilder WithWebAPIKey(string webApiKey)
        {
            state.WebAPIKey = webApiKey ?? throw new ArgumentNullException(nameof(webApiKey));
            return this;
        }
    }
}
