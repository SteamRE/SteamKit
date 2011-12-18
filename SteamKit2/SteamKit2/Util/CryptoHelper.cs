/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Classless.Hasher;

namespace SteamKit2
{
    public static class CryptoHelper
    {
        private static RSACryptoServiceProvider rsaProvider;
		
        public static void InitializeRSA( byte[] key )
        {			
		    AsnKeyParser keyParser = new AsnKeyParser( key );
		    RSAParameters rsaParam = keyParser.ParseRSAPublicKey();
			
		    rsaProvider = new RSACryptoServiceProvider();
		    rsaProvider.ImportParameters(rsaParam);
        }
		
        public static byte[] RSAEncrypt( byte[] input )
        {
            byte[] output = rsaProvider.Encrypt( input, true );
			
            return output;
        }

        public static byte[] SHAHash( byte[] input )
        {
            SHA1Managed sha = new SHA1Managed();

            byte[] output = sha.ComputeHash( input );

            sha.Clear();

            return output;
        }


        public static byte[] AESEncrypt( byte[] input, byte[] key, byte[] iv )
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform aesTransform = aes.CreateEncryptor( key, iv );

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Write );

            cs.Write( input, 0, input.Length );
            cs.FlushFinalBlock();

            byte[] cipherText = ms.ToArray();


            cs.Close();
            ms.Close();

            aes.Clear();

            return cipherText;
        }

        public static byte[] AESDecrypt( byte[] input, byte[] key, byte[] iv )
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;

            aes.Mode = CipherMode.CBC;

            byte[] plainText = new byte[ input.Length ];
            int outLen = 0;

            using ( ICryptoTransform aesTransform = aes.CreateDecryptor( key, iv ) )
            using ( MemoryStream ms = new MemoryStream( input ) )
            using ( CryptoStream cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Read ) )
            {
                outLen = cs.Read( plainText, 0, plainText.Length );
            }

            byte[] output = new byte[ outLen ];
            Array.Copy( plainText, 0, output, 0, output.Length );

            return output;
        }


        public static byte[] SymmetricEncrypt( byte[] input, byte[] key )
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 256;

            // generate iv
            byte[] iv = PsuedoRandom.GenerateRandomBlock( 16 );
            byte[] cryptedIv = new byte[ 16 ];

            // encrypt iv using ECB and provided key
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            ICryptoTransform aesTransform = aes.CreateEncryptor( key, null );
            cryptedIv = aesTransform.TransformFinalBlock( iv, 0, iv.Length );

            // encrypt input plaintext with CBC using the generated IV and the provided key
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aesTransform = aes.CreateEncryptor( key, iv );

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Write );

            cs.Write( input, 0, input.Length );
            cs.FlushFinalBlock();

            byte[] cipherText = ms.ToArray();

            // clean up
            cs.Close();
            ms.Close();
            aes.Clear();

            // ciphertext iv becomes the first 16 bytes of the output
            byte[] output = new byte[ cryptedIv.Length + cipherText.Length ];

            Array.Copy( cryptedIv, 0, output, 0, cryptedIv.Length );
            Array.Copy( cipherText, 0, output, cryptedIv.Length, cipherText.Length );

            return output;
        }

        public static byte[] SymmetricDecrypt( byte[] input, byte[] key )
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 256;

            // first 16 bytes of input is the ECB encrypted IV
            byte[] cryptedIv = new byte[ 16 ];
            byte[] iv = new byte[ cryptedIv.Length ];
            Array.Copy( input, 0, cryptedIv, 0, cryptedIv.Length );

            // the rest is ciphertext
            byte[] cipherText = new byte[ input.Length - cryptedIv.Length ];
            Array.Copy( input, cryptedIv.Length, cipherText, 0, cipherText.Length );

            // decrypt the IV using ECB
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            ICryptoTransform aesTransform = aes.CreateDecryptor( key, null );
            iv = aesTransform.TransformFinalBlock( cryptedIv, 0, cryptedIv.Length );

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aesTransform = aes.CreateDecryptor( key, iv );

            MemoryStream ms = new MemoryStream( cipherText );
            CryptoStream cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Read );

            // plaintext is never longer than ciphertext
            byte[] plaintext = new byte[ cipherText.Length ];

            int len = cs.Read( plaintext, 0, plaintext.Length );

            byte[] output = new byte[ len ];
            Array.Copy( plaintext, 0, output, 0, len );

            // clean up
            cs.Close();
            ms.Close();
            aes.Clear();

            return output;
        }

        public static byte[] JenkinsHash( byte[] input )
        {
            using ( JenkinsHash jHash = new JenkinsHash() )
            {
                byte[] hash = jHash.ComputeHash( input );
                Array.Reverse( hash );

                return hash;
            }
        }

        public static byte[] CRCHash( byte[] input )
        {
            using ( Crc crc = new Crc( CrcParameters.GetParameters( CrcStandard.Crc32Bit ) ) )
            {
                byte[] hash = crc.ComputeHash( input );
                Array.Reverse( hash );

                return hash;
            }
        }

        public static byte[] AdlerHash(byte[] input)
        {
            using ( Adler32 adler = new Adler32() )
            {
                byte[] hash = adler.ComputeHash( input );
                Array.Reverse( hash );

                return hash;
            }
        }

        public static byte[] GenerateRandomBlock( int size )
        {
            return PsuedoRandom.GenerateRandomBlock( size );
        }

    }
}
