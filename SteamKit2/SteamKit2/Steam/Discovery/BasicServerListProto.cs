﻿using ProtoBuf;

namespace SteamKit2.Discovery
{
    [ProtoContract]
    class BasicServerListProto
    {
        [ProtoMember(1)]
        public string Address { get; set; }

        [ProtoMember(2)]
        public int Port { get; set; }

        [ProtoMember(3)]
        public ProtocolTypes Protocols
        {
            get => protocolTypes ?? ProtocolTypes.Tcp;
            set => protocolTypes = value;
        }

        ProtocolTypes? protocolTypes;
    }
}
