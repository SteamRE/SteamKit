/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class UFSClient
    {
        /// <summary>
        /// This callback is received after attempting to connect to the UFS server.
        /// </summary>
        public sealed class ConnectedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the connection attempt.
            /// </summary>
            /// <value>The result.</value>
            public EResult Result { get; private set; }


            internal ConnectedCallback( MsgChannelEncryptResult result )
                : this( result.Result )
            {
            }

            internal ConnectedCallback( EResult result )
            {
                this.Result = result;
            }
        }

        /// <summary>
        /// This callback is received when the client is physically disconnected from the UFS server.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
        }

        /// <summary>
        /// This callback is returned in response to an attempt to log on to the UFS server through <see cref="UFSClient"/>.
        /// </summary>
        public sealed class LoggedOnCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the logon
            /// </summary>
            public EResult Result { get; private set; }


            internal LoggedOnCallback( CMsgClientUFSLoginResponse body )
            {
                Result = ( EResult )body.eresult;
            }
        }
    }
}
