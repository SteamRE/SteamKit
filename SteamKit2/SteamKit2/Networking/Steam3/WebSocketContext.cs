using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2
{
    partial class WebSocketConnection : IConnection
    {
        class WebSocketContext : IDisposable
        {
            public WebSocketContext(WebSocketConnection connection, EndPoint endPoint)
            {
                this.connection = connection;
                EndPoint = endPoint;

                cts = new CancellationTokenSource();
                socket = new ClientWebSocket();
                hostAndPort = GetHostAndPort(endPoint);
            }

            readonly WebSocketConnection connection;
            readonly CancellationTokenSource cts;
            readonly ClientWebSocket socket;
            readonly string hostAndPort;
            Task runloopTask;
            int disposed;

            public EndPoint EndPoint { get; }

            public void Start(TimeSpan connectionTimeout)
            {
                runloopTask = RunCore(cts.Token, connectionTimeout);
            }

            async Task RunCore(CancellationToken cancellationToken, TimeSpan connectionTimeout)
            {
                var uri = new Uri(FormattableString.Invariant($"wss://{hostAndPort}/cmsocket/"));

                using (var timeout = new CancellationTokenSource())
                using (var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token))
                {
                    timeout.CancelAfter(connectionTimeout);

                    try
                    {
                        await socket.ConnectAsync(uri, combinedCancellation.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) when (timeout.IsCancellationRequested)
                    {
                        DebugLog.WriteLine(nameof(WebSocketContext), "Time out connecting websocket {0} after {1}", uri, connectionTimeout);
                        DisconnectNonBlocking(userInitiated: false);
                        return;
                    }
                    catch (Exception ex)
                    {
                        DebugLog.WriteLine(nameof(WebSocketContext), "Exception connecting websocket: {0} - {1}", ex.GetType().FullName, ex.Message);
                        DisconnectNonBlocking(userInitiated: false);
                        return;
                    }
                }

                DebugLog.WriteLine(nameof(WebSocketContext), "Connected to {0}", uri);
                connection.Connected?.Invoke(connection, EventArgs.Empty);

                while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    var packet = await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
                    if (packet != null)
                    {
                        connection.NetMsgReceived?.Invoke(connection, new NetMsgEventArgs(packet, EndPoint));
                    }
                }

                if (socket.State == WebSocketState.Open)
                {
                    DebugLog.WriteLine(nameof(WebSocketContext), "Closing connection...");
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default(CancellationToken)).ConfigureAwait(false);
                }
            }

            public async Task SendAsync(byte[] data)
            {
                var segment = new ArraySegment<byte>(data, 0, data.Length);
                await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cts.Token).ConfigureAwait(false);
                DebugLog.WriteLine(nameof(WebSocketContext), "Sent {0} bytes.", data.Length);
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) == 1)
                {
                    return;
                }

                cts.Cancel();
                cts.Dispose();

                try
                {
                    runloopTask?.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // We know, we canceled it.
                }
                runloopTask = null;

                socket.Dispose();
            }

            async Task<byte[]> ReadMessageAsync(CancellationToken cancellationToken)
            {
                using (var ms = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    var segment = new ArraySegment<byte>(buffer);

                    WebSocketReceiveResult result;
                    do
                    {

                        try
                        {
                            result = await socket.ReceiveAsync(segment, cancellationToken).ConfigureAwait(false);
                        }
                        catch (ObjectDisposedException)
                        {
                            DisconnectNonBlocking(userInitiated: cancellationToken.IsCancellationRequested);
                            return null;
                        }
                        catch (WebSocketException)
                        {
                            DisconnectNonBlocking(userInitiated: false);
                            return null;
                        }
                        catch (Win32Exception)
                        {
                            DisconnectNonBlocking(userInitiated: false);
                            return null;
                        }

                        switch (result.MessageType)
                        {
                            case WebSocketMessageType.Binary:
                                ms.Write(buffer, 0, result.Count);
                                DebugLog.WriteLine(nameof(WebSocketContext), "Recieved {0} bytes.", result.Count);
                                break;

                            case WebSocketMessageType.Text:
                                DebugLog.WriteLine(nameof(WebSocketContext), "Recieved websocket text message.");
                                break;

                            case WebSocketMessageType.Close:
                            default:
                                DisconnectNonBlocking(userInitiated: false);
                                return null;
                        }
                    }
                    while (!result.EndOfMessage);

                    return ms.ToArray();
                }
            }

            void DisconnectNonBlocking(bool userInitiated)
                => Task.Run(() => connection.DisconnectCore(userInitiated, this));

            static string GetHostAndPort(EndPoint endPoint)
            {
                switch (endPoint)
                {
                    case IPEndPoint ipep:
                        switch (ipep.AddressFamily)
                        {
                            case AddressFamily.InterNetwork:
                                return FormattableString.Invariant($"{ipep.Address}:{ipep.Port}");

                            case AddressFamily.InterNetworkV6:
                                // RFC 2732
                                return FormattableString.Invariant($"[{ipep.ToString()}]:{ipep.Port}");
                        }

                        break;

                    case DnsEndPoint dns:
                        return FormattableString.Invariant($"{dns.Host}:{dns.Port}");
                }

                throw new InvalidOperationException("Unsupported endpoint type.");
            }
        }
    }
}
