using System;
using System.Net;
using System.IO;

namespace SteamKit
{
    class NetworkEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; private set; }

        public NetworkEventArgs(IPEndPoint sender)
        {
            this.Sender = sender;
        }
    }

    class ChallengeEventArgs : NetworkEventArgs
    {
        public ChallengeData Data { get; private set; }

        public ChallengeEventArgs(IPEndPoint sender, ChallengeData reply)
            : base(sender)
        {
            this.Data = reply;
        }
    }

    class DataEventArgs : NetworkEventArgs
    {
        public MemoryStream Data { get; private set; }

        public DataEventArgs(IPEndPoint sender, MemoryStream data)
            : base(sender)
        {
            this.Data = data;
        }
    }
}
