using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SteamKit2
{
    public class AuthServerClient : ServerClient
    {
        uint internalIp, externalIp;
        uint salt1, salt2;

        byte[] aesKey, iv;

        public AuthServerClient()
        {
        }

        public bool Login( string user, string pass, out ClientTGT clientTgt, out byte[] serverTgt, out Blob accRecord )
        {
            clientTgt = null;
            serverTgt = null;
            accRecord = null;

            if ( !this.RequestIP( user ) )
                return false;

            if ( !this.GetSalt( user ) )
                return false;

            if ( !this.DoLogin( pass ) )
                return false;

            if ( !this.GetAccountInfo( out clientTgt, out serverTgt, out accRecord ) )
                return false;

            return true;

        }

        bool RequestIP( string user )
        {
            try
            {
                uint userHash = BitConverter.ToUInt32( CryptoHelper.JenkinsHash( Encoding.ASCII.GetBytes( user ) ), 0 );

                ByteBuffer bb = new ByteBuffer( true );

                bb.Append<uint>( 0 );
                bb.Append<byte>( 4 );
                bb.Append( internalIp );
                bb.Append( userHash );

                Socket.Send( bb.ToArray() );

                byte result = Socket.Reader.ReadByte();

                if ( result != 0 )
                {
#if DEBUG
                    Trace.WriteLine( string.Format( "AuthServerClient RequestIP failed. Result: {0}", result ), "Steam2" );
#endif

                    Socket.Disconnect();
                    return false;
                }

                externalIp = NetHelpers.EndianSwap( Socket.Reader.ReadUInt32() );
            }
            catch ( Exception ex )
            {
#if DEBUG
                Trace.WriteLine( string.Format( "AuthServerClient RequestIP threw an exception.\n{0}", ex.ToString() ), "Steam2" );
#endif

                Socket.Disconnect();
                return false;
            }

            return true;
        }
        bool GetSalt( string user )
        {
            ushort userLen = ( ushort )user.Length;

            if ( !this.SendCommand( 2, userLen, user, userLen, user ) )
            {
#if DEBUG
                Trace.WriteLine( "AuthServerClient GetSalt failed.", "Steam2" );
#endif

                Socket.Disconnect();
                return false;
            }

            try
            {
                salt1 = Socket.Reader.ReadUInt32();
                salt2 = Socket.Reader.ReadUInt32();
            }
            catch ( Exception ex )
            {
#if DEBUG
                Trace.WriteLine( string.Format( "AuthServerClient GetSalt threw an exception.\n{0}", ex.ToString() ), "Steam2" );
#endif

                Socket.Disconnect();
                return false;
            }

            return true;
        }
        bool DoLogin( string pass )
        {
            try
            {
                aesKey = AuthServerClient.GenerateAESKey( salt1, salt2, pass );
                iv = CryptoHelper.GenerateRandomBlock( 16 );

                MicroTime mt = MicroTime.Now;
                mt = mt ^ AuthServerClient.GetObfuscationMask( internalIp, externalIp );

                byte[] plainText = AuthServerClient.GetPlaintext( mt, internalIp );
                byte[] cipherText = CryptoHelper.AESEncrypt( plainText, aesKey, iv );


                TcpPacket packet = new TcpPacket();

                packet.Append( iv );
                packet.Append( ( ushort )plainText.Length );
                packet.Append( ( ushort )cipherText.Length );
                packet.Append( cipherText );

                Socket.Send( packet );
            }
            catch ( Exception ex )
            {
#if DEBUG
                Trace.WriteLine( string.Format( "AuthServerClient DoLogin threw an exception.\n{0}", ex.ToString() ), "Steam2" );
#endif

                Socket.Disconnect();
                return false;
            }

            return true;
        }
        bool GetAccountInfo( out ClientTGT clientTgt, out byte[] serverTgt, out Blob accRecord )
        {
            clientTgt = null;
            serverTgt = null;
            accRecord = null;

            try
            {

                byte result = Socket.Reader.ReadByte();

                if ( result != 0 )
                {
#if DEBUG
                    Trace.WriteLine( string.Format( "AuthServerClient GetAccountInfo failed. Result: {0}", result ), "Steam2" );
#endif

                    Socket.Disconnect();
                    return false;
                }

                ulong loginTime = Socket.Reader.ReadUInt64();
                ulong loginExpiry = Socket.Reader.ReadUInt64();

                TcpPacket packet = Socket.ReceivePacket();

                DataStream ds = new DataStream( packet.GetPayload(), true );

                ushort versionNum = ds.ReadUInt16();
                byte[] tgtIv = ds.ReadBytes( 16 );

                ushort tgtPlainSize = ds.ReadUInt16();
                ushort tgtCipherSize = ds.ReadUInt16();

                byte[] tgtCipher = ds.ReadBytes( tgtCipherSize );
                byte[] tgtPlain = CryptoHelper.AESDecrypt( tgtCipher, aesKey, tgtIv );

                clientTgt = ClientTGT.Deserialize( tgtPlain );

                ushort serverTgtSize = ds.ReadUInt16();
                serverTgt = ds.ReadBytes( serverTgtSize );

                uint accRecordSize = ds.ReadUInt32();
                byte[] accData = ds.ReadBytes( ds.SizeRemaining() - 40 );

                BlobParser.SetKey( clientTgt.AccountRecordKey );
                accRecord = BlobParser.ParseBlob( accData );

            }
            catch ( Exception ex )
            {
#if DEBUG
                Trace.WriteLine( string.Format( "AuthServerClient GetAccountInfo threw an exception.\n{0}", ex.ToString() ), "Steam2" );
#endif

                Socket.Disconnect();
                return false;
            }

            return true;
        }

        static byte[] GenerateAESKey( uint salt1, uint salt2, string pass )
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( salt1 );
            bb.Append( Encoding.ASCII.GetBytes( pass ) );
            bb.Append( salt2 );

            byte[] digest = CryptoHelper.SHAHash( bb.ToArray() );

            byte[] aesKey = new byte[ 16 ];
            Array.Copy( digest, 0, aesKey, 0, 16 );

            return aesKey;
        }
        static ulong GetObfuscationMask( uint internalIp, uint externalIp )
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( externalIp );
            bb.Append( internalIp );

            byte[] digest = CryptoHelper.SHAHash( bb.ToArray() );

            return BitConverter.ToUInt64( digest, 0 );
        }
        static byte[] GetPlaintext( ulong timeStamp, uint internalIp )
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( timeStamp );
            bb.Append( internalIp );

            return bb.ToArray();
        }
    }

}
