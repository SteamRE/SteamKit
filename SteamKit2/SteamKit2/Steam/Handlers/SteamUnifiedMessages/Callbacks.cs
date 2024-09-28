/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using ProtoBuf;

namespace SteamKit2
{
    public partial class SteamUnifiedMessages
    {
        /// <summary>
        /// This callback is returned in response to a service method sent through <see cref="SteamUnifiedMessages"/>.
        /// </summary>
        public class ServiceMethodResponse<TResult> : CallbackMsg where TResult : IExtensible, new()
        {
            /// <summary>
            /// Gets the result of the message.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the protobuf body.
            /// </summary>
            public TResult Body { get; private set; }

            internal ServiceMethodResponse( PacketClientMsgProtobuf packetMsg )
            {
                var protoHeader = packetMsg.Header.Proto;
                JobID = protoHeader.jobid_target;
                Result = ( EResult )protoHeader.eresult;
                Body = new ClientMsgProtobuf<TResult>( packetMsg ).Body;
            }
        }
    }
}
