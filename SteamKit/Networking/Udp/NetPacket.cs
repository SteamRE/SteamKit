using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamKit
{
    struct MsgSegment
    {
        public uint DataSequence;
        public MemoryStream Data;
    }

    class NetPacket
    {
        uint seqStart;
        uint seqEnd;

        uint numPkts;

        List<MsgSegment> msgSegments;
        int totalSize;


        public bool IsCompleted { get; private set; }


        public NetPacket( uint seqStart, uint numPkts )
        {
            msgSegments = new List<MsgSegment>();

            this.seqStart = seqStart;
            this.seqEnd = seqStart + numPkts;

            this.numPkts = numPkts;
            this.totalSize = 0;

            this.IsCompleted = false;
        }


        public void AddData( UdpHeader udpHdr, MemoryStream data )
        {
            if ( HasData( udpHdr ) )
                return;

            MsgSegment msgSeg = new MsgSegment();

            msgSeg.Data = data;
            msgSeg.DataSequence = udpHdr.SeqThis;

            msgSegments.Add( msgSeg );
            totalSize += (int)data.Length;

            if ( msgSegments.Count == this.numPkts )
                IsCompleted = true;
        }

        public MemoryStream GetData()
        {
            msgSegments.Sort( ( left, right ) =>
            {
                if ( left.DataSequence < right.DataSequence )
                    return -1;

                if ( left.DataSequence == right.DataSequence )
                    return 0;

                return 1;
            } );

            MemoryStream final = new MemoryStream( totalSize );
            msgSegments.ForEach( ( seg ) =>
            {
                seg.Data.CopyTo(final);
            } );

            final.Seek(0, SeekOrigin.Begin);
            return final;
        }

        bool HasData( UdpHeader udpHdr )
        {
            foreach ( MsgSegment msgSeg in msgSegments )
            {
                if ( msgSeg.DataSequence == udpHdr.SeqThis )
                    return true;
            }
            return false;
        }
    }
}
