using ProtoBuf;
using System;

namespace SteamKit2.Discovery
{
    [ProtoContract]
    class BasicServerListProto
    {
        [ProtoMember(1)]
        public string address { get; set; }
        [ProtoMember(2)]
        public int port { get; set; }
        [ProtoMember(3)]
        public bool websocket { get; set; }
    }
}
