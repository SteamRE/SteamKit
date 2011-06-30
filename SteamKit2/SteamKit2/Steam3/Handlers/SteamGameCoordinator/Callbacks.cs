using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public partial class SteamGameCoordinator
    {
        /// <summary>
        /// This callback is fired when a game coordinator message is recieved from the network.
        /// </summary>
        public class MessageCallback : CallbackMsg
        {
            EGCMsg eMsg;
            /// <summary>
            /// Gets the game coordinator message type.
            /// </summary>
            public EGCMsg EMsg { get { return MsgUtil.GetGCMsg( eMsg ); } }
            /// <summary>
            /// Gets the AppID of the game coordinator the message is from.
            /// </summary>
            public uint AppID { get; private set; }
            /// <summary>
            /// Gets a value indicating whether this message is protobuf'd.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is protobuf'd; otherwise, <c>false</c>.
            /// </value>
            public bool IsProto { get { return MsgUtil.IsProtoBuf( eMsg ); } }

            /// <summary>
            /// Gets the actual message.
            /// </summary>
            public byte[] Payload { get; private set; }

            internal MessageCallback( MsgClientFromGC gcMsg, byte[] payload )
            {
                this.eMsg = ( EGCMsg )gcMsg.GCEMsg;
                this.AppID = gcMsg.AppId;

                this.Payload = payload;
            }
        }
    }
}
