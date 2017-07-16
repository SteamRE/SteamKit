using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace SteamKit2
{
    partial class WebSocketConnection : IConnection
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
    }
}
