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
    public sealed class Steam2ChecksumData
    {
        public byte[] RawData { get; private set; }

        uint ItemCount;
        uint ChecksumCount;

        public sealed class ChecksumMapEntry
        {
            public ChecksumMapEntry( uint count, uint start )
            {
                this.ChecksumCount = count;
                this.FirstChecksumIndex = start;
            }
            public uint ChecksumCount { get; private set; }
            public uint FirstChecksumIndex { get; private set; }
        };

        private List<ChecksumMapEntry> maps;
        private Dictionary<uint, int> checksums;

        public Steam2ChecksumData( byte[] blob )
        {
            maps = new List<ChecksumMapEntry>();
            checksums = new Dictionary<uint, int>();
            RawData = blob;
            using ( MemoryStream ms = new MemoryStream( RawData ) )
            {
                using ( BinaryReaderEx br = new BinaryReaderEx( ms ) )
                {
                    // we don't care about the first two, always the same
                    br.ReadBytes( 8 );
                    ItemCount = br.ReadUInt32();
                    ChecksumCount = br.ReadUInt32();
                    uint count;
                    uint start = 0;
                    for ( int i = 0 ; i < ItemCount ; i++ )
                    {
                        count = br.ReadUInt32();
                        start = br.ReadUInt32();
                        maps.Add( new ChecksumMapEntry( count, start ) );
                    }

                    for ( uint i = 0 ; i < ChecksumCount ; i++ )
                    {
                        long pos = br.BaseStream.Position;
                        checksums[ i ] = br.ReadInt32();
                    }
                }
            }
        }

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

        private string ByteArrayToHexString( byte[] bytes )
        {
            return BitConverter.ToString( bytes ).Replace( '-', ' ' );
        }

        public void DumpToFile( string filename )
        {
            using ( var file = File.CreateText( filename ) )
            {
                using ( MemoryStream ms = new MemoryStream( RawData ) )
                {
                    using ( BinaryReaderEx br = new BinaryReaderEx( ms ) )
                    {
                        file.WriteLine( "Dummy0:\t" + ByteArrayToHexString( br.ReadBytes( 4 ) ) );
                        file.WriteLine( "Always1:\t" + ByteArrayToHexString( br.ReadBytes( 4 ) ) );
                        uint icount = br.ReadUInt32();
                        file.WriteLine( "ItemCount:\t" + ByteArrayToHexString( BitConverter.GetBytes( icount ) ) + " (" + icount.ToString() + ")" );
                        uint ccount = br.ReadUInt32();
                        file.WriteLine( "ChecksumCount:\t" + ByteArrayToHexString( BitConverter.GetBytes( ccount ) ) + " (" + ccount.ToString() + ")" );
                        file.WriteLine();
                        for ( int i = 0 ; i < icount ; i++ )
                        {
                            file.WriteLine( "File " + i.ToString() );
                            uint cscount = br.ReadUInt32();
                            uint csstart = br.ReadUInt32();
                            file.WriteLine( "\t- ChecksumCount:\t" + ByteArrayToHexString( BitConverter.GetBytes( cscount ) ) + " (" + cscount.ToString() + ")" );
                            file.WriteLine( "\t- FirstChecksum:\t" + ByteArrayToHexString( BitConverter.GetBytes( csstart ) ) + " (" + csstart.ToString() + ")" );
                            file.WriteLine();
                        }

                        for ( uint i = 0 ; i < ccount ; i++ )
                        {
                            file.WriteLine( "Checksum " + i.ToString() + ":\t" + ByteArrayToHexString( br.ReadBytes( 4 ) ) );
                        }
                    }
                }
            }
        }
    }
}