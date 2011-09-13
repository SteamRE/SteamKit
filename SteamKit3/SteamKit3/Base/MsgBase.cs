/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;

namespace SteamKit3
{
    public interface IClientMsg
    {
        ulong TargetJobID { get; set; }
        ulong SourceJobID { get; set; }

        byte[] Serialize();
        void Deserialize( byte[] data );
    }

    public interface IPacketMsg
    {
        bool IsProto { get; }

        EMsg MsgType { get; }

        ulong TargetJobID { get; }
        ulong SourceJobID { get; }

        byte[] GetData();
    }


    public sealed class PacketClientMsgProtobuf : IPacketMsg
    {
        public bool IsProto { get { return true; } }

        public EMsg MsgType { get; private set; }

        public ulong TargetJobID { get; private set; }
        public ulong SourceJobID { get; private set; }

        byte[] payload;


        public PacketClientMsgProtobuf( EMsg eMsg, byte[] data )
        {
            MsgType = eMsg;
            payload = data;

            MsgHdrProtoBuf protobufHeader = new MsgHdrProtoBuf();

            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                protobufHeader.Deserialize( ms );
            }

            TargetJobID = protobufHeader.ProtoHeader.job_id_target;
            SourceJobID = protobufHeader.ProtoHeader.job_id_source;
        }


