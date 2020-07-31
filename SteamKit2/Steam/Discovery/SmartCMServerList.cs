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
        [DebuggerDisplay("ServerInfo ({EndPoint}, {Protocol}, Bad: {LastBadConnectionDateTimeUtc.HasValue})")]
        class ServerInfo
        {
            public ServerInfo( ServerRecord record, ProtocolTypes protocolType )
            {
                Record = record;
                Protocol = protocolType;
            }

            public ServerRecord Record { get; }
            public ProtocolTypes Protocol { get; }
            public DateTime? LastBadConnectionTimeUtc { get; set; }
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

        Task? listTask;

        object listLock;
        Collection<ServerInfo> servers;

        private void StartFetchingServers()
        {
            lock ( listLock )
            {
                // if the server list has been populated, no need to perform any additional work
                if ( servers.Count > 0 )
                {
                    listTask = Task.CompletedTask;
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
                listTask!.GetAwaiter().GetResult();
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

                endpointList = cm0.Select( ipaddr => ServerRecord.CreateSocketServer( new IPEndPoint(ipaddr, 27017) ) ).ToList();
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
            var cutoff = DateTime.UtcNow - BadConnectionMemoryTimeSpan;

            lock ( listLock )
            {
                foreach ( var serverInfo in servers )
                {
                    if ( serverInfo.LastBadConnectionTimeUtc.HasValue && serverInfo.LastBadConnectionTimeUtc.Value < cutoff )
                    {
                        serverInfo.LastBadConnectionTimeUtc = null;
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
            if ( endpointList == null )
            {
                throw new ArgumentNullException( nameof(endpointList) );
            }

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
            foreach ( var protocolType in endPoint.ProtocolTypes.GetFlags() )
            {
                var info = new ServerInfo( endPoint, protocolType );
                servers.Add( info );
            }
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
                    if ( server.LastBadConnectionTimeUtc.HasValue )
                    {
                        server.LastBadConnectionTimeUtc = null;
                    }
                }
            }
        }

        internal bool TryMark( EndPoint endPoint, ProtocolTypes protocolTypes, ServerQuality quality )
        {
            lock ( listLock )
            {
                var serverInfos = servers.Where( x => x.Record.EndPoint.Equals( endPoint ) && x.Protocol.HasFlagsFast( protocolTypes ) ).ToArray();
                if ( serverInfos.Length == 0 )
                {
                    return false;
                }

                foreach ( var serverInfo in serverInfos )
                {
                    MarkServerCore( serverInfo, quality );
                }
                return true;
            }
        }

        void MarkServerCore( ServerInfo serverInfo, ServerQuality quality )
        {
            switch ( quality )
            {
                case ServerQuality.Good:
                {
                    if ( serverInfo.LastBadConnectionTimeUtc.HasValue )
                    {
                        serverInfo.LastBadConnectionTimeUtc = null;
                    }
                    break;
                }

                case ServerQuality.Bad:
                {
                        serverInfo.LastBadConnectionTimeUtc = DateTime.UtcNow;
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
        private ServerRecord? GetNextServerCandidateInternal( ProtocolTypes supportedProtocolTypes )
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
                    where server.Protocol.HasFlagsFast( supportedProtocolTypes )
                    let lastBadConnectionTime = server.LastBadConnectionTimeUtc
                    orderby lastBadConnectionTime.HasValue, index
                    select new { EndPoint = server.Record.EndPoint, Protocol = server.Protocol };
                var result = query.FirstOrDefault();
                
                if ( result == null )
                {
                    return null;
                }

                DebugWrite( $"Next server candidiate: {result.EndPoint} ({result.Protocol})" );
                return new ServerRecord( result.EndPoint, result.Protocol );
            }
        }

        /// <summary>
        /// Get the next server in the list.
        /// </summary>
        /// <param name="supportedProtocolTypes">The minimum supported <see cref="ProtocolTypes"/> of the server to return.</param>
        /// <returns>An <see cref="System.Net.IPEndPoint"/>, or null if the list is empty.</returns>
        public ServerRecord? GetNextServerCandidate( ProtocolTypes supportedProtocolTypes )
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
        public async Task<ServerRecord?> GetNextServerCandidateAsync( ProtocolTypes supportedProtocolTypes )
        {
            StartFetchingServers();
            await listTask!.ConfigureAwait( false );

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
                endPoints = servers.Select(s => s.Record).Distinct().ToArray();
            }

            return endPoints;
        }

        static void DebugWrite( string msg, params object[] args )
        {
            DebugLog.WriteLine( "ServerList", msg, args);
        }
    }
}
