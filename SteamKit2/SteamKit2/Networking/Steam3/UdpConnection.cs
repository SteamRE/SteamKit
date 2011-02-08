/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SteamKit2
{
    class UdpConnection : Connection
    {
        public UdpConnection()
        {
            throw new NotImplementedException();
        }

        public override void Connect( IPEndPoint endPoint )
        {
            throw new NotImplementedException();
        }
        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void Send( IClientMsg clientMsg )
        {
            throw new NotImplementedException();
        }
    }
}
