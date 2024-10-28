/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This class is a utility for routing callbacks to function calls.
    /// In order to bind callbacks to functions, an instance of this class must be created for the
    /// <see cref="SteamClient"/> instance that will be posting callbacks.
    /// </summary>
    public sealed class CallbackManager
    {
        readonly SteamClient client;
        readonly SteamUnifiedMessages steamUnifiedMessages;

        ImmutableList<CallbackBase> registeredCallbacks = [];



        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackManager"/> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> instance to handle the callbacks of.</param>
        public CallbackManager( SteamClient client )
        {
            ArgumentNullException.ThrowIfNull( client );

            this.client = client;
            this.steamUnifiedMessages = client.GetHandler<SteamUnifiedMessages>()!;
        }


        /// <summary>
        /// Runs a single queued callback.
        /// If no callback is queued, this method will instantly return.
        /// </summary>
        /// <returns>Returns true if a callback has been run, false otherwise.</returns>
        public bool RunCallbacks()
        {
            var call = client.GetCallback();

            if ( call == null )
                return false;

            Handle( call );
            return true;
        }
        /// <summary>
        /// Blocks the current thread to run a single queued callback.
        /// If no callback is queued, the method will block for the given timeout or until a callback becomes available.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        /// <returns>Returns true if a callback has been run, false otherwise.</returns>
        public bool RunWaitCallbacks( TimeSpan timeout )
        {
            var call = client.WaitForCallback( timeout );

            if ( call == null )
                return false;

            Handle( call );
            return true;
        }
        /// <summary>
        /// Blocks the current thread to run all queued callbacks.
        /// If no callback is queued, the method will block for the given timeout or until a callback becomes available.
        /// This method returns once the queue has been emptied.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        public void RunWaitAllCallbacks( TimeSpan timeout )
        {
            if ( !RunWaitCallbacks( timeout ) )
            {
                return;
            }

            while ( RunCallbacks() )
            {
                //
            }
        }
        /// <summary>
        /// Blocks the current thread to run a single queued callback.
        /// If no callback is queued, the method will block until one becomes available.
        /// </summary>
        public void RunWaitCallbacks()
        {
            var call = client.WaitForCallback();
            Handle( call );
        }
        /// <summary>
        /// Blocks the current thread to run a single queued callback.
        /// If no callback is queued, the method will asynchronously await until one becomes available.
        /// </summary>
        public async Task RunWaitCallbackAsync( CancellationToken cancellationToken = default )
        {
            var call = await client.WaitForCallbackAsync( cancellationToken );
            Handle( call );
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks of type <typeparamref name="TCallback" />.
        /// </summary>
        /// <param name="jobID">The <see cref="JobID"/> of the callbacks that should be subscribed to.
        ///		If this is <see cref="JobID.Invalid"/>, all callbacks of type <typeparamref name="TCallback" /> will be received.</param>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <typeparam name="TCallback">The type of callback to subscribe to.</typeparam>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable Subscribe<TCallback>( JobID jobID, Action<TCallback> callbackFunc )
            where TCallback : CallbackMsg
        {
            ArgumentNullException.ThrowIfNull( jobID );

            ArgumentNullException.ThrowIfNull( callbackFunc );

            var callback = new Internal.Callback<TCallback>( callbackFunc, this, jobID );
            return callback;
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks of type <typeparam name="TCallback" />.
        /// </summary>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable Subscribe<TCallback>( Action<TCallback> callbackFunc )
            where TCallback : CallbackMsg
        {
            return Subscribe( JobID.Invalid, callbackFunc );
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks for notifications from the service of type <typeparam name="TService" />
        /// with the notification message of type <typeparam name="TNotification"></typeparam>.
        /// </summary>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable SubscribeServiceNotification<TService, TNotification>( Action<SteamUnifiedMessages.ServiceMethodNotification<TNotification>> callbackFunc )
            where TService : SteamUnifiedMessages.UnifiedService, new()
            where TNotification : IExtensible, new()
        {
            ArgumentNullException.ThrowIfNull( callbackFunc );

            steamUnifiedMessages.CreateService<TService>();

            var callback = new Callback<SteamUnifiedMessages.ServiceMethodNotification<TNotification>>( callbackFunc, this, JobID.Invalid );
            return callback;
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks for responses of <see cref="SteamUnifiedMessages"/> requests
        /// made by the service of type <typeparam name="TService" /> with the response of type <typeparam name="TResponse"></typeparam>.
        /// </summary>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable SubscribeServiceResponse<TService, TResponse>( Action<SteamUnifiedMessages.ServiceMethodResponse<TResponse>> callbackFunc )
            where TService : SteamUnifiedMessages.UnifiedService, new()
            where TResponse : IExtensible, new()
        {
            ArgumentNullException.ThrowIfNull( callbackFunc );

            steamUnifiedMessages.CreateService<TService>();

            var callback = new Callback<SteamUnifiedMessages.ServiceMethodResponse<TResponse>>( callbackFunc, this, JobID.Invalid );
            return callback;
        }

        internal void Register( CallbackBase call )
            => ImmutableInterlocked.Update( ref registeredCallbacks, static ( list, item ) => list.Add( item ), call );

        internal void Unregister( CallbackBase call )
            => ImmutableInterlocked.Update( ref registeredCallbacks, static ( list, item ) => list.Remove( item ), call );

        void Handle( CallbackMsg call )
        {
            var callbacks = registeredCallbacks;
            var type = call.GetType();

            // find handlers interested in this callback
            foreach ( var callback in callbacks )
            {
                if ( callback.CallbackType.IsAssignableFrom( type ) )
                {
                    callback.Run( call );
                }
            }
        }
    }
}
