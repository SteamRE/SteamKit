using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class AsyncJobFacts
    {
        class Callback : CallbackMsg
        {
            public bool IsFinished { get; set; }
        }

        [TestMethod]
        public void AsyncJobCtorRegistersJob()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            Assert.IsTrue( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should contain the jobid key" );
            Assert.IsTrue( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should contain jobid key as a value type" );
        }

        [TestMethod]
        public void AysncJobCompletesOnCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            client.PostCallback( new Callback { JobID = 123 } );

            Assert.IsTrue( asyncTask.IsCompleted, "Async job should be completed after callback is posted" );
            Assert.IsFalse( asyncTask.IsCanceled, "Async job should not be canceled after callback is posted" );
            Assert.IsFalse( asyncTask.IsFaulted, "Async job should not be faulted after callback is posted" );
        }

        [TestMethod]
        public void AsyncJobClearsOnCompletion()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            client.PostCallback( new Callback { JobID = 123 } );

            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should no longer contain jobid key after callback is posted" );
            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should no longer contain jobid key (as value type) after callback is posted" );
        }

        [TestMethod]
        public async Task AsyncJobClearsOnTimeout()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            client.jobManager.CancelTimedoutJobs();

            await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

            client.jobManager.CancelTimedoutJobs();

            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should no longer contain jobid key after timeout" );
            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should no longer contain jobid key (as value type) after timeout" );
        }

        [TestMethod]
        public async Task AsyncJobCancelsOnSetFailedTimeout()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            asyncJob.SetFailed( dueToRemoteFailure: false );

            Assert.IsTrue( asyncTask.IsCompleted, "Async job should be completed on message timeout" );
            Assert.IsTrue( asyncTask.IsCanceled, "Async job should be canceled on message timeout" );
            Assert.IsFalse( asyncTask.IsFaulted, "Async job should not be faulted on message timeout" );

            await Assert.ThrowsExceptionAsync<TaskCanceledException>( async () => await asyncTask );
        }

        [TestMethod]
        public async Task AsyncJobGivesBackCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> jobTask = asyncJob.ToTask();

            Callback ourCallback = new Callback { JobID = 123 };

            client.PostCallback( ourCallback );

            Assert.AreSame( await jobTask, ourCallback );
        }

        [TestMethod]
        public async Task AsyncJobTimesout()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            Task<Callback> asyncTask = asyncJob.ToTask();

            client.jobManager.CancelTimedoutJobs();

            await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

            client.jobManager.CancelTimedoutJobs();

            Assert.IsTrue( asyncTask.IsCompleted, "Async job should be completed after 5 seconds of a 1 second job timeout" );
            Assert.IsTrue( asyncTask.IsCanceled, "Async job should be canceled after 5 seconds of a 1 second job timeout" );
            Assert.IsFalse( asyncTask.IsFaulted, "Async job should not be faulted after 5 seconds of a 1 second job timeout" );

            await Assert.ThrowsExceptionAsync<TaskCanceledException>( async () => await asyncTask );
        }

        [TestMethod]
        public void AsyncJobThrowsExceptionOnNullCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            Assert.ThrowsException<ArgumentNullException>( () => asyncJob.AddResult( null ) );
        }

        [TestMethod]
        public async Task AsyncJobThrowsFailureExceptionOnFailure()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            asyncJob.SetFailed( dueToRemoteFailure: true );

            Assert.IsTrue( asyncTask.IsCompleted, "Async job should be completed after job failure" );
            Assert.IsFalse( asyncTask.IsCanceled, "Async job should not be canceled after job failure" );
            Assert.IsTrue( asyncTask.IsFaulted, "Async job should be faulted after job failure" );

            await Assert.ThrowsExceptionAsync<AsyncJobFailedException>( async () => await asyncTask );
        }

        [TestMethod]
        public void AsyncJobMultipleFinishedOnEmptyPredicate()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( new Callback { JobID = 123 } );

            Assert.IsTrue( jobFinished, "Async job should inform that it is completed when completion predicate is always true and a result is given" );
            Assert.IsTrue( asyncTask.IsCompleted, "Async job should be completed when empty predicate result is given" );
            Assert.IsFalse( asyncTask.IsCanceled, "Async job should not be canceled when empty predicate result is given" );
            Assert.IsFalse( asyncTask.IsFaulted, "Async job should not be faulted when empty predicate result is given" );
        }

        [TestMethod]
        public void AsyncJobMultipleFinishedOnPredicate()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( new Callback { JobID = 123, IsFinished = false } );

            Assert.IsFalse( jobFinished, "Async job should not inform that it is finished when completion predicate is false after a result is given" );
            Assert.IsFalse( asyncTask.IsCompleted, "Async job should not be completed when completion predicate is false" );
            Assert.IsFalse( asyncTask.IsCanceled, "Async job should not be canceled when completion predicate is false" );
            Assert.IsFalse( asyncTask.IsFaulted, "Async job should not be faulted when completion predicate is false" );

            jobFinished = asyncJob.AddResult( new Callback { JobID = 123, IsFinished = true } );

            Assert.IsTrue( jobFinished, "Async job should inform completion when completion predicat is passed after a result is given" );
            Assert.IsTrue( asyncTask.IsCompleted, "Async job should be completed when completion predicate is true" );
            Assert.IsFalse( asyncTask.IsCanceled, "Async job should not be canceled when completion predicate is true" );
            Assert.IsFalse( asyncTask.IsFaulted, "Async job should not be faulted when completion predicate is true" );
        }

        [TestMethod]
        public void AsyncJobMultipleClearsOnCompletion()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );

            client.PostCallback( new Callback { JobID = 123, IsFinished = true } );

            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should not contain jobid key for AsyncJobMultiple on completion" );
            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should not contain jobid key (as value type) for AsyncJobMultiple on completion" );
        }

        [TestMethod]
        public async Task AsyncJobMultipleClearsOnTimeout()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, ccall => true );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            client.jobManager.CancelTimedoutJobs();

            await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

            client.jobManager.CancelTimedoutJobs();

            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( asyncJob ), "Async job dictionary should no longer contain jobid key after timeout" );
            Assert.IsFalse( client.jobManager.asyncJobs.ContainsKey( 123 ), "Async job dictionary should no longer contain jobid key (as value type) after timeout" );
        }

        [TestMethod]
        public async Task AsyncJobMultipleExtendsTimeoutOnMessage()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 500 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            client.jobManager.CancelTimedoutJobs();

            // wait before we post any results to this job at all
            await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

            client.jobManager.CancelTimedoutJobs();

            // we should not be completed or canceled yet
            Assert.IsFalse( asyncTask.IsCompleted, "AsyncJobMultiple should not be completed yet" );
            Assert.IsFalse( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled yet" );
            Assert.IsFalse( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted yet" );
            Assert.IsFalse( asyncJob.IsTimedout, "AsyncJobMultiple should not be timed out yet" );

            // give result 1 of 2
            asyncJob.AddResult( new Callback { JobID = 123, IsFinished = false } );

            // delay for what the original timeout would have been
            await Task.Delay( TimeSpan.FromMilliseconds( 300 ) );

            client.jobManager.CancelTimedoutJobs();

            // we still shouldn't be completed or canceled (timed out)
            Assert.IsFalse( asyncTask.IsCompleted, "AsyncJobMultiple should not be completed after a result was added (result should extend timeout)" );
            Assert.IsFalse( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled after a result was added (result should extend timeout)" );
            Assert.IsFalse( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted after a result was added (result should extend timeout)" );
            Assert.IsFalse( asyncJob.IsTimedout, "AsyncJobMultiple should not be timed out (result should extend timeout)" );

            asyncJob.AddResult( new Callback { JobID = 123, IsFinished = true } );

            // we should be completed but not canceled or faulted
            Assert.IsTrue( asyncTask.IsCompleted, "AsyncJobMultiple should be completed when final result is added to set" );
            Assert.IsFalse( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled when final result is added to set" );
            Assert.IsFalse( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted when final result is added to set" );
        }

        [TestMethod]
        public async Task AsyncJobMultipleTimesout()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => false );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

            client.jobManager.CancelTimedoutJobs();

            Assert.IsTrue( asyncTask.IsCompleted, "AsyncJobMultiple should be completed" );
            Assert.IsTrue( asyncTask.IsCanceled, "AsyncJobMultiple should be canceled" );
            Assert.IsFalse( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted" );
            Assert.IsTrue( asyncJob.IsTimedout, "AsyncJobMultiple should be timed out" );

            await Assert.ThrowsExceptionAsync<TaskCanceledException>( async () => await asyncTask );
        }

        [TestMethod]
        public async Task AsyncJobMultipleCompletesOnIncompleteResult()
        {
            SteamClient client = new SteamClient();
            client.jobManager.SetTimeoutsEnabled( true );

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            Callback onlyResult = new Callback { JobID = 123, IsFinished = false };

            asyncJob.AddResult( onlyResult );

            // adding a result will extend the job's timeout, but we'll cheat here and decrease it
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

            client.jobManager.CancelTimedoutJobs();

            Assert.IsTrue( asyncTask.IsCompleted, "AsyncJobMultiple should be completed on partial (timed out) result set" );
            Assert.IsFalse( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled on partial (timed out) result set" );
            Assert.IsFalse( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted on a partial (failed) result set" );

            AsyncJobMultiple<Callback>.ResultSet result = await asyncTask;

            Assert.IsFalse( result.Complete, "ResultSet should be incomplete" );
            Assert.IsFalse( result.Failed, "ResultSet should not be failed" );
            Assert.IsTrue( result.Results.Count == 1 );
            Assert.IsTrue( result.Results.Contains( onlyResult ) );
        }

        [TestMethod]
        public async Task AsyncJobMultipleCompletesOnIncompleteResultAndFailure()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            asyncJob.Timeout = TimeSpan.FromMilliseconds( 100 );

            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            Callback onlyResult = new Callback { JobID = 123, IsFinished = false };

            asyncJob.AddResult( onlyResult );

            asyncJob.SetFailed( dueToRemoteFailure: true );

            client.jobManager.CancelTimedoutJobs();

            Assert.IsTrue( asyncTask.IsCompleted, "AsyncJobMultiple should be completed on partial (failed) result set" );
            Assert.IsFalse( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled on partial (failed) result set" );
            Assert.IsFalse( asyncTask.IsFaulted, "AsyncJobMultiple should not be faulted on a partial (failed) result set" );

            AsyncJobMultiple<Callback>.ResultSet result = await asyncTask;

            Assert.IsFalse( result.Complete, "ResultSet should be incomplete" );
            Assert.IsTrue( result.Failed, "ResultSet should be failed" );
            Assert.IsTrue( result.Results.Count == 1 );
            Assert.IsTrue( result.Results.Contains( onlyResult ) );
        }

        [TestMethod]
        public void AsyncJobMultipleThrowsExceptionOnNullCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );

            Assert.ThrowsException<ArgumentNullException>( () => asyncJob.AddResult( null ) );
        }

        [TestMethod]
        public async Task AsyncJobMultipleThrowsFailureExceptionOnFailure()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => false );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            asyncJob.SetFailed( dueToRemoteFailure: true );

            Assert.IsTrue( asyncTask.IsCompleted, "AsyncJobMultiple should be completed after job failure" );
            Assert.IsFalse( asyncTask.IsCanceled, "AsyncJobMultiple should not be canceled after job failure" );
            Assert.IsTrue( asyncTask.IsFaulted, "AsyncJobMultiple should be faulted after job failure" );

            await Assert.ThrowsExceptionAsync<AsyncJobFailedException>( async () => await asyncTask );
        }

        [TestMethod]
        public void AsyncJobContinuesAsynchronously()
        {
            SteamClient client = new SteamClient();

            var asyncJob = new AsyncJob<Callback>( client, 123 );
            var asyncTask = asyncJob.ToTask();

            var continuationThreadID = -1;
            var continuation = asyncTask.ContinueWith( t =>
            {
                continuationThreadID = Environment.CurrentManagedThreadId;
            }, TaskContinuationOptions.ExecuteSynchronously );

            var completionThreadID = Environment.CurrentManagedThreadId;
            asyncJob.AddResult( new Callback { JobID = 123 } );

            WaitForTaskWithoutRunningInline( continuation );

            Assert.AreNotEqual( -1, continuationThreadID );
            Assert.AreNotEqual( completionThreadID, continuationThreadID );
        }

        [TestMethod]
        public void AsyncJobMultipleContinuesAsynchronously()
        {
            SteamClient client = new SteamClient();

            var asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );
            var asyncTask = asyncJob.ToTask();

            var continuationThreadID = -1;
            var continuation = asyncTask.ContinueWith( t =>
            {
                continuationThreadID = Environment.CurrentManagedThreadId;
            }, TaskContinuationOptions.ExecuteSynchronously );

            var completionThreadID = Environment.CurrentManagedThreadId;
            asyncJob.AddResult( new Callback { JobID = 123 } );

            WaitForTaskWithoutRunningInline( continuation );

            Assert.AreNotEqual( -1, continuationThreadID );
            Assert.AreNotEqual( completionThreadID, continuationThreadID );
        }

        static void WaitForTaskWithoutRunningInline( Task task )
        {
            // If we await the task, our Thread can go back to the scheduler and come eligible to
            // run task continuations. If we call .Wait with an infinite timeout / no cancellation token, then
            // the .NET runtime will attempt to run the task inline... on the current thread.
            // To avoid that we need to supply a cancellable-but-never-cancelled token, or do other hackery
            // with IAsyncResult or mutexes. This appears to be the simplest.
            using var cts = new CancellationTokenSource();
            task.Wait( cts.Token );
        }
    }
}
