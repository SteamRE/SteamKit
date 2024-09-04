/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

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

        public static async Task<bool> DetectLancacheServerAsync(HttpClient httpClient)
        {
            // Gets a list of ipv4 addresses, Lancache cannot use ipv6 currently
            var ipAddresses = (await Dns.GetHostAddressesAsync( TriggerDomain ) )
                .Where(e => e.AddressFamily == AddressFamily.InterNetwork)
                .ToArray();

            // If there are no private IPs, then there can't be a Lancache instance.  Lancache's IP must resolve to an RFC 1918 address
            if (!ipAddresses.Any(e => e.IsPrivateAddress()))
            {
                return false;
            }

            // DNS hostnames can possibly resolve to more than one IP address (one-to-many), so we must check each one for a Lancache server
            foreach (var ip in ipAddresses)
            {
                try
                {
                    // If the IP resolves to a private subnet, then we want to query the Lancache server to see if it is actually there.
                    // Requests that are served from the cache will have an additional header.
                    var response = await httpClient.GetAsync(new Uri($"http://{ip}/lancache-heartbeat"));
                    if (response.Headers.Contains("X-LanCache-Processed-By"))
                    {
                        Console.WriteLine($"Enabling local content cache at '{ip}' from lookup of lancache.steamcontent.com.");
                        return true;
                    }
                }
                catch (Exception e) when (e is HttpRequestException | e is TaskCanceledException)
                {
                    // Target machine refused connection errors are to be expected if there is no Lancache at that IP address.
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if an IP address is a private address, as specified in RFC1918
        /// </summary>
        /// <param name="toTest">The IP address that will be tested</param>
        /// <returns>Returns true if the IP is a private address, false if it isn't private</returns>
        private static bool IsPrivateAddress( this IPAddress toTest )
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
