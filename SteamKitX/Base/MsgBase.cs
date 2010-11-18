using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SteamKit
{
    interface IClientMsg
    {
        MemoryStream serialize();
    }

    public class ClientMsg<MsgType, Hdr> : IClientMsg
        where Hdr : ISteamSerializableHeader, new()
        where MsgType : ISteamSerializableMessage, new()
    {

        public Hdr Header { get; private set; }
        public MsgType Msg { get; private set; }

        public MemoryStream Payload { get; private set; }

        public ClientMsg()
        {
            Header = new Hdr();
            Msg = new MsgType();
            Payload = new MemoryStream();

            Header.SetEMsg(Msg.GetEMsg());
        }

        public ClientMsg(MemoryStream ms)
            : this()
        {
            Header.deserialize(ms);
            Msg.deserialize(ms);

            ms.CopyTo(Payload);
        }


        public EMsg GetEMsg()
        {
            return Msg.GetEMsg();
        }

        public MemoryStream serialize()
        {
            Payload.Seek(0, SeekOrigin.Begin);

            MemoryStream header = Header.serialize();
            MemoryStream msg = Msg.serialize();

            MemoryStream final = new MemoryStream((int)(header.Length + msg.Length + Payload.Length));
            header.CopyTo(final);
            msg.CopyTo(final);
            Payload.CopyTo(final);

            final.Seek(0, SeekOrigin.Begin);
            return final;
        }
    }

    public class ClientMsgProtobuf<MsgType> : ClientMsg<MsgType, MsgHdrProtoBuf>
        where MsgType : ISteamSerializableMessage, new()
    {
        public CMsgProtoBufHeader ProtoHeader
        {
            get
            {
                return Header.ProtoHeader;
            }
        }

        public ClientMsgProtobuf()
            : base()
        {
        }

        public ClientMsgProtobuf(MemoryStream ms)
            : base(ms)
        {
        }
    }
}
