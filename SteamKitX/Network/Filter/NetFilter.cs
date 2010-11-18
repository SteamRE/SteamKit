using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamKit
{
    class NetFilterEncryption
    {
        private byte[] aesSessionKey;

        public NetFilterEncryption(byte[] key)
        {
            aesSessionKey = key;
        }

        public MemoryStream ProcessIncoming(MemoryStream data)
        {
            return CryptoHelper.SymmetricDecrypt(data, aesSessionKey);
        }

        public byte[] ProcessOutgoing(byte[] data)
        {
            return CryptoHelper.SymmetricEncrypt(data, aesSessionKey);
        }
    }
}
