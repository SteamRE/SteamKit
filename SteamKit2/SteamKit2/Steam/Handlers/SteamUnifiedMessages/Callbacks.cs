/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using ProtoBuf;
using System.IO;
using System.Linq;
using SteamKit2.Internal;
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace SteamKit2
{
    public partial class SteamUnifiedMessages : ClientMsgHandler
    {
        /// <summary>
        /// This callback is returned in response to a service method sent through <see cref="SteamUnifiedMessages"/>.
        /// </summary>
        public class ServiceMethodResponse : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the message.
            /// </summary>
            public EResult Result { get; private set; }

            /// <summary>
            /// Gets the raw binary response.
            /// </summary>
            public byte[] ResponseRaw { get; private set; }

            /// <summary>
            /// Gets the name of the Service.
            /// </summary>
            public string ServiceName
            {
                get { return MethodName.Split( '.' )[0]; }
            }

            /// <summary>
            /// Gets the name of the RPC method.
            /// </summary>
            public string RpcName
            {
                get { return MethodName.Substring( ServiceName.Length + 1 ).Split( '#' )[0]; }
            }

            /// <summary>
            /// Gets the full name of the service method.
            /// </summary>
            public string MethodName { get; private set; }


            internal ServiceMethodResponse( JobID jobID, EResult result, CMsgClientServiceMethodLegacyResponse resp )
            {
                JobID = jobID;

                Result = result;
                ResponseRaw = resp.serialized_method_response;
                MethodName = resp.method_name ?? string.Empty;
            }


            /// <summary>
            /// Deserializes the response into a protobuf object.
            /// </summary>
            /// <typeparam name="T">Protobuf type of the response message.</typeparam>
            /// <returns>The response to the message sent through <see cref="SteamUnifiedMessages"/>.</returns>
            public T GetDeserializedResponse<T>()
                where T : IExtensible
            {
                using ( var ms = new MemoryStream( ResponseRaw ) )
                {
                    return Serializer.Deserialize<T>( ms );
                }
            }
        }

        /// <summary>
        /// This callback represents a service notification recieved though <see cref="SteamUnifiedMessages"/>.
        /// </summary>
        public class ServiceMethodNotification : CallbackMsg
        {
            /// <summary>
            /// Gets the name of the Service.
            /// </summary>
            public string ServiceName
            {
                get { return MethodName.Split( '.' )[0]; }
            }

            /// <summary>
            /// Gets the name of the RPC method.
            /// </summary>
            public string RpcName
            {
                get { return MethodName.Substring( ServiceName.Length + 1 ).Split( '#' )[0]; }
            }

            /// <summary>
            /// Gets the full name of the service method.
            /// </summary>
            [DisallowNull, NotNull]
            public string? MethodName { get; private set; }

            /// <summary>
            /// Gets the protobuf notification body.
            /// </summary>
            [DisallowNull, NotNull]
            public object? Body { get; private set; }


            internal ServiceMethodNotification( Type messageType, IPacketMsg packetMsg )
            {
                // Bounce into generic-land.
                var setupMethod = GetType().GetMethod( nameof(Setup), BindingFlags.Instance | BindingFlags.NonPublic ).MakeGenericMethod( messageType );
                setupMethod.Invoke( this, new[] { packetMsg } );
            }

            void Setup<T>( IPacketMsg packetMsg )
                where T : IExtensible, new()
            {
                var clientMsg = new ClientMsgProtobuf<T>( packetMsg );

                MethodName = clientMsg.Header.Proto.target_job_name;
                Body = clientMsg.Body;
            }
        }
    }
}
