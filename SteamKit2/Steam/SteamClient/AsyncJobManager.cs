using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit2
{
    class AsyncJobManager
    {
        internal ConcurrentDictionary<JobID, AsyncJob> asyncJobs;
        internal ScheduledFunction jobTimeoutFunc;

        public AsyncJobManager()
        {
            asyncJobs = new ConcurrentDictionary<JobID, AsyncJob>();

            jobTimeoutFunc = new ScheduledFunction( CancelTimedoutJobs, TimeSpan.FromSeconds( 1 ) );
        }


        /// <summary>
        /// Tracks a job with this manager.
        /// </summary>
        /// <param name="asyncJob">The asynchronous job to track.</param>
        public void StartJob( AsyncJob asyncJob )
        {
            asyncJobs.TryAdd( asyncJob, asyncJob );
        }
        /// <summary>
        /// Passes a callback to a pending async job.
        /// If the given callback completes the job, the job is removed from this manager.
        /// </summary>
        /// <param name="jobId">The JobID.</param>
        /// <param name="callback">The callback.</param>
        public void TryCompleteJob( JobID jobId, CallbackMsg callback )
        {
            var asyncJob = GetJob( jobId );

            if ( asyncJob == null )
            {
                // not a job we are tracking ourselves, can ignore it
                return;
            }

            // pass this callback into the job so it can determine if the job is finished (in the case of multiple responses to a job)
            bool jobFinished = asyncJob.AddResult( callback );

            if ( jobFinished )
            {
                // if the job is finished, we can stop tracking it
                asyncJobs.TryRemove( jobId, out asyncJob );
            }
        }

        /// <summary>
        /// Extends the lifetime of a job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public void HeartbeatJob( JobID jobId )
        {
            var asyncJob = GetJob( jobId );

            if ( asyncJob == null )
            {
                // ignore heartbeats for jobs we're not tracking
                return;
            }

            asyncJob.Heartbeat();
        }
        /// <summary>
        /// Marks a certain job as remotely failed.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public void FailJob( JobID jobId )
        {
            var asyncJob = GetJob( jobId, andRemove: true );

            if ( asyncJob == null )
            {
                // ignore remote failures for jobs we're not tracking
                return;
            }

            asyncJob.SetFailed( dueToRemoteFailure: true );
            
        }

        /// <summary>
        /// Cancels and clears all jobs being tracked.
        /// </summary>
        public void CancelPendingJobs()
        {
            foreach ( AsyncJob asyncJob in asyncJobs.Values )
            {
                asyncJob.SetFailed( dueToRemoteFailure: false );
            }

            asyncJobs.Clear();
        }


        /// <summary>
        /// Enables or disables periodic checks for job timeouts.
        /// </summary>
        /// <param name="enable">Whether or not job timeout checks should be enabled.</param>
        public void SetTimeoutsEnabled( bool enable )
        {
            if ( enable )
            {
                jobTimeoutFunc.Start();
            }
            else
            {
                jobTimeoutFunc.Stop();
            }
        }


        /// <summary>
        /// This is called periodically to cancel and clear out any jobs that have timed out (no response from Steam).
        /// </summary>
        void CancelTimedoutJobs()
        {
            // ConcurrentDictionary.Values performs a full copy, so this iteration is safe
            // see: http://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/ConcurrentDictionary.cs,fe55c11912af21d2
            foreach ( AsyncJob job in asyncJobs.Values )
            {
                if ( job.IsTimedout )
                {
                    job.SetFailed( dueToRemoteFailure: false );

                    AsyncJob ignored;
                    asyncJobs.TryRemove( job, out ignored );
                }
            }
        }


        /// <summary>
        /// Retrieves a job from this manager, and optionally removes it from tracking.
        /// </summary>
        /// <param name="jobId">The JobID.</param>
        /// <param name="andRemove">If set to <c>true</c>, this job is removed from tracking.</param>
        /// <returns></returns>
        AsyncJob? GetJob( JobID jobId, bool andRemove = false )
        {
            AsyncJob asyncJob;
            bool foundJob = false;

            if ( andRemove )
            {
                foundJob = asyncJobs.TryRemove( jobId, out asyncJob );
            }
            else
            {
                foundJob = asyncJobs.TryGetValue( jobId, out asyncJob );
            }

            if ( !foundJob )
            {
                // requested a job we're not tracking
                return null;
            }

            return asyncJob;
        }
    }


    /// <summary>
    /// Thrown when Steam encounters a remote error with a pending <see cref="AsyncJob"/>.
    /// </summary>
    public class AsyncJobFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJobFailedException"/> class.
        /// </summary>
        public AsyncJobFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJobFailedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AsyncJobFailedException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJobFailedException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AsyncJobFailedException( string message, Exception innerException )
            : base( message, innerException )
        {
        }
    }
}
