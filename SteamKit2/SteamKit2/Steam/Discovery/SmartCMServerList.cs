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
        [DebuggerDisplay("ServerInfo ({Record.EndPoint})")]
        class ServerInfo
        {
            public ServerInfo( ServerRecord record )
            {
                Record = record;
                LastBadConnectionTimeMap = new Dictionary<ProtocolTypes, DateTime?>();

                foreach ( var protocolType in record.ProtocolTypes.GetFlags() )
                {
                    ResetLastBadConnectionTime( protocolType );
                }
            }

            public ServerRecord Record { get; set; }
            public Dictionary<ProtocolTypes, DateTime?> LastBadConnectionTimeMap { get; set; }

            public void ResetLastBadConnectionTime( ProtocolTypes protocol )
                => LastBadConnectionTimeMap[ protocol ] = null;

            public void SetLastBadConnectionTime( ProtocolTypes protocol, DateTime dateTime )
                => LastBadConnectionTimeMap[ protocol ] = dateTime;
        }

        /// <summary>
        /// Initialize SmartCMServerList with a given server list provider
        /// </summary>
        /// <param name="configuration">The Steam configuration to use.</param>
        /// <exception cref="ArgumentNullException">The configuration object is null.</exception>
        public SmartCMServerList( SteamConfiguration configuration )
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            servers = new Collection<ServerInfo>();
            listLock = new object();
            BadConnectionMemoryTimeSpan = TimeSpan.FromMinutes( 5 );
        }

        readonly SteamConfiguration configuration;

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
                listTask.GetAwaiter().GetResult();
                return true;
            }
            catch ( Exception ex )
            {
                DebugWrite( "Failed to retrieve server list: {0}", ex );
            }

            return false;
        }

        private async Task ResolveServerList()
        {
            DebugWrite( "Resolving server list" );

            IEnumerable<ServerRecord> serverList = await configuration.ServerListProvider.FetchServerListAsync().ConfigureAwait( false );
            IReadOnlyCollection<ServerRecord> endpointList = serverList.ToList();

            if ( endpointList.Count == 0 && configuration.AllowDirectoryFetch )
            {
                DebugWrite( "Server list provider had no entries, will query SteamDirectory" );
                endpointList = await SteamDirectory.LoadAsync( configuration ).ConfigureAwait( false );
            }

            if ( endpointList.Count == 0 && configuration.AllowDirectoryFetch )
            {
                DebugWrite( "Could not query SteamDirectory, falling back to cm0" );
                var cm0 = await Dns.GetHostAddressesAsync( "cm0.steampowered.com" ).ConfigureAwait( false );

                endpointList = cm0.Select( ipaddr => ServerRecord.CreateSocketServer( new IPEndPoint(ipaddr, 27015) ) ).ToList();
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
                    foreach ( var protocolType in serverInfo.LastBadConnectionTimeMap.Keys )
                    {
                        var lastBadConnectionTime = serverInfo.LastBadConnectionTimeMap[ protocolType ];
                        if ( lastBadConnectionTime.HasValue && lastBadConnectionTime.Value + BadConnectionMemoryTimeSpan < DateTime.UtcNow )
                        {
                            serverInfo.LastBadConnectionTimeMap[ protocolType ] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace the list with a new list of servers provided to us by the Steam servers.
        /// </summary>
        /// <param name="endpointList">The <see cref="ServerRecord"/>s to use for this <see cref="SmartCMServerList"/>.</param>
        public void ReplaceList( IEnumerable<ServerRecord> endpointList )
        {
            lock ( listLock )
            {
                var distinctEndPoints = endpointList.Distinct().ToArray();

                servers.Clear();

                for ( var i = 0; i < distinctEndPoints.Length; i++ )
                {
                    AddCore( distinctEndPoints[ i ] );
                }

                configuration.ServerListProvider.UpdateServerListAsync( distinctEndPoints ).GetAwaiter().GetResult();
            }
        }

        void AddCore( ServerRecord endPoint )
        {
            var info = new ServerInfo( endPoint );

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
                    foreach ( var protocolType in server.LastBadConnectionTimeMap.Keys )
                    {
                        server.ResetLastBadConnectionTime( protocolType );
                    }
                }
            }
        }

        internal bool TryMark( EndPoint endPoint, ProtocolTypes protocolTypes, ServerQuality quality )
        {
            lock ( listLock )
            {
                var serverInfo = servers.Where( x => x.Record.EndPoint.Equals( endPoint ) ).SingleOrDefault();
                if ( serverInfo == null )
                {
                    return false;
                }
                MarkServerCore( serverInfo, protocolTypes, quality );
                return true;
            }
        }

        void MarkServerCore( ServerInfo serverInfo, ProtocolTypes protocolTypes, ServerQuality quality )
        {
            foreach ( var protocol in protocolTypes.GetFlags() )
            {
                switch ( quality )
                {
                    case ServerQuality.Good:
                    {
                        serverInfo.ResetLastBadConnectionTime( protocol );
                        break;
                    }

                    case ServerQuality.Bad:
                    {
                        serverInfo.SetLastBadConnectionTime( protocol, DateTime.UtcNow );
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException( "quality" );
                }
            }
        }

        /// <summary>
        /// Perform the actual score lookup of the server list and return the candidate
        /// </summary>
        /// <returns>IPEndPoint candidate</returns>
        private ServerRecord GetNextServerCandidateInternal( ProtocolTypes supportedProtocolTypes )
        {
            lock ( listLock )
            {
                // ResetOldScores takes a lock internally, however
                // locks are re-entrant on the same thread, so this
                // isn't a problem.
                ResetOldScores();

                var query = 
                    from o in servers.Select((server, index) => new { server, index })
                    let server = o.server
                    let index = o.index
                    where server.Record.ProtocolTypes.HasFlagsFast( supportedProtocolTypes )
                    from protocol in server.LastBadConnectionTimeMap.Keys
                    where supportedProtocolTypes.HasFlagsFast( protocol )
                    let lastBadConnectionTime = server.LastBadConnectionTimeMap[ protocol ]
                    orderby lastBadConnectionTime.HasValue, index
                    select new { Record = server.Record, IsBad = lastBadConnectionTime.HasValue, Index = index, Protocol = protocol };
                var serverInfo = query.FirstOrDefault();
                
                if ( serverInfo == null )
                {
                    return null;
                }

                DebugWrite( $"Next server candidiate: {serverInfo.Record.EndPoint} ({serverInfo.Record.ProtocolTypes})" );
                return new ServerRecord( serverInfo.Record.EndPoint, serverInfo.Protocol );
            }
        }

        /// <summary>
        /// Get the next server in the list.
        /// </summary>
        /// <param name="supportedProtocolTypes">The minimum supported <see cref="ProtocolTypes"/> of the server to return.</param>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public ServerRecord GetNextServerCandidate( ProtocolTypes supportedProtocolTypes )
        {
            if ( !WaitForServersFetched() )
            {
                return null;
            }

            return GetNextServerCandidateInternal( supportedProtocolTypes );
        }

        /// <summary>
        /// Get the next server in the list.
        /// </summary>
        /// <param name="supportedProtocolTypes">The minimum supported <see cref="ProtocolTypes"/> of the server to return.</param>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public async Task<ServerRecord> GetNextServerCandidateAsync( ProtocolTypes supportedProtocolTypes )
        {
            StartFetchingServers();
            await listTask.ConfigureAwait( false );

            return GetNextServerCandidateInternal( supportedProtocolTypes );
        }

        /// <summary>
        /// Gets the <see cref="System.Net.IPEndPoint"/>s of all servers in the server list.
        /// </summary>
        /// <returns>An <see cref="T:System.Net.IPEndPoint[]"/> array contains the <see cref="System.Net.IPEndPoint"/>s of the servers in the list</returns>
        public ServerRecord[] GetAllEndPoints()
        {
            ServerRecord[] endPoints;

            if ( !WaitForServersFetched() )
            {
                return new ServerRecord[0];
            }

            lock ( listLock )
            {
                var numServers = servers.Count;
                endPoints = new ServerRecord[ numServers ];

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
