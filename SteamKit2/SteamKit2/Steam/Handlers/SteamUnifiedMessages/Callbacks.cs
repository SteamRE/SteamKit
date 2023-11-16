﻿/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Reflection;
using ProtoBuf;

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
            /// Gets the name of the Service.
            /// </summary>
            public string ServiceName
            {
                get { return MethodName.Split( '.' )[ 0 ]; }
            }

            /// <summary>
            /// Gets the name of the RPC method.
            /// </summary>
            public string RpcName
            {
                get { return MethodName[ ( ServiceName.Length + 1 ).. ].Split( '#' )[ 0 ]; }
            }

            /// <summary>
            /// Gets the full name of the service method.
            /// </summary>
            public string MethodName { get; private set; }

            private PacketClientMsgProtobuf PacketMsg;

            internal ServiceMethodResponse( PacketClientMsgProtobuf packetMsg )
            {
                var protoHeader = packetMsg.Header.Proto;
                JobID = protoHeader.jobid_target;
                Result = ( EResult )protoHeader.eresult;
                MethodName = protoHeader.target_job_name;
                PacketMsg = packetMsg;
            }


            /// <summary>
            /// Deserializes the response into a protobuf object.
            /// </summary>
            /// <typeparam name="T">Protobuf type of the response message.</typeparam>
            /// <returns>The response to the message sent through <see cref="SteamUnifiedMessages"/>.</returns>
            public T GetDeserializedResponse<T>()
                where T : IExtensible, new()
            {
                var msg = new ClientMsgProtobuf<T>( PacketMsg );
                return msg.Body;
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
                get { return MethodName.Split( '.' )[ 0 ]; }
            }

            /// <summary>
            /// Gets the name of the RPC method.
            /// </summary>
            public string RpcName
            {
                get { return MethodName[ ( ServiceName.Length + 1 ).. ].Split( '#' )[ 0 ]; }
            }

            /// <summary>
            /// Gets the full name of the service method.
            /// </summary>
            public string MethodName { get; private set; }

            /// <summary>
            /// Gets the protobuf notification body.
            /// </summary>
            public object Body { get; private set; }


            internal ServiceMethodNotification( Type messageType, IPacketMsg packetMsg )
            {
                // Bounce into generic-land.
                var setupMethod = GetType().GetMethod( nameof(Setup), BindingFlags.Static | BindingFlags.NonPublic )!.MakeGenericMethod( messageType );
                (MethodName, Body) = ((string, object))setupMethod.Invoke( this, new[] { packetMsg } )!;
            }

            static (string methodName, object body) Setup<T>( IPacketMsg packetMsg )
                where T : IExtensible, new()
            {
                var clientMsg = new ClientMsgProtobuf<T>( packetMsg );
                return (clientMsg.Header.Proto.target_job_name, clientMsg.Body);
            }
        }
    }
}
