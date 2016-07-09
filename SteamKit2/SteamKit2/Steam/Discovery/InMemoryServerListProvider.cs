using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
{
	/// <summary>
	/// A server list provider that uses in-memory collection to persist the server list.
	/// It allows you to implement your own saving logic on <see cref="ServerListUpdated"/> event.
	/// </summary>
	public class InMemoryServerListProvider : IServerListProvider
	{
		/// <summary>
		/// Actual collection used for keeping server list in memory.
		/// </summary>
		public HashSet<IPEndPoint> Servers { get; set; } = new HashSet<IPEndPoint>();

		/// <summary>
		/// Event being fired after <see cref="UpdateServerListAsync(IEnumerable{IPEndPoint})"/> gets executed.
		/// You should use it for serializing <see cref="Servers"/> field for further re-use.
		/// </summary>
		public event EventHandler ServerListUpdated = delegate { };

		/// <summary>
		/// Returns servers from <see cref="Servers"/> collection.
		/// </summary>
		/// <returns>A list of IPEndPoints representing servers</returns>
		public Task<IEnumerable<IPEndPoint>> FetchServerListAsync() => Task.FromResult<IEnumerable<IPEndPoint>>(Servers);

		/// <summary>
		/// Updates <see cref="Servers"/> collection. Fires <see cref="ServerListUpdated"/> event once done.
		/// </summary>
		/// <param name="endpoints">List of server endpoints</param>
		/// <returns>Awaitable task for completion</returns>
		public Task UpdateServerListAsync(IEnumerable<IPEndPoint> endpoints)
		{
			Servers.Clear();
			foreach (IPEndPoint endpoint in endpoints)
			{
				Servers.Add(endpoint);
			}

			ServerListUpdated(this, EventArgs.Empty);

			return Task.Delay(0);
		}
	}
}
