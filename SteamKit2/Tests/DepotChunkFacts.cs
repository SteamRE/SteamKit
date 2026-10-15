using System;
using System.IO;
using System.IO.Hashing;
using System.Reflection;
using System.Security.Cryptography;
using SteamKit2;
using SteamKit2.CDN;
using Xunit;

namespace Tests
{
    public class DepotChunkFacts
    {
        [Fact]
        public void DecryptsAndDecompressesDepotChunkPKZip()
        {
            var chunkData = ReadChunkFile( "depot_440_chunk_bac8e2657470b2eb70d6ddcd6c07004be8738697.bin" );

            var chunk = new DepotManifest.ChunkData(
                id: [], // id is not needed here
                checksum: 2130218374,
                offset: 0,
                comp_length: 320,
                uncomp_length: 544
            );

            var destination = new byte[ chunk.UncompressedLength ];
            var writtenLength = DepotChunk.Process( chunk, chunkData, destination, [
                0x44, 0xCE, 0x5C, 0x52, 0x97, 0xA4, 0x15, 0xA1,
                0xA6, 0xF6, 0x9C, 0x85, 0x60, 0x37, 0xA5, 0xA2,
                0xFD, 0xD8, 0x2C, 0xD4, 0x74, 0xFA, 0x65, 0x9E,
                0xDF, 0xB4, 0xD5, 0x9B, 0x2A, 0xBC, 0x55, 0xFC
            ] );

            Assert.Equal( chunk.CompressedLength, ( uint )chunkData.Length );
            Assert.Equal( chunk.UncompressedLength, ( uint )writtenLength );

            var hash = Convert.ToHexString( SHA1.HashData( destination ) );
            Assert.Equal( "BAC8E2657470B2EB70D6DDCD6C07004BE8738697", hash );
        }

        [Fact]
        public void DecryptsAndDecompressesDepotChunkVZip()
        {
            var chunkData = ReadChunkFile( "depot_232250_chunk_7b8567d9b3c09295cdbf4978c32b348d8e76c750.bin" );

            var chunk = new DepotManifest.ChunkData(
                id: [], // id is not needed here
                checksum: 2894626744,
                offset: 0,
                comp_length: 304,
                uncomp_length: 798
            );

            var destination = new byte[ chunk.UncompressedLength ];
            var writtenLength = DepotChunk.Process( chunk, chunkData, destination, [
                0xE5, 0xF6, 0xAE, 0xD5, 0x5E, 0x9E, 0xCE, 0x42,
                0x9E, 0x56, 0xB8, 0x13, 0xFB, 0xF6, 0xBF, 0xE9,
                0x24, 0xF3, 0xCF, 0x72, 0x97, 0x2F, 0xDB, 0xD0,
                0x57, 0x1F, 0xFC, 0xAD, 0x9F, 0x2F, 0x7D, 0xAA,
            ] );

            Assert.Equal( chunk.CompressedLength, ( uint )chunkData.Length );
            Assert.Equal( chunk.UncompressedLength, ( uint )writtenLength );

            var hash = Convert.ToHexString( SHA1.HashData( destination ) );
            Assert.Equal( "7B8567D9B3C09295CDBF4978C32B348D8E76C750", hash );
        }

        [Fact]
        public void DecryptsAndDecompressesDepotChunkZStd()
        {
            var chunkData = ReadChunkFile( "depot_3441461_chunk_9e72678e305540630a665b93e1463bc3983eb55a.bin" );

            var chunk = new DepotManifest.ChunkData(
                id: [], // id is not needed herein
                checksum: 3753325726,
                offset: 0,
                comp_length: 176,
                uncomp_length: 156
            );

            var destination = new byte[ chunk.UncompressedLength ];
            var writtenLength = DepotChunk.Process( chunk, chunkData, destination, [
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
            ] );

            Assert.Equal( chunk.CompressedLength, ( uint )chunkData.Length );
            Assert.Equal( chunk.UncompressedLength, ( uint )writtenLength );

            var hash = Convert.ToHexString( SHA1.HashData( destination ) );
            Assert.Equal( "9E72678E305540630A665B93E1463BC3983EB55A", hash );
        }

