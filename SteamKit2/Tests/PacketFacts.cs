#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Internal;
using Xunit;

#nullable enable
namespace Tests
{
    public class PacketFacts
    {
        internal record struct TestPacket( EMsg EMsg, byte[] Data );

        private static Type? GetCallback( EMsg msg ) => msg switch
        {
            EMsg.ClientPICSProductInfoResponse => typeof( SteamApps.PICSProductInfoCallback ),
            _ => null,
        };

        [Fact]
        public async Task PostsExpectedCallbacks()
        {
            var steamClient = new SteamClient();

            await foreach ( var (eMsg, data) in GetPackets( "in" ) )
            {
                var expectedCallback = GetCallback( eMsg );
                Assert.NotNull( expectedCallback );

                var packetMsg = CMClient.GetPacketMsg( data, steamClient );
                Assert.NotNull( packetMsg );
                Assert.IsType<PacketClientMsgProtobuf>( packetMsg, exactMatch: false );

                Assert.Null( steamClient.GetCallback() ); // There must be no callbacks queued

                steamClient.ReceiveTestPacketMsg( packetMsg );

                var callback = steamClient.GetCallback();
                Assert.NotNull( callback );
                Assert.Equal( expectedCallback, callback.GetType() );
            }
        }

        private static async IAsyncEnumerable<TestPacket> GetPackets( string direction )
        {
            var folder = Path.Join( AppDomain.CurrentDomain.BaseDirectory, "Packets" );
            var files = Directory.GetFiles( folder, "*.bin" );

            foreach ( var filename in files )
            {
                var packet = await GetPacket( filename, direction );

                if ( packet.HasValue )
                {
                    yield return packet.Value;
                }
            }
        }

        private static async Task<TestPacket?> GetPacket( string filename, string direction )
        {
            var parts = Path.GetFileNameWithoutExtension( filename ).Split( '_' );

            Assert.True( parts.Length > 3 );

            if ( parts[ 1 ] != direction )
            {
                return null;
            }

            var emsg = ( EMsg )uint.Parse( parts[ 2 ] );

            var data = await File.ReadAllBytesAsync( filename );

            return new( emsg, data );
        }
    }
}
#nullable disable
#endif
