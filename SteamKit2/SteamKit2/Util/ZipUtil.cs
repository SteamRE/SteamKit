/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.IO.Compression;
using System.IO.Hashing;

namespace SteamKit2
{
    static class ZipUtil
    {
        public static byte[] Decompress( byte[] buffer )
        {
            using var ms = new MemoryStream( buffer );
            return Decompress( ms );
        }

        public static byte[] Decompress( MemoryStream ms )
        {
            using var zip = new ZipArchive( ms );
            var entries = zip.Entries;

            DebugLog.Assert( entries.Count == 1, nameof( ZipUtil ), "Expected the zip to contain only one file" );

            var entry = entries[ 0 ];
            var decompressed = new byte[ entry.Length ];

            using var entryStream = entry.Open();
            using var entryMemory = new MemoryStream( decompressed );
            entryStream.CopyTo( entryMemory );

            var checkSum = Crc32.HashToUInt32( decompressed );

            if ( checkSum != entry.Crc32 )
            {
                throw new Exception( "Checksum validation failed for decompressed file" );
            }

            return decompressed;
        }
    }
}
