/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;

namespace SteamKit2
{
    namespace Internal
    {
        /// <summary>
        /// This is the base class for the utility <see cref="Callback&lt;TCall&gt;" /> class.
        /// This is for internal use only, and shouldn't be used directly.
        /// </summary>
        public abstract class CallbackBase
        {
            internal abstract Type CallbackType { get; }
            internal abstract void Run( object callback );
        }
    }

    /// <summary>
    /// This utility class is used for binding a callback to a function.
    /// </summary>
    /// <typeparam name="TCall">The callback type this instance will handle.</typeparam>
    [Obsolete( "Callback<T> and related APIs are obsolete and will be removed in a future version of SteamKit. Use CallbackManager.Subscribe instead." )]
    public class Callback<TCall> : Internal.CallbackBase, IDisposable
        where TCall : class, ICallbackMsg
    {
        CallbackManager mgr;

        /// <summary>
        /// Gets or sets the job ID this callback will handle.
        /// Setting this field to the maximum value of a ulong will unbind this handler,
        /// allowing all callbacks of type TCall to be handled.
        /// </summary>
        public JobID JobID { get; set; }

        /// <summary>
        /// Gets or sets the function to call when a callback of type TCall arrives.
        /// </summary>
        public Action<TCall> OnRun { get; set; }

        internal override Type CallbackType { get { return typeof( TCall ); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Callback&lt;TCall&gt;"/> class.
        /// </summary>
        /// <param name="func">The function to call when a callback of type TCall arrives.</param>
        /// <param name="mgr">The <see cref="CallbackManager"/> that is responsible for the routing of callbacks to this handler, or null if the callback will be registered manually.</param>
        public Callback(Action<TCall> func, CallbackManager mgr = null)
            : this ( func, mgr, JobID.Invalid )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Callback&lt;TCall&gt;"/> class.
        /// </summary>
        /// <param name="func">The function to call when a callback of type TCall arrives.</param>
        /// <param name="mgr">The <see cref="CallbackManager"/> that is responsible for the routing of callbacks to this handler, or null if the callback will be registered manually.</param>
        /// <param name="jobID">The <see cref="JobID"/>to filter matching callbacks by. Specify <see cref="P:JobID.Invalid"/> to recieve all callbacks of type TCall.</param>
        public Callback(Action<TCall> func, CallbackManager mgr, JobID jobID)
        {
            this.JobID = jobID;
            this.OnRun = func;

            AttachTo(mgr);
        }

        /// <summary>
        /// Attaches the specified <see cref="CallbackManager"/> to this handler.
        /// </summary>
        /// <param name="mgr">The manager to attach.</param>
        protected void AttachTo( CallbackManager mgr )
        {
            if ( mgr == null )
                return;

            this.mgr = mgr;
            mgr.Register( this );
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Callback&lt;TCall&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~Callback()
        {
            Dispose();
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// This function will unregister the callback.
        /// </summary>
        public void Dispose()
        {
            if ( mgr != null )
                mgr.Unregister( this );

            System.GC.SuppressFinalize( this );
        }


        internal override void Run( object callback )
        {
            var cb = callback as TCall;
            if (cb != null && (cb.JobID == JobID || JobID == JobID.Invalid) && OnRun != null)
            {
                OnRun(cb);
            }
        }

        static Action<TCall, JobID> CreateJoblessAction(Action<TCall> func)
        {
            return delegate(TCall callback, JobID jobID)
            {
                func(callback);
            };
        }
    }

    /// <summary>
    /// This class is a utility for routing callbacks to function calls.
    /// In order to bind callbacks to functions, an instance of this class must be created for the
    /// <see cref="SteamClient"/> instance that will be posting callbacks.
    /// </summary>
    public sealed class CallbackManager
    {
        SteamClient client;

        List<Internal.CallbackBase> registeredCallbacks;



        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackManager"/> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> instance to handle the callbacks of.</param>
        public CallbackManager( SteamClient client )
        {
            registeredCallbacks = new List<Internal.CallbackBase>();

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
        /// Blocks the current thread to run a single queued callback.
        /// If no callback is queued, the method will block until one is posted.
        /// </summary>
        public void RunWaitCallbacks()
        {
            RunWaitCallbacks( TimeSpan.FromMilliseconds( -1 ) );
        }


        /// <summary>
        /// Manually registers the specified callback handler.
        /// This is generally not required, as a handler will register itself when it is created.
        /// If the specified callback is already registered, no exception is thrown.
        /// </summary>
        /// <param name="call">The callback handler to register.</param>
        [Obsolete( "Callback<T> and related APIs are obsolete and will be removed in a future version of SteamKit. Use CallbackManager.Subscribe instead." )]
        public void Register( Internal.CallbackBase call )
        {
            if ( registeredCallbacks.Contains( call ) )
                return;

            registeredCallbacks.Add( call );
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
#pragma warning disable 0618 // Callback<T> is not obsolete internally, only to consumers
            var callback = new Callback<TCallback>( callbackFunc, this, jobID );
#pragma warning restore 0618
            return new Subscription( callback, this );
        }

        /// <summary>
        /// Registers the provided <see cref="Action{T}"/> to receive callbacks of type <typeparam name="TCallback" />.
        /// </summary>
        /// <param name="callbackFunc">The function to invoke with the callback.</param>
        /// <returns>An <see cref="IDisposable"/>. Disposing of the return value will unsubscribe the <paramref name="callbackFunc"/>.</returns>
        public IDisposable Subscribe<TCallback>( Action<TCallback> callbackFunc)
            where TCallback : class, ICallbackMsg
        {
            return Subscribe( JobID.Invalid, callbackFunc );
        }
        /// <summary>
        /// Unregisters the specified callback handler.
        /// This is generally not required, as a handler will unregister itself when disposed or finalized.
        /// If the specified callback isn't registered, no exception is thrown.
        /// </summary>
        /// <param name="call">The callback handler to unregister.</param>
        [Obsolete( "Callback<T> and related APIs are obsolete and will be removed in a future version of SteamKit. Use CallbackManager.Subscribe instead." )]
        public void Unregister( Internal.CallbackBase call )
        {
            Unsubscribe( call );
        }

        void Handle( ICallbackMsg call )
        {
            registeredCallbacks
                .FindAll( callback => callback.CallbackType.IsAssignableFrom( call.GetType() ) ) // find handlers interested in this callback
                .ForEach( callback => callback.Run( call ) ); // run them
        }

        internal void Unsubscribe( Internal.CallbackBase call )
        {
            registeredCallbacks.Remove( call );
        }

        sealed class Subscription : IDisposable
        {
            public Subscription(Internal.CallbackBase call, CallbackManager manager)
            {
                this.manager = manager;
                this.call = call;
            }

            CallbackManager manager;
            Internal.CallbackBase call;

            void IDisposable.Dispose()
            {
                if (call != null && manager != null)
                {
                    manager.Unsubscribe( call );
                    call = null;
                    manager = null;
                }
            }
        }
    }
}
