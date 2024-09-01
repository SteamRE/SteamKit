using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Ioctl;
using static Windows.Win32.PInvoke;

namespace SteamKit2.Util
{
    [SupportedOSPlatform( "windows5.1.2600" )]
    static partial class Win32Helpers
    {
        public static string? GetBootDiskSerialNumber()
        {
            try
            {
                var bootDiskNumber = GetBootDiskNumber();
                var serialNumber = GetPhysicalDriveSerialNumber( bootDiskNumber );
                return serialNumber;
            }
            catch ( Exception e )
            {
                DebugLog.WriteLine( nameof( Win32Helpers ), $"Failed to get boot disk serial number: {e}" );
                return null;
            }
        }

        static string GetBootDiskLogicalVolume()
        {
            var volume = Path.GetPathRoot( Environment.SystemDirectory.AsSpan() ).TrimEnd( Path.DirectorySeparatorChar );

            if ( volume.Length == 0 )
            {
                throw new InvalidOperationException( "Could not determine system drive letter." );
            }

            return volume.ToString();
        }

        static unsafe uint GetBootDiskNumber()
        {
            using var handle = CreateFile(
                $@"\\.\{GetBootDiskLogicalVolume()}",
                0,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            if ( handle == null || handle.IsInvalid )
            {
                throw new Win32Exception();
            }

            var extents = new VOLUME_DISK_EXTENTS();

            if ( !DeviceIoControl(
                handle,
                IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS,
                null,
                0,
                &extents,
                ( uint )VOLUME_DISK_EXTENTS.SizeOf( 1 ),
                null,
                null
            ) )
            {
                throw new Win32Exception();
            }

            if ( extents.NumberOfDiskExtents != 1 )
            {
                throw new InvalidOperationException( "Unexpected number of disk extents" );
            }

            var diskID = extents.Extents[ 0 ].DiskNumber;
            return diskID;
        }

        static unsafe string? GetPhysicalDriveSerialNumber( uint driveNumber )
        {
            using var handle = CreateFile(
                $@"\\.\PhysicalDrive{driveNumber}",
                0,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            if ( handle == null || handle.IsInvalid )
            {
                throw new Win32Exception();
            }

            var query = new STORAGE_PROPERTY_QUERY()
            {
                PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty,
                QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery,
            };

            // 1. Call DeviceIoControl(STORAGE_PROPERTY_QUERY, out STORAGE_DESCRIPTOR_HEADER) to figure out how many bytes
            // we need to allocate.

            var header = new STORAGE_DESCRIPTOR_HEADER();

            if ( !DeviceIoControl(
                handle,
                IOCTL_STORAGE_QUERY_PROPERTY,
                &query,
                ( uint )STORAGE_PROPERTY_QUERY.SizeOf( 1 ),
                &header,
                ( uint )sizeof( STORAGE_DESCRIPTOR_HEADER ),
                null,
                null
            ) )
            {
                throw new Win32Exception();
            }

            // 2. Call DeviceIOControl(STORAGE_PROPERTY_QUERY, STORAGE_DEVICE_DESCRIPTOR) to get a bunch of device info with a header
            // containing the offsets to each piece of information.

            var descriptorPtr = Marshal.AllocHGlobal( ( int )header.Size );

            try
            {
                if ( !DeviceIoControl(
                        handle,
                        IOCTL_STORAGE_QUERY_PROPERTY,
                        &query,
                        ( uint )STORAGE_PROPERTY_QUERY.SizeOf( 1 ),
                        ( void* )descriptorPtr,
                        header.Size,
                        null,
                        null
                    ) )
                {
                    throw new Win32Exception();
                }

                var descriptor = Marshal.PtrToStructure<STORAGE_DEVICE_DESCRIPTOR>( descriptorPtr );

                // 3. Figure out where in the blob the serial number is
                // and read it from there.

                var serialNumberOffset = descriptor.SerialNumberOffset;

                if ( serialNumberOffset == 0 )
                {
                    throw new InvalidOperationException( "Serial number offset is zero." );
                }

                var serialNumberPtr = IntPtr.Add( descriptorPtr, ( int )serialNumberOffset );

                var serialNumber = Marshal.PtrToStringAnsi( serialNumberPtr );
                return serialNumber;
            }
            finally
            {
                Marshal.FreeHGlobal( descriptorPtr );
            }
        }
    }
}
