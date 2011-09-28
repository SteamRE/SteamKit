/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SteamKit3
{
#if !STATIC_CALLBACKS

    class CallbackMgr
    {
        Dictionary<Type, List<ICallback>> registeredCallbacks;

        SteamClient client;


        public CallbackMgr( SteamClient client )
        {
            this.client = client;

            registeredCallbacks = new Dictionary<Type, List<ICallback>>();
        }


        internal void Register( ICallback call )
        {
            Contract.Requires( call != null );
            Contract.Requires( call.CallType != null );

            EnsureType( call.CallType );

            registeredCallbacks[ call.CallType ].Add( call );
        }
        internal void Unregister( ICallback call )
        {
            Contract.Requires( call != null );
            Contract.Requires( call.CallType != null );

            EnsureType( call.CallType );

            registeredCallbacks[ call.CallType ].Remove( call );
        }

        public void RunCallbacks()
        {
            CallbackMsg callMsg = null;

            while ( ( callMsg = client.GetCallback() ) != null )
            {
                // handle all pending callbacks
                RunCallback( callMsg );
            }
        }
        public void WaitForCallbacks( TimeSpan? timeout = null )
        {
            var callMsg = client.WaitForCallback( timeout );

            if ( callMsg == null )
                return;

            RunCallback( callMsg );
        }


        void RunCallback( CallbackMsg msg )
        {
            Type callType = msg.GetType();

            EnsureType( callType );

            // fire all handlers
            registeredCallbacks[ callType ].ForEach( call => call.Run( msg ) );
        }


        void EnsureType( Type type )
        {
            // make sure we have a list for this callback type

            if ( registeredCallbacks.ContainsKey( type ) )
                return;

            registeredCallbacks.Add( type, new List<ICallback>() );
        }
    }
#endif
}
