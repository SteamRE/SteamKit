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
        /// <summary>
        /// This callback is received when a job related operation on the backend has completed, or a client operation should begin.
        /// </summary>
        /// <typeparam name="T">The inner callback this job represents.</typeparam>
        public sealed class JobCallback<T> : CallbackMsg
            where T : CallbackMsg
        {
            /// <summary>
            /// Gets the Job ID of this callback. For client based jobs, this will match the Job ID of a function call.
            /// For server based jobs, this is provided to respond to the correct job.
            /// </summary>
            public ulong JobID { get; private set; }


            /// <summary>
            /// Gets the inner callback message for this job.
            /// </summary>
            public T Callback { get; private set; }

#if STATIC_CALLBACKS
            internal JobCallback( SteamClient client, ulong jobId, T callback )
                : base( client )
#else
            internal JobCallback( ulong jobId, T callback )
#endif
            {
                JobID = jobId;
                Callback = callback;
            }
        }

        /// <summary>
        /// This callback is received after attempting to connect to the Steam network.
        /// </summary>
        public sealed class ConnectedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }

#if STATIC_CALLBACKS
            internal ConnectedCallback( SteamClient client, MsgChannelEncryptResult result )
                : this( client, result.Result )
#else
            internal ConnectedCallback( MsgChannelEncryptResult result )
                : this( result.Result )
#endif
            {
            }

#if STATIC_CALLBACKS
            internal ConnectedCallback( SteamClient client, EResult result )
                : base( client )
#else
            internal ConnectedCallback( EResult result )
#endif
            {
                this.Result = result;
            }
        }


        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
#if STATIC_CALLBACKS
            public DisconnectedCallback( SteamClient client )
                : base( client )
            {
            }
#endif
        }
    }
}