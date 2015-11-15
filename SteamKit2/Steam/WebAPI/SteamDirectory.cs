using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
                CMClient.Servers.Clear();
                CMClient.Servers.TryAddRange(servers);
            }, CancellationToken.None, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current );
        }

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="System.Net.IPEndPoint"/>s.</returns>
        public static Task<IEnumerable<IPEndPoint>> LoadAsync( uint cellid = 0 )
        {
            return LoadAsync( cellid, CancellationToken.None );
        }

        /// <summary>
        /// Load a list of servers from the Steam Directory.
        /// </summary>
        /// <param name="cellid">Cell ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> with the Result set to an enumerable list of <see cref="System.Net.IPEndPoint"/>s.</returns>
        public static Task<IEnumerable<IPEndPoint>> LoadAsync( uint cellid, CancellationToken cancellationToken )
        {
            var directory = new WebAPI.AsyncInterface( "ISteamDirectory", null );
            var args = new Dictionary<string, string>
            {
                { "cellid", cellid.ToString() }
            };

            cancellationToken.ThrowIfCancellationRequested();

            var task = directory.Call( "GetCMList", version: 1, args: args, secure: true );
            return task.ContinueWith( t =>
            {
                var response = task.Result;
                var result = ( EResult )response[ "result" ].AsInteger( ( int )EResult.Invalid );
                if ( result != EResult.OK )
                {
                    throw new InvalidOperationException( string.Format( "Steam Web API returned EResult.{0}", result ) );
                }

                var list = response[ "serverlist" ];

                cancellationToken.ThrowIfCancellationRequested();

                var endPoints = new List<IPEndPoint>( capacity: list.Children.Count );

                foreach ( var child in list.Children )
                {
                    IPEndPoint endpoint;
                    if ( !NetHelpers.TryParseIPEndPoint( child.Value, out endpoint ) )
                    {
                        continue;
                    }

                    endPoints.Add( endpoint );
                }

                return endPoints.AsEnumerable();
            }, cancellationToken, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current );
        }
    }
}
