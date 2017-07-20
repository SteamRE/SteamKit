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
