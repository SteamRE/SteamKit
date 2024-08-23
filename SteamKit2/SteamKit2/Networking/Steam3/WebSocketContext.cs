using System;
using System.Buffers;
using System.ComponentModel;
using System.Net;
using System.Net.WebSockets;
using System.Text;
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

            public void Start(TimeSpan connectionTimeout)
            {
                runloopTask = RunCore(connectionTimeout, cts.Token).IgnoringCancellation(cts.Token);
            }

            async Task RunCore(TimeSpan connectionTimeout, CancellationToken cancellationToken)
            {
                using (var timeout = new CancellationTokenSource())
                using (var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token))
                {
                    timeout.CancelAfter(connectionTimeout);

                    try
                    {
                        await socket.ConnectAsync(connectionUri, combinedCancellation.Token).ConfigureAwait(false);
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

                connection.log.LogDebug( nameof(WebSocketContext), "Connected to {0}", connectionUri);
                connection.Connected?.Invoke(connection, EventArgs.Empty);

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
                        connection.NetMsgReceived?.Invoke(connection, new NetMsgEventArgs(packet, EndPoint));
                    }
                }

                if (socket.State == WebSocketState.Open)
                {
                    connection.log.LogDebug( nameof(WebSocketContext), "Closing connection...");
                    try
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default).ConfigureAwait(false);
                    }
                    catch (Win32Exception ex)
                    {
                        connection.log.LogDebug( nameof(WebSocketContext), "Error closing connection: {0}", ex.Message);
                    }
                }
            }

            public async Task SendAsync(Memory<byte> data)
            {
                try
                {
                    await socket.SendAsync(data, WebSocketMessageType.Binary, true, cts.Token).ConfigureAwait(false);
                }
                catch (WebSocketException ex)
                {
                    connection.log.LogDebug( nameof(WebSocketContext), "{0} exception when sending message: {1}", ex.GetType().FullName, ex.Message);
                    connection.DisconnectCore(userInitiated: false, specificContext: this);
                    return;
                }
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) == 1)
                {
                    return;
                }

                cts.Cancel();
                cts.Dispose();
                runloopTask = null;

                socket.Dispose();
            }

            async Task<byte[]?> ReadMessageAsync( CancellationToken cancellationToken )
            {
                var outputBuffer = ArrayPool<byte>.Shared.Rent( 1024 );
                var readBuffer = ArrayPool<byte>.Shared.Rent( 1024 );
                var readMemory = readBuffer.AsMemory();

                ValueWebSocketReceiveResult result;
                var outputLength = 0;

                try
                {
                    do
                    {
                        try
                        {
                            result = await socket.ReceiveAsync( readMemory, cancellationToken ).ConfigureAwait( false );
                        }
                        catch ( ObjectDisposedException )
                        {
                            connection.DisconnectCore( userInitiated: cancellationToken.IsCancellationRequested, specificContext: this );
                            return null;
                        }
                        catch ( WebSocketException )
                        {
                            connection.DisconnectCore( userInitiated: false, specificContext: this );
                            return null;
                        }
                        catch ( Win32Exception )
                        {
                            connection.DisconnectCore( userInitiated: false, specificContext: this );
                            return null;
                        }

                        switch ( result.MessageType )
                        {
                            case WebSocketMessageType.Binary:
                                if ( outputLength + result.Count > outputBuffer.Length )
                                {
                                    var newBuffer = ArrayPool<byte>.Shared.Rent( outputBuffer.Length * 2 );
                                    Buffer.BlockCopy( outputBuffer, 0, newBuffer, 0, outputLength );
                                    ArrayPool<byte>.Shared.Return( outputBuffer );
                                    outputBuffer = newBuffer;
                                }

                                Buffer.BlockCopy( readBuffer, 0, outputBuffer, outputLength, result.Count );
                                outputLength += result.Count;

                                break;

                            case WebSocketMessageType.Text:
                                try
                                {
                                    var message = Encoding.UTF8.GetString( readBuffer, 0, result.Count );
                                    connection.log.LogDebug( nameof( WebSocketContext ), "Received websocket text message: \"{0}\"", message );
                                }
                                catch
                                {
                                    var frameBytes = new byte[ result.Count ];
                                    Array.Copy( readBuffer, 0, frameBytes, 0, result.Count );
                                    connection.log.LogDebug( nameof( WebSocketContext ), "Received websocket text message: 0x{0}", Utils.EncodeHexString( frameBytes ) );
                                }
                                break;

                            case WebSocketMessageType.Close:
                            default:
                                connection.DisconnectCore( userInitiated: false, specificContext: this );
                                return null;
                        }
                    }
                    while ( !result.EndOfMessage );

                    var output = new byte[ outputLength ];
                    Buffer.BlockCopy( outputBuffer, 0, output, 0, output.Length );

                    return output;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return( readBuffer );
                    ArrayPool<byte>.Shared.Return( outputBuffer );
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
