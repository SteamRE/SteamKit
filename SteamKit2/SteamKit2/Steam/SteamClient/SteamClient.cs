/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ProtoBuf;
using SteamKit2.Authentication;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// Represents a single client that connects to the Steam3 network.
    /// This class is also responsible for handling the registration of client message handlers and callbacks.
    /// </summary>
    public sealed partial class SteamClient : CMClient
    {
        List<ClientMsgHandler> handlers;

        long currentJobId = 0;
        DateTime processStartTime;

        BufferBlock<ICallbackMsg> callbackQueue = new();

        internal AsyncJobManager jobManager;

        SteamAuthentication? _authentication = null;

        /// <summary>
        /// Handler used for authenticating on Steam.
        /// </summary>
        public SteamAuthentication Authentication => _authentication ??= new SteamAuthentication( this );

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
            // Start calculating machine info so that it is (hopefully) ready by the time we get to logging in.
            HardwareUtils.Init( configuration.MachineInfoProvider );

            // add this library's handlers
            const int HANDLERS_COUNT = 15; // this number should match the amount of AddHandlerCore calls below
            this.handlers = new( HANDLERS_COUNT );

            // notice: SteamFriends should be added before SteamUser due to AccountInfoCallback
            this.AddHandlerCore( new SteamFriends() );
            this.AddHandlerCore( new SteamUser() );
            this.AddHandlerCore( new SteamApps() );
            this.AddHandlerCore( new SteamGameCoordinator() );
            this.AddHandlerCore( new SteamGameServer() );
            this.AddHandlerCore( new SteamUserStats() );
            this.AddHandlerCore( new SteamMasterServer() );
            this.AddHandlerCore( new SteamCloud() );
            this.AddHandlerCore( new SteamWorkshop() );
            this.AddHandlerCore( new SteamUnifiedMessages() );
            this.AddHandlerCore( new SteamScreenshots() );
            this.AddHandlerCore( new SteamMatchmaking() );
            this.AddHandlerCore( new SteamNetworking() );
            this.AddHandlerCore( new SteamContent() );
            this.AddHandlerCore( new SteamAuthTicket() );

            Debug.Assert( this.handlers.Count == HANDLERS_COUNT );

            using ( var process = Process.GetCurrentProcess() )
            {
                this.processStartTime = process.StartTime;
            }

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
            ArgumentNullException.ThrowIfNull( handler );

            var type = handler.GetType();
            var msgIndex = handlers.FindIndex( h => h.GetType() == type );

            if ( msgIndex > -1 )
            {
                throw new InvalidOperationException( string.Format( "A handler of type \"{0}\" is already registered.", handler.GetType() ) );
            }

            AddHandlerCore( handler );
        }

        private void AddHandlerCore( ClientMsgHandler handler )
        {
            handler.Setup( this );
            handlers.Add( handler );
        }

        /// <summary>
        /// Removes a registered handler by name.
        /// </summary>
        /// <param name="handler">The handler name to remove.</param>
        public void RemoveHandler( Type handler )
        {
            var msgIndex = handlers.FindIndex( h => h.GetType() == handler );

            if ( msgIndex > -1 )
            {
                handlers.RemoveAt( msgIndex );
            }
        }

        /// <summary>
        /// Removes a registered handler.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveHandler( ClientMsgHandler handler )
        {
            handlers.Remove( handler );
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

            return handlers.Find( h => h.GetType() == type ) as T;
        }
        #endregion


        #region Callbacks
        /// <summary>
        /// Gets the next callback object in the queue, and removes it.
        /// </summary>
        /// <returns>The next callback in the queue, or null if no callback is waiting.</returns>
        public ICallbackMsg? GetCallback()
        {
            if ( callbackQueue.TryReceive( out var msg ) )
            {
                return msg;
            }
            return null;
        }

        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, and removes it.
        /// </summary>
        /// <returns>The callback object from the queue.</returns>
        public ICallbackMsg WaitForCallback()
        {
            return callbackQueue.Receive();
        }

        /// <summary>
        /// Asynchronously awaits until a callback object is posted to the queue, and removes it.
        /// </summary>
        /// <returns>The callback object from the queue.</returns>
        public Task<ICallbackMsg> WaitForCallbackAsync( CancellationToken cancellationToken = default )
        {
            return callbackQueue.ReceiveAsync( cancellationToken );
        }

        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, and removes it.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        /// <returns>A callback object from the queue if a callback has been posted, or null if the timeout has elapsed.</returns>
        public ICallbackMsg? WaitForCallback( TimeSpan timeout )
        {
            try
            {
                return callbackQueue.Receive( timeout );
            }
            catch ( TimeoutException )
            {
            }

            return null;
        }

        /// <summary>
        /// Posts a callback to the queue. This is normally used directly by client message handlers.
        /// </summary>
        /// <param name="msg">The message.</param>
        public void PostCallback( CallbackMsg msg )
        {
            if ( msg == null )
                return;

            callbackQueue.Post( msg );
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

            ArgumentNullException.ThrowIfNull( packetMsg );

            // we want to handle some of the clientmsgs before we pass them along to registered handlers
            switch ( packetMsg.MsgType )
            {
                case EMsg.ClientCMList:
                    HandleCMList( packetMsg );
                    break;

                case EMsg.JobHeartbeat:
                    HandleJobHeartbeat( packetMsg );
                    break;

                case EMsg.DestJobFailed:
                    HandleJobFailed( packetMsg );
                    break;
            }

            // pass along the clientmsg to all registered handlers
            foreach ( var value in handlers )
            {
                try
                {
                    value.HandleMsg( packetMsg );
                }
                catch ( ProtoException ex )
                {
                    LogDebug( nameof( SteamClient ), $"'{value.GetType().Name}' handler failed to (de)serialize a protobuf: {ex}" );
                    Disconnect( userInitiated: false );
                    return false;
                }
                catch ( Exception ex )
                {
                    LogDebug( nameof( SteamClient ), $"Unhandled exception from '{value.GetType().Name}' handler: {ex}" );
                    Disconnect( userInitiated: false );
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
