/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using ProtoBuf;
using System.IO;
using System.Linq;
using SteamKit2.Internal;

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
            public string ServiceName { get; private set; }

            /// <summary>
            /// Gets the name of the RPC method.
            /// </summary>
            public string RpcName { get; private set; }

            /// <summary>
            /// Gets the full name of the service method. This takes the form ServiceName.RpcName.
            /// </summary>
            public string MethodName
            {
                get { return ServiceName + "." + RpcName; }
            }


            internal ServiceMethodResponse( EResult result, CMsgClientServiceMethodResponse resp )
            {
                Result = result;
                ResponseRaw = resp.serialized_method_response;

                if ( resp.method_name != null )
                {
                    var methodParts = resp.method_name.Split( '.' );

                    ServiceName = methodParts.FirstOrDefault();
                    RpcName = string.Join( ".", methodParts.Skip( 1 ) );
                }
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
    }
}