/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using ProtoBuf;
using SteamKit2.Internal;
using System.Collections.Generic;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Represents a depot delta patch.
    /// </summary>
    public sealed class DepotDeltaChunks
    {
        /// <summary>
        /// Represents a single delta chunk.
        /// </summary>
        public class DeltaChunkData
        {
            /// <summary>
            /// Gets or sets the chunk content.
            /// </summary>
            public byte[] Chunk { get; set; }
            /// <summary>
            /// Gets or sets the expected SHA-1 hash of the original chunk.
            /// </summary>
            public byte[] SourceSHA { get; set; }
            /// <summary>
            /// Gets or sets the expected SHA-1 hash of the target chunk.
            /// </summary>
            public byte[] TargetSHA { get; set; }
            /// <summary>
            /// Gets or sets the length of this chunk.
            /// </summary>
            public uint Length { get; set; }
            /// <summary>
            /// Gets or sets the patch method.
            /// </summary>
            public uint PatchMethod { get; set; }


            /// <summary>
            /// Initializes a new instance of the <see cref="DeltaChunkData"/> class.
            /// </summary>
            public DeltaChunkData()
            {
            }

            // TODO: Figure out patch_method and make it an enum
            internal DeltaChunkData( byte[] chunk, uint patchMethod, uint length, byte[] sourceSHA, byte[] targetSHA )
            {
                this.Chunk = chunk;
                this.Length = length;
                this.SourceSHA = sourceSHA;
                this.TargetSHA = targetSHA;
                this.PatchMethod = patchMethod;
            }
        }

        /// <summary>
        /// Gets the list of chunk patches.
        /// </summary>
        public List<DeltaChunkData> Chunks { get; private set; }
        /// <summary>
        /// Gets the depot id.
        /// </summary>
        public uint DepotID { get; private set; }
        /// <summary>
        /// Gets the original manifest id.
        /// </summary>
        public ulong SourceManifestGID { get; private set; }
        /// <summary>
        /// Gets the target manifest id.
        /// </summary>
        public ulong TargetManifestGID { get; private set; }

        internal DepotDeltaChunks( byte[] payload )
        {
            Deserialize( payload );
        }

        void Deserialize( byte[] payload )
        {
            ContentDeltaChunks patch;

            using ( var ms_payload = new MemoryStream( payload ) )
            {
                patch = Serializer.Deserialize<ContentDeltaChunks>( ms_payload );
            }

            this.DepotID = patch.depot_id;
            this.SourceManifestGID = patch.manifest_id_source;
            this.TargetManifestGID = patch.manifest_id_target;

            foreach ( var chunk in patch.deltaChunks )
            {
                this.Chunks.Add( new DeltaChunkData( chunk.chunk, chunk.patch_method, chunk.size_original, chunk.sha_source, chunk.sha_target ) );
            }
        }
    }
}
