using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Steam3Lib
{
    class PsuedoRandom : Singleton<PsuedoRandom>
    {
        RandomNumberGenerator rng;

        public PsuedoRandom()
        {
            rng = RNGCryptoServiceProvider.Create();

        }

        public uint GetRandomInt()
        {
            byte[] data = new byte[ 4 ];
            rng.GetBytes( data );

            return BitConverter.ToUInt32( data, 0 );
        }

        public uint GetRandomInt( int max )
        {
            uint rand = GetRandomInt();

            return ( uint )( ( float )( max * rand ) / ( float )uint.MaxValue );
        }

        public uint GetRandomInt( int min, int max )
        {
            uint rand = GetRandomInt();

            return ( uint )( ( min + ( ( max - min + 1 ) * rand ) ) / ( float )uint.MaxValue );
        }

    }
}
