/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SteamKit3
{
    [Job( EMsg.ChannelEncryptRequest, JobType.ClientJob )]
    class ChannelHandshakeJob : ClientJob
    {
        public ChannelHandshakeJob( SteamClient client )
            : base( client )
        {
        }

        protected async override Task YieldingRunJobFromMsg( IPacketMsg clientMsg )
        {
            var msg = new Msg<MsgChannelEncryptRequest>( clientMsg );

            EUniverse eUniv = msg.Body.Universe;

            Client.ConnectedUniverse = eUniv;
            
            byte[] sessionKey = CryptoHelper.GenerateRandomBlock( 32 );
            byte[] pubKey = KeyDictionary.GetPublicKey( eUniv );

            Debug.Assert( pubKey != null, string.Format( "Unable to get public key for universe {0}", eUniv ) );

            CryptoHelper.InitializeRSA( pubKey );

            byte[] cryptedSessionKey = CryptoHelper.RSAEncrypt( sessionKey );
            byte[] keyCrc = CryptoHelper.CRCHash( cryptedSessionKey );

            var reply = new Msg<MsgChannelEncryptResponse>( msg );

            reply.Write( cryptedSessionKey );
            reply.Write( keyCrc );
            reply.Write( ( uint )0 );

            SendMessage( reply );

            IPacketMsg resultPacket = await YieldingWaitForMsg( EMsg.ChannelEncryptResult );
            var result = new Msg<MsgChannelEncryptResult>( resultPacket );

            if ( result.Body.Result == EResult.OK )
            {
                Client.Connection.NetFilter = new NetFilterEncryption( sessionKey );
                Client.PostCallback( new SteamClient.ConnectedCallback( result.Body ) );
            }
        }
    }

    [Job( EMsg.Multi, JobType.ClientJob ) ]
    class MultiplexMultiJob : ClientJob
    {
        public MultiplexMultiJob( SteamClient client )
            : base( client )
        {
        }

        protected async override Task YieldingRunJobFromMsg( IPacketMsg clientMsg )
        {
            Debug.Assert( clientMsg.IsProto, "Got non-proto MsgMulti!" );

            var msg = new ClientMsgProtobuf<CMsgMulti>( clientMsg );

            byte[] payload = msg.Body.message_body;

            if ( msg.Body.size_unzipped > 0 )
            {
                // unzip our payload
                payload = ZipUtil.Decompress( payload );
            }

            using ( MemoryStream ms = new MemoryStream( payload ) )
            using ( BinaryReader br = new BinaryReader( ms ) )
            {
                int subSize = br.ReadInt32();
                byte[] subData = br.ReadBytes( subSize );

                Client.OnMulti( subData );
            }
        }
    }
}
