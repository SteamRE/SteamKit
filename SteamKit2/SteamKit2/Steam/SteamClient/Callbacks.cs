/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamClient
    {
        /// <summary>
        /// This callback is received after attempting to connect to the Steam network.
        /// </summary>
        public sealed class ConnectedCallback : CallbackMsg
        {
            internal ConnectedCallback()
            {
            }
        }


        /// <summary>
        /// This callback is received when the steamclient is physically disconnected from the Steam network.
        /// </summary>
        public sealed class DisconnectedCallback : CallbackMsg
        {
            /// <summary>
            /// If true, the disconnection was initiated by calling <see cref="CMClient.Disconnect()"/>.
            /// If false, the disconnection was the cause of something not user-controlled, such as a network failure or
            /// a forcible disconnection by the remote server.
            /// </summary>
            public bool UserInitiated { get; private set; }

            internal DisconnectedCallback( bool userInitiated )
            {
                this.UserInitiated = userInitiated;
            }
        }
    }
}
