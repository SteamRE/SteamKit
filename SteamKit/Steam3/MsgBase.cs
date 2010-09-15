using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Serializer = ProtoBuf.Serializer;

namespace SteamLib
{
    interface IMsgHdr
    {
        // in order to interface cleanly with IClientMsg's
        void SetEMsg( EMsg eMsg );
        EMsg GetEMsg();
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgHdr : Serializable<MsgHdr>, IMsgHdr
    {
        public EMsg EMsg;

        public ulong TargetJobID;
        public ulong SourceJobID;


        public MsgHdr()
        {
            this.EMsg = EMsg.Invalid;

            this.TargetJobID = UInt64.MaxValue;
            this.SourceJobID = UInt64.MaxValue;
        }


        public void SetEMsg( EMsg eMsg )
        {
            this.EMsg = eMsg;
        }

        public EMsg GetEMsg()
        {
            return this.EMsg;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class ExtendedClientMsgHdr : Serializable<ExtendedClientMsgHdr>, IMsgHdr
    {
        public EMsg EMsg;

        public byte HeaderSize;

        public ushort HeaderVersion;

        public ulong TargetJobID;
        public ulong SourceJobID;

        public byte HeaderCanary;

        public ulong SteamID;

        public int SessionID;


        public ExtendedClientMsgHdr()
        {
            this.EMsg = EMsg.Invalid;

            this.HeaderSize = 36;

            this.HeaderVersion = 2;

            this.TargetJobID = UInt64.MaxValue;
            this.SourceJobID = UInt64.MaxValue;

            this.HeaderCanary = 239;

            this.SessionID = 0;
        }


        public void SetEMsg( EMsg eMsg )
        {
            this.EMsg = eMsg;
        }

        public EMsg GetEMsg()
        {
            return this.EMsg;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MsgHdrProtobuf : Serializable<MsgHdrProtobuf>, IMsgHdr
    {


        public EMsg EMsg;

        public int headerLength;

        public MsgHdrProtobuf()
        {
            SetEMsg(EMsg.Invalid);

            headerLength = 0;
        }


        public void SetEMsg(EMsg eMsg)
        {
            this.EMsg = MsgUtil.MakeMsg(eMsg, true);
        }

        public EMsg GetEMsg()
        {
            return MsgUtil.GetMsg(this.EMsg);
        }
    }

    class ClientMsg<MsgHdr, Hdr> : Serializable<Hdr>
        where Hdr : Serializable<Hdr>, IMsgHdr, new()
        where MsgHdr : Serializable<MsgHdr>, IClientMsg, new()
    {

        public Hdr Header;
        public MsgHdr MsgHeader;

        ByteBuffer byteBuff;


        public ClientMsg()
        {
            byteBuff = new ByteBuffer();

            Header = new Hdr();
            MsgHeader = new MsgHdr();

            Header.SetEMsg( MsgHeader.GetEMsg() );
        }

        public ClientMsg( byte[] data )
        {
            byteBuff = new ByteBuffer();

            int headerSize = Marshal.SizeOf( typeof( Hdr ) );

            this.Header = Serializable<Hdr>.Deserialize( data );
            this.MsgHeader = Serializable<MsgHdr>.Deserialize( data, headerSize );

            headerSize += Marshal.SizeOf( typeof( MsgHdr ) );

            byte[] payload = new byte[ data.Length - headerSize ];
            Array.Copy( data, headerSize, payload, 0, payload.Length );

            SetPayload( payload );
        }


        public EMsg GetEMsg()
        {
            return MsgHeader.GetEMsg();
        }


        public void SetPayload( byte[] data )
        {
            byteBuff.Clear();
            byteBuff.Append( data );
        }

        public byte[] GetPayload()
        {
            return byteBuff.ToArray();
        }


        public override byte[] Serialize()
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( this.Header.Serialize() );
            bb.Append( this.MsgHeader.Serialize() );

            bb.Append( byteBuff.ToArray() );

            return bb.ToArray();
        }


        public void Write<T>( T obj ) where T : struct
        {
            byteBuff.Append( obj );
        }
        public void Write( byte[] obj )
        {
            byteBuff.Append( obj );
        }
        public void WriteNullTermString( string data )
        {
            byteBuff.AppendNullTermString( data );
        }
        public void WriteNullTermString( string data, Encoding encoding )
        {
            byteBuff.AppendNullTermString( data, encoding );
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
            return new ClientMsg<MsgHdr, Hdr>( data ).GetPayload();
        }
    }

    class ClientMsgProtobuf<ProtoBuf> : Serializable<MsgHdrProtobuf>
        where ProtoBuf : global::ProtoBuf.IExtensible, new()
    {
        public MsgHdrProtobuf Header;
        public CMsgProtoBufHeader ProtoHeader;
        public ProtoBuf Proto;

        // ProtoBuf doesn't implement IClientMsg
        public ClientMsgProtobuf(EMsg msg)
        {
            Header = new MsgHdrProtobuf();
            ProtoHeader = null;
            Proto = new ProtoBuf();

            Header.SetEMsg(msg);
        }

        // construct that also makes a protoheader
        public ClientMsgProtobuf(EMsg msg, bool header) : this(msg)
        {
            if (header)
            {
                ActivateHeader();
            }
        }

        public ClientMsgProtobuf( byte[] data )
        {
            int headerSize = Marshal.SizeOf(typeof(MsgHdrProtobuf));

            this.Header = Serializable<MsgHdrProtobuf>.Deserialize( data );

            MemoryStream ms;

            if (this.Header.headerLength > 0)
            {
                ms = new MemoryStream(data, headerSize, this.Header.headerLength);
                this.ProtoHeader = Serializer.Deserialize<CMsgProtoBufHeader>(ms);

                headerSize += this.Header.headerLength;
            }

            ms = new MemoryStream(data, headerSize, data.Length - headerSize);
            this.Proto = Serializer.Deserialize<ProtoBuf>( ms );
        }

        public void ActivateHeader()
        {
            if (this.ProtoHeader == null)
            {
                this.ProtoHeader = new CMsgProtoBufHeader();
            }
        }

        public override byte[] Serialize()
        {
            ByteBuffer bb = new ByteBuffer();

            byte[] protoHdrBuf = null;

            if (ProtoHeader != null)
            {
                MemoryStream msProtoHeader = new MemoryStream();
                Serializer.Serialize<CMsgProtoBufHeader>(msProtoHeader, ProtoHeader);

                protoHdrBuf = msProtoHeader.ToArray();
                this.Header.headerLength = protoHdrBuf.Length;
            }

            bb.Append(this.Header.Serialize());

            if ( protoHdrBuf != null )
                bb.Append(protoHdrBuf);

            Serializer.Serialize<ProtoBuf>( bb.GetStreamForWrite(), this.Proto );

            return bb.ToArray();
        }
    }
}
