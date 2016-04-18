using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// A server list provider that returns an empty list, for consumers that populate the server list themselves
    /// </summary>
    public class NullServerListProvider : ServerListProvider
    {
        /// <summary>
        /// No-op implementation that returns an empty server list
        /// </summary>
        /// <returns>Empty server list</returns>
        public Task<ICollection<IPEndPoint>> FetchServerList()
        {
            ICollection<IPEndPoint> empty = new List<IPEndPoint>();
            return Task.FromResult(empty);
        }

        /// <summary>
        /// No-op implementation that does not persist server list
        /// </summary>
        /// <param name="endpoints">Server list</param>
        /// <returns>Completed task</returns>
        public Task UpdateServerList(IEnumerable<IPEndPoint> endpoints)
        {
            return Task.FromResult(0);
        }
    }
}
