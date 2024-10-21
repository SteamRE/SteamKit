using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that uses an in-memory list
    /// </summary>
    public class MemoryServerListProvider : IServerListProvider
    {
        private IEnumerable<ServerRecord> _servers = [];
        private DateTime _lastUpdated = DateTime.MinValue;

        /// <summary>
        /// Returns the last time the server list was updated
        /// </summary>
        public DateTime LastServerListRefresh => _lastUpdated;

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
            _lastUpdated = DateTime.UtcNow;

            return Task.CompletedTask;
        }
    }
}
