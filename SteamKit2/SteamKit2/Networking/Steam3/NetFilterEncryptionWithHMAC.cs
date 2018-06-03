/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using SteamKit2.Util;

namespace SteamKit2
{
    class NetFilterEncryptionWithHMAC : INetFilterEncryption
    {
        readonly CryptographicContext context;

        public NetFilterEncryptionWithHMAC( byte[] sessionKey )
        {
            DebugLog.Assert( sessionKey.Length == 32, nameof(NetFilterEncryption), "AES session key was not 32 bytes!" );

            var hmacSecret = new byte[ 16 ];
            Array.Copy( sessionKey, 0, hmacSecret, 0, hmacSecret.Length );

            this.context = new CryptographicContext(sessionKey, hmacSecret);
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            var buffer = ArrayPool<byte>.Shared.Rent(data.Length);
            try
            {
                var segment = context.SymmetricDecryptWithIVHMAC(data, buffer);

                var incoming = new byte[segment.Count];
                Array.Copy(segment.Array, segment.Offset, incoming, 0, incoming.Length);
                return incoming;
            }
            catch ( CryptographicException ex )
            {
                DebugLog.WriteLine( nameof(NetFilterEncryptionWithHMAC), "Unable to decrypt incoming packet: " + ex.Message );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public byte[] ProcessOutgoing( byte[] data )
        {
            var buffer = ArrayPool<byte>.Shared.Rent(context.CalculateMaxEncryptedDataLength(data.Length));
            try
            {
                var segment = context.SymmetricEncryptWithIVHMAC(data, buffer);

                var outgoing = new byte[segment.Count];
                Array.Copy(segment.Array, segment.Offset, outgoing, 0, outgoing.Length);
                return outgoing;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
