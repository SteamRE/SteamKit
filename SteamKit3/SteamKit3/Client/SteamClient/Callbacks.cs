/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit3
{
    partial class SteamClient
    {
        public sealed class DisconnectedCallback : CallbackMsg
        {
        }

        public sealed class ConnectedCallback : CallbackMsg
        {
            public EResult Result { get; private set; }

            internal ConnectedCallback( MsgChannelEncryptResult msg )
            {
                Result = msg.Result;
            }
        }
    }
}
