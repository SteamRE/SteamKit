/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Net;

namespace SteamKit2
{
    /// <summary>
    /// Represents data that has been received over the network.
    /// </summary>
    class NetMsgEventArgs : EventArgs
    {
        public byte[] Data { get; }
        public EndPoint EndPoint { get; }

        public NetMsgEventArgs( byte[] data, EndPoint endPoint )
        {
            this.Data = data;
            this.EndPoint = endPoint;
        }

        public NetMsgEventArgs WithData( byte[] data )
            => new NetMsgEventArgs( data, EndPoint );
    }

}
