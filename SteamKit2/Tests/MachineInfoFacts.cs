using System;
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
    }
}