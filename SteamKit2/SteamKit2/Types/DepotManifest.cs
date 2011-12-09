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

            public uint CompressedLength { get; private set; }
            public uint UncompressedLength { get; private set; }

            internal ChunkData(byte[] id, byte[] crc, ulong offset, uint comp_length, uint uncomp_length)
            {
                this.ChunkID = id;
                this.CRC = crc;
                this.Offset = offset;

                this.CompressedLength = comp_length;
                this.UncompressedLength = uncomp_length;
            }
        }

        public class FileData
        {
            public string FileName { get; private set; }
            public List<ChunkData> Chunks { get; private set; }

            public ulong TotalSize { get; private set; }

            internal FileData(string filename, ulong size)
            {
                this.FileName = filename;
                this.TotalSize = size;
                this.Chunks = new List<ChunkData>();
            }

            internal void AddChunk(ChunkData chunk)
            {
                this.Chunks.Add(chunk);
            }

            internal void SetName(string name)
            {
                this.FileName = name;
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

        public bool DecryptFilenames(byte[] encryptionKey)
        {
            if (!FilenamesEncrypted)
                return true;

            foreach (var file in Files)
            {
                byte[] enc_filename = Convert.FromBase64String(file.FileName);
                byte[] filename;
                try
                {
                    filename = CryptoHelper.SymmetricDecrypt(enc_filename, encryptionKey);
                }
                catch (Exception)
                {
                    return false;
                }

                file.SetName(Encoding.ASCII.GetString(filename).TrimEnd(new char[] { '\0' }));
            }

            FilenamesEncrypted = false;
            return true;
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
                            ParseBinaryManifest(binaryManifest);
                            break;
                        default:
                            Console.WriteLine("Unrecognized magic value {0:X} in depot manifest.", magic);
                            return;
                    }

                    uint marker = ds.ReadUInt32();
                    if (marker != magic)
                        Console.WriteLine("Unable to find end of message marker for depot manifest");
                }
        }

        void ParseBinaryManifest(Steam3Manifest manifest)
        {
            Files = new List<FileData>();
            FilenamesEncrypted = manifest.AreFileNamesEncrypted;

            foreach (var file_mapping in manifest.Mapping)
            {
                FileData filedata = new FileData(file_mapping.FileName, file_mapping.TotalSize);

                foreach (var chunk in file_mapping.Chunks)
                {
                    filedata.AddChunk(new ChunkData(chunk.ChunkGID, chunk.CRC, chunk.Offset, chunk.CompressedSize, chunk.DecompressedSize));
                }

                Files.Add(filedata);
            }
        }

    }
}
