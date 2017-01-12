/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;

namespace SteamKit2
{
    /// <summary>
    /// This class implements the base requirements every message handler should inherit from.
    /// Provides simple message dispatching logic based on dispatch map.
    /// </summary>
    public abstract class ClientMsgMappingHandler : ClientMsgHandler
    {
        /// <summary>
        /// Stores action mapping for different message types.
        /// </summary>
        protected abstract Dictionary<EMsg, Action<IPacketMsg>> DispatchMap { get; }
        
        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public sealed override void HandleMsg( IPacketMsg packetMsg )
        {
            Action<IPacketMsg> handlerFunc;
            bool haveFunc = DispatchMap.TryGetValue( packetMsg.MsgType, out handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }
    }
}
