/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.IO;
using System.Security.Cryptography;

namespace SteamKit2
{
    class NetFilterEncryptionWithHMAC : INetFilterEncryption
    {
        readonly byte[] sessionKey;
        readonly byte[] hmacSecret;
        readonly ILogContext log;

        public NetFilterEncryptionWithHMAC( byte[] sessionKey, ILogContext log )
        {
            DebugLog.Assert( sessionKey.Length == 32, nameof(NetFilterEncryption), "AES session key was not 32 bytes!" );

            this.sessionKey = sessionKey;
            this.log = log ?? throw new ArgumentNullException( nameof( log ) );
            this.hmacSecret = new byte[ 16 ];
            Array.Copy( sessionKey, 0, hmacSecret, 0, hmacSecret.Length );
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            try
            {
                return CryptoHelper.SymmetricDecryptHMACIV( data, sessionKey, hmacSecret );
            }
            catch ( CryptographicException ex )
            {
                log.LogDebug( nameof(NetFilterEncryptionWithHMAC), "Unable to decrypt incoming packet: " + ex.Message );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
        }

        public byte[] ProcessOutgoing( byte[] data )
        {
            return CryptoHelper.SymmetricEncryptWithHMACIV( data, sessionKey, hmacSecret );
        }
    }
}
