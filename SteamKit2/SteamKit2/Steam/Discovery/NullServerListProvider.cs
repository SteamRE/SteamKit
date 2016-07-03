using System.Collections.Generic;
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
        public Task<ICollection<IPEndPoint>> FetchServerListAsync()
        {
            ICollection<IPEndPoint> empty = new List<IPEndPoint>();
            return Task.FromResult(empty);
        }

        /// <summary>
        /// No-op implementation that does not persist server list
        /// </summary>
        /// <param name="endpoints">Server list</param>
        /// <returns>Completed task</returns>
        public Task UpdateServerListAsync(IEnumerable<IPEndPoint> endpoints)
        {
            return Task.Delay(0);
        }
    }
}
