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
using log4net;

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

            // generate a session key for this connection
            byte[] sessionKey = CryptoHelper.GenerateRandomBlock( 32 );
            byte[] pubKey = KeyDictionary.GetPublicKey( eUniv );

            if ( pubKey == null )
            {
                Log.ErrorFormat( "Unable to get public key for universe: {0}", eUniv );

                Client.Disconnect();
                return;
            }

            byte[] cryptedSessionKey = null;

            using ( var rsa = CryptoHelper.CreateRSA( pubKey ) )
            {
                // the key is encrypted with the universe's public key
                cryptedSessionKey = rsa.Encrypt( sessionKey );
            }

            byte[] keyCrc = CryptoHelper.CRCHash( cryptedSessionKey );


            var response = new Msg<MsgChannelEncryptResponse>( msg );

            response.Write( cryptedSessionKey );
            response.Write( keyCrc );
            response.Write( ( uint )0 );


            // send off our response and wait for the result
            var resultPacket = await YieldingSendMsgAndWaitForMsg( response, EMsg.ChannelEncryptResult );

            if ( resultPacket == null )
            {
                Client.Disconnect();
                return;
            }

            var result = new Msg<MsgChannelEncryptResult>( resultPacket );

            Log.InfoFormat( "ChannelEncryptResult: {0}", result.Body.Result );

            if ( result.Body.Result == EResult.OK )
            {
                Client.ConnectedUniverse = eUniv;
                Client.Connection.NetFilter = new NetFilterEncryption( sessionKey );
            }

            Client.PostCallback( new SteamClient.ConnectedCallback( result.Body, eUniv ) );
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
            if ( !clientMsg.IsProto )
            {
                Log.Error( "Got non-proto MsgMulti!" );
                return;
            }

            var msg = new ClientMsgProtobuf<CMsgMulti>( clientMsg );

            byte[] payload = msg.Body.message_body;

            if ( msg.Body.size_unzipped > 0 )
            {
                // unzip our payload
                try
                {
                    payload = ZipUtil.Decompress( payload );
                }
                catch ( Exception ex )
                {
                    Log.Error( "Unable to decompress MsgMulti payload.", ex );
                    return;
                }
            }

            using ( MemoryStream ms = new MemoryStream( payload ) )
            using ( BinaryReader br = new BinaryReader( ms ) )
            {
                while ( ms.Position != ms.Length )
                {
                    int subSize = br.ReadInt32();
                    byte[] subData = br.ReadBytes( subSize );

                    Client.OnMulti( subData );
                }
            }

        }
    }
}
