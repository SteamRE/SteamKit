/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ProtoBuf;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with Steamworks unified messaging
    /// </summary>
    public partial class SteamUnifiedMessages : ClientMsgHandler
    {
        private readonly List<UnifiedService> _handlers = [ ];

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService CreateService<TService>() where TService : UnifiedService, new()
        {
            var service = new TService
            {
                UnifiedMessages = this
            };

            _handlers.Add( service);
            return service;
        }

        /// <summary>
        /// Sends a message.
        /// Results are returned in a <see cref="ServiceMsg{TResult}"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <typeparam name="TRequest">The type of a protobuf object.</typeparam>
        /// <param name="name">Name of the RPC endpoint. Takes the format ServiceName.RpcName</param>
        /// <param name="message">The message to send.</param>
        /// <returns>The JobID of the request. This can be used to find the appropriate <see cref="ServiceMsg{TResult}"/>.</returns>
        public AsyncJob<ServiceMsg<TResult>> SendMessage<TRequest, TResult>( string name, TRequest message )
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

            return new AsyncJob<ServiceMsg<TResult>>( Client, msg.SourceJobID );
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
            foreach ( var service in _handlers )
            {
                service.HandleMsg( packetMsg );
            }
        }

        internal void HandleServiceMsg<TService>( IPacketMsg packetMsg )
            where TService : IExtensible, new()
        {
            var callback = new ServiceMsg<TService>( (packetMsg as PacketClientMsgProtobuf)! );
            Client.PostCallback( callback );
        }

        internal static bool CanHandleMsg( IPacketMsg packetMsg, string service, [NotNullWhen(true)] out string? methodName )
        {
            if (packetMsg is not PacketClientMsgProtobuf packetMsgProto)
            {
                methodName = null;
                return false;
            }

            var jobNameStr = packetMsgProto.Header.Proto.target_job_name;
            if ( string.IsNullOrEmpty( jobNameStr ) )
            {
                methodName = null;
                return false;
            }

            // format: Service.Method#Version
            var jobName = jobNameStr.AsSpan();
            var dot = jobName.IndexOf( '.' );
            var hash = jobName.LastIndexOf( '#' );
            if ( dot < 0 || hash < 0 )
            {
                methodName = null;
                return false;
            }

            var serviceName = jobName[ ..dot ].ToString();
            if (serviceName != service)
            {
                methodName = null;
                return false;
            }

            methodName = jobName[ ( dot + 1 )..hash ].ToString();
            return true;
        }

        public abstract class UnifiedService : IDisposable
        {
            internal abstract void HandleMsg( IPacketMsg packetMsg );

            internal SteamUnifiedMessages UnifiedMessages { get; set; }

            /// <inheritdoc />
            public void Dispose()
            {
                UnifiedMessages._handlers.Remove( this );
            }
        }
    }
}
