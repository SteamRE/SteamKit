/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SteamKit2
{
    /// <summary>
    /// Represents a client capable of connecting to an authentication server.
    /// </summary>
    public sealed class AuthServerClient : ServerClient
    {
        uint internalIp, externalIp;
        uint salt1, salt2;

        byte[] aesKey, iv;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthServerClient"/> class.
        /// </summary>
        public AuthServerClient()
        {
        }


        /// <summary>
        /// Represents a set of possible results for calling Login.
        /// </summary>
        public enum LoginResult
        {
            /// <summary>
            /// An error occurred while requesting the client's external IP.
            /// </summary>
            ErrorRequestIP,
            /// <summary>
            /// An error occurred while reading the salt.
            /// </summary>
            ErrorGetSalt,
            /// <summary>
            /// An error occured while attempting the login command.
            /// </summary>
            ErrorDoLogin,
            /// <summary>
            /// An error occured while reading account info.
            /// </summary>
            ErrorGetAccountInfo,

            /// <summary>
            /// Successfully logged in.
            /// </summary>
            LoggedIn,

            /// <summary>
            /// Ocurrs when the password for the account is invalid, or the client's system clock is not accurate.
            /// </summary>
            InvalidPassword,
            /// <summary>
            /// The specified account does not exist.
            /// </summary>
            AccountNotFound,
            /// <summary>
            /// The account is disabled.
            /// </summary>
            AccountDisabled,
        }

        /// <summary>
        /// Attempts a login to the connected authentication server.
        /// </summary>
        /// <param name="user">The username.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="clientTgt">The client TGT.</param>
        /// <param name="serverTgt">The server TGT.</param>
        /// <param name="accRecord">The client account record.</param>
        /// <returns>A LoginResult value describing what result.</returns>
        public LoginResult Login( string user, string pass, out ClientTGT clientTgt, out byte[] serverTgt, out Blob accRecord )
        {
            clientTgt = null;
            serverTgt = null;
            accRecord = null;

            if ( !this.RequestIP( user ) )
                return LoginResult.ErrorRequestIP;

            if ( !this.GetSalt( user ) )
                return LoginResult.ErrorGetSalt;

            if ( !this.DoLogin( pass ) )
                return LoginResult.ErrorDoLogin;

            sbyte accResult = this.GetAccountInfo( out clientTgt, out serverTgt, out accRecord );

            switch ( accResult )
            {
                case -1:
                    return LoginResult.ErrorGetAccountInfo;

                case 0:
                    return LoginResult.LoggedIn;

                case 1:
                    return LoginResult.AccountNotFound;

                case 2:
                    return LoginResult.InvalidPassword;

                case 4:
                    return LoginResult.AccountDisabled;

                default:
                    return LoginResult.ErrorGetAccountInfo;
            }

        }


        bool RequestIP( string user )
        {
            try
            {
                internalIp = NetHelpers.GetIPAddress( Socket.GetLocalIP() );

                byte[] userHash = CryptoHelper.JenkinsHash( Encoding.ASCII.GetBytes( user ) );
                uint userData = BitConverter.ToUInt32( userHash, 0 ) & 1;

                BinaryWriterEx bb = new BinaryWriterEx(true);

                bb.WriteType<uint>( 0 );
                bb.WriteType<byte>( 4 );
                bb.Write( internalIp );
                bb.Write( userData );

                Socket.Send( bb.ToArray() );

                byte result = Socket.Reader.ReadByte();

                if ( result != 0 )
                {
                    DebugLog.WriteLine( "AuthServerClient", "RequestIP failed. Result: {0}", result );

                    Socket.Disconnect();
                    return false;
                }

                externalIp = NetHelpers.EndianSwap( Socket.Reader.ReadUInt32() );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "AuthServerClient", "RequestIP threw an exception.\n{0}", ex.ToString() );

                Socket.Disconnect();
                return false;
            }

            return true;
        }
        bool GetSalt( string user )
        {
            ushort userLen = ( ushort )user.Length;
            user = user.ToLower();

            if ( !this.SendCommand( 2, userLen, user, userLen, user ) )
            {
                DebugLog.WriteLine( "AuthServerClient", "GetSalt command '2' failed." );

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
                DebugLog.WriteLine ("AuthServerClient", "GetSalt threw an exception while reading the salt.\n{0}", ex.ToString() );

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

                packet.Write( iv );
                packet.Write( ( ushort )plainText.Length );
                packet.Write( ( ushort )cipherText.Length );
                packet.Write( cipherText );

                Socket.Send( packet );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "AuthServerClient", "DoLogin threw an exception while sending login packet.\n{0}", ex.ToString() );

                Socket.Disconnect();
                return false;
            }

            return true;
        }
        sbyte GetAccountInfo( out ClientTGT clientTgt, out byte[] serverTgt, out Blob accRecord )
        {
            clientTgt = null;
            serverTgt = null;
            accRecord = null;

            try
            {

                byte result = Socket.Reader.ReadByte();

                if ( result != 0 )
                {
                    DebugLog.WriteLine( "AuthServerClient", "GetAccountInfo failed. Result: {0}", result );

                    Socket.Disconnect();
                    return ( sbyte )result;
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
                DebugLog.WriteLine( "AuthServerClient", "GetAccountInfo threw an exception while reading account info.\n{0}", ex.ToString() );

                Socket.Disconnect();
                return -1;
            }

            return 0;
        }


        static byte[] GenerateAESKey( uint salt1, uint salt2, string pass )
        {
            BinaryWriterEx bb = new BinaryWriterEx();

            bb.Write( salt1 );
            bb.Write( Encoding.ASCII.GetBytes( pass ) );
            bb.Write( salt2 );

            byte[] digest = CryptoHelper.SHAHash( bb.ToArray() );

            byte[] aesKey = new byte[ 16 ];
            Array.Copy( digest, 0, aesKey, 0, 16 );

            return aesKey;
        }
        static ulong GetObfuscationMask( uint internalIp, uint externalIp )
        {
            BinaryWriterEx bb = new BinaryWriterEx();

            bb.Write( externalIp );
            bb.Write( internalIp );

            byte[] digest = CryptoHelper.SHAHash( bb.ToArray() );

            return BitConverter.ToUInt64( digest, 0 );
        }
        static byte[] GetPlaintext( ulong timeStamp, uint internalIp )
        {
            BinaryWriterEx bb = new BinaryWriterEx();

            bb.Write( timeStamp );
            bb.Write( internalIp );

            return bb.ToArray();
        }
    }

}
