/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SteamKit2
{
    /// <summary>
    /// Interface all debug log listeners must implement in order to register themselves.
    /// </summary>
    public interface IDebugListener
    {
        /// <summary>
        /// Called when the DebugLog wishes to inform listeners of debug spew.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="msg">The message to log.</param>
        void WriteLine( string category, string msg );
    }

    class ActionListener : IDebugListener
    {
        public Action<string, string> Action;

        public ActionListener( Action<string, string> action )
        {
            this.Action = action;
        }

        public void WriteLine( string category, string msg )
        {
            Action( category, msg );
        }

    }

    /// <summary>
    /// Represents the root debug logging functionality. 
    /// </summary>
    public static class DebugLog
    {
        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool Enabled { get; set; }

        internal static List<IDebugListener> listeners = new List<IDebugListener>();


        /// <summary>
        /// Initializes the <see cref="DebugLog"/> class.
        /// </summary>
        static DebugLog()
        {
#if DEBUG
            Enabled = true;
#else
            Enabled = false;
#endif
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void AddListener( IDebugListener listener )
        {
            if ( listener == null )
            {
                throw new ArgumentNullException( nameof(listener) );
            }
            
            listeners.Add( listener );
        }
        /// <summary>
        /// Adds an action listener.
        /// </summary>
        /// <param name="listenerAction">The listener action.</param>
        public static void AddListener( Action<string, string> listenerAction )
        {
            if ( listenerAction == null )
            {
                return;
            }

            AddListener( new ActionListener( listenerAction ) );
        }
        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener( IDebugListener listener )
        {
            listeners.Remove( listener );
        }
        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listenerAction">The previously registered listener action.</param>
        public static void RemoveListener( Action<string, string> listenerAction )
        {
            // probably don't need this function at all, since actions are designed to be anonymous methods
            // Just In Case

            if ( listenerAction == null )
            {
                return;
            }

            var removals = listeners
                 .Where( list => list is ActionListener )
                 .Cast<ActionListener>()
                 .Where( actList => actList.Action == listenerAction )
                 .ToArray();

            if ( removals.Length == 0 )
                return;

            listeners.Remove( removals[ 0 ] );
        }

        /// <summary>
        /// Clears all registered listeners from the <see cref="DebugLog"/>.
        /// </summary>
        public static void ClearListeners()
        {
            listeners.Clear();
        }

        /// <summary>
        /// Writes a line to the debug log, informing all listeners.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="msg">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more objects to format.</param>
        public static void WriteLine( string category, string msg, params object?[]? args )
        {
            if ( !DebugLog.Enabled )
            {
                return;
            }

            string strMsg;

            if ( args == null || args.Length == 0 )
            {
                strMsg = msg;
            }
            else
            {
                strMsg = string.Format( msg, args );
            }

            foreach ( IDebugListener debugListener in listeners )
            {
                debugListener.WriteLine( category, strMsg );
            }
        }


        /// <summary>
        /// Checks for a condition; if the condition is <c>false</c>, outputs a specified message and displays a message box that shows the call stack.
        /// This method is equivalent to System.Diagnostics.Debug.Assert, however, it is tailored to spew failed assertions into the SteamKit debug log.
        /// </summary>
        /// <param name="condition">The conditional expression to evaluate. If the condition is <c>true</c>, the specified message is not sent and the message box is not displayed.</param>
        /// <param name="category">The category of the message.</param>
        /// <param name="message">The message to display if the assertion fails.</param>
        public static void Assert( [DoesNotReturnIf(false)] bool condition, string category, string message )
        {
            // make use of .NET's assert facility first
            Debug.Assert( condition, string.Format( "{0}: {1}", category, message ) );

            // then spew to our debuglog, so we can get info in release builds
            if ( !condition )
                WriteLine( category, "Assertion Failed! " + message );
        }
    }
}
