using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2
{
    class WebSocketConnection : IConnection
    {
        WebSocketContext currentContext;

        public event EventHandler<NetMsgEventArgs> NetMsgReceived;

        public event EventHandler Connected;

        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public EndPoint CurrentEndPoint => currentContext?.EndPoint;

        public ProtocolTypes ProtocolTypes => ProtocolTypes.WebSocket;

        public void Connect(EndPoint endPoint, int timeout = 5000)
        {
            if (!(endPoint is DnsEndPoint dnsEp))
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Given endpoint was not a DnsEndPoint.");
                Disconnected?.Invoke(this, new DisconnectedEventArgs(false));
                return;
            }

            var newContext = new WebSocketContext(this, dnsEp);
            var oldContext = Interlocked.Exchange(ref currentContext, newContext);
            if (oldContext != null)
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Attempted to connect while already connected. Closing old connection...");
                oldContext.Dispose();
                Disconnected?.Invoke(this, new DisconnectedEventArgs(false));
            }

            newContext.Start(TimeSpan.FromMilliseconds(timeout));
        }

        public void Disconnect()
            => DisconnectCore(userInitiated: true, specificContext: null);

        public IPAddress GetLocalIP() => IPAddress.None;

        public void Send(byte[] data)
        {
            try
            {
                currentContext?.SendAsync(data).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Exception while sending data: {0} - {1}", ex.GetType().FullName, ex.Message);
                DisconnectCore(userInitiated: false, specificContext: null);
            }
        }

        void DisconnectCore(bool userInitiated, WebSocketContext specificContext)
        {
            var oldContext = Interlocked.Exchange(ref currentContext, null);
            if (oldContext != null && (specificContext == null || oldContext == specificContext))
            {
                oldContext.Dispose();

                Disconnected?.Invoke(this, new DisconnectedEventArgs(userInitiated));
            }
            else
            {
                specificContext?.Dispose();
            }
        }

        class WebSocketContext : IDisposable
        {
            public WebSocketContext(WebSocketConnection connection, DnsEndPoint endPoint)
            {
                this.connection = connection;
                EndPoint = endPoint;

                cts = new CancellationTokenSource();
                socket = new ClientWebSocket();
            }

            readonly WebSocketConnection connection;
            readonly CancellationTokenSource cts;
            readonly ClientWebSocket socket;
            Task runloopTask;
            bool disposed;

            public DnsEndPoint EndPoint { get; }

            public void Start(TimeSpan connectionTimeout)
            {
                runloopTask = RunCore(cts.Token, connectionTimeout);
            }

            async Task RunCore(CancellationToken cancellationToken, TimeSpan connectionTimeout)
            {
                var uri = new Uri(FormattableString.Invariant($"wss://{EndPoint.Host}:{EndPoint.Port}/cmsocket/"));

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
                if (disposed)
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

                disposed = true;
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
        }
    }
}
