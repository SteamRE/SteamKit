using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2.Networking.Steam3
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

        void Clear()
        {
            servers.Clear();
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

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// This will replace any servers currently in the list.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        public Task LoadListFromDirectoryAsync( int cellid = 0 )
        {
            return LoadListFromDirectoryAsync( cellid, CancellationToken.None );
        }

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// This will replace any servers currently in the list.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        public Task LoadListFromDirectoryAsync( int cellid, CancellationToken cancellationToken )
        {
            var directory = new WebAPI.AsyncInterface( "ISteamDirectory", null );
            var args = new Dictionary<string, string>
            {
                { "cellid", cellid.ToString() }
            };

            cancellationToken.ThrowIfCancellationRequested();

            var task = directory.Call( "GetCMList", version: 1, args: args, secure: true );
            return task.ContinueWith(t =>
            {
                var response = task.Result;
                var result = ( EResult )response[ "result" ].AsInteger( ( int ) EResult.Invalid );
                if ( result != EResult.OK )
                {
                    throw new InvalidOperationException( string.Format( "Steam Web API returned EResult.{0}", Enum.GetName( typeof( EResult ), result ) ) );
                }

                var list = response[ "serverlist" ];

                cancellationToken.ThrowIfCancellationRequested();

                lock ( listLock )
                {
                    Clear();
                    foreach( var child in list.Children )
                    {
                        IPEndPoint endpoint;
                        if ( !NetHelpers.TryParseIPEndPoint( child.Value, out endpoint ) )
                        {
                            continue;
                        }

                        Add( endpoint );
                    }
                }
            }, cancellationToken, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current);
        }

        /// <summary>
        /// Marks the server with the supplied <see cref="System.Net.IPEndPoint"/> with the given <see cref="ServerQuality"/>
        /// </summary>
        /// <param name="endPoint">The endpoint of the server to mark</param>
        /// <param name="quality">The new server quality</param>
        /// <exception cref="System.ArgumentException">The supplied endPoint does not represent a server in the list.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The supplied <see cref="ServerQuality"/> is not a valid value.</exception>
        public void Mark( IPEndPoint endPoint, ServerQuality quality )
        {
            if ( !TryMark( endPoint, quality ) )
            {
                throw new ArgumentException( "The supplied endpoint is not in the server list.", "endPoint" );
            }
        }

        /// <summary>
        /// Marks the server with the supplied <see cref="System.Net.IPEndPoint"/> with the given <see cref="ServerQuality"/>
        /// </summary>
        /// <param name="endPoint">The endpoint of the server to mark</param>
        /// <param name="quality">The new server quality</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The supplied <see cref="ServerQuality"/> is not a valid value.</exception>
        /// <returns>True if the server exists in the list, false otherwise</returns>
        public bool TryMark( IPEndPoint endPoint, ServerQuality quality )
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
                    var newScore = Convert.ToInt32( serverInfo.Score * GoodWeighting );
                    SetServerScore( serverInfo, Math.Min( newScore, MaxScore ) );
                    break;
                }

                case ServerQuality.Bad:
                {
                    var newScore = Convert.ToInt32( serverInfo.Score * BadWeighting );
                    SetServerScore( serverInfo, Math.Max( newScore, MinScore ) );
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
        public IPEndPoint GetNextServer()
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
    }
}
