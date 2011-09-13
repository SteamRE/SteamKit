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
    [Handler]
    public sealed partial class SteamUser : ClientHandler
    {
        /// <summary>
        /// Represents the details required to log into Steam3.
        /// </summary>
        public class LogOnDetails
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>The username.</value>
            public string Username { get; set; }
            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>The password.</value>
            public string Password { get; set; }


            /// <summary>
            /// Gets or sets the Steam Guard auth code used to login. This is the code sent to the user's email.
            /// </summary>
            /// <value>The auth code.</value>
            public string AuthCode { get; set; }

            /// <summary>
            /// Gets or sets the account instance. 1 for the PC instance or 2 for the Console (PS3) instance.
            /// </summary>
            /// <value>The account instance.</value>
            public uint AccountInstance { get; set; }


            public LogOnDetails()
            {
                AccountInstance = 1; // use the default pc steam instance
            }
        }

        internal SteamUser()
        {
        }

        public void Logon( LogOnDetails logonDetails )
        {
            JobMgr.LaunchJob( new LogonJob( Client, logonDetails ) );
        }
    }
}
