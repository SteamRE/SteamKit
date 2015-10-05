using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            Assert.True( client.asyncJobs.ContainsKey( asyncJob ) );
            Assert.True( client.asyncJobs.ContainsKey( 123 ) );
        }

        [Fact]
        public void AysncJobCompletesOnCallback()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            client.PostCallback( new Callback { JobID = 123 } );

            Assert.True( asyncTask.IsCompleted );
            Assert.False( asyncTask.IsCanceled );
            Assert.False( asyncTask.IsFaulted );
        }

        [Fact]
        public void AsyncJobClearsOnCompletion()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );

            client.PostCallback( new Callback { JobID = 123 } );

            Assert.False( client.asyncJobs.ContainsKey( asyncJob ) );
            Assert.False( client.asyncJobs.ContainsKey( 123 ) );
        }

        [Fact]
        public async void AsyncJobCancelsOnNull()
        {
            SteamClient client = new SteamClient();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            Task<Callback> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( null );

            Assert.True( jobFinished );
            Assert.True( asyncTask.IsCompleted );
            Assert.True( asyncTask.IsCanceled );
            
            await Assert.ThrowsAsync( typeof( TaskCanceledException ), async () => await asyncTask );
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
        public async void AsyncJobTimesout()
        {
            SteamClient client = new SteamClient();
            client.jobTimeoutFunc.Start();

            AsyncJob<Callback> asyncJob = new AsyncJob<Callback>( client, 123 );
            asyncJob.Timeout = TimeSpan.FromSeconds( 1 );

            Task<Callback> asyncTask = asyncJob.ToTask();

            await Task.Delay( TimeSpan.FromSeconds( 10 ) );

            Assert.True( asyncTask.IsCompleted );
            Assert.True( asyncTask.IsCanceled );

            await Assert.ThrowsAsync( typeof( TaskCanceledException ), async () => await asyncTask );
        }

        [Fact]
        public void AsyncJobMultipleFinishedOnEmptyPredicate()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => true );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( new Callback { JobID = 123 } );

            Assert.True( jobFinished );
            Assert.True( asyncTask.IsCompleted );
            Assert.False( asyncTask.IsCanceled );
            Assert.False( asyncTask.IsFaulted );
        }

        [Fact]
        public void AsyncJobMultipleFinishedOnPredicate()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );
            Task<AsyncJobMultiple<Callback>.ResultSet> asyncTask = asyncJob.ToTask();

            bool jobFinished = asyncJob.AddResult( new Callback { JobID = 123, IsFinished = false } );

            Assert.False( jobFinished );
            Assert.False( asyncTask.IsCompleted );
            Assert.False( asyncTask.IsCanceled );
            Assert.False( asyncTask.IsFaulted );

            jobFinished = asyncJob.AddResult( new Callback { JobID = 123, IsFinished = true } );

            Assert.True( jobFinished );
            Assert.True( asyncTask.IsCompleted );
            Assert.False( asyncTask.IsCanceled );
            Assert.False( asyncTask.IsFaulted );
        }

        [Fact]
        public void AsyncJobMultipleClearsOnCompletion()
        {
            SteamClient client = new SteamClient();

            AsyncJobMultiple<Callback> asyncJob = new AsyncJobMultiple<Callback>( client, 123, call => call.IsFinished );

            client.PostCallback( new Callback { JobID = 123, IsFinished = true } );

            Assert.False( client.asyncJobs.ContainsKey( asyncJob ) );
            Assert.False( client.asyncJobs.ContainsKey( 123 ) );
        }
    }
}
