/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
                public byte[]? ChunkGID { get; set; } // sha1 hash for this chunk

                public byte[]? Checksum { get; set; }
                public ulong Offset { get; set; }

                public uint DecompressedSize { get; set; }
                public uint CompressedSize { get; set; }


                internal void Deserialize( BinaryReader ds )
                {
                    ChunkGID = ds.ReadBytes( 20 );

                    Checksum = ds.ReadBytes( 4 );

                    Offset = ds.ReadUInt64();

                    DecompressedSize = ds.ReadUInt32();
                    CompressedSize = ds.ReadUInt32();
                }
            }

            public string? FileName { get; set; }

            public ulong TotalSize { get; set; }
            public EDepotFileFlag Flags { get; set; }

            public byte[]? HashFileName { get; set; }
            public byte[]? HashContent { get; set; }

            public uint NumChunks { get; set; }
            public Chunk[]? Chunks { get; private set; }

            public FileMapping()
            {
            }


            internal void Deserialize( BinaryReader ds )
            {
                FileName = ds.BaseStream.ReadNullTermString( Encoding.UTF8 );

                TotalSize = ds.ReadUInt64();

                Flags = (EDepotFileFlag)ds.ReadUInt32();

                HashContent = ds.ReadBytes( 20 );
                HashFileName = ds.ReadBytes( 20 );

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

        [NotNull]
        public List<FileMapping>? Mapping { get; private set; }

        public Steam3Manifest(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Deserialize(data);
        }

        internal Steam3Manifest(BinaryReader data)
        {
            Deserialize(data);
        }

        void Deserialize(byte[] data)
        {
            using ( var ms = new MemoryStream( data ) )
            using ( var br = new BinaryReader( ms ) )
            {
                Deserialize( br );
            }
        }

        void Deserialize( BinaryReader ds )
        {
            Magic = ds.ReadUInt32();

            if (Magic != MAGIC)
            {
                throw new InvalidDataException("data is not a valid steam3 manifest: incorrect magic.");
            }

            Version = ds.ReadUInt32();

            DepotID = ds.ReadUInt32();

            ManifestGID = ds.ReadUInt64();
            CreationTime = DateUtils.DateTimeFromUnixTime( ds.ReadUInt32() );

            AreFileNamesEncrypted = ds.ReadUInt32() != 0;

            TotalUncompressedSize = ds.ReadUInt64();
            TotalCompressedSize = ds.ReadUInt64();

            ChunkCount = ds.ReadUInt32();

            FileEntryCount = ds.ReadUInt32();
            FileMappingSize = ds.ReadUInt32();

            Mapping = new List<FileMapping>( ( int )FileMappingSize );

            EncryptedCRC = ds.ReadUInt32();
            DecryptedCRC = ds.ReadUInt32();

            Flags = ds.ReadUInt32();

            for (uint i = FileMappingSize; i > 0; )
            {
                long start = ds.BaseStream.Position;

                FileMapping mapping = new FileMapping();
                mapping.Deserialize(ds);
                Mapping.Add(mapping);

                i -= (uint)(ds.BaseStream.Position - start);
            }
        }

    }

}
