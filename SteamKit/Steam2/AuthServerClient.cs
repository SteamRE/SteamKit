using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace SteamKit
{
    class AuthServerClient : ServerClient
    {
        IPEndPoint endPoint;

        uint internalIp, externalIp;

        uint salt1, salt2;

        byte[] aesKey;
        byte[] iv;

        public AuthServerClient()
        {
        }


        public bool Connect( IPEndPoint authserver )
        {
            this.endPoint = authserver;

            this.internalIp = NetHelpers.GetIPAddress( NetHelpers.GetLocalIP() );
            return this.ConnectToServer( authserver );
        }

        public bool RequestIP()
        {
            return RequestIP( 0 );
        }

        bool RequestIP( uint type )
        {

            ByteBuffer bb = new ByteBuffer( true );

            bb.Append( ( uint )0 );
            bb.Append( ( byte )4 );
            bb.Append( internalIp );
            bb.Append( type );

            try
            {
                this.socket.Writer.Write( bb.ToArray() );
                byte result = this.socket.Reader.ReadByte();

                if ( result != 0 && type == 0 )
                {
                    this.Disconnect();
                    if ( !this.Connect( this.endPoint ) )
                        return false;

                    return this.RequestIP( 1 );
                }
                else if ( result != 0 )
                {
                    this.Disconnect();
                    return false;
                }

                externalIp = NetHelpers.EndianSwap( this.socket.Reader.ReadUInt32() );
            }
            catch
            {
                this.Disconnect();
                return false;
            }

            return true;

        }

        public bool GetSalt( string userName )
        {
            if ( !this.SendCommand( 2, ( ushort )userName.Length, userName, ( ushort )userName.Length, userName ) )
            {
                this.Disconnect();
                return false;
            }

            try
            {
                salt1 = this.socket.Reader.ReadUInt32();
                salt2 = this.socket.Reader.ReadUInt32();
            }
            catch
            {
                this.Disconnect();
                return false;
            }

            return true;
        }

        public bool SendLogin( string password )
        {
            aesKey = AuthServerClient.GenerateAESKey( salt1, salt2, password );
            iv = CryptoHelper.GenerateRandomBlock( 16 );

            MicroTime mt = MicroTime.Now;
            mt = mt ^ AuthServerClient.GetObfuscationMask( internalIp, externalIp );

            byte[] plainText = GetPlaintext( mt, internalIp );
            byte[] cipherText = CryptoHelper.AESEncrypt( plainText, aesKey, iv );

            TcpPacket packet = new TcpPacket();
            packet.Append( iv );
            packet.Append( ( ushort )plainText.Length );
            packet.Append( ( ushort )cipherText.Length );
            packet.Append( cipherText );

            try
            {
                this.socket.Send( packet );
            }
            catch
            {
                this.Disconnect();
                return false;
            }

            return true;
        }

        public bool GetAccountInfo( out ClientTGT clientTGT, out byte[] serverTGT, out byte[] accRecord )
        {
            clientTGT = null;
            serverTGT = null;
            accRecord = null;

            try
            {
                byte result = this.socket.Reader.ReadByte();

                if ( result != 0 )
                    return false;

                ulong loginTime = this.socket.Reader.ReadUInt64();
                ulong unk = this.socket.Reader.ReadUInt64();

                TcpPacket packet = this.RecvPacket();

                DataStream ds = new DataStream( packet.GetPayload(), true );

                ushort versionNum = ds.ReadUInt16();
                byte[] tgtIv = ds.ReadBytes( 16 );

                ushort tgtPlaintextSize = ds.ReadUInt16();
                ushort tgtCiphertextSize = ds.ReadUInt16();

                byte[] tgtEncrypted = ds.ReadBytes( tgtCiphertextSize );
                byte[] tgtPlaintext = CryptoHelper.AESDecrypt( tgtEncrypted, aesKey, tgtIv );

                clientTGT = ClientTGT.Deserialize( tgtPlaintext );

                ushort serverTGTSize = ds.ReadUInt16();
                serverTGT = ds.ReadBytes( serverTGTSize );

                uint accRecordSize = ds.ReadUInt32();
                accRecord = ds.ReadBytes( ds.SizeRemaining() - 40 );

                
            }
            catch
            {
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
