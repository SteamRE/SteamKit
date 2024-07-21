/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SteamKit2
{
    /// <summary>
    /// Provides Crypto functions used in Steam protocols
    /// </summary>
    internal static class CryptoHelper
    {
        /// <summary>
        /// Performs an encryption using AES/CBC/PKCS7 with an input byte array and key, with a random IV prepended using AES/ECB/None
        /// </summary>
        public static byte[] SymmetricEncryptWithIV( byte[] input, byte[] key, byte[] iv )
        {
            ArgumentNullException.ThrowIfNull( input );

            ArgumentNullException.ThrowIfNull( key );

            ArgumentNullException.ThrowIfNull( iv );

            DebugLog.Assert( key.Length == 32, "CryptoHelper", "SymmetricEncrypt used with non 32 byte key!" );

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;

            byte[] cryptedIv;

            // encrypt iv using ECB and provided key
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using ( var aesTransform = aes.CreateEncryptor( key, null ) )
            {
                cryptedIv = aesTransform.TransformFinalBlock( iv, 0, iv.Length );
            }

            // encrypt input plaintext with CBC using the generated (plaintext) IV and the provided key
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ( var aesTransform = aes.CreateEncryptor( key, iv ) )
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

        /// <summary>
        /// Performs an encryption using AES/CBC/PKCS7 with an input byte array and key, with a random IV prepended using AES/ECB/None
        /// </summary>
        public static byte[] SymmetricEncrypt( byte[] input, byte[] key )
        {
            ArgumentNullException.ThrowIfNull( input );

            ArgumentNullException.ThrowIfNull( key );

            var iv = RandomNumberGenerator.GetBytes( 16 );
            return SymmetricEncryptWithIV( input, key, iv );
        }

        /// <summary>
        /// Performs an encryption using AES/CBC/PKCS7 with an input byte array and key, with a IV (comprised of random bytes and the HMAC-SHA1 of the random bytes and plaintext) prepended using AES/ECB/None
        /// </summary>
        public static byte[] SymmetricEncryptWithHMACIV( byte[] input, byte[] key, byte[] hmacSecret )
        {
            ArgumentNullException.ThrowIfNull( input );

            ArgumentNullException.ThrowIfNull( key );

            ArgumentNullException.ThrowIfNull( hmacSecret );

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
            
            return SymmetricEncryptWithIV( input, key, iv );
        }

        /// <summary>
        /// Decrypts using AES/CBC/PKCS7 with an input byte array and key, using the random IV prepended using AES/ECB/None
        /// </summary>
        public static byte[] SymmetricDecrypt( byte[] input, byte[] key )
        {
            ArgumentNullException.ThrowIfNull( input );

            ArgumentNullException.ThrowIfNull( key );

            return SymmetricDecrypt( input, key, out _ );
        }

        /// <summary>
        /// Decrypts using AES/CBC/PKCS7 with an input byte array and key, using the IV (comprised of random bytes and the HMAC-SHA1 of the random bytes and plaintext) prepended using AES/ECB/None
        /// </summary>
        public static byte[] SymmetricDecryptHMACIV( byte[] input, byte[] key, byte[] hmacSecret )
        {
            ArgumentNullException.ThrowIfNull( input );

            ArgumentNullException.ThrowIfNull( key );

            ArgumentNullException.ThrowIfNull( hmacSecret );

            DebugLog.Assert( key.Length >= 16, "CryptoHelper", "SymmetricDecryptHMACIV used with a key smaller than 16 bytes." );
            var truncatedKeyForHmac = new byte[ 16 ];
            Array.Copy( key, 0, truncatedKeyForHmac, 0, truncatedKeyForHmac.Length );

            var plaintextData = SymmetricDecrypt( input, key, out var iv );

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
                throw new CryptographicException( string.Format( CultureInfo.InvariantCulture, "{0} was unable to decrypt packet: HMAC from server did not match computed HMAC.", nameof(CryptoHelper) ) );
            }

            return plaintextData;
        }

        /// <summary>
        /// Decrypts using AES/CBC/PKCS7 with an input byte array and key, using the random IV prepended using AES/ECB/None
        /// </summary>
        static byte[] SymmetricDecrypt( byte[] input, byte[] key, out byte[] iv )
        {
            ArgumentNullException.ThrowIfNull( input );

            ArgumentNullException.ThrowIfNull( key );

            DebugLog.Assert( key.Length == 32, "CryptoHelper", "SymmetricDecrypt used with non 32 byte key!" );

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;

            // first 16 bytes of input is the ECB encrypted IV
            byte[] cryptedIv = new byte[ 16 ];
            iv = new byte[ cryptedIv.Length ];
            Array.Copy( input, 0, cryptedIv, 0, cryptedIv.Length );

            // the rest is ciphertext
            byte[] cipherText = new byte[ input.Length - cryptedIv.Length ];
            Array.Copy( input, cryptedIv.Length, cipherText, 0, cipherText.Length );

            // decrypt the IV using ECB
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using ( var aesTransform = aes.CreateDecryptor( key, null ) )
            {
                iv = aesTransform.TransformFinalBlock( cryptedIv, 0, cryptedIv.Length );
            }

            // decrypt the remaining ciphertext in cbc with the decrypted IV
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ( var aesTransform = aes.CreateDecryptor( key, iv ) )
            using ( var ms = new MemoryStream( cipherText ) )
            using ( var cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Read ) )
            {
                // plaintext is never longer than ciphertext
                byte[] plaintext = new byte[ cipherText.Length ];

                int len = cs.ReadAll( plaintext );

                byte[] output = new byte[ len ];
                Array.Copy( plaintext, 0, output, 0, len );

                return output;
            }
        }
    }
}
