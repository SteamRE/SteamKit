/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    public interface IClientMsg
    {
        byte[] Serialize();
    }

    public class ClientMsg<MsgType, Hdr> : IClientMsg
        where Hdr : ISteamSerializableHeader, new()
        where MsgType : ISteamSerializableMessage, new()
    {

        public Hdr Header { get; private set; }
        public MsgType Msg { get; private set; }

        public ByteBuffer Payload { get; private set; }

        public ClientMsg()
        {
            Header = new Hdr();
            Msg = new MsgType();
            Payload = new ByteBuffer();

            Header.SetEMsg( Msg.GetEMsg() );
        }

        public ClientMsg( byte[] data )
            : this()
        {
            MemoryStream ms = new MemoryStream( data );

            Header.Deserialize( ms );
            Msg.Deserialize( ms );

            byte[] payload = new byte[ ms.Length - ms.Position ];
            ms.Read( payload, 0, payload.Length );

            Payload.Append( payload );
        }


        public EMsg GetEMsg()
        {
            return Msg.GetEMsg();
        }

        public byte[] Serialize()
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( Header.Serialize() );
            bb.Append( Msg.Serialize() );

            bb.Append( Payload.ToArray() );

            return bb.ToArray();
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

        public ClientMsgProtobuf( byte[] data )
            : base( data )
        {
        }
    }
}
