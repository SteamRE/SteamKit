/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SteamKit3
{
    /// <summary>
    /// This handler handles all user log on/log off related actions and callbacks.
    /// </summary>
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
        }


        internal SteamUser()
        {
        }

        /// <summary>
        /// Logs the client into the Steam3 network. The client should already have been connected at this point.
        /// Results are returned in a <see cref="SteamUser.LoggedOnCallback"/>.
        /// </summary>
        /// <param name="logonDetails">The details.</param>
        public void Logon( LogOnDetails logonDetails )
        {
            Contract.Requires( logonDetails != null );
            Contract.Requires( !string.IsNullOrEmpty( logonDetails.Username ), "logon details must have a username." );
            Contract.Requires( !string.IsNullOrEmpty( logonDetails.Password ), "logon details must have a password." );

            JobMgr.LaunchJob( new LogonJob( Client, logonDetails ) );
        }

        /// <summary>
        /// Logs the currently logged on client off of the Steam3 network.
        /// Results are returned in a <see cref="SteamUser.LoggedOffCallback"/>.
        /// </summary>
        public void LogOff()
        {
            JobMgr.LaunchJob( new LogOffJob( Client ) );
        }
    }
}
