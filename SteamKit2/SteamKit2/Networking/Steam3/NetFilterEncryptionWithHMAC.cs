/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SteamKit2
{
    class NetFilterEncryptionWithHMAC : INetFilterEncryption
    {
        readonly byte[] sessionKey;
        readonly byte[] hmacSecret;
        readonly ILogContext log;

        public NetFilterEncryptionWithHMAC( byte[] sessionKey, ILogContext log )
        {
            ArgumentNullException.ThrowIfNull( log );

            DebugLog.Assert( sessionKey.Length == 32, nameof( NetFilterEncryptionWithHMAC ), "AES session key was not 32 bytes!" );

            this.sessionKey = sessionKey;
            this.log = log;
            this.hmacSecret = new byte[ 16 ];
            Array.Copy( sessionKey, 0, hmacSecret, 0, hmacSecret.Length );
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            try
            {
                return SymmetricDecryptHMACIV( data );
            }
            catch ( CryptographicException ex )
            {
                log.LogDebug( nameof( NetFilterEncryptionWithHMAC ), $"Unable to decrypt incoming packet: {ex.Message}" );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
        }

        public byte[] ProcessOutgoing( byte[] data )
        {
            return SymmetricEncryptWithHMACIV( data );
        }

        /// <summary>
        /// Decrypts using AES/CBC/PKCS7 with an input byte array and key, using the IV (comprised of random bytes and the HMAC-SHA1 of the random bytes and plaintext) prepended using AES/ECB/None
        /// </summary>
        byte[] SymmetricDecryptHMACIV( byte[] input )
        {
            ArgumentNullException.ThrowIfNull( input );

            var truncatedKeyForHmac = new byte[ 16 ];
            Array.Copy( sessionKey, 0, truncatedKeyForHmac, 0, truncatedKeyForHmac.Length );

            var plaintextData = CryptoHelper.SymmetricDecrypt( input, sessionKey, out var iv );

            // validate HMAC
            byte[] hmacBytes;
            using ( var hmac = new HMACSHA1( hmacSecret ) )
            using ( var ms = new MemoryStream() )
            {
                ms.Write( iv, iv.Length - 3, 3 );
                ms.Write( plaintextData, 0, plaintextData.Length );
                ms.Seek( 0, SeekOrigin.Begin );

                hmacBytes = hmac.ComputeHash( ms );
            }

            if ( !hmacBytes.Take( iv.Length - 3 ).SequenceEqual( iv.Take( iv.Length - 3 ) ) )
            {
                throw new CryptographicException( $"{nameof( SymmetricDecryptHMACIV )} was unable to decrypt packet: HMAC from server did not match computed HMAC." );
            }

            return plaintextData;
        }

        /// <summary>
        /// Performs an encryption using AES/CBC/PKCS7 with an input byte array and key, with a IV (comprised of random bytes and the HMAC-SHA1 of the random bytes and plaintext) prepended using AES/ECB/None
        /// </summary>
        byte[] SymmetricEncryptWithHMACIV( byte[] input )
        {
            ArgumentNullException.ThrowIfNull( input );

            // IV is HMAC-SHA1(Random(3) + Plaintext) + Random(3). (Same random values for both)
            var iv = new byte[ 16 ];
            var random = RandomNumberGenerator.GetBytes( 3 );
            Array.Copy( random, 0, iv, iv.Length - random.Length, random.Length );

            using ( var hmac = new HMACSHA1( hmacSecret ) )
            using ( var ms = new MemoryStream() )
            {
                ms.Write( random, 0, random.Length );
                ms.Write( input, 0, input.Length );
                ms.Seek( 0, SeekOrigin.Begin );

                var hash = hmac.ComputeHash( ms );
                Array.Copy( hash, iv, iv.Length - random.Length );
            }

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;

            byte[] cryptedIv;

            // encrypt iv using ECB and provided key
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using ( var aesTransform = aes.CreateEncryptor( sessionKey, null ) )
            {
                cryptedIv = aesTransform.TransformFinalBlock( iv, 0, iv.Length );
            }

            // encrypt input plaintext with CBC using the generated (plaintext) IV and the provided key
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ( var aesTransform = aes.CreateEncryptor( sessionKey, iv ) )
            using ( var ms = new MemoryStream() )
            using ( var cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Write ) )
            {
                cs.Write( input, 0, input.Length );
                cs.FlushFinalBlock();

                var cipherText = ms.ToArray();

                // final output is 16 byte ecb crypted IV + cbc crypted plaintext
                var output = new byte[ cryptedIv.Length + cipherText.Length ];

                Array.Copy( cryptedIv, 0, output, 0, cryptedIv.Length );
                Array.Copy( cipherText, 0, output, cryptedIv.Length, cipherText.Length );

                return output;
            }
        }
    }
}
