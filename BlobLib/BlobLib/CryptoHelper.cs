using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace BlobLib
{
    static class CryptoHelper
    {

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
            //aes.Padding = PaddingMode.PKCS7;

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
    }
}
