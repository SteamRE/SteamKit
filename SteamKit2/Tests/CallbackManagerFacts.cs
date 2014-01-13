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
                Assert.Equal(cb.UniqueID, callback.UniqueID);
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr))
            {
                PostAndRunCallback(callback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedJobCallbackTriggersAction()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };
            var jobID = new JobID(123456);
            var jobCallback = new SteamClient.JobCallback<CallbackForTest>(jobID, callback);

            var didCall = false;
            Action<CallbackForTest, JobID> action = delegate(CallbackForTest cb, JobID jid)
            {
                Assert.Equal(cb.UniqueID, callback.UniqueID);
                Assert.Equal(jobID, jid);
                didCall = true;
            };

            using (new JobCallback<CallbackForTest>(action, mgr))
            {
                PostAndRunCallback(jobCallback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedJobCallbackTriggersActionForExplicitJobIDInvalid()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };
            var jobID = new JobID(123456);
            var jobCallback = new SteamClient.JobCallback<CallbackForTest>(jobID, callback);

            var didCall = false;
            Action<CallbackForTest, JobID> action = delegate(CallbackForTest cb, JobID jid)
            {
                Assert.Equal(cb.UniqueID, callback.UniqueID);
                Assert.Equal(jobID, jid);
                didCall = true;
            };

            using (new JobCallback<CallbackForTest>(action, mgr, JobID.Invalid))
            {
                PostAndRunCallback(jobCallback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedJobCallbackDoesNotTriggerActionForWrongJobID()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };
            var jobID = new JobID(123456);
            var jobCallback = new SteamClient.JobCallback<CallbackForTest>(jobID, callback);

            var didCall = false;
            Action<CallbackForTest, JobID> action = delegate(CallbackForTest cb, JobID jid)
            {
                Assert.Equal(jobID, jid);
                didCall = true;
            };

            using (new JobCallback<CallbackForTest>(action, mgr, new JobID(123)))
            {
                PostAndRunCallback(jobCallback);
            }

            Assert.False(didCall);
        }

        [Fact]
        public void PostedJobCallbackTriggersCallbackForJobID()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };
            var jobID = new JobID(123456);
            var jobCallback = new SteamClient.JobCallback<CallbackForTest>(jobID, callback);

            var didCall = false;
            Action<CallbackForTest, JobID> action = delegate(CallbackForTest cb, JobID jid)
            {
                Assert.Equal(jobID, jid);
                didCall = true;
            };

            using (new JobCallback<CallbackForTest>(action, mgr, jobID))
            {
                PostAndRunCallback(jobCallback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void PostedJobCallbackDoesNotTriggerCallback()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };
            var jobID = new JobID(123456);
            var jobCallback = new SteamClient.JobCallback<CallbackForTest>(jobID, callback);

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                didCall = true;
            };

            using (new Callback<CallbackForTest>(action, mgr))
            {
                PostAndRunCallback(jobCallback);
            }

            Assert.False(didCall);
        }

        [Fact]
        public void PostedCallbackDoesNotTriggerJobCallback()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest, JobID> action = delegate(CallbackForTest cb, JobID jid)
            {
                didCall = true;
            };

            using (new JobCallback<CallbackForTest>(action, mgr))
            {
                PostAndRunCallback(callback);
            }

            Assert.False(didCall);
        }

        void PostAndRunCallback(CallbackMsg callback)
        {
            client.PostCallback(callback);
            mgr.RunCallbacks();
        }
    }
}
