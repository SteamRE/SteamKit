using System;
using System.Net;
using System.Threading.Tasks;

namespace SteamKit2
{
    class WebSocketConnection : Connection
    {
        public override EndPoint CurrentEndPoint => throw new NotImplementedException();

        public override void Connect(Task<EndPoint> endPointTask, int timeout = 5000)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override IPAddress GetLocalIP()
        {
            throw new NotImplementedException();
        }

        public override void Send(IClientMsg clientMsg)
        {
            throw new NotImplementedException();
        }

        public override void SetNetEncryptionFilter(INetFilterEncryption filter)
        {
            throw new NotImplementedException();
        }
    }
}
