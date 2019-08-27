using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SteamKit2.Util
{
    sealed class CryptographicContext : IDisposable
    {
        public CryptographicContext(byte[] key, byte[] hmacSecret)
        {
            this.key = key;
            this.hmacSecret = hmacSecret;

            rng = RandomNumberGenerator.Create();

            hmac = new HMACSHA1(hmacSecret);

            ivAes = Aes.Create();
            ivAes.BlockSize = 128;
            ivAes.KeySize = 256;
            ivAes.Mode = CipherMode.ECB;
            ivAes.Padding = PaddingMode.None;

            ivEncryptor = ivAes.CreateEncryptor(key, null);
            ivDecryptor = ivAes.CreateDecryptor(key, null);

            cipherAes = Aes.Create();
            cipherAes.BlockSize = 128;
            cipherAes.KeySize = 256;

            cipherAes.Mode = CipherMode.CBC;
            cipherAes.Padding = PaddingMode.PKCS7;
        }

        const int InitializationVectorLength = 16;
        const int InitializationVectorRandomLength = 3;

        readonly byte[] key;
        readonly byte[] hmacSecret;

        readonly RandomNumberGenerator rng;
        readonly HMACSHA1 hmac;

        readonly Aes ivAes;
        readonly ICryptoTransform ivEncryptor;
        readonly ICryptoTransform ivDecryptor;

        readonly Aes cipherAes;

        public ArraySegment<byte> SymmetricEncryptWithIVHMAC(byte[] plainText, byte[] cipherTextBuffer)
        {
            var iv = new byte[InitializationVectorLength];
            GenerateInitializationVector(plainText, new ArraySegment<byte>(iv, 0, iv.Length));
            return SymmetricEncryptWithIV(plainText, iv, cipherTextBuffer);
        }

        public ArraySegment<byte> SymmetricDecryptWithIVHMAC(byte[] cipherText, byte[] plainTextBuffer)
        {
            var iv = ivDecryptor.TransformFinalBlock(cipherText, 0, InitializationVectorLength);

            using (var aesTransform = cipherAes.CreateDecryptor(key, iv))
            using (var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
            using (var cs = new CryptoStream(ms, aesTransform, CryptoStreamMode.Read))
            {
                // plaintext is never longer than ciphertext
                int len = cs.Read(plainTextBuffer, 0, plainTextBuffer.Length);
                var plainText = new ArraySegment<byte>(plainTextBuffer, 0, len);
                ValidateInitializationVector(plainText, new ArraySegment<byte>(iv, 0, iv.Length));
                return plainText;
            }
        }

        void GenerateInitializationVector(byte[] plainText, ArraySegment<byte> iv)
        {
            var ivRandom = new ArraySegment<byte>(iv.Array, iv.Offset + iv.Count - InitializationVectorRandomLength, InitializationVectorRandomLength);
            rng.GetBytes(ivRandom.Array, ivRandom.Offset, ivRandom.Count);

            var hmacBufferLength = ivRandom.Count + plainText.Length;
            var hmacBuffer = ArrayPool<byte>.Shared.Rent(hmacBufferLength);
            try
            {
                Array.Copy(ivRandom.Array, ivRandom.Offset, hmacBuffer, 0, ivRandom.Count);
                plainText.CopyTo(hmacBuffer, ivRandom.Count);

                var hmacValue = hmac.ComputeHash(hmacBuffer, 0, hmacBufferLength);
                Array.Copy(hmacValue, 0, iv.Array, iv.Offset, iv.Count - ivRandom.Count);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(hmacBuffer);
            }
        }

        void ValidateInitializationVector(ArraySegment<byte> plainText, ArraySegment<byte> iv)
        {
            var ivRandom = new ArraySegment<byte>(iv.Array, iv.Offset + iv.Count - InitializationVectorRandomLength, InitializationVectorRandomLength);
            var hmacBufferLength = ivRandom.Count + plainText.Count;
            var hmacBuffer = ArrayPool<byte>.Shared.Rent(hmacBufferLength);

            try
            {
                Array.Copy(ivRandom.Array, ivRandom.Offset, hmacBuffer, 0, ivRandom.Count);
                Array.Copy(plainText.Array, plainText.Offset, hmacBuffer, ivRandom.Count, plainText.Count);

                var hmacValue = hmac.ComputeHash(hmacBuffer, 0, hmacBufferLength);

                var hmacIsOkSoFar = true;

                for (var i = 0; i < InitializationVectorLength - InitializationVectorRandomLength; i++)
                {
                    hmacIsOkSoFar &= (hmacValue[i] == iv.Array[iv.Offset + i]);
                }

                if (!hmacIsOkSoFar)
                {
                    throw new Exception("HMAC did not validate.");
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(hmacBuffer);
            }
        }

        ArraySegment<byte> SymmetricEncryptWithIV(byte[] plaintext, byte[] iv, byte[] cipherTextBuffer)
        {
            var encryptedIv = ivEncryptor.TransformFinalBlock(iv, 0, iv.Length);

            var cipherTextSize = CalculateMaxEncryptedDataLength(plaintext.Length + encryptedIv.Length);

            using (var aesTransform = cipherAes.CreateEncryptor(key, iv))
            using (var ms = new MemoryStream(cipherTextBuffer))
            using (var cs = new CryptoStream(ms, aesTransform, CryptoStreamMode.Write))
            {
                ms.Write(encryptedIv, 0, encryptedIv.Length);

                cs.Write(plaintext, 0, plaintext.Length);
                cs.FlushFinalBlock();

                return new ArraySegment<byte>(cipherTextBuffer, 0, (int)ms.Position);
            }
        }

        public int CalculateMaxEncryptedDataLength(int plaintextDataLength)
            => CalculateMaxEncryptedDataLengthRaw(16) + CalculateMaxEncryptedDataLengthRaw(plaintextDataLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int CalculateMaxEncryptedDataLengthRaw(int plaintextDataLength)
        {
            var numberOfBlocksRequired = (int)Math.Ceiling((double)plaintextDataLength / (cipherAes.BlockSize / 8));
            var cipherTextSize = cipherAes.BlockSize * numberOfBlocksRequired / 8;
            return cipherTextSize;
        }

        public void Dispose()
        {
            rng.Dispose();

            hmac.Dispose();

            ivEncryptor.Dispose();
            ivDecryptor.Dispose();
            ivAes.Dispose();

            cipherAes.Dispose();
        }
    }
}
