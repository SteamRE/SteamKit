using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SteamKit2;
using Xunit;

namespace Tests;

#if DEBUG
public class WebSocketConnectionFacts
{
    static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds( 5 );

    [Fact]
    public async Task DisconnectFromConnectedHandlerRaisesDisconnectedOnce()
    {
        using var fixture = new Fixture();

        var handlerFinished = new TaskCompletionSource<Exception>( TaskCreationOptions.RunContinuationsAsynchronously );
        fixture.Connection.Connected += ( sender, e ) =>
        {
            try
            {
                fixture.Connection.Disconnect( userInitiated: true );
                handlerFinished.TrySetResult( null );
            }
            catch ( Exception ex )
            {
                handlerFinished.TrySetResult( ex );
            }
        };

        fixture.Connection.Connect( fixture.EndPoint );

        var exception = await handlerFinished.Task.WaitAsync( WaitTimeout, TestContext.Current.CancellationToken );
        Assert.Null( exception );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.True( args.UserInitiated );
        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task DisconnectWhileDispatchingMessageRaisesDisconnected()
    {
        using var fixture = new Fixture();

        var dispatching = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        using var releaseHandler = new ManualResetEventSlim();
        fixture.Connection.NetMsgReceived += ( sender, e ) =>
        {
            dispatching.TrySetResult();
            releaseHandler.Wait( WaitTimeout );
        };

        fixture.Connection.Connect( fixture.EndPoint );
        var server = await fixture.Handler.AcceptServerAsync();
        await fixture.WaitConnectedAsync();

        await server.SendAsync( new byte[] { 1 }, WebSocketMessageType.Binary, true, CancellationToken.None );
        await dispatching.Task.WaitAsync( WaitTimeout, TestContext.Current.CancellationToken );

        try
        {
            fixture.Connection.Disconnect( userInitiated: true );
        }
        finally
        {
            releaseHandler.Set();
        }

        var args = await fixture.ReadDisconnectedAsync();
        Assert.True( args.UserInitiated );
    }

    [Fact]
    public async Task ThrowingNetMsgReceivedHandlerKeepsReading()
    {
        using var fixture = new Fixture();
        fixture.Connection.NetMsgReceived += ( sender, e ) => throw new InvalidOperationException( "handler failure" );

        var server = await fixture.ConnectAsync();

        await server.SendAsync( new byte[] { 1 }, WebSocketMessageType.Binary, true, CancellationToken.None );
        await server.SendAsync( new byte[] { 2 }, WebSocketMessageType.Binary, true, CancellationToken.None );

        var first = await fixture.ReadReceivedAsync();
        var second = await fixture.ReadReceivedAsync();
        Assert.Equal( new byte[] { 1 }, first.Data );
        Assert.Equal( new byte[] { 2 }, second.Data );
        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task ThrowingConnectedHandlerRaisesDisconnected()
    {
        using var fixture = new Fixture();
        fixture.Connection.Connected += ( sender, e ) => throw new InvalidOperationException( "handler failure" );

        fixture.Connection.Connect( fixture.EndPoint );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.False( args.UserInitiated );
    }

    [Fact]
    public async Task SendReachesNewServerAfterReconnect()
    {
        using var fixture = new Fixture();

        fixture.Connection.Connect( fixture.EndPoint );
        var firstServer = await fixture.Handler.AcceptServerAsync();
        await fixture.WaitConnectedAsync();

        var firstMessage = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        fixture.Connection.NetMsgReceived += ( sender, e ) => firstMessage.TrySetResult();
        await firstServer.SendAsync( new byte[] { 1 }, WebSocketMessageType.Binary, true, CancellationToken.None );
        await firstMessage.Task.WaitAsync( WaitTimeout, TestContext.Current.CancellationToken );

        fixture.Connection.Connect( fixture.EndPoint );
        var secondServer = await fixture.Handler.AcceptServerAsync();
        await fixture.WaitConnectedAsync();

        await WaitForCloseAsync( firstServer );

        var replacedArgs = await fixture.ReadDisconnectedAsync();
        Assert.False( replacedArgs.UserInitiated );

        var payload = new byte[] { 9, 8, 7, 6 };
        fixture.Connection.Send( payload );

        var received = await ReceiveBinaryAsync( secondServer );
        Assert.Equal( payload, received );

        fixture.Connection.Disconnect( userInitiated: true );

        var disconnectedArgs = await fixture.ReadDisconnectedAsync();
        Assert.True( disconnectedArgs.UserInitiated );
    }

    [Fact]
    public async Task BinaryMessageRaisesNetMsgReceived()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        var payload = new byte[] { 1, 2, 3, 4, 5 };
        await server.SendAsync( payload, WebSocketMessageType.Binary, true, CancellationToken.None );

        var args = await fixture.ReadReceivedAsync();
        Assert.Equal( payload, args.Data );
        Assert.Equal( fixture.EndPoint, args.EndPoint );
    }

    [Fact]
    public async Task LargeBinaryMessageRaisesNetMsgReceivedOnce()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        var payload = new byte[ 100 * 1024 ];
        for ( var i = 0; i < payload.Length; i++ )
        {
            payload[ i ] = ( byte )( i * 31 + i / 256 );
        }

        await server.SendAsync( payload, WebSocketMessageType.Binary, true, CancellationToken.None );
        var args = await fixture.ReadReceivedAsync();
        Assert.Equal( payload, args.Data );

        await server.SendAsync( new byte[] { 1 }, WebSocketMessageType.Binary, true, CancellationToken.None );
        var next = await fixture.ReadReceivedAsync();
        Assert.Equal( new byte[] { 1 }, next.Data );
    }

