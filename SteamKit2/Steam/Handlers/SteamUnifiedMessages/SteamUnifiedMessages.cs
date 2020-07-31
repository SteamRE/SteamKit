/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using ProtoBuf;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with Steamworks unified messaging
    /// </summary>
    public partial class SteamUnifiedMessages : ClientMsgHandler
    {
        /// <summary>
        /// This wrapper is used for expression-based RPC calls using Steam Unified Messaging.
        /// </summary>
        public class UnifiedService<TService>
        {
            internal UnifiedService( SteamUnifiedMessages steamUnifiedMessages )
            {
                this.steamUnifiedMessages = steamUnifiedMessages;
            }

            readonly SteamUnifiedMessages steamUnifiedMessages;

            /// <summary>
            /// Sends a message.
            /// Results are returned in a <see cref="ServiceMethodResponse"/>.
            /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
            /// </summary>
            /// <typeparam name="TResponse">The type of the protobuf object which is the response to the RPC call.</typeparam>
            /// <param name="expr">RPC call expression, e.g. x => x.SomeMethodCall(message);</param>
            /// <param name="isNotification">Whether this message is a notification or not.</param>
            /// <returns>The JobID of the request. This can be used to find the appropriate <see cref="ServiceMethodResponse"/>.</returns>
            public AsyncJob<ServiceMethodResponse> SendMessage<TResponse>( Expression<Func<TService, TResponse>> expr, bool isNotification = false )
            {
                if ( expr == null )
                {
                    throw new ArgumentNullException( nameof(expr) );
                }

                var call = ExtractMethodCallExpression( expr, nameof(expr) );
                var methodInfo = call.Method;

                var argument = call.Arguments.Single();
                object message;

                if ( argument.NodeType == ExpressionType.MemberAccess )
                {
                    var unary = Expression.Convert( argument, typeof(object) );
                    var lambda = Expression.Lambda<Func<object>>( unary );
                    var getter = lambda.Compile();
                    message = getter();
                }
                else
                {
                    throw new NotSupportedException( "Unknown Expression type" );
                }

                var serviceName = typeof(TService).Name.Substring( 1 ); // IServiceName - remove 'I'
                var methodName = methodInfo.Name;
                var version = 1;

                var rpcName = string.Format( "{0}.{1}#{2}", serviceName, methodName, version );

                var method = typeof(SteamUnifiedMessages).GetMethod( nameof(SteamUnifiedMessages.SendMessage) ).MakeGenericMethod( message.GetType() );
                var result = method.Invoke( this.steamUnifiedMessages, new[] { rpcName, message, isNotification } );
                return ( AsyncJob<ServiceMethodResponse> )result;
            }
            
            static MethodCallExpression ExtractMethodCallExpression<TResponse>( Expression<Func<TService, TResponse>> expression, string paramName )
            {
                switch ( expression.NodeType )
                {
                    // Older code/tests/whatever were compiled down to just a single MethodCallExpression.
                    case ExpressionType.Call:
                        return ( MethodCallExpression )expression.Body;

                    // Newer code/tests/whatever are now compiled by wrapping the MethodCallExpression in a LambdaExpression.
                    case ExpressionType.Lambda:
                        if ( expression.Body.NodeType == ExpressionType.Call )
                        {
                            var lambda = ( LambdaExpression )expression;
                            return ( MethodCallExpression )lambda.Body;
                        }
                        break;
                }

                throw new ArgumentException( "Expression must be a method call.", paramName );
            }
        }


        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamUnifiedMessages()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientServiceMethodLegacyResponse, HandleClientServiceMethodResponse },
                { EMsg.ServiceMethod, HandleServiceMethod },
            };
        }

        /// <summary>
        /// Sends a message.
        /// Results are returned in a <see cref="ServiceMethodResponse"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <typeparam name="TRequest">The type of a protobuf object.</typeparam>
        /// <param name="name">Name of the RPC endpoint. Takes the format ServiceName.RpcName</param>
        /// <param name="message">The message to send.</param>
        /// <param name="isNotification">Whether this message is a notification or not.</param>
        /// <returns>The JobID of the request. This can be used to find the appropriate <see cref="ServiceMethodResponse"/>.</returns>
        public AsyncJob<ServiceMethodResponse> SendMessage<TRequest>( string name, TRequest message, bool isNotification = false )
            where TRequest : IExtensible
        {
            if ( message == null )
            {
                throw new ArgumentNullException( nameof(message) );
            }

            var msg = new ClientMsgProtobuf<CMsgClientServiceMethodLegacy>( EMsg.ClientServiceMethodLegacy );
            msg.SourceJobID = Client.GetNextJobID();

            using ( var ms = new MemoryStream() )
            {
                Serializer.Serialize( ms, message );
                msg.Body.serialized_method = ms.ToArray();
            }

            msg.Body.method_name = name;
            msg.Body.is_notification = isNotification;

            Client.Send( msg );

            return new AsyncJob<ServiceMethodResponse>( this.Client, msg.SourceJobID );
        }

        /// <summary>
        /// Creates a <see cref="UnifiedService&lt;TService&gt;"/> wrapper for expression-based unified messaging.
        /// </summary>
        /// <typeparam name="TService">The type of a service interface.</typeparam>
        /// <returns>The <see cref="UnifiedService&lt;TService&gt;"/> wrapper.</returns>
        public UnifiedService<TService> CreateService<TService>()
        {
            return new UnifiedService<TService>( this );
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof(packetMsg) );
            }

            bool haveFunc = dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc );

            if ( !haveFunc )
            {
                // ignore messages that we don't have a handler function for
                return;
            }

            handlerFunc( packetMsg );
        }


        #region ClientMsg Handlers
        void HandleClientServiceMethodResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientServiceMethodLegacyResponse>( packetMsg );
            var callback = new ServiceMethodResponse(response.TargetJobID, (EResult)response.ProtoHeader.eresult, response.Body);
            Client.PostCallback( callback );
        }

        void HandleServiceMethod( IPacketMsg packetMsg )
        {
            var notification = new ClientMsgProtobuf( packetMsg );

            var jobName = notification.Header.Proto.target_job_name;
            if ( !string.IsNullOrEmpty( jobName ) )
            {
                var splitByDot = jobName.Split( '.' );
                var splitByHash = splitByDot[1].Split( '#' );

                var serviceName = splitByDot[0];
                var methodName = splitByHash[0];

                var serviceInterfaceName = "SteamKit2.Internal.I" + serviceName;
                var serviceInterfaceType = Type.GetType( serviceInterfaceName );
                if (serviceInterfaceType != null)
                {
                    var method = serviceInterfaceType.GetMethod( methodName );
                    if ( method != null )
                    {
                        var argumentType = method.GetParameters().Single().ParameterType;

                        var callback = new ServiceMethodNotification( argumentType, packetMsg );
                        Client.PostCallback( callback );
                    }
                }
            }
        }
        #endregion
    }
}
