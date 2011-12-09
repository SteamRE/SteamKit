/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    // Steam 3 Depot Manifest
    public class DepotManifest
    {
        public class ChunkData
        {
            public byte[] ChunkID { get; private set; }
            public byte[] CRC { get; private set; }
            public ulong Offset { get; private set; }

            internal ChunkData(byte[] id, byte[] crc, ulong offset)
            {
                this.ChunkID = id;
                this.CRC = crc;
                this.Offset = offset;
            }
        }

        public class FileData
        {
            public string FileName { get; private set; }
            public List<ChunkData> Chunks { get; private set; }

            internal FileData(string filename)
            {
                this.FileName = filename;
                this.Chunks = new List<ChunkData>();
            }

            internal void AddChunk(ChunkData chunk)
            {
                this.Chunks.Add(chunk);
            }
        }

        public List<FileData> Files { get; private set; }
        public bool FilenamesEncrypted { get; private set; }

        private DepotManifest()
        {
        }

        public DepotManifest(byte[] data)
        {
            Deserialize(data);
        }

        void Deserialize(byte[] data)
        {
            using (DataStream ds = new DataStream(data))
                while (ds.SizeRemaining() > 0)
                {
                    uint magic = ds.ReadUInt32();
                    ds.Seek(-4, SeekOrigin.Current);

                    switch (magic)
                    {
                        case Steam3Manifest.MAGIC:
                            Steam3Manifest binaryManifest = new Steam3Manifest(ds);

                            uint marker = ds.ReadUInt32();
                            if (marker != magic)
                                Console.WriteLine("Unable to find end of message marker for Steam3Manifest");
                            else
                                ParseBinaryManifest(binaryManifest);
                            break;
                        default:
                            Console.WriteLine("Unrecognized magic value {0:X} in depot manifest.", magic);
                            break;
                    }
                }
        }

        void ParseBinaryManifest(Steam3Manifest manifest)
        {
            Files = new List<FileData>();
            FilenamesEncrypted = manifest.AreFileNamesEncrypted;

            foreach (var file_mapping in manifest.Mapping)
            {
                FileData filedata = new FileData(file_mapping.FileName);

                foreach (var chunk in file_mapping.Chunks)
                {
                    filedata.AddChunk(new ChunkData(chunk.ChunkGID, chunk.CRC, chunk.Offset));
                }

                Files.Add(filedata);
            }
        }

    }
}
