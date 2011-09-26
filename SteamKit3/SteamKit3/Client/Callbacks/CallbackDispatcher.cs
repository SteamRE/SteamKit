using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace SteamKit3
{
#if !STATIC_CALLBACKS
    class DispatcherInfo
    {
        public Task Task { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
    }

    /// <summary>
    /// This utility class faciliates the operation of a thread for handling callbacks.
    /// </summary>
    public static class CallbackDispatcher
    {
        static Dictionary<SteamClient, DispatcherInfo> dispatchMap;


        static CallbackDispatcher()
        {
            dispatchMap = new Dictionary<SteamClient, DispatcherInfo>();
        }


        /// <summary>
        /// Spawns a new callback dispatcher thread for a specified <see cref="SteamClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> .</param>
        public static void SpawnDispatcher( SteamClient client )
        {
            Contract.Requires( client != null );

            lock ( dispatchMap )
            {
                if ( dispatchMap.ContainsKey( client ) )
                    return;
            }

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var task = new Task( () => DispatchFunc( client, token ), token, TaskCreationOptions.LongRunning );

            var dispatchInfo = new DispatcherInfo()
            {
                Task = task,
                TokenSource = tokenSource,
            };

            lock ( dispatchMap )
            {
                dispatchMap.Add( client, dispatchInfo );
            }

            task.Start();
        }

        /// <summary>
        /// Stops a spawned dispatcher thread for the specified <see cref="SteamClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/>.</param>
        public static void StopDispatcher( SteamClient client )
        {
            Contract.Requires( client != null );

            DispatcherInfo info = null;

            lock ( dispatchMap )
            {
                if ( !dispatchMap.ContainsKey( client ) )
                    return;

                info = dispatchMap[ client ];
            }

            info.TokenSource.Cancel();

            // wait for the task to finish
            Task.WaitAll( info.Task );
        }


        static void DispatchFunc( SteamClient client, CancellationToken cancelToken )
        {
            while ( !cancelToken.IsCancellationRequested )
            {
                client.CallbackMgr.WaitForCallbacks( TimeSpan.FromMilliseconds( 100 ) );
            }
        }
    }
#endif
}
