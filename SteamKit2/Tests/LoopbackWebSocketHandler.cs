using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Tests;

sealed class UpgradedStreamContent : HttpContent
{
    readonly Stream stream;

    public UpgradedStreamContent( Stream stream )
    {
        this.stream = stream;
    }

    protected override Task SerializeToStreamAsync( Stream target, TransportContext context )
        => Task.CompletedTask;

    protected override Stream CreateContentReadStream( CancellationToken cancellationToken )
        => stream;

    protected override Task<Stream> CreateContentReadStreamAsync()
        => Task.FromResult( stream );

    protected override bool TryComputeLength( out long length )
    {
        length = 0;
        return false;
    }
}

sealed class LoopbackWebSocketHandler : HttpMessageHandler
{
    const string AcceptGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    static readonly TimeSpan AcceptTimeout = TimeSpan.FromSeconds( 5 );

    readonly Channel<WebSocket> servers = Channel.CreateUnbounded<WebSocket>();
    readonly List<IDisposable> resources = new List<IDisposable>();

    public async Task<WebSocket> AcceptServerAsync()
        => await servers.Reader.ReadAsync().AsTask().WaitAsync( AcceptTimeout );

    protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
    {
        var key = string.Join( string.Empty, request.Headers.GetValues( "Sec-WebSocket-Key" ) );
        var acceptHash = SHA1.HashData( Encoding.ASCII.GetBytes( key + AcceptGuid ) );

        using var listener = new TcpListener( IPAddress.Loopback, 0 );
        listener.Start();

        var clientTcp = new TcpClient();
        var acceptTask = listener.AcceptTcpClientAsync( cancellationToken ).AsTask();
        var port = ( ( IPEndPoint )listener.LocalEndpoint ).Port;
        await clientTcp.ConnectAsync( IPAddress.Loopback, port, cancellationToken );
        var serverTcp = await acceptTask;

        var serverSocket = WebSocket.CreateFromStream( serverTcp.GetStream(), isServer: true, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan );

        lock ( resources )
        {
            resources.Add( clientTcp );
            resources.Add( serverTcp );
            resources.Add( serverSocket );
        }

        servers.Writer.TryWrite( serverSocket );

        var response = new HttpResponseMessage( HttpStatusCode.SwitchingProtocols );
        response.Headers.TryAddWithoutValidation( "Connection", "Upgrade" );
        response.Headers.TryAddWithoutValidation( "Upgrade", "websocket" );
        response.Headers.TryAddWithoutValidation( "Sec-WebSocket-Accept", Convert.ToBase64String( acceptHash ) );
        response.Content = new UpgradedStreamContent( clientTcp.GetStream() );
        return response;
    }

    protected override void Dispose( bool disposing )
    {
        if ( disposing )
        {
            lock ( resources )
            {
                foreach ( var resource in resources )
                {
                    resource.Dispose();
                }
            }
        }

        base.Dispose( disposing );
    }
}
