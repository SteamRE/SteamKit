using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        [DebuggerDisplay( "ServerInfo ({Record.EndPoint}, {Protocol}, Bad: {LastBadConnectionTimeUtc.HasValue})" )]
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
            this.configuration = configuration ?? throw new ArgumentNullException( nameof( configuration ) );
        }

        /// <summary>
        /// The default fallback Websockets server to attempt connecting to if fetching server list through other means fails.
        /// </summary>
        /// <remarks>
        /// If the default server set here no longer works, please create a pull request to update it.
        /// </remarks>
        public static string DefaultServerWebsocket { get; set; } = "cmp1-sea1.steamserver.net:443";

        /// <summary>
        /// The default fallback TCP/UDP server to attempt connecting to if fetching server list through other means fails.
        /// </summary>
        /// <remarks>
        /// If the default server set here no longer works, please create a pull request to update it.
        /// </remarks>
        public static string DefaultServerNetfilter { get; set; } = "ext1-sea1.steamserver.net:27017";

        readonly SteamConfiguration configuration;

        Task? listTask;

        object listLock = new();
        Collection<ServerInfo> servers = [];
        DateTime serversLastRefresh = DateTime.MinValue;

        private void StartFetchingServers()
        {
            lock ( listLock )
            {
                if ( servers.Count > 0 )
                {
                    // if the server list has been populated, check if it is still fresh
                    if ( DateTime.UtcNow - serversLastRefresh >= ServerListBeforeRefreshTimeSpan )
                    {
                        listTask = ResolveServerList( forceRefresh: true );
                    }
                    else
                    {
                        // no work needs to be done
                        listTask = Task.CompletedTask;
                    }
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
                DebugWrite( $"Failed to retrieve server list: {ex}" );
            }

            return false;
        }

        private async Task ResolveServerList( bool forceRefresh = false )
        {
            var providerRefreshTime = configuration.ServerListProvider.LastServerListRefresh;
            var alreadyTriedDirectoryFetch = false;

            // If this is the first time server list is being resolved,
            // check if the cache is old enough that requires refreshing from the API first
            if ( !forceRefresh && DateTime.UtcNow - providerRefreshTime >= ServerListBeforeRefreshTimeSpan )
            {
                forceRefresh = true;
            }

            // Server list can only be force refreshed if the API is allowed in the first place
            if ( forceRefresh && configuration.AllowDirectoryFetch )
            {
                DebugWrite( $"Querying {nameof( SteamDirectory )} for a fresh server list" );

                var directoryList = await SteamDirectory.LoadAsync( configuration ).ConfigureAwait( false );
                alreadyTriedDirectoryFetch = true;

                // Fresh server list has been loaded
                if ( directoryList.Count > 0 )
                {
                    DebugWrite( $"Resolved {directoryList.Count} servers from {nameof( SteamDirectory )}" );
                    ReplaceList( directoryList, writeProvider: true, DateTime.UtcNow );
                    return;
                }

                DebugWrite( $"Could not query {nameof( SteamDirectory )}, falling back to provider" );
            }
            else
            {
                DebugWrite( "Resolving server list using the provider" );
            }

            IEnumerable<ServerRecord> serverList = await configuration.ServerListProvider.FetchServerListAsync().ConfigureAwait( false );
            IReadOnlyCollection<ServerRecord> endpointList = serverList.ToList();

            // Provider server list is fresh enough and it provided servers
            if ( endpointList.Count > 0 )
            {
                DebugWrite( $"Resolved {endpointList.Count} servers from the provider" );
                ReplaceList( endpointList, writeProvider: false, providerRefreshTime );
                return;
            }

            // If API fetch is not allowed, bail out with no servers
            if ( !configuration.AllowDirectoryFetch )
            {
                DebugWrite( $"Server list provider had no entries, and {nameof( SteamConfiguration.AllowDirectoryFetch )} is false" );
                ReplaceList( [], writeProvider: false, DateTime.MinValue );
                return;
            }

            // If the force refresh tried to fetch the server list already, do not fetch it again
            if ( !alreadyTriedDirectoryFetch )
            {
                DebugWrite( $"Server list provider had no entries, will query {nameof( SteamDirectory )}" );
                endpointList = await SteamDirectory.LoadAsync( configuration ).ConfigureAwait( false );

                if ( endpointList.Count > 0 )
                {
                    DebugWrite( $"Resolved {endpointList.Count} servers from {nameof( SteamDirectory )}" );
                    ReplaceList( endpointList, writeProvider: true, DateTime.UtcNow );
                    return;
                }
            }

            // This is a last effort to attempt any valid connection to Steam
            DebugWrite( $"Server list provider had no entries, {nameof( SteamDirectory )} failed, falling back to default servers" );

            endpointList =
            [
                ServerRecord.CreateWebSocketServer( DefaultServerWebsocket ),
                ServerRecord.CreateDnsSocketServer( DefaultServerNetfilter ),
            ];

            ReplaceList( endpointList, writeProvider: false, DateTime.MinValue );
        }

        /// <summary>
        /// Determines how long the server list cache is used as-is before attempting to refresh from the Steam Directory.
        /// </summary>
        public TimeSpan ServerListBeforeRefreshTimeSpan { get; set; } = TimeSpan.FromDays( 7 );

        /// <summary>
        /// Determines how long a server's bad connection state is remembered for.
        /// </summary>
        public TimeSpan BadConnectionMemoryTimeSpan { get; set; } = TimeSpan.FromMinutes( 5 );

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
        /// <param name="writeProvider">If true, the replaced list will be updated in the server list provider.</param>
        /// <param name="serversTime">The time when the provided server list has been updated.</param>
        public void ReplaceList( IEnumerable<ServerRecord> endpointList, bool writeProvider = true, DateTime? serversTime = null )
        {
            ArgumentNullException.ThrowIfNull( endpointList );

            lock ( listLock )
            {
                var distinctEndPoints = endpointList.Distinct().ToArray();

                serversLastRefresh = serversTime ?? DateTime.UtcNow;
                servers.Clear();

                for ( var i = 0; i < distinctEndPoints.Length; i++ )
                {
                    AddCore( distinctEndPoints[ i ] );
                }

                if ( writeProvider )
                {
                    configuration.ServerListProvider.UpdateServerListAsync( distinctEndPoints ).GetAwaiter().GetResult();
                }
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
                ServerInfo[] serverInfos;

                if ( quality == ServerQuality.Good )
                {
                    serverInfos = servers.Where( x => x.Record.EndPoint.Equals( endPoint ) && x.Protocol.HasFlagsFast( protocolTypes ) ).ToArray();
                }
                else
                {
                    // If we're marking this server for any failure, mark all endpoints for the host at the same time
                    var host = NetHelpers.ExtractEndpointHost( endPoint );
                    serverInfos = servers.Where( x => x.Record.GetHost().Equals( host, StringComparison.Ordinal ) ).ToArray();
                }

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

        static void MarkServerCore( ServerInfo serverInfo, ServerQuality quality )
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
                    throw new ArgumentOutOfRangeException( nameof( quality ) );
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

                var result = servers
                    .Where( o => o.Protocol.HasFlagsFast( supportedProtocolTypes ) )
                    .Select( static ( server, index ) => (Server: server, Index: index) )
                    .OrderBy( static o => o.Server.LastBadConnectionTimeUtc.GetValueOrDefault() )
                    .ThenBy( static o => o.Index )
                    .Select( static o => o.Server )
                    .FirstOrDefault();

                if ( result == null )
                {
                    return null;
                }

                DebugWrite( $"Next server candidate: {result.Record.EndPoint} ({result.Protocol})" );
                return new ServerRecord( result.Record.EndPoint, result.Protocol );
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
                return [];
            }

            lock ( listLock )
            {
                endPoints = servers.Select( static s => s.Record ).Distinct().ToArray();
            }

            return endPoints;
        }

        /// <summary>
        /// Force refresh the server list. If directory fetch is allowed, it will refresh from the API first,
        /// and then fallback to the server list provider.
        /// </summary>
        /// <returns>Task to be awaited that refreshes the server list.</returns>
        public Task ForceRefreshServerList()
        {
            lock ( listLock )
            {
                listTask = ResolveServerList( forceRefresh: true );

                return listTask;
            }
        }

        static void DebugWrite( string msg )
        {
            DebugLog.WriteLine( "ServerList", msg );
        }
    }
}
