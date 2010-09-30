using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamKit
{
    class NetFilter
    {
        protected object input;

        public NetFilter( object input )
        {
            this.input = input;
        }

        public virtual MemoryStream ProcessIncoming( MemoryStream data )
        {
            data.Seek(0, SeekOrigin.Begin);
            return data;
        }

        public virtual byte[] ProcessOutgoing( byte[] data )
        {
            return data;
        }
    }

    class NetFilterEncryption : NetFilter
    {
        byte[] aesSessionKey { get { return ( byte[] )input; } }

        public NetFilterEncryption( object input )
            : base( input )
        {
        }

        public override MemoryStream ProcessIncoming( MemoryStream data )
        {
            return CryptoHelper.SymmetricDecrypt( data, aesSessionKey );
        }

        public override byte[] ProcessOutgoing( byte[] data )
        {
            return CryptoHelper.SymmetricEncrypt( data, aesSessionKey );
        }
    }
}
