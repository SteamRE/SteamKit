using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
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

        [TestMethod]
        public void PostedCallbackTriggersAction()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.AreEqual(callback.UniqueID, cb.UniqueID);
                didCall = true;
            };

            using (mgr.Subscribe(action))
            {
                PostAndRunCallback(callback);
            }

            Assert.IsTrue(didCall);
        }

        [TestMethod]
        public void PostedCallbackTriggersAction_CatchAll()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackMsg> action = delegate(CallbackMsg cb)
            {
                Assert.IsInstanceOfType<CallbackForTest>(cb);
                var cft = (CallbackForTest)cb;
                Assert.AreEqual(callback.UniqueID, cft.UniqueID);
                didCall = true;
            };

            using (mgr.Subscribe(action))
            {
                PostAndRunCallback(callback);
            }

            Assert.IsTrue(didCall);
        }

        [TestMethod]
        public void PostedCallbackTriggersActionForExplicitJobIDInvalid()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.AreEqual(callback.UniqueID, cb.UniqueID);
                Assert.AreEqual(jobID, cb.JobID);
                didCall = true;
            };

            using (mgr.Subscribe(JobID.Invalid, action))
            {
                PostAndRunCallback(callback);
            }

            Assert.IsTrue(didCall);
        }

        [TestMethod]
        public void PostedCallbackWithJobIDTriggersActionWhenNoJobIDSpecified()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.AreEqual(callback.UniqueID, cb.UniqueID);
                Assert.AreEqual(jobID, cb.JobID);
                didCall = true;
            };

            using (mgr.Subscribe(action))
            {
                PostAndRunCallback(callback);
            }

            Assert.IsTrue(didCall);
        }

        [TestMethod]
        public void PostedCallbackDoesNotTriggerActionForWrongJobID()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                didCall = true;
            };

            using (mgr.Subscribe(123, action))
            {
                PostAndRunCallback(callback);
            }

            Assert.IsFalse(didCall);
        }

        [TestMethod]
        public void PostedCallbackWithJobIDTriggersCallbackForJobID()
        {
            var jobID = new JobID(123456);
            var callback = new CallbackForTest { JobID = jobID, UniqueID = Guid.NewGuid() };

            var didCall = false;
            Action<CallbackForTest> action = delegate(CallbackForTest cb)
            {
                Assert.AreEqual(callback.UniqueID, cb.UniqueID);
                Assert.AreEqual(jobID, cb.JobID);
                didCall = true;
            };

            using (mgr.Subscribe(123456, action))
            {
                PostAndRunCallback(callback);
            }

            Assert.IsTrue(didCall);
        }

        [TestMethod]
        public void SubscribedFunctionDoesNotRunWhenSubscriptionIsDisposed()
        {
            var callback = new CallbackForTest();

            var callCount = 0;
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
            {
                callCount++;
            };

            using (mgr.Subscribe(action))
            {
                PostAndRunCallback(callback);
            }
            PostAndRunCallback(callback);

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void PostedCallbacksTriggerActions()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var numCallbacksRun = 0;
            Action<CallbackForTest> action = delegate (CallbackForTest cb)
            {
                Assert.AreEqual(callback.UniqueID, cb.UniqueID);
                numCallbacksRun++;
            };

            using (mgr.Subscribe(action))
            {
                for (var i = 0; i < 10; i++)
                {
                    client.PostCallback(callback);
                }

                mgr.RunWaitAllCallbacks(TimeSpan.Zero);
                Assert.AreEqual(10, numCallbacksRun);

                // Callbacks should have been freed.
                mgr.RunWaitAllCallbacks(TimeSpan.Zero);
                Assert.AreEqual(10, numCallbacksRun);
            }
        }

        void PostAndRunCallback(CallbackMsg callback)
        {
            client.PostCallback(callback);
            mgr.RunCallbacks();
        }
    }
}
