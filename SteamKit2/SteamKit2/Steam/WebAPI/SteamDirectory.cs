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
    /// Helper class to load servers from the Steam Directory Web API.
    /// </summary>
    public static class SteamDirectory
    {
        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="ServerRecord"/>s.</returns>
        public static Task<IReadOnlyCollection<ServerRecord>> LoadAsync( SteamConfiguration configuration )
            => LoadCoreAsync( configuration, null, CancellationToken.None );

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="ServerRecord"/>s.</returns>
        public static Task<IReadOnlyCollection<ServerRecord>> LoadAsync( SteamConfiguration configuration, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, null, cancellationToken );

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="configuration">Configuration Object</param>
        /// <param name="maxNumServers">Max number of servers to return. The API will typically return this number per server type (socket and websocket).</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="ServerRecord"/>s.</returns>
        public static Task<IReadOnlyCollection<ServerRecord>> LoadAsync( SteamConfiguration configuration, int maxNumServers, CancellationToken cancellationToken )
            => LoadCoreAsync( configuration, maxNumServers, cancellationToken );

        static async Task<IReadOnlyCollection<ServerRecord>> LoadCoreAsync( SteamConfiguration configuration, int? maxNumServers, CancellationToken cancellationToken )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof(configuration) );
            }

            var directory = configuration.GetAsyncWebAPIInterface( "ISteamDirectory" );
            var args = new Dictionary<string, object>
            {
                ["cellid"] = configuration.CellID.ToString( CultureInfo.InvariantCulture )
            };

            if ( maxNumServers.HasValue )
            {
                args[ "maxcount" ] = maxNumServers.Value.ToString( CultureInfo.InvariantCulture );
            }

            cancellationToken.ThrowIfCancellationRequested();

            var response = await directory.CallAsync( HttpMethod.Get, "GetCMList", version: 1, args: args ).ConfigureAwait( false );

            var result = ( EResult )response[ "result" ].AsInteger( ( int )EResult.Invalid );
            if ( result != EResult.OK )
            {
                throw new InvalidOperationException( string.Format( "Steam Web API returned EResult.{0}", result ) );
            }

            var socketList = response[ "serverlist" ];
            var websocketList = response[ "serverlist_websockets" ];

            cancellationToken.ThrowIfCancellationRequested();

            var serverRecords = new List<ServerRecord>( capacity: socketList.Children.Count + websocketList.Children.Count );

                foreach ( var child in socketList.Children )
                {
                    if ( child.Value is null || !ServerRecord.TryCreateSocketServer( child.Value, out var record ))
                    {
                        continue;
                    }

                    serverRecords.Add( record );
                }

            foreach ( var child in websocketList.Children )
            {
                if ( child.Value is null )
                {
                    continue;
                }

                serverRecords.Add( ServerRecord.CreateWebSocketServer( child.Value ) );
            }

            return serverRecords.AsReadOnly();
        }
    }
}
