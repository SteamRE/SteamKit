using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Steam3Lib
{
    public enum EUDPPktType : byte
    {
        ChallengeReq = 1,
        Challenge = 2,
        Connect = 3,
        Accept = 4,
        Disconnect = 5,
        Data = 6,
        Datagram = 7,
        Max = 8,
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class UDPPktHdr
    {
        public UInt32 m_nMagic;

        public UInt16 m_cbPkt;

        public EUDPPktType m_EUDPPktType;
        public byte m_nFlags;

        public UInt32 m_nSrcConnectionID;
        public UInt32 m_nDstConnectionID;

        public UInt32 m_nSeqThis;
        public UInt32 m_nSeqAcked;

        public UInt32 m_nPktsInMsg;
        public UInt32 m_nMsgStartSeq;

        public UInt32 m_cbMsgData;


        public byte[] Serialize()
        {
            int structSize = Marshal.SizeOf( this );
            IntPtr ptrMem = Marshal.AllocHGlobal( structSize );

            Marshal.StructureToPtr( this, ptrMem, true );

            byte[] structData = new byte[ structSize ];

            Marshal.Copy( ptrMem, structData, 0, structData.Length );

            Marshal.DestroyStructure( ptrMem, typeof( UDPPktHdr ) );
            Marshal.FreeHGlobal( ptrMem );

            return structData;
        }
        public static UDPPktHdr Deserialize( byte[] data )
        {
            int structSize = Marshal.SizeOf( typeof( UDPPktHdr ) );

            if ( data.Length < structSize )
                return null;

            IntPtr ptrMem = Marshal.AllocHGlobal( structSize );
            Marshal.Copy( data, 0, ptrMem, structSize );

            UDPPktHdr udpPkt = ( UDPPktHdr )Marshal.PtrToStructure( ptrMem, typeof( UDPPktHdr ) );

            Marshal.FreeHGlobal( ptrMem );

            return udpPkt;
        }
    }
}
