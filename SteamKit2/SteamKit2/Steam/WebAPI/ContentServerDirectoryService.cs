using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.Internal;

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
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDN.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDN.Server>> LoadAsync( SteamConfiguration configuration )
            => LoadCoreAsync( configuration, null, null, CancellationToken.None );

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDN.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDN.Server>> LoadAsync( SteamConfiguration configuration, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, null, null, cancellationToken );

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cellId">Preferred steam cell id</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDN.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDN.Server>> LoadAsync( SteamConfiguration configuration, int cellId, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, cellId, null, cancellationToken );

        /// <summary>
        /// Load a list of servers from the Content Server Directory Service.
        /// You can use <see cref="SteamContent.GetServersForSteamPipe"></see> instead to go over a CM connection.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cellId">Preferred steam cell id</param>
        /// <param name="maxNumServers">Max number of servers to return.</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CDN.Server"/>s.</returns>
        public static Task<IReadOnlyCollection<CDN.Server>> LoadAsync( SteamConfiguration configuration, int cellId, int maxNumServers, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, cellId, maxNumServers, cancellationToken );

        static async Task<IReadOnlyCollection<CDN.Server>> LoadCoreAsync( SteamConfiguration configuration, int? cellId, int? maxNumServers, CancellationToken cancellationToken )
        {
            ArgumentNullException.ThrowIfNull( configuration );

            using var directory = configuration.GetAsyncWebAPIInterface( "IContentServerDirectoryService" );
            var args = new Dictionary<string, object?>();

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

            var response = await directory.CallProtobufAsync<CContentServerDirectory_GetServersForSteamPipe_Response>(
                HttpMethod.Get,
                nameof( IContentServerDirectory.GetServersForSteamPipe ),
                version: 1,
                args: args
            ).ConfigureAwait( false );

            cancellationToken.ThrowIfCancellationRequested();

            return ConvertServerList( response );
        }

        internal static IReadOnlyCollection<CDN.Server> ConvertServerList( CContentServerDirectory_GetServersForSteamPipe_Response response )
        {
            var serverRecords = new List<CDN.Server>( capacity: response.servers.Count );

            foreach ( var child in response.servers )
            {
                var httpsSupport = child.https_support;
                var protocol = httpsSupport == "mandatory" ? CDN.Server.ConnectionProtocol.HTTPS : CDN.Server.ConnectionProtocol.HTTP;

                serverRecords.Add( new CDN.Server
                {
                    Protocol = protocol,
                    Host = child.host,
                    VHost = child.vhost,
                    Port = protocol == CDN.Server.ConnectionProtocol.HTTPS ? 443 : 80,

                    Type = child.type,
                    SourceID = child.source_id,
                    CellID = ( uint )child.cell_id,

                    Load = child.load,
                    WeightedLoad = child.weighted_load,
                    NumEntries = child.num_entries_in_client_list,
                    SteamChinaOnly = child.steam_china_only,

                    UseAsProxy = child.use_as_proxy,
                    ProxyRequestPathTemplate = child.proxy_request_path_template,

                    AllowedAppIds = child.allowed_app_ids.ToArray(),
                }
                );
            }

            return serverRecords.AsReadOnly();
        }
    }
}
