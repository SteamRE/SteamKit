using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamKit2.Util
{
	static class Win32Helpers
	{
		#region Boot Disk Serial Number

		public static string? GetBootDiskSerialNumber()
		{
			try
			{
				var bootDiskNumber = GetBootDiskNumber();
				var serialNumber = GetPhysicalDriveSerialNumber( bootDiskNumber );
				return serialNumber;
			}
			catch
			{
				return null;
			}
		}

		static string GetBootDiskLogicalVolume()
		{
			return Environment.GetEnvironmentVariable( "SystemDrive" );
		}

		static uint GetBootDiskNumber()
		{
			var volumeName = $@"\\.\{ GetBootDiskLogicalVolume() }";
			using ( var handle = NativeMethods.CreateFile( volumeName, 0, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, IntPtr.Zero, NativeMethods.OPEN_EXISTING, 0, IntPtr.Zero ) )
			{
				if ( handle == null || handle.IsInvalid )
				{
					throw new FileNotFoundException( "Unable to open boot volume.", volumeName );
				}

				var bufferSize = 0x20;
				var pointer = Marshal.AllocHGlobal( bufferSize );
				try
				{
					uint bytesReturned;

					if ( !NativeMethods.DeviceIoControl( handle, NativeMethods.IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, pointer, (uint)bufferSize, out bytesReturned, IntPtr.Zero ) )
					{
						throw new Win32Exception();
					}

					var extents = Marshal.PtrToStructure<NativeMethods.VOLUME_DISK_EXTENTS>(pointer);
					if ( extents.NumberOfDiskExtents != 1 )
					{
						throw new InvalidOperationException( "Unexpected number of disk extents" );
					}

					var diskID = extents.Extents[0].DiskNumber;
					return diskID;
				}
				finally
				{
					Marshal.FreeHGlobal( pointer );
				}
			}
		}

		//
		// Here Be Dragons
		//
		static string? GetPhysicalDriveSerialNumber( uint driveNumber )
		{
			using (var handle = NativeMethods.CreateFile( $@"\\.\PhysicalDrive{driveNumber}", 0, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, IntPtr.Zero, NativeMethods.OPEN_EXISTING, 0, IntPtr.Zero ) )
			{
				if ( handle == null || handle.IsInvalid )
				{
					return null;
				}

				uint descriptorSize;

				// 1. Call DeviceIoControl(STORAGE_PROPERTY_QUERY, out STORAGE_DESCRIPTOR_HEADER) to figure out how many bytes
				// we need to allocate.

				var querySize = Marshal.SizeOf<NativeMethods.STORAGE_PROPERTY_QUERY>();
				var queryPtr = Marshal.AllocHGlobal( querySize );
				try
				{
					var query = new NativeMethods.STORAGE_PROPERTY_QUERY();
					query.PropertyId = NativeMethods.StorageDeviceProperty;
					query.QueryType = NativeMethods.PropertyStandardQuery;
					Marshal.StructureToPtr( query, queryPtr, fDeleteOld: false );

					uint bytesReturned;

					var headerSize = Marshal.SizeOf<NativeMethods.STORAGE_DESCRIPTOR_HEADER>();
					var headerPtr = Marshal.AllocHGlobal( headerSize );
					try
					{
						if ( !NativeMethods.DeviceIoControl( handle, NativeMethods.IOCTL_STORAGE_QUERY_PROPERTY, queryPtr, ( uint )querySize, headerPtr, ( uint )headerSize, out bytesReturned, IntPtr.Zero ) )
						{
							throw new Win32Exception();
						}

						var header = Marshal.PtrToStructure<NativeMethods.STORAGE_DESCRIPTOR_HEADER>( headerPtr );
						descriptorSize = header.Size;
					}
					finally
					{
						Marshal.FreeHGlobal( headerPtr);
					}

					// 2. Call DeviceIOControl(STORAGE_PROPERTY_QUERY, STORAGE_DEVICE_DESCRIPTOR) to get a bunch of device info with a header
					// containing the offsets to each piece of information.

					var descriptorPtr = Marshal.AllocHGlobal( ( int ) descriptorSize );
					try
					{
						if ( !NativeMethods.DeviceIoControl( handle, NativeMethods.IOCTL_STORAGE_QUERY_PROPERTY, queryPtr, ( uint )querySize, descriptorPtr, descriptorSize, out bytesReturned, IntPtr.Zero ) )
						{
							throw new Win32Exception();
						}

						var descriptor = Marshal.PtrToStructure<NativeMethods.STORAGE_DEVICE_DESCRIPTOR>( descriptorPtr);

						// 3. Figure out where in the blob the serial number is
						// and read it from there.
						var serialNumberOffset = descriptor.SerialNumberOffset;
						var serialNumberPtr = IntPtr.Add( descriptorPtr, ( int )serialNumberOffset );

						var serialNumber = Marshal.PtrToStringAnsi( serialNumberPtr );
						return serialNumber;
					}
					finally
					{
						Marshal.FreeHGlobal( descriptorPtr);
					}
				}
				finally
				{
					Marshal.FreeHGlobal( queryPtr );
				}
			}
		}

		#endregion

		sealed class FileSafeHandle : SafeHandle
		{
			FileSafeHandle()
				: base( IntPtr.Zero, ownsHandle: true )
			{
			}

			public override bool IsInvalid => handle == IntPtr.Zero;

			protected override bool ReleaseHandle()
			{
				return NativeMethods.CloseHandle( handle );
			}
		}

		static class NativeMethods
		{
			#region CreateFile

			public static uint FILE_SHARE_READ = 1;
			public static uint FILE_SHARE_WRITE = 2;
			public static uint OPEN_EXISTING = 3;

			[DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
			public static extern FileSafeHandle CreateFile( string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile );

			#endregion

			#region CloseHandle

			[DllImport( "kernel32.dll", SetLastError = true )]
			[return: MarshalAs( UnmanagedType.Bool )]
			public static extern bool CloseHandle( IntPtr hObject );

			#endregion

			#region DeviceIoControl

#pragma warning disable 0649 // Field <field> is never assigned to, and will always have its default value <value>.

			public struct DISK_EXTENT
			{
				public uint DiskNumber;
				public ulong StartingOffset;
				public ulong ExtentLength;
			};

			public struct VOLUME_DISK_EXTENTS
			{
				public uint NumberOfDiskExtents;

				[MarshalAs( UnmanagedType.ByValArray )]
				public DISK_EXTENT[] Extents;
			}

			public struct STORAGE_PROPERTY_QUERY
			{
				public int PropertyId;
				public int QueryType;
				[MarshalAs( UnmanagedType.ByValArray, SizeConst = 1 )]
				public byte[] AdditionalParameters;
			}

			public struct STORAGE_DESCRIPTOR_HEADER
			{
				public uint Version;
				public uint Size;
			}

			public struct STORAGE_DEVICE_DESCRIPTOR
			{
				public uint Version;
				public uint Size;
				public byte DeviceType;
				public byte DeviceTypeModifier;
				[MarshalAs(UnmanagedType.I1)]
				public bool RemovableMedia;
				[MarshalAs(UnmanagedType.I1)]
				public bool CommandQueueing;
				public uint VendorIdOffset;
				public uint ProductIdOffset;
				public uint ProductRevisionOffset;
				public uint SerialNumberOffset;
				public int StorageBusType;
				public uint RawPropertiesLength;
				[MarshalAs( UnmanagedType.ByValArray, SizeConst = 1 )]
				public byte[] RawDeviceProperties;
			}

#pragma warning restore 0649

			public static uint IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x00560000;
			public static uint IOCTL_STORAGE_QUERY_PROPERTY = 0x002D1400;
			public static int StorageDeviceProperty = 0;
			public static int PropertyStandardQuery = 0;

			[DllImport( "kernel32.dll", SetLastError = true )]
			[return: MarshalAs( UnmanagedType.Bool )]
			public static extern bool DeviceIoControl( SafeHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped );

			#endregion
		}
	}
}