        static readonly byte[] Depot1004Key = Convert.FromHexString( "F21FAA4F29ECEE9D192FC1784D1701376EB87E13521824CEAF0116B10C558A81" );

        static readonly DepotManifest.ChunkData Depot1004Chunk = new(
            id: Convert.FromHexString( "B4E70AFDE4693284936DA1A67758C897A492E1FC" ),
            checksum: 1759764290,
            offset: 0,
            comp_length: 33536,
            uncomp_length: 113816
        );

        internal static byte[] ReadChunkFile( string name )
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream( $"Tests.Files.{name}" );
            using var ms = new MemoryStream();
            stream.CopyTo( ms );
            return ms.ToArray();
        }

        [Fact]
        public void DecryptsAndDecompressesLargerDepotChunkZStd()
        {
            var chunkData = ReadChunkFile( "depot_1004_chunk_b4e70afde4693284936da1a67758c897a492e1fc.bin" );

            var destination = new byte[ Depot1004Chunk.UncompressedLength ];
            var writtenLength = DepotChunk.Process( Depot1004Chunk, chunkData, destination, Depot1004Key );

            Assert.Equal( Depot1004Chunk.CompressedLength, ( uint )chunkData.Length );
            Assert.Equal( Depot1004Chunk.UncompressedLength, ( uint )writtenLength );
            Assert.Equal( Depot1004Chunk.ChunkID, SHA1.HashData( destination ) );
        }

        [Fact]
        public void ThrowsOnCorruptDepotChunkZStd()
        {
            var chunkData = ReadChunkFile( "depot_1004_chunk_b4e70afde4693284936da1a67758c897a492e1fc.bin" );
            chunkData[ chunkData.Length / 2 ] ^= 0x55;

            var destination = new byte[ Depot1004Chunk.UncompressedLength ];
            Assert.Throws<InvalidDataException>( () => DepotChunk.Process( Depot1004Chunk, chunkData, destination, Depot1004Key ) );
        }

#if DEBUG
        [Fact]
        public void ZStdThrowsOnCorruptFrame()
        {
            var payload = CryptoHelper.SymmetricDecrypt( ReadChunkFile( "depot_1004_chunk_b4e70afde4693284936da1a67758c897a492e1fc.bin" ), Depot1004Key );
            var destination = new byte[ Depot1004Chunk.UncompressedLength ];

            // Drop the middle third of the frame, keeping the VZstd header and footer intact
            var third = payload.Length / 3;
            var truncated = new byte[ payload.Length - third ];
            payload.AsSpan( 0, third ).CopyTo( truncated );
            payload.AsSpan( third * 2 ).CopyTo( truncated.AsSpan( third ) );
            Assert.Throws<InvalidDataException>( () => VZstdUtil.Decompress( truncated, destination, verifyChecksum: true ) );

            var badFrameMagic = payload.AsSpan().ToArray();
            badFrameMagic[ 8 ] ^= 0xFF;
            Assert.Throws<InvalidDataException>( () => VZstdUtil.Decompress( badFrameMagic, destination, verifyChecksum: true ) );
        }

        [Fact]
        public void ZStdThrowsOnShortInput()
        {
            var payload = CryptoHelper.SymmetricDecrypt( ReadChunkFile( "depot_1004_chunk_b4e70afde4693284936da1a67758c897a492e1fc.bin" ), Depot1004Key );
            var destination = new byte[ Depot1004Chunk.UncompressedLength ];

            Assert.Throws<InvalidDataException>( () => VZstdUtil.Decompress( payload.AsSpan( 0, 22 ), destination ) );
        }
#endif
    }
}
