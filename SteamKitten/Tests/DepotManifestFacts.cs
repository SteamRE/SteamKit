using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using SteamKitten;
using Xunit;

namespace Tests
{
    public class DepotManifestFacts
    {
        private static readonly byte[] Depot440DecryptionKey =
        [
            0x44, 0xCE, 0x5C, 0x52, 0x97, 0xA4, 0x15, 0xA1,
            0xA6, 0xF6, 0x9C, 0x85, 0x60, 0x37, 0xA5, 0xA2,
            0xFD, 0xD8, 0x2C, 0xD4, 0x74, 0xFA, 0x65, 0x9E,
            0xDF, 0xB4, 0xD5, 0x9B, 0x2A, 0xBC, 0x55, 0xFC,
        ];

        [Fact]
        public void ParsesAndDecryptsManifestVersion4()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream( "Tests.Files.depot_440_1118032470228587934_v4.manifest" );
            using var ms = new MemoryStream();
            stream.CopyTo( ms );

            var manifestData = ms.ToArray();

            var depotManifest = DepotManifest.Deserialize( manifestData );

            Assert.True( depotManifest.FilenamesEncrypted );
            Assert.Equal( 1195249848u, depotManifest.EncryptedCRC );

            depotManifest.DecryptFilenames( Depot440DecryptionKey );

            TestDecryptedManifest( depotManifest );
        }

        [Fact]
        public void ParsesAndDecryptsManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream( "Tests.Files.depot_440_1118032470228587934.manifest" );
            using var ms = new MemoryStream();
            stream.CopyTo( ms );

            var manifestData = ms.ToArray();

            var depotManifest = DepotManifest.Deserialize( manifestData );

            Assert.True( depotManifest.FilenamesEncrypted );
            Assert.Equal( 1606273976u, depotManifest.EncryptedCRC );

            depotManifest.DecryptFilenames( Depot440DecryptionKey );

            TestDecryptedManifest( depotManifest );
        }

        [Fact]
        public void ParsesDecryptedManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream( "Tests.Files.depot_440_1118032470228587934_decrypted.manifest" );
            using var ms = new MemoryStream();
            stream.CopyTo( ms );

            var manifestData = ms.ToArray();

            var depotManifest = DepotManifest.Deserialize( manifestData );

            TestDecryptedManifest( depotManifest );
        }

        [Fact]
        public void RoundtripSerializesManifestEncryptedManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream( "Tests.Files.depot_440_1118032470228587934.manifest" );
            using var ms = new MemoryStream();
            stream.CopyTo( ms );

            var manifestData = ms.ToArray();

            var depotManifest = DepotManifest.Deserialize( manifestData );

            using var actualStream = new MemoryStream();
            depotManifest.Serialize( actualStream );

            var actual = actualStream.ToArray().AsSpan();

            // We are unable to write signatures, so validate everything except for the signature
            var signature = new byte[ 4 ] { 0x17, 0xB8, 0x81, 0x1B };

            var expected = manifestData.AsSpan();
            var actualOffset = actual.IndexOf( signature ); // DepotManifest.PROTOBUF_SIGNATURE_MAGIC
            var expectedOffset = expected.IndexOf( signature );

            Assert.True( actualOffset > 0 );
            Assert.True( expectedOffset > 0 );
            Assert.Equal( expected[ ..expectedOffset ], actual[ ..actualOffset ] );

            var expectedSignatureLength = BitConverter.ToInt32( expected[ ( expectedOffset + 4 ).. ] );
            var actualSignatureLength = BitConverter.ToInt32( actual[ ( actualOffset + 4 ).. ] );

            Assert.Equal( 131, expectedSignatureLength );
            Assert.Equal( 0, actualSignatureLength );
            Assert.Equal( expected[ ( expectedOffset + expectedSignatureLength + 8 ).. ], actual[ ( actualOffset + 8 ).. ] );
        }

        [Fact]
        public void RoundtripSerializesManifestByteIndentical()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream( "Tests.Files.depot_440_1118032470228587934_decrypted.manifest" );
            using var ms = new MemoryStream();
            stream.CopyTo( ms );

            var manifestData = ms.ToArray();

            var depotManifest = DepotManifest.Deserialize( manifestData );

            using var actualStream = new MemoryStream();
            depotManifest.Serialize( actualStream );

            var actual = actualStream.ToArray();

            Assert.Equal( manifestData, actual );
        }

        private static void TestDecryptedManifest( DepotManifest depotManifest )
        {
            Assert.False( depotManifest.FilenamesEncrypted );
            Assert.Equal( 440u, depotManifest.DepotID );
            Assert.Equal( 1118032470228587934ul, depotManifest.ManifestGID );
            Assert.Equal( 825745u, depotManifest.TotalUncompressedSize );
            Assert.Equal( 43168u, depotManifest.TotalCompressedSize );
            Assert.Equal( 7, depotManifest.Files.Count );
            Assert.Equal( new DateTime( 2013, 4, 17, 20, 39, 24, DateTimeKind.Utc ), depotManifest.CreationTime );

            Assert.Equal( Path.Join( "bin", "dxsupport.cfg" ), depotManifest.Files[ 0 ].FileName );
            Assert.Equal( Path.Join( "bin", "dxsupport.csv" ), depotManifest.Files[ 1 ].FileName );
            Assert.Equal( Path.Join( "bin", "dxsupport_episodic.cfg" ), depotManifest.Files[ 2 ].FileName );
            Assert.Equal( Path.Join( "bin", "dxsupport_sp.cfg" ), depotManifest.Files[ 3 ].FileName );
            Assert.Equal( Path.Join( "bin", "vidcfg.bin" ), depotManifest.Files[ 4 ].FileName );
            Assert.Equal( Path.Join( "hl2", "media", "startupvids.txt" ), depotManifest.Files[ 5 ].FileName );
            Assert.Equal( Path.Join( "tf", "media", "startupvids.txt" ), depotManifest.Files[ 6 ].FileName );

            Assert.Equal( ( EDepotFileFlag )0, depotManifest.Files[ 0 ].Flags );
            Assert.Equal( 398709u, depotManifest.Files[ 0 ].TotalSize );
            Assert.Equal( Convert.FromHexString( "BAC8E2657470B2EB70D6DDCD6C07004BE8738697" ), depotManifest.Files[ 2 ].FileHash );

            foreach ( var file in depotManifest.Files )
            {
                Assert.Equal( file.FileNameHash, SHA1.HashData( Encoding.UTF8.GetBytes( file.FileName.Replace( '/', '\\' ) ) ) );
                Assert.NotNull( file.LinkTarget );
                Assert.Single( file.Chunks );
            }

            var chunk = depotManifest.Files[ 6 ].Chunks[ 0 ];
            Assert.Equal( 963249608u, chunk.Checksum );
            Assert.Equal( 144u, chunk.CompressedLength );
            Assert.Equal( 17u, chunk.UncompressedLength );
            Assert.Equal( 0u, chunk.Offset );
            Assert.Equal( Convert.FromHexString( "94020BDE145A521EDEC9A9424E7A90FD042481E9" ), chunk.ChunkID );
        }
    }
}
