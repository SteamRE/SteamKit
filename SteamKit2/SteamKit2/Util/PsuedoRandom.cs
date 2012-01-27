/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace SteamKit2
{
    static class PsuedoRandom
    {
        static RandomNumberGenerator rng;

        static PsuedoRandom()
        {
            rng = RNGCryptoServiceProvider.Create();

        }

        public static byte[] GenerateRandomBlock( int size )
        {
            byte[] block = new byte[ size ];
            rng.GetNonZeroBytes( block );

            return block;
        }

        public static uint GetRandomInt()
        {
            byte[] data = GenerateRandomBlock( 4 );

            return BitConverter.ToUInt32( data, 0 );
        }

        public static uint GetRandomInt( int max )
        {
            uint rand = GetRandomInt();

            return ( uint )( ( float )( max * rand ) / ( float )uint.MaxValue );
        }

        public static uint GetRandomInt( int min, int max )
        {
            uint rand = GetRandomInt();

            return ( uint )( ( min + ( ( max - min + 1 ) * rand ) ) / ( float )uint.MaxValue );
        }

    }
}
