/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
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


            internal BaseJobCallback( ulong jobId )
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


            /// <summary>
            /// Initializes a new instance of the <see cref="JobCallback&lt;T&gt;"/> class.
            /// </summary>
            /// <param name="jobId">The for this callback.</param>
            /// <param name="callback">The inner callback object.</param>
            public JobCallback( JobID jobId, T callback )
                : base( jobId )
            {
                DebugLog.Assert( jobId != ulong.MaxValue, "JobCallback", "JobCallback used for non job based callback!" );

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


            internal ConnectedCallback( MsgChannelEncryptResult result )
                : this( result.Result )
            {
            }

            internal ConnectedCallback( EResult result )
            {
                this.Result = result;
            }
        }


        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
        }


        /// <summary>
        /// This callback is received when the client has received the CM list from Steam.
        /// </summary>
        public sealed class CMListCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the CM server list.
            /// </summary>
            public ReadOnlyCollection<IPEndPoint> Servers { get; private set; }


            internal CMListCallback( CMsgClientCMList cmMsg )
            {
                var cmList = cmMsg.cm_addresses
                    .Zip( cmMsg.cm_ports, ( addr, port ) => new IPEndPoint( NetHelpers.GetIPAddress( addr ), ( int )port ) );

                Servers = new ReadOnlyCollection<IPEndPoint>( cmList.ToList() );
            }
        }
    }
}