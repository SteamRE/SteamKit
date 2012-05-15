/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamClient
    {
        /// <summary>
        /// This callback serves as the base class for all job based callbacks.
        /// This allows you to retrieve results based on the Job ID without knowing the inner callback type.
        /// </summary>
        public abstract class BaseJobCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the Job ID of this callback. For client based jobs, this will match the Job ID of a function call.
            /// For server based jobs, this is provided to respond to the correct job.
            /// </summary>
            public JobID JobID { get; protected set; }

            /// <summary>
            /// Gets the type of the callback.
            /// </summary>
            public abstract Type CallbackType { get; }

#if STATIC_CALLBACKS
            internal BaseJobCallback( SteamClient client, ulong jobId )
                : base( client )
#else
            internal BaseJobCallback( ulong jobId )
#endif
            {
                this.JobID = jobId;
            }
        }


        /// <summary>
        /// This callback is received when a job related operation on the backend has completed, or a client operation should begin.
        /// </summary>
        /// <typeparam name="T">The inner callback this job represents.</typeparam>
        public sealed class JobCallback<T> : BaseJobCallback
            where T : CallbackMsg
        {
            /// <summary>
            /// Gets the type of the callback.
            /// </summary>
            public override Type CallbackType { get { return typeof( T ); } }

            /// <summary>
            /// Gets the inner callback message for this job.
            /// </summary>
            public T Callback { get; private set; }


#if STATIC_CALLBACKS
            internal JobCallback( SteamClient client, ulong jobId, T callback )
                : base( client, jobId )
#else
            internal JobCallback( JobID jobId, T callback )
                : base( jobId )
#endif
            {
                Debug.Assert( jobId != ulong.MaxValue, "JobCallback used for non job based callback!" );

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
            internal DisconnectedCallback( SteamClient client )
                : base( client )
            {
            }
#endif
        }
    }
}