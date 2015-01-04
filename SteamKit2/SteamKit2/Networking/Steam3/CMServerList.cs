using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;

namespace SteamKit2.Networking.Steam3
{
    /// <summary>
    /// Currently marked quality of a server. All servers start off as Undetermined.
    /// </summary>
    public enum ServerQuality
    {
        /// <summary>
        /// No quality marked, or validity period expired.
        /// </summary>
        Undetermined = 0,

        /// <summary>
        /// Known good server.
        /// </summary>
        Good,

        /// <summary>
        /// Known bad server.
        /// </summary>
        Bad
    };

    /// <summary>
    /// Smart list of CM servers.
    /// </summary>
    public class SmartCMServerList
    {
        const int BaseWeighting = 100;
        const int GoodWeighting = 450;
        const int BadWeighting = 15;
        
		[System.Diagnostics.DebuggerDisplay("ServerInfo ({EndPoint}, Weighting {Weighting})")]
        class ServerInfo
        {
            public IPEndPoint EndPoint { get; set; }
            public int Weighting { get; set; }
            public DateTime LastWeightingChangeTimeUtc { get; set; }
        }

        internal SmartCMServerList()
        {
            servers = new Collection<ServerInfo>();
            weightingValidityTimeSpan = TimeSpan.FromMinutes(5);
        }

        Collection<ServerInfo> servers;
        TimeSpan weightingValidityTimeSpan;

        void ResetOldWeightings()
        {
            foreach(var serverInfo in servers)
            {
                if (serverInfo.LastWeightingChangeTimeUtc + weightingValidityTimeSpan <= DateTime.UtcNow)
                {
                    serverInfo.Weighting = BaseWeighting;
                }
            }
        }

        /// <summary>
        /// Adds an <see cref="System.Net.IPEndPoint" /> to the server list.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.IPEndPoint"/> to add.</param>
        /// <returns>false if the server is already in the list, true otherwise.</returns>
        public bool TryAdd(IPEndPoint endPoint)
        {
            if (servers.Where(x => x.EndPoint == endPoint).Any())
            {
                return false;
            }

            AddCore(endPoint);
            return true;
        }

        void Add(IPEndPoint endPoint)
        {
            if (servers.Where(x => x.EndPoint == endPoint).Any())
            {
                throw new ArgumentException("The supplied endpoint is already in the server list.", "endPoint");
            }

            AddCore(endPoint);
        }

        void AddCore(IPEndPoint endPoint)
        {
            var info = new ServerInfo
            {
                EndPoint = endPoint,
                Weighting = BaseWeighting,
                LastWeightingChangeTimeUtc = DateTime.UtcNow,
            };

            servers.Add(info);
        }

        /// <summary>
        /// Explicitly resets the quality of every stored server.
        /// </summary>
        public void ResetWeightings()
        {
            foreach(var server in servers)
            {
                SetServerQuality(server, ServerQuality.Undetermined);
            }
        }

        void Clear()
        {
            servers.Clear();
        }

        /// <summary>
        /// Loads a hardcoded list of servers.
        /// </summary>
        public void UseInbuiltList()
        {
            Clear();
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27020 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.202" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.202" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.202" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.203" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.203" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.203" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.204" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.204" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.204" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.205" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.205" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.64.200.205" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.9" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.9" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.9" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.10" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.10" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.10" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.11" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.11" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.11" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.12" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.12" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.12" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.13" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.13" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.13" ), 27019 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.14" ), 27017 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.14" ), 27018 ));
            Add(new IPEndPoint( IPAddress.Parse( "208.78.164.14" ), 27019 ));
        }

        /// <summary>
        /// Marks the server with the supplied <see cref="System.Net.IPEndPoint"/> with the given <see cref="ServerQuality"/>
        /// </summary>
        /// <param name="endPoint">The endpoint of the server to mark</param>
        /// <param name="quality">The new server quality</param>
        /// <exception cref="System.ArgumentException">The supplied endPoint does not represent a server in the list.</exception>
        public void Mark(IPEndPoint endPoint, ServerQuality quality)
        {
            var serverInfo = servers.Where(x => x.EndPoint.Equals(endPoint)).SingleOrDefault();
            if (serverInfo == null)
            {
                throw new ArgumentException("The supplied endpoint is not in the server list.", "endPoint");
            }

            SetServerQuality(serverInfo, quality);
        }

        static int GetWeighting(ServerQuality quality)
        {
            switch (quality)
            {
                case ServerQuality.Bad:
                    return BadWeighting;

                case ServerQuality.Good:
                    return GoodWeighting;

                case ServerQuality.Undetermined:
                    return BaseWeighting;

                default:
                    throw new ArgumentOutOfRangeException("quality");
            }
        }

        void SetServerQuality(ServerInfo serverInfo, ServerQuality quality)
        {
            serverInfo.Weighting = GetWeighting(quality);
            serverInfo.LastWeightingChangeTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Get the next server in the list, ordered by quality.
        /// </summary>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public IPEndPoint GetNextServer()
        {
            ResetOldWeightings();

            var totalWeightingValue = servers.Sum(x => x.Weighting);
            var randomValue = new Random().Next(totalWeightingValue);

            var weightingMarker = 0;
            foreach(var serverInfo in servers)
            {
                weightingMarker += serverInfo.Weighting;
                if (weightingMarker >= randomValue)
                {
                    return serverInfo.EndPoint;
                }
            }

            return null;
        }
    }
}
