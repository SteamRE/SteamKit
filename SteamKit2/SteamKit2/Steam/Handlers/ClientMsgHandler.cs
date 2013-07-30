/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


namespace SteamKit2
{
    /// <summary>
    /// This class implements the base requirements every message handler should inherit from.
    /// </summary>
    public abstract class ClientMsgHandler
    {

        /// <summary>
        /// Gets the underlying <see cref="SteamClient"/> for use in sending replies.
        /// </summary>
        protected SteamClient Client { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgHandler"/> class.
        /// </summary>
        public ClientMsgHandler()
        {
        }


        internal void Setup( SteamClient client )
        {
            this.Client = client;
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public abstract void HandleMsg( IPacketMsg packetMsg );
    }
}
