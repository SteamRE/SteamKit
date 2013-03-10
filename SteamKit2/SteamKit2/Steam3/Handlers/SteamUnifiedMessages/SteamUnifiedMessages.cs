/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using ProtoBuf;
using SteamKit2.Internal;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for interacting with Steamworks unified messaging
    /// </summary>
    public partial class SteamUnifiedMessages : ClientMsgHandler
    {

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <typeparam name="T">The type of a protobuf object.</typeparam>
        /// <param name="name">Name of the RPC endpoint. Takes the format ServiceName.RpcName</param>
        /// <param name="message">The message to send.</param>
        /// <param name="isNotification">Whether this message is a notification or not.</param>
        /// <returns>The JobID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID SendMessage<T>( string name, T message, bool isNotification = false )
            where T : IExtensible
        {
            var msg = new ClientMsgProtobuf<CMsgClientServiceMethod>( EMsg.ClientServiceMethod );
            msg.SourceJobID = Client.GetNextJobID();

            using ( var ms = new MemoryStream() )
            {
                Serializer.Serialize( ms, message );
                msg.Body.serialized_method = ms.ToArray();
            }

            msg.Body.method_name = name;
            msg.Body.is_notification = isNotification;

            Client.Send( msg );

            return msg.SourceJobID;
        }


        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            switch ( packetMsg.MsgType )
            {
                case EMsg.ClientServiceMethodResponse:
                    HandleClientServiceMethodResponse( packetMsg );
                    break;
            }
        }

        void HandleClientServiceMethodResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientServiceMethodResponse>( packetMsg );

#if STATIC_CALLBACKS
            var responseCallback = new ServiceMethodResponse( Client, ( EResult )response.ProtoHeader.eresult, response.Body );
            var jobCallback = new SteamClient.JobCallback<ServiceMethodResponse>( Client, response.TargetJobID, responseCallback );

            SteamClient.PostCallback( jobCallback );
#else
            var responseCallback = new ServiceMethodResponse( ( EResult )response.ProtoHeader.eresult, response.Body );
            var jobCallback = new SteamClient.JobCallback<ServiceMethodResponse>( response.TargetJobID, responseCallback );

            Client.PostCallback( jobCallback );
#endif

        }

    }
}