using SteamKit2;
using System;
using Xunit;

namespace Tests
{
#pragma warning disable 0618
    public class CallbackManagerFacts_LegacyBehavior
    {
        public class CallbackForTest : CallbackMsg
        {
            public Guid UniqueID { get; set; }
        }

        public CallbackManagerFacts_LegacyBehavior()
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
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
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
        public void PostedCallbackTriggersAction_CatchAll()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackMsg> action = delegate (CallbackMsg cb)
            {
                Assert.IsType<CallbackForTest>(cb);
                var cft = (CallbackForTest)cb;
                Assert.Equal(callback.UniqueID, cft.UniqueID);
                didCall = true;
            };

            using (new Callback<CallbackMsg>(action, mgr))
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
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
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
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
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
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
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
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
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
#pragma warning restore 0618
}
