using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ProtobufDumper
{
    static class Native
    {

        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern IntPtr LoadLibraryEx( string lpszLib, IntPtr hFile, LoadLibraryFlags dwFlags );

        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool FreeLibrary( IntPtr hModule );

        public enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008,
        }

        public const ushort IMAGE_DOS_SIGNATURE = 0x5A4D; // MZ
        public const uint IMAGE_NT_SIGNATURE = 0x00004550; // PE00

        [StructLayout( LayoutKind.Sequential )]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct IMAGE_DOS_HEADER
        {
            public UInt16 e_magic;       // Magic number
            public UInt16 e_cblp;        // Bytes on last page of file
            public UInt16 e_cp;          // Pages in file
            public UInt16 e_crlc;        // Relocations
            public UInt16 e_cparhdr;     // Size of header in paragraphs
            public UInt16 e_minalloc;    // Minimum extra paragraphs needed
            public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
            public UInt16 e_ss;          // Initial (relative) SS value
            public UInt16 e_sp;          // Initial SP value
            public UInt16 e_csum;        // Checksum
            public UInt16 e_ip;          // Initial IP value
            public UInt16 e_cs;          // Initial (relative) CS value
            public UInt16 e_lfarlc;      // File address of relocation table
            public UInt16 e_ovno;        // Overlay number
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
            public UInt16[] e_res1;        // Reserved words
            public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo;     // OEM information; e_oemid specific
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 10 )]
            public UInt16[] e_res2;        // Reserved words
            public Int32 e_lfanew;      // File address of new exe header
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct IMAGE_NT_HEADERS
        {
            public UInt32 Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            //
            // Standard fields.
            //
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            //
            // NT additional fields.
            //
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        [StructLayout( LayoutKind.Explicit )]
        public struct Misc
        {
            [FieldOffset( 0 )]
            public UInt32 PhysicalAddress;
            [FieldOffset( 0 )]
            public UInt32 VirtualSize;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct IMAGE_SECTION_HEADER
        {
            [Flags]
            public enum CharacteristicFlags : uint
            {
                Reserved1 = 0x00000000,
                Reserved2 = 0x00000001,
                Reserved3 = 0x00000002,
                Reserved4 = 0x00000004,
                IMAGE_SCN_TYPE_NO_PAD = 0x00000008,
                Reserved5 = 0x00000010,
                IMAGE_SCN_CNT_CODE = 0x00000020,
                IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040,
                IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080,
                IMAGE_SCN_LNK_OTHER = 0x00000100,
                IMAGE_SCN_LNK_INFO = 0x00000200,
                Reserved6 = 0x00000400,
                IMAGE_SCN_LNK_REMOVE = 0x00000800,
                IMAGE_SCN_LNK_COMDAT = 0x00001000,
                Reserved7 = 0x00002000,
                IMAGE_SCN_NO_DEFER_SPEC_EXC = 0x00004000,
                IMAGE_SCN_GPREL = 0x00008000,
                Reserved8 = 0x00010000,
                IMAGE_SCN_MEM_PURGEABLE = 0x00020000,
                IMAGE_SCN_MEM_LOCKED = 0x00040000,
                IMAGE_SCN_MEM_PRELOAD = 0x00080000,
                IMAGE_SCN_ALIGN_1BYTES = 0x00100000,
                IMAGE_SCN_ALIGN_2BYTES = 0x00200000,
                IMAGE_SCN_ALIGN_4BYTES = 0x00300000,
                IMAGE_SCN_ALIGN_8BYTES = 0x00400000,
                IMAGE_SCN_ALIGN_16BYTES = 0x00500000,
                IMAGE_SCN_ALIGN_32BYTES = 0x00600000,
                IMAGE_SCN_ALIGN_64BYTES = 0x00700000,
                IMAGE_SCN_ALIGN_128BYTES = 0x00800000,
                IMAGE_SCN_ALIGN_256BYTES = 0x00900000,
                IMAGE_SCN_ALIGN_512BYTES = 0x00A00000,
                IMAGE_SCN_ALIGN_1024BYTES = 0x00B00000,
                IMAGE_SCN_ALIGN_2048BYTES = 0x00C00000,
                IMAGE_SCN_ALIGN_4096BYTES = 0x00D00000,
                IMAGE_SCN_ALIGN_8192BYTES = 0x00E00000,
                IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000,
                IMAGE_SCN_MEM_DISCARDABLE = 0x02000000,
                IMAGE_SCN_MEM_NOT_CACHED = 0x04000000,
                IMAGE_SCN_MEM_NOT_PAGED = 0x08000000,
                IMAGE_SCN_MEM_SHARED = 0x10000000,
                IMAGE_SCN_MEM_EXECUTE = 0x20000000,
                IMAGE_SCN_MEM_READ = 0x40000000,
                IMAGE_SCN_MEM_WRITE = 0x80000000,


            }

            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 8 )]
            public string Name;
            public Misc Misc;
            public UInt32 VirtualAddress;
            public UInt32 SizeOfRawData;
            public UInt32 PointerToRawData;
            public UInt32 PointerToRelocations;
            public UInt32 PointerToLinenumbers;
            public UInt16 NumberOfRelocations;
            public UInt16 NumberOfLinenumbers;
            public CharacteristicFlags Characteristics;
        }

        public static T PtrToStruct<T>( IntPtr addr )
        {
            return ( T )Marshal.PtrToStructure( addr, typeof( T ) );
        }

        public static T PtrToStruct<T>( uint addr )
        {
            return PtrToStruct<T>( new IntPtr( addr ) );
        }
    }
}
