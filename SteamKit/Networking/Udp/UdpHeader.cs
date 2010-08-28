using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamLib
{
    enum EUdpPacketType : byte
    {
        Invalid = 0,
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
    class UdpHeader : Serializable<UdpHeader>
    {
        public const uint PACKET_MAGIC = 0x31305356; // "VS01"

        public uint Magic;

        public ushort PayloadSize;

        public EUdpPacketType PacketType;
        public byte Flags;

        public uint SourceConnID;
        public uint DestinationConnID;

        public uint SequenceThis;
        public uint SequenceAcked;

        public uint PacketsInMsg;
        public uint MsgStartSequence;

        public uint MsgSize;


        public UdpHeader()
        {
            Magic = PACKET_MAGIC;

            PayloadSize = 0;

            PacketType = EUdpPacketType.Invalid;
            Flags = 0;

            SourceConnID = 0x200;
            DestinationConnID = 0;

            SequenceThis = 0;
            SequenceAcked = 0;

            PacketsInMsg = 0;
            MsgStartSequence = 0;

            MsgSize = 0;
        }
    }
}
