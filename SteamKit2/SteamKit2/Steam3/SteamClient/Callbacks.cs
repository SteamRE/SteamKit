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
    public partial class SteamClient
    {
        public sealed class JobCallback<T> : CallbackMsg
            where T : CallbackMsg
        {
            public long JobID { get; private set; }
            public T Callback { get; private set; }

#if STATIC_CALLBACKS
            internal JobCallback( SteamClient client, long jobId, T callback )
                : base( client )
#else
            internal JobCallback( long jobId, T callback )
#endif
            {
                JobID = jobId;
                Callback = callback;
            }
        }

        /// <summary>
        /// This callback is received after attempting to connect to the Steam network.
        /// </summary>
        public sealed class ConnectCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

#if STATIC_CALLBACKS
            internal ConnectCallback( SteamClient client, MsgChannelEncryptResult result )
                : this( client, result.Result )
#else
            internal ConnectCallback( MsgChannelEncryptResult result )
                : this( result.Result )
#endif
            {
            }

#if STATIC_CALLBACKS
            internal ConnectCallback( SteamClient client, EResult result )
                : base( client )
#else
            internal ConnectCallback( EResult result )
#endif
            {
                this.Result = result;
            }
        }


        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectCallback : CallbackMsg
        {
#if STATIC_CALLBACKS
            public DisconnectCallback( SteamClient client )
                : base( client )
            {
            }
#endif
        }
    }
}