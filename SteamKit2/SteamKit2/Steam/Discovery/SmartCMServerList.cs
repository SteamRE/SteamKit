using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SteamKit2.Discovery
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
        [DebuggerDisplay("ServerInfo ({EndPoint}, Bad: {LastBadConnectionDateTimeUtc.HasValue})")]
        class ServerInfo
        {
            public CMServerRecord Record { get; set; }
            public DateTime? LastBadConnectionDateTimeUtc { get; set; }
        }

        /// <summary>
        /// Initialize SmartCMServerList with a given server list provider
        /// </summary>
        /// <param name="provider">The ServerListProvider to persist servers</param>
        /// <param name="allowDirectoryFetch">Specifies if we can query SteamDirectory to discover more servers</param>
        public SmartCMServerList( IServerListProvider provider, bool allowDirectoryFetch = true )
        {
            ServerListProvider = provider;
            canFetchDirectory = allowDirectoryFetch;

            servers = new Collection<ServerInfo>();
            listLock = new object();
            BadConnectionMemoryTimeSpan = TimeSpan.FromMinutes( 5 );
        }

        /// <summary>
        /// Initialize SmartCMServerList with the default <see cref="NullServerListProvider"/> server list provider
        /// </summary>
        public SmartCMServerList() :
            this( new NullServerListProvider() )
        {
        }

        /// <summary>
        /// The server list provider chosen to provide a persistent list of servers to connect to
        /// </summary>
        public IServerListProvider ServerListProvider
        {
            get;
            set;
        }

        /// <summary>
        /// The preferred cell id for retrieving the list of servers from the Steam directory
        /// </summary>
        public uint CellID
        {
            get;
            set;
        }

        bool canFetchDirectory;
        Task listTask;

        object listLock;
        Collection<ServerInfo> servers;

        private void StartFetchingServers()
        {
            lock ( listLock )
            {
                // if the server list has been populated, no need to perform any additional work
                if ( servers.Count > 0 )
                {
                    listTask = Task.Delay( 0 );
                }
                else if ( listTask == null || listTask.IsFaulted || listTask.IsCanceled )
                {
                    listTask = ResolveServerList();
                }
            }
        }

        private bool WaitForServersFetched()
        {
            StartFetchingServers();

            try
            {
                listTask.Wait();
                return true;
            }
            catch ( AggregateException ae )
            {
                foreach( var ex in ae.Flatten().InnerExceptions )
                {
                    DebugWrite( "Failed to retrieve server list: {0}", ex );
                }
            }

            return false;
        }

        private async Task ResolveServerList()
        {
            DebugWrite( "Resolving server list" );

            IEnumerable<CMServerRecord> serverList = await ServerListProvider.FetchServerListAsync().ConfigureAwait( false );
            List<CMServerRecord> endpointList = serverList.ToList();

            if ( endpointList.Count == 0 && canFetchDirectory )
            {
                DebugWrite( "Server list provider had no entries, will query SteamDirectory" );
                var directoryList = await SteamDirectory.LoadAsync( CellID ).ConfigureAwait( false );

                endpointList = directoryList.Select(CMServerRecord.SocketServer).ToList();
            }

            if ( endpointList.Count == 0 && canFetchDirectory )
            {
                DebugWrite( "Could not query SteamDirectory, falling back to cm0" );
                var cm0 = await Dns.GetHostAddressesAsync( "cm0.steampowered.com" ).ConfigureAwait( false );

                endpointList = cm0.Select( ipaddr => CMServerRecord.SocketServer( new IPEndPoint(ipaddr, 27015) ) ).ToList();
            }

            DebugWrite( "Resolved {0} servers", endpointList.Count );
            ReplaceList( endpointList );
        }

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
        /// Replace the list with a new list of servers provided to us by the Steam servers.
        /// </summary>
        /// <param name="endpointList">The <see cref="CMServerRecord"/>s to use for this <see cref="SmartCMServerList"/>.</param>
        public void ReplaceList( IEnumerable<CMServerRecord> endpointList )
        {
            lock ( listLock )
            {
                var distinctEndPoints = endpointList.Distinct().ToArray();

                servers.Clear();

                for ( var i = 0; i < distinctEndPoints.Length; i++ )
                {
                    AddCore( distinctEndPoints[ i ] );
                }

                ServerListProvider.UpdateServerListAsync( distinctEndPoints ).Wait();
            }
        }

        void AddCore(CMServerRecord endPoint )
        {
            var info = new ServerInfo { Record = endPoint };

            servers.Add( info );
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

        internal bool TryMark( EndPoint endPoint, ServerQuality quality )
        {
            lock ( listLock )
            {
                var serverInfo = servers.Where( x => x.Record.EndPoint.Equals( endPoint ) ).SingleOrDefault();
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
        /// Perform the actual score lookup of the server list and return the candidate
        /// </summary>
        /// <returns>IPEndPoint candidate</returns>
        private CMServerRecord GetNextServerCandidateInternal()
        {
            lock ( listLock )
            {
                // ResetOldScores takes a lock internally, however
                // locks are re-entrant on the same thread, so this
                // isn't a problem.
                ResetOldScores();

                var serverInfo = servers
                    .Select( (s, index) => new { EndPoint = s.Record, IsBad = s.LastBadConnectionDateTimeUtc.HasValue, Index = index } )
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
        /// Get the next server in the list.
        /// </summary>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public CMServerRecord GetNextServerCandidate()
        {
            if ( !WaitForServersFetched() )
            {
                return null;
            }

            return GetNextServerCandidateInternal();
        }

        /// <summary>
        /// Get the next server in the list.
        /// </summary>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public async Task<CMServerRecord> GetNextServerCandidateAsync()
        {
            StartFetchingServers();
            await listTask.ConfigureAwait( false );

            return GetNextServerCandidateInternal();
        }

        /// <summary>
        /// Gets the <see cref="System.Net.IPEndPoint"/>s of all servers in the server list.
        /// </summary>
        /// <returns>An <see cref="T:System.Net.IPEndPoint[]"/> array contains the <see cref="System.Net.IPEndPoint"/>s of the servers in the list</returns>
        public CMServerRecord[] GetAllEndPoints()
        {
            CMServerRecord[] endPoints;

            if ( !WaitForServersFetched() )
            {
                return new CMServerRecord[0];
            }

            lock ( listLock )
            {
                var numServers = servers.Count;
                endPoints = new CMServerRecord[ numServers ];

                for ( int i = 0; i < numServers; i++ )
                {
                    var serverInfo = servers[ i ];
                    endPoints[ i ] = serverInfo.Record;
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
