using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SteamKit2.Discovery
{
    [ProtoContract]
    class BasicServerListProto
    {
        [ProtoMember(1)]
        public String ipAddress { get; set; }
        [ProtoMember(2)]
        public int port { get; set; }
    }
}
