using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;

namespace SteamKit2
{
    abstract class MachineInfoProvider
    {
        public static MachineInfoProvider GetProvider()
        {
            switch ( Environment.OSVersion.Platform )
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                    return new WindowsInfoProvider();
            }

            return new DefaultInfoProvider();
        }

        public abstract byte[] GetMachineGuid();
        public abstract byte[] GetMacAddress();
        public abstract byte[] GetDiskId();
    }

    class DefaultInfoProvider : MachineInfoProvider
    {
        public override byte[] GetMachineGuid()
        {
            return Encoding.UTF8.GetBytes( "SteamKit-MachineGuid" );
        }

        public override byte[] GetMacAddress()
        {
            // mono seems to have a pretty solid implementation of NetworkInterface for our platforms
            // if it turns out to be buggy we can always roll our own and poke into /sys/class/net on nix

            var firstEth = NetworkInterface.GetAllNetworkInterfaces()
                .Where( i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet )
                .FirstOrDefault();

            if ( firstEth == null )
            {
                // well...
                return Encoding.UTF8.GetBytes( "SteamKit-MacAddress" );
            }

            return firstEth.GetPhysicalAddress().GetAddressBytes();
        }

        public override byte[] GetDiskId()
        {
            return Encoding.UTF8.GetBytes( "SteamKit-DiskId" );
        }
    }

    class WindowsInfoProvider : DefaultInfoProvider
    {
        public override byte[] GetMachineGuid()
        {
            RegistryKey localKey = RegistryKey
                .OpenBaseKey( Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64 )
                .OpenSubKey( @"SOFTWARE\Microsoft\Cryptography" );

            if ( localKey == null )
            {
                return base.GetMachineGuid();
            }

            object guid = localKey.GetValue( "MachineGuid" );

            if ( guid == null )
            {
                return base.GetMachineGuid();
            }

            return Encoding.UTF8.GetBytes( guid.ToString() );
        }

        public override byte[] GetDiskId()
        {
            var activePartition = WmiQuery(
                @"SELECT DiskIndex FROM Win32_DiskPartition
                  WHERE Bootable = 1"
                ).FirstOrDefault();

            if ( activePartition == null )
            {
                return base.GetDiskId();
            }

            uint index = (uint)activePartition["DiskIndex"];

            var bootableDisk = WmiQuery(
                @"SELECT SerialNumber FROM Win32_DiskDrive
                  WHERE Index = {0}", index
                ).FirstOrDefault();

            if ( bootableDisk == null )
            {
                return base.GetDiskId();
            }

            string serialNumber = (string)bootableDisk["SerialNumber"];

            return Encoding.UTF8.GetBytes( serialNumber );
        }

        IEnumerable<ManagementObject> WmiQuery( string queryFormat, params object[] args )
        {
            string query = string.Format( queryFormat, args );

            var searcher = new ManagementObjectSearcher( query );

            return searcher.Get().Cast<ManagementObject>();
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

        public static byte[] GenerateMachineID()
        {
            // the aug 25th 2015 CM update made well-formed machine MessageObjects required for logon
            // this was flipped off shortly after the update rolled out, likely due to linux steamclients running on distros without a way to build a machineid
            // so while a valid MO isn't currently (as of aug 25th) required, they could be in the future and we'll abide by The Valve Law now

            var machineId = new MachineID();

            using ( var ms = new MemoryStream() )
            {
                MachineInfoProvider provider = MachineInfoProvider.GetProvider();

                machineId.SetBB3( GetHexString( provider.GetMachineGuid() ) );
                machineId.SetFF2( GetHexString( provider.GetMacAddress() ) );
                machineId.Set3B3( GetHexString( provider.GetDiskId() ) );

                // 333 is currently unused

                machineId.WriteToStream( ms );

                return ms.ToArray();
            }
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
