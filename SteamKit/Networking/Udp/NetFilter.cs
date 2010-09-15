using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit
{
    class NetFilter
    {
        protected object input;

        public NetFilter( object input )
        {
            this.input = input;
        }

        public virtual byte[] ProcessIncoming( byte[] data )
        {
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

        public override byte[] ProcessIncoming( byte[] data )
        {
            byte[] output = CryptoHelper.SymmetricDecrypt( data, aesSessionKey );

            return output;
        }

        public override byte[] ProcessOutgoing( byte[] data )
        {
            byte[] output = CryptoHelper.SymmetricEncrypt( data, aesSessionKey );

            return output;

        }
    }
}
