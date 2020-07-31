/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.IO;
using System.Security.Cryptography;

namespace SteamKit2
{
    class NetFilterEncryption : INetFilterEncryption
    {
        readonly byte[] sessionKey;
        readonly ILogContext log;

        public NetFilterEncryption( byte[] sessionKey, ILogContext log )
        {
            DebugLog.Assert( sessionKey.Length == 32, nameof(NetFilterEncryption), "AES session key was not 32 bytes!" );

            this.sessionKey = sessionKey;
            this.log = log ?? throw new ArgumentNullException( nameof( log ) );
        }

        public byte[] ProcessIncoming( byte[] data )
        {
            try
            {
                return CryptoHelper.SymmetricDecrypt( data, sessionKey );
            }
            catch ( CryptographicException ex )
            {
                log.LogDebug( nameof(NetFilterEncryption), "Unable to decrypt incoming packet: " + ex.Message );

                // rethrow as an IO exception so it's handled in the network thread
                throw new IOException( "Unable to decrypt incoming packet", ex );
            }
        }

        public byte[] ProcessOutgoing( byte[] data )
        {
            return CryptoHelper.SymmetricEncrypt( data, sessionKey );
        }
    }
}
