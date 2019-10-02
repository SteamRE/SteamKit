/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.IO;
using ProtoBuf;
using SteamKit2.Internal;
using CMsgProtoBufHeader = SteamKit2.GC.Internal.CMsgProtoBufHeader;
using MsgGCHdr = SteamKit2.Internal.MsgGCHdr;
using MsgGCHdrProtoBuf = SteamKit2.Internal.MsgGCHdrProtoBuf;

namespace SteamKit2.GC
{
    /// <summary>
    /// Represents a protobuf backed game coordinator message.
    /// </summary>
    /// <typeparam name="TBody">The body type of this message.</typeparam>
    public sealed class ClientGCMsgProtobuf<TBody> : GCMsgBase<MsgGCHdrProtoBuf>
        where TBody : IExtensible, new()
    {
        /// <summary>
        /// Gets a value indicating whether this gc message is protobuf backed.
        /// Client messages of this type are always protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public override bool IsProto => true;
        /// <summary>
        /// Gets the network message type of this gc message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public override uint MsgType => Header.Msg;

        /// <summary>
        /// Gets or sets the target job id for this gc message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public override JobID TargetJobID
        {
            get => ProtoHeader.job_id_target;
            set => ProtoHeader.job_id_target = value ?? throw new ArgumentNullException( nameof(value) );
        }
        /// <summary>
        /// Gets or sets the source job id for this gc message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public override JobID SourceJobID
        {
            get => ProtoHeader.job_id_source;
            set => ProtoHeader.job_id_source = value ?? throw new ArgumentNullException( nameof(value) );
        }


        /// <summary>
        /// Shorthand accessor for the protobuf header.
        /// </summary>
        public CMsgProtoBufHeader ProtoHeader => Header.Proto;

        /// <summary>
        /// Gets the body structure of this message.
        /// </summary>
        public TBody Body { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientGCMsgProtobuf&lt;BodyType&gt;"/> class.
        /// This is a client send constructor.
        /// </summary>
        /// <param name="eMsg">The network message type this gc message represents.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientGCMsgProtobuf( uint eMsg, int payloadReserve = 64 )
            : base( payloadReserve )
        {
            Body = new TBody();

            // set our emsg
            Header.Msg = eMsg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientGCMsgProtobuf&lt;BodyType&gt;"/> class.
        /// This a reply constructor.
        /// </summary>
        /// <param name="eMsg">The network message type this gc message represents.</param>
        /// <param name="msg">The message that this instance is a reply for.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientGCMsgProtobuf( uint eMsg, GCMsgBase<MsgGCHdrProtoBuf> msg, int payloadReserve = 64 )
            : this( eMsg, payloadReserve )
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( nameof(msg) );
            }

            // our target is where the message came from
            Header.Proto.job_id_target = msg.Header.Proto.job_id_source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientGCMsgProtobuf&lt;BodyType&gt;"/> class.
        /// This is a recieve constructor.
        /// </summary>
        /// <param name="msg">The packet message to build this gc message from.</param>
        public ClientGCMsgProtobuf( IPacketGCMsg msg )
            : this( msg.GetMsgTypeWithNullCheck( nameof(msg) ) )
        {
            DebugLog.Assert( msg.IsProto, "ClientGCMsgProtobuf", "ClientGCMsgProtobuf used for non-proto message!" );

            Deserialize( msg.GetData() );
        }

        /// <summary>
        /// Serializes this gc message instance to a byte array.
        /// </summary>
        /// <returns>
        /// Data representing a gc message.
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
        /// Initializes this gc message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a gc message.</param>
        public override void Deserialize( byte[] data )
        {
            if ( data == null )
            {
                throw new ArgumentNullException( nameof(data) );
            }

            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body = Serializer.Deserialize<TBody>( ms );

                // the rest of the data is the payload
                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Length - ms.Position );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }
    }

    /// <summary>
    /// Represents a struct backed game coordinator message.
    /// </summary>
    /// <typeparam name="TBody">The body type of this message.</typeparam>
    public sealed class ClientGCMsg<TBody> : GCMsgBase<MsgGCHdr>
        where TBody : IGCSerializableMessage, new()
    {
        /// <summary>
        /// Gets a value indicating whether this gc message is protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public override bool IsProto => false;

        readonly uint msgType;
        /// <summary>
        /// Gets the network message type of this gc message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public override uint MsgType => msgType;

        /// <summary>
        /// Gets or sets the target job id for this gc message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public override JobID TargetJobID
        {
            get => Header.TargetJobID;
            set => Header.TargetJobID = value ?? throw new ArgumentNullException( nameof(value) );
        }
        /// <summary>
        /// Gets or sets the source job id for this gc message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public override JobID SourceJobID
        {
            get => Header.SourceJobID;
            set => Header.SourceJobID = value ?? throw new ArgumentNullException( nameof(value) );
        }


        /// <summary>
        /// Gets the body structure of this message.
        /// </summary>
        public TBody Body { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientGCMsg&lt;BodyType&gt;"/> class.
        /// This is a client send constructor.
        /// </summary>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientGCMsg( int payloadReserve = 64 )
            : base( payloadReserve )
        {
            Body = new TBody();

            // assign our emsg
            msgType = Body.GetEMsg();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientGCMsg&lt;BodyType&gt;"/> class.
        /// This a reply constructor.
        /// </summary>
        /// <param name="msg">The message that this instance is a reply for.</param>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public ClientGCMsg( GCMsgBase<MsgGCHdr> msg, int payloadReserve = 64 )
            : this( payloadReserve )
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( nameof(msg) );
            }

            // our target is where the message came from
            Header.TargetJobID = msg.Header.SourceJobID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientGCMsg&lt;BodyType&gt;"/> class.
        /// This is a recieve constructor.
        /// </summary>
        /// <param name="msg">The packet message to build this gc message from.</param>
        public ClientGCMsg( IPacketGCMsg msg )
            : this()
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( nameof(msg) );
            }

            DebugLog.Assert( !msg.IsProto, "ClientGCMsg", "ClientGCMsg used for proto message!" );

            Deserialize( msg.GetData() );
        }

        /// <summary>
        /// Serializes this gc message instance to a byte array.
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
        /// Initializes this gc message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a client message.</param>
        public override void Deserialize( byte[] data )
        {
            if ( data == null )
            {
                throw new ArgumentNullException( nameof(data) );
            }

            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Body.Deserialize( ms );

                // the rest of the data is the payload
                int payloadOffset = ( int )ms.Position;
                int payloadLen = ( int )( ms.Length - ms.Position );

                Payload.Write( data, payloadOffset, payloadLen );
            }
        }
    }
}
