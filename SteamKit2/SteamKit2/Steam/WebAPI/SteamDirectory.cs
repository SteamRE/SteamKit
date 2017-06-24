using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2.Discovery;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// Helper class to load servers from the Steam Directory Web API.
    /// </summary>
    public static class SteamDirectory
    {
        /// <summary>
        /// Initializes <see cref="SteamKit2.Internal.CMClient"/>'s server list with servers from the Steam Directory.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        public static Task Initialize( uint cellid = 0 )
        {
            return LoadAsync( cellid ).ContinueWith( t =>
            {
                var servers = t.Result;
                CMClient.Servers.ReplaceList(servers);
            }, CancellationToken.None, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current );
        }

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CMServerRecord"/>s.</returns>
        public static Task<IReadOnlyCollection<CMServerRecord>> LoadAsync( uint cellid = 0 )
        {
            return LoadAsync( cellid, CancellationToken.None );
        }

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="CMServerRecord"/>s.</returns>
        public static Task<IReadOnlyCollection<CMServerRecord>> LoadAsync( uint cellid, CancellationToken cancellationToken )
        {
            var directory = new WebAPI.AsyncInterface( "ISteamDirectory", null );
            var args = new Dictionary<string, string>
            {
                { "cellid", cellid.ToString() }
            };

            cancellationToken.ThrowIfCancellationRequested();

            var task = directory.CallAsync( HttpMethod.Get, "GetCMList", version: 1, args: args, secure: true );
            return task.ContinueWith( t =>
            {
                var response = task.Result;
                var result = ( EResult )response[ "result" ].AsInteger( ( int )EResult.Invalid );
                if ( result != EResult.OK )
                {
                    throw new InvalidOperationException( string.Format( "Steam Web API returned EResult.{0}", result ) );
                }

                var socketList = response[ "serverlist" ];
                var websocketList = response[ "serverlist_websockets" ];

                cancellationToken.ThrowIfCancellationRequested();

                var serverRecords = new List<CMServerRecord>( capacity: socketList.Children.Count + websocketList.Children.Count );

                foreach ( var child in socketList.Children )
                {
                    if ( !NetHelpers.TryParseIPEndPoint( child.Value, out var endpoint ) )
                    {
                        continue;
                    }

                    serverRecords.Add( CMServerRecord.SocketServer( endpoint ) );
                }

                foreach ( var child in websocketList.Children )
                {
                    serverRecords.Add( CMServerRecord.WebSocketServer( child.Value ) );
                }

                return (IReadOnlyCollection<CMServerRecord>)serverRecords;
            }, cancellationToken, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current );
        }
    }
}
