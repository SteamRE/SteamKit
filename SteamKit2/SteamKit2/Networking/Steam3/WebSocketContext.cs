using System;
using System.ComponentModel;
using System.IO;
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
                    var packet = await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
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

            public async Task SendAsync(byte[] data)
            {
                var segment = new ArraySegment<byte>(data, 0, data.Length);
                try
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cts.Token).ConfigureAwait(false);
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

            async Task<byte[]?> ReadMessageAsync(CancellationToken cancellationToken)
            {
                using var ms = new MemoryStream();
                var buffer = new byte[ 1024 ];
                var segment = new ArraySegment<byte>( buffer );

                WebSocketReceiveResult result;
                do
                {
                    try
                    {
                        result = await socket.ReceiveAsync( segment, cancellationToken ).ConfigureAwait( false );
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
                            ms.Write( buffer, 0, result.Count );
                            break;

                        case WebSocketMessageType.Text:
                            try
                            {
                                var message = Encoding.UTF8.GetString( buffer, 0, result.Count );
                                connection.log.LogDebug( nameof( WebSocketContext ), "Recieved websocket text message: \"{0}\"", message );
                            }
                            catch
                            {
                                var frameBytes = new byte[ result.Count ];
                                Array.Copy( buffer, 0, frameBytes, 0, result.Count );
                                var frameHexBytes = BitConverter.ToString( frameBytes ).Replace( "-", string.Empty );
                                connection.log.LogDebug( nameof( WebSocketContext ), "Recieved websocket text message: 0x{0}", frameHexBytes );
                            }
                            break;

                        case WebSocketMessageType.Close:
                        default:
                            connection.DisconnectCore( userInitiated: false, specificContext: this );
                            return null;
                    }
                }
                while ( !result.EndOfMessage );

                return ms.ToArray();
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
