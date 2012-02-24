/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.IO;

namespace SteamKit2
{
    /*
     * This format was partially figured out with help from HLLib's GCFFile.h.
     * 
     * Each file has a checksum for each 32Kb (0x8000 bytes), and one more for the
     * remainder data, if any.
     * 
     * Disassembly and GCF format suggest that the checksum from Steam is the adler32
     * checksum of the data chunk xor'd with the CRC checksum of the data chunk.
     * 
     * TODO: figure out what that doesn't seem to match up.
     */

    /// <summary>
    /// Represents file checksum data for Steam2 manifests.
    /// </summary>
    public sealed class Steam2ChecksumData
    {
        byte[] data;

        uint itemCount;
        uint checksumCount;

        sealed class ChecksumMapEntry
        {
            internal ChecksumMapEntry( uint count, uint start )
            {
                this.ChecksumCount = count;
                this.FirstChecksumIndex = start;
            }

            public uint ChecksumCount { get; private set; }
            public uint FirstChecksumIndex { get; private set; }
        };

        List<ChecksumMapEntry> maps;
        Dictionary<uint, int> checksums;


        internal Steam2ChecksumData( byte[] blob )
        {
            maps = new List<ChecksumMapEntry>();
            checksums = new Dictionary<uint, int>();

            data = blob;

            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                using ( BinaryReader br = new BinaryReader( ms ) )
                {
                    // we don't care about the first two, always the same
                    br.ReadBytes( 8 );

                    itemCount = br.ReadUInt32();
                    checksumCount = br.ReadUInt32();

                    uint count;
                    uint start = 0;
                    for ( int i = 0 ; i < itemCount ; i++ )
                    {
                        count = br.ReadUInt32();
                        start = br.ReadUInt32();
                        maps.Add( new ChecksumMapEntry( count, start ) );
                    }

                    for ( uint i = 0 ; i < checksumCount ; i++ )
                    {
                        long pos = br.BaseStream.Position;
                        checksums[ i ] = br.ReadInt32();
                    }
                }
            }
        }

        /// <summary>
        /// Gets checksums for the given FileID.
        /// </summary>
        /// <param name="FileID">The FileID.</param>
        /// <returns>A list of checksums for the given file.</returns>
        public int[] GetFileChecksums( int FileID )
        {
            ChecksumMapEntry map = maps[ FileID ];

            uint count = map.ChecksumCount;
            int[] ret = new int[ count ];

            for ( int i = 0 ; i < count ; i++ )
            {
                ret[ i ] = checksums[ map.FirstChecksumIndex + ( uint )i ];
            }

            return ret;
        }
    }
}