using System;
using System.Net;

namespace SteamKit
{
    abstract class Connection
    {
        public event EventHandler<ChallengeEventArgs> ChallengeReceived;
        public event EventHandler<NetworkEventArgs> AcceptReceived;
        public event EventHandler<DataEventArgs> NetMsgReceived;
        public event EventHandler<DataEventArgs> DatagramReceived;
        public event EventHandler<NetworkEventArgs> DisconnectReceived;

        protected NetFilterEncryption netFilter;

        public abstract void SetTargetEndPoint(IPEndPoint remoteEndPoint);
        public abstract void SendConnect(UInt32 challengeValue);
        public abstract void SendMessage(IClientMsg clientmsg);
        public abstract IPAddress GetLocalIP();

        public void SetNetFilter(NetFilterEncryption filter)
        {
            netFilter = filter;
        }

        protected void OnChallengeReceived(ChallengeEventArgs e)
        {
            if(ChallengeReceived != null)
                ChallengeReceived(this, e);
        }

        protected void OnAcceptReceived(NetworkEventArgs e)
        {
            if (AcceptReceived != null)
                AcceptReceived(this, e);
        }

        protected void OnNetMsgReceived(DataEventArgs e)
        {
            if (NetMsgReceived != null)
                NetMsgReceived(this, e);
        }

        protected void OnDatagramReceived(DataEventArgs e)
        {
            if (DatagramReceived != null)
                DatagramReceived(this, e);
        }

        protected void OnDisconnectReceived(NetworkEventArgs e)
        {
            if (DisconnectReceived != null)
                DisconnectReceived(this, e);
        }
    }
}
