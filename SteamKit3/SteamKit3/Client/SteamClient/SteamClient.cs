/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using log4net;

namespace SteamKit3
{
    /// <summary>
    /// Represents a single client that connects to the Steam3 network.
    /// This class is also responsible for handling the registration of client message handlers and callbacks.
    /// </summary>
    public sealed partial class SteamClient : CMClient
    {
        static List<Type> registeredHandlers = new List<Type>();

        Dictionary<Type, ClientHandler> handlers;

#if STATIC_CALLBACKS
        static Queue<CallbackMsg> callbackQueue;
#else 
        Queue<CallbackMsg> callbackQueue;
#endif

        static readonly ILog log = LogManager.GetLogger( typeof( SteamClient ) );


        /// <summary>
        /// Gets the <see cref="JobMgr"/> for this <see cref="SteamClient"/> instance.
        /// </summary>
        public JobMgr JobMgr { get; private set; }

#if !STATIC_CALLBACKS
        internal CallbackMgr CallbackMgr { get; private set; }
#endif


        static SteamClient()
        {
            registeredHandlers = new List<Type>();

#if STATIC_CALLBACKS
            callbackQueue = new Queue<CallbackMsg>();
#endif

            RegisterHandlers( Assembly.GetExecutingAssembly() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class using a specific connection type.
        /// </summary>
        /// <param name="connType">The connection type to use.</param>
        public SteamClient( ConnectionType connType = ConnectionType.Tcp )
            : base( connType )
        {
            handlers = new Dictionary<Type, ClientHandler>();

#if !STATIC_CALLBACKS
            callbackQueue = new Queue<CallbackMsg>();

            CallbackMgr = new CallbackMgr( this );
#endif

            JobMgr = new JobMgr( this );

            SetupHandlers();
        }


        /// <summary>
        /// Called when the client successfully connects and the channel is encrypted.
        /// </summary>
        protected override void OnClientConnected()
        {
            // todo: when connected on the non-encrypted port (27014?) this should post a ConnectedCallback, since the remote server
            // will never attempt a channel handshake, and will wait on the client to do something
        }
        /// <summary>
        /// Called when the client is disconnected.
        /// </summary>
        protected override void OnClientDisconnected()
        {
#if STATIC_CALLBACKS
            PostCallback( new DisconnectedCallback( this ) );
#else
            this.PostCallback( new DisconnectedCallback() );
#endif
        }
        /// <summary>
        /// Called when the client receieves a network message.
        /// </summary>
        /// <param name="msg">The packet message.</param>
        protected override void OnClientMsgReceived( IPacketMsg msg )
        {
            JobMgr.RouteMsgToJob( msg );
        }


        // internal for access from the MultiplexMultiJob
        internal void OnMulti( byte[] data )
        {
            OnClientMsgReceived( GetPacketMsg( data ) );
        }


        #region Callbacks
        /// <summary>
        /// Posts a callback to the queue. This is normally used directly by client jobs.
        /// </summary>
        /// <param name="msg">The message.</param>
#if STATIC_CALLBACKS
        public static void PostCallback( CallbackMsg msg )
#else
        public void PostCallback( CallbackMsg msg )
#endif
        {
            lock ( callbackQueue )
            {
                callbackQueue.Enqueue( msg );
                Monitor.Pulse( callbackQueue );
            }
        }

        /// <summary>
        /// Gets the next callback object in the queue.
        /// </summary>
        /// <returns>The next callback in the queue, or null if no callback is waiting.</returns>
#if STATIC_CALLBACKS
        public static CallbackMsg GetCallback()
#else
        public CallbackMsg GetCallback()
#endif
        {
            lock ( callbackQueue )
            {
                if ( callbackQueue.Count > 0 )
                    return callbackQueue.Dequeue();

                return null;
            }
        }
        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, or null after the timeout has elapsed.
        /// </summary>
        /// <param name="timeout">The length of time to block, or null to block until a callback is posted.</param>
        /// <returns>A callback object from the queue if a callback has been posted, or null if the timeout has elapsed.</returns>
#if STATIC_CALLBACKS
        public static CallbackMsg WaitForCallback( TimeSpan? timeout = null )
#else
        public CallbackMsg WaitForCallback( TimeSpan? timeout = null )
#endif
        {
            lock ( callbackQueue )
            {
                while ( callbackQueue.Count == 0 )
                {
                    TimeSpan waitTime = timeout.GetValueOrDefault( TimeSpan.FromMilliseconds( -1 ) );

                    if ( !Monitor.Wait( callbackQueue, waitTime ) )
                        return null;
                }

                return callbackQueue.Dequeue();
            }
        }

#if !STATIC_CALLBACKS
        /// <summary>
        /// Handles all currently queued callbacks with the callback manager, and then returns.
        /// </summary>
        public void RunCallbacks()
        {
            CallbackMgr.RunCallbacks();
        }

        /// <summary>
        /// Blocks the current thread to handle a callback with the callback manager.
        /// </summary>
        /// <param name="timeout">The length of time to block, or null to block until a callback is posted.</param>
        public void RunWaitCallbacks( TimeSpan? timeout = null )
        {
            CallbackMgr.WaitForCallbacks( timeout );
        }
#endif
        #endregion

        #region Handlers
        /// <summary>
        /// Registers all available handlers in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void RegisterHandlers( Assembly assembly )
        {
            foreach ( var type in assembly.GetTypes() )
            {
                var attribs = type.GetCustomAttributes( typeof( HandlerAttribute ), false ) as HandlerAttribute[];

                if ( attribs == null || attribs.Length == 0 )
                    continue;

                registeredHandlers.Add( type );
            }
        }

        /// <summary>
        /// Returns a registered handler.
        /// </summary>
        /// <typeparam name="T">The type of the handler to cast to. Must derive from <see cref="ClientHandler"/>.</typeparam>
        /// <returns>A registered handler on success, or null if the handler could not be found.</returns>
        public T GetHandler<T>()
            where T : ClientHandler
        {
            var type = typeof( T );

            if ( !handlers.ContainsKey( type ) )
                return null;

            return handlers[ type ] as T;
        }


        void SetupHandlers()
        {
            // create a new instance of every handler we know about for this client

            foreach ( var type in registeredHandlers )
            {
                ClientHandler handler = Activator.CreateInstance( type, true ) as ClientHandler;

                handler.Setup( this );

                handlers.Add( type, handler );
            }
        }
        #endregion
    }
}
