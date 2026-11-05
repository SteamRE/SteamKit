using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.Discovery;
using SteamKit2.Internal;
using Xunit;

namespace Tests;

public class SteamClientWebSocketFacts
{
    static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds( 5 );

    [Fact]
    public async Task DisconnectWhileDispatchingMessageRaisesDisconnectedCallback()
    {
        using var fixture = new Fixture();

        var dispatching = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        using var releaseHandler = new ManualResetEventSlim();
        fixture.Client.AddHandler( new BlockingHandler( dispatching, releaseHandler ) );

        var server = await fixture.ConnectAsync();

        var message = new ClientMsgProtobuf<CMsgClientHello>( EMsg.ClientHello );
        await server.SendAsync( message.Serialize(), WebSocketMessageType.Binary, true, CancellationToken.None );
        await dispatching.Task.WaitAsync( WaitTimeout, TestContext.Current.CancellationToken );

        try
        {
            fixture.Client.Disconnect();
        }
        finally
        {
            releaseHandler.Set();
        }

        var disconnected = await fixture.WaitForCallbackAsync<SteamClient.DisconnectedCallback>();
        Assert.True( disconnected.UserInitiated );
        Assert.False( fixture.Client.IsConnected );
    }

    [Fact]
    public async Task UnparseablePacketRaisesDisconnectedCallback()
    {
        using var fixture = new Fixture();

        var server = await fixture.ConnectAsync();

        await server.SendAsync( new byte[] { 1, 2, 3 }, WebSocketMessageType.Binary, true, CancellationToken.None );

        var disconnected = await fixture.WaitForCallbackAsync<SteamClient.DisconnectedCallback>();
        Assert.False( disconnected.UserInitiated );
        Assert.False( fixture.Client.IsConnected );
    }

    sealed class BlockingHandler : ClientMsgHandler
    {
        readonly TaskCompletionSource dispatching;
        readonly ManualResetEventSlim release;

        public BlockingHandler( TaskCompletionSource dispatching, ManualResetEventSlim release )
        {
            this.dispatching = dispatching;
            this.release = release;
        }

        public override void HandleMsg( IPacketMsg packetMsg )
        {
            dispatching.TrySetResult();
            release.Wait( WaitTimeout );
        }
    }

    sealed class Fixture : IDisposable
    {
        readonly LoopbackWebSocketHandler handler = new LoopbackWebSocketHandler();

        public Fixture()
        {
            var configuration = SteamConfiguration.Create( builder => builder
                .WithProtocolTypes( ProtocolTypes.WebSocket )
                .WithHttpClientFactory( purpose => new HttpClient( handler, disposeHandler: false ) ) );

            Client = new SteamClient( configuration );
        }

        public SteamClient Client { get; }

        public async Task<WebSocket> ConnectAsync()
        {
            Client.Connect( ServerRecord.CreateWebSocketServer( "localhost:1" ) );

            var server = await handler.AcceptServerAsync();
            await WaitForCallbackAsync<SteamClient.ConnectedCallback>();
            Assert.True( Client.IsConnected );

            return server;
        }

        public async Task<T> WaitForCallbackAsync<T>() where T : CallbackMsg
        {
            using var cts = new CancellationTokenSource( WaitTimeout );

            try
            {
                while ( true )
                {
                    var callback = await Client.WaitForCallbackAsync( cts.Token );
                    if ( callback is T match )
                    {
                        return match;
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                Assert.Fail( $"{typeof( T ).Name} was not posted in time." );
                return null;
            }
        }

        public void Dispose()
        {
            handler.Dispose();
        }
    }
}
