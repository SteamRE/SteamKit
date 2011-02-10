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
        /// Determines whether this callback is a certain type.
        /// </summary>
        /// <typeparam name="T">The type to check against</typeparam>
        /// <returns>
        /// 	<c>true</c> if this callback is the type specified; otherwise, <c>false</c>.
        /// </returns>
        public bool IsType<T>()
            where T : CallbackMsg
        {
            return ( this is T );
        }
    }
}
