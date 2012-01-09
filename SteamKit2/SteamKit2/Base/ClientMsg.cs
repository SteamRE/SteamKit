using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;

namespace SteamKit2
{
    /// <summary>
    /// Represents a protobuf backed client message.
    /// </summary>
    /// <typeparam name="BodyType">The body type of this message.</typeparam>
    public sealed class ClientMsgProtobuf<BodyType> : MsgBase<MsgHdrProtoBuf>
        where BodyType : IExtensible, new()
    {
        /// <summary>
        /// Gets a value indicating whether this client message is protobuf backed.
        /// Client messages of this type are always protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public override bool IsProto { get { return true; } }
        /// <summary>
        /// Gets the network message type of this client message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public override EMsg MsgType { get { return Header.Msg; } }

        /// <summary>
        /// Gets or sets the session id for this client message.
        /// </summary>
        /// <value>
        /// The session id.
        /// </value>
        public override int SessionID
        {
            get { return ProtoHeader.client_session_id; }
            set { ProtoHeader.client_session_id = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="SteamID"/> for this client message.
        /// </summary>
        /// <value>
        /// The <see cref="SteamID"/>.
        /// </value>
        public override SteamID SteamID
        {
            get { return ProtoHeader.client_steam_id; }
            set { ProtoHeader.client_steam_id = value; }
        }

        /// <summary>
        /// Gets or sets the target job id for this client message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public override ulong TargetJobID
        {
            get { return ProtoHeader.job_id_target; }
            set { ProtoHeader.job_id_target = value; }
        }
        /// <summary>
        /// Gets or sets the source job id for this client message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public override ulong SourceJobID
        {
            get { return ProtoHeader.job_id_source; }
            set { ProtoHeader.job_id_source = value; }
        }


        /// <summary>
        /// Shorthand accessor for the protobuf header.
        /// </summary>
        public CMsgProtoBufHeader ProtoHeader { get { return Header.Proto; } }

        /// <summary>
        /// Gets the body structure of this message.
        /// </summary>
        public BodyType Body { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgProtobuf&lt;BodyType&gt;"/> class.
        /// This is a client send constructor.
        /// </summary>
        /// <param name="eMsg">The network message type this client message represents.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientMsgProtobuf( EMsg eMsg, int payloadReserve = 64 )
            : base( payloadReserve )
        {
            Body = new BodyType();

            // set our emsg
            Header.Msg = eMsg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgProtobuf&lt;BodyType&gt;"/> class.
        /// This a reply constructor.
        /// </summary>
        /// <param name="eMsg">The network message type this client message represents.</param>
        /// <param name="msg">The message that this instance is a reply for.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientMsgProtobuf( EMsg eMsg, MsgBase<MsgHdrProtoBuf> msg, int payloadReserve = 64 )
            : this( eMsg, payloadReserve )
        {
            // our target is where the message came from
            Header.Proto.job_id_target = msg.Header.Proto.job_id_source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgProtobuf&lt;BodyType&gt;"/> class.
        /// This is a recieve constructor.
        /// </summary>
        /// <param name="msg">The packet message to build this client message from.</param>
        public ClientMsgProtobuf( IPacketMsg msg )
            : this( msg.MsgType )
        {
            Deserialize( msg.GetData() );
        }

        /// <summary>
        /// Serializes this client message instance to a byte array.
        /// </summary>
        /// <returns>
        /// Data representing a client message.
        /// </returns>
        public override byte[] Serialize()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );
                Serializer.Serialize( ms, Body );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }
        /// <summary>
        /// Initializes this client message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a client message.</param>
        public override void Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body = Serializer.Deserialize<BodyType>( ms );

                // the rest of the data is the payload
                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Position - ms.Length );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }
    }

    /// <summary>
    /// Represents a struct backed client message.
    /// </summary>
    /// <typeparam name="BodyType">The body type of this message.</typeparam>
    public sealed class ClientMsg<BodyType> : MsgBase<ExtendedClientMsgHdr>
        where BodyType : ISteamSerializableMessage, new()
    {
        /// <summary>
        /// Gets a value indicating whether this client message is protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public override bool IsProto { get { return false; } }
        /// <summary>
        /// Gets the network message type of this client message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public override EMsg MsgType { get { return Header.Msg; } }

        /// <summary>
        /// Gets or sets the session id for this client message.
        /// </summary>
        /// <value>
        /// The session id.
        /// </value>
        public override int SessionID
        {
            get { return Header.SessionID; }
            set { Header.SessionID = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="SteamID"/> for this client message.
        /// </summary>
        /// <value>
        /// The <see cref="SteamID"/>.
        /// </value>
        public override SteamID SteamID
        {
            get { return Header.SteamID; }
            set { Header.SteamID = value; }
        }

        /// <summary>
        /// Gets or sets the target job id for this client message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public override ulong TargetJobID
        {
            get { return Header.TargetJobID; }
            set { Header.TargetJobID = value; }
        }
        /// <summary>
        /// Gets or sets the source job id for this client message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public override ulong SourceJobID
        {
            get { return Header.SourceJobID; }
            set { Header.SourceJobID = value; }
        }


        /// <summary>
        /// Gets the body structure of this message.
        /// </summary>
        public BodyType Body { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsg&lt;BodyType&gt;"/> class.
        /// This is a client send constructor.
        /// </summary>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientMsg( int payloadReserve = 64 )
            : base( payloadReserve )
        {
            Body = new BodyType();

            // assign our emsg
            Header.SetEMsg( Body.GetEMsg() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsg&lt;BodyType&gt;"/> class.
        /// This a reply constructor.
        /// </summary>
        /// <param name="msg">The message that this instance is a reply for.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientMsg( MsgBase<ExtendedClientMsgHdr> msg, int payloadReserve = 64 )
            : this( payloadReserve )
        {
            // our target is where the message came from
            Header.TargetJobID = msg.Header.SourceJobID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsg&lt;BodyType&gt;"/> class.
        /// This is a recieve constructor.
        /// </summary>
        /// <param name="msg">The packet message to build this client message from.</param>
        public ClientMsg( IPacketMsg msg )
            : this()
        {
            Deserialize( msg.GetData() );
        }

        /// <summary>
        /// Serializes this client message instance to a byte array.
        /// </summary>
        /// <returns>
        /// Data representing a client message.
        /// </returns>
        public override byte[] Serialize()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );
                Body.Serialize( ms );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }
        /// <summary>
        /// Initializes this client message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a client message.</param>
        public override void Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body.Deserialize( ms );

                // the rest of the data is the payload
                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Position - ms.Length );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }
    }

    /// <summary>
    /// Represents a struct backed message without session or client info.
    /// </summary>
    /// <typeparam name="BodyType">The body type of this message.</typeparam>
    public sealed class Msg<BodyType> : MsgBase<MsgHdr>
        where BodyType : ISteamSerializableMessage, new()
    {
        /// <summary>
        /// Gets a value indicating whether this client message is protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public override bool IsProto { get { return false; } }
        /// <summary>
        /// Gets the network message type of this client message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public override EMsg MsgType { get { return Header.Msg; } }

        /// <summary>
        /// Gets or sets the session id for this client message.
        /// This type of client message does not support session ids
        /// </summary>
        /// <value>
        /// The session id.
        /// </value>
        public override int SessionID { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="SteamID"/> for this client message.
        /// This type of client message goes not support <see cref="SteamID">SteamIDs</see>.
        /// </summary>
        /// <value>
        /// The <see cref="SteamID"/>.
        /// </value>
        public override SteamID SteamID { get; set; }

        /// <summary>
        /// Gets or sets the target job id for this client message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public override ulong TargetJobID
        {
            get { return Header.TargetJobID; }
            set { Header.TargetJobID = value; }
        }
        /// <summary>
        /// Gets or sets the source job id for this client message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public override ulong SourceJobID
        {
            get { return Header.SourceJobID; }
            set { Header.SourceJobID = value; }
        }


        /// <summary>
        /// Gets the structure body of the message.
        /// </summary>
        public BodyType Body { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Msg&lt;BodyType&gt;"/> class.
        /// This is a client send constructor.
        /// </summary>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public Msg( int payloadReserve = 0 )
            : base( payloadReserve )
        {
            Body = new BodyType();

            // assign our emsg
            Header.SetEMsg( Body.GetEMsg() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Msg&lt;BodyType&gt;"/> class.
        /// This a reply constructor.
        /// </summary>
        /// <param name="msg">The message that this instance is a reply for.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public Msg( MsgBase<MsgHdr> msg, int payloadReserve = 0 )
            : this( payloadReserve )
        {
            // our target is where the message came from
            Header.TargetJobID = msg.Header.SourceJobID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Msg&lt;BodyType&gt;"/> class.
        /// This is a recieve constructor.
        /// </summary>
        /// <param name="msg">The packet message to build this client message from.</param>
        public Msg( IPacketMsg msg )
            : this()
        {
            Deserialize( msg.GetData() );
        }


        /// <summary>
        /// Serializes this client message instance to a byte array.
        /// </summary>
        /// <returns>
        /// Data representing a client message.
        /// </returns>
        public override byte[] Serialize()
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                Header.Serialize( ms );
                Body.Serialize( ms );
                Payload.WriteTo( ms );

                return ms.ToArray();
            }
        }
        /// <summary>
        /// Initializes this client message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a client message.</param>
        public override void Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body.Deserialize( ms );

                // the rest of the data is the payload
                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Position - ms.Length );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }

    }
}
