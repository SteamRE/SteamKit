using System.Collections.Generic;
using ProtoBuf;

namespace SteamKit2.Discovery
{
    /// <summary>
    /// The server list as stored by <see cref="FileStorageServerListProvider"/> and <see cref="IsolatedStorageServerListProvider"/>.
    /// </summary>
    [ProtoContract]
    public sealed class BasicServerList
    {
        /// <summary>
        /// Gets the servers in the list.
        /// </summary>
        [ProtoMember( 1 )]
        public List<BasicServerListProto> Servers { get; } = [];

        internal static BasicServerList FromServerRecords( IEnumerable<ServerRecord> endpoints )
        {
            var serverList = new BasicServerList();

            foreach ( var endpoint in endpoints )
            {
                serverList.Servers.Add( new BasicServerListProto
                {
                    Address = endpoint.GetHost(),
                    Port = endpoint.GetPort(),
                    Protocols = endpoint.ProtocolTypes
                } );
            }

            return serverList;
        }
    }
}
