 using System;
using System.Net;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// Represents the information needed to connect to a CM server
    /// </summary>
    public class CMServerRecord
    {
        CMServerRecord(EndPoint endPoint, ProtocolTypes protocolTypes)
        {
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            ProtocolTypes = protocolTypes;
        }

        /// <summary>
        /// The endpoint of the server to connect to.
        /// </summary>
        public EndPoint EndPoint { get; }

        /// <summary>
        /// The various protocol types that can be used to communicate with this server.
        /// </summary>
        public ProtocolTypes ProtocolTypes { get; }

        /// <summary>
        /// Gets the IP address of the associated endpoint, if this is a socket serve.r
        /// </summary>
        /// <returns>The <see cref="IPAddress"/> of the associated endpoint.</returns>
        public IPAddress GetIPAddress()
        {
            if (EndPoint is IPEndPoint ipep)
            {
                return ipep.Address;
            }

            throw new InvalidOperationException("IP Address is not supported on this type of server record");
        }

        /// <summary>
        /// Gets the hostname of the associated endpoint, if this is a websocket server.
        /// </summary>
        /// <returns>The hostname of the associated endpoint.</returns>
        public string GetHostname()
        {
            if (EndPoint is DnsEndPoint dns)
            {
                return dns.Host;
            }

            throw new InvalidOperationException("Hostname is not supported on this type of server record");
        }

        /// <summary>
        /// Gets the port number of the associated endpoint.
        /// </summary>
        /// <returns>The port numer of the associated endpoint.</returns>
        public int GetPort()
        {
            switch (EndPoint)
            {
                case IPEndPoint ipep:
                    return ipep.Port;

                case DnsEndPoint dns:
                    return dns.Port;

                default:
                    throw new InvalidOperationException("Unreachable code");
            }
        }

        /// <summary>
        /// Creates a Socket server given an IP endpoint.
        /// </summary>
        /// <param name="endPoint">The IP address and port of the server.</param>
        /// <returns>A new <see cref="CMServerRecord"/> instance</returns>
        public static CMServerRecord SocketServer(IPEndPoint endPoint)
            => new CMServerRecord(endPoint, ProtocolTypes.Tcp | ProtocolTypes.Udp);

        /// <summary>
        /// Creates a WebSocket server given an address in the form of "hostname:port".
        /// </summary>
        /// <param name="address">The name and port of the server</param>
        /// <returns>A new <see cref="CMServerRecord"/> instance</returns>
        public static CMServerRecord WebSocketServer(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            EndPoint endPoint;
            const int DefaultPort = 443;

            var indexOfColon = address.IndexOf(':');
            if (indexOfColon >= 0)
            {
                var hostname = address.Substring(0, indexOfColon);
                var portNumber = address.Substring(indexOfColon + 1);

                if (!int.TryParse(portNumber, out var port))
                {
                    throw new ArgumentException("Port number must be a valid integer value.", nameof(address));
                }

                endPoint = new DnsEndPoint(hostname, port);
            }
            else
            {
                endPoint = new DnsEndPoint(address, DefaultPort);
            }

            return new CMServerRecord(endPoint, ProtocolTypes.WebSocket);
        }
    }
}
