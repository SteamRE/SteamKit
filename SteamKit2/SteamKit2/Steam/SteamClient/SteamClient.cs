/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProtoBuf;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// Represents a single client that connects to the Steam3 network.
    /// This class is also responsible for handling the registration of client message handlers and callbacks.
    /// </summary>
    public sealed partial class SteamClient : CMClient
    {
        OrderedDictionary handlers;

        long currentJobId = 0;
        DateTime processStartTime;

        object callbackLock = new object();
        Queue<ICallbackMsg> callbackQueue;

        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal AsyncJobManager jobManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class with the default configuration.
        /// </summary>
        public SteamClient()
            : this( SteamConfiguration.CreateDefault() )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class a specific identifier.
        /// </summary>
        /// <param name="identifier">A specific identifier to be used to uniquely identify this instance.</param>
        public SteamClient( string identifier )
            : this( SteamConfiguration.CreateDefault(), identifier )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class with a specific configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use for this client.</param>
        /// <exception cref="ArgumentNullException">The configuration object is <c>null</c></exception>
        public SteamClient( SteamConfiguration configuration )
            : this( configuration, Guid.NewGuid().ToString( "N" ) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class with a specific configuration and identifier
        /// </summary>
        /// <param name="configuration">The configuration to use for this client.</param>
        /// <param name="identifier">A specific identifier to be used to uniquely identify this instance.</param>
        /// <exception cref="ArgumentNullException">The configuration object or identifier is <c>null</c></exception>
        /// <exception cref="ArgumentException">The identifier is an empty string</exception>
        public SteamClient( SteamConfiguration configuration, string identifier )
            : base( configuration, identifier )
        {
            callbackQueue = new Queue<ICallbackMsg>();

            this.handlers = new OrderedDictionary();

            // add this library's handlers
            // notice: SteamFriends should be added before SteamUser due to AccountInfoCallback
            this.AddHandler( new SteamFriends() );
            this.AddHandler( new SteamUser() );
            this.AddHandler( new SteamApps() );
            this.AddHandler( new SteamGameCoordinator() );
            this.AddHandler( new SteamGameServer() );
            this.AddHandler( new SteamUserStats() );
            this.AddHandler( new SteamMasterServer() );
            this.AddHandler( new SteamCloud() );
            this.AddHandler( new SteamWorkshop() );
            this.AddHandler( new SteamTrading() );
            this.AddHandler( new SteamUnifiedMessages() );
            this.AddHandler( new SteamScreenshots() );
            this.AddHandler( new SteamMatchmaking() );
            this.AddHandler( new SteamNetworking() );

            using ( var process = Process.GetCurrentProcess() )
            {
                this.processStartTime = process.StartTime;
            }

            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientCMList, HandleCMList },

                // to support asyncjob life time
                { EMsg.JobHeartbeat, HandleJobHeartbeat },
                { EMsg.DestJobFailed, HandleJobFailed },
            };

            jobManager = new AsyncJobManager();
        }


        #region Handlers
        /// <summary>
        /// Adds a new handler to the internal list of message handlers.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        /// <exception cref="InvalidOperationException">A handler of that type is already registered.</exception>
        public void AddHandler( ClientMsgHandler handler )
        {
            if ( handlers.Contains( handler.GetType() ) )
            {
                throw new InvalidOperationException( string.Format( "A handler of type \"{0}\" is already registered.", handler.GetType() ) );

            }

            handler.Setup( this );
            handlers[ handler.GetType() ] = handler;
        }

        /// <summary>
        /// Removes a registered handler by name.
        /// </summary>
        /// <param name="handler">The handler name to remove.</param>
        public void RemoveHandler( Type handler )
        {
            handlers.Remove( handler );
        }
        /// <summary>
        /// Removes a registered handler.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveHandler( ClientMsgHandler handler )
        {
            this.RemoveHandler( handler.GetType() );
        }

        /// <summary>
        /// Returns a registered handler.
        /// </summary>
        /// <typeparam name="T">The type of the handler to cast to. Must derive from ClientMsgHandler.</typeparam>
        /// <returns>
        /// A registered handler on success, or null if the handler could not be found.
        /// </returns>
        public T? GetHandler<T>()
            where T : ClientMsgHandler
        {
            Type type = typeof( T );

            if ( handlers.Contains( type ) )
            {
                return handlers[ type ] as T;
            }

            return null;
        }
        #endregion


        #region Callbacks
        /// <summary>
        /// Gets the next callback object in the queue.
        /// This function does not dequeue the callback, you must call FreeLastCallback after processing it.
        /// </summary>
        /// <returns>The next callback in the queue, or null if no callback is waiting.</returns>
        public ICallbackMsg? GetCallback()
        {
            return GetCallback( false );
        }
        /// <summary>
        /// Gets the next callback object in the queue, and optionally frees it.
        /// </summary>
        /// <param name="freeLast">if set to <c>true</c> this function also frees the last callback if one existed.</param>
        /// <returns>The next callback in the queue, or null if no callback is waiting.</returns>
        public ICallbackMsg? GetCallback( bool freeLast )
        {
            lock ( callbackLock )
            {
                if ( callbackQueue.Count > 0 )
                    return ( freeLast ? callbackQueue.Dequeue() : callbackQueue.Peek() );
            }

            return null;
        }

        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue.
        /// This function does not dequeue the callback, you must call FreeLastCallback after processing it.
        /// </summary>
        /// <returns>The callback object from the queue.</returns>
        public ICallbackMsg? WaitForCallback()
        {
            return WaitForCallback( false );
        }
        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, or null after the timeout has elapsed.
        /// This function does not dequeue the callback, you must call FreeLastCallback after processing it.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        /// <returns>A callback object from the queue if a callback has been posted, or null if the timeout has elapsed.</returns>
        public ICallbackMsg? WaitForCallback( TimeSpan timeout )
        {
            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                {
                    if ( !Monitor.Wait( callbackLock, timeout ) )
                        return null;
                }

                return callbackQueue.Peek();
            }
        }
        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, and optionally frees it.
        /// </summary>
        /// <param name="freeLast">if set to <c>true</c> this function also frees the last callback.</param>
        /// <returns>The callback object from the queue.</returns>
        public ICallbackMsg WaitForCallback( bool freeLast )
        {
            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                    Monitor.Wait( callbackLock );

                return ( freeLast ? callbackQueue.Dequeue() : callbackQueue.Peek() );
            }
        }
        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, and optionally frees it.
        /// </summary>
        /// <param name="freeLast">if set to <c>true</c> this function also frees the last callback.</param>
        /// <param name="timeout">The length of time to block.</param>
        /// <returns>A callback object from the queue if a callback has been posted, or null if the timeout has elapsed.</returns>
        public ICallbackMsg? WaitForCallback( bool freeLast, TimeSpan timeout )
        {
            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                {
                    if ( !Monitor.Wait( callbackLock, timeout ) )
                        return null;
                }

                return ( freeLast ? callbackQueue.Dequeue() : callbackQueue.Peek() );
            }
        }
        /// <summary>
        /// Blocks the calling thread until the queue contains a callback object. Returns all callbacks, and optionally frees them.
        /// </summary>
        /// <param name="freeLast">if set to <c>true</c> this function also frees all callbacks.</param>
        /// <param name="timeout">The length of time to block.</param>
        /// <returns>All current callback objects in the queue.</returns>
        public IEnumerable<ICallbackMsg> GetAllCallbacks( bool freeLast, TimeSpan timeout )
        {
            IEnumerable<ICallbackMsg> callbacks;

            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                {
                    if ( !Monitor.Wait( callbackLock, timeout ) )
                    {
                        return Enumerable.Empty<ICallbackMsg>();
                    }
                }

                callbacks = callbackQueue.ToArray();
                if ( freeLast )
                {
                    callbackQueue.Clear();
                }
            }

            return callbacks;
        }
        /// <summary>
        /// Frees the last callback in the queue.
        /// </summary>
        public void FreeLastCallback()
        {
            lock ( callbackLock )
            {
                if ( callbackQueue.Count == 0 )
                    return;

                callbackQueue.Dequeue();
            }
        }

        /// <summary>
        /// Posts a callback to the queue. This is normally used directly by client message handlers.
        /// </summary>
        /// <param name="msg">The message.</param>
        public void PostCallback( CallbackMsg msg )
        {
            if ( msg == null )
                return;

            lock ( callbackLock )
            {
                callbackQueue.Enqueue( msg );
                Monitor.Pulse( callbackLock );
            }

            jobManager.TryCompleteJob( msg.JobID, msg );
        }
        #endregion


        #region Jobs
        /// <summary>
        /// Returns the next available JobID for job based messages.
        /// This function is thread-safe.
        /// </summary>
        /// <returns>The next available JobID.</returns>
        public JobID GetNextJobID()
        {
            var sequence = ( uint )Interlocked.Increment( ref currentJobId );
            return new JobID
            {
                BoxID = 0,
                ProcessID = 0,
                SequentialCount = sequence,
                StartTime = processStartTime
            };
        }
        internal void StartJob( AsyncJob job )
        {
            jobManager.StartJob( job );
        }
        #endregion


        /// <summary>
        /// Called when a client message is received from the network.
        /// </summary>
        /// <param name="packetMsg">The packet message.</param>
        protected override bool OnClientMsgReceived( IPacketMsg? packetMsg )
        {
            // let the underlying CMClient handle this message first
            if ( !base.OnClientMsgReceived( packetMsg ) )
            {
                return false;
            }

            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc );

            if ( haveFunc )
            {
                // we want to handle some of the clientmsgs before we pass them along to registered handlers
                handlerFunc( packetMsg );
            }

            // pass along the clientmsg to all registered handlers
            foreach ( DictionaryEntry kvp in handlers )
            {
                var key = (Type) kvp.Key;
                var value = (ClientMsgHandler) kvp.Value;

                try
                {
                    value.HandleMsg( packetMsg );
                }
                catch ( ProtoException ex )
                {
                    LogDebug( "SteamClient", "'{0}' handler failed to (de)serialize a protobuf: '{1}'", key.Name, ex.Message );
                    Disconnect();
                    return false;
                }
                catch ( Exception ex )
                {
                    LogDebug( "SteamClient", "Unhandled '{0}' exception from '{1}' handler: '{2}'", ex.GetType().Name, key.Name, ex.Message );
                    Disconnect();
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Called when the client is securely connected to Steam3.
        /// </summary>
        protected override void OnClientConnected()
        {
            base.OnClientConnected();

            jobManager.SetTimeoutsEnabled( true );

            PostCallback( new ConnectedCallback() );
        }
        /// <summary>
        /// Called when the client is physically disconnected from Steam3.
        /// </summary>
        protected override void OnClientDisconnected( bool userInitiated )
        {
            base.OnClientDisconnected( userInitiated );

            // if we are disconnected, cancel all pending jobs
            jobManager.CancelPendingJobs();

            jobManager.SetTimeoutsEnabled( false );

            ClearHandlerCaches();

            PostCallback( new DisconnectedCallback( userInitiated ) );
        }


        void ClearHandlerCaches()
        {
            GetHandler<SteamMatchmaking>()?.ClearLobbyCache();
        }

        void HandleCMList( IPacketMsg packetMsg )
        {
            var cmMsg = new ClientMsgProtobuf<CMsgClientCMList>( packetMsg );

            PostCallback( new CMListCallback( cmMsg.Body ) );
        }

        void HandleJobHeartbeat( IPacketMsg packetMsg )
        {
            jobManager.HeartbeatJob( packetMsg.TargetJobID );
        }
        void HandleJobFailed( IPacketMsg packetMsg )
        {
            jobManager.FailJob( packetMsg.TargetJobID );
        }

    }
}
