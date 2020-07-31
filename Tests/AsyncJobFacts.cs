using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class AsyncJobFacts
    {
        class Callback : CallbackMsg
        {
            public bool IsFinished { get; set; }
        }

        [Fact]
        public void AsyncJobCtorRegistersJob()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            Assert.True( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should contain the jobid key" );
            Assert.True( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should contain jobid key as a value type" );
        }

        [Fact]
        public void AysncJobCompletesOnCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            client.PostCallback( new Callback { JobID = 123 } );

            Assert.True( asyncTask.IsCompleted, "Async job should be completed after callback is posted" );
            Assert.False( asyncTask.IsCanceled, "Async job should not be canceled after callback is posted" );
            Assert.False( asyncTask.IsFaulted, "Async job should not be faulted after callback is posted" );
        }

        [Fact]
        public void AsyncJobClearsOnCompletion()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            client.PostCallback( new Callback { JobID = 123 } );

            Assert.False( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should no longer contain jobid key after callback is posted" );
            Assert.False( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should no longer contain jobid key (as value type) after callback is posted" );
        }

        [Fact]
        public async Task AsyncJobClearsOnTimeout()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            Assert.False( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should no longer contain jobid key after timeout" );
            Assert.False( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should no longer contain jobid key (as value type) after timeout" );
        }

        [Fact]
        public async Task AsyncJobCancelsOnSetFailedTimeout()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            asyncJob.SetFailed( dueToRemoteFailure: false );
            
            Assert.True( asyncTask.IsCompleted, "Async job should be completed on message timeout" );
            Assert.True( asyncTask.IsCanceled, "Async job should be canceled on message timeout" );
            Assert.False( asyncTask.IsFaulted, "Async job should not be faulted on message timeout" );

            await Assert.ThrowsAsync<TaskCanceledException>( async () => await asyncTask );
        }

        [Fact]
        public void AsyncJobGivesBackCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> jobTask = asyncJob.ToTask();

            Callback ourCallback = new Callback { JobID = 123 };

            client.PostCallback( ourCallback );

            Assert.Same( jobTask.Result, ourCallback );
        }

        [Fact]
        public async Task AsyncJobTimesout()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            Task<Callback> asyncTask = asyncJob.ToTask();

            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            Assert.True( asyncTask.IsCompleted, "Async job should be completed after 5 seconds of a 1 second job timeout" );
            Assert.True( asyncTask.IsCanceled, "Async job should be canceled after 5 seconds of a 1 second job timeout" );
            Assert.False( asyncTask.IsFaulted, "Async job should not be faulted after 5 seconds of a 1 second job timeout" );

            await Assert.ThrowsAsync<TaskCanceledException>( async () => await asyncTask );
        }

        [Fact]
        public void AsyncJobThrowsExceptionOnNullCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            Assert.Throws<ArgumentNullException>( () => asyncJob.AddResult( null ) );
        }

        [Fact]
        public async Task AsyncJobThrowsFailureExceptionOnFailure()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            asyncJob.SetFailed( dueToRemoteFailure: true );

            Assert.True( asyncTask.IsCompleted, "Async job should be completed after job failure" );
            Assert.False( asyncTask.IsCanceled, "Async job should not be canceled after job failure" );
            Assert.True( asyncTask.IsFaulted, "Async job should be faulted after job failure" );

            await Assert.ThrowsAsync<AsyncJobFailedException>( async () => await asyncTask );
        }

        [Fact]
        public void AsyncJobMultipleFinishedOnEmptyPredicate()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( new Callback { JobID = 123 } );

            Assert.True( jobFinished, "Async job should inform that it is completed when completion predicate is always true and a result is given" );
            Assert.True( asyncTask.IsCompleted, "Async job should be completed when empty predicate result is given" );
            Assert.False( asyncTask.IsCanceled, "Async job should not be canceled when empty predicate result is given" );
            Assert.False( asyncTask.IsFaulted, "Async job should not be faulted when empty predicate result is given" );
        }

        [Fact]
        public void AsyncJobMultipleFinishedOnPredicate()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( new Callback { JobID = 123, IsFinished = false } );

            Assert.False( jobFinished, "Async job should not inform that it is finished when completion predicate is false after a result is given" );
            Assert.False( asyncTask.IsCompleted, "Async job should not be completed when completion predicate is false" );
            Assert.False( asyncTask.IsCanceled, "Async job should not be canceled when completion predicate is false" );
            Assert.False( asyncTask.IsFaulted, "Async job should not be faulted when completion predicate is false" );

            jobFinished = asyncJob.AddResult( new Callback { JobID = 123, IsFinished = true } );

            Assert.True( jobFinished, "Async job should inform completion when completion predicat is passed after a result is given" );
            Assert.True( asyncTask.IsCompleted, "Async job should be completed when completion predicate is true" );
            Assert.False( asyncTask.IsCanceled, "Async job should not be canceled when completion predicate is true" );
            Assert.False( asyncTask.IsFaulted, "Async job should not be faulted when completion predicate is true" );
        }

        [Fact]
        public void AsyncJobMultipleClearsOnCompletion()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );

            client.PostCallback( new Callback { JobID = 123, IsFinished = true } );

            Assert.False( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should not contain jobid key for AsyncJobMultiple on completion" );
            Assert.False( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should not contain jobid key (as value type) for AsyncJobMultiple on completion" );
        }

        [Fact]
        public async Task AsyncJobMultipleClearsOnTimeout()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, ccall => true );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            Assert.False( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should no longer contain jobid key after timeout" );
            Assert.False( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should no longer contain jobid key (as value type) after timeout" );
        }

        [Fact]
        public async Task AsyncJobMultipleExtendsTimeoutOnMessage()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            asyncJob.Timeout = TimeSpan.FromSeconds( 5 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            // wait 3 seconds before we post any results to this job at all
            await Task.Delay( TimeSpan.FromSeconds( 3 ) );

            // we should not be completed or canceled yet
            Assert.False( asyncTask.IsCompleted, "AsyncJobMultiple should not be completed after 3 seconds of 5 second timeout" );
            Assert.False( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled after 3 seconds of 5 second timeout" );
            Assert.False( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted after 3 econds of 5 second timeout" );

            // give result 1 of 2
            asyncJob.AddResult( new Callback { JobID = 123, IsFinished = false } );

            // delay for what the original timeout would have been
            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            // we still shouldn't be completed or canceled (timed out)
            Assert.False( asyncTask.IsCompleted, "AsyncJobMultiple should not be completed 5 seconds after a result was added (result should extend timeout)" );
            Assert.False( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled 5 seconds after a result was added (result should extend timeout)" );
            Assert.False( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted 5 seconds aftr a result was added (result should extend timeout)" );

            asyncJob.AddResult( new Callback { JobID = 123, IsFinished = true } );

            // we should be completed but not canceled or faulted
            Assert.True( asyncTask.IsCompleted, "AsyncJobMultiple should be completed when final result is added to set" );
            Assert.False( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled when final result is added to set" );
            Assert.False( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted when final result is added to set" );
        }

        [Fact]
        public async Task AsyncJobMultipleTimesout()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => false );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            Assert.True( asyncTask.IsCompleted, "AsyncJobMultiple should be completed after 5 seconds of a 1 second job timeout" );
            Assert.True( asyncTask.IsCanceled, "AsyncJobMultiple should be canceled after 5 seconds of a 1 second job timeout" );
            Assert.False( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted after 5 seconds of a 1 second job timeout" );

            await Assert.ThrowsAsync<TaskCanceledException>( async () => await asyncTask );
        }

        [Fact]
        public async Task AsyncJobMultipleCompletesOnIncompleteResult()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            Callback onlyResult = new Callback { JobID = 123, IsFinished = false };

            asyncJob.AddResult( onlyResult );

            // adding a result will extend the job's timeout, but we'll cheat here and decrease it
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            await Task.Delay( TimeSpan.FromSeconds( 5 ) );

            Assert.True( asyncTask.IsCompleted, "AsyncJobMultiple should be completed on partial (timed out) result set" );
            Assert.False( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled on partial (timed out) result set" );
            Assert.False( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted on a partial (failed) result set" );

            AsyncJobMultiple<Callback>.ResultSet result = asyncTask.Result;

            Assert.False( result.Complete, "ResultSet should be incomplete" );
            Assert.False( result.Failed, "ResultSet should not be failed" );
            Assert.Single( result.Results );
            Assert.Contains( onlyResult, result.Results );
        }

        [Fact]
        public void AsyncJobMultipleCompletesOnIncompleteResultAndFailure()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            Callback onlyResult = new Callback { JobID = 123, IsFinished = false };

            asyncJob.AddResult( onlyResult );

            asyncJob.SetFailed( dueToRemoteFailure: true );

            Assert.True( asyncTask.IsCompleted, "AsyncJobMultiple should be completed on partial (failed) result set" );
            Assert.False( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled on partial (failed) result set" );
            Assert.False( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted on a partial (failed) result set" );

            AsyncJobMultiple<Callback>.ResultSet result = asyncTask.Result;

            Assert.False( result.Complete, "ResultSet should be incomplete" );
            Assert.True( result.Failed, "ResultSet should be failed" );
            Assert.Single( result.Results );
            Assert.Contains( onlyResult, result.Results );
        }

        [Fact]
        public void AsyncJobMultipleThrowsExceptionOnNullCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );

            Assert.Throws<ArgumentNullException>( () => asyncJob.AddResult( null ) );
        }

        [Fact]
        public async Task AsyncJobMultipleThrowsFailureExceptionOnFailure()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => false );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            asyncJob.SetFailed( dueToRemoteFailure: true );

            Assert.True( asyncTask.IsCompleted, "AsyncJobMultiple should be completed after job failure" );
            Assert.False( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled after job failure" );
            Assert.True( asyncTask.IsFaulted, "AsyncJobMultiple should be faulted after job failure" );

            await Assert.ThrowsAsync<AsyncJobFailedException>( async () => await asyncTask );
        }

        [Fact]
        public async Task AsyncJobContinuesAsynchronously()
        {
            SteamClient client = new SteamClient();

            var asyncJob = new AsyncJob<Callback>( client, 123 );
            var asyncTask = asyncJob.ToTask();

            var continuationThreadID = -1;
            var continuation = asyncTask.ContinueWith( t =>
            {
                continuationThreadID = Thread.CurrentThread.ManagedThreadId;
            }, TaskContinuationOptions.ExecuteSynchronously );

            var completionThreadID = Thread.CurrentThread.ManagedThreadId;
            asyncJob.AddResult( new Callback { JobID = 123 } );

            await continuation;

            Assert.NotEqual( -1, continuationThreadID );
            Assert.NotEqual( completionThreadID, continuationThreadID );
        }

        [Fact]
        public async Task AsyncJobMultipleContinuesAsynchronously()
        {
            SteamClient client = new SteamClient();

            var asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );
            var asyncTask = asyncJob.ToTask();

            var continuationThreadID = -1;
            var continuation = asyncTask.ContinueWith( t =>
            {
                continuationThreadID = Thread.CurrentThread.ManagedThreadId;
            }, TaskContinuationOptions.ExecuteSynchronously );

            var completionThreadID = Thread.CurrentThread.ManagedThreadId;
            asyncJob.AddResult( new Callback { JobID = 123 } );

            await continuation;

            Assert.NotEqual( -1, continuationThreadID );
            Assert.NotEqual( completionThreadID, continuationThreadID );
        }
    }
}
