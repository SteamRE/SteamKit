using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SteamKit2
{
    class WebSocketConnection : Connection
    {
        WebSocketContext currentContext;

        public override EndPoint CurrentEndPoint => currentContext?.EndPoint;

        public override CMConnectionType Kind => CMConnectionType.WebSocket;

        public override async void Connect(Task<EndPoint> endPointTask, int timeout = 5000)
        {
            EndPoint endPoint;
            try
            {
                endPoint = await endPointTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Exception while awaiting endpoint task: {0} - {1}", ex.GetType().FullName, ex.Message);
                OnDisconnected(new DisconnectedEventArgs(false));
                return;
            }

            if (!(endPoint is DnsEndPoint dnsEp))
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Given endpoint was not a DnsEndPoint.");
                OnDisconnected(new DisconnectedEventArgs(false));
                return;
            }

            var newContext = new WebSocketContext(this, dnsEp);
            var oldContext = Interlocked.Exchange(ref currentContext, newContext);
            if (oldContext != null)
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Attempted to connect while already connected. Closing old connection...");
                oldContext.Dispose();
                OnDisconnected(new DisconnectedEventArgs(false));
            }

            newContext.Start();
        }

        public override void Disconnect()
            => DisconnectCore(userInitiated: true, specificContext: null);

        public override IPAddress GetLocalIP() => IPAddress.None;

        public override void Send(IClientMsg clientMsg)
        {
            try
            {
                currentContext?.SendAsync(clientMsg).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine(nameof(WebSocketConnection), "Exception while sending data: {0} - {1}", ex.GetType().FullName, ex.Message);
                DisconnectCore(userInitiated: false, specificContext: null);
            }
        }

        public override void SetNetEncryptionFilter(INetFilterEncryption filter)
            => throw new NotSupportedException("Websocket uses the underlying TLS encryption. Custom encryption is not supported.");

        void DisconnectCore(bool userInitiated, WebSocketContext specificContext)
        {
            var oldContext = Interlocked.Exchange(ref currentContext, null);
            if (oldContext != null && (specificContext == null || oldContext == specificContext))
            {
                oldContext.Dispose();

                OnDisconnected(new DisconnectedEventArgs(userInitiated));
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

            public DnsEndPoint EndPoint { get; }

            public void Start()
            {
                runloopTask = RunCore();
            }

            async Task RunCore()
            {
                var uri = new Uri(FormattableString.Invariant($"wss://{EndPoint.Host}:{EndPoint.Port}/cmsocket/"));

                try
                {
                    await socket.ConnectAsync(uri, cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine(nameof(WebSocketContext), "Exception connecting websocket: {0} - {1}", ex.GetType().FullName, ex.Message);
                    DisconnectNonBlocking(userInitiated: false);
                    return;
                }

                DebugLog.WriteLine(nameof(WebSocketContext), "Connected to {0}", uri);
                connection.OnConnected(new ConnectedEventArgs(secureChannel: true, universe: EUniverse.Public));

                while (!cts.Token.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    var packet = await ReadMessageAsync().ConfigureAwait(false);
                    if (packet != null)
                    {
                        connection.OnNetMsgReceived(new NetMsgEventArgs(packet, EndPoint));
                    }
                }

                if (socket.State == WebSocketState.Open)
                {
                    DebugLog.WriteLine(nameof(WebSocketContext), "Closing connection...");
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default(CancellationToken)).ConfigureAwait(false);
                }
            }

            public async Task SendAsync(IClientMsg clientMsg)
            {
                var data = clientMsg.Serialize();
                var segment = new ArraySegment<byte>(data, 0, data.Length);
                await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cts.Token).ConfigureAwait(false);
                DebugLog.WriteLine(nameof(WebSocketContext), "Sent {0} bytes.", data.Length);
            }

            public void Dispose()
            {
                cts.Cancel();
                cts.Dispose();

                runloopTask?.Wait();
                runloopTask = null;

                socket.Dispose();
            }

            async Task<byte[]> ReadMessageAsync()
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
                            result = await socket.ReceiveAsync(segment, cts.Token).ConfigureAwait(false);
                        }
                        catch (ObjectDisposedException)
                        {
                            DisconnectNonBlocking(userInitiated: cts.Token.IsCancellationRequested);
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
