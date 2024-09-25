using System;
using System.Collections.Generic;
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
        public static Task<IReadOnlyCollection<CDN.Server>> LoadAsync( SteamConfiguration configuration, uint cellId, CancellationToken cancellationToken )
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
        public static Task<IReadOnlyCollection<CDN.Server>> LoadAsync( SteamConfiguration configuration, uint cellId, uint maxNumServers, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, cellId, maxNumServers, cancellationToken );

        static async Task<IReadOnlyCollection<CDN.Server>> LoadCoreAsync( SteamConfiguration configuration, uint? cellId, uint? maxNumServers, CancellationToken cancellationToken )
        {
            ArgumentNullException.ThrowIfNull( configuration );

            using var directory = configuration.GetAsyncWebAPIInterface( "IContentServerDirectoryService" );
            var request = new CContentServerDirectory_GetServersForSteamPipe_Request();

            if ( cellId.HasValue )
            {
                request.cell_id = cellId.Value;
            }
            else
            {
                request.cell_id = configuration.CellID;
            }

            if ( maxNumServers.HasValue )
            {
                request.max_servers = maxNumServers.Value;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var response = await directory.CallProtobufAsync<CContentServerDirectory_GetServersForSteamPipe_Response, CContentServerDirectory_GetServersForSteamPipe_Request>(
                HttpMethod.Get,
                nameof( ContentServerDirectory.GetServersForSteamPipe ),
                request,
                version: 1
            ).ConfigureAwait( false );

            cancellationToken.ThrowIfCancellationRequested();

            return ConvertServerList( response.Body );
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
