using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    public sealed class Manifest
    {

        public sealed class DirectoryEntry
        {
            public uint ItemSize { get; set; }

            public int FileID { get; set; }
            public uint Type { get; set; }
            public int ParentIndex { get; set; }

            public string Name { get; set; }
            public string FullName { get; set; }
        }

        public uint DepotID { get; set; }
        public uint DepotVersion { get; set; }

        public uint NumDirEntries { get; set; }
        public uint NumFiles { get; set; }

        public uint DepotChecksum { get; set; }

        public List<DirectoryEntry> DirEntries { get; private set; }


        const uint HEADER_SIZE = 56;
        const uint ENTRY_SIZE = 28;


        public Manifest( byte[] manifestBlob )
        {
            this.DirEntries = new List<DirectoryEntry>();

            using ( DataStream ds = new DataStream( manifestBlob ))
            {
                uint unk = ds.ReadUInt32();

                this.DepotID = ds.ReadUInt32();
                this.DepotVersion = ds.ReadUInt32();

                this.NumDirEntries = ds.ReadUInt32();
                this.NumFiles = ds.ReadUInt32();

                // checksum is the last field in the header
                ds.Seek( HEADER_SIZE - 4, SeekOrigin.Begin );

                this.DepotChecksum = ds.ReadUInt32();

                uint namesStart = HEADER_SIZE + ( this.NumDirEntries * ENTRY_SIZE );


                for ( int x = 0 ; x < this.NumDirEntries ; ++x )
                {
                    ds.Seek( HEADER_SIZE + ( x * ENTRY_SIZE ), SeekOrigin.Begin );

                    uint nameOffset = namesStart + ds.ReadUInt32();

                    DirectoryEntry entry = new DirectoryEntry
                    {
                        ItemSize = ds.ReadUInt32(),
                        FileID = ds.ReadInt32(),
                        Type = ds.ReadUInt32(),
                        ParentIndex = ds.ReadInt32()
                    };

                    ds.Seek( nameOffset, SeekOrigin.Begin );

                    entry.Name = ds.ReadNullTermString( Encoding.ASCII );

                    this.DirEntries.Add( entry );
                }
            }

            // now build full names
            for ( int x = 0 ; x < this.NumDirEntries ; ++x )
            {
                DirectoryEntry entry = this.DirEntries[ x ];
                string fullName = entry.Name;

                while ( entry.ParentIndex != -1 )
                {
                    entry = this.DirEntries[ entry.ParentIndex ];
                    fullName = Path.Combine( entry.Name, fullName );
                }

                entry = this.DirEntries[ x ];
                entry.FullName = fullName;
            }
        }
    }

}
