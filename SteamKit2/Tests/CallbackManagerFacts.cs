﻿using System;
using System.Threading.Tasks;
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
            void action( CallbackForTest cb )
            {
                Assert.Equal( callback.UniqueID, cb.UniqueID );
                didCall = true;
            }

            using (mgr.Subscribe<CallbackForTest>( action ))
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
            void action( CallbackMsg cb )
            {
                Assert.IsType<CallbackForTest>( cb );
                var cft = ( CallbackForTest )cb;
                Assert.Equal( callback.UniqueID, cft.UniqueID );
                didCall = true;
            }

            using ( mgr.Subscribe<CallbackMsg>( action ) )
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
            void action( CallbackForTest cb )
            {
                Assert.Equal( callback.UniqueID, cb.UniqueID );
                Assert.Equal( jobID, cb.JobID );
                didCall = true;
            }

            using ( mgr.Subscribe<CallbackForTest>( JobID.Invalid, action ) )
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
            void action( CallbackForTest cb )
            {
                Assert.Equal( callback.UniqueID, cb.UniqueID );
                Assert.Equal( jobID, cb.JobID );
                didCall = true;
            }

            using ( mgr.Subscribe<CallbackForTest>( action ) )
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
            void action( CallbackForTest cb )
            {
                didCall = true;
            }

            using ( mgr.Subscribe<CallbackForTest>( 123, action ) )
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
            void action( CallbackForTest cb )
            {
                Assert.Equal( callback.UniqueID, cb.UniqueID );
                Assert.Equal( jobID, cb.JobID );
                didCall = true;
            }

            using ( mgr.Subscribe<CallbackForTest>( 123456, action ) )
            {
                PostAndRunCallback(callback);
            }

            Assert.True(didCall);
        }

        [Fact]
        public void SubscribedFunctionDoesNotRunWhenSubscriptionIsDisposed()
        {
            var callback = new CallbackForTest();

            var callCount = 0;
            void action( CallbackForTest cb )
            {
                callCount++;
            }

            using ( mgr.Subscribe<CallbackForTest>( action ) )
            {
                PostAndRunCallback(callback);
            }
            PostAndRunCallback(callback);

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void PostedCallbacksTriggerActions()
        {
            var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };

            var numCallbacksRun = 0;
            void action( CallbackForTest cb )
            {
                Assert.Equal( callback.UniqueID, cb.UniqueID );
                numCallbacksRun++;
            }

            using ( mgr.Subscribe<CallbackForTest>( action ) )
            {
                for (var i = 0; i < 10; i++)
                {
                    client.PostCallback(callback);
                }

                mgr.RunWaitAllCallbacks(TimeSpan.Zero);
                Assert.Equal(10, numCallbacksRun);

                // Callbacks should have been freed.
                mgr.RunWaitAllCallbacks(TimeSpan.Zero);
                Assert.Equal(10, numCallbacksRun);
            }
        }

        [Fact]
        public async Task PostedCallbacksTriggerActionsAsync()
        {
            var callbacks = new CallbackForTest[ 10 ];

            var numCallbacksRun = 0;
            void action( CallbackForTest cb )
            {
                Assert.True( numCallbacksRun < callbacks.Length );
                var callback = callbacks[ numCallbacksRun ];
                Assert.Equal( callback.UniqueID, cb.UniqueID );
                numCallbacksRun++;
            }

            using ( mgr.Subscribe<CallbackForTest>( action ) )
            {
                for ( var i = 0; i < callbacks.Length; i++ )
                {
                    var callback = new CallbackForTest { UniqueID = Guid.NewGuid() };
                    callbacks[ i ] = callback;
                    client.PostCallback( callback );
                }

                for ( var i = 1; i <= callbacks.Length; i++ )
                {
                    await mgr.RunWaitCallbackAsync( TestContext.Current.CancellationToken );
                    Assert.Equal( i, numCallbacksRun );
                }

                // Callbacks should have been freed.
                mgr.RunWaitAllCallbacks( TimeSpan.Zero );
                Assert.Equal( 10, numCallbacksRun );
            }
        }

        [Fact]
        public void CorrectlyUnsubscribesFromInsideOfCallback()
        {
            static void nothing( CallbackForTest cb )
            {
                //
            }

            using var s1 = mgr.Subscribe<CallbackForTest>( nothing );

            IDisposable subscription = null;

            void unsubscribe( CallbackForTest cb )
            {
                Assert.NotNull( subscription );
                subscription.Dispose();
                subscription = null;
            }

            subscription = mgr.Subscribe<CallbackForTest>( unsubscribe );

            PostAndRunCallback( new CallbackForTest { UniqueID = Guid.NewGuid() } );
        }

        [Fact]
        public void CorrectlysubscribesFromInsideOfCallback()
        {
            static void nothing( CallbackForTest cb )
            {
                //
            }

            void subscribe( CallbackForTest cb )
            {
                using var s2 = mgr.Subscribe<CallbackForTest>( nothing );
            }

            using var s1 = mgr.Subscribe<CallbackForTest>( nothing );
            using var se = mgr.Subscribe<CallbackForTest>( subscribe );

            PostAndRunCallback( new CallbackForTest { UniqueID = Guid.NewGuid() } );
        }

        void PostAndRunCallback(CallbackMsg callback)
        {
            client.PostCallback(callback);
            mgr.RunCallbacks();
        }
    }
}
