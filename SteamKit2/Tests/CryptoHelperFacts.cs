using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;

namespace Tests
{
    [TestClass]
    public class CryptoHelperFacts
    {
        [TestMethod]
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

            Assert.AreEqual( decryptedExpected, decryptedString );
        }
    }
}
