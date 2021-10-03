using System;
using System.Threading;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class MachineInfoFacts
    {
        [Fact]
        public void ResultIsCached()
        {
            var provider = new CountingMachineInfoProvider();

            HardwareUtils.Init(provider);
            HardwareUtils.GetMachineID(provider);

            var invocations = provider.TotalInvocations;

            for (var i = 0; i < 100; i++)
            {
                HardwareUtils.Init(provider);
                HardwareUtils.GetMachineID(provider);
            }

            Assert.Equal(invocations, provider.TotalInvocations);
        }

        [Fact]
        public void ResultIsCachedByInstance()
        {
            var provider = new CountingMachineInfoProvider();
            HardwareUtils.Init(provider);
            HardwareUtils.GetMachineID(provider);

            var invocations = provider.TotalInvocations;

            for (var i = 0; i < 100; i++)
            {
                var newProvider = new CountingMachineInfoProvider();
                Assert.Equal(0, newProvider.TotalInvocations);

                HardwareUtils.Init(newProvider);
                HardwareUtils.GetMachineID(newProvider);

                Assert.Equal(invocations, newProvider.TotalInvocations);
                Assert.Equal(invocations, provider.TotalInvocations);
            }

            Assert.Equal(invocations, provider.TotalInvocations);
        }

        [Fact]
        public void MachineInfoIsProcessedInBackground()
        {
            var provider = new ThreadRejectingMachineInfoProvider(Thread.CurrentThread.ManagedThreadId);
            HardwareUtils.Init(provider);

            // Should not throw
            HardwareUtils.GetMachineID(provider);
        }

        [Fact]
        public void ProviderIsNotRetained()
        {
            WeakReference Setup()
            {
                var provider = new CountingMachineInfoProvider();
                HardwareUtils.Init(provider);
                HardwareUtils.GetMachineID(provider);
                return new WeakReference(provider);
            }

            var provider = Setup();
            GC.Collect();
            Assert.False(provider.IsAlive);
        }

        sealed class CountingMachineInfoProvider : IMachineInfoProvider
        {
            public int TotalInvocations { get; private set; }

            public byte[] GetDiskId()
            {
                TotalInvocations++;
                return Array.Empty<byte>();
            }

            public byte[] GetMacAddress()
            {
                TotalInvocations++;
                return Array.Empty<byte>();
            }
            public byte[] GetMachineGuid()
            {
                TotalInvocations++;
                return Array.Empty<byte>();
            }
        }

        sealed class ThreadRejectingMachineInfoProvider : IMachineInfoProvider
        {
            public ThreadRejectingMachineInfoProvider(int threadId)
            {
                ThreadIdToReject = threadId;
            }

            public int ThreadIdToReject { get; }

            public byte[] GetDiskId()
            {
                EnsureNotOnRejectedThread();
                return Array.Empty<byte>();
            }

            public byte[] GetMacAddress()
            {
                EnsureNotOnRejectedThread();
                return Array.Empty<byte>();
            }

            public byte[] GetMachineGuid()
            {
                EnsureNotOnRejectedThread();
                return Array.Empty<byte>();
            }

            void EnsureNotOnRejectedThread()
            {
                if (Thread.CurrentThread.ManagedThreadId == ThreadIdToReject)
                {
                    throw new InvalidOperationException("Operation must not be run on rejected thread.");
                }
            }
        }
    }
}