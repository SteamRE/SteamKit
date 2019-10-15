/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Reflection;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This class is a utility for routing callbacks to function calls.
    /// In order to bind callbacks to functions, an instance of this class must be created for the
    /// <see cref="SteamClient"/> instance that will be posting callbacks.
    /// </summary>
    public sealed class CallbackManager : ICallbackMgrInternals
    {
        SteamClient client;

        List<CallbackBase> registeredCallbacks;



        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackManager"/> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> instance to handle the callbacks of.</param>
        public CallbackManager( SteamClient client )
        {
            if ( client == null )
            {
                throw new ArgumentNullException( nameof(client) );
            }

            registeredCallbacks = new List<CallbackBase>();

            this.client = client;
        }


        /// <summary>
        /// Runs a single queued callback.
        /// If no callback is queued, this method will instantly return.
        /// </summary>
        public void RunCallbacks()
        {
            var call = client.GetCallback( true );

            if ( call == null )
                return;

            Handle( call );
        }
        /// <summary>
        /// Blocks the current thread to run a single queued callback.
        /// If no callback is queued, the method will block for the given timeout.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        public void RunWaitCallbacks( TimeSpan timeout )
        {
            var call = client.WaitForCallback( true, timeout );

            if ( call == null )
                return;

            Handle( call );
        }
        /// <summary>
        /// Blocks the current thread to run all queued callbacks.
        /// If no callback is queued, the method will block for the given timeout.
        /// </summary>
        /// <param name="timeout">The length of time to block.</param>
        public void RunWaitAllCallbacks( TimeSpan timeout )
        {
            var calls = client.GetAllCallbacks( true, timeout );
            foreach ( var call in calls )
            {
                Handle( call );
            }
        }
        /// <summary>
        /// Blocks the current thread to run a single queued callback.
        /// If no callback is queued, the method will block until one is posted.
        /// </summary>
        public void RunWaitCallbacks()
        {
            RunWaitCallbacks( TimeSpan.FromMilliseconds( -1 ) );
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks of type <typeparamref name="TCallback" />.
        /// </summary>
        /// <param name="jobID">The <see cref="JobID"/> of the callbacks that should be subscribed to.
        ///		If this is <see cref="JobID.Invalid"/>, all callbacks of type <typeparamref name="TCallback" /> will be recieved.</param>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <typeparam name="TCallback">The type of callback to subscribe to.</typeparam>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable Subscribe<TCallback>( JobID jobID, Action<TCallback> callbackFunc )
            where TCallback : class, ICallbackMsg
        {
            if ( jobID == null )
            {
                throw new ArgumentNullException( nameof(jobID) );
            }

            if ( callbackFunc == null )
            {
                throw new ArgumentNullException( nameof(callbackFunc) );
            }

            var callback = new Internal.Callback<TCallback>( callbackFunc, this, jobID );
            return new Subscription( callback, this );
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks of type <typeparam name="TCallback" />.
        /// </summary>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable Subscribe<TCallback>( Action<TCallback> callbackFunc )
            where TCallback : class, ICallbackMsg
        {
            return Subscribe( JobID.Invalid, callbackFunc );
        }

        void ICallbackMgrInternals.Register( CallbackBase call )
        {
            if ( registeredCallbacks.Contains( call ) )
                return;

            registeredCallbacks.Add( call );
        }

        void Handle( ICallbackMsg call )
        {
            registeredCallbacks
                .FindAll( callback => callback.CallbackType.IsAssignableFrom( call.GetType() ) ) // find handlers interested in this callback
                .ForEach( callback => callback.Run( call ) ); // run them
        }

        void ICallbackMgrInternals.Unregister( CallbackBase call )
        {
            registeredCallbacks.Remove( call );
        }

        sealed class Subscription : IDisposable
        {
            public Subscription( CallbackBase call, ICallbackMgrInternals manager )
            {
                this.manager = manager;
                this.call = call;
            }

            ICallbackMgrInternals? manager;
            CallbackBase? call;

            void IDisposable.Dispose()
            {
                if ( call != null && manager != null )
                {
                    manager.Unregister( call );
                    call = null;
                    manager = null;
                }
            }
        }
    }
}
