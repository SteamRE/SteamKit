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
    /// Represents a Steam3 depot manifest.
    /// </summary>
    public sealed class DepotManifest
    {
        // Mono is nuts and has '/' for both dirchar and altdirchar, going against the lore
        private static char altDirChar = (Path.DirectorySeparatorChar == '\\') ? '/' : '\\';

        /// <summary>
        /// Represents a single chunk within a file.
        /// </summary>
        public class ChunkData
        {
            /// <summary>
            /// Gets the SHA-1 hash chunk id.
            /// </summary>
            public byte[] ChunkID { get; private set; }
            /// <summary>
            /// Gets the expected Adler32 checksum of this chunk.
            /// </summary>
            public byte[] Checksum { get; private set; }
            /// <summary>
            /// Gets the chunk offset.
            /// </summary>
            public ulong Offset { get; private set; }

            /// <summary>
            /// Gets the compressed length of this chunk.
            /// </summary>
            public uint CompressedLength { get; private set; }
            /// <summary>
            /// Gets the decompressed length of this chunk.
            /// </summary>
            public uint UncompressedLength { get; private set; }


            internal ChunkData( byte[] id, byte[] checksum, ulong offset, uint comp_length, uint uncomp_length )
            {
                this.ChunkID = id;
                this.Checksum = checksum;
                this.Offset = offset;

                this.CompressedLength = comp_length;
                this.UncompressedLength = uncomp_length;
            }
        }

        /// <summary>
        /// Represents a single file within a manifest.
        /// </summary>
        public class FileData
        {
            /// <summary>
            /// Gets the name of the file.
            /// </summary>
            public string FileName { get; internal set; }
            /// <summary>
            /// Gets the chunks that this file is composed of.
            /// </summary>
            public List<ChunkData> Chunks { get; private set; }

            /// <summary>
            /// Gets the file flags
            /// </summary>
            public EDepotFileFlag Flags { get; private set; }

            /// <summary>
            /// Gets the total size of this file.
            /// </summary>
            public ulong TotalSize { get; private set; }
            /// <summary>
            /// Gets the hash of this file.
            /// </summary>
            public byte[] FileHash { get; private set; }


            internal FileData(string filename, EDepotFileFlag flag, ulong size, byte[] hash, bool encrypted)
            {
                if (encrypted)
                    this.FileName = filename;
                else
                    this.FileName = filename.Replace(altDirChar, Path.DirectorySeparatorChar);

                this.Flags = flag;
                this.TotalSize = size;
                this.FileHash = hash;
                this.Chunks = new List<ChunkData>();
            }
        }

        /// <summary>
        /// Gets the list of files within this manifest.
        /// </summary>
        public List<FileData> Files { get; private set; }
        /// <summary>
        /// Gets a value indicating whether filenames within this depot are encrypted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filenames are encrypted; otherwise, <c>false</c>.
        /// </value>
        public bool FilenamesEncrypted { get; private set; }


        internal DepotManifest(byte[] data)
        {
            Deserialize(data);
        }


        /// <summary>
        /// Attempts to decrypts file names with the given encryption key.
        /// </summary>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <returns><c>true</c> if the file names were successfully decrypted; otherwise, <c>false</c>.</returns>
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

                file.FileName = Encoding.ASCII.GetString( filename ).TrimEnd( new char[] { '\0' } ).Replace(altDirChar, Path.DirectorySeparatorChar);
            }

            FilenamesEncrypted = false;
            return true;
        }

        void Deserialize(byte[] data)
        {
            using ( DataStream ds = new DataStream( data ) )
            {
                while ( ds.SizeRemaining() > 0 )
                {
                    uint magic = ds.ReadUInt32();
                    ds.Seek( -4, SeekOrigin.Current );

                    switch ( magic )
                    {
                        case Steam3Manifest.MAGIC:
                            Steam3Manifest binaryManifest = new Steam3Manifest( ds );
                            ParseBinaryManifest( binaryManifest );
                            break;

                            // todo: handle protobuf manifest?

                        default:
                            throw new NotImplementedException( string.Format( "Unrecognized magic value {0:X} in depot manifest.", magic ) );
                    }

                    uint marker = ds.ReadUInt32();
                    if ( marker != magic )
                        throw new InvalidDataException( "Unable to find end of message marker for depot manifest" );
                }
            }
        }

        void ParseBinaryManifest(Steam3Manifest manifest)
        {
            Files = new List<FileData>();
            FilenamesEncrypted = manifest.AreFileNamesEncrypted;

            foreach (var file_mapping in manifest.Mapping)
            {
                FileData filedata = new FileData(file_mapping.FileName, file_mapping.Flags, file_mapping.TotalSize, file_mapping.HashContent, FilenamesEncrypted);

                foreach (var chunk in file_mapping.Chunks)
                {
                    filedata.Chunks.Add( new ChunkData( chunk.ChunkGID, chunk.Checksum, chunk.Offset, chunk.CompressedSize, chunk.DecompressedSize ) );
                }

                Files.Add(filedata);
            }
        }

    }
}
