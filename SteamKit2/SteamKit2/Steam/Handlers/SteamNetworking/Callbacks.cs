using SteamKit2.Internal;

namespace SteamKit2
{
    public sealed partial class SteamNetworking
    {
        /// <summary>
        /// This callback is received in response to calling <see cref="RequestNetworkingCertificate"/>. This can be used to populate a CMsgSteamDatagramCertificateSigned for socket communication.
        /// </summary>
		public sealed class NetworkingCertificateCallback : CallbackMsg
        {
            /// <summary>
            /// The certificate signed by the Steam CA. This contains a CMsgSteamDatagramCertificate with the supplied public key.
            /// </summary>
            public byte[] Certificate { get; private set; }

            /// <summary>
            /// the ID of the CA used to sign this certificate.
            /// </summary>
            public ulong CAKeyID { get; private set; }

            /// <summary>
            /// The signature used to verify <see cref="Certificate"/>.
            /// </summary>
            public byte[] CASignature { get; private set; }

            internal NetworkingCertificateCallback( IPacketMsg packetMsg )
            {
                var resp = new ClientMsgProtobuf<CMsgClientNetworkingCertReply>( packetMsg );
                var msg = resp.Body;

                JobID = resp.TargetJobID;

                Certificate = msg.cert;
                CAKeyID = msg.ca_key_id;
                CASignature = msg.ca_signature;
            }
        }
    }
}
