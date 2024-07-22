/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SteamKit2
{
    class NetFilterEncryptionWithHMAC
    {
        const int InitializationVectorLength = 16;
        const int InitializationVectorRandomLength = 3;

        readonly Aes aes;
        readonly ILogContext log;
        readonly byte[] hmacSecret;

        public NetFilterEncryptionWithHMAC( byte[] sessionKey, ILogContext log )
        {
            ArgumentNullException.ThrowIfNull( log );

            DebugLog.Assert( sessionKey.Length == 32, nameof( NetFilterEncryptionWithHMAC ), "AES session key was not 32 bytes!" );

            this.log = log;

            hmacSecret = new byte[ 16 ];
            Array.Copy( sessionKey, 0, hmacSecret, 0, hmacSecret.Length );

            aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = sessionKey;
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

        public int ProcessOutgoing( Span<byte> data, byte[] output ) => SymmetricEncryptWithHMACIV( data, output );

        /// <summary>
        /// Decrypts using AES/CBC/PKCS7 with an input byte array and key, using the IV (comprised of random bytes and the HMAC-SHA1 of the random bytes and plaintext) prepended using AES/ECB/None
        /// </summary>
        byte[] SymmetricDecryptHMACIV( Span<byte> input )
        {
            Span<byte> iv = stackalloc byte[ InitializationVectorLength ];

            aes.DecryptEcb( input[ ..iv.Length ], iv, PaddingMode.None );
            byte[] plainText = aes.DecryptCbc( input[ iv.Length.. ], iv, PaddingMode.PKCS7 );

            ValidateInitializationVector( plainText, iv );

            return plainText;
        }

        /// <summary>
        /// Performs an encryption using AES/CBC/PKCS7 with an input byte array and key, with a IV (comprised of random bytes and the HMAC-SHA1 of the random bytes and plaintext) prepended using AES/ECB/None
        /// </summary>
        int SymmetricEncryptWithHMACIV( Span<byte> input, byte[] output )
        {
            // IV is HMAC-SHA1(Random(3) + Plaintext) + Random(3). (Same random values for both)
            Span<byte> iv = stackalloc byte[ InitializationVectorLength ];

            GenerateInitializationVector( input, iv );

            var outputSpan = output.AsSpan();

            // final output is 16 byte ecb crypted IV + cbc crypted plaintext
            var cryptedIvLength = aes.EncryptEcb( iv, outputSpan, PaddingMode.None );
            var cipherTextLength = aes.EncryptCbc( input, iv, outputSpan[ cryptedIvLength.. ], PaddingMode.PKCS7 );

            return cryptedIvLength + cipherTextLength;
        }

        void GenerateInitializationVector( Span<byte> plainText, Span<byte> iv )
        {
            var hashLength = InitializationVectorLength - InitializationVectorRandomLength;
            RandomNumberGenerator.Fill( iv[ hashLength.. ] );

            var hmacBufferLength = plainText.Length + InitializationVectorRandomLength;
            var hmacBuffer = ArrayPool<byte>.Shared.Rent( hmacBufferLength );

            try
            {
                var hmacBufferSpan = hmacBuffer.AsSpan()[ ..hmacBufferLength ];

                // Random(3) + Plaintext
                iv[ ^InitializationVectorRandomLength.. ].CopyTo( hmacBufferSpan[ ..InitializationVectorRandomLength ] );
                plainText.CopyTo( hmacBufferSpan[ InitializationVectorRandomLength.. ] );

                Span<byte> hashValue = stackalloc byte[ HMACSHA1.HashSizeInBytes ];

                HMACSHA1.HashData( hmacSecret, hmacBufferSpan, hashValue );

                hashValue[ ..hashLength ].CopyTo( iv );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( hmacBuffer );
            }
        }

        void ValidateInitializationVector( Span<byte> plainText, Span<byte> iv )
        {
            var hashLength = InitializationVectorLength - InitializationVectorRandomLength;
            var hmacBufferLength = plainText.Length + InitializationVectorRandomLength;
            var hmacBuffer = ArrayPool<byte>.Shared.Rent( hmacBufferLength );

            try
            {
                var hmacBufferSpan = hmacBuffer.AsSpan()[ ..hmacBufferLength ];

                // Random(3) + Plaintext
                iv[ ^InitializationVectorRandomLength.. ].CopyTo( hmacBufferSpan[ ..InitializationVectorRandomLength ] );
                plainText.CopyTo( hmacBufferSpan[ InitializationVectorRandomLength.. ] );

                Span<byte> hashValue = stackalloc byte[ HMACSHA1.HashSizeInBytes ];

                HMACSHA1.HashData( hmacSecret, hmacBufferSpan, hashValue );

                if ( !hashValue[ ..hashLength ].SequenceEqual( iv[ ..hashLength ] ) )
                {
                    throw new CryptographicException( "HMAC from server did not match computed HMAC." );
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( hmacBuffer );
            }
        }

        public int CalculateMaxEncryptedDataLength( int plaintextDataLength )
        {
            int blockSize = aes.BlockSize / 8;
            int cipherTextSize = ( plaintextDataLength + blockSize ) / blockSize * blockSize;
            return InitializationVectorLength + cipherTextSize;
        }
    }
}
