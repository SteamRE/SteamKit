/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit2
{
    /// <summary>
    /// Represents an identifier of a network task known as a job.
    /// </summary>
    public class JobID : GlobalID
    {
        /// <summary>
        /// Represents an invalid JobID.
        /// </summary>
        public static readonly JobID Invalid = new JobID();


        /// <summary>
        /// Initializes a new instance of the <see cref="JobID"/> class.
        /// </summary>
        public JobID()
            : base()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JobID"/> class.
        /// </summary>
        /// <param name="jobId">The Job ID to initialize this instance with.</param>
        public JobID( ulong jobId )
            : base( jobId )
        {
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.JobID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="jobId">The Job ID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ulong ( JobID jobId )
        {
            return jobId.Value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.JobID"/>.
        /// </summary>
        /// <param name="jobId">The Job ID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JobID( ulong jobId )
        {
            return new JobID( jobId );
        }
    }

    public abstract class AsyncJob : JobID
    {
        DateTime jobStart;

        internal bool IsTimedout
        {
            get { return DateTime.UtcNow >= jobStart + TimeSpan.FromMinutes( 1 ); }
        }


        public AsyncJob( SteamClient client, ulong jobId )
            : base( jobId )
        {
            jobStart = DateTime.UtcNow;

            client.StartJob( this );
        }


        internal abstract void Complete( object callback );
    }

    public sealed class AsyncJob<T> : AsyncJob
        where T : CallbackMsg
    {
        TaskCompletionSource<T> tcs;


        public AsyncJob( SteamClient client, ulong jobId )
            : base( client, jobId )
        {
            tcs = new TaskCompletionSource<T>();
        }


        public Task<T> ToTask()
        {
            return tcs.Task;
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return ToTask().GetAwaiter();
        }


        internal override void Complete( object callback )
        {
            if ( callback == null )
            {
                // if we're completing with a null callback object, this is a signal that the job has been cancelled
                // without a valid result from the steam servers

                tcs.TrySetCanceled();

                return;
            }

            tcs.TrySetResult( (T)callback );
        }
    }
}
