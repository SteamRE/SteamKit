/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    /// <summary>
    /// Decodes raw LZMA streams whose uncompressed size is known up front.
    /// </summary>
    /// <remarks>
    /// The output buffer doubles as the dictionary, so the whole output is decoded in one call.
    /// </remarks>
    static class LzmaDecoder
    {
        /// <summary>
        /// Size of the properties that precede an LZMA stream: one byte for lc/lp/pb and four for the dictionary size.
        /// </summary>
        public const int PropertiesSize = 5;

        private const int StateCount = 12;
        private const int LiteralStateCount = 7;
        private const int PosStateBitsMax = 4;
        private const int LiteralCoderSize = 0x300;

        private const int EndPosModelIndex = 14;
        private const int FullDistanceCount = 1 << ( EndPosModelIndex >> 1 );
        private const int AlignBits = 4;

        // Length coder: two choice bits, then a 3-bit low tree and a 3-bit mid tree per pos state, then an 8-bit high tree.
        private const int LengthChoice = 0;
        private const int LengthChoice2 = 1;
        private const int LengthLow = 2;
        private const int LengthMid = LengthLow + ( 8 << PosStateBitsMax );
        private const int LengthHigh = LengthMid + ( 8 << PosStateBitsMax );
        private const int LengthCoderSize = LengthHigh + 256;

        // Offsets into the single probability array.
        private const int IsMatch = 0;
        private const int IsRep = IsMatch + ( StateCount << PosStateBitsMax );
        private const int IsRepG0 = IsRep + StateCount;
        private const int IsRepG1 = IsRepG0 + StateCount;
        private const int IsRepG2 = IsRepG1 + StateCount;
        private const int IsRep0Long = IsRepG2 + StateCount;
        private const int PosSlot = IsRep0Long + ( StateCount << PosStateBitsMax );
        private const int SpecPos = PosSlot + ( 4 << 6 );
        private const int Align = SpecPos + FullDistanceCount - EndPosModelIndex;
        private const int MatchLength = Align + ( 1 << AlignBits );
        private const int RepLength = MatchLength + LengthCoderSize;
        private const int Literal = RepLength + LengthCoderSize;

        // Properties with lc + lp up to 4, which includes the default lc=3 lp=0, keep the probabilities on the stack
        private const int MaxStackProbabilityCount = Literal + ( LiteralCoderSize << 4 );

        /// <summary>
        /// Decodes a raw LZMA stream into <paramref name="output"/>, after an optional preset dictionary at its start.
        /// </summary>
        /// <param name="properties">The 5 property bytes.</param>
        /// <param name="input">The compressed stream.</param>
        /// <param name="output">Buffer that starts with the preset dictionary, and receives uncompressed bytes in the rest of its length.</param>
        /// <param name="presetDictionaryLength">Number of bytes at the start of <paramref name="output"/> that matches can refer to before any are decoded.</param>
        /// <exception cref="InvalidDataException">The properties or the stream are invalid.</exception>
        [SkipLocalsInit]
        public static void Decode( ReadOnlySpan<byte> properties, ReadOnlySpan<byte> input, Span<byte> output, int presetDictionaryLength = 0 )
        {
            ArgumentOutOfRangeException.ThrowIfNegative( presetDictionaryLength );
            ArgumentOutOfRangeException.ThrowIfGreaterThan( presetDictionaryLength, output.Length );

            if ( properties.Length < PropertiesSize )
            {
                ThrowInvalidData( "LZMA properties are too short" );
            }

            var propertiesByte = properties[ 0 ];

            if ( propertiesByte >= 9 * 5 * 5 )
            {
                ThrowInvalidData( "Invalid LZMA properties" );
            }

            var lc = propertiesByte % 9;
            var lp = propertiesByte / 9 % 5;
            var pb = propertiesByte / ( 9 * 5 );

            // The dictionary size is not needed, because the output buffer holds every byte a match can refer to
            var probabilityCount = Literal + ( LiteralCoderSize << ( lc + lp ) );

            if ( probabilityCount <= MaxStackProbabilityCount )
            {
                Span<ushort> probabilities = stackalloc ushort[ MaxStackProbabilityCount ];
                DecodeStream( probabilities[ ..probabilityCount ], input, output, presetDictionaryLength, lc, lp, pb );
                return;
            }

            var rented = ArrayPool<ushort>.Shared.Rent( probabilityCount );

            try
            {
                DecodeStream( rented.AsSpan( 0, probabilityCount ), input, output, presetDictionaryLength, lc, lp, pb );
            }
            finally
            {
                ArrayPool<ushort>.Shared.Return( rented );
            }
        }

        private static void DecodeStream( Span<ushort> probabilities, ReadOnlySpan<byte> input, Span<byte> output, int position, int lc, int lp, int pb )
        {
            probabilities.Fill( RangeDecoder.InitialProbability );

            ref var probs = ref MemoryMarshal.GetReference( probabilities );
            var rangeDecoder = new RangeDecoder( input );

            var posStateMask = ( 1 << pb ) - 1;
            var literalPosMask = ( 1 << lp ) - 1;
            var literalShift = 8 - lc;

            var state = 0;
            var rep0 = 0u;
            var rep1 = 0u;
            var rep2 = 0u;
            var rep3 = 0u;

            while ( position < output.Length )
            {
                var posState = position & posStateMask;
                var stateIndex = ( state << PosStateBitsMax ) + posState;

                if ( rangeDecoder.DecodeBit( ref Unsafe.Add( ref probs, IsMatch + stateIndex ) ) == 0 )
                {
                    var previousByte = position > 0 ? output[ position - 1 ] : 0;
                    var literalState = ( ( position & literalPosMask ) << lc ) + ( previousByte >> literalShift );
                    ref var literalProbs = ref Unsafe.Add( ref probs, Literal + ( LiteralCoderSize * literalState ) );

                    if ( state < LiteralStateCount )
                    {
                        output[ position ] = rangeDecoder.DecodeLiteral( ref literalProbs );
                    }
                    else
                    {
                        var matchByte = output[ position - ( int )rep0 - 1 ];
                        output[ position ] = rangeDecoder.DecodeMatchedLiteral( ref literalProbs, matchByte );
                    }

                    position++;
                    state = GetStateAfterLiteral( state );
                    continue;
                }

                uint length;

                if ( rangeDecoder.DecodeBit( ref Unsafe.Add( ref probs, IsRep + state ) ) == 0 )
                {
                    rep3 = rep2;
                    rep2 = rep1;
                    rep1 = rep0;

                    length = rangeDecoder.DecodeLength( ref Unsafe.Add( ref probs, MatchLength ), posState );
                    state = state < LiteralStateCount ? 7 : 10;
                    rep0 = rangeDecoder.DecodeDistance( ref probs, length );

                    if ( rep0 == uint.MaxValue )
                    {
                        // End marker; decoding stops at the end of the output before a valid one can be read
                        ThrowInvalidData( "LZMA stream ended before the expected uncompressed size" );
                    }

                    if ( rep0 >= position )
                    {
                        ThrowInvalidData( "LZMA match distance is outside of the decoded data" );
                    }
                }
                else
                {
                    if ( position == 0 )
                    {
                        ThrowInvalidData( "LZMA stream starts with a repeated match" );
                    }

                    if ( rangeDecoder.DecodeBit( ref Unsafe.Add( ref probs, IsRepG0 + state ) ) == 0 )
                    {
                        if ( rangeDecoder.DecodeBit( ref Unsafe.Add( ref probs, IsRep0Long + stateIndex ) ) == 0 )
                        {
                            output[ position ] = output[ position - ( int )rep0 - 1 ];
                            position++;
                            state = state < LiteralStateCount ? 9 : 11;
                            continue;
                        }
                    }
                    else
                    {
                        uint distance;

                        if ( rangeDecoder.DecodeBit( ref Unsafe.Add( ref probs, IsRepG1 + state ) ) == 0 )
                        {
                            distance = rep1;
                        }
                        else
                        {
                            if ( rangeDecoder.DecodeBit( ref Unsafe.Add( ref probs, IsRepG2 + state ) ) == 0 )
                            {
                                distance = rep2;
                            }
                            else
                            {
                                distance = rep3;
                                rep3 = rep2;
                            }

                            rep2 = rep1;
                        }

                        rep1 = rep0;
                        rep0 = distance;
                    }

                    length = rangeDecoder.DecodeLength( ref Unsafe.Add( ref probs, RepLength ), posState );
                    state = state < LiteralStateCount ? 8 : 11;
                }

                // A match that runs past the end of the output is cut short
                var matchLength = Math.Min( ( int )length + 2, output.Length - position );
                var matchDistance = ( int )rep0 + 1;

                // Both ends are in range: the distance was checked against the position and the length against the output
                ref var destination = ref Unsafe.Add( ref MemoryMarshal.GetReference( output ), position );
                ref var source = ref Unsafe.Subtract( ref destination, matchDistance );

                if ( matchDistance >= matchLength )
                {
                    Unsafe.CopyBlockUnaligned( ref destination, ref source, ( uint )matchLength );
                }
                else if ( matchDistance == 1 )
                {
                    Unsafe.InitBlockUnaligned( ref destination, source, ( uint )matchLength );
                }
                else
                {
                    // The match overlaps the bytes it produces, so it has to be copied forwards one byte at a time
                    for ( var i = 0; i < matchLength; i++ )
                    {
                        Unsafe.Add( ref destination, i ) = Unsafe.Add( ref source, i );
                    }
                }

                position += matchLength;
            }
        }

        private static int GetStateAfterLiteral( int state )
        {
            if ( state < 4 )
            {
                return 0;
            }

            if ( state < 10 )
            {
                return state - 3;
            }

            return state - 6;
        }

        [DoesNotReturn]
        private static void ThrowInvalidData( string message ) => throw new InvalidDataException( message );

        private ref struct RangeDecoder
        {
            public const ushort InitialProbability = 1 << ( BitModelTotalBits - 1 );

            private const int InitialBytes = 5;
            private const int BitModelTotalBits = 11;
            private const int MoveBits = 5;
            private const uint TopValue = 1 << 24;
            private const uint ProbabilityZeroOffset = unchecked( ( 1u << MoveBits ) - 1 - ( 1u << BitModelTotalBits ) );

            private readonly ReadOnlySpan<byte> Input;
            private int Position;
            private uint Range;
            private uint Code;

            public RangeDecoder( ReadOnlySpan<byte> input )
            {
                // The first byte is always written as 0 by the encoder and is skipped
                if ( input.Length < InitialBytes )
                {
                    ThrowInvalidData( "LZMA stream is truncated" );
                }

                Input = input;
                Position = InitialBytes;
                Range = uint.MaxValue;
                Code = BinaryPrimitives.ReadUInt32BigEndian( input[ 1.. ] );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public uint DecodeBit( ref ushort probability )
            {
                var prob = ( uint )probability;
                var bound = ( Range >> BitModelTotalBits ) * prob;
                uint bit;

                if ( Code < bound )
                {
                    Range = bound;
                    probability = ( ushort )( prob + ( ( ( 1u << BitModelTotalBits ) - prob ) >> MoveBits ) );
                    bit = 0;
                }
                else
                {
                    Range -= bound;
                    Code -= bound;
                    probability = ( ushort )( prob - ( prob >> MoveBits ) );
                    bit = 1;
                }

                if ( Range < TopValue )
                {
                    Normalize();
                }

                return bit;
            }

            /// <summary>
            /// Same as <see cref="DecodeBit"/>, but selects the results with masks instead of branching.
            /// Bits inside trees are close to random, so a branch on them is often mispredicted.
            /// </summary>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private uint DecodeTreeBit( ref ushort probability )
            {
                var prob = ( uint )probability;
                var bound = ( Range >> BitModelTotalBits ) * prob;

                // All ones when the bit is 0, all zeroes when it is 1
                var zeroMask = 0u - ( uint )( ( ( ulong )Code - bound ) >> 63 );
                var oneMask = ~zeroMask;

                Range = ( bound & zeroMask ) | ( ( Range - bound ) & oneMask );
                Code -= bound & oneMask;

                // For a 1 this subtracts prob >> 5. For a 0 the offset makes the sum wrap below zero,
                // so after truncating to 16 bits the subtraction adds (2048 - prob) >> 5 instead.
                var adjusted = prob + ( zeroMask & ProbabilityZeroOffset );
                probability = ( ushort )( prob - ( adjusted >> MoveBits ) );

                if ( Range < TopValue )
                {
                    Normalize();
                }

                return oneMask & 1;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public byte DecodeLiteral( ref ushort probs )
            {
                var symbol = 1u;

                while ( symbol < 0x100 )
                {
                    symbol = ( symbol << 1 ) | DecodeTreeBit( ref Unsafe.Add( ref probs, symbol ) );
                }

                return ( byte )symbol;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public byte DecodeMatchedLiteral( ref ushort probs, uint matchByte )
            {
                var symbol = 1u;

                // While the decoded bits agree with the match byte, each bit uses the probabilities selected by the match bit.
                // The offset is 0x100 while they agree and drops to 0 at the first difference, which selects the plain probabilities.
                var offset = 0x100u;

                while ( symbol < 0x100 )
                {
                    matchByte <<= 1;

                    var previousOffset = offset;
                    offset &= matchByte;

                    var bit = DecodeTreeBit( ref Unsafe.Add( ref probs, previousOffset + offset + symbol ) );
                    symbol = ( symbol << 1 ) | bit;

                    // Leaves 0x100 only when the decoded bit equals the match bit
                    offset ^= previousOffset & ( bit - 1 );
                }

                return ( byte )symbol;
            }

            /// <summary>
            /// Decodes a match length, minus the minimum match length of 2.
            /// </summary>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public uint DecodeLength( ref ushort probs, int posState )
            {
                if ( DecodeBit( ref Unsafe.Add( ref probs, LengthChoice ) ) == 0 )
                {
                    return DecodeBitTree( ref Unsafe.Add( ref probs, LengthLow + ( posState << 3 ) ), 3 );
                }

                if ( DecodeBit( ref Unsafe.Add( ref probs, LengthChoice2 ) ) == 0 )
                {
                    return 8 + DecodeBitTree( ref Unsafe.Add( ref probs, LengthMid + ( posState << 3 ) ), 3 );
                }

                return 16 + DecodeBitTree( ref Unsafe.Add( ref probs, LengthHigh ), 8 );
            }

            /// <summary>
            /// Decodes a match distance, minus one. Returns <see cref="uint.MaxValue"/> for the end marker.
            /// </summary>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public uint DecodeDistance( ref ushort probs, uint length )
            {
                var lengthState = Math.Min( length, 3 );
                var posSlot = DecodeBitTree( ref Unsafe.Add( ref probs, PosSlot + ( int )( lengthState << 6 ) ), 6 );

                if ( posSlot < 4 )
                {
                    return posSlot;
                }

                var directBits = ( int )( posSlot >> 1 ) - 1;
                var distance = ( 2 | ( posSlot & 1 ) ) << directBits;

                if ( posSlot < EndPosModelIndex )
                {
                    // Trees for these slots are packed back to back, each starting one before its first used index.
                    ref var tree = ref Unsafe.Add( ref probs, SpecPos + ( int )distance - ( int )posSlot - 1 );
                    return distance + DecodeReverseBitTree( ref tree, directBits );
                }

                distance += DecodeDirectBits( directBits - AlignBits ) << AlignBits;
                return distance + DecodeReverseBitTree( ref Unsafe.Add( ref probs, Align ), AlignBits );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private uint DecodeBitTree( ref ushort probs, int bitCount )
            {
                var symbol = 1u;

                for ( var i = 0; i < bitCount; i++ )
                {
                    symbol = ( symbol << 1 ) | DecodeTreeBit( ref Unsafe.Add( ref probs, symbol ) );
                }

                return symbol - ( 1u << bitCount );
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private uint DecodeReverseBitTree( ref ushort probs, int bitCount )
            {
                var index = 1u;
                var symbol = 0u;

                for ( var i = 0; i < bitCount; i++ )
                {
                    var bit = DecodeTreeBit( ref Unsafe.Add( ref probs, index ) );
                    index = ( index << 1 ) | bit;
                    symbol |= bit << i;
                }

                return symbol;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private uint DecodeDirectBits( int bitCount )
            {
                var result = 0u;

                for ( var i = 0; i < bitCount; i++ )
                {
                    Range >>= 1;

                    // All ones when the bit is 0, all zeroes when it is 1.
                    var mask = 0u - ( ( Code - Range ) >> 31 );
                    Code -= Range & ~mask;
                    result = ( result << 1 ) | ( ~mask & 1 );

                    if ( Range < TopValue )
                    {
                        Normalize();
                    }
                }

                return result;
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private void Normalize()
            {
                if ( ( uint )Position >= ( uint )Input.Length )
                {
                    ThrowInvalidData( "LZMA stream is truncated" );
                }

                Range <<= 8;
                Code = ( Code << 8 ) | Unsafe.Add( ref MemoryMarshal.GetReference( Input ), Position );
                Position++;
            }
        }
    }
}
