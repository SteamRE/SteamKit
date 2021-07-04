using System;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class HardwareUtilsFacts
    {
        public class CustomMachineInfoProvider : IMachineInfoProvider
        {
            public byte[] GetMachineGuid() => new byte[ 32 ];

            public byte[] GetMacAddress() => new byte[ 32 ];

            public byte[] GetDiskId() => new byte[ 32 ];
        }

        [Fact]
        public void GetOSProviderReturnsDefaultMachineInfoProvider()
        {
            IMachineInfoProvider machineInfoProvider = HardwareUtils.GetOSProvider();
            Assert.NotNull( machineInfoProvider );
            Assert.IsAssignableFrom<DefaultInfoProvider>( machineInfoProvider );
        }

        [Fact]
        public void InitSetsCustomMachineInfoProvider()
        {
            HardwareUtils.Init( new CustomMachineInfoProvider() );
            Assert.NotNull( HardwareUtils.MachineInfoProvider );
            Assert.IsType<CustomMachineInfoProvider>( HardwareUtils.MachineInfoProvider );
            HardwareUtils.ResetMachineProvider();
        }

        [Fact]
        public void InitSetsDefaultMachineInfoProvider()
        {
            HardwareUtils.InitDefaultProvider();
            Assert.NotNull( HardwareUtils.MachineInfoProvider );
            Assert.IsAssignableFrom<DefaultInfoProvider>( HardwareUtils.MachineInfoProvider );
            HardwareUtils.ResetMachineProvider();
        }
    }
}
