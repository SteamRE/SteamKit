/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit3
{
#if !STATIC_CALLBACKS

    internal interface ICallback
    {
        Type CallType { get; }

        void Run( CallbackMsg msg );
    }

    /// <summary>
    /// This class is a helper for routing callbacks to associated handler functions.
    /// <example>
    /// <code>
    /// class MyClient
    /// {
    ///     SteamClient client;
    ///     
    ///     public MyClient()
    ///     {
    ///         client = new SteamClient();
    ///         
    ///         var connectedCall = new Callback&lt;SteamClient.ConnectedCallback&gt;( OnConnected, client );
    ///     }
    ///     
    ///     void OnConnected( SteamClient.ConnectedCallback msg )
    ///     {
    ///         // ...
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The type of callback this instance will handle.</typeparam>
    public class Callback<T> : ICallback
        where T : CallbackMsg
    {
        /// <summary>
        /// A delegate used for the handler function.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public delegate void CallbackFunc( T msg );


        /// <summary>
        /// Occurs when this handler recieves a callback.
        /// </summary>
        public event CallbackFunc OnRun;

        Type ICallback.CallType { get { return typeof( T ); } }
        CallbackMgr mgr;


        /// <summary>
        /// Initializes a new instance of the <see cref="Callback&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="func">The function to call when the callback is recieved.</param>
        /// <param name="client">The client that this callback is associated with.</param>
        public Callback( CallbackFunc func, SteamClient client )
        {
            mgr = client.CallbackMgr;
            OnRun += func;

            Register();
        }

        /// <summary>
        /// Registers this instance to be available for callbacks.
        /// </summary>
        public void Register()
        {
            mgr.Register( this );
        }
        /// <summary>
        /// Unregisters this instance to be unavailable for callbacks.
        /// </summary>
        public void Unregister()
        {
            mgr.Unregister( this );
        }


        void ICallback.Run( CallbackMsg msg )
        {
            if ( OnRun != null )
                OnRun( msg as T );
        }
    }
#endif
}
