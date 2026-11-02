using ProtoBuf;
using SteamKit2.Discovery;

namespace SteamKit2.Internal
{
    /// <summary>
    /// A server list entry as stored by <see cref="FileStorageServerListProvider"/> and <see cref="IsolatedStorageServerListProvider"/>.
    /// </summary>
    [ProtoContract]
    public sealed class BasicServerListProto
    {
        /// <summary>
        /// Gets or sets the host name or IP address of the server.
        /// </summary>
        [ProtoMember( 1 )]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port of the server.
        /// </summary>
        [ProtoMember(2)]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the protocols supported by the server.
        /// </summary>
        [ProtoMember(3)]
        public ProtocolTypes Protocols
        {
            get => protocolTypes ?? (ProtocolTypes.Tcp | ProtocolTypes.Udp);
            set => protocolTypes = value;
        }

        ProtocolTypes? protocolTypes;
    }
}
