using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Hashing;
using SteamKit2;
using SteamKit2.CDN;
using SteamKit2.Internal;
using Xunit;

namespace Tests;

#if DEBUG
public class LzmaFacts
{
    static byte[] ReadDepotChunkContents()
    {
        var chunkData = DepotChunkFacts.ReadChunkFile( "depot_4_chunk_dc036a660987d8f705936d4443fedb5e1d2e5d9b.bin" );
        var destination = new byte[ DepotChunkFacts.Depot4Chunk.UncompressedLength ];
        DepotChunk.Process( DepotChunkFacts.Depot4Chunk, chunkData, destination, DepotChunkFacts.Depot4Key );
        return destination;
    }

    static SevenZip.Compression.LZMA.Encoder CreateEncoder( int dictionarySize = 1 << 20 )
    {
        var encoder = new SevenZip.Compression.LZMA.Encoder();
        encoder.SetCoderProperties(
            [
                SevenZip.CoderPropID.DictionarySize,
                SevenZip.CoderPropID.PosStateBits,
                SevenZip.CoderPropID.LitContextBits,
                SevenZip.CoderPropID.LitPosBits,
                SevenZip.CoderPropID.EndMarker,
            ],
            [
                dictionarySize,
                2,
                3,
                0,
                false,
            ]
        );
        return encoder;
    }

    // The encoder uses zero as the byte before its first literal, while the delta decoder uses the last byte of the source.
    // Both select the same literal coder when that byte is below 1 << (8 - lc).
    static byte[] CreateDeltaSource( byte[] contents )
    {
        var length = contents.Length;

        while ( contents[ length - 1 ] >= 32 )
        {
            length--;
        }

        return contents[ ..length ];
    }

    static byte[] CreateDeltaTarget( byte[] source )
    {
        var target = new byte[ source.Length + 1000 ];
        source.AsSpan( 0, 20000 ).CopyTo( target );
        new Random( 1950 ).NextBytes( target.AsSpan( 20000, 1000 ) );
        source.AsSpan( 20000 ).CopyTo( target.AsSpan( 21000 ) );
        target[ ^1 ] ^= 0xFF;
        return target;
    }

    static byte[] CreateVZipDelta( byte[] source, byte[] target, int dictionarySize = 1 << 20 )
    {
        var crc = Crc32.HashToUInt32( target );

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter( ms );
        writer.Write( ( ushort )0x5A56 );
        writer.Write( ( byte )'d' );
        writer.Write( crc );

        var encoder = CreateEncoder( dictionarySize );
        encoder.SetTrainSize( ( uint )source.Length );
        encoder.WriteCoderProperties( ms );

        var input = new byte[ source.Length + target.Length ];
        source.CopyTo( input, 0 );
        target.CopyTo( input, source.Length );

        using ( var inputStream = new MemoryStream( input ) )
        {
            encoder.Code( inputStream, ms, -1, -1, null );
        }

        writer.Write( crc );
        writer.Write( ( uint )target.Length );
        writer.Write( ( ushort )0x767A );

        return ms.ToArray();
    }

    [Fact]
    public void VZipDeltaDecodesAgainstSourceChunk()
    {
        var source = CreateDeltaSource( ReadDepotChunkContents() );
        var target = CreateDeltaTarget( source );
        var delta = CreateVZipDelta( source, target );

        // Most of the target comes from the source, so the delta is much smaller than the target
        Assert.True( delta.Length < target.Length / 10, $"Delta is {delta.Length} bytes" );

        var decompressed = VZipDeltaUtil.Decompress( delta, source );

        Assert.Equal( target, decompressed );
    }

    [Fact]
    public void VZipDeltaDecodesWithSourceChunkLargerThanDictionary()
    {
        const int DictionarySize = 1 << 14;

        var source = CreateDeltaSource( ReadDepotChunkContents() );
        Assert.True( source.Length > DictionarySize * 2 );

        // Repeats bytes from the end of the source that are within reach of the dictionary
        var target = new byte[ 10000 ];
        source.AsSpan( source.Length - target.Length ).CopyTo( target );
        target[ 100 ] ^= 0xFF;

        var delta = CreateVZipDelta( source, target, DictionarySize );
        Assert.True( delta.Length < target.Length / 10, $"Delta is {delta.Length} bytes" );

        var decompressed = VZipDeltaUtil.Decompress( delta, source );

        Assert.Equal( target, decompressed );
    }

    [Fact]
    public void VZipDeltaThrowsWithWrongSourceChunk()
    {
        var source = CreateDeltaSource( ReadDepotChunkContents() );
        var target = CreateDeltaTarget( source );
        var delta = CreateVZipDelta( source, target );

        var wrongSource = source.AsSpan().ToArray();
        wrongSource[ 100 ] ^= 0xFF;

        Assert.Throws<InvalidDataException>( () => VZipDeltaUtil.Decompress( delta, wrongSource ) );
    }

    [Fact]
    public void LzmaUtilDecompressesBlob()
    {
        var contents = ReadDepotChunkContents();

        using var compressedStream = new MemoryStream();
        CreateEncoder().Code( new MemoryStream( contents ), compressedStream, -1, -1, null );
        var compressed = compressedStream.ToArray();

        using var blob = new MemoryStream();
        var header = new byte[ 12 ];
        BinaryPrimitives.WriteUInt32BigEndian( header, 0x4C5A4D41 );
        BinaryPrimitives.WriteInt32LittleEndian( header.AsSpan( 4 ), contents.Length );
        BinaryPrimitives.WriteInt32LittleEndian( header.AsSpan( 8 ), compressed.Length );
        blob.Write( header );
        CreateEncoder().WriteCoderProperties( blob );
        blob.Write( compressed );

        Assert.True( LzmaUtil.HasLzmaHeader( blob.ToArray() ) );

        blob.Position = 0;
        Assert.True( LzmaUtil.TryDecompress( blob, static capacity => new MemoryStream( capacity ), out var output ) );

        using ( output )
        {
            Assert.Equal( 0, output.Position );
            Assert.Equal( contents, ( ( MemoryStream )output ).ToArray() );
        }
    }
}
#endif
