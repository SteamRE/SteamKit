using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamKit2.Util.MacHelpers
{
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments. All the APIs in this file deal with regular UTF-8 strings (char *). With CharSet.Unicode, SK2 just crashes.
    class CFTypeRef : SafeHandle
    {
        CFTypeRef()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return false;
            }

            CoreFoundation.CFRelease(handle);
            return true;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        public static CFTypeRef None
        {
            get { return new CFTypeRef(); }
        }
    }

    // Taken from <sys/mount.h>
    struct statfs
    {
        const int MFSTYPENAMELEN = 16;
        const int PATH_MAX = 1024;

        public uint    f_bsize;    /* fundamental file system block size */
        public int     f_iosize;   /* optimal transfer block size */
        public ulong   f_blocks;   /* total data blocks in file system */
        public ulong   f_bfree;    /* free blocks in fs */
        public ulong   f_bavail;   /* free blocks avail to non-superuser */
        public ulong   f_files;    /* total file nodes in file system */
        public ulong   f_ffree;    /* free file nodes in fs */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[]  f_fsid;     /* file system id */
        public uint    f_owner;    /* user that mounted the filesystem */
        public uint    f_type;     /* type of filesystem */
        public uint    f_flags;    /* copy of mount exported flags */
        public uint    f_fssubtype;    /* fs sub-type (flavor) */

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MFSTYPENAMELEN)]
        public string  f_fstypename;  /* fs type name */

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PATH_MAX)]
        public string  f_mntonname;    /* directory on which mounted */

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PATH_MAX)]
        public string  f_mntfromname ; /* mounted filesystem */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[]  f_reserved;  /* For future use */
    }

    static class LibC
    {
        const string LibraryName = "libc";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int statfs64(string path, ref statfs buf);
    }

    static class CoreFoundation
    {
        const string LibraryName = "CoreFoundation.framework/CoreFoundation";

        public enum CFStringEncoding : uint
        {
            kCFStringEncodingASCII = 0x0600
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CFRelease(IntPtr cf);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool CFDictionaryGetValueIfPresent(CFTypeRef theDict, CFTypeRef key, out IntPtr value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CFTypeRef CFStringCreateWithCString(CFTypeRef allocator, string cStr, CFStringEncoding encoding);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.U1)]
        public static extern bool CFStringGetCString(CFTypeRef theString, StringBuilder buffer, long bufferSize, CFStringEncoding encoding);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CFTypeRef CFUUIDCreateString(CFTypeRef allocator, IntPtr uuid);
    }

    static class DiskArbitration
    {
        const string LibraryName = "DiskArbitration.framework/DiskArbitration";
        public const string kDADiskDescriptionMediaUUIDKey = "DAMediaUUID";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CFTypeRef DASessionCreate(CFTypeRef allocator);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CFTypeRef DADiskCreateFromBSDName(CFTypeRef allocator, CFTypeRef session, string name);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CFTypeRef DADiskCopyDescription(CFTypeRef disk);
    }

    static class IOKit
    {
        const string LibraryName = "IOKit.framework/IOKit";

        public const uint kIOMasterPortDefault = 0;
        public const string kIOPlatformSerialNumberKey = "IOPlatformSerialNumber";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CFTypeRef IORegistryEntryCreateCFProperty(uint entry, CFTypeRef key, CFTypeRef allocator, uint options);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IOServiceMatching(string name);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int IOObjectRelease(uint @object);
    }
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
}

