/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SteamKit2
{
    /// <summary>
    /// Represents a single client that connects to the Steam3 network.
    /// This class is also responsible for handling the registration of client message handlers and callbacks.
    /// </summary>
    public sealed partial class SteamClient : CMClient
    {
        Dictionary<string, ClientMsgHandler> handlers;

        object callbackLock = new object();
        Queue<CallbackMsg> callbackQueue;


        /// <summary>
        /// Initializes a new instance of the <see cref="SteamClient"/> class.
        /// </summary>
        public SteamClient()
        {
            this.handlers = new Dictionary<string, ClientMsgHandler>( StringComparer.OrdinalIgnoreCase );
            this.callbackQueue = new Queue<CallbackMsg>();

            // add this library's handlers
            this.AddHandler( new SteamUser() );
            this.AddHandler( new SteamFriends() );
            this.AddHandler( new SteamApps() );
        }


        #region Handlers
        /// <summary>
        /// Adds a new handler to the internal list of message handlers.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        /// <exception cref="InvalidOperationException">
        /// A handler with that name is already registered.
        /// </exception>
        public void AddHandler( ClientMsgHandler handler )
        {
            if ( handlers.ContainsKey( handler.Name ) )
                throw new InvalidOperationException( string.Format( "A handler with name \"{0}\" is already registered.", handler.Name ) );

            handlers[ handler.Name ] = handler;
            handler.Setup( this );
        }

        /// <summary>
        /// Removes a registered handler by name.
        /// </summary>
        /// <param name="handler">The handler name to remove.</param>
        public void RemoveHandler( string handler )
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
            this.RemoveHandler( handler.Name );
        }

        /// <summary>
        /// Returns a registered handler by name.
        /// </summary>
        /// <typeparam name="T">The type of the handler to cast to. Must derive from ClientMsgHandler.</typeparam>
        /// <param name="name">The name of the handler to get.</param>
        /// <returns>A registered handler on success, or null if the handler could not be found.</returns>
        public T GetHandler<T>( string name ) where T : ClientMsgHandler
        {
            if ( handlers.ContainsKey( name ) )
                return ( T )handlers[ name ];

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


        protected override void OnClientMsgReceived( ClientMsgEventArgs e )
        {
            if ( e.EMsg == EMsg.ChannelEncryptResult )
                HandleEncryptResult( e );

            // pass along the clientmsg to all registered handlers
            foreach ( var kvp in handlers )
            {
                kvp.Value.HandleMsg( e );
            }
        }
        protected override void OnClientDisconnected()
        {
            this.PostCallback( new SteamClient.DisconnectCallback() );
        }


        // we're interested in handling the encryption result callback to see if we've properly connected or not
        void HandleEncryptResult( ClientMsgEventArgs e )
        {
            // if the EResult is OK, we've finished the crypto handshake and can send commands (such as LogOn)
            ClientMsg<MsgChannelEncryptResult, MsgHdr> encResult = null;

            try
            {
                encResult = new ClientMsg<MsgChannelEncryptResult, MsgHdr>( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamClient", "HandleEncryptResult encountered an exception while reading client msg.\n{0}", ex.ToString() );

                PostCallback( new ConnectCallback( EResult.Fail ) );
                return;
            }

            PostCallback( new ConnectCallback( encResult.Msg ) );
        }

    }
}
