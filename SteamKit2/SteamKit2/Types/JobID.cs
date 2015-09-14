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

    /// <summary>
    /// The base class for awaitable versions of a <see cref="JobID"/>.
    /// Should not be used or constructed directly, but rather with <see cref="AsyncJob{T}"/>.
    /// </summary>
    public abstract class AsyncJob : JobID
    {
        DateTime jobStart;


        /// <summary>
        /// Gets or sets the period of time before this job will be considered timed out and will be canceled. By default this is 1 minute.
        /// </summary>
        /// <value>
        /// The timeout value.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes( 1 );

        internal bool IsTimedout
        {
            get { return DateTime.UtcNow >= jobStart + Timeout; }
        }


        internal AsyncJob( SteamClient client, ulong jobId )
            : base( jobId )
        {
            jobStart = DateTime.UtcNow;

            client.StartJob( this );
        }


        internal abstract void Complete( object callback );
    }

    /// <summary>
    /// Represents an awaitable version of a <see cref="JobID"/>.
    /// Can either be converted to a TPL <see cref="Task"/> with <see cref="ToTask"/> or can be awaited directly.
    /// </summary>
    /// <typeparam name="T">The callback type that will be returned by this async job.</typeparam>
    public sealed class AsyncJob<T> : AsyncJob
        where T : CallbackMsg
    {
        TaskCompletionSource<T> tcs;


        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJob{T}" /> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> that this job will be associated with.</param>
        /// <param name="jobId">The Job ID value associated with this async job.</param>
        public AsyncJob( SteamClient client, ulong jobId )
            : base( client, jobId )
        {
            tcs = new TaskCompletionSource<T>();
        }


        /// <summary>
        /// Converts this <see cref="AsyncJob{T}"/> instance into a TPL <see cref="Task{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Task<T> ToTask()
        {
            return tcs.Task;
        }

        /// <summary>Gets an awaiter used to await this <see cref="AsyncJob{T}"/>.</summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler use rather than use directly in code.</remarks>
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
