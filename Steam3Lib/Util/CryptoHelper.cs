using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.DirectoryServices.Protocols;
using System.IO;

namespace SteamLib
{
    static class CryptoHelper
    {
        public static byte[] RSAEncrypt( byte[] input, byte[] key )
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider( 1024 );

            RSAParameters rsaParams = new RSAParameters();
            rsaParams.Modulus = new byte[ rsa.KeySize / 8 ];
            rsaParams.Exponent = new byte[ 1 ]; // valve keys have a 1 byte exponent

            Array.Copy( key, 29, rsaParams.Modulus, 0, rsaParams.Modulus.Length );
            Array.Copy( key, 29 + rsaParams.Modulus.Length + 2, rsaParams.Exponent, 0, rsaParams.Exponent.Length );

            rsa.ImportParameters( rsaParams );

            byte[] output = rsa.Encrypt( input, true );

            rsa.Clear();

            return output;
        }

        public static byte[] AESDecrypt( byte[] input, byte[] key, byte[] iv )
        {
            RijndaelManaged aes = new RijndaelManaged();

            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform aesTransform = aes.CreateDecryptor( key, iv );

            MemoryStream ms = new MemoryStream( input );
            CryptoStream cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Read );

            // plaintext is never longer than ciphertext
            byte[] output = new byte[ input.Length ];

            int len = cs.Read( output, 0, output.Length );

            byte[] realOutput = new byte[ len ];
            Array.Copy( output, 0, realOutput, 0, len );

            cs.Close();
            ms.Close();

            aes.Clear();

            return realOutput;
        }

        public static byte[] AESEncrypt( byte[] input, byte[] key, byte[] iv )
        {
            RijndaelManaged aes = new RijndaelManaged();

            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform aesTransform = aes.CreateEncryptor( key, iv );

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream( ms, aesTransform, CryptoStreamMode.Write );

            cs.Write( input, 0, input.Length );

            cs.FlushFinalBlock();

            byte[] output = ms.ToArray();

            cs.Close();
            ms.Close();

            aes.Clear();

            return output;
        }

        public static byte[] GenerateRandomBlock( int size )
        {
            return PsuedoRandom.Instance.GenerateRandomBlock( size );
        }
    }
}
