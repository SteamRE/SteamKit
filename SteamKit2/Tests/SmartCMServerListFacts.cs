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
            var configuration = SteamConfiguration.Create(b => b.WithDirectoryFetch(false));
            serverList = new SmartCMServerList(configuration);
        }

        readonly SmartCMServerList serverList;
        
        [Fact]
        public void TryMergeWithList_AddsToHead_AndMovesExisting()
        {
            serverList.GetAllEndPoints();

            var seedList = new[]
            {
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27025 )),
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27035 )),
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27045 )),
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27105 )),
            };
            serverList.ReplaceList( seedList );
            Assert.Equal( 4, seedList.Length );

            var listToReplace = new[]
            {
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27015 )),
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27035 )),
                ServerRecord.CreateSocketServer(new IPEndPoint( IPAddress.Loopback, 27105 )),
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

            var record = ServerRecord.CreateSocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( record.EndPoint, nextRecord.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, nextRecord.ProtocolTypes );
        }

        [Fact]
        public void GetNextServerCandidate_ReturnsServer_IfListHasServers_EvenIfAllServersAreBad()
        {
            serverList.GetAllEndPoints();

            var record = ServerRecord.CreateSocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );
            serverList.TryMark( record.EndPoint, record.ProtocolTypes, ServerQuality.Bad );

            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( record.EndPoint, nextRecord.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, nextRecord.ProtocolTypes );
        }

        [Fact]
        public void GetNextServerCandidate_IsBiasedTowardsServerOrdering()
        {
            serverList.GetAllEndPoints();

            var serverA = IPAddress.Parse( "10.0.0.1" );
            var serverB = IPAddress.Parse( "10.0.0.2" );
            
            var goodRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverA, 27015 ) );
            var neutralRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverA, 27016 ) );
            var badRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverA, 27017 ) );
            var serverBRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverB, 27017 ) );

            serverList.ReplaceList( new List<ServerRecord>() { badRecord, neutralRecord, goodRecord, serverBRecord } );

            serverList.TryMark( badRecord.EndPoint, badRecord.ProtocolTypes, ServerQuality.Bad );
            serverList.TryMark( goodRecord.EndPoint, goodRecord.ProtocolTypes, ServerQuality.Good );
            
            // Server A's endpoints were all marked bad, with goodRecord being recovered
            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( goodRecord.EndPoint, nextRecord.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, nextRecord.ProtocolTypes );

            serverList.TryMark( badRecord.EndPoint, badRecord.ProtocolTypes, ServerQuality.Good);
            
            // Server A's bad record is now at the front, having been marked good
            nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( badRecord.EndPoint, nextRecord.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, nextRecord.ProtocolTypes );
        }

        
        [Fact]
        public void GetNextServerCandidate_AllEndpointsByHostAreBad()
        {
            serverList.GetAllEndPoints();

            var serverA = IPAddress.Parse( "10.0.0.1" );
            var serverB = IPAddress.Parse( "10.0.0.2" );
            
            var goodRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverA, 27015 ) );
            var neutralRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverA, 27016 ) );
            var badRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverA, 27017 ) );
            var serverBRecord = ServerRecord.CreateSocketServer( new IPEndPoint( serverB, 27017 ) );

            serverList.ReplaceList( new List<ServerRecord>() { goodRecord, neutralRecord, badRecord, serverBRecord } );

            serverList.TryMark( goodRecord.EndPoint, goodRecord.ProtocolTypes, ServerQuality.Good );
            serverList.TryMark( badRecord.EndPoint, badRecord.ProtocolTypes, ServerQuality.Bad );

            // Server A's endpoints are all bad. Server B is our next candidate.
            var nextRecord = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( serverBRecord.EndPoint, nextRecord.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, nextRecord.ProtocolTypes );
        }
        
        [Fact]
        public void GetNextServerCandidate_OnlyReturnsMatchingServerOfType()
        {
            var record = ServerRecord.CreateWebSocketServer( "localhost:443" );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Null( endPoint );
            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Udp );
            Assert.Null( endPoint );
            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp | ProtocolTypes.Udp);
            Assert.Null( endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.WebSocket );
            Assert.Equal( record.EndPoint, endPoint.EndPoint );
            Assert.Equal( ProtocolTypes.WebSocket, endPoint.ProtocolTypes );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.All );
            Assert.Equal( record.EndPoint, endPoint.EndPoint );
            Assert.Equal( ProtocolTypes.WebSocket, endPoint.ProtocolTypes );

            record = ServerRecord.CreateSocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.WebSocket );
            Assert.Null( endPoint );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp );
            Assert.Equal( record.EndPoint, endPoint.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, endPoint.ProtocolTypes );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Udp);
            Assert.Equal( record.EndPoint, endPoint.EndPoint );
            Assert.Equal( ProtocolTypes.Udp, endPoint.ProtocolTypes );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.Tcp | ProtocolTypes.Udp );
            Assert.Equal( record.EndPoint, endPoint.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, endPoint.ProtocolTypes );

            endPoint = serverList.GetNextServerCandidate( ProtocolTypes.All );
            Assert.Equal( record.EndPoint, endPoint.EndPoint );
            Assert.Equal( ProtocolTypes.Tcp, endPoint.ProtocolTypes );
        }

        [Fact]
        public void TryMark_ReturnsTrue_IfServerInList()
        {
            var record = ServerRecord.CreateSocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ));
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var marked = serverList.TryMark( record.EndPoint, record.ProtocolTypes, ServerQuality.Good );
            Assert.True( marked );
        }

        [Fact]
        public void TryMark_ReturnsFalse_IfServerNotInList()
        {
            var record = ServerRecord.CreateSocketServer( new IPEndPoint( IPAddress.Loopback, 27015 ) );
            serverList.ReplaceList( new List<ServerRecord>() { record } );

            var marked = serverList.TryMark( new IPEndPoint( IPAddress.Loopback, 27016 ), record.ProtocolTypes, ServerQuality.Good );
            Assert.False( marked );
        }
    }
}
