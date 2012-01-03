using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public interface ICallback
    {
        Type CallbackType { get; }
        void Run( object callback );
    }

    public sealed class Callback<TCall> : ICallback
        where TCall : CallbackMsg
    {
        public delegate void CallbackFunc( TCall callback );


        CallbackFunc func;

        public Type CallbackType { get { return typeof( TCall ); } }


        public Callback( CallbackFunc func, CallbackMgr mgr )
        {
            this.func = func;
            mgr.Register( this );
        }


        public void Run( object callback )
        {
            func( callback as TCall );
        }
    }

    public sealed class CallbackMgr
    {
#if !STATIC_CALLBACKS
        SteamClient client;
#endif

        List<ICallback> registeredCallbacks;


        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMgr"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
#if STATIC_CALLBACKS
        public CallbackMgr()
#else
        public CallbackMgr( SteamClient client )
#endif
        {
            registeredCallbacks = new List<ICallback>();

#if !STATIC_CALLBACKS
            this.client = client;
#endif
        }


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
        public void RunWaitCallbacks()
        {
            RunWaitCallbacks( TimeSpan.FromMilliseconds( -1 ) );
        }


        internal void Register( ICallback call )
        {
            registeredCallbacks.Add( call );
        }

        void Handle( CallbackMsg call )
        {
            registeredCallbacks
                .FindAll( callback => callback.CallbackType == call.GetType() ) // find handlers interested in this callback
                .ForEach( callback => callback.Run( call ) ); // run them
        }
    }
}
