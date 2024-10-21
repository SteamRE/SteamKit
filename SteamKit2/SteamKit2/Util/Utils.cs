/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    static class Utils
    {
        /// <summary>
        /// Performs an Adler32 on the given input
        /// </summary>
        public static uint AdlerHash( ReadOnlySpan<byte> input )
        {
            uint a = 0, b = 0;
            for ( int i = 0; i < input.Length; i++ )
            {
                a = ( a + input[ i ] ) % 65521;
                b = ( b + a ) % 65521;
            }

            return a | ( b << 16 );
        }

        public static string EncodeHexString(byte[] input)
        {
            return Convert.ToHexString(input).ToLowerInvariant();
        }

        [return: NotNullIfNotNull( nameof( hex ) )]
        public static byte[]? DecodeHexString(string? hex)
        {
            if (hex == null)
                return null;

            return Convert.FromHexString( hex );
        }

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

                PlatformID.Unix when RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) => ver.Major switch
                {
                    11 => EOSType.MacOS107, // "Lion"
                    12 => EOSType.MacOS108, // "Mountain Lion"
                    13 => EOSType.MacOS109, // "Mavericks"
                    14 => EOSType.MacOS1010, // "Yosemite"
                    15 => EOSType.MacOS1011, // El Capitan
                    16 => EOSType.MacOS1012, // Sierra
                    17 => EOSType.Macos1013, // High Sierra
                    18 => EOSType.Macos1014, // Mojave
                    19 => EOSType.Macos1015, // Catalina
                    20 => EOSType.MacOS11, // Big Sur
                    21 => EOSType.MacOS12, // Monterey
                    22 => EOSType.MacOS13, // Ventura
                    23 => EOSType.MacOS14, // Sonoma
                    24 => EOSType.MacOS15, // Sequoia
                    _ => EOSType.MacOSUnknown,
                },

                _ => EOSType.Unknown,
            };
        }
    }
}
