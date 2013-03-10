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

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <typeparam name="T">A protobuf type</typeparam>
        /// <param name="name">Name of the RPC endpoint. Takes the format ServiceName.RpcName</param>
        /// <param name="message">The message to send</param>
        /// <param name="isNotification">Whether this message is a notification or not.</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="SteamClient.JobCallback&lt;T&gt;"/>.</returns>
        public JobID SendMessage<T>( string name, T message, bool isNotification = false)
        {
            var msg = new ClientMsgProtobuf<CMsgClientServiceMethod>( EMsg.ClientServiceMethod );
            msg.SourceJobID = Client.GetNextJobID();
            
            using ( var ms = new MemoryStream() )
            {
                Serializer.Serialize( ms, message );
                msg.Body.serialized_method = BuildStringRaw( ms.ToArray() );
            }

            msg.Body.method_name = name;
            msg.Body.is_notification = isNotification;

            Client.Send( msg );

            return msg.SourceJobID;
        }

        void HandleClientServiceMethodResponse( IPacketMsg packetMsg )
        {
            var response = new ClientMsgProtobuf<CMsgClientServiceMethodResponse>( packetMsg );

            var methodName = response.Body.method_name;
            var dataAsString = response.Body.serialized_method_response;

            var responseCallback = new ServiceMethodResponse( (EResult)response.ProtoHeader.eresult, methodName, BuildDataRaw( dataAsString ) );
            var jobCallback = new SteamClient.JobCallback<ServiceMethodResponse>( response.TargetJobID, responseCallback );

            Client.PostCallback( jobCallback );
        }

        static string BuildStringRaw( byte[] data )
        {
            var sb = new StringBuilder( data.Length );
            foreach ( var @byte in data )
            {
                sb.Append( (char)@byte );
            }
            return sb.ToString();
        }

        static byte[] BuildDataRaw( string dataAsString )
        {
            return dataAsString.Select( x => (byte)x ).ToArray();
        }
    }
}
