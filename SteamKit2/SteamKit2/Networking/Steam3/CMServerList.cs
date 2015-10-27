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
        [DebuggerDisplay("ServerInfo ({EndPoint}, Bad: {LastBadConnectionDateTime.HasValue})")]
        class ServerInfo
        {
            public IPEndPoint EndPoint { get; set; }
            public DateTime? LastBadConnectionDateTimeUtc { get; set; }
        }

        internal SmartCMServerList()
        {
            servers = new Collection<ServerInfo>();
            listLock = new object();
            BadConnectionMemoryTimeSpan = TimeSpan.FromMinutes( 5 );
        }

        object listLock;
        Collection<ServerInfo> servers;

        /// <summary>
        /// Determines how long a server's bad connection state is remembered for.
        /// </summary>
        public TimeSpan BadConnectionMemoryTimeSpan
        {
            get;
            set;
        }

        /// <summary>
        /// Resets the scores of all servers which has a last bad connection more than <see cref="BadConnectionMemoryTimeSpan"/> ago.
        /// </summary>
        public void ResetOldScores()
        {
            lock ( listLock )
            {
                foreach ( var serverInfo in servers )
                {
                    if ( serverInfo.LastBadConnectionDateTimeUtc.HasValue && ( serverInfo.LastBadConnectionDateTimeUtc.Value + BadConnectionMemoryTimeSpan < DateTime.UtcNow ) )
                    {
                        serverInfo.LastBadConnectionDateTimeUtc = null;
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
        /// Adds the elements of the specified collection of <see cref="IPEndPoint" />s to the server list.
        /// </summary>
        /// <param name="endPoints">The collection of <see cref="IPEndPoint"/>s to add.</param>
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

        /// <summary>
        /// Merges the list with a new list of servers provided to us by the Steam servers.
        /// This adds the new list of <see cref="IPEndPoint"/>s to the beginning of the list,
        /// ensuring that any pre-existing servers are moved into their new place in order near
        /// the beginning of the list.
        /// </summary>
        /// <param name="listToMerge">The <see cref="IPEndPoint"/>s to merge into this <see cref="SmartCMServerList"/>.</param>
        public void MergeWithList( IEnumerable<IPEndPoint> listToMerge )
        {
            lock ( listLock )
            {
                var distinctEndPoints = listToMerge.Distinct().ToArray();
                var endpointsAlreadyInList = servers.Select( x => x.EndPoint );

                var preExistingServers = servers.Where( s => distinctEndPoints.Contains( s.EndPoint ) ).ToArray();

                // This will let us do a simpler insert, but will also reset the bad connection state.
                // If we were just told by Steam to use this CM, give it a second chance.
                foreach ( var serverInfo in preExistingServers )
                {
                    servers.Remove( serverInfo );
                }
                
                for ( var i = 0; i < distinctEndPoints.Length; i++ )
                {
                    AddCore( distinctEndPoints[ i ], i );
                }
            }
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
            AddCore( endPoint, servers.Count );
        }

        void AddCore( IPEndPoint endPoint, int index )
        {
            var info = new ServerInfo { EndPoint = endPoint };

            servers.Insert( index, info );
        }

        /// <summary>
        /// Explicitly resets the known state of all servers.
        /// </summary>
        public void ResetBadServers()
        {
            lock ( listLock )
            {
                foreach ( var server in servers )
                {
                    server.LastBadConnectionDateTimeUtc = null;
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
                    serverInfo.LastBadConnectionDateTimeUtc = null;
                    break;
                }

                case ServerQuality.Bad:
                {
                    serverInfo.LastBadConnectionDateTimeUtc = DateTime.UtcNow;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException( "quality" );
            }
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

                var serverInfo = servers
                    .Select( (s, index) => new { EndPoint = s.EndPoint, IsBad = s.LastBadConnectionDateTimeUtc.HasValue, Index = index } )
                    .OrderBy( x => x.IsBad )
                    .ThenBy( x => x.Index )
                    .FirstOrDefault();
                
                if ( serverInfo == null )
                {
                    return null;
                }

                DebugWrite( $"Next server candidiate: {serverInfo.EndPoint}" );
                return serverInfo.EndPoint;
            }
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
