using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text;
using SteamKit2.Util;
using SteamKit2.Util.MacHelpers;
using Microsoft.Win32;

using static SteamKit2.Util.MacHelpers.LibC;
using static SteamKit2.Util.MacHelpers.CoreFoundation;
using static SteamKit2.Util.MacHelpers.DiskArbitration;
using static SteamKit2.Util.MacHelpers.IOKit;
using System.Runtime.CompilerServices;

namespace SteamKit2
{
    static class MachineInfoProvider
    {
        public static IMachineInfoProvider GetProvider()
        {
            switch ( Environment.OSVersion.Platform )
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                    return new WindowsMachineInfoProvider();

                case PlatformID.Unix:
                    if ( Utils.IsMacOS() )
                    {
                        return new MacOSMachineInfoProvider();
                    }
                    else
                    {
                        return new LinuxMachineInfoProvider();
                    }
            }

            return new DefaultMachineInfoProvider();
        }
    }

    sealed class DefaultMachineInfoProvider : IMachineInfoProvider
    {
        public static DefaultMachineInfoProvider Instance { get; } = new DefaultMachineInfoProvider();

        public byte[] GetMachineGuid()
        {
            return Encoding.UTF8.GetBytes( Environment.MachineName + "-SteamKit" );
        }

        public byte[] GetMacAddress()
        {
            // mono seems to have a pretty solid implementation of NetworkInterface for our platforms
            // if it turns out to be buggy we can always roll our own and poke into /sys/class/net on nix

            try
            {
                var firstEth = NetworkInterface.GetAllNetworkInterfaces()
                    .Where( i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet || i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 )
                    .FirstOrDefault();

                if ( firstEth != null )
                {
                    return firstEth.GetPhysicalAddress().GetAddressBytes();
                }
            }
            catch ( NetworkInformationException )
            {
                // See: https://github.com/SteamRE/SteamKit/issues/629
            }
            // well...
            return Encoding.UTF8.GetBytes( "SteamKit-MacAddress" );
        }

        public byte[] GetDiskId()
        {
            return Encoding.UTF8.GetBytes( "SteamKit-DiskId" );
        }
    }

    sealed class WindowsMachineInfoProvider : IMachineInfoProvider
    {
        public byte[]? GetMachineGuid()
        {
            var localKey = RegistryKey
                .OpenBaseKey( Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64 )
                .OpenSubKey( @"SOFTWARE\Microsoft\Cryptography" );

            if ( localKey == null )
            {
                return null;
            }

            var guid = localKey.GetValue( "MachineGuid" );

            if ( guid == null )
            {
                return null;
            }

            return Encoding.UTF8.GetBytes( guid.ToString() );
        }

        public byte[]? GetMacAddress() => null;

        public byte[]? GetDiskId()
        {
            var serialNumber = Win32Helpers.GetBootDiskSerialNumber();

            if ( string.IsNullOrEmpty( serialNumber ) )
            {
                return null;
            }

            return Encoding.UTF8.GetBytes( serialNumber );
        }
    }

    sealed class LinuxMachineInfoProvider : IMachineInfoProvider
    {
        public byte[]? GetMachineGuid()
        {
            string[] machineFiles =
            {
                "/etc/machine-id", // present on at least some gentoo systems
                "/var/lib/dbus/machine-id",
                "/sys/class/net/eth0/address",
                "/sys/class/net/eth1/address",
                "/sys/class/net/eth2/address",
                "/sys/class/net/eth3/address",
                "/etc/hostname",
            };

            foreach ( var fileName in machineFiles )
            {
                try
                {
                    return File.ReadAllBytes( fileName );
                }
                catch
                {
                    // if we can't read a file, continue to the next until we hit one we can
                    continue;
                }
            }

            return null;
        }

        public byte[]? GetMacAddress() => null;

        public byte[]? GetDiskId()
        {
            string[] bootParams = GetBootOptions();

            string[] paramsToCheck =
            {
                "root=UUID=",
                "root=PARTUUID=",
            };

            foreach ( string param in paramsToCheck )
            {
                var paramValue = GetParamValue( bootParams, param );

                if ( !string.IsNullOrEmpty( paramValue ) )
                {
                    return Encoding.UTF8.GetBytes( paramValue );
                }
            }

            string[] diskUuids = GetDiskUUIDs();

            if ( diskUuids.Length > 0 )
            {
                return Encoding.UTF8.GetBytes( diskUuids.FirstOrDefault() );
            }

            return null;
        }


        string[] GetBootOptions()
        {
            string bootOptions;

            try
            {
                bootOptions = File.ReadAllText( "/proc/cmdline" );
            }
            catch
            {
                return new string[0];
            }

            return bootOptions.Split( ' ' );
        }

        string[] GetDiskUUIDs()
        {
            try
            {
                var dirInfo = new DirectoryInfo( "/dev/disk/by-uuid" );

                // we want the oldest disk symlinks first
                return dirInfo.GetFiles()
                    .OrderBy( f => f.LastWriteTime )
                    .Select( f => f.Name )
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        string? GetParamValue( string[] bootOptions, string param )
        {
            string paramString = bootOptions
                .FirstOrDefault( p => p.StartsWith( param, StringComparison.OrdinalIgnoreCase ) );

            if ( paramString == null )
                return null;

            return paramString.Substring( param.Length );
        }
    }

    sealed class MacOSMachineInfoProvider : IMachineInfoProvider
    {
        public byte[]? GetMachineGuid()
        {
            uint platformExpert = IOServiceGetMatchingService( kIOMasterPortDefault, IOServiceMatching( "IOPlatformExpertDevice" ) );
            if ( platformExpert != 0 )
            {
                try
                {
                    using ( var serialNumberKey = CFStringCreateWithCString( CFTypeRef.None, kIOPlatformSerialNumberKey, CFStringEncoding.kCFStringEncodingASCII ) )
                    {
                        var serialNumberAsString = IORegistryEntryCreateCFProperty( platformExpert, serialNumberKey, CFTypeRef.None, 0 );
                        var sb = new StringBuilder( 64 );
                        if ( CFStringGetCString( serialNumberAsString, sb, sb.Capacity, CFStringEncoding.kCFStringEncodingASCII ) )
                        {
                            return Encoding.ASCII.GetBytes( sb.ToString() );
                        }
                    }
                }
                finally
                {
                    IOObjectRelease( platformExpert );
                }
            }

            return null;
        }

        public byte[]? GetMacAddress() => null;

        public byte[]? GetDiskId()
        {
            var stat = new statfs();
            var statted = statfs64( "/", ref stat );
            if ( statted == 0 )
            {
                using ( var session = DASessionCreate( CFTypeRef.None ) )
                using ( var disk = DADiskCreateFromBSDName( CFTypeRef.None, session, stat.f_mntfromname ) )
                using ( var properties = DADiskCopyDescription( disk ) )
                using ( var key = CFStringCreateWithCString( CFTypeRef.None, DiskArbitration.kDADiskDescriptionMediaUUIDKey, CFStringEncoding.kCFStringEncodingASCII ) )
                {
                    IntPtr cfuuid = IntPtr.Zero;
                    if ( CFDictionaryGetValueIfPresent( properties, key, out cfuuid ) )
                    {
                        using ( var uuidString = CFUUIDCreateString( CFTypeRef.None, cfuuid ) )
                        {
                            var stringBuilder = new StringBuilder( 64 );
                            if ( CFStringGetCString( uuidString, stringBuilder, stringBuilder.Capacity, CFStringEncoding.kCFStringEncodingASCII ) )
                            {
                                return Encoding.ASCII.GetBytes( stringBuilder.ToString() );
                            }
                        }
                    }
                }
            }

            return null;
        }
    }

    static class HardwareUtils
    {
        class MachineID : MessageObject
        {
            public MachineID()
                : base()
            {
                this.KeyValues["BB3"] = new KeyValue();
                this.KeyValues["FF2"] = new KeyValue();
                this.KeyValues["3B3"] = new KeyValue();
            }


            public void SetBB3( string value )
            {
                this.KeyValues["BB3"].Value = value;
            }

            public void SetFF2( string value )
            {
                this.KeyValues["FF2"].Value = value;
            }

            public void Set3B3( string value )
            {
                this.KeyValues["3B3"].Value = value;
            }

            public void Set333( string value )
            {
                this.KeyValues["333"] = new KeyValue( value: value );
            }
        }

        static ConditionalWeakTable<IMachineInfoProvider, Task<MachineID>> generationTable;

        public static void Init(IMachineInfoProvider machineInfoProvider)
        {
            _ = generationTable.GetValue(machineInfoProvider, p => Task.Factory.StartNew( GenerateMachineID, state: p ));
        }

        public static byte[]? GetMachineID(IMachineInfoProvider machineInfoProvider)
        {
            if (!generationTable.TryGetValue(machineInfoProvider, out var generateTask))
            {
                DebugLog.WriteLine( nameof( HardwareUtils ), "GetMachineID() called before Init()" );
                return null;
            }

            DebugLog.Assert(generateTask != null, nameof( HardwareUtils ), "GetMachineID() found null task - should be impossible.");

            bool didComplete = generateTask.Wait( TimeSpan.FromSeconds( 30 ) );

            if ( !didComplete )
            {
                DebugLog.WriteLine( nameof( HardwareUtils ), "Unable to generate machine_id in a timely fashion, logons may fail" );
                return null;
            }

            MachineID machineId = generateTask.Result;

            using ( MemoryStream ms = new MemoryStream() )
            {
                machineId.WriteToStream( ms );

                return ms.ToArray();
            }
        }


        static MachineID GenerateMachineID(object state)
        {
            // the aug 25th 2015 CM update made well-formed machine MessageObjects required for logon
            // this was flipped off shortly after the update rolled out, likely due to linux steamclients running on distros without a way to build a machineid
            // so while a valid MO isn't currently (as of aug 25th) required, they could be in the future and we'll abide by The Valve Law now

            var provider = (IMachineInfoProvider)state;

            var machineId = new MachineID();

            // Custom implementations can fail for any particular field, in which case we fall back to DefaultMachineInfoProvider.
            machineId.SetBB3( GetHexString( provider.GetMachineGuid() ?? DefaultMachineInfoProvider.Instance.GetMachineGuid() ) );
            machineId.SetFF2( GetHexString( provider.GetMacAddress() ?? DefaultMachineInfoProvider.Instance.GetMachineGuid() ) );
            machineId.Set3B3( GetHexString( provider.GetDiskId() ?? DefaultMachineInfoProvider.Instance.GetMachineGuid() ) );

            // 333 is some sort of user supplied data and is currently unused

            return machineId;
        }

        static string GetHexString( byte[] data )
        {
            data = CryptoHelper.SHAHash( data );

            return BitConverter.ToString( data )
                .Replace( "-", "" )
                .ToLower();
        }
    }
}
