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
    /// <summary>
    /// Represents the base object all callbacks are based off.
    /// </summary>
    public abstract class CallbackMsg
    {

#if STATIC_CALLBACKS
        /// <summary>
        /// Gets the underlying <see cref="SteamClient"/> instance that posted this callback.
        /// </summary>
        public SteamClient Client { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMsg"/> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> that is posting this callback.</param>
        protected CallbackMsg( SteamClient client )
        {
            this.Client = client;
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMsg"/> class.
        /// </summary>
        protected CallbackMsg()
        {
        }
#endif


        /// <summary>
        /// Determines whether this callback is a certain type.
        /// </summary>
        /// <typeparam name="T">The type to check against.</typeparam>
        /// <returns>
        /// 	<c>true</c> if this callback is the type specified; otherwise, <c>false</c>.
        /// </returns>
        public bool IsType<T>()
            where T : CallbackMsg
        {
            return ( this is T );
        }

        /// <summary>
        /// Invokes the specified handler delegate if the callback matches the type parameter.
        /// </summary>
        /// <typeparam name="T">The type to check against.</typeparam>
        /// <param name="handler">The handler to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <c>handler</c> is null.
        /// </exception>
        public void Handle<T>( Action<T> handler )
            where T : CallbackMsg
        {
            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            var callback = this as T;

            if ( callback != null )
            {
                handler( callback );
            }
        }
    }
}
