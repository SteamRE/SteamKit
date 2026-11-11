using System;
using System.Buffers;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2
{
    partial class WebSocketConnection : IConnection
    {
        internal class WebSocketContext : IDisposable
        {
            public WebSocketContext(WebSocketConnection connection, EndPoint endPoint)
            {
                this.connection = connection ?? throw new ArgumentNullException( nameof( connection ) );
                EndPoint = endPoint ?? throw new ArgumentNullException( nameof( endPoint ) );

                cts = new CancellationTokenSource();
                socket = new ClientWebSocket();
                connectionUri = ConstructUri(endPoint);
            }

            readonly WebSocketConnection connection;
            readonly CancellationTokenSource cts;
            readonly ClientWebSocket socket;
            readonly Uri connectionUri;
            Task? runloopTask;
            int disposed;

            public EndPoint EndPoint { get; }

            public void Start(HttpMessageInvoker invoker, TimeSpan connectionTimeout)
            {
                runloopTask = RunCore(invoker, connectionTimeout, cts.Token);
            }

            async Task RunCore(HttpMessageInvoker invoker, TimeSpan connectionTimeout, CancellationToken cancellationToken)
            {
                using (var timeout = new CancellationTokenSource())
                using (var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token))
                {
                    timeout.CancelAfter(connectionTimeout);

                    try
                    {
                        await socket.ConnectAsync(connectionUri, invoker, combinedCancellation.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) when (timeout.IsCancellationRequested)
                    {
                        connection.log.LogDebug(nameof(WebSocketContext), "Time out connecting websocket {0} after {1}", connectionUri, connectionTimeout);
                        connection.DisconnectCore(userInitiated: false, specificContext: this);
                        return;
                    }
                    catch (Exception ex)
                    {
                        connection.log.LogDebug( nameof(WebSocketContext), "Exception connecting websocket: {0} - {1}", ex.GetType().FullName, ex.Message);
                        connection.DisconnectCore(userInitiated: false, specificContext: this);
                        return;
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                connection.log.LogDebug( nameof(WebSocketContext), "Connected to {0}", connectionUri);

                try
                {
                    connection.Connected?.Invoke(connection, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    connection.log.LogDebug( nameof(WebSocketContext), "Exception while setting up connection to {0}: {1}", connectionUri, ex);
                    connection.DisconnectCore(userInitiated: false, specificContext: this);
                    return;
                }

                while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    byte[]? packet = null;

                    try
                    {
                        packet = await ReadMessageAsync( cancellationToken ).ConfigureAwait( false );
                    }
                    catch ( Exception ex )
                    {
                        connection.log.LogDebug( nameof( WebSocketContext ), "Exception reading from websocket: {0} - {1}", ex.GetType().FullName, ex.Message );
                        connection.DisconnectCore( userInitiated: false, specificContext: this );
                        return;
                    }

                    if (packet != null && packet.Length > 0)
                    {
                        try
                        {
                            connection.NetMsgReceived?.Invoke(connection, new NetMsgEventArgs(packet, EndPoint));
                        }
                        catch (Exception ex)
                        {
                            connection.log.LogDebug( nameof(WebSocketContext), "Unexpected exception propagated back to the read loop: {0}", ex);
                        }
                    }
                }
            }

            public ValueTask SendAsync(Memory<byte> data)
                => socket.SendAsync(data, WebSocketMessageType.Binary, true, cts.Token);

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) == 1)
                {
                    return;
                }

                if ( socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived )
                {
                    try
                    {
                        socket.CloseOutputAsync( WebSocketCloseStatus.NormalClosure, null, CancellationToken.None ).GetAwaiter().GetResult();
                    }
                    catch ( Exception ex )
                    {
                        connection.log.LogDebug( nameof( WebSocketContext ), "Exception closing websocket: {0} - {1}", ex.GetType().FullName, ex.Message );
                    }
                }

                cts.Cancel();
                cts.Dispose();
                runloopTask = null;
                socket.Dispose();
            }

            async Task<byte[]?> ReadMessageAsync( CancellationToken cancellationToken )
            {
                var buffer = ArrayPool<byte>.Shared.Rent( 1024 );
                var length = 0;

                try
                {
                    ValueWebSocketReceiveResult result;

                    do
                    {
                        if ( length == buffer.Length )
                        {
                            var newBuffer = ArrayPool<byte>.Shared.Rent( buffer.Length * 2 );
                            Buffer.BlockCopy( buffer, 0, newBuffer, 0, length );
                            ArrayPool<byte>.Shared.Return( buffer );
                            buffer = newBuffer;
                        }

                        result = await socket.ReceiveAsync( buffer.AsMemory( length ), cancellationToken ).ConfigureAwait( false );

                        if ( result.MessageType != WebSocketMessageType.Binary )
                        {
                            connection.DisconnectCore( userInitiated: false, specificContext: this );
                            return null;
                        }

                        length += result.Count;
                    }
                    while ( !result.EndOfMessage );

                    return buffer.AsSpan( 0, length ).ToArray();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return( buffer );
                }
            }

            internal static Uri ConstructUri(EndPoint endPoint)
            {
                var uri = new UriBuilder();
                uri.Scheme = "wss";
                uri.Path = "/cmsocket/";

                switch (endPoint)
                {
                    case IPEndPoint ipep:
                        uri.Port = ipep.Port;
                        uri.Host = ipep.Address.ToString();
                        break;

                    case DnsEndPoint dns:
                        uri.Host = dns.Host;
                        uri.Port = dns.Port;
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported endpoint type.");
                }

                return uri.Uri;
            }
        }
    }
}
