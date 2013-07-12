/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SteamKit2.Internal;
using System.Diagnostics;

namespace SteamKit2
{
    /// <summary>
    /// Represents a single client that connects to the Steam3 network.
    /// This class is also responsible for handling the registration of client message handlers and callbacks.
    /// </summary>
    public sealed partial class SteamClient : CMClient
    {
        Dictionary<Type, ClientMsgHandler> handlers;

        long currentJobId = 0;
        DateTime processStartTime;

        object callbackLock = new object();
        Queue<CallbackMsg> callbackQueue;


        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class with a specific connection type.
        /// </summary>
        /// <param name="type">The connection type to use.</param>
        public SteamClient( ProtocolType type = ProtocolType.Tcp )
            : base( type )
        {
            callbackQueue = new Queue<CallbackMsg>();

            this.handlers = new Dictionary<Type, ClientMsgHandler>();

            // add this library's handlers
            this.AddHandler( new SteamUser() );
            this.AddHandler( new SteamFriends() );
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

            using ( var process = Process.GetCurrentProcess())
            {
                this.processStartTime = process.StartTime;
            }
        }


        #region Handlers
        /// <summary>
        /// Adds a new handler to the internal list of message handlers.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        /// <exception cref="InvalidOperationException">A handler of that type is already registered.</exception>
        public void AddHandler( ClientMsgHandler handler )
        {
            if ( handlers.ContainsKey( handler.GetType() ) )
                throw new InvalidOperationException( string.Format( "A handler of type \"{0}\" is already registered.", handler.GetType() ) );

            handlers[ handler.GetType() ] = handler;
            handler.Setup( this );
        }

        /// <summary>
        /// Removes a registered handler by name.
        /// </summary>
        /// <param name="handler">The handler name to remove.</param>
        public void RemoveHandler( Type handler )
        {
            if ( !handlers.ContainsKey( handler ) )
                return;

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
        public T GetHandler<T>()
            where T : ClientMsgHandler
        {
            Type type = typeof( T );

            if ( handlers.ContainsKey( type ) )
                return handlers[ type ] as T;

            return null;
        }
        #endregion


        #region Callbacks
        /// <summary>
        /// Gets the next callback object in the queue.
        /// This function does not dequeue the callback, you must call FreeLastCallback after processing it.
        /// </summary>
        /// <returns>The next callback in the queue, or null if no callback is waiting.</returns>
        public CallbackMsg GetCallback()
        {
            return GetCallback( false );
        }
        /// <summary>
        /// Gets the next callback object in the queue, and optionally frees it.
        /// </summary>
        /// <param name="freeLast">if set to <c>true</c> this function also frees the last callback if one existed.</param>
        /// <returns>The next callback in the queue, or null if no callback is waiting.</returns>
        public CallbackMsg GetCallback( bool freeLast )
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
        public CallbackMsg WaitForCallback()
        {
            return WaitForCallback( false );
        }
        /// <summary>
        /// Blocks the calling thread until a callback object is posted to the queue, or null after the timeout has elapsed.
        /// This function does not dequeue the callback, you must call FreeLastCallback after processing it.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        /// <returns>A callback object from the queue if a callback has been posted, or null if the timeout has elapsed.</returns>
        public CallbackMsg WaitForCallback( TimeSpan timeout )
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
        public CallbackMsg WaitForCallback( bool freeLast )
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
        public CallbackMsg WaitForCallback( bool freeLast, TimeSpan timeout )
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
        }
        #endregion


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


        /// <summary>
        /// Called when a client message is received from the network.
        /// </summary>
        /// <param name="packetMsg">The packet message.</param>
        protected override void OnClientMsgReceived( IPacketMsg packetMsg )
        {
            // let the underlying CMClient handle this message first
            base.OnClientMsgReceived( packetMsg );

            switch ( packetMsg.MsgType )
            {
                case EMsg.ChannelEncryptResult:
                    HandleEncryptResult( packetMsg ); // we're interested in this client message to post the connected callback
                    break;

                case EMsg.ClientCMList:
                    HandleCMList( packetMsg );
                    break;
            }

            // pass along the clientmsg to all registered handlers
            foreach ( var kvp in handlers )
            {
                kvp.Value.HandleMsg( packetMsg );
            }
        }
        /// <summary>
        /// Called when the client is physically disconnected from Steam3.
        /// </summary>
        protected override void OnClientDisconnected()
        {
            this.PostCallback( new DisconnectedCallback() );
        }


        void HandleEncryptResult( IPacketMsg packetMsg )
        {
            var encResult = new Msg<MsgChannelEncryptResult>( packetMsg );

            PostCallback( new ConnectedCallback( encResult.Body ) );
        }

        void HandleCMList( IPacketMsg packetMsg )
        {
            var cmMsg = new ClientMsgProtobuf<CMsgClientCMList>( packetMsg );

            PostCallback( new CMListCallback( cmMsg.Body ) );
        }

    }
}
