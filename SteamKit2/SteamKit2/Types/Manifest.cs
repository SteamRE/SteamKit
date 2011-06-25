using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    public sealed class Steam3Manifest
    {
        public sealed class FileMapping
        {
            public sealed class Chunk
            {
                public byte[] ChunkGID { get; set; } // sha1 hash for this chunk

                // 4 bytes of unknown data, (d)serialized as a blob
                public byte[] Unknown1 { get; set; }

                public ulong Unknown2 { get; set; }

                // these look similar to some kind of flag
                public uint Unknown3 { get; set; }
                public uint Unknown4 { get; set; }


                internal void Deserialize( DataStream ds )
                {
                    ChunkGID = ds.ReadBytes( 20 );

                    Unknown1 = ds.ReadBytes( 4 );

                    Unknown2 = ds.ReadUInt64();

                    Unknown3 = ds.ReadUInt32();
                    Unknown4 = ds.ReadUInt32();
                }
            }

            public string FileName { get; set; }

            public ulong MaxSize { get; set; }

            public uint Unknown1 { get; set; }

            // these two most likely represent hashes of encrypted and decrypted data
            // much like the crc's in the manifest
            // the data that is hashed is unknown
            public byte[] UnknownHash1 { get; set; }
            public byte[] UnknownHash2 { get; set; }

            public uint NumChunks { get; set; }

            public Chunk[] Chunks { get; private set; }

            public FileMapping()
            {
            }


            internal void Deserialize( DataStream ds )
            {
                FileName = ds.ReadNullTermString( Encoding.ASCII );

                MaxSize = ds.ReadUInt64();

                Unknown1 = ds.ReadUInt32();

                UnknownHash1 = ds.ReadBytes( 20 );
                UnknownHash2 = ds.ReadBytes( 20 );

                NumChunks = ds.ReadUInt32();

                Chunks = new Chunk[ NumChunks ];

                for ( int x = 0 ; x < Chunks.Length ; ++x )
                {
                    Chunks[ x ].Deserialize( ds );
                }
            }
        }

        const uint MAGIC = 372545409;
        const uint CURRENT_VERSION = 4;


        public uint Magic { get; set; }
        public uint Version { get; set; } 

        public uint DepotID { get; set; }

        public ulong ManifestGID { get; set; } // sha1 hash for this manifest
        public DateTime CreationTime { get; set; } // unsure

        public bool IsEncrypted { get; set; }

        public ulong MaxSize { get; set; }
        public ulong Unknown1 { get; set; }

        public uint NumChunks { get; set; }
        public uint Unknown2 { get; set; }

        public uint FileMappingSize { get; set; }

        public uint EncryptedCRC { get; set; }
        public uint DecryptedCRC { get; set; }

        public uint UltimateAnswerToTheUltimateQuestionOfLifeTheUniverseAndEverything { get; set; }

        public FileMapping Mapping { get; private set; }


        public Steam3Manifest()
        {
            Mapping = new FileMapping();
        }


        void Deserialize( byte[] data )
        {
            using (DataStream ds = new DataStream( data ) )
            {

                Magic = ds.ReadUInt32();

                if ( Magic != MAGIC )
                {
                    throw new InvalidDataException( "data is not a valid steam3 manifest: incorrect magic." );
                }

                Version = ds.ReadUInt32();

                DepotID = ds.ReadUInt32();

                ManifestGID = ds.ReadUInt64();
                CreationTime = Utils.DateTimeFromUnixTime( ds.ReadUInt32() );

                IsEncrypted = ds.ReadUInt32() != 0;

                MaxSize = ds.ReadUInt64();
                Unknown1 = ds.ReadUInt64();

                NumChunks = ds.ReadUInt32();
                Unknown2 = ds.ReadUInt32();

                FileMappingSize = ds.ReadUInt32();

                EncryptedCRC = ds.ReadUInt32();
                DecryptedCRC = ds.ReadUInt32();
                
                // i'm sorry to say that we'll be breaking from canon and we shall not be taking 7 and a half million years to read this value
                UltimateAnswerToTheUltimateQuestionOfLifeTheUniverseAndEverything = ds.ReadUInt32();

                Mapping.Deserialize( ds );
                
            }
        }

    }

    public sealed class Steam2Manifest
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

            internal Steam2Manifest Parent { get; set; }
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


        public Steam2Manifest( byte[] manifestBlob )
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
