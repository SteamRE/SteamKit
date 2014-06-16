using System;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class CallbackManagerFacts
    {
        public class CallbackForTest : CallbackMsg
        {
            public Guid UniqueID { get; set; }
        }

        public CallbackManagerFacts()
        {
            client = new SteamClient();
            mgr = new CallbackManager(client);
        }

        readonly SteamClient client;
        readonly CallbackManager mgr;

        [Fact]
        public void PostedCallbackTriggersAction()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.Equal(callback.UniqueID, cb.UniqueID);
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr))
            {
                PostAndRunCallback(callback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedCallbackTriggersActionForExplicitJobIDInvalid()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.Equal(callback.UniqueID, cb.UniqueID);
                Assert.Equal(jobID, cb.JobID);
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr, JobID.Invalid))
            {
                PostAndRunCallback(callback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedCallbackWithJobIDTriggersActionWhenNoJobIDSpecified()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.Equal(callback.UniqueID, cb.UniqueID);
                Assert.Equal(jobID, cb.JobID);
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr))
            {
                PostAndRunCallback(callback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedCallbackDoesNotTriggerActionForWrongJobID()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr, new JobID(123)))
            {
                PostAndRunCallback(callback);
            }

            Assert.False(didCall);
        }

        [Fact]
        public void PostedCallbackWithJobIDTriggersCallbackForJobID()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.Equal(callback.UniqueID, cb.UniqueID);
                Assert.Equal(jobID, cb.JobID);
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr, new JobID(123456)))
            {
                PostAndRunCallback(callback);
            }

            Assert.True(didCall);
        }

        void PostAndRunCallback(CallbackMsg callback)
        {
            client.PostCallback(callback);
            mgr.RunCallbacks();
        }
    }
}
