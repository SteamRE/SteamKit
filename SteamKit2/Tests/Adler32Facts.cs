using System;
using System.Text;
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
    [InlineData( 15 )]
    [InlineData( 16 )]
    [InlineData( 17 )]
    [InlineData( 215 )]
    [InlineData( 1024 )]
    [InlineData( 1024 + 15 )]
    [InlineData( 2034 )]
    [InlineData( 4096 )]
    [InlineData( 5552 - 1 )]
    [InlineData( 5552 )]
    [InlineData( 5552 + 1 )]
    [InlineData( 5552 + 16 )]
    [InlineData( 5552 * 2 )]
    public void MatchesReference( int length )
    {
        const uint Seed = 0;

        var data = new byte[ length ];
        Random.Shared.NextBytes( data );

        var expected = ReferenceImplementation( Seed, data );
        var actual = Adler32.Calculate( Seed, data );
        Assert.Equal( expected, actual );
    }

    [Theory]
    [InlineData( 1, "a", 0x00620062 )]
    [InlineData( 1, "Wikipedia", 0x11E60398 )]
    [InlineData( 1, "123456789", 0x091e01de )]
    [InlineData( 1, "SteamKit is good software, you should use it. :)", 0xaf8110e6 )]
    public void MatchesKnownVectors( uint seed, string input, uint expected )
    {
        var data = Encoding.ASCII.GetBytes( input );
        var actual = Adler32.Calculate( seed, data );
        Assert.Equal( expected, actual );
    }

    [Fact]
    public void NoOverflowWithMaxBytes()
    {
        var data = new byte[ 5552 ];
        Array.Fill( data, (byte)0xFF );

        var expected = ReferenceImplementation( 1, data );
        var actual = Adler32.Calculate( 1, data );
        Assert.Equal( expected, actual );
    }

    private static uint ReferenceImplementation( uint seed, ReadOnlySpan<byte> input )
    {
        uint a = seed, b = 0;
        for ( var i = 0; i < input.Length; i++ )
        {
            a = ( a + input[ i ] ) % 65521;
            b = ( b + a ) % 65521;
        }

        return a | ( b << 16 );
    }
}
#endif
