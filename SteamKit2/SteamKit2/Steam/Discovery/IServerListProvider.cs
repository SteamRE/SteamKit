using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// An interface for persisting the server list for connection discovery
    /// </summary>
    public interface IServerListProvider
    {
        /// <summary>
        /// When the server list was last refreshed, used to determine if the server list should be refreshed from the Steam Directory
        /// </summary>
        /// <remarks>
        /// This should return DateTime with the UTC kind
        /// </remarks>
        DateTime LastServerListRefresh { get; }

        /// <summary>
        /// Ask a provider to fetch any servers that it has available
        /// </summary>
        /// <returns>A list of IPEndPoints representing servers</returns>
        Task<IEnumerable<ServerRecord>> FetchServerListAsync();

        /// <summary>
        /// Update the persistent list of endpoints
        /// </summary>
        /// <param name="endpoints">List of endpoints</param>
        Task UpdateServerListAsync(IEnumerable<ServerRecord> endpoints);
    }
}
