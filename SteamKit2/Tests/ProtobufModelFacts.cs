using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2;
using SteamKit2.Discovery;
using SteamKit2.GC;
using SteamKit2.Internal;
using Xunit;

namespace Tests
{
    public class ProtobufModelFacts
    {
        [Fact]
        public async Task ServerListFileMatchesReflectionSerializer()
        {
            var endpoints = new List<ServerRecord>
            {
                ServerRecord.CreateSocketServer( new IPEndPoint( IPAddress.Loopback, 27017 ) ),
                ServerRecord.CreateWebSocketServer( "cmp1-fra1.steamserver.net:443" ),
                ServerRecord.CreateServer( "example.com", 27030, ProtocolTypes.Udp ),
            };

            var entries = endpoints.Select( endpoint => new BasicServerListProto
            {
                Address = endpoint.GetHost(),
                Port = endpoint.GetPort(),
                Protocols = endpoint.ProtocolTypes
            } );

            using var expected = new MemoryStream();
            RuntimeTypeModel.Create().Serialize<IEnumerable<BasicServerListProto>>( expected, entries );

            var fileName = Path.GetTempFileName();

            try
            {
                var provider = new FileStorageServerListProvider( fileName );
                await provider.UpdateServerListAsync( endpoints );

                var bytes = await File.ReadAllBytesAsync( fileName, TestContext.Current.CancellationToken );
                Assert.Equal( expected.ToArray(), bytes );

                var items = RuntimeTypeModel.Create().DeserializeItems<BasicServerListProto>( new MemoryStream( bytes ), PrefixStyle.Base128, 1 )
                    .Select( item => ServerRecord.CreateServer( item.Address, item.Port, item.Protocols ) )
                    .ToList();
                Assert.Equal( endpoints, items );

                var servers = ( await provider.FetchServerListAsync() ).ToList();
                Assert.Equal( endpoints, servers );
            }
            finally
            {
                File.Delete( fileName );
            }
        }

        [Fact]
        public async Task EmptyServerListFileReadsAsEmptyList()
        {
            var fileName = Path.GetTempFileName();

            try
            {
                var provider = new FileStorageServerListProvider( fileName );
                var servers = await provider.FetchServerListAsync();

                Assert.Empty( servers );
            }
            finally
            {
                File.Delete( fileName );
            }
        }

        [Fact]
        public void ClientLogonMatchesReflectionSerializer()
        {
            var logon = new CMsgClientLogon
            {
                protocol_version = 65580,
                client_os_type = 20,
                account_name = "Ünïcode",
                obfuscated_private_ip = new CMsgIPAddress { v4 = 0x7F000001 },
                machine_id = [ 1, 2, 3 ],
                should_remember_password = false,
            };

            using var expected = new MemoryStream();
            RuntimeTypeModel.Create().Serialize( expected, logon );

            using var actual = new MemoryStream();
            SteamKitProtobuf.Model.Serialize( actual, logon );

            Assert.Equal( expected.ToArray(), actual.ToArray() );

            var msg = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon, SteamKitProtobuf.Model )
            {
                Body = logon
            };
            msg.ProtoHeader.client_sessionid = 1234;

            var packet = new PacketClientMsgProtobuf( EMsg.ClientLogon, msg.Serialize() );
            var received = new ClientMsgProtobuf<CMsgClientLogon>( packet, SteamKitProtobuf.Model );

            Assert.Equal( 1234, received.ProtoHeader.client_sessionid );
            Assert.Equal( "Ünïcode", received.Body.account_name );
            Assert.Equal( 0x7F000001u, received.Body.obfuscated_private_ip.v4 );
            Assert.True( received.Body.ShouldSerializeshould_remember_password() );
        }

        [Theory]
        [InlineData( typeof( CMsgProtoBufHeader ) )]
        [InlineData( typeof( SteamKit2.GC.Internal.CMsgProtoBufHeader ) )]
        [InlineData( typeof( CMsgMulti ) )]
        [InlineData( typeof( CMsgGCClient ) )]
        [InlineData( typeof( ParentalSettings ) )]
        [InlineData( typeof( ContentManifestPayload ) )]
        [InlineData( typeof( ContentManifestMetadata ) )]
        [InlineData( typeof( ContentManifestSignature ) )]
        [InlineData( typeof( CContentServerDirectory_GetServersForSteamPipe_Request ) )]
        [InlineData( typeof( CContentServerDirectory_GetServersForSteamPipe_Response ) )]
        [InlineData( typeof( BasicServerList ) )]
        [InlineData( typeof( BasicServerListProto ) )]
        public void ModelContainsTypesSerializedOutsideOfMessages( Type type )
        {
            Assert.True( SteamKitProtobuf.Model.CanSerializeContractType( type ) );
        }

        [Fact]
        public void ModelOverloadThrowsForTypeMissingFromModel()
        {
            var msg = new ClientMsgProtobuf<UnknownContract>( EMsg.ClientLogon, SteamKitProtobuf.Model );
            var ex = Assert.Throws<InvalidOperationException>( () => msg.Serialize() );
            Assert.Contains( typeof( UnknownContract ).FullName, ex.Message );

            var gcMsg = new ClientGCMsgProtobuf<SteamKit2.GC.Dota.Internal.CMsgClientHello>( 4006, SteamKitProtobuf.Model );
            Assert.Throws<InvalidOperationException>( () => gcMsg.Serialize() );
        }

        [Fact]
        public void ModelOverloadThrowsForNullModel()
        {
            Assert.Throws<ArgumentNullException>( () => new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon, ( TypeModel )null ) );
        }

        [Fact]
        public void OverloadWithoutModelUsesReflection()
        {
            var msg = new ClientMsgProtobuf<UnknownContract>( EMsg.ClientLogon );
            msg.Body.Value = 42;

            var received = new ClientMsgProtobuf<UnknownContract>( new PacketClientMsgProtobuf( EMsg.ClientLogon, msg.Serialize() ) );

            Assert.Equal( 42, received.Body.Value );
        }

        [ProtoContract]
        public class UnknownContract : IExtensible
        {
            IExtension extensionData;

            IExtension IExtensible.GetExtensionObject( bool createIfMissing ) => Extensible.GetExtensionObject( ref extensionData, createIfMissing );

            [ProtoMember( 1 )]
            public int Value { get; set; }
        }
    }
}
