using ProtoBuf;

namespace SteamKitten.Discovery
{
    [ProtoContract]
    class BasicServerListProto
    {
        [ProtoMember( 1 )]
        public string Address { get; set; } = string.Empty;

        [ProtoMember(2)]
        public int Port { get; set; }

        [ProtoMember(3)]
        public ProtocolTypes Protocols
        {
            get => protocolTypes ?? (ProtocolTypes.Tcp | ProtocolTypes.Udp);
            set => protocolTypes = value;
        }

        ProtocolTypes? protocolTypes;
    }
}
