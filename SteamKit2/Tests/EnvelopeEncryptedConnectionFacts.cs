using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using SteamKit2;
using SteamKit2.Internal;
using Xunit;

namespace Tests
{
#if DEBUG
    public class EnvelopeEncryptedConnectionFacts
    {
        sealed class FakeConnection : IConnection
        {
            static readonly IPEndPoint FakeEndPoint = new( IPAddress.Loopback, 27015 );

            public List<bool> Disconnects { get; } = [];

            public EndPoint CurrentEndPoint => FakeEndPoint;

            public ProtocolTypes ProtocolTypes => ProtocolTypes.Tcp;

            public event EventHandler<NetMsgEventArgs> NetMsgReceived;

            public event EventHandler Connected;

            public event EventHandler<DisconnectedEventArgs> Disconnected
            {
                add { }
                remove { }
            }

            public void Connect( EndPoint endPoint, int timeout = 5000 )
            {
            }

            public void Disconnect( bool userInitiated )
            {
                Disconnects.Add( userInitiated );
            }

            public void Send( Memory<byte> data )
            {
            }

            public IPAddress GetLocalIP() => IPAddress.Loopback;

            public void RaiseConnected()
            {
                Connected?.Invoke( this, EventArgs.Empty );
            }

            public void RaiseNetMsgReceived( byte[] data )
            {
                NetMsgReceived?.Invoke( this, new NetMsgEventArgs( data, FakeEndPoint ) );
            }
        }

        static byte[] SerializeEncryptRequest()
        {
            var request = new Msg<MsgChannelEncryptRequest>();
            request.Body.Universe = EUniverse.Public;
            request.Body.ProtocolVersion = 1;
            request.Write( RandomNumberGenerator.GetBytes( 16 ) );

            return request.Serialize();
        }

        static byte[] SerializeEncryptResult()
        {
            var result = new Msg<MsgChannelEncryptResult>();
            result.Body.Result = EResult.OK;

            return result.Serialize();
        }

        [Fact]
        public void DisconnectsWhenEncryptedPacketCannotBeDecrypted()
        {
            var inner = new FakeConnection();
            var connection = new EnvelopeEncryptedConnection( inner, EUniverse.Public, DebugLogContext.Instance, debugNetworkListener: null );

            var connectedCount = 0;
            var receivedCount = 0;
            connection.Connected += ( sender, e ) => connectedCount++;
            connection.NetMsgReceived += ( sender, e ) => receivedCount++;

            inner.RaiseConnected();
            inner.RaiseNetMsgReceived( SerializeEncryptRequest() );
            inner.RaiseNetMsgReceived( SerializeEncryptResult() );

            Assert.Equal( 1, connectedCount );
            Assert.Empty( inner.Disconnects );

            var garbage = RandomNumberGenerator.GetBytes( 48 );
            var exception = Record.Exception( () => inner.RaiseNetMsgReceived( garbage ) );

            Assert.Null( exception );
            Assert.Equal( 0, receivedCount );
            Assert.Equal( [ false ], inner.Disconnects );
        }
    }
#endif
}
