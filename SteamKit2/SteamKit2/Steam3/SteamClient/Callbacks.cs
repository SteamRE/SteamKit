using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public partial class SteamClient
    {
        public sealed class ConnectCallback : CallbackMsg
        {
            public EResult Result { get; set; }

            internal ConnectCallback( MsgChannelEncryptResult result )
            {
                this.Result = result.Result;
            }
        }
    }
}
