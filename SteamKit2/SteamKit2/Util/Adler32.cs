/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SteamKit2;

// See https://www.rfc-editor.org/rfc/rfc1950.html
internal static class Adler32
{
    // Largest prime smaller than 65536
    private const uint BASE = 65521;

    // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
    private const uint NMAX = 5552;

    // The vectorized paths consume 32 bytes per block, and reduce modulo BASE after as many whole blocks as fit in NMAX bytes
    private const int BlockSize = 32;
    private const int BlocksPerReduction = ( int )NMAX / BlockSize;

    /// <summary>
    /// Calculates the Adler32 checksum with the bytes taken from the span.
    /// </summary>
    /// <param name="adler">The input Adler32 value.</param>
    /// <param name="buffer">The readonly span of bytes.</param>
    /// <returns>The <see cref="uint"/>.</returns>
    public static uint Calculate( uint adler, ReadOnlySpan<byte> buffer )
    {
        if ( buffer.Length >= BlockSize )
        {
            if ( Vector256.IsHardwareAccelerated )
            {
                return CalculateVector256( adler, buffer );
            }

            if ( Vector128.IsHardwareAccelerated )
            {
                return CalculateVector128( adler, buffer );
            }
        }

        return CalculateScalar( adler, buffer );
    }

    // Each 32 byte block adds 32 * s1 to s2, plus the block's bytes weighted 32 down to 1.
    // The vectors keep per-lane sums of the bytes (for s1), of the weighted bytes (for s2),
    // and of s1's lanes before each block (for the 32 * s1 term), which are combined at each reduction.
    internal static uint CalculateVector256( uint adler, ReadOnlySpan<byte> buffer )
    {
        uint s1 = adler & 0xFFFF;
        uint s2 = ( adler >> 16 ) & 0xFFFF;

        var weightsLower = Vector256.Create( ( ushort )32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17 );
        var weightsUpper = Vector256.Create( ( ushort )16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 );

        ref var data = ref MemoryMarshal.GetReference( buffer );
        var offset = 0;
        var blocksRemaining = buffer.Length / BlockSize;

        while ( blocksRemaining > 0 )
        {
            var blockCount = Math.Min( blocksRemaining, BlocksPerReduction );
            blocksRemaining -= blockCount;

            s2 += s1 * BlockSize * ( uint )blockCount;

            var byteSums = Vector256<uint>.Zero;
            var weightedSums = Vector256<uint>.Zero;
            var previousByteSums = Vector256<uint>.Zero;

            for ( var i = 0; i < blockCount; i++ )
            {
                var bytes = Vector256.LoadUnsafe( ref data, ( nuint )offset );
                offset += BlockSize;

                previousByteSums += byteSums;

                var (lower, upper) = Vector256.Widen( bytes );

                var (byteSumsLower, byteSumsUpper) = Vector256.Widen( lower + upper );
                byteSums += byteSumsLower + byteSumsUpper;

                var weighted = ( lower * weightsLower ) + ( upper * weightsUpper );
                var (weightedLower, weightedUpper) = Vector256.Widen( weighted );
                weightedSums += weightedLower + weightedUpper;
            }

            s1 += Vector256.Sum( byteSums );
            s2 += Vector256.Sum( weightedSums ) + ( Vector256.Sum( previousByteSums ) * BlockSize );

            s1 %= BASE;
            s2 %= BASE;
        }

        return CalculateScalar( ( s2 << 16 ) | s1, buffer[ offset.. ] );
    }

    internal static uint CalculateVector128( uint adler, ReadOnlySpan<byte> buffer )
    {
        uint s1 = adler & 0xFFFF;
        uint s2 = ( adler >> 16 ) & 0xFFFF;

        var weights0 = Vector128.Create( ( ushort )32, 31, 30, 29, 28, 27, 26, 25 );
        var weights1 = Vector128.Create( ( ushort )24, 23, 22, 21, 20, 19, 18, 17 );
        var weights2 = Vector128.Create( ( ushort )16, 15, 14, 13, 12, 11, 10, 9 );
        var weights3 = Vector128.Create( ( ushort )8, 7, 6, 5, 4, 3, 2, 1 );

        ref var data = ref MemoryMarshal.GetReference( buffer );
        var offset = 0;
        var blocksRemaining = buffer.Length / BlockSize;

        while ( blocksRemaining > 0 )
        {
            var blockCount = Math.Min( blocksRemaining, BlocksPerReduction );
            blocksRemaining -= blockCount;

            s2 += s1 * BlockSize * ( uint )blockCount;

            var byteSums = Vector128<uint>.Zero;
            var weightedSums = Vector128<uint>.Zero;
            var previousByteSums = Vector128<uint>.Zero;

            for ( var i = 0; i < blockCount; i++ )
            {
                var first = Vector128.LoadUnsafe( ref data, ( nuint )offset );
                var second = Vector128.LoadUnsafe( ref data, ( nuint )offset + 16 );
                offset += BlockSize;

                previousByteSums += byteSums;

                var (bytes0, bytes1) = Vector128.Widen( first );
                var (bytes2, bytes3) = Vector128.Widen( second );

                var (byteSumsLower, byteSumsUpper) = Vector128.Widen( bytes0 + bytes1 + bytes2 + bytes3 );
                byteSums += byteSumsLower + byteSumsUpper;

                var weighted = ( bytes0 * weights0 ) + ( bytes1 * weights1 ) + ( bytes2 * weights2 ) + ( bytes3 * weights3 );
                var (weightedLower, weightedUpper) = Vector128.Widen( weighted );
                weightedSums += weightedLower + weightedUpper;
            }

            s1 += Vector128.Sum( byteSums );
            s2 += Vector128.Sum( weightedSums ) + ( Vector128.Sum( previousByteSums ) * BlockSize );

            s1 %= BASE;
            s2 %= BASE;
        }

        return CalculateScalar( ( s2 << 16 ) | s1, buffer[ offset.. ] );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static uint CalculateScalar( uint adler, ReadOnlySpan<byte> buffer )
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
