using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace SteamKit2
{
    /// <summary>
    /// Currently marked quality of a server. All servers start off as Undetermined.
    /// </summary>
    public enum ServerQuality
    {
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
        const int BaseScore = 1000;
        const float GoodWeighting = 1.2f;
        const float BadWeighting = 0.8f;
        const int MaxScore = 4000;
        const int MinScore = 250;
        
        [DebuggerDisplay("ServerInfo ({EndPoint}, Score {Score})")]
        class ServerInfo
        {
            public IPEndPoint EndPoint { get; set; }
            public int Score { get; set; }
            public DateTime LastScoreChangeTimeUtc { get; set; }
        }

        internal SmartCMServerList()
        {
            servers = new Collection<ServerInfo>();
            listLock = new object();
            ScoreExpiryTimeSpan = TimeSpan.FromMinutes( 30 );
        }

        object listLock;
        Collection<ServerInfo> servers;

        /// <summary>
        /// Determines after how much time a server's score should expire and be reset to it's base value.
        /// </summary>
        public TimeSpan ScoreExpiryTimeSpan
        {
            get;
            set;
        }

        /// <summary>
        /// Resets the scores of all servers which had their scores last updated a <see cref="ScoreExpiryTimeSpan"/> ago.
        /// </summary>
        public void ResetOldScores()
        {
            lock ( listLock )
            {
                foreach ( var serverInfo in servers )
                {
                    if ( serverInfo.LastScoreChangeTimeUtc + ScoreExpiryTimeSpan <= DateTime.UtcNow )
                    {
                        serverInfo.Score = BaseScore;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an <see cref="System.Net.IPEndPoint" /> to the server list.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.IPEndPoint"/> to add.</param>
        /// <returns>false if the server is already in the list, true otherwise.</returns>
        public bool TryAdd( IPEndPoint endPoint )
        {
            lock ( listLock )
            {
                if ( servers.Any( x => x.EndPoint.Equals( endPoint ) ) )
                {
                    return false;
                }

                AddCore( endPoint );
            }
            return true;
        }

        /// <summary>
        /// Adds the elements of the specified collection of <see cref="System.Net.IPEndPoint" />s to the server list.
        /// </summary>
        /// <param name="endPoints">The collection of <see cref="System.Net.IPEndPoint"/>s to add.</param>
        /// <returns>false if any of the specified servers are already in the list, true otherwise.</returns>
        public bool TryAddRange( IEnumerable<IPEndPoint> endPoints )
        {
            lock ( listLock )
            {
                var distinctEndPoints = endPoints.Distinct();

                var endpointsAlreadyInList = servers.Select( x => x.EndPoint );
                var overlappingEndPoints = endpointsAlreadyInList.Intersect( distinctEndPoints, EqualityComparer<IPEndPoint>.Default );
                if ( overlappingEndPoints.Any() )
                {
                    return false;
                }

                foreach ( var endPoint in distinctEndPoints )
                {
                    AddCore( endPoint );
                }
            }
            return true;
        }

        void Add( IPEndPoint endPoint )
        {
            if ( servers.Any( x => x.EndPoint == endPoint ) )
            {
                throw new ArgumentException( "The supplied endpoint is already in the server list.", "endPoint" );
            }

            AddCore( endPoint );
        }

        void AddCore( IPEndPoint endPoint )
        {
            var info = new ServerInfo
            {
                EndPoint = endPoint,
                Score = BaseScore,
                LastScoreChangeTimeUtc = DateTime.UtcNow,
            };

            servers.Add( info );
        }

        /// <summary>
        /// Explicitly resets the quality of every stored server.
        /// </summary>
        public void ResetAllScores()
        {
            lock ( listLock )
            {
                foreach ( var server in servers )
                {
                    SetServerScore( server, BaseScore );
                }
            }
        }

        /// <summary>
        /// Removes all servers from the list.
        /// </summary>
        public void Clear()
        {
            lock ( listLock )
            {
                servers.Clear();
            }
        }

        internal void UseInbuiltList()
        {
            lock ( listLock )
            {
                Clear();
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.201" ), 27020 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.202" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.202" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.202" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.203" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.203" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.203" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.204" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.204" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.204" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.205" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.205" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.64.200.205" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.9" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.9" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.9" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.10" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.10" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.10" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.11" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.11" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.11" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.12" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.12" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.12" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.13" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.13" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.13" ), 27019 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.14" ), 27017 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.14" ), 27018 ) );
                Add( new IPEndPoint( IPAddress.Parse( "208.78.164.14" ), 27019 ) );
            }
        }
        
        internal bool TryMark( IPEndPoint endPoint, ServerQuality quality )
        {
            lock ( listLock )
            {
                var serverInfo = servers.Where( x => x.EndPoint.Equals( endPoint ) ).SingleOrDefault();
                if ( serverInfo == null )
                {
                    return false;
                }
                MarkServerCore( serverInfo, quality );
                return true;
            }
        }

        void MarkServerCore( ServerInfo serverInfo, ServerQuality quality )
        {

            switch ( quality )
            {
                case ServerQuality.Good:
                {
                    var newScore = Math.Min( Convert.ToInt32( serverInfo.Score * GoodWeighting ), MaxScore );
                    if ( newScore > serverInfo.Score )
                    {
                        DebugWrite( "{0} is good - increasing score from {1} to {2}.", serverInfo.EndPoint, serverInfo.Score, newScore );
                        SetServerScore( serverInfo, newScore );
                    }
                    else
                    {
                        DebugWrite( "{0} is good but has hit the score ceiling of {1}.", serverInfo.EndPoint, MaxScore );
                    }
                    break;
                }

                case ServerQuality.Bad:
                {
                    var newScore = Math.Max( Convert.ToInt32( serverInfo.Score * BadWeighting ), MinScore );
                    if ( newScore < serverInfo.Score )
                    {
                        DebugWrite( "{0} is bad - dropping score from {1} to {2}.", serverInfo.EndPoint, serverInfo.Score, newScore );
                        SetServerScore( serverInfo, newScore );
                    }
                    else
                    {
                        DebugWrite( "{0} is bad but has hit the score floor of {1}.", serverInfo.EndPoint, MinScore );
                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException( "quality" );
            }
        }

        void SetServerScore( ServerInfo serverInfo, int score )
        {
            serverInfo.Score = score;
            serverInfo.LastScoreChangeTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Get the next server in the list.
        /// </summary>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public IPEndPoint GetNextServerCandidate()
        {
            lock ( listLock)
            {
                // ResetOldScores takes a lock internally, however
                // locks are re-entrant on the same thread, so this
                // isn't a problem.
                ResetOldScores();

                var totalScoreValue = servers.Sum( x => x.Score );
                var randomValue = new Random().Next( totalScoreValue );

                var scoreMarker = 0;
                foreach ( var serverInfo in servers )
                {
                    scoreMarker += serverInfo.Score;
                    if ( scoreMarker >= randomValue )
                    {
                        return serverInfo.EndPoint;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="System.Net.IPEndPoint"/>s of all servers in the server list.
        /// </summary>
        /// <returns>An <see cref="T:System.Net.IPEndPoint[]"/> array contains the <see cref="System.Net.IPEndPoint"/>s of the servers in the list</returns>
        public IPEndPoint[] GetAllEndPoints()
        {
            IPEndPoint[] endPoints;

            lock( listLock )
            {
                var numServers = servers.Count;
                endPoints = new IPEndPoint[ numServers ];

                for ( int i = 0; i < numServers; i++ )
                {
                    var serverInfo = servers[ i ];
                    endPoints[ i ] = serverInfo.EndPoint;
                }
            }

            return endPoints;
        }

        static void DebugWrite( string msg, params object[] args )
        {
            DebugLog.WriteLine( "ServerList", msg, args);
        }
    }
}