        public byte[] GetData()
        {
            return payload;
        }
    }

    public sealed class PacketClientMsg : IPacketMsg
    {
        public bool IsProto { get { return false; } }

        public EMsg MsgType { get; private set; }

        public ulong TargetJobID { get; private set; }
        public ulong SourceJobID { get; private set; }

        byte[] payload;


        public PacketClientMsg( EMsg eMsg, byte[] data )
        {
            MsgType = eMsg;
            payload = data;

            ExtendedClientMsgHdr extendedHdr = new ExtendedClientMsgHdr();

            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                extendedHdr.Deserialize( ms );
            }

            TargetJobID = extendedHdr.TargetJobID;
            SourceJobID = extendedHdr.SourceJobID;
        }


        public byte[] GetData()
        {
            return payload;
        }
    }

    public sealed class PacketMsg : IPacketMsg
    {
        public bool IsProto { get { return false; } }

        public EMsg MsgType { get; private set; }

        public ulong TargetJobID { get; private set; }
        public ulong SourceJobID { get; private set; }

        byte[] payload;


        public PacketMsg( EMsg eMsg, byte[] data )
        {
            MsgType = eMsg;
            payload = data;

            MsgHdr msgHdr = new MsgHdr();

            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                msgHdr.Deserialize( ms );
            }

            TargetJobID = msgHdr.TargetJobID;
            SourceJobID = msgHdr.SourceJobID;
        }


        public byte[] GetData()
        {
            return payload;
        }
    }


    public abstract class MsgBase<HdrType>
        where HdrType : ISteamSerializableHeader, new()
    {
        public const ulong GIDNil = ulong.MaxValue;

        public HdrType Header { get; private set; }

        public MemoryStream Payload { get; private set; }
        BinaryReader reader;
        BinaryWriter writer;

        public MsgBase( int payloadReserve = 0 )
        {
            Header = new HdrType();

            Payload = new MemoryStream( payloadReserve );
            reader = new BinaryReader( Payload );
            writer = new BinaryWriter( Payload );
        }

        public void Write( byte data )
        {
            writer.Write( data );
        }
        public void Write( sbyte data )
        {
            writer.Write( data );
        }
        public void Write( byte[] data )
        {
            writer.Write( data );
        }
        public void Write( short data )
        {
            writer.Write( data );
        }
        public void Write( ushort data )
        {
            writer.Write( data );
        }
        public void Write( int data )
        {
            writer.Write( data );
        }
        public void Write( uint data )
        {
            writer.Write( data );
        }
        public void Write( long data )
        {
            writer.Write( data );
        }
        public void Write( ulong data )
        {
            writer.Write( data );
        }

    }


    public sealed class ClientMsgProtobuf<MsgType> : MsgBase<MsgHdrProtoBuf>, IClientMsg
        where MsgType : IExtensible, new()
    {
        public ulong TargetJobID
        {
            get { return ProtoHeader.job_id_target; }
            set { ProtoHeader.job_id_target = value; }
        }
        public ulong SourceJobID
        {
            get { return ProtoHeader.job_id_source; }
            set { ProtoHeader.job_id_source = value; }
        }

        public CMsgProtoBufHeader ProtoHeader { get { return Header.ProtoHeader; } }

        public MsgType Body { get; private set; }


        // client send constructor
        public ClientMsgProtobuf( EMsg eMsg, int payloadReserve = 64 )
            : base( payloadReserve )
        {
            Body = new MsgType();

            Header.Msg = eMsg;
        }

        // reply constructor
        public ClientMsgProtobuf( EMsg eMsg, MsgBase<MsgHdrProtoBuf> msg, int payloadReserve = 64 )
            : this( eMsg, payloadReserve )
        {
            Header.ProtoHeader.client_steam_id = msg.Header.ProtoHeader.client_steam_id;
            Header.ProtoHeader.job_id_target = msg.Header.ProtoHeader.job_id_source;
        }

        // recieve constructor
        public ClientMsgProtobuf( IPacketMsg msg )
            : this( msg.MsgType )
        {
            Deserialize( msg.GetData() );
        }

        public byte[] Serialize()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );
                Serializer.Serialize( ms, Body );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }

        public void Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body = Serializer.Deserialize<MsgType>( ms );

                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Position - ms.Length );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }
    }

    public sealed class ClientMsg<MsgType> : MsgBase<ExtendedClientMsgHdr>, IClientMsg
        where MsgType : ISteamSerializableMessage, new()
    {
        public ulong TargetJobID
        {
            get { return Header.TargetJobID; }
            set { Header.TargetJobID = value; }
        }
        public ulong SourceJobID
        {
            get { return Header.SourceJobID; }
            set { Header.SourceJobID = value; }
        }

        public MsgType Body { get; private set; }


        // client send constructor
        public ClientMsg( int payloadReserve = 64 )
            : base( payloadReserve )
        {
            Body = new MsgType();

            Header.SetEMsg( Body.GetEMsg() );
        }

        // reply constructor
        public ClientMsg( MsgBase<ExtendedClientMsgHdr> msg, int payloadReserve = 64 )
            : this( payloadReserve )
        {
            Header.SteamID = msg.Header.SteamID;
            Header.TargetJobID = msg.Header.SourceJobID;
        }

        // recieve constructor
        public ClientMsg( byte[] data )
        {
            Deserialize( data );
        }

        public byte[] Serialize()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );
                Body.Serialize( ms );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }

        public void Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body.Deserialize( ms );

                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Position - ms.Length );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }
    }

    public sealed class Msg<MsgType> : MsgBase<MsgHdr>, IClientMsg
        where MsgType : ISteamSerializableMessage, new()
    {
        public ulong TargetJobID
        {
            get { return Header.TargetJobID; }
            set { Header.TargetJobID = value; }
        }
        public ulong SourceJobID
        {
            get { return Header.SourceJobID; }
            set { Header.SourceJobID = value; }
        }
        public MsgType Body { get; private set; }


        // client send constructor
        public Msg( int payloadReserve = 0 )
            : base( payloadReserve )
        {
            Body = new MsgType();

            Header.SetEMsg( Body.GetEMsg() );
        }

        // reply constructor
        public Msg( MsgBase<MsgHdr> msg, int payloadReserve = 0 )
            : this( payloadReserve )
        {
            Header.TargetJobID = msg.Header.SourceJobID;
        }

        // recieve constructor
        public Msg( IPacketMsg msg )
            : this()
        {
            Deserialize( msg.GetData() );
        }


        public byte[] Serialize()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );
                Body.Serialize( ms );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }

        public void Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body.Deserialize( ms );

                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Position - ms.Length );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }

    }
}
