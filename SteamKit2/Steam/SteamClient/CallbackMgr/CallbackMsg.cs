/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;

namespace SteamKit2
{
    /// <summary>
    /// A callback message
    /// </summary>
    public interface ICallbackMsg
    {
        /// <summary>
        /// The <see cref="JobID"/> that this callback is associated with. If there is no job associated,
        /// then this will be <see cref="P:JobID.Invalid"/>
        /// </summary>
        JobID JobID { get; set; }
    }

    /// <summary>
    /// Useful extensions for ICallbackMsg
    /// </summary>
    public static class CallbackMsgExtensions
    {
        /// <summary>
        /// Determines whether this callback is a certain type.
        /// </summary>
        /// <typeparam name="T">The type to check against.</typeparam>
        /// <returns>
        /// 	<c>true</c> if this callback is the type specified; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 	<c>msg</c> is null.
        /// </exception>
        [Obsolete( "This method will be removed in a future version of SteamKit. Please migrate to CallbackManager." )]
        public static bool IsType<T>( this ICallbackMsg msg )
            where T : ICallbackMsg
        {
            if ( msg == null )
                throw new ArgumentNullException( "msg" );

            return ( msg is T );
        }

        /// <summary>
        /// Invokes the specified handler delegate if the callback matches the type parameter.
        /// </summary>
        /// <typeparam name="T">The type to check against.</typeparam>
        /// <param name="msg">The callback in question.</param>
        /// <param name="handler">The handler to invoke.</param>
        /// <returns>
        /// 	<c>true</c> if the callback matches and the handler was called; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 	<c>msg</c> is null or <c>handler</c> is null.
        /// </exception>
        [Obsolete( "This method will be removed in a future version of SteamKit. Please migrate to CallbackManager." )]
        public static bool Handle<T>( this ICallbackMsg msg, Action<T> handler )
            where T : class, ICallbackMsg
        {
            if ( msg == null )
                throw new ArgumentNullException( "msg" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            var callback = msg as T;

            if ( callback != null )
            {
                handler( callback );
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents the base object all callbacks are based off.
    /// </summary>
    public abstract class CallbackMsg : ICallbackMsg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackMsg"/> class.
        /// </summary>
        protected CallbackMsg()
        {
            JobID = JobID.Invalid;
        }

        /// <summary>
        /// Gets or sets the job ID this callback refers to. If it is not a job callback, it will be <see cref="P:JobID.Invalid" />.
        /// </summary>
        public JobID JobID { get; set; }
    }
}
