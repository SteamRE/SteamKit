using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// An interface for persisting the server list for connection discovery
    /// </summary>
    public interface ServerListProvider
    {
        /// <summary>
        /// Ask a provider to fetch any servers that it has available
        /// </summary>
        /// <returns>A list of IPEndPoints representing servers</returns>
        Task<ICollection<IPEndPoint>> FetchServerList();

        /// <summary>
        /// Update the persistent list of endpoints
        /// </summary>
        /// <param name="endpoints">List of endpoints</param>
        Task UpdateServerList(IEnumerable<IPEndPoint> endpoints);
    }
}
