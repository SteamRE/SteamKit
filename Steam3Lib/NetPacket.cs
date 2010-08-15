using System;
using System.Collections.Generic;
using System.Text;

namespace Steam3Lib
{
    struct MsgSegment
    {
        public uint DataSequence;
        public byte[] Data;
    }

    public class NetPacket
    {
        uint seqStart;
        uint seqEnd;

        uint numPkts;

        List<MsgSegment> msgSegments;


        public bool IsCompleted { get; private set; }
        public bool IsEncrypted { get; private set; }


        public NetPacket( uint seqStart, uint numPkts, bool isEncrypted )
        {
            msgSegments = new List<MsgSegment>();

            this.seqStart = seqStart;
            this.seqEnd = seqStart + numPkts;

            this.numPkts = numPkts;

            this.IsEncrypted = isEncrypted;
            this.IsCompleted = false;
        }


        public void AddData( UDPPktHdr udpHdr, byte[] data )
        {
            if ( HasData( udpHdr ) )
                return;

            MsgSegment msgSeg = new MsgSegment();

            msgSeg.Data = data;
            msgSeg.DataSequence = udpHdr.m_nSeqThis;

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


        bool HasData( UDPPktHdr udpHdr )
        {
            foreach ( MsgSegment msgSeg in msgSegments )
            {
                if ( msgSeg.DataSequence == udpHdr.m_nSeqThis )
                    return true;
            }
            return false;
        }
    }
}
