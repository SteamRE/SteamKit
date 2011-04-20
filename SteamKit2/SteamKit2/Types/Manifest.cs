using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    public sealed class Manifest
    {

        public sealed class Node
        {
            [Flags]
            public enum Attribs
            {
                UserConfigurationFile = 0x1,
                LaunchFile = 0x2,
                LockedFile = 0x8,
                NoCacheFile = 0x20,
                VersionedFile = 0x40,
                PurgeFile = 0x80,
                EncryptedFile = 0x100,
                ReadOnly = 0x200,
                HiddenFile = 0x400,
                ExecutableFile = 0x800,
                File = 0x4000,
            }

            public uint SizeOrCount { get; set; }

            public int FileID { get; set; }
            public Attribs Attributes { get; set; }
            public int ParentIndex { get; set; }

            public string Name { get; set; }
            public string FullName { get; set; }

            internal Manifest Parent { get; set; }
        }

        public byte[] RawData { get; set; }

        public uint DepotID { get; set; }
        public uint DepotVersion { get; set; }

        public uint NodeCount { get; set; }
        public uint FileCount { get; set; }

        public uint BlockSize { get; set; }

        public uint DepotChecksum { get; set; }

        public List<Node> Nodes { get; private set; }


        const uint HEADER_SIZE = 56;
        const uint ENTRY_SIZE = 28;


        public Manifest( byte[] manifestBlob )
        {
            this.RawData = manifestBlob;

            this.Nodes = new List<Node>();

            using ( DataStream ds = new DataStream( manifestBlob ))
            {
                uint headerVersion = ds.ReadUInt32();

                this.DepotID = ds.ReadUInt32();
                this.DepotVersion = ds.ReadUInt32();

                this.NodeCount = ds.ReadUInt32();
                this.FileCount = ds.ReadUInt32();

                this.BlockSize = ds.ReadUInt32();

                // checksum is the last field in the header
                ds.Seek( HEADER_SIZE - 4, SeekOrigin.Begin );

                this.DepotChecksum = ds.ReadUInt32();

                uint namesStart = HEADER_SIZE + ( this.NodeCount * ENTRY_SIZE );


                for ( int x = 0 ; x < this.NodeCount ; ++x )
                {
                    ds.Seek( HEADER_SIZE + ( x * ENTRY_SIZE ), SeekOrigin.Begin );

                    uint nameOffset = namesStart + ds.ReadUInt32();

                    Node entry = new Node
                    {
                        SizeOrCount = ds.ReadUInt32(),
                        FileID = ds.ReadInt32(),
                        Attributes = ( Node.Attribs )ds.ReadUInt32(),
                        ParentIndex = ds.ReadInt32(),

                        Parent = this,
                    };

                    ds.Seek( nameOffset, SeekOrigin.Begin );

                    entry.Name = ds.ReadNullTermString( Encoding.ASCII );

                    this.Nodes.Add( entry );
                }
            }

            // now build full names
            for ( int x = 0 ; x < this.NodeCount ; ++x )
            {
                Node entry = this.Nodes[ x ];
                string fullName = entry.Name;

                while ( entry.ParentIndex != -1 )
                {
                    entry = this.Nodes[ entry.ParentIndex ];
                    fullName = Path.Combine( entry.Name, fullName );
                }

                entry = this.Nodes[ x ];
                entry.FullName = fullName;
            }
        }
    }

}
