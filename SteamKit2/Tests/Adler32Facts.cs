using System;
using SteamKit2;
using Xunit;

namespace Tests;

#if DEBUG
public class Adler32Facts
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void ReturnsCorrectWhenEmpty( uint input ) => Assert.Equal( input, Adler32.Calculate( input, default ) );

    [Theory]
    [InlineData( 0 )]
    [InlineData( 8 )]
    [InlineData( 215 )]
    [InlineData( 1024 )]
    [InlineData( 1024 + 15 )]
    [InlineData( 2034 )]
    [InlineData( 4096 )]
    public void MatchesReference( int length )
    {
        const uint Seed = 0;

        var data = new byte[ length ];
        Random.Shared.NextBytes( data );

        var expected = ReferenceImplementation( data );

        var actual = Adler32.Calculate( Seed, data );
        Assert.Equal( expected, actual );

        actual = Adler32.CalculateScalar( Seed, data );
        Assert.Equal( expected, actual );

        if ( System.Runtime.Intrinsics.X86.Avx2.IsSupported && length >= 64 )
        {
            actual = Adler32.CalculateAvx2( Seed, data );
            Assert.Equal( expected, actual );
        }

        if ( System.Runtime.Intrinsics.X86.Ssse3.IsSupported && length >= 64 )
        {
            actual = Adler32.CalculateSse( Seed, data );
            Assert.Equal( expected, actual );
        }

        if ( System.Runtime.Intrinsics.Arm.AdvSimd.IsSupported )
        {
            actual = Adler32.CalculateArm( Seed, data );
            Assert.Equal( expected, actual );
        }
    }

    private static uint ReferenceImplementation( ReadOnlySpan<byte> input )
    {
        uint a = 0, b = 0;
        for ( var i = 0; i < input.Length; i++ )
        {
            a = ( a + input[ i ] ) % 65521;
            b = ( b + a ) % 65521;
        }

        return a | ( b << 16 );
    }
}
#endif
