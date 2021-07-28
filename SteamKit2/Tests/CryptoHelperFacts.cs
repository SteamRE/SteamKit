using System;
using System.Net;
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

            using var sha256 = SHA256.Create();
            var key = sha256.ComputeHash( Encoding.UTF8.GetBytes( encryptionKey ) );

            var encryptedData = Encoding.UTF8.GetBytes( decryptedExpected );
            encryptedData = CryptoHelper.SymmetricEncrypt( encryptedData, key );
            var encryptedString = Convert.ToBase64String( encryptedData );

            var decryptedData = Convert.FromBase64String( encryptedString );
            decryptedData = CryptoHelper.SymmetricDecrypt( decryptedData, key );
            var decryptedString = Encoding.UTF8.GetString( decryptedData );

            Assert.Equal( decryptedExpected, decryptedString );
        }
    }
}
