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

namespace SteamKit2.CDN
{
    public partial class Client
    {
        /// <summary>
        /// When set to true, will attempt to download from a Lancache instance on the LAN rather than going out to Steam's CDNs.
        /// </summary>
        public static bool UseLancacheServer { get; private set; }

        private static string TriggerDomain = "lancache.steamcontent.com";

        /// <summary>
        /// Attempts to automatically resolve a Lancache on the local network.  If detected, SteamKit will route all downloads through the cache
        /// rather than through Steam's CDN.  Will try to detect the Lancache through the poisoned DNS entry for lancache.steamcontent.com
        ///
        /// This is a modified version from the original source : https://github.com/tpill90/lancache-prefill-common/blob/main/dotnet/LancacheIpResolver.cs
        /// </summary>
        public static async Task DetectLancacheServerAsync()
        {
            var dns = await Dns.GetHostAddressesAsync( TriggerDomain ).ConfigureAwait( false );
            var ipAddresses = dns
                .Where( e => e.AddressFamily == AddressFamily.InterNetwork || e.AddressFamily == AddressFamily.InterNetworkV6 )
                .ToArray();

            if ( ipAddresses.Any( e => IsPrivateAddress(e) ) )
            {
                UseLancacheServer = true;
                return;
            }

            //If there are no private IPs, then there can't be a Lancache instance. Lancache's IP must resolve to a private RFC 1918 address.
            UseLancacheServer = false;
        }

        /// <summary>
        /// Determines if an IP address is a private address, as specified in RFC1918
        /// </summary>
        /// <param name="toTest">The IP address that will be tested</param>
        /// <returns>Returns true if the IP is a private address, false if it isn't private</returns>
        internal static bool IsPrivateAddress( IPAddress toTest )
        {
            if ( IPAddress.IsLoopback( toTest ) )
            {
                return true;
            }

            byte[] bytes = toTest.GetAddressBytes();

            // IPv4
            if ( toTest.AddressFamily == AddressFamily.InterNetwork )
            {
                switch ( bytes[ 0 ] )
                {
                    case 10:
                        return true;
                    case 172:
                        return bytes[ 1 ] >= 16 && bytes[ 1 ] < 32;
                    case 192:
                        return bytes[ 1 ] == 168;
                    default:
                        return false;
                }
            }

            // IPv6
            if ( toTest.AddressFamily == AddressFamily.InterNetworkV6 )
            {
                // Check for Unique Local Address (fc00::/7) and loopback (::1)
                return ( bytes[ 0 ] & 0xFE ) == 0xFC || toTest.IsIPv6LinkLocal;
            }

            return false;
        }

        static HttpRequestMessage BuildLancacheRequest( Server server, string command, string? query)
        {
            var builder = new UriBuilder
            {
                Scheme = "http",
                Host = "lancache.steamcontent.com",
                Port = 80,
                Path = command,
                Query = query ?? string.Empty
            };

            var request = new HttpRequestMessage( HttpMethod.Get, builder.Uri );
            request.Headers.Host = server.Host;
            // User agent must match the Steam client in order for Lancache to correctly identify and cache Valve's CDN content
            request.Headers.Add( "User-Agent", "Valve/Steam HTTP Client 1.0" );

            return request;
        }
    }
}
