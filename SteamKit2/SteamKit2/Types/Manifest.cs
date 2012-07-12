/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// Represents the binary Steam3 manifest format.
    /// </summary>
    sealed class Steam3Manifest
    {
        public sealed class FileMapping
        {
            public sealed class Chunk
            {
                public byte[] ChunkGID { get; set; } // sha1 hash for this chunk

                public byte[] CRC { get; set; }
                public ulong Offset { get; set; }

                public uint DecompressedSize { get; set; }
                public uint CompressedSize { get; set; }


                internal void Deserialize( DataStream ds )
                {
                    ChunkGID = ds.ReadBytes( 20 );

                    CRC = ds.ReadBytes( 4 );

                    Offset = ds.ReadUInt64();

                    DecompressedSize = ds.ReadUInt32();
                    CompressedSize = ds.ReadUInt32();
                }
            }

            public string FileName { get; set; }

            public ulong TotalSize { get; set; }
            public EDepotFileFlag Flags { get; set; }

            public byte[] HashFileName { get; set; }
            public byte[] HashContent { get; set; }

            public uint NumChunks { get; set; }
            public Chunk[] Chunks { get; private set; }

            public FileMapping()
            {
            }


            internal void Deserialize( DataStream ds )
            {
                FileName = ds.ReadNullTermString( Encoding.ASCII );

                TotalSize = ds.ReadUInt64();

                Flags = (EDepotFileFlag)ds.ReadUInt32();

                HashFileName = ds.ReadBytes( 20 );
                HashContent = ds.ReadBytes( 20 );

                NumChunks = ds.ReadUInt32();

                Chunks = new Chunk[ NumChunks ];

                for ( int x = 0 ; x < Chunks.Length ; ++x )
                {
                    Chunks[ x ] = new Chunk();
                    Chunks[ x ].Deserialize( ds );
                }
            }
        }

        public const uint MAGIC = 0x16349781;
        const uint CURRENT_VERSION = 4;

        public uint Magic { get; set; }
        public uint Version { get; set; } 

        public uint DepotID { get; set; }

        public ulong ManifestGID { get; set; }
        public DateTime CreationTime { get; set; }

        public bool AreFileNamesEncrypted { get; set; }

        public ulong TotalUncompressedSize { get; set; }
        public ulong TotalCompressedSize { get; set; }

        public uint ChunkCount { get; set; }

        public uint FileEntryCount { get; set; }
        public uint FileMappingSize { get; set; }

        public uint EncryptedCRC { get; set; }
        public uint DecryptedCRC { get; set; }

        public uint Flags { get; set; }

        public List<FileMapping> Mapping { get; private set; }


        private Steam3Manifest()
        {
        }

        public Steam3Manifest(byte[] data)
        {
            Deserialize(data);
        }

        internal Steam3Manifest(DataStream data)
        {
            Deserialize(data);
        }

        void Deserialize(byte[] data)
        {
            using (DataStream ds = new DataStream(data))
            {
                Deserialize(ds);
            }
        }

        void Deserialize( DataStream ds )
        {
            Mapping = new List<FileMapping>();

            Magic = ds.ReadUInt32();

            if (Magic != MAGIC)
            {
                throw new InvalidDataException("data is not a valid steam3 manifest: incorrect magic.");
            }

            Version = ds.ReadUInt32();

            DepotID = ds.ReadUInt32();

            ManifestGID = ds.ReadUInt64();
            CreationTime = Utils.DateTimeFromUnixTime(ds.ReadUInt32());

            AreFileNamesEncrypted = ds.ReadUInt32() != 0;

            TotalUncompressedSize = ds.ReadUInt64();
            TotalCompressedSize = ds.ReadUInt64();

            ChunkCount = ds.ReadUInt32();

            FileEntryCount = ds.ReadUInt32();
            FileMappingSize = ds.ReadUInt32();

            EncryptedCRC = ds.ReadUInt32();
            DecryptedCRC = ds.ReadUInt32();

            Flags = ds.ReadUInt32();

            for (uint i = FileMappingSize; i > 0; )
            {
                long start = ds.Position;

                FileMapping mapping = new FileMapping();
                mapping.Deserialize(ds);
                Mapping.Add(mapping);

                i -= (uint)(ds.Position - start);
            }
        }

    }

    /// <summary>
    /// Represents the manifest describing every file within a Steam2 depot.
    /// </summary>
    public sealed class Steam2Manifest
    {

        /// <summary>
        /// Represents a single file or folder within a manifest.
        /// </summary>
        public sealed class Node
        {
            /// <summary>
            /// The various attributes of a manifest node.
            /// </summary>
            [Flags]
            public enum Attribs
            {
                /// <summary>
                /// This node is a user configuration file.
                /// </summary>
                UserConfigurationFile = 0x1,
                /// <summary>
                /// This node is a launch file.
                /// </summary>
                LaunchFile = 0x2,
                /// <summary>
                /// This node is a locked file.
                /// </summary>
                LockedFile = 0x8,
                /// <summary>
                /// This node is a no-cache file.
                /// </summary>
                NoCacheFile = 0x20,
                /// <summary>
                /// This node is a versioned file.
                /// </summary>
                VersionedFile = 0x40,
                /// <summary>
                /// This node is a purge file.
                /// </summary>
                PurgeFile = 0x80,
                /// <summary>
                /// This node is an encrypted file.
                /// </summary>
                EncryptedFile = 0x100,
                /// <summary>
                /// This node is a read-only file.
                /// </summary>
                ReadOnly = 0x200,
                /// <summary>
                /// This node is a hidden file.
                /// </summary>
                HiddenFile = 0x400,
                /// <summary>
                /// This node is an executable file.
                /// </summary>
                ExecutableFile = 0x800,
                /// <summary>
                /// This node is a file, and not a folder.
                /// </summary>
                File = 0x4000,
            }

            /// <summary>
            /// Gets the size (in bytes, if this node is a file) or count (of inner nodes, if this node is a directory).
            /// </summary>
            public uint SizeOrCount { get; internal set; }

            /// <summary>
            /// Gets the FileID of this node.
            /// </summary>
            public int FileID { get; internal set; }
            /// <summary>
            /// Gets the attributes of this node.
            /// </summary>
            public Attribs Attributes { get; internal set; }
            /// <summary>
            /// Gets the index of the parent node.
            /// </summary>
            public int ParentIndex { get; internal set; }

            /// <summary>
            /// Gets the name of the node.
            /// </summary>
            public string Name { get; internal set; }
            /// <summary>
            /// Gets the full name of the node, built from parent nodes.
            /// </summary>
            public string FullName { get; internal set; }

            internal Steam2Manifest Parent { get; set; }
        }

        /// <summary>
        /// Gets the DepotID this manifest is for.
        /// </summary>
        public uint DepotID { get; private set; }
        /// <summary>
        /// Gets the depot version this manifest is for.
        /// </summary>
        public uint DepotVersion { get; private set; }

        /// <summary>
        /// Gets the count of nodes within this manifest.
        /// </summary>
        public uint NodeCount { get; private set; }
        /// <summary>
        /// Gets the count of files within this manifest.
        /// </summary>
        public uint FileCount { get; private set; }

        /// <summary>
        /// Gets the block size for this depot, used when downloading files.
        /// </summary>
        public uint BlockSize { get; private set; }

        /// <summary>
        /// Gets the depot checksum.
        /// </summary>
        public uint DepotChecksum { get; private set; }

        /// <summary>
        /// Gets the nodes within this manifest.
        /// </summary>
        public List<Node> Nodes { get; private set; }


        const uint HEADER_SIZE = 56; // the size of just the manifest header data
        const uint ENTRY_SIZE = 28; // the size of a single node


        internal Steam2Manifest( byte[] manifestBlob )
        {
            this.Nodes = new List<Node>();

            using ( DataStream ds = new DataStream( manifestBlob ) )
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

                // the start of the names section is after the header and every node
                uint namesStart = HEADER_SIZE + ( this.NodeCount * ENTRY_SIZE );

                for ( int x = 0 ; x < this.NodeCount ; ++x )
                {
                    ds.Seek( HEADER_SIZE + ( x * ENTRY_SIZE ), SeekOrigin.Begin );

                    // the first value within a node is the offset from the start of the names section
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
