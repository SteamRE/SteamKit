/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Runtime.CompilerServices;

namespace SteamKit2;

// See https://www.rfc-editor.org/rfc/rfc1950.html
internal static class Adler32
{
    // Largest prime smaller than 65536
    private const uint BASE = 65521;

    // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
    private const uint NMAX = 5552;

    /// <summary>
    /// Calculates the Adler32 checksum with the bytes taken from the span.
    /// </summary>
    /// <param name="adler">The input Adler32 value.</param>
    /// <param name="buffer">The readonly span of bytes.</param>
    /// <returns>The <see cref="uint"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static uint Calculate( uint adler, ReadOnlySpan<byte> buffer )
    {
        uint s1 = adler & 0xFFFF;
        uint s2 = ( adler >> 16 ) & 0xFFFF;

        while ( buffer.Length > 0 )
        {
            var k = buffer.Length < NMAX ? buffer.Length : (int)NMAX;
            var remaining = k;

            while ( remaining >= 16 )
            {
                s2 += s1 += buffer[ 0 ];
                s2 += s1 += buffer[ 1 ];
                s2 += s1 += buffer[ 2 ];
                s2 += s1 += buffer[ 3 ];
                s2 += s1 += buffer[ 4 ];
                s2 += s1 += buffer[ 5 ];
                s2 += s1 += buffer[ 6 ];
                s2 += s1 += buffer[ 7 ];
                s2 += s1 += buffer[ 8 ];
                s2 += s1 += buffer[ 9 ];
                s2 += s1 += buffer[ 10 ];
                s2 += s1 += buffer[ 11 ];
                s2 += s1 += buffer[ 12 ];
                s2 += s1 += buffer[ 13 ];
                s2 += s1 += buffer[ 14 ];
                s2 += s1 += buffer[ 15 ];
                buffer = buffer[ 16.. ];
                remaining -= 16;
            }

            for ( var i = 0; i < remaining; i++ )
            {
                s2 += s1 += buffer[ i ];
            }

            buffer = buffer[ remaining.. ];
            s1 %= BASE;
            s2 %= BASE;
        }

        return ( s2 << 16 ) | s1;
    }
}
