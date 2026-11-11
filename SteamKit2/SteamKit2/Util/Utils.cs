/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    static class Utils
    {
        public static EOSType GetOSType()
        {
            var osVer = Environment.OSVersion;
            var ver = osVer.Version;

            return osVer.Platform switch
            {
                PlatformID.Win32Windows => ver.Minor switch
                {
                    0 => EOSType.Win95,
                    10 => EOSType.Win98,
                    90 => EOSType.WinME,
                    _ => EOSType.WinUnknown,
                },

                PlatformID.Win32NT => ver.Major switch
                {
                    4 => EOSType.WinNT,
                    5 => ver.Minor switch
                    {
                        0 => EOSType.Win2000,
                        1 => EOSType.WinXP,
                        // Assume nobody runs Windows XP Professional x64 Edition
                        // It's an edition of Windows Server 2003 anyway.
                        2 => EOSType.Win2003,
                        _ => EOSType.WinUnknown,
                    },
                    6 => ver.Minor switch
                    {
                        0 => EOSType.WinVista, // Also Server 2008
                        1 => EOSType.Windows7, // Also Server 2008 R2
                        2 => EOSType.Windows8, // Also Server 2012
                        // Note: The OSVersion property reports the same version number (6.2.0.0) for both Windows 8 and Windows 8.1.- http://msdn.microsoft.com/en-us/library/system.environment.osversion(v=vs.110).aspx
                        // In practice, this will only get hit if the application targets Windows 8.1 in the app manifest.
                        // See http://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx for more info.
                        3 => EOSType.Windows81, // Also Server 2012 R2
                        _ => EOSType.WinUnknown,
                    },
                    10 when ver.Build >= 22000 => EOSType.Win11,
                    10 => EOSType.Windows10,// Also Server 2016, Server 2019, Server 2022
                    _ => EOSType.WinUnknown,
                },

                // The specific minor versions only exist in Valve's enum for LTS versions
                PlatformID.Unix when RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) => ver.Major switch
                {
                    2 => ver.Minor switch
                    {
                        2 => EOSType.Linux22,
                        4 => EOSType.Linux24,
                        6 => EOSType.Linux26,
                        _ => EOSType.LinuxUnknown,
                    },
                    3 => ver.Minor switch
                    {
                        2 => EOSType.Linux32,
                        5 => EOSType.Linux35,
                        6 => EOSType.Linux36,
                        10 => EOSType.Linux310,
                        16 => EOSType.Linux316,
                        18 => EOSType.Linux318,
                        _ => EOSType.Linux3x,
                    },
                    4 => ver.Minor switch
                    {
                        1 => EOSType.Linux41,
                        4 => EOSType.Linux44,
                        9 => EOSType.Linux49,
                        14 => EOSType.Linux414,
                        19 => EOSType.Linux419,
                        _ => EOSType.Linux4x,
                    },
                    5 => ver.Minor switch
                    {
                        4 => EOSType.Linux54,
                        10 => EOSType.Linux510,
                        _ => EOSType.Linux5x,
                    },
                    6 => EOSType.Linux6x,
                    7 => EOSType.Linux7x,
                    _ => EOSType.LinuxUnknown,
                },

                // OSVersion on macOS is the product version from NSProcessInfo, not the Darwin kernel version
                PlatformID.Unix when RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) => ver.Major switch
                {
                    // 10.16 and later are what macOS reports to apps linked against an older SDK
                    10 => ver.Minor switch
                    {
                        11 => EOSType.MacOS1011, // El Capitan
                        12 => EOSType.MacOS1012, // Sierra
                        13 => EOSType.Macos1013, // High Sierra
                        14 => EOSType.Macos1014, // Mojave
                        16 => EOSType.MacOS1016,
                        17 => EOSType.MacOS1017,
                        18 => EOSType.MacOS1018,
                        19 => EOSType.MacOS1019,
                        20 => EOSType.MacOS1020,
                        21 => EOSType.MacOS1021,
                        22 => EOSType.MacOS1022,
                        23 => EOSType.MacOS1023,
                        _ => EOSType.Macos1015, // Catalina
                    },
                    11 when ver.Minor == 0 => EOSType.MacOS11, // Big Sur
                    11 => EOSType.MacOS111,
                    12 => EOSType.MacOS12, // Monterey
                    13 => EOSType.MacOS13, // Ventura
                    14 => EOSType.MacOS14, // Sonoma
                    15 => EOSType.MacOS15, // Sequoia
                    26 => EOSType.MacOS26, // Tahoe
                    27 => EOSType.MacOS27,
                    28 => EOSType.MacOS28,
                    _ => EOSType.MacOSUnknown,
                },

                _ => EOSType.Unknown,
            };
        }
    }
}
