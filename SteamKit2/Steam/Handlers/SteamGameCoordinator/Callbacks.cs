using System;
using SteamKit2.GC;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamGameCoordinator
    {
        /// <summary>
        /// This callback is fired when a game coordinator message is recieved from the network.
        /// </summary>
        public class MessageCallback : CallbackMsg
        {
            // raw emsg (with protobuf flag, if present)
            uint eMsg;

            /// <summary>
            /// Gets the game coordinator message type.
            /// </summary>
            public uint EMsg { get { return MsgUtil.GetGCMsg( eMsg ); } }
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
            public IPacketGCMsg Message { get; private set; }


            internal MessageCallback( CMsgGCClient gcMsg )
            {
                this.eMsg = gcMsg.msgtype;
                this.AppID = gcMsg.appid;
                this.Message = GetPacketGCMsg( gcMsg.msgtype, gcMsg.payload );
            }


            static IPacketGCMsg GetPacketGCMsg( uint eMsg, byte[] data )
            {
                // strip off the protobuf flag
                uint realEMsg = MsgUtil.GetGCMsg( eMsg );

                if ( MsgUtil.IsProtoBuf( eMsg ) )
                {
                    return new PacketClientGCMsgProtobuf( realEMsg, data );
                }
                else
                {
                    return new PacketClientGCMsg( realEMsg, data );
                }
            }
        }
    }
}
