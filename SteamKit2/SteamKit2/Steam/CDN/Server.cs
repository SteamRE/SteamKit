/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Net;

namespace SteamKit2.CDN
{
    /// <summary>
    /// Represents a single Steam3 'Steampipe' content server.
    /// </summary>
    public sealed class Server
    {
        /// <summary>
        /// The protocol used to connect to this server
        /// </summary>
        public enum ConnectionProtocol
        {
            /// <summary>
            /// Server does not advertise HTTPS support, connect over HTTP
            /// </summary>
            HTTP = 0,
            /// <summary>
            /// Server advertises it supports HTTPS, connection made over HTTPS
            /// </summary>
            HTTPS = 1
        }

        /// <summary>
        /// Gets the supported connection protocol of the server.
        /// </summary>
        public ConnectionProtocol Protocol { get; internal set; }
        /// <summary>
        /// Gets the hostname of the server.
        /// </summary>
        public string? Host { get; internal set; }
        /// <summary>
        /// Gets the virtual hostname of the server.
        /// </summary>
        public string? VHost { get; internal set; }
        /// <summary>
        /// Gets the port of the server.
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Gets the type of the server.
        /// </summary>
        public string? Type { get; internal set; }

        /// <summary>
        /// Gets the SourceID this server belongs to.
        /// </summary>
        public int SourceID { get; internal set; }

        /// <summary>
        /// Gets the CellID this server belongs to.
        /// </summary>
        public uint CellID { get; internal set; }

        /// <summary>
        /// Gets the load value associated with this server.
        /// </summary>
        public int Load { get; internal set; }
        /// <summary>
        /// Gets the weighted load.
        /// </summary>
        public float WeightedLoad { get; internal set; }
        /// <summary>
        /// Gets the number of entries this server is worth.
        /// </summary>
        public int NumEntries { get; internal set; }
        /// <summary>
        /// Gets the preferred server status.
        /// </summary>
        [Obsolete("This flag is no longer set.")]
        public bool PreferredServer { get; internal set; }
        /// <summary>
        /// Gets the flag whether this server is for Steam China only.
        /// </summary>
        public bool SteamChinaOnly { get; internal set; }
        /// <summary>
        /// Gets the download proxy status.
        /// </summary>
        public bool UseAsProxy { get; internal set; }
        /// <summary>
        /// Gets the transformation template applied to request paths.
        /// </summary>
        public string? ProxyRequestPathTemplate { get; internal set; }

        /// <summary>
        /// Gets the list of app ids this server can be used with.
        /// </summary>
        public uint[] AllowedAppIds { get; internal set; } = [];

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Net.IPEndPoint"/> to <see cref="Server"/>.
        /// </summary>
        /// <param name="endPoint">A IPEndPoint to convert into a <see cref="Server"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Server( IPEndPoint endPoint )
        {
            return new Server
            {
                Protocol = endPoint.Port == 443 ? ConnectionProtocol.HTTPS : ConnectionProtocol.HTTP,
                Host = endPoint.Address.ToString(),
                VHost = endPoint.Address.ToString(),
                Port = endPoint.Port,
            };
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Net.DnsEndPoint"/> to <see cref="Server"/>.
        /// </summary>
        /// <param name="endPoint">A DnsEndPoint to convert into a <see cref="Server"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Server( DnsEndPoint endPoint )
        {
            return new Server
            {
                Protocol = endPoint.Port == 443 ? ConnectionProtocol.HTTPS : ConnectionProtocol.HTTP,
                Host = endPoint.Host,
                VHost = endPoint.Host,
                Port = endPoint.Port,
            };
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this server.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this server.
        /// </returns>
        public override string ToString()
        {
            return $"{Host}:{Port} ({Type})";
        }
    }
}
