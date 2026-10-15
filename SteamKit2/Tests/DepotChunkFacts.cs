using System;
using System.Buffers.Binary;
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

        internal static readonly byte[] Depot4Key = Convert.FromHexString( "57EF98DE5388A84CBD6BA94DCDA36F8BB749177DAABF35212FEFCC098333E4D5" );
        static readonly byte[] Depot1004Key = Convert.FromHexString( "F21FAA4F29ECEE9D192FC1784D1701376EB87E13521824CEAF0116B10C558A81" );

        internal static readonly DepotManifest.ChunkData Depot4Chunk = new(
            id: Convert.FromHexString( "DC036A660987D8F705936D4443FEDB5E1D2E5D9B" ),
            checksum: 649235822,
            offset: 0,
            comp_length: 25072,
            uncomp_length: 68887
        );

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
        public void DecryptsAndDecompressesLargerDepotChunkVZip()
        {
            var chunkData = ReadChunkFile( "depot_4_chunk_dc036a660987d8f705936d4443fedb5e1d2e5d9b.bin" );

            var destination = new byte[ Depot4Chunk.UncompressedLength ];
            var writtenLength = DepotChunk.Process( Depot4Chunk, chunkData, destination, Depot4Key );

            Assert.Equal( Depot4Chunk.CompressedLength, ( uint )chunkData.Length );
            Assert.Equal( Depot4Chunk.UncompressedLength, ( uint )writtenLength );
            Assert.Equal( Depot4Chunk.ChunkID, SHA1.HashData( destination ) );
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
        public void ThrowsOnCorruptDepotChunkVZip()
        {
            var chunkData = ReadChunkFile( "depot_4_chunk_dc036a660987d8f705936d4443fedb5e1d2e5d9b.bin" );
            chunkData[ chunkData.Length / 2 ] ^= 0x55;

            var destination = new byte[ Depot4Chunk.UncompressedLength ];
            Assert.Throws<InvalidDataException>( () => DepotChunk.Process( Depot4Chunk, chunkData, destination, Depot4Key ) );
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
        // VZip is a 7 byte header, 5 bytes of LZMA properties, the LZMA stream, and a 10 byte footer of CRC, size and magic
        const int VZipPropertiesOffset = 7;
        const int VZipStreamOffset = 12;
        const int VZipFooterLength = 10;

        static byte[] ReadDecryptedVZipChunk()
            => CryptoHelper.SymmetricDecrypt( ReadChunkFile( "depot_4_chunk_dc036a660987d8f705936d4443fedb5e1d2e5d9b.bin" ), Depot4Key );

        static byte[] WithStream( byte[] payload, ReadOnlySpan<byte> stream )
        {
            var result = new byte[ VZipStreamOffset + stream.Length + VZipFooterLength ];
            payload.AsSpan( 0, VZipStreamOffset ).CopyTo( result );
            stream.CopyTo( result.AsSpan( VZipStreamOffset ) );
            payload.AsSpan( payload.Length - VZipFooterLength ).CopyTo( result.AsSpan( result.Length - VZipFooterLength ) );
            return result;
        }

        static byte[] WithDecompressedSize( byte[] payload, int size )
        {
            var result = payload.AsSpan().ToArray();
            BinaryPrimitives.WriteInt32LittleEndian( result.AsSpan( result.Length - 6 ), size );
            return result;
        }

        [Fact]
        public void VZipIgnoresDictionarySizeSmallerThanOutput()
        {
            var payload = ReadDecryptedVZipChunk();
            Assert.True( BinaryPrimitives.ReadUInt32LittleEndian( payload.AsSpan( VZipPropertiesOffset + 1 ) ) >= Depot4Chunk.UncompressedLength );

            // Matches only reach back into the output, so the dictionary size in the properties does not limit them
            BinaryPrimitives.WriteUInt32LittleEndian( payload.AsSpan( VZipPropertiesOffset + 1 ), 1 << 12 );

            var destination = new byte[ Depot4Chunk.UncompressedLength ];
            var written = VZipUtil.Decompress( payload, destination, verifyChecksum: true );

            Assert.Equal( Depot4Chunk.ChunkID, SHA1.HashData( destination.AsSpan( 0, written ) ) );
        }

        [Fact]
        public void VZipCutsMatchesAtEndOfOutput()
        {
            var payload = ReadDecryptedVZipChunk();
            var expected = new byte[ Depot4Chunk.UncompressedLength ];
            VZipUtil.Decompress( payload, expected, verifyChecksum: true );

            // Some of these lengths end inside a match, which has to be cut short rather than rejected
            for ( var length = expected.Length - 64; length < expected.Length; length++ )
            {
                var destination = new byte[ length ];
                var written = VZipUtil.Decompress( WithDecompressedSize( payload, length ), destination, verifyChecksum: false );

                Assert.Equal( length, written );
                Assert.True( destination.AsSpan().SequenceEqual( expected.AsSpan( 0, length ) ) );
            }
        }

        [Fact]
        public void VZipThrowsOnTruncatedStream()
        {
            var payload = ReadDecryptedVZipChunk();
            var stream = payload.AsSpan( VZipStreamOffset, payload.Length - VZipStreamOffset - VZipFooterLength );
            var destination = new byte[ Depot4Chunk.UncompressedLength ];

            var halfStream = WithStream( payload, stream[ ..( stream.Length / 2 ) ] );
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( halfStream, destination, verifyChecksum: false ) );

            var tooShortForRangeDecoder = WithStream( payload, stream[ ..4 ] );
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( tooShortForRangeDecoder, destination, verifyChecksum: false ) );

            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( payload.AsSpan( 0, 21 ), destination ) );
        }

        [Fact]
        public void VZipThrowsWhenOutputIsLargerThanStream()
        {
            var payload = WithDecompressedSize( ReadDecryptedVZipChunk(), ( int )Depot4Chunk.UncompressedLength + 1000 );
            var destination = new byte[ Depot4Chunk.UncompressedLength + 1000 ];

            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( payload, destination, verifyChecksum: false ) );
        }

        [Fact]
        public void VZipThrowsOnInvalidHeaderFooterOrProperties()
        {
            var payload = ReadDecryptedVZipChunk();
            var destination = new byte[ Depot4Chunk.UncompressedLength ];

            var badHeader = payload.AsSpan().ToArray();
            badHeader[ 0 ] = ( byte )'X';
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( badHeader, destination ) );

            var badVersion = payload.AsSpan().ToArray();
            badVersion[ 2 ] = ( byte )'d';
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( badVersion, destination ) );

            var badFooter = payload.AsSpan().ToArray();
            badFooter[ ^1 ] = ( byte )'X';
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( badFooter, destination ) );

            var negativeSize = WithDecompressedSize( payload, -1 );
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( negativeSize, destination ) );

            var badProperties = payload.AsSpan().ToArray();
            badProperties[ VZipPropertiesOffset ] = 9 * 5 * 5;
            Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( badProperties, destination ) );

            Assert.Throws<ArgumentException>( () => VZipUtil.Decompress( payload, new byte[ Depot4Chunk.UncompressedLength - 1 ] ) );
        }

        [Fact]
        public void VZipThrowsOnMatchBeforeAnyData()
        {
            var payload = WithDecompressedSize( ReadDecryptedVZipChunk(), 16 );
            var destination = new byte[ 16 ];

            // The initial code decodes a match bit followed by a simple match with distance 1
            var match = new byte[ 32 ];
            match[ 1 ] = 0x7F;
            match[ 2 ] = 0xFF;
            match[ 3 ] = 0xFD;

            var exception = Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( WithStream( payload, match ), destination, verifyChecksum: false ) );
            Assert.Contains( "distance", exception.Message );

            // The initial code decodes a match bit followed by a repeated match bit
            var repeatedMatch = new byte[ 32 ];
            repeatedMatch[ 1 ] = 0xFF;
            repeatedMatch[ 2 ] = 0xFF;
            repeatedMatch[ 3 ] = 0xFF;
            repeatedMatch[ 4 ] = 0xF0;

            exception = Assert.Throws<InvalidDataException>( () => VZipUtil.Decompress( WithStream( payload, repeatedMatch ), destination, verifyChecksum: false ) );
            Assert.Contains( "repeated", exception.Message );
        }

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
