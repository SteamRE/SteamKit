/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using ProtoBuf;
using SteamKit2.Internal;
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

        private const int PROTOBUF_PAYLOAD_MAGIC = 0x71F617D0;
        private const int PROTOBUF_METADATA_MAGIC = 0x1F4812BE;
        private const int PROTOBUF_SIGNATURE_MAGIC = 0x1B81B817;
        private const int PROTOBUF_ENDOFMANIFEST_MAGIC = 0x32C415AB;

        /// <summary>
        /// Represents a single chunk within a file.
        /// </summary>
        public class ChunkData
        {
            /// <summary>
            /// Gets or sets the SHA-1 hash chunk id.
            /// </summary>
            public byte[]? ChunkID { get; set; }
            /// <summary>
            /// Gets or sets the expected Adler32 checksum of this chunk.
            /// </summary>
            public byte[]? Checksum { get; set; }
            /// <summary>
            /// Gets or sets the chunk offset.
            /// </summary>
            public ulong Offset { get; set; }

            /// <summary>
            /// Gets or sets the compressed length of this chunk.
            /// </summary>
            public uint CompressedLength { get; set; }
            /// <summary>
            /// Gets or sets the decompressed length of this chunk.
            /// </summary>
            public uint UncompressedLength { get; set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="ChunkData"/> class.
            /// </summary>
            public ChunkData()
            {
            }

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


            internal FileData(string filename, EDepotFileFlag flag, ulong size, byte[] hash, bool encrypted, int numChunks)
            {
                if (encrypted)
                {
                    this.FileName = filename;
                }
                else
                {
                    this.FileName = filename.Replace(altDirChar, Path.DirectorySeparatorChar);
                }

                this.Flags = flag;
                this.TotalSize = size;
                this.FileHash = hash;
                this.Chunks = new List<ChunkData>( numChunks );
            }
        }

        /// <summary>
        /// Gets the list of files within this manifest.
        /// </summary>
        public List<FileData>? Files { get; private set; }
        /// <summary>
        /// Gets a value indicating whether filenames within this depot are encrypted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filenames are encrypted; otherwise, <c>false</c>.
        /// </value>
        public bool FilenamesEncrypted { get; private set; }
        /// <summary>
        /// Gets the depot id.
        /// </summary>
        public uint DepotID { get; private set; }
        /// <summary>
        /// Gets the manifest id.
        /// </summary>
        public ulong ManifestGID { get; private set; }
        /// <summary>
        /// Gets the depot creation time.
        /// </summary>
        public DateTime CreationTime { get; private set; }
        /// <summary>
        /// Gets the total uncompressed size of all files in this depot.
        /// </summary>
        public ulong TotalUncompressedSize { get; private set; }
        /// <summary>
        /// Gets the total compressed size of all files in this depot.
        /// </summary>
        public ulong TotalCompressedSize { get; private set; }


        internal DepotManifest(byte[] data)
        {
            InternalDeserialize(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepotManifest"/> class.
        /// Depot manifests may come from the Steam CDN or from Steam/depotcache/ manifest files.
        /// </summary>
        /// <param name="data">Raw depot manifest data to deserialize.</param>
        /// <exception cref="InvalidDataException">Thrown if the given data is not something recognizable.</exception>
        public static DepotManifest Deserialize(byte[] data) => new DepotManifest(data);

        /// <summary>
        /// Attempts to decrypts file names with the given encryption key.
        /// </summary>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <returns><c>true</c> if the file names were successfully decrypted; otherwise, <c>false</c>.</returns>
        public bool DecryptFilenames(byte[] encryptionKey)
        {
            if (!FilenamesEncrypted)
            {
                return true;
            }

            DebugLog.Assert( Files != null, nameof( DepotManifest ), "Files was null when attempting to decrypt filenames." );

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

                file.FileName = Encoding.UTF8.GetString( filename ).TrimEnd( new char[] { '\0' } ).Replace(altDirChar, Path.DirectorySeparatorChar);
            }

            FilenamesEncrypted = false;
            return true;
        }

        void InternalDeserialize(byte[] data)
        {
            ContentManifestPayload? payload = null;
            ContentManifestMetadata? metadata = null;
            ContentManifestSignature? signature = null;

            using ( var ms = new MemoryStream( data ) )
            using ( var br = new BinaryReader( ms ) )
            {
                while ( ( ms.Length - ms.Position ) > 0 )
                {
                    uint magic = br.ReadUInt32();

                    switch ( magic )
                    {
                        case Steam3Manifest.MAGIC:
                            ms.Seek(-4, SeekOrigin.Current);
                            Steam3Manifest binaryManifest = new Steam3Manifest( br );
                            ParseBinaryManifest( binaryManifest );

                            uint marker = br.ReadUInt32();
                            if ( marker != magic )
                                throw new InvalidDataException( "Unable to find end of message marker for depot manifest" );
                            break;

                        case DepotManifest.PROTOBUF_PAYLOAD_MAGIC:
                            uint payload_length = br.ReadUInt32();
                            byte[] payload_bytes = br.ReadBytes( (int)payload_length );
                            using ( var ms_payload = new MemoryStream( payload_bytes ) ) 
                                payload = Serializer.Deserialize<ContentManifestPayload>( ms_payload );
                            break;

                        case DepotManifest.PROTOBUF_METADATA_MAGIC:
                            uint metadata_length = br.ReadUInt32();
                            byte[] metadata_bytes = br.ReadBytes( (int)metadata_length );
                            using ( var ms_metadata = new MemoryStream( metadata_bytes ) )
                                metadata = Serializer.Deserialize<ContentManifestMetadata>( ms_metadata );
                            break;

                        case DepotManifest.PROTOBUF_SIGNATURE_MAGIC:
                            uint signature_length = br.ReadUInt32();
                            byte[] signature_bytes = br.ReadBytes( (int)signature_length );
                            using ( var ms_signature = new MemoryStream( signature_bytes ) )
                                signature = Serializer.Deserialize<ContentManifestSignature>( ms_signature );
                            break;

                        case DepotManifest.PROTOBUF_ENDOFMANIFEST_MAGIC:
                            break;

                        default:
                            throw new InvalidDataException( $"Unrecognized magic value {magic:X} in depot manifest." );
                    }
                }
            }

            if (payload != null && metadata != null && signature != null)
            {
                ParseProtobufManifestMetadata(metadata);
                ParseProtobufManifestPayload(payload);
            }
            else
            {
                throw new InvalidDataException("Missing ContentManifest sections required for parsing depot manifest");
            }
        }

        void ParseBinaryManifest(Steam3Manifest manifest)
        {
            Files = new List<FileData>( manifest.Mapping.Count );
            FilenamesEncrypted = manifest.AreFileNamesEncrypted;
            DepotID = manifest.DepotID;
            ManifestGID = manifest.ManifestGID;
            CreationTime = manifest.CreationTime;
            TotalUncompressedSize = manifest.TotalUncompressedSize;
            TotalCompressedSize = manifest.TotalCompressedSize;

            foreach (var file_mapping in manifest.Mapping)
            {
                FileData filedata = new FileData(file_mapping.FileName!, file_mapping.Flags, file_mapping.TotalSize, file_mapping.HashContent!, FilenamesEncrypted, file_mapping.Chunks!.Length);

                foreach (var chunk in file_mapping.Chunks)
                {
                    filedata.Chunks.Add( new ChunkData( chunk.ChunkGID!, chunk.Checksum!, chunk.Offset, chunk.CompressedSize, chunk.DecompressedSize ) );
                }

                Files.Add(filedata);
            }
        }

        void ParseProtobufManifestPayload(ContentManifestPayload payload)
        {
            Files = new List<FileData>(payload.mappings.Count);

            foreach (var file_mapping in payload.mappings)
            {
                FileData filedata = new FileData(file_mapping.filename, (EDepotFileFlag)file_mapping.flags, file_mapping.size, file_mapping.sha_content, FilenamesEncrypted, file_mapping.chunks.Count);

                foreach (var chunk in file_mapping.chunks)
                {
                    filedata.Chunks.Add( new ChunkData( chunk.sha, BitConverter.GetBytes(chunk.crc), chunk.offset, chunk.cb_compressed, chunk.cb_original ) );
                }

                Files.Add(filedata);
            }
        }

        void ParseProtobufManifestMetadata(ContentManifestMetadata metadata)
        {
            FilenamesEncrypted = metadata.filenames_encrypted;
            DepotID = metadata.depot_id;
            ManifestGID = metadata.gid_manifest;
            CreationTime = DateUtils.DateTimeFromUnixTime( metadata.creation_time );
            TotalUncompressedSize = metadata.cb_disk_original;
            TotalCompressedSize = metadata.cb_disk_compressed;
        }
    }
}
