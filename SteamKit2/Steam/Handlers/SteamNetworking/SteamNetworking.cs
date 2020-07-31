using SteamKit2.Internal;
using System;
using System.Collections.Generic;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used for Steam networking sockets
    /// </summary>
    public sealed partial class SteamNetworking : ClientMsgHandler
    {
        Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;

        internal SteamNetworking()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientNetworkingCertRequestResponse, HandleNetworkingCertRequestResponse },
            };
        }


        /// <summary>
        /// Request a signed networking certificate from Steam for your Ed25519 public key for the given app id.
        /// Results are returned in a <see cref="NetworkingCertificateCallback"/>.
        /// The returned <see cref="AsyncJob{T}"/> can also be awaited to retrieve the callback result.
        /// </summary>
        /// <param name="appId">The App ID the certificate will be generated for</param>
        /// <param name="publicKey">Your Ed25519 public key</param>
        /// <returns>The Job ID of the request. This can be used to find the appropriate <see cref="NetworkingCertificateCallback"/>.</returns>
        public AsyncJob<NetworkingCertificateCallback> RequestNetworkingCertificate( uint appId, byte[] publicKey )
        {
            if ( publicKey == null )
            {
                throw new ArgumentNullException( nameof( publicKey ) );
            }

            var msg = new ClientMsgProtobuf<CMsgClientNetworkingCertRequest>( EMsg.ClientNetworkingCertRequest );
            msg.SourceJobID = Client.GetNextJobID();

            msg.Body.app_id = appId;
            msg.Body.key_data = publicKey;

            Client.Send( msg );

            return new AsyncJob<NetworkingCertificateCallback>( this.Client, msg.SourceJobID );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof( packetMsg ) );
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
        void HandleNetworkingCertRequestResponse( IPacketMsg packetMsg )
        {
            var resp = new ClientMsgProtobuf<CMsgClientNetworkingCertReply>( packetMsg );

            var callback = new NetworkingCertificateCallback( resp.TargetJobID, resp.Body );
            Client.PostCallback( callback );
        }
        #endregion

    }
}
