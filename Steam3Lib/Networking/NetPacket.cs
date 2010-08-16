using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLib
{
    struct MsgSegment
    {
        public uint DataSequence;
        public byte[] Data;
    }

    class NetPacket
    {
        uint seqStart;
        uint seqEnd;

        uint numPkts;

        List<MsgSegment> msgSegments;


        public bool IsCompleted { get; private set; }


        public NetPacket( uint seqStart, uint numPkts )
        {
            msgSegments = new List<MsgSegment>();

            this.seqStart = seqStart;
            this.seqEnd = seqStart + numPkts;

            this.numPkts = numPkts;

            this.IsCompleted = false;
        }


        public void AddData( UdpHeader udpHdr, byte[] data )
        {
            if ( HasData( udpHdr ) )
                return;

            MsgSegment msgSeg = new MsgSegment();

            msgSeg.Data = data;
            msgSeg.DataSequence = udpHdr.SequenceThis;

            msgSegments.Add( msgSeg );

            if ( msgSegments.Count == this.numPkts )
                IsCompleted = true;
        }

        public byte[] GetData()
        {
            uint size = GetSize();
            byte[] data = new byte[ size ];

            msgSegments.Sort( ( left, right ) =>
            {
                if ( left.DataSequence < right.DataSequence )
                    return -1;

                if ( left.DataSequence == right.DataSequence )
                    return 0;

                return 1;
            } );

            int offset = 0;
            msgSegments.ForEach( ( seg ) =>
            {
                Array.Copy( seg.Data, 0, data, offset, seg.Data.Length );
                offset += seg.Data.Length;
            } );

            return data;
        }

        public uint GetSize()
        {
            uint size = 0;
            msgSegments.ForEach( ( seg ) => { size += (uint)seg.Data.Length; } );

            return size;
        }


        bool HasData( UdpHeader udpHdr )
        {
            foreach ( MsgSegment msgSeg in msgSegments )
            {
                if ( msgSeg.DataSequence == udpHdr.SequenceThis )
                    return true;
            }
            return false;
        }
    }
}
