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
        class ServerInfoComparer : IComparer<ServerInfo>
        {
            public ServerInfoComparer(TimeSpan timeSpanToRespectQualityFor)
            {
                this.timeSpanToRespectQualityFor = timeSpanToRespectQualityFor;
                this.serverQualityComparer = new ServerQualityComparer();
            }

            readonly TimeSpan timeSpanToRespectQualityFor;
            readonly IComparer<ServerQuality> serverQualityComparer;

            public int Compare(ServerInfo x, ServerInfo y)
            {
                var xQuality = GetQuality(x);
                var yQuality = GetQuality(y);

                return serverQualityComparer.Compare(xQuality, yQuality);
            }

            ServerQuality GetQuality(ServerInfo info)
            {
                // Only use the stored quality for the validity period.
                if (info.LastQualityChangeTimeUtc.Add(timeSpanToRespectQualityFor) > DateTime.UtcNow)
                {
                    return info.Quality;
                }

                return ServerQuality.Undetermined;
            }
        }

        class ServerQualityComparer : IComparer<ServerQuality>
        {
            // Good > Undetermined > Bad
            public int Compare(ServerQuality x, ServerQuality y)
            {
                if (x == y)
                {
                    return 0;
                }
                else if (x == ServerQuality.Good && (y == ServerQuality.Undetermined || y == ServerQuality.Bad))
                {
                    return -1;
                }
                else if (y == ServerQuality.Good && (x == ServerQuality.Undetermined || x == ServerQuality.Bad))
                {
                    return 1;
                }
                else if (x == ServerQuality.Undetermined && y == ServerQuality.Bad)
                {
                    return -1;
                }
                else if (y == ServerQuality.Undetermined && x == ServerQuality.Bad)
                {
                    return 1;
                }
                else // Probably invalid enum values
                {
                    throw new ArgumentOutOfRangeException("x or y is out of range");
                }
            }
        }

        class ServerInfo
        {
            public IPEndPoint EndPoint { get; set; }
            public ServerQuality Quality { get; set; }
            public DateTime LastQualityChangeTimeUtc { get; set; }
        }

        internal SmartCMServerList()
        {
            servers = new Collection<ServerInfo>();
            qualityValidityTimeSpan = TimeSpan.FromMinutes(5);
        }

        Collection<ServerInfo> servers;
        TimeSpan qualityValidityTimeSpan;

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
                Quality = ServerQuality.Undetermined,
                LastQualityChangeTimeUtc = DateTime.UtcNow,
            };

            servers.Add(info);
        }

        /// <summary>
        /// Explicitly resets the quality of every stored server.
        /// </summary>
        public void ResetQualitys()
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

        void SetServerQuality(ServerInfo serverInfo, ServerQuality quality)
        {
            serverInfo.Quality = quality;
            serverInfo.LastQualityChangeTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Get the next server in the list, ordered by quality.
        /// </summary>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public IPEndPoint GetNextServer()
        {
            var serverComparer = new ServerInfoComparer(qualityValidityTimeSpan);
            return servers
                .OrderBy(x => x, serverComparer)
                .Select(x => x.EndPoint)
                .FirstOrDefault();
        }
    }
}
