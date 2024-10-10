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
            /// The result of the message.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// The protobuf body.
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

        /// <summary>
        /// This callback represents a service notification received though <see cref="SteamUnifiedMessages"/>.
        /// </summary>
        public class ServiceMethodNotification : CallbackMsg
        {
            private readonly PacketClientMsgProtobuf _packetMsg;

            /// <summary>
            /// The name of the Service.
            /// </summary>
            public string ServiceName { get; }

            /// <summary>
            /// The name of the RPC method.
            /// </summary>
            public string MethodName { get; }

            /// <summary>
            /// The version of the <see cref="SteamUnifiedMessages"/> notification.
            /// </summary>
            public int Version { get; }

            /// <summary>
            /// Deserializes the response into a protobuf object.
            /// </summary>
            /// <typeparam name="T">Protobuf type of the response message.</typeparam>
            /// <returns>The response to a <see cref="SteamUnifiedMessages"/> notification.</returns>
            public T GetDeserializedResponse<T>() where T : IExtensible, new()
            {
                var msg = new ClientMsgProtobuf<T>( _packetMsg );
                return msg.Body;
            }

            internal ServiceMethodNotification( PacketClientMsgProtobuf packetMsg, string serviceName, string methodName, int version )
            {
                _packetMsg = packetMsg;
                ServiceName = serviceName;
                MethodName = methodName;
                Version = version;
            }
        }
    }
}
