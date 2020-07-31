using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.Discovery;

namespace SteamKit2
{
    /// <summary>
    /// Helper class to load servers from the Content Server Directory Service Web API.
    /// </summary>
    public static class ContentServerDirectoryService
    {
        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDNClient.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDNClient.Server>> LoadAsync( SteamConfiguration configuration )
            => LoadCoreAsync( configuration, null, null, CancellationToken.None );

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDNClient.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDNClient.Server>> LoadAsync( SteamConfiguration configuration, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, null, null, cancellationToken );

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cellId">Preferred steam cell id</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDNClient.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDNClient.Server>> LoadAsync( SteamConfiguration configuration, int cellId, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, cellId, null, cancellationToken );

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cellId">Preferred steam cell id</param>
        /// <param name="maxNumServers">Max number of servers to return.</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDNClient.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDNClient.Server>> LoadAsync( SteamConfiguration configuration, int cellId, int maxNumServers, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, cellId, maxNumServers, cancellationToken );

        static async Task<IReadOnlyCollection<CDNClient.Server>> LoadCoreAsync( SteamConfiguration configuration, int? cellId, int? maxNumServers, CancellationToken cancellationToken )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            var directory = configuration.GetAsyncWebAPIInterface( "IContentServerDirectoryService" );
            var args = new Dictionary<string, object>();

            if ( cellId.HasValue )
            {
                args[ "cell_id" ] = cellId.Value.ToString( CultureInfo.InvariantCulture );
            }
            else
            {
                args[ "cell_id" ] = configuration.CellID.ToString( CultureInfo.InvariantCulture );
            }

            if ( maxNumServers.HasValue )
            {
                args[ "max_servers" ] = maxNumServers.Value.ToString( CultureInfo.InvariantCulture );
            }

            cancellationToken.ThrowIfCancellationRequested();

            var response = await directory.CallAsync( HttpMethod.Get, "GetServersForSteamPipe", version: 1, args: args ).ConfigureAwait( false );

            var result = ( EResult )response[ "result" ].AsInteger( ( int )EResult.OK );
            if ( result != EResult.OK || response["servers"] == KeyValue.Invalid )
            {
                throw new InvalidOperationException( string.Format( "Steam Web API returned EResult.{0}", result ) );
            }

            var serverList = response[ "servers" ];

            cancellationToken.ThrowIfCancellationRequested();

            var serverRecords = new List<CDNClient.Server>( capacity: serverList.Children.Count );

            foreach ( var child in serverList.Children )
            {
                var httpsSupport = child[ "https_support" ].AsString();
                var protocol = ( httpsSupport == "optional" || httpsSupport == "mandatory" ) ? CDNClient.Server.ConnectionProtocol.HTTPS : CDNClient.Server.ConnectionProtocol.HTTP;

                serverRecords.Add( new CDNClient.Server
                {
                    Protocol = protocol,
                    Host = child[ "host" ].AsString(),
                    VHost = child[ "vhost" ].AsString(),
                    Port = protocol == CDNClient.Server.ConnectionProtocol.HTTPS ? 443 : 80,

                    Type = child[ "type" ].AsString(),
                    SourceID = child[ "source_id"].AsInteger(),
                    CellID = (uint)child[ "cell_id" ].AsInteger(),

                    Load = child[ "load" ].AsInteger(),
                    WeightedLoad = child[ "weighted_load" ].AsInteger(),
                    NumEntries = child[ "num_entries_in_client_list" ].AsInteger( 1 )
                } 
                );
            }

            return serverRecords.AsReadOnly();
        }
    }
}
