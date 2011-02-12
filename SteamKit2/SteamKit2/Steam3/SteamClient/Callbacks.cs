/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public partial class SteamClient
    {
        /// <summary>
        /// This callback is received after attempting to connect to the Steam network.
        /// </summary>
        public sealed class ConnectCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }


            internal ConnectCallback( MsgChannelEncryptResult result )
                : this( result.Result )
            {
            }

            internal ConnectCallback( EResult result )
            {
                this.Result = result;
            }
        }
    }
}