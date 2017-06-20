using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that returns an empty list, for consumers that populate the server list themselves
    /// </summary>
    public class NullServerListProvider : IServerListProvider
    {
        /// <summary>
        /// No-op implementation that returns an empty server list
        /// </summary>
        /// <returns>Empty server list</returns>
        public Task<IEnumerable<CMServerRecord>> FetchServerListAsync()
            => Task.FromResult(Enumerable.Empty<CMServerRecord>());

        /// <summary>
        /// No-op implementation that does not persist server list
        /// </summary>
        /// <param name="endpoints">Server list</param>
        /// <returns>Completed task</returns>
        public Task UpdateServerListAsync(IEnumerable<CMServerRecord> endpoints)
            => Task.CompletedTask;
    }
}
