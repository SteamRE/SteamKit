/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Net.Http;
using SteamKit2.Discovery;

namespace SteamKit2
{
    /// <summary>
    /// Interface to configure a <see cref="SteamConfiguration" /> before it is created.
    /// A reference to the underlying object should not be live beyond the configurator function's scope.
    /// </summary>
    public interface ISteamConfigurationBuilder
    {
        /// <summary>
        /// Configures this <see cref="SteamConfiguration" /> for a particular Steam cell.
        /// </summary>
        /// <param name="cellID">The Steam Cell ID to prioritize when connecting.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithCellID(uint cellID);

        /// <summary>
        /// Configures this <see cref="SteamConfiguration" /> with a connection timeout.
        /// </summary>
        /// <param name="connectionTimeout">The connection timeout used when connecting to Steam serves.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithConnectionTimeout(TimeSpan connectionTimeout);

        /// <summary>
        /// Configures this <see cref="SteamConfiguration" /> with the default <see cref="EClientPersonaStateFlag"/>s to request from Steam.
        /// </summary>
        /// <param name="personaStateFlags">The default persona state flags used when requesting information for a new friend, or
        /// when calling <c>SteamFriends.RequestFriendInfo</c> without specifying flags.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithDefaultPersonaStateFlags(EClientPersonaStateFlag personaStateFlags);

        /// <summary>
        /// Configures this <see cref="SteamConfiguration" /> to discover available servers.
        /// </summary>
        /// <param name="allowDirectoryFetch">Whether or not to use the Steam Directory to discover available servers.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithDirectoryFetch(bool allowDirectoryFetch);

        /// <summary>
        /// Configures this <see cref="SteamConfiguration" /> with custom HTTP behaviour.
        /// </summary>
        /// <param name="factoryFunction">A function to create and configure a new HttpClient.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithHttpClientFactory(HttpClientFactory factoryFunction);

        /// <summary>
        /// Configures how this <see cref="SteamConfiguration" /> will be used to connect to Steam.
        /// </summary>
        /// <param name="protocolTypes">The supported protocol types to use when attempting to connect to Steam.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithProtocolTypes(ProtocolTypes protocolTypes);

        /// <summary>
        /// Configures the server list provider for this <see cref="SteamConfiguration" />.
        /// </summary>
        /// <param name="provider">The server list provider to use..</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithServerListProvider(IServerListProvider provider);

        /// <summary>
        /// Configures the Universe that this <see cref="SteamConfiguration" /> belongs to.
        /// </summary>
        /// <param name="universe">The Universe to connect to. This should always be <see cref="EUniverse.Public"/> unless
        /// you work at Valve and are using this internally. If this is you, hello there.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithUniverse(EUniverse universe);

        /// <summary>
        /// Configures the Steam Web API address for this <see cref="SteamConfiguration" />.
        /// </summary>
        /// <param name="baseAddress">The base address of the Steam Web API to connect to.
        /// Use of "partner.steam-api.com" requires a Partner API Key.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithWebAPIBaseAddress(Uri baseAddress);

        /// <summary>
        /// Configures this <see cref="SteamConfiguration" /> with a Web API key to attach to requests.
        /// </summary>
        /// <param name="webApiKey">An API key to be used for authorized requests.
        /// Keys can be obtained from https://steamcommunity.com/dev or the Steamworks Partner site.</param>
        /// <returns>A builder with modified configuration.</returns>
        ISteamConfigurationBuilder WithWebAPIKey(string webApiKey);
    }
}
