/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System.Collections.Generic;

namespace SteamKit2
{
    /// <summary>
    /// Represents a Steam2 authentication ticket container used for downloading authenticated content from Steam2 servers.
    /// </summary>
    public sealed class Steam2Ticket
    {
        /// <summary>
        /// Represents a single data entry within the ticket container.
        /// </summary>
        public sealed class Entry
        {
            /// <summary>
            /// Gets the magic.
            /// </summary>
            public ushort Magic { get; private set; } // 0x0400, probably entry magic? idk

            /// <summary>
            /// Gets the index of this entry.
            /// </summary>
            public uint Index { get; private set; }

            /// <summary>
            /// Gets the data of this entry.
            /// </summary>
            public byte[] Data { get; private set; }


            internal void Deserialize( DataStream ds )
            {
                Magic = ds.ReadUInt16();

                uint length = ds.ReadUInt32();
                Index = ds.ReadUInt32();

                Data = ds.ReadBytes( length );
            }
        }


        /// <summary>
        /// Gets the magic of the container.
        /// </summary>
        public ushort Magic { get; private set; } // 0x0150, more crazy magic?

        /// <summary>
        /// Gets the length, in bytes, of the container.
        /// </summary>
        public uint Length { get; private set; }

        uint unknown1;

        /// <summary>
        /// Gets the <see cref="Entry">entries</see> within this container.
        /// </summary>
        public List<Entry> Entries { get; private set; }


        internal Steam2Ticket( byte[] blob )
        {
            Entries = new List<Entry>();

            using ( var ds = new DataStream( blob ) )
            {
                Magic = ds.ReadUInt16();

                Length = ds.ReadUInt32();
                unknown1 = ds.ReadUInt32();

                while ( ds.SizeRemaining() > 0 )
                {
                    var entry = new Entry();
                    entry.Deserialize( ds );

                    Entries.Add( entry );
                }
            }
        }
    }
}
