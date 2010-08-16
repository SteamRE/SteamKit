using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLib
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
            // first 16 bytes are the iv
            byte[] iv = new byte[ 16 ];
            Array.Copy( data, 0, iv, 0, iv.Length );

            // rest is ciphertext
            byte[] cipherText = new byte[ data.Length - iv.Length ];
            Array.Copy( data, iv.Length, cipherText, 0, cipherText.Length );

            return CryptoHelper.AESDecrypt( cipherText, aesSessionKey, iv );
        }

        public override byte[] ProcessOutgoing( byte[] data )
        {
            byte[] iv = CryptoHelper.GenerateRandomBlock( 16 );
            byte[] cipherText = CryptoHelper.AESEncrypt( data, aesSessionKey, iv );

            // combine everything
            byte[] output = new byte[ iv.Length + data.Length ];


            Array.Copy( iv, 0, output, 0, iv.Length );
            Array.Copy( cipherText, 0, output, iv.Length, cipherText.Length );

            return output;
        }
    }
}
