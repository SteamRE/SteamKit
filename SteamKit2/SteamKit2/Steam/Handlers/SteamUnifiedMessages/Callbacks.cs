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
        public class ServiceMethodResponse<T> : CallbackMsg where T : IExtensible, new()
        {
            /// <summary>
            /// The result of the message.
            /// </summary>
            public EResult Result { get; }

            /// <summary>
            /// The protobuf body.
            /// </summary>
            public T Body { get; }

            internal ServiceMethodResponse( PacketClientMsgProtobuf packetMsg )
            {
                var protoHeader = packetMsg.Header.Proto;
                JobID = protoHeader.jobid_target;
                Result = ( EResult )protoHeader.eresult;
                Body = new ClientMsgProtobuf<T>( packetMsg ).Body;
            }
        }

        /// <summary>
        /// This callback represents a service notification received though <see cref="SteamUnifiedMessages"/>.
        /// </summary>
        public class ServiceMethodNotification<T> : CallbackMsg where T : IExtensible, new()
        {
            /// <summary>
            /// The name of the job, in the format Service.Method#Version.
            /// </summary>
            public string JobName { get; }

            /// <summary>
            /// The protobuf body.
            /// </summary>
            public T Body { get; }

            internal ServiceMethodNotification( PacketClientMsgProtobuf packetMsg)
            {
                JobID = JobID.Invalid;
                JobName = packetMsg.Header.Proto.target_job_name;
                Body = new ClientMsgProtobuf<T>( packetMsg ).Body;
            }
        }
    }
}
