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
            serverList = new SmartCMServerList(new NullServerListProvider(), allowDirectoryFetch: false);
        }

        readonly SmartCMServerList serverList;
        
        [Fact]
        public void TryMergeWithList_AddsToHead_AndMovesExisting()
        {
            serverList.GetAllEndPoints();

            var seedList = new[]
            {
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27025 )),
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27035 )),
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27045 )),
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27105 )),
            };
            serverList.ReplaceList( seedList );
            Assert.Equal( 4, seedList.Length );

            var listToReplace = new[]
            {
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27015 )),
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27035 )),
                CMServerRecord.SocketServer(new IPEndPoint( IPAddress.Loopback, 27105 )),
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
            var endPoint = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Null( endPoint );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers()
        {
            serverList.GetAllEndPoints();

            var record = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<CMServerRecord>() { record } );

            var nextRecord = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Equal( record, nextRecord );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers_EvenIfAllServersAreBad()
        {
            serverList.GetAllEndPoints();

            var record = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<CMServerRecord>() { record } );
            serverList.TryMark( record.EndPoint, ServerQuality.Bad );

            var nextRecord = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Equal( record, nextRecord );
        }

        [Fact]
        public void GetNextServerCandidate_IsBiasedTowardsServerOrdering()
        {
            serverList.GetAllEndPoints();

            var goodRecord = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            var neutralRecord = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27016 ) );
            var badRecord = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27017 ) );

            serverList.ReplaceList( new List<CMServerRecord>() { badRecord, neutralRecord, goodRecord } );

            serverList.TryMark( badRecord.EndPoint, ServerQuality.Bad );
            serverList.TryMark( goodRecord.EndPoint, ServerQuality.Good );

            var nextRecord = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Equal( neutralRecord, nextRecord );

            serverList.TryMark( badRecord.EndPoint, ServerQuality.Good);

            nextRecord = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Equal( badRecord, nextRecord );
        }

        [Fact]
        public void GetNextServerCandidate_OnlyReturnsMatchingServerOfType_Socket()
        {
            var record = CMServerRecord.WebSocketServer( "localhost:443" );
            serverList.ReplaceList( new List<CMServerRecord>() { record } );

            var endPoint = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Null( endPoint );

            endPoint = serverList.GetNextServerCandidate( CMConnectionType.WebSocket );
            Assert.Same( record, endPoint );
        }

        [Fact]
        public void GetNextServerCandidate_OnlyReturnsMatchingServerOfType_WebSocket()
        {
            var record = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<CMServerRecord>() { record } );

            var endPoint = serverList.GetNextServerCandidate( CMConnectionType.WebSocket );
            Assert.Null( endPoint );

            endPoint = serverList.GetNextServerCandidate( CMConnectionType.Socket );
            Assert.Same( record, endPoint );
        }

        [Fact]
        public void TryMark_ReturnsTrue_IfServerInList()
        {
            var record = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ));
            serverList.ReplaceList( new List<CMServerRecord>() { record } );

            var marked = serverList.TryMark( record.EndPoint, ServerQuality.Good );
            Assert.True( marked );
        }

        [Fact]
        public void TryMark_ReturnsFalse_IfServerNotInList()
        {
            var record = CMServerRecord.SocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<CMServerRecord>() { record } );

            var marked = serverList.TryMark( new IPEndPoint( IPAddress.Loopback, 27016 ), ServerQuality.Good );
            Assert.False( marked );
        }
    }
}
