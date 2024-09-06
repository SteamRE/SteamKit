/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SteamKit2.Steam.CDN
{
    /// <summary>
    /// Attempts to automatically resolve the Lancache's IP address in the same manner that the Steam client does.
    ///
    /// Will automatically try to detect the Lancache through the poisoned DNS entries.
    /// This is a modified version from the original source : https://github.com/tpill90/lancache-prefill-common/blob/main/dotnet/LancacheIpResolver.cs
    /// </summary>
    public static class LancacheDetector
    {
        private static string TriggerDomain = "lancache.steamcontent.com";

        public static bool DetectLancacheServer()
        {
            // Gets a list of ipv4 addresses, Lancache cannot use ipv6 currently
            var ipAddresses = Dns.GetHostAddresses( TriggerDomain )
                .Where(e => e.AddressFamily == AddressFamily.InterNetwork)
                .ToArray();

            // If there are no private IPs, then there can't be a Lancache instance.  Lancache's IP must resolve to an RFC 1918 address
            if (ipAddresses.Any(e => IsPrivateAddress( e ) ))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if an IP address is a private address, as specified in RFC1918
        /// </summary>
        /// <param name="toTest">The IP address that will be tested</param>
        /// <returns>Returns true if the IP is a private address, false if it isn't private</returns>
        private static bool IsPrivateAddress( IPAddress toTest )
        {
            if ( IPAddress.IsLoopback( toTest ) )
            {
                return true;
            }

            byte[] bytes = toTest.GetAddressBytes();
            switch ( bytes[ 0 ] )
            {
                case 10:
                    return true;
                case 172:
                    return bytes[ 1 ] < 32 && bytes[ 1 ] >= 16;
                case 192:
                    return bytes[ 1 ] == 168;
                default:
                    return false;
            }
        }
    }
}
