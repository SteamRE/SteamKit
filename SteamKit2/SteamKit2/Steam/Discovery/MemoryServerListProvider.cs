using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that uses an in-memory list
    /// </summary>
    public class MemoryServerListProvider : IServerListProvider
    {
        private IEnumerable<ServerRecord> _servers = Enumerable.Empty<ServerRecord>();

        /// <summary>
        /// Returns the stored server list in memory
        /// </summary>
        /// <returns>List of servers if persisted, otherwise an empty list</returns>
        public Task<IEnumerable<ServerRecord>> FetchServerListAsync()
            => Task.FromResult( _servers );

        /// <summary>
        /// Stores the supplied list of servers in memory
        /// </summary>
        /// <param name="endpoints">Server list</param>
        /// <returns>Completed task</returns>
        public Task UpdateServerListAsync( IEnumerable<ServerRecord> endpoints )
        {
            _servers = endpoints;

            return Task.CompletedTask;
        }
    }
}
