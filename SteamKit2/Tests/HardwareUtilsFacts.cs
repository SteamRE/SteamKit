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

        [ Fact ]
        public void GetOSProviderReturnsDefaultForOSMachineInfoProvider()
        {
            IMachineInfoProvider machineInfoProvider = HardwareUtils.GetOSProvider();
            Assert.NotNull( machineInfoProvider );
            switch ( Environment.OSVersion.Platform )
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                    Assert.IsType<WindowsInfoProvider>( machineInfoProvider );
                    break;
                case PlatformID.Unix:
                    if ( Utils.IsMacOS() )
                    {
                        Assert.IsType<OSXInfoProvider>( machineInfoProvider );
                    }
                    else
                    {
                        Assert.IsType<LinuxInfoProvider>( machineInfoProvider );
                    }

                    break;
                default:
                    Assert.IsType<DefaultInfoProvider>( machineInfoProvider );
                    break;
            }
        }

        [ Fact ]
        public void InitSetsCustomMachineInfoProvider()
        {
            HardwareUtils.Init( new CustomMachineInfoProvider() );
            Assert.NotNull( HardwareUtils.MachineInfoProvider );
            Assert.IsType<CustomMachineInfoProvider>( HardwareUtils.MachineInfoProvider );
        }

        [ Fact ]
        public void InitSetsDefaultMachineInfoProvider()
        {
            IMachineInfoProvider machineInfoProvider = HardwareUtils.GetOSProvider();
            HardwareUtils.Init( machineInfoProvider );
            Assert.NotNull( HardwareUtils.MachineInfoProvider );
            Assert.IsAssignableFrom<DefaultInfoProvider>( HardwareUtils.MachineInfoProvider );
        }
        
        
    }
}
