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
        /// <summary>
        /// A handler delegate for callbacks when using CallbackMsg.Handle
        /// </summary>
        public delegate void HandleDelegate<T>( T callMsg );


#if STATIC_CALLBACKS
        public SteamClient Client { get; private set; }


        public CallbackMsg( SteamClient client )
        {
            this.Client = client;
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
        public void Handle<T>( HandleDelegate<T> handler )
            where T : CallbackMsg
        {
            if ( IsType<T>() )
            {
                handler( ( T )this );
            }
        }
    }
}
