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
using System.Linq;
using System.Diagnostics;

namespace SteamKit2
{
    class RSACrypto : IDisposable
    {
        RSACryptoServiceProvider rsa;


        public RSACrypto( byte[] key )
        {
            AsnKeyParser keyParser = new AsnKeyParser( key );

            rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters( keyParser.ParseRSAPublicKey() );
        }


        public byte[] Encrypt( byte[] input )
        {
            return rsa.Encrypt( input, true );
        }


        public void Dispose()
        {
            rsa.Dispose();
        }
    }

    static class CryptoHelper
    {
        public static byte[] SHAHash( byte[] input )
        {
            using ( var sha = new SHA1Managed() )
            {
                return sha.ComputeHash( input );
            }
        }


        public static byte[] AESEncrypt( byte[] input, byte[] key, byte[] iv )
        {
            using ( var aes = new RijndaelManaged() )
            {
                aes.BlockSize = 128;
                aes.KeySize = 128;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using ( var aesTransform = aes.CreateEncryptor( key, iv ) )
                using ( var ms = new MemoryStream() )
                using ( var cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Write ) )
                {
                    cs.Write( input, 0, input.Length );
                    cs.FlushFinalBlock();
                    
                    return ms.ToArray();
                }
            }
        }

        public static byte[] AESDecrypt( byte[] input, byte[] key, byte[] iv )
        {
            using ( var aes = new RijndaelManaged() )
            {
                aes.BlockSize = 128;
                aes.KeySize = 128;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] plainText = new byte[ input.Length ];
                int outLen = 0;

                using ( var aesTransform = aes.CreateDecryptor( key, iv ) )
                using ( var ms = new MemoryStream( input ) )
                using ( var cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Read ) )
                {
                    outLen = cs.Read( plainText, 0, plainText.Length );
                }

                byte[] output = new byte[ outLen ];
                Array.Copy( plainText, 0, output, 0, output.Length );

                return output;
            }
        }


        public static byte[] SymmetricEncrypt( byte[] input, byte[] key )
        {
            Debug.Assert( key.Length == 32 );

            using ( var aes = new RijndaelManaged() )
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;

                // generate iv
                byte[] iv = GenerateRandomBlock( 16 );
                byte[] cryptedIv = new byte[ 16 ];


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

                    byte[] cipherText = ms.ToArray();

                    // final output is 16 byte ecb crypted IV + cbc crypted plaintext
                    byte[] output = new byte[ cryptedIv.Length + cipherText.Length ];

                    Array.Copy( cryptedIv, 0, output, 0, cryptedIv.Length );
                    Array.Copy( cipherText, 0, output, cryptedIv.Length, cipherText.Length );

                    return output;
                }
            }
        }

        public static byte[] SymmetricDecrypt( byte[] input, byte[] key )
        {
            Debug.Assert( key.Length == 32 );

            using ( var aes = new RijndaelManaged() )
            {
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

                    int len = cs.Read( plaintext, 0, plaintext.Length );

                    byte[] output = new byte[ len ];
                    Array.Copy( plaintext, 0, output, 0, len );

                    return output;
                }
            }
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

        public static byte[] AdlerHash( byte[] input )
        {
            uint a = 0, b = 0;
            for ( int i = 0 ; i < input.Length ; i++ )
            {
                a = ( a + input[ i ] ) % 65521;
                b = ( b + a ) % 65521;
            }
            return BitConverter.GetBytes( a | ( b << 16 ) );
        }

        public static byte[] GenerateRandomBlock( int size )
        {
            using ( var rng = new RNGCryptoServiceProvider() )
            {
                byte[] block = new byte[ size ];

                rng.GetBytes( block );

                return block;
            }
        }

    }
}