    [Fact]
    public async Task FragmentedMessageIsReassembled()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        await server.SendAsync( new byte[] { 1, 2, 3 }, WebSocketMessageType.Binary, false, CancellationToken.None );
        await server.SendAsync( new byte[] { 4, 5 }, WebSocketMessageType.Binary, false, CancellationToken.None );
        await server.SendAsync( new byte[] { 6 }, WebSocketMessageType.Binary, true, CancellationToken.None );

        var args = await fixture.ReadReceivedAsync();
        Assert.Equal( new byte[] { 1, 2, 3, 4, 5, 6 }, args.Data );

        await server.SendAsync( new byte[] { 7 }, WebSocketMessageType.Binary, true, CancellationToken.None );
        var next = await fixture.ReadReceivedAsync();
        Assert.Equal( new byte[] { 7 }, next.Data );
    }

    [Fact]
    public async Task TextMessageIsIgnored()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        var text = Encoding.UTF8.GetBytes( "hello" );
        await server.SendAsync( text, WebSocketMessageType.Text, true, CancellationToken.None );
        await server.SendAsync( new byte[] { 5 }, WebSocketMessageType.Binary, true, CancellationToken.None );

        var args = await fixture.ReadReceivedAsync();
        Assert.Equal( new byte[] { 5 }, args.Data );
        Assert.False( fixture.TryReadReceived( out _ ) );
        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task EmptyBinaryMessageIsIgnored()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        await server.SendAsync( Array.Empty<byte>(), WebSocketMessageType.Binary, true, CancellationToken.None );
        await server.SendAsync( new byte[] { 5 }, WebSocketMessageType.Binary, true, CancellationToken.None );

        var args = await fixture.ReadReceivedAsync();
        Assert.Equal( new byte[] { 5 }, args.Data );
        Assert.False( fixture.TryReadReceived( out _ ) );
    }

    [Fact]
    public async Task SendDeliversSingleBinaryMessage()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        var payload = new byte[] { 10, 20, 30 };
        fixture.Connection.Send( payload );

        var buffer = new byte[ 1024 ];
        using var cts = new CancellationTokenSource( WaitTimeout );
        var result = await server.ReceiveAsync( buffer.AsMemory(), cts.Token );

        Assert.Equal( WebSocketMessageType.Binary, result.MessageType );
        Assert.True( result.EndOfMessage );
        Assert.Equal( payload, buffer.AsSpan( 0, result.Count ).ToArray() );
    }

    [Fact]
    public void SendBeforeConnectDoesNotThrowOrDisconnect()
    {
        using var fixture = new Fixture();

        fixture.Connection.Send( new byte[] { 1 } );

        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task SendAfterDisconnectDoesNotThrow()
    {
        using var fixture = new Fixture();
        await fixture.ConnectAsync();

        fixture.Connection.Disconnect( userInitiated: true );
        await fixture.ReadDisconnectedAsync();

        fixture.Connection.Send( new byte[] { 1 } );

        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task ConnectRaisesConnectedAndSetsEndPoint()
    {
        using var fixture = new Fixture();
        await fixture.ConnectAsync();

        Assert.Equal( fixture.EndPoint, fixture.Connection.CurrentEndPoint );
        Assert.Equal( ProtocolTypes.WebSocket, fixture.Connection.ProtocolTypes );

        fixture.Connection.Disconnect( userInitiated: true );
        await fixture.ReadDisconnectedAsync();

        Assert.False( fixture.TryReadConnected() );
    }

    [Fact]
    public async Task UserDisconnectRaisesDisconnectedAndClosesServer()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        fixture.Connection.Disconnect( userInitiated: true );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.True( args.UserInitiated );
        Assert.Null( fixture.Connection.CurrentEndPoint );
        Assert.False( fixture.TryReadDisconnected( out _ ) );

        await WaitForCloseAsync( server );
        Assert.NotEqual( WebSocketState.Open, server.State );
    }

    [Fact]
    public async Task ServerCloseFrameRaisesDisconnected()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        using var cts = new CancellationTokenSource( WaitTimeout );
        await server.CloseOutputAsync( WebSocketCloseStatus.NormalClosure, null, cts.Token );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.False( args.UserInitiated );
        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task ServerAbortRaisesDisconnected()
    {
        using var fixture = new Fixture();
        var server = await fixture.ConnectAsync();

        server.Abort();

        var args = await fixture.ReadDisconnectedAsync();
        Assert.False( args.UserInitiated );
        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public void DisconnectWhenNeverConnectedDoesNothing()
    {
        using var fixture = new Fixture();

        fixture.Connection.Disconnect( userInitiated: true );

        Assert.False( fixture.TryReadDisconnected( out _ ) );
    }

    [Fact]
    public async Task ConnectTimeoutRaisesDisconnected()
    {
        var handler = new DelegateHandler( async ( request, cancellationToken ) =>
        {
            await Task.Delay( Timeout.Infinite, cancellationToken );
            return null;
        } );
        using var fixture = new Fixture( handler );

        fixture.Connection.Connect( fixture.EndPoint, timeout: 200 );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.False( args.UserInitiated );
        Assert.False( fixture.TryReadConnected() );
    }

    [Fact]
    public async Task RejectedHandshakeRaisesDisconnected()
    {
        var handler = new DelegateHandler( async ( request, cancellationToken ) =>
        {
            await Task.Yield();
            return new HttpResponseMessage( HttpStatusCode.NotFound );
        } );
        using var fixture = new Fixture( handler );

        fixture.Connection.Connect( fixture.EndPoint );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.False( args.UserInitiated );
        Assert.False( fixture.TryReadConnected() );
    }

    [Fact]
    public async Task SynchronouslyRejectedHandshakeRaisesDisconnected()
    {
        var handler = new DelegateHandler( ( request, cancellationToken ) => Task.FromResult( new HttpResponseMessage( HttpStatusCode.NotFound ) ) );
        using var fixture = new Fixture( handler );

        fixture.Connection.Connect( fixture.EndPoint );

        var args = await fixture.ReadDisconnectedAsync();
        Assert.False( args.UserInitiated );
        Assert.False( fixture.TryReadConnected() );
    }

    sealed class DelegateHandler : HttpMessageHandler
    {
        readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback;

        public DelegateHandler( Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback )
        {
            this.callback = callback;
        }

        protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            => callback( request, cancellationToken );
    }

    static async Task WaitForCloseAsync( WebSocket server )
    {
        var buffer = new byte[ 1024 ];
        using var cts = new CancellationTokenSource( WaitTimeout );

        try
        {
            var result = await server.ReceiveAsync( buffer, cts.Token );
            while ( result.MessageType != WebSocketMessageType.Close )
            {
                result = await server.ReceiveAsync( buffer, cts.Token );
            }
        }
        catch ( WebSocketException )
        {
        }
    }

    static async Task<byte[]> ReceiveBinaryAsync( WebSocket server )
    {
        var buffer = new byte[ 1024 ];
        using var cts = new CancellationTokenSource( WaitTimeout );

        ValueWebSocketReceiveResult result;

        try
        {
            result = await server.ReceiveAsync( buffer.AsMemory(), cts.Token );
        }
        catch ( OperationCanceledException )
        {
            Assert.Fail( "Server did not receive a message in time." );
            return null;
        }

        Assert.Equal( WebSocketMessageType.Binary, result.MessageType );

        return buffer.AsSpan( 0, result.Count ).ToArray();
    }

    sealed class Fixture : IDisposable
    {
        readonly Channel<bool> connected = Channel.CreateUnbounded<bool>();
        readonly Channel<DisconnectedEventArgs> disconnected = Channel.CreateUnbounded<DisconnectedEventArgs>();
        readonly Channel<NetMsgEventArgs> received = Channel.CreateUnbounded<NetMsgEventArgs>();
        readonly HttpMessageInvoker invoker;

        public Fixture()
            : this( new LoopbackWebSocketHandler() )
        {
        }

        public Fixture( HttpMessageHandler handler )
        {
            Handler = handler as LoopbackWebSocketHandler;
            invoker = new HttpMessageInvoker( handler );
            Connection = new WebSocketConnection( DebugLogContext.Instance, invoker );
            Connection.Connected += ( sender, e ) => connected.Writer.TryWrite( true );
            Connection.Disconnected += ( sender, e ) => disconnected.Writer.TryWrite( e );
            Connection.NetMsgReceived += ( sender, e ) => received.Writer.TryWrite( e );
        }

        public LoopbackWebSocketHandler Handler { get; }
        public WebSocketConnection Connection { get; }
        public EndPoint EndPoint { get; } = new IPEndPoint( IPAddress.Loopback, 1 );

        public async Task<NetMsgEventArgs> ReadReceivedAsync()
            => await received.Reader.ReadAsync().AsTask().WaitAsync( WaitTimeout );

        public bool TryReadReceived( out NetMsgEventArgs args )
            => received.Reader.TryRead( out args );

        public bool TryReadConnected()
            => connected.Reader.TryRead( out _ );

        public async Task<WebSocket> ConnectAsync()
        {
            Connection.Connect( EndPoint );
            var server = await Handler.AcceptServerAsync();
            await WaitConnectedAsync();
            return server;
        }

        public async Task WaitConnectedAsync()
            => await connected.Reader.ReadAsync().AsTask().WaitAsync( WaitTimeout );

        public async Task<DisconnectedEventArgs> ReadDisconnectedAsync()
            => await disconnected.Reader.ReadAsync().AsTask().WaitAsync( WaitTimeout );

        public bool TryReadDisconnected( out DisconnectedEventArgs args )
            => disconnected.Reader.TryRead( out args );

        public void Dispose()
        {
            Connection.Dispose();
            invoker.Dispose();
        }
    }
}
#endif
