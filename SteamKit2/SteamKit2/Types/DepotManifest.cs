/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf;
using SteamKit2.Internal;

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
            public uint Checksum { get; set; }
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

            /// <summary>
            /// Initializes a new instance of the <see cref="ChunkData"/> class with specified values.
            /// </summary>
            public ChunkData( byte[] id, uint checksum, ulong offset, uint comp_length, uint uncomp_length )
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
            public string FileName { get; set; }
            /// <summary>
            /// Gets SHA-1 hash of this file's name.
            /// </summary>
            public byte[] FileNameHash { get; set; }
            /// <summary>
            /// Gets the chunks that this file is composed of.
            /// </summary>
            public List<ChunkData> Chunks { get; set; }

            /// <summary>
            /// Gets the file flags
            /// </summary>
            public EDepotFileFlag Flags { get; set; }

            /// <summary>
            /// Gets the total size of this file.
            /// </summary>
            public ulong TotalSize { get; set; }
            /// <summary>
            /// Gets SHA-1 hash of this file.
            /// </summary>
            public byte[] FileHash { get; set; }
            /// <summary>
            /// Gets symlink target of this file.
            /// </summary>
            public string? LinkTarget { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileData"/> class.
            /// </summary>
            public FileData()
            {
                FileName = string.Empty;
                FileNameHash = [];
                Chunks = [];
                FileHash = [];
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileData"/> class with specified values.
            /// </summary>
            public FileData(string filename, byte[] filenameHash, EDepotFileFlag flag, ulong size, byte[] hash, string linkTarget, bool encrypted, int numChunks)
            {
                if (encrypted)
                {
                    this.FileName = filename;
                }
                else
                {
                    this.FileName = filename.Replace(altDirChar, Path.DirectorySeparatorChar);
                }

                this.FileNameHash = filenameHash;
                this.Flags = flag;
                this.TotalSize = size;
                this.FileHash = hash;
                this.Chunks = new List<ChunkData>( numChunks );
                this.LinkTarget = linkTarget;
            }
        }

        /// <summary>
        /// Gets the list of files within this manifest.
        /// </summary>
        public List<FileData>? Files { get; set; }
        /// <summary>
        /// Gets a value indicating whether filenames within this depot are encrypted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filenames are encrypted; otherwise, <c>false</c>.
        /// </value>
        public bool FilenamesEncrypted { get; set; }
        /// <summary>
        /// Gets the depot id.
        /// </summary>
        public uint DepotID { get; set; }
        /// <summary>
        /// Gets the manifest id.
        /// </summary>
        public ulong ManifestGID { get; set; }
        /// <summary>
        /// Gets the depot creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// Gets the total uncompressed size of all files in this depot.
        /// </summary>
        public ulong TotalUncompressedSize { get; set; }
        /// <summary>
        /// Gets the total compressed size of all files in this depot.
        /// </summary>
        public ulong TotalCompressedSize { get; set; }
        /// <summary>
        /// Gets CRC-32 checksum of encrypted manifest payload.
        /// </summary>
        public uint EncryptedCRC { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepotManifest"/> class.
        /// Depot manifests may come from the Steam CDN or from Steam/depotcache/ manifest files.
        /// </summary>
        /// <param name="stream">Raw depot manifest stream to deserialize.</param>
        /// <exception cref="InvalidDataException">Thrown if the given data is not something recognizable.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the given data is not complete.</exception>
        public static DepotManifest Deserialize( Stream stream )
        {
            var manifest = new DepotManifest();
            manifest.InternalDeserialize( stream );
            return manifest;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepotManifest"/> class.
        /// Depot manifests may come from the Steam CDN or from Steam/depotcache/ manifest files.
        /// </summary>
        /// <param name="data">Raw depot manifest data to deserialize.</param>
        /// <exception cref="InvalidDataException">Thrown if the given data is not something recognizable.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the given data is not complete.</exception>
        public static DepotManifest Deserialize( byte[] data )
        {
            using var ms = new MemoryStream( data );
            return Deserialize( ms );
        }

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
            DebugLog.Assert( encryptionKey.Length == 32, nameof( DepotManifest ), $"Decrypt filnames used with non 32 byte key!" );

            // This was copypasted from <see cref="CryptoHelper.SymmetricDecrypt"/> to avoid allocating Aes instance for every filename
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = encryptionKey;

            Span<byte> iv = stackalloc byte[ 16 ];
            var bufferDecoded = ArrayPool<byte>.Shared.Rent( 256 );
            var bufferDecrypted = ArrayPool<byte>.Shared.Rent( 256 );

            try
            {
                foreach ( var file in Files )
                {
                    if ( !TryDecryptName( file.FileName, iv, out var name ) )
                    {
                        return false;
                    }

                    file.FileName = name;

                    if ( !string.IsNullOrEmpty( file.LinkTarget ) )
                    {
                        if ( !TryDecryptName( file.LinkTarget, iv, out var linkName ) )
                        {
                            return false;
                        }

                        file.LinkTarget = linkName;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( bufferDecoded );
                ArrayPool<byte>.Shared.Return( bufferDecrypted );
            }

            // Sort file entries alphabetically because that's what Steam does
            // TODO: Doesn't match Steam sorting if there are non-ASCII names present
            Files.Sort( ( f1, f2 ) => StringComparer.OrdinalIgnoreCase.Compare( f1.FileName, f2.FileName ) );

            FilenamesEncrypted = false;
            return true;

            bool TryDecryptName( string name, Span<byte> iv, [MaybeNullWhen(false)] out string decoded )
            {
                decoded = null;

                var decodedLength = name.Length / 4 * 3; // This may be higher due to padding

                // Majority of filenames are short, even when they are encrypted and base64 encoded,
                // so this resize will be hit *very* rarely
                if ( decodedLength > bufferDecoded.Length )
                {
                    ArrayPool<byte>.Shared.Return( bufferDecoded );
                    bufferDecoded = ArrayPool<byte>.Shared.Rent( decodedLength );

                    ArrayPool<byte>.Shared.Return( bufferDecrypted );
                    bufferDecrypted = ArrayPool<byte>.Shared.Rent( decodedLength );
                }

                if ( !Convert.TryFromBase64Chars( name, bufferDecoded, out decodedLength ) )
                {
                    DebugLog.Assert( false, nameof( DepotManifest ), "Failed to base64 decode the filename." );
                    return false;
                }

                int filenameLength;
                try
                {
                    var encryptedFilename = bufferDecoded.AsSpan()[ ..decodedLength ];
                    aes.DecryptEcb( encryptedFilename[ ..iv.Length ], iv, PaddingMode.None );
                    filenameLength = aes.DecryptCbc( encryptedFilename[ iv.Length.. ], iv, bufferDecrypted, PaddingMode.PKCS7 );
                }
                catch ( Exception )
                {
                    DebugLog.Assert( false, nameof( DepotManifest ), "Failed to decrypt the filename." );
                    return false;
                }

                // Trim the ending null byte, safe for UTF-8
                if ( filenameLength > 0 && bufferDecrypted[ filenameLength ] == 0 )
                {
                    filenameLength--;
                }

                // ASCII is subset of UTF-8, so it safe to replace the raw bytes here
                MemoryExtensions.Replace( bufferDecrypted.AsSpan(), ( byte )altDirChar, ( byte )Path.DirectorySeparatorChar );

                decoded = Encoding.UTF8.GetString( bufferDecrypted, 0, filenameLength );
                return true;
            }
        }

        /// <summary>
        /// Serializes depot manifest and saves the output to a file.
        /// </summary>
        /// <param name="filename">Output file name.</param>
        public void SaveToFile( string filename )
        {
            using var fs = File.Open( filename, FileMode.Create );
            Serialize( fs );
        }

        /// <summary>
        /// Loads binary manifest from a file and deserializes it.
        /// </summary>
        /// <param name="filename">Input file name.</param>
        /// <returns><c>DepotManifest</c> object if deserialization was successful; otherwise, <c>null</c>.</returns>
        /// <exception cref="InvalidDataException">Thrown if the given data is not something recognizable.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the given data is not complete.</exception>
        public static DepotManifest? LoadFromFile( string filename )
        {
            if ( !File.Exists( filename ) )
                return null;

            using var fs = File.Open( filename, FileMode.Open );
            return Deserialize( fs );
        }

        void InternalDeserialize( Stream stream )
        {
            ContentManifestPayload? payload = null;
            ContentManifestMetadata? metadata = null;
            ContentManifestSignature? signature = null;

            using var br = new BinaryReader( stream, Encoding.UTF8, leaveOpen: true );

            while ( true )
            {
                uint magic = br.ReadUInt32();

                if ( magic == DepotManifest.PROTOBUF_ENDOFMANIFEST_MAGIC )
                {
                    break;
                }

                switch ( magic )
                {
                    case Steam3Manifest.MAGIC:
                        Steam3Manifest binaryManifest = new Steam3Manifest();
                        binaryManifest.Deserialize( br );
                        ParseBinaryManifest( binaryManifest );

                        uint marker = br.ReadUInt32();
                        if ( marker != magic )
                            throw new InvalidDataException( "Unable to find end of message marker for depot manifest" );

                        // This is an intentional return because v4 manifest does not have the separate sections
                        // and it will be parsed by ParseBinaryManifest. If we get here, the entire buffer has been already processed.
                        return;

                    case DepotManifest.PROTOBUF_PAYLOAD_MAGIC:
                        uint payload_length = br.ReadUInt32();
                        payload = Serializer.Deserialize<ContentManifestPayload>( stream, length: payload_length );
                        break;

                    case DepotManifest.PROTOBUF_METADATA_MAGIC:
                        uint metadata_length = br.ReadUInt32();
                        metadata = Serializer.Deserialize<ContentManifestMetadata>( stream, length: metadata_length );
                        break;

                    case DepotManifest.PROTOBUF_SIGNATURE_MAGIC:
                        uint signature_length = br.ReadUInt32();
                        signature = Serializer.Deserialize<ContentManifestSignature>( stream, length: signature_length );
                        break;

                    default:
                        throw new InvalidDataException( $"Unrecognized magic value {magic:X} in depot manifest." );
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
            EncryptedCRC = manifest.EncryptedCRC;

            foreach (var file_mapping in manifest.Mapping)
            {
                FileData filedata = new FileData(file_mapping.FileName!, file_mapping.HashFileName!, file_mapping.Flags, file_mapping.TotalSize, file_mapping.HashContent!, "", FilenamesEncrypted, file_mapping.Chunks!.Length);

                foreach (var chunk in file_mapping.Chunks)
                {
                    filedata.Chunks.Add( new ChunkData( chunk.ChunkGID!, chunk.Checksum, chunk.Offset, chunk.CompressedSize, chunk.DecompressedSize ) );
                }

                Files.Add(filedata);
            }
        }

        void ParseProtobufManifestPayload(ContentManifestPayload payload)
        {
            Files = new List<FileData>(payload.mappings.Count);

            foreach (var file_mapping in payload.mappings)
            {
                FileData filedata = new FileData(file_mapping.filename, file_mapping.sha_filename, (EDepotFileFlag)file_mapping.flags, file_mapping.size, file_mapping.sha_content, file_mapping.linktarget, FilenamesEncrypted, file_mapping.chunks.Count);

                foreach (var chunk in file_mapping.chunks)
                {
                    filedata.Chunks.Add( new ChunkData( chunk.sha, chunk.crc, chunk.offset, chunk.cb_compressed, chunk.cb_original ) );
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
            EncryptedCRC = metadata.crc_encrypted;
        }

        class ChunkIdComparer : IEqualityComparer<byte[]>
        {
            public bool Equals( byte[]? x, byte[]? y )
            {
                if ( ReferenceEquals( x, y ) ) return true;
                if ( x == null || y == null ) return false;
                return x.SequenceEqual( y );
            }

            public int GetHashCode( byte[] obj )
            {
                ArgumentNullException.ThrowIfNull( obj );

                // ChunkID is SHA-1, so we can just use the first 4 bytes
                return BitConverter.ToInt32( obj, 0 );
            }
        }

        /// <summary>
        /// Serializes the depot manifest into the provided output stream.
        /// </summary>
        /// <param name="output">The stream to which the serialized depot manifest will be written.</param>
        public void Serialize( Stream output )
        {
            DebugLog.Assert( Files != null, nameof( DepotManifest ), "Files was null when attempting to serialize manifest." );

            var payload = new ContentManifestPayload();
            var uniqueChunks = new HashSet<byte[]>( new ChunkIdComparer() );

            foreach ( var file in Files )
            {
                var protofile = new ContentManifestPayload.FileMapping();
                protofile.size = file.TotalSize;
                protofile.flags = ( uint )file.Flags;
                if ( FilenamesEncrypted )
                {
                    // Assume the name is unmodified
                    protofile.filename = file.FileName;
                    protofile.sha_filename = file.FileNameHash;
                }
                else
                {
                    protofile.filename = file.FileName.Replace( '/', '\\' );
                    protofile.sha_filename = SHA1.HashData( Encoding.UTF8.GetBytes( file.FileName.Replace( '/', '\\' ).ToLowerInvariant() ) );
                }
                protofile.sha_content = file.FileHash;
                if ( !string.IsNullOrWhiteSpace( file.LinkTarget ) )
                {
                    protofile.linktarget = file.LinkTarget;
                }

                foreach ( var chunk in file.Chunks )
                {
                    var protochunk = new ContentManifestPayload.FileMapping.ChunkData();
                    protochunk.sha = chunk.ChunkID;
                    protochunk.crc = chunk.Checksum;
                    protochunk.offset = chunk.Offset;
                    protochunk.cb_original = chunk.UncompressedLength;
                    protochunk.cb_compressed = chunk.CompressedLength;

                    protofile.chunks.Add( protochunk );
                    uniqueChunks.Add( chunk.ChunkID! );
                }

                payload.mappings.Add( protofile );
            }

            var metadata = new ContentManifestMetadata();
            metadata.depot_id = DepotID;
            metadata.gid_manifest = ManifestGID;
            metadata.creation_time = ( uint )DateUtils.DateTimeToUnixTime( CreationTime );
            metadata.filenames_encrypted = FilenamesEncrypted;
            metadata.cb_disk_original = TotalUncompressedSize;
            metadata.cb_disk_compressed = TotalCompressedSize;
            metadata.unique_chunks = ( uint )uniqueChunks.Count;

            // Calculate payload CRC
            using ( var ms_payload = new MemoryStream() )
            {
                Serializer.Serialize<ContentManifestPayload>( ms_payload, payload );

                int len = ( int )ms_payload.Length;
                byte[] data = new byte[ 4 + len ];
                Buffer.BlockCopy( BitConverter.GetBytes( len ), 0, data, 0, 4 );
                Buffer.BlockCopy( ms_payload.ToArray(), 0, data, 4, len );
                uint crc32 = Crc32.HashToUInt32( data );

                if ( FilenamesEncrypted )
                {
                    metadata.crc_encrypted = crc32;
                    metadata.crc_clear = 0;
                }
                else
                {
                    metadata.crc_encrypted = EncryptedCRC;
                    metadata.crc_clear = crc32;
                }
            }

            using var bw = new BinaryWriter( output, Encoding.Default, true );

            // Write Protobuf payload
            using ( var ms_payload = new MemoryStream() )
            {
                Serializer.Serialize<ContentManifestPayload>( ms_payload, payload );
                bw.Write( DepotManifest.PROTOBUF_PAYLOAD_MAGIC );
                bw.Write( ( int )ms_payload.Length );
                bw.Write( ms_payload.GetBuffer().AsSpan( 0, ( int )ms_payload.Length ) );
            }

            // Write Protobuf metadata
            using ( var ms_metadata = new MemoryStream() )
            {
                Serializer.Serialize<ContentManifestMetadata>( ms_metadata, metadata );
                bw.Write( DepotManifest.PROTOBUF_METADATA_MAGIC );
                bw.Write( ( int )ms_metadata.Length );
                bw.Write( ms_metadata.GetBuffer().AsSpan( 0, ( int )ms_metadata.Length ) );
            }

            // Write empty signature section
            bw.Write( DepotManifest.PROTOBUF_SIGNATURE_MAGIC );
            bw.Write( 0 );

            // Write EOF marker
            bw.Write( DepotManifest.PROTOBUF_ENDOFMANIFEST_MAGIC );
        }
    }
}
