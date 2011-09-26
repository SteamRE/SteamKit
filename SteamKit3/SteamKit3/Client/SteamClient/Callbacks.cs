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
        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
#if STATIC_CALLBACKS
            internal DisconnectedCallback( SteamClient client )
                : base( client )
            {
            }
#endif
        }

        /// <summary>
        /// This callback is received after attempting to connect to the Steam network.
        /// </summary>
        public sealed class ConnectedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the universe we've connected to.
            /// </summary>
            public EUniverse Universe { get; private set; }
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            public EResult Result { get; private set; }


#if STATIC_CALLBACKS
            internal ConnectedCallback( SteamClient client, MsgChannelEncryptResult msg, EUniverse eUniverse )
                : base( client )
#else
            internal ConnectedCallback( MsgChannelEncryptResult msg, EUniverse eUniverse )
#endif
            {
                Universe = eUniverse;
                Result = msg.Result;
            }
        }
    }
}
