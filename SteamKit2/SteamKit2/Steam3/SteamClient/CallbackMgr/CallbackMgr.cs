/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class Callback<TCall> : Internal.CallbackBase, IDisposable
        where TCall : CallbackMsg
    {
        CallbackManager mgr;

        /// <summary>
        /// Gets or sets the function to call when a callback of type TCall arrives.
        /// </summary>
        public Action<TCall> OnRun { get; set; }

        internal override Type CallbackType { get { return typeof( TCall ); } }


        internal Callback()
        {
            this.mgr = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Callback&lt;TCall&gt;"/> class.
        /// </summary>
        /// <param name="func">The function to call when a callback of type TCall arrives.</param>
        public Callback( Action<TCall> func )
            : this()
        {
            this.OnRun = func;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Callback&lt;TCall&gt;"/> class.
        /// </summary>
        /// <param name="func">The function to call when a callback of type TCall arrives.</param>
        /// <param name="mgr">The <see cref="CallbackManager"/> that is responsible for the routing of callbacks to this handler.</param>
        public Callback( Action<TCall> func, CallbackManager mgr )
        {
            this.OnRun = func;
            AttachTo( mgr );
        }

        /// <summary>
        /// Attaches the specified <see cref="CallbackManager"/> to this handler.
        /// </summary>
        /// <param name="mgr">The manager to attach.</param>
        protected void AttachTo( CallbackManager mgr )
        {
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

            GC.SuppressFinalize( this );
        }


        internal override void Run( object callback )
        {
            OnRun( callback as TCall );
        }
    }

    /// <summary>
    /// This utility class is used for binding job callbacks to functions.
    /// </summary>
    /// <typeparam name="TCall">The callback type this instance will handle.</typeparam>
    public sealed class JobCallback<TCall> : Callback<SteamClient.JobCallback<TCall>>
        where TCall : CallbackMsg
    {

        private ulong jobID;

        /// <summary>
        /// Gets or sets the function to call when a job based callback of type TCall arrives.
        /// </summary>
        public new Action<TCall> OnRun { get; set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="JobCallback&lt;TCall&gt;"/> is completed.
        /// Completion is defined as the callback for the given job id being received and handled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if completed; otherwise, <c>false</c>.
        /// </value>
        public bool Completed { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="JobCallback&lt;TCall&gt;"/> class.
        /// </summary>
        /// <param name="jobID">The Job ID this callback will handle.</param>
        /// <param name="func">The function to call when a job based callback of type TCall arrives.</param>
        public JobCallback( ulong jobID, Action<TCall> func )
        {
            this.jobID = jobID;
            base.OnRun = HandleCallback;
            this.OnRun = func;

            this.Completed = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobCallback&lt;TCall&gt;"/> class.
        /// </summary>
        /// <param name="jobID">The Job ID this callback will handle.</param>
        /// <param name="func">The function to call when a job based callback of type TCall arrives.</param>
        /// <param name="mgr">The <see cref="CallbackManager"/> that is responsible for the routing of callbacks to this handler.</param>
        public JobCallback( ulong jobID, Action<TCall> func, CallbackManager mgr )
            : this( jobID, func )
        {
            AttachTo( mgr );
        }


        void HandleCallback( SteamClient.JobCallback<TCall> callback )
        {
            if ( callback.JobID == jobID )
            {
                OnRun( callback.Callback );
                Completed = true;
            }
        }
    }

    /// <summary>
    /// This class is a utility for routing callbacks to function calls.
    /// In order to bind callbacks to functions, an instance of this class must be created for the
    /// <see cref="SteamClient"/> instance that will be posting callbacks.
    /// </summary>
    public sealed class CallbackManager
    {
#if !STATIC_CALLBACKS
        SteamClient client;
#endif

        List<Internal.CallbackBase> registeredCallbacks;


        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackManager"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
#if STATIC_CALLBACKS
        public CallbackManager()
#else
        public CallbackManager( SteamClient client )
#endif
        {
            registeredCallbacks = new List<Internal.CallbackBase>();

#if !STATIC_CALLBACKS
            this.client = client;
#endif
        }


        /// <summary>
        /// Runs a single queued callback.
        /// If no callback is queued, this method will instantly return.
        /// </summary>
        public void RunCallbacks()
        {
#if STATIC_CALLBACKS
            var call = SteamClient.GetCallback( true );
#else
            var call = client.GetCallback( true );
#endif

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
#if STATIC_CALLBACKS
            var call = SteamClient.WaitForCallback( true, timeout );
#else
            var call = client.WaitForCallback( true, timeout );
#endif

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
        public void Register( Internal.CallbackBase call )
        {
            if ( registeredCallbacks.Contains( call ) )
                return;

            registeredCallbacks.Add( call );
        }
        /// <summary>
        /// Unregisters the specified callback handler.
        /// This is generally not required, as a handler will unregister itself when disposed or finalized.
        /// If the specified callback isn't registered, no exception is thrown.
        /// </summary>
        /// <param name="call">The callback handler to unregister.</param>
        public void Unregister( Internal.CallbackBase call )
        {
            registeredCallbacks.Remove( call );
        }

        void Handle( CallbackMsg call )
        {
            registeredCallbacks
                .FindAll( callback => callback.CallbackType == call.GetType() ) // find handlers interested in this callback
                .ForEach( callback => callback.Run( call ) ); // run them
        }
    }
}
