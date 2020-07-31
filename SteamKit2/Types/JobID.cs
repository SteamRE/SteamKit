/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            if ( jobId == null )
            {
                throw new ArgumentNullException( nameof(jobId) );
            }

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

        /// <summary>
        /// Performs an implicit conversion from <see cref="AsyncJob"/> to <see cref="JobID"/>.
        /// </summary>
        /// <param name="asyncJob">The asynchronous job.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JobID( AsyncJob asyncJob )
        {
            if ( asyncJob == null )
            {
                throw new ArgumentNullException( nameof(asyncJob) );
            }

            return asyncJob.JobID;
        }
    }

    /// <summary>
    /// The base class for awaitable versions of a <see cref="JobID"/>.
    /// Should not be used or constructed directly, but rather with <see cref="AsyncJob{T}"/>.
    /// </summary>
    public abstract class AsyncJob
    {
        DateTime jobStart;


        /// <summary>
        /// Gets the <see cref="JobID"/> for this job.
        /// </summary>
        public JobID JobID { get; }

        /// <summary>
        /// Gets or sets the period of time before this job will be considered timed out and will be canceled. By default this is 10 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds( 10 );

        internal bool IsTimedout
        {
            get { return DateTime.UtcNow >= jobStart + Timeout; }
        }


        internal AsyncJob( SteamClient client, JobID jobId )
        {
            if ( client == null )
            {
                throw new ArgumentNullException( nameof(client) );
            }
            
            if ( jobId == null )
            {
                throw new ArgumentNullException( nameof(jobId) );
            }

            jobStart = DateTime.UtcNow;
            JobID = jobId;

            client.StartJob( this );
        }


        /// <summary>
        /// Adds a callback to the async job's result set.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns><c>true</c> if this result completes the set; otherwise, <c>false</c>.</returns>
        internal abstract bool AddResult( CallbackMsg callback );

        /// <summary>
        /// Sets this job as failed, either remotely or due to a message timeout.
        /// </summary>
        /// <param name="dueToRemoteFailure">
        /// If set to <c>true</c> this job is marked as failed because Steam informed us of a job failure;
        /// otherwise, this job has failed due to a message timeout.
        /// </param>
        internal abstract void SetFailed( bool dueToRemoteFailure );

        /// <summary>
        /// Marks this job as having received a heartbeat and extends the job's timeout.
        /// </summary>
        internal void Heartbeat()
        {
            // extend timeout for this job, as Steam is informing us that more messages will follow
            Timeout += TimeSpan.FromSeconds( 10 );
        }
    }

    /// <summary>
    /// Represents an awaitable version of a <see cref="JobID"/>.
    /// Can either be converted to a TPL <see cref="Task"/> with <see cref="ToTask"/> or can be awaited directly.
    /// </summary>
    /// <typeparam name="T">The callback type that will be returned by this async job.</typeparam>
    public sealed class AsyncJob<T> : AsyncJob
        where T : CallbackMsg
    {
        readonly TaskCompletionSource<T> tcs;


        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJob{T}" /> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> that this job will be associated with.</param>
        /// <param name="jobId">The Job ID value associated with this async job.</param>
        public AsyncJob( SteamClient client, JobID jobId )
            : base( client, jobId )
        {
            tcs = new TaskCompletionSource<T>( TaskCreationOptions.RunContinuationsAsynchronously );
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


        /// <summary>
        /// Adds a callback to the async job's result set. For an <see cref="AsyncJob{T}"/>, this always completes the set.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns>Always <c>true</c>.</returns>
        internal override bool AddResult( CallbackMsg callback )
        {
            if ( callback == null )
            {
                throw new ArgumentNullException( nameof( callback ) );
            }

            // we're complete with just this callback
            tcs.TrySetResult( (T)callback );

            // inform steamclient that this job wishes to be removed from tracking since we've recieved the single callback we were waiting for
            return true;
        }

        /// <summary>
        /// Sets this job as failed, either remotely or due to a message timeout.
        /// </summary>
        /// <param name="dueToRemoteFailure">
        /// If set to <c>true</c> this job is marked as failed because Steam informed us of a job failure;
        /// otherwise, this job has failed due to a message timeout.
        /// </param>
        internal override void SetFailed( bool dueToRemoteFailure )
        {
            if ( dueToRemoteFailure )
            {
                // if steam informs us of a remote failure, we cancel with our exception
                tcs.TrySetException( new AsyncJobFailedException() );
            }
            else
            {
                // if we time out, we trigger a normal cancellation
                tcs.TrySetCanceled();
            }
            
        }
    }

    /// <summary>
    /// Represents an awaitable version of a <see cref="JobID"/>.
    /// Can either be converted to a TPL <see cref="Task"/> with <see cref="ToTask"/> or can be awaited directly.
    /// This type of async job can contain multiple callback results.
    /// </summary>
    /// <typeparam name="T">The callback type that will be returned by this async job.</typeparam>
    public sealed class AsyncJobMultiple<T> : AsyncJob
        where T : CallbackMsg
    {
        /// <summary>
        /// The set of callback results for an <see cref="AsyncJobMultiple{T}"/>.
        /// </summary>
        public sealed class ResultSet
        {
            /// <summary>
            /// Gets a value indicating whether this <see cref="AsyncJobMultiple{T}.ResultSet" /> is complete and contains every result sent by Steam.
            /// </summary>
            public bool Complete { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether the parent <see cref="AsyncJobMultiple{T}" /> received an incomplete result set and then encountered a remote failure.
            /// </summary>
            public bool Failed { get; internal set; }

            /// <summary>
            /// Gets a read only collection of callback results for this async job.
            /// </summary>
            public ReadOnlyCollection<T>? Results { get; internal set; }
        }


        TaskCompletionSource<ResultSet> tcs;
        Predicate<T> finishCondition;

        List<T> results = new List<T>();


        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJob{T}" /> class.
        /// </summary>
        /// <param name="client">The <see cref="SteamClient"/> that this job will be associated with.</param>
        /// <param name="jobId">The Job ID value associated with this async job.</param>
        /// <param name="finishCondition">The condition that must be fulfilled for the result set to be considered complete.</param>
        public AsyncJobMultiple( SteamClient client, JobID jobId, Predicate<T> finishCondition )
                    : base( client, jobId )
        {
            tcs = new TaskCompletionSource<ResultSet>( TaskCreationOptions.RunContinuationsAsynchronously );

            this.finishCondition = finishCondition;
        }


        /// <summary>
        /// Converts this <see cref="AsyncJob{T}"/> instance into a TPL <see cref="Task{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Task<ResultSet> ToTask()
        {
            return tcs.Task;
        }

        /// <summary>Gets an awaiter used to await this <see cref="AsyncJob{T}"/>.</summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler use rather than use directly in code.</remarks>
        public TaskAwaiter<ResultSet> GetAwaiter()
        {
            return ToTask().GetAwaiter();
        }


        /// <summary>
        /// Adds a callback to the async job's result set.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns><c>true</c> if this result completes the set; otherwise, <c>false</c>.</returns>
        internal override bool AddResult( CallbackMsg callback )
        {
            if ( callback == null )
            {
                throw new ArgumentNullException( nameof( callback ) );
            }

            T callbackMsg = (T)callback;

            // add this callback to our result set
            results.Add( callbackMsg );

            if ( finishCondition( callbackMsg ) )
            {
                // if we've passed our finish condition based on this callback
                // (for instance, if steam tells us we have no more pending messages for this job)
                // then we're complete

                tcs.TrySetResult( new ResultSet { Complete = true, Results = new ReadOnlyCollection<T>( results ) } );

                return true;
            }
            else
            {
                // otherwise, we're not complete and we'll wait for the next message
                // trigger heartbeat logic to keep this job alive as it waits for the next message
                Heartbeat();

                return false;
            }
        }

        /// <summary>
        /// Sets this job as failed, either remotely or due to a message timeout.
        /// </summary>
        /// <param name="dueToRemoteFailure">
        /// If set to <c>true</c> this job is marked as failed because Steam informed us of a job failure;
        /// otherwise, this job has failed due to a message timeout.
        /// </param>
        internal override void SetFailed( bool dueToRemoteFailure )
        {
            // steamclient is informing this async job that we've either timed out or failed while waiting for an additional callback
            // now we have to determine what to do:

            if ( results.Count == 0 )
            {
                // if we have zero callbacks in our result set, we cancel this task

                if ( dueToRemoteFailure )
                {
                    // if we're canceling with a remote failure, post a job failure exception
                    tcs.TrySetException( new AsyncJobFailedException() );
                }
                else
                {
                    // otherwise, normal task cancelation for timeouts
                    tcs.TrySetCanceled();
                }
            }
            else
            {
                // if we do have a partial result set, we can complete this job with the results we have and let consumers
                // work out what they want to do with the incomplete set

                var resultSet = new ResultSet
                {
                    // we're not a complete set
                    Complete = false,
                    // let consumers know if we've failed because of a remote failure
                    Failed = dueToRemoteFailure,

                    Results = new ReadOnlyCollection<T>( results ),
                };

                tcs.TrySetResult( resultSet );
            }
        }
    }
}
