using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Steam3Lib
{
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgHdr : Serializable<MsgHdr>
    {
        public EMsg EMsg;

        public ulong TargetJobID;
        public ulong SourceJobID;


        public MsgHdr()
        {
            EMsg = EMsg.Invalid;

            TargetJobID = 0xFFFFFFFFFFFFFFFF;
            SourceJobID = 0xFFFFFFFFFFFFFFFF;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class ExtendedClientMsgHdr : Serializable<ExtendedClientMsgHdr>
    {
        public EMsg EMsg; // EMsg

        public byte HeaderSize;

        public ushort HeaderVersion;

        public ulong TargetJobID;
        public ulong SourceJobID;

        public byte HeaderCanary;

        public ulong SteamID; // todo: CSteamID

        public int SessionID;


        public ExtendedClientMsgHdr()
        {
            EMsg = EMsg.Invalid;

            HeaderSize = 36;

            HeaderVersion = 2;

            TargetJobID = 0xFFFFFFFFFFFFFFFF;
            SourceJobID = 0xFFFFFFFFFFFFFFFF;

            HeaderCanary = 239;

            SessionID = 0;
        }
    };


    public class ClientMsg<MsgHdr, Hdr>
        : Serializable<Hdr>
        where Hdr : Serializable<Hdr>, new()
        where MsgHdr : Serializable<MsgHdr>, new()
    {

        public Hdr Header { get; private set; }
        public MsgHdr MsgHeader { get; private set; }

        public byte[] Payload { get; private set; }


        public ClientMsg( EMsg eMsg )
        {
            Header = new Hdr();

            byte[] headerData = Header.Serialize();
            byte[] eMsgData = BitConverter.GetBytes( ( uint )eMsg );

            eMsgData.CopyTo( headerData, 0 );

            Header = Serializable<Hdr>.Deserialize( headerData );

            MsgHeader = new MsgHdr();
        }

        public ClientMsg( EMsg eMsg, byte[] payload )
            : this( eMsg )
        {
            Payload = payload;
        }

        public ClientMsg( byte[] data )
        {
            int headerSize = Marshal.SizeOf( typeof( Hdr ) );

            this.Header = Serializable<Hdr>.Deserialize( data );
            this.MsgHeader = Serializable<MsgHdr>.Deserialize( data, headerSize );

            headerSize += Marshal.SizeOf( typeof( MsgHdr ) );

            this.Payload = new byte[ data.Length - headerSize ];

            Array.Copy( data, headerSize, this.Payload, 0, Payload.Length );
        }


        public void SetPayload( byte[] data )
        {
            this.Payload = data;
        }


        public byte[] GetData()
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( this.Header.Serialize() );
            bb.Append( this.MsgHeader.Serialize() );

            bb.Append( this.Payload );

            return bb.ToArray();
        }


        public static Hdr GetHeader( byte[] data )
        {
            return new ClientMsg<MsgHdr, Hdr>( data ).Header;
        }
        public static MsgHdr GetMsgHeader( byte[] data )
        {
            return new ClientMsg<MsgHdr, Hdr>( data ).MsgHeader;
        }
        public static byte[] GetPayload( byte[] data )
        {
            return new ClientMsg<MsgHdr, Hdr>( data ).Payload;
        }
    }


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgChannelEncryptRequest : Serializable<MsgChannelEncryptRequest>
    {
        public uint ProtocolVersion;
        public int Universe; // todo: EUniverse

        public MsgChannelEncryptRequest()
        {
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgChannelEncryptResponse : Serializable<MsgChannelEncryptResponse>
    {
        public uint ProtocolVersion;
        public uint KeySize;

        public MsgChannelEncryptResponse()
        {
        }
    }
}
