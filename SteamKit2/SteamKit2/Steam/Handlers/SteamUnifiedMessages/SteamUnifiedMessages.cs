/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Concurrent;
using ProtoBuf;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with Steamworks unified messaging.
    /// </summary>
    public partial class SteamUnifiedMessages : ClientMsgHandler
    {
        private readonly ConcurrentDictionary<string, UnifiedService> _handlers = [];

        /// <summary>
        /// Creates a service that can be used to send messages and receive notifications via Steamworks unified messaging.
        /// </summary>
        /// <typeparam name="TService">The type of the service to create.</typeparam>
        /// <returns>The instance to create requests from.</returns>
        public TService CreateService<TService>() where TService : UnifiedService, new()
        {
            var service = new TService
            {
                UnifiedMessages = this
            };

            return ( _handlers.GetOrAdd( service.ServiceName, service ) as TService )!;
        }

        /// <summary>
        /// Removes a service so it no longer can be used to send messages or receive notifications.
        /// </summary>
        /// <typeparam name="TService">The type of the service to remove.</typeparam>
        public void RemoveService<TService>() where TService : UnifiedService, new()
        {
            var serviceName = new TService().ServiceName;
            _handlers.TryRemove( serviceName, out _ );
        }

        /// <summary>
        /// Sends a message.
        /// Results are returned in a <see cref="ServiceMethodResponse{TResult}"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <typeparam name="TRequest">The type of a protobuf object.</typeparam>
        /// <typeparam name="TResult">The type of the result of the request.</typeparam>
        /// <param name="name">Name of the RPC endpoint. Takes the format ServiceName.RpcName</param>
        /// <param name="message">The message to send.</param>
        /// <returns>The JobID of the request. This can be used to find the appropriate <see cref="ServiceMethodResponse{TResult}"/>.</returns>
        public AsyncJob<ServiceMethodResponse<TResult>> SendMessage<TRequest, TResult>( string name, TRequest message )
            where TRequest : IExtensible, new() where TResult : IExtensible, new()
        {
            if ( message is null )
            {
                throw new ArgumentNullException( nameof( message ) );
            }

            var eMsg = Client.SteamID is null ? EMsg.ServiceMethodCallFromClientNonAuthed : EMsg.ServiceMethodCallFromClient;
            var msg = new ClientMsgProtobuf<TRequest>( eMsg )
            {
                SourceJobID = Client.GetNextJobID()
            };

            msg.Header.Proto.target_job_name = name;
            msg.Body = message;
            Client.Send( msg );

            return new AsyncJob<ServiceMethodResponse<TResult>>( Client, msg.SourceJobID );
        }

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <typeparam name="TRequest">The type of a protobuf object.</typeparam>
        /// <param name="name">Name of the RPC endpoint. Takes the format ServiceName.RpcName</param>
        /// <param name="message">The message to send.</param>
        public void SendNotification<TRequest>( string name, TRequest message )
            where TRequest : IExtensible, new()
        {
            if ( message is null )
            {
                throw new ArgumentNullException( nameof( message ) );
            }

            // Notifications do not set source jobid, otherwise Steam server will actively reject this message
            // if the method being used is a "Notification"
            var eMsg = Client.SteamID is null ? EMsg.ServiceMethodCallFromClientNonAuthed : EMsg.ServiceMethodCallFromClient;
            var msg = new ClientMsgProtobuf<TRequest>( eMsg );
            msg.Header.Proto.target_job_name = name;
            msg.Body = message;
            Client.Send( msg );
        }

        /// <inheritdoc />
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg is not PacketClientMsgProtobuf { MsgType: EMsg.ServiceMethod or EMsg.ServiceMethodResponse } packetMsgProto )
                return;

            var jobName = packetMsgProto.Header.Proto.target_job_name.AsSpan();
            if ( jobName.IsEmpty )
                return;

            // format: Service.Method#Version
            var dot = jobName.IndexOf( '.' );
            var hash = jobName.LastIndexOf( '#' );
            if ( dot < 0 || hash < 0 )
                return;

            var serviceName = jobName[ ..dot ].ToString();

            if ( !_handlers.TryGetValue( serviceName, out var handler ) )
                return;

            var methodName = jobName[ ( dot + 1 )..hash ].ToString();

            switch ( packetMsgProto.MsgType )
            {
                case EMsg.ServiceMethodResponse:
                    handler.HandleResponseMsg( methodName, packetMsgProto );
                    break;
                case EMsg.ServiceMethod:
                    handler.HandleNotificationMsg( methodName, packetMsgProto );
                    break;
            }
        }

        internal void HandleResponseMsg<TResponse>( PacketClientMsgProtobuf packetMsg ) where TResponse : IExtensible, new()
        {
            var callback = new ServiceMethodResponse<TResponse>( packetMsg );
            Client.PostCallback( callback );
        }

        internal void HandleNotificationMsg<TNotification>( PacketClientMsgProtobuf packetMsg ) where TNotification : IExtensible, new()
        {
            var callback = new ServiceMethodNotification<TNotification>( packetMsg );
            Client.PostCallback( callback );
        }

        /// <summary>
        /// Abstract definition of a steam unified messages service.
        /// </summary>
        public abstract class UnifiedService
        {
            /// <summary>
            /// Handles a response message for this service. This should not be called directly.
            /// </summary>
            /// <param name="methodName">The name of the method the service should handle</param>
            /// <param name="packetMsg">The packet message that contains the data</param>
            public abstract void HandleResponseMsg( string methodName, PacketClientMsgProtobuf packetMsg );

            /// <summary>
            /// Handles a notification message for this service. This should not be called directly.
            /// </summary>
            /// <param name="methodName">The name of the method the service should handle</param>
            /// <param name="packetMsg">The packet message that contains the data</param>
            public abstract void HandleNotificationMsg( string methodName, PacketClientMsgProtobuf packetMsg );

            /// <summary>
            /// Dispatches the provided data as a service method response.
            /// </summary>
            /// <param name="packetMsg">The packet message that contains the data.</param>
            /// <typeparam name="TResponse">The type of the response.</typeparam>
            protected void PostResponseMsg<TResponse>( PacketClientMsgProtobuf packetMsg ) where TResponse : IExtensible, new()
            {
	            UnifiedMessages?.HandleResponseMsg<TResponse>( packetMsg );
            }

            /// <summary>
            /// Dispatches the provided data as a service method notification.
            /// </summary>
            /// <param name="packetMsg">The packet message that contains the data.</param>
            /// <typeparam name="TNotification">The type of the notification.</typeparam>
            protected void PostNotificationMsg<TNotification>( PacketClientMsgProtobuf packetMsg ) where TNotification : IExtensible, new()
            {
	            UnifiedMessages?.HandleNotificationMsg<TNotification>( packetMsg );
            }

            /// <summary>
            /// The name of the steam unified messages service.
            /// </summary>
            public abstract string ServiceName { get; }

            /// <summary>
            /// A reference to the <see cref="SteamUnifiedMessages"/> instance this service was created from.
            /// </summary>
            public SteamUnifiedMessages? UnifiedMessages { get; init; }
        }
    }
}
