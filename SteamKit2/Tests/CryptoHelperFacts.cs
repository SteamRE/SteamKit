using System;
using System.Security.Cryptography;
using System.Text;
using SteamKit2;
using Xunit;

namespace Tests
{
    public class CryptoHelperFacts
    {
        [Fact]
        public void TestSymmetricEncryption()
        {
            const string decryptedExpected = "this is a 24 byte string";
            const string encryptionKey = "encryption key";

            var key = SHA256.HashData( Encoding.UTF8.GetBytes( encryptionKey ) );

            var iv = RandomNumberGenerator.GetBytes( 16 );
            var encryptedData = Encoding.UTF8.GetBytes( decryptedExpected );
            encryptedData = SymmetricEncryptWithIV( encryptedData, key, iv );

            var encryptedString = Convert.ToBase64String( encryptedData );

            var decryptedData = Convert.FromBase64String( encryptedString );
            decryptedData = CryptoHelper.SymmetricDecrypt( decryptedData, key );
            var decryptedString = Encoding.UTF8.GetString( decryptedData );

            Assert.Equal( decryptedExpected, decryptedString );
        }

        static byte[] SymmetricEncryptWithIV( byte[] input, byte[] key, byte[] iv )
        {
            ArgumentNullException.ThrowIfNull( input );
            ArgumentNullException.ThrowIfNull( key );
            ArgumentNullException.ThrowIfNull( iv );

            DebugLog.Assert( key.Length == 32, nameof( CryptoHelper ), $"{nameof( SymmetricEncryptWithIV )} used with non 32 byte key!" );

            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = key;

            var cryptedIv = aes.EncryptEcb( iv, PaddingMode.None );
            var cipherText = aes.EncryptCbc( input, iv, PaddingMode.PKCS7 );

            // final output is 16 byte ecb crypted IV + cbc crypted plaintext
            var output = new byte[ cryptedIv.Length + cipherText.Length ];

            Array.Copy( cryptedIv, 0, output, 0, cryptedIv.Length );
            Array.Copy( cipherText, 0, output, cryptedIv.Length, cipherText.Length );

            return output;
        }
    }
}
