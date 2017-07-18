using System.Collections.Generic;
using System.Net;
using SteamKit2;
using SteamKit2.Discovery;
using Xunit;

namespace Tests
{
    public class SmartCMServerListFacts
    {
        public SmartCMServerListFacts()
        {
            var configuration = new SteamConfiguration { AllowDirectoryFetch = false };
            serverList = new SmartCMServerList(configuration);
        }

        readonly SmartCMServerList serverList;
        
        [Fact]
        public void TryMergeWithList_AddsToHead_AndMovesExisting()
        {
            serverList.GetAllEndPoints();

            var seedList = new[]
            {
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27025 )),
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27035 )),
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27045 )),
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27105 )),
            };
            serverList.ReplaceList( seedList );
            Assert.Equal( 4, seedList.Length );

            var listToReplace = new[]
            {
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27015 )),
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27035 )),
                ServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27105 )),
            };

            serverList.ReplaceList( listToReplace );

            var addresses = serverList.GetAllEndPoints();
            Assert.Equal( 3, addresses.Length );
            Assert.Equal( listToReplace[ 0 ], addresses[ 0 ] );
            Assert.Equal( listToReplace[ 1 ], addresses[ 1 ] );
            Assert.Equal( listToReplace[ 2 ], addresses[ 2 ] );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsNull_IfListIsEmpty()
        {
            var endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Null( endPoint );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers()
        {
            serverList.GetAllEndPoints();

            var record = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( record, nextRecord );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers_EvenIfAllServersAreBad()
        {
            serverList.GetAllEndPoints();

            var record = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );
            serverList.TryMark( record.EndPoint, ServerQuality.Bad );

            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( record, nextRecord );
        }

        [Fact]
        public void GetNextServerCandidate_IsBiasedTowardsServerOrdering()
        {
            serverList.GetAllEndPoints();

            var goodRecord = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            var neutralRecord = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27016 ) );
            var badRecord = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27017 ) );

            serverList.ReplaceList( new List<ServerRecord>() { badRecord, neutralRecord, goodRecord } );

            serverList.TryMark( badRecord.EndPoint, ServerQuality.Bad );
            serverList.TryMark( goodRecord.EndPoint, ServerQuality.Good );

            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( neutralRecord, nextRecord );

            serverList.TryMark( badRecord.EndPoint, ServerQuality.Good);

            nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( badRecord, nextRecord );
        }

        [Fact]
        public void GetNextServerCandidate_OnlyReturnsMatchingServerOfType()
        {
            var record = ServerRecord.WebSocketServer( "localhost:443" );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Null( endPoint );
            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Udp );
            Assert.Null( endPoint );
            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp | ProtocolTypes.Udp);
            Assert.Null( endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.WebSocket );
            Assert.Same( record, endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.All );
            Assert.Same(record, endPoint);

            record = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.WebSocket );
            Assert.Null( endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Same( record, endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Udp);
            Assert.Same( record, endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp | ProtocolTypes.Udp );
            Assert.Same( record, endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.All );
            Assert.Same(record, endPoint);
        }

        [Fact]
        public void TryMark_ReturnsTrue_IfServerInList()
        {
            var record = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ));
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var marked = serverList.TryMark( record.EndPoint, ServerQuality.Good );
            Assert.True( marked );
        }

        [Fact]
        public void TryMark_ReturnsFalse_IfServerNotInList()
        {
            var record = ServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var marked = serverList.TryMark( new IPEndPoint( IPAddress.Loopback, 27016 ), ServerQuality.Good );
            Assert.False( marked );
        }
    }
}
