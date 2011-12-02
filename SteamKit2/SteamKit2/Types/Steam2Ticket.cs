using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public sealed class Steam2Ticket
    {
        public sealed class Entry
        {
            public ushort Magic { get; set; } // 0x0400, probably entry magic? idk
            // public uint Length { get; set; }
            public uint Index { get; set; }
            public byte[] Data { get; set; }

            internal void Deserialize( DataStream ds )
            {
                Magic = ds.ReadUInt16();

                uint length = ds.ReadUInt32();
                Index = ds.ReadUInt32();

                Data = ds.ReadBytes( length );
            }
        }

        // header stuff
        public ushort Magic { get; set; } // 0x0150, more crazy magic

        public uint Length { get; set; }
        public uint Unknown1 { get; set; } // who knows!

        public List<Entry> Entries { get; private set; }


        public Steam2Ticket( byte[] blob )
        {
            Entries = new List<Entry>();

            using ( var ds = new DataStream( blob ) )
            {
                Magic = ds.ReadUInt16();

                Length = ds.ReadUInt32();
                Unknown1 = ds.ReadUInt32();

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
