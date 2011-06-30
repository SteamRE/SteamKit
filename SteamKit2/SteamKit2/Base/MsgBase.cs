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
    /// <summary>
    /// Represents a client message that can be (de)serialized.
    /// </summary>
    public interface IClientMsg
    {
        void Serialize( Stream stream );
        void SetData( byte[] data );
    }

    /// <summary>
    /// Represents a game coordinator message.
    /// </summary>
    /// <typeparam name="MsgType">The message body type of the message.</typeparam>
    /// <typeparam name="Hdr">The message header type of the message.</typeparam>
    public class GCMsg<MsgType, Hdr> : IClientMsg
        where Hdr : IGCSerializableHeader, new()
        where MsgType : IGCSerializableMessage, new()
    {
        /// <summary>
        /// Gets the header.
        /// </summary>
        public Hdr Header { get; private set; }
        /// <summary>
        /// Gets the message body.
        /// </summary>
        public MsgType Msg { get; private set; }
        /// <summary>
        /// Gets the optional message payload.
        /// </summary>
        public BinaryWriterEx Payload { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="GCMsg&lt;MsgType, Hdr&gt;"/> class.
        /// </summary>
        public GCMsg()
        {
            Header = new Hdr();
            Msg = new MsgType();
            Payload = new BinaryWriterEx();

            Header.SetEMsg( Msg.GetEMsg() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GCMsg&lt;MsgType, Hdr&gt;"/> class.
        /// </summary>
        /// <param name="data">The data to construct the message from.</param>
        public GCMsg( byte[] data )
            : this()
        {
            this.SetData( data );
        }


        /// <summary>
        /// Deserializes the message from data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SetData( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Msg.Deserialize( ms );

                // the rest of the data must be the payload
                byte[] payload = new byte[ ms.Length - ms.Position ];
                ms.Read( payload, 0, payload.Length );

                Payload.Write( payload );
            }
        }


        /// <summary>
        /// Gets the message type of the message.
        /// </summary>
        /// <returns>The message type.</returns>
        public EGCMsg GetEMsg()
        {
            return Msg.GetEMsg();
        }

        /// <summary>
        /// Serializes the message to the specified stream.
        /// </summary>
        /// <param name="s">The stream.</param>
        public void Serialize( Stream s )
        {
            using ( BinaryWriterEx bb = new BinaryWriterEx( s ) )
            {
                Header.Serialize( bb );
                Msg.Serialize( bb );

                bb.Write( Payload.ToArray() );
            }
        }
    }

    /// <summary>
    /// Represents a basic steam client message.
    /// </summary>
    /// <typeparam name="MsgType">The message body type of the message.</typeparam>
    /// <typeparam name="Hdr">The message header type of the message.</typeparam>
    public class ClientMsg<MsgType, Hdr> : IClientMsg
        where Hdr : ISteamSerializableHeader, new()
        where MsgType : ISteamSerializableMessage, new()
    {

        /// <summary>
        /// Gets the header.
        /// </summary>
        public Hdr Header { get; private set; }
        /// <summary>
        /// Gets the message body.
        /// </summary>
        public MsgType Msg { get; private set; }
        /// <summary>
        /// Gets the optional message payload.
        /// </summary>
        public BinaryWriterEx Payload { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsg&lt;MsgType, Hdr&gt;"/> class.
        /// </summary>
        public ClientMsg()
        {
            Header = new Hdr();
            Msg = new MsgType();
            Payload = new BinaryWriterEx();

            Header.SetEMsg( Msg.GetEMsg() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsg&lt;MsgType, Hdr&gt;"/> class.
        /// </summary>
        /// <param name="data">The data to construct the message from.</param>
        public ClientMsg( byte[] data )
            : this()
        {
            this.SetData( data );
        }


        /// <summary>
        /// Deserializes the message from data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SetData( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                Header.Deserialize( ms );
                Msg.Deserialize( ms );

                // the rest of the data must be the payload
                byte[] payload = new byte[ ms.Length - ms.Position ];
                ms.Read( payload, 0, payload.Length );

                Payload.Write( payload );
            }
        }


        /// <summary>
        /// Gets the message type of the message.
        /// </summary>
        /// <returns>The message type.</returns>
        public EMsg GetEMsg()
        {
            return Msg.GetEMsg();
        }

        /// <summary>
        /// Serializes the mesage to the specified stream.
        /// </summary>
        /// <param name="s">The stream.</param>
        public void Serialize( Stream s )
        {
            using ( BinaryWriterEx bb = new BinaryWriterEx( s ) )
            {
                Header.Serialize( bb );
                Msg.Serialize( bb );

                bb.Write( Payload.ToArray() );
            }
        }
    }


    /// <summary>
    /// Represents a protobuf steam client message.
    /// </summary>
    /// <typeparam name="MsgType">The message body type of the message.</typeparam>
    public class ClientMsgProtobuf<MsgType> : ClientMsg<MsgType, MsgHdrProtoBuf>
        where MsgType : ISteamSerializableMessage, new()
    {

        /// <summary>
        /// Gets the protobuf header.
        /// </summary>
        public SteamKit2.CMsgProtoBufHeader ProtoHeader
        {
            get { return Header.ProtoHeader; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgProtobuf&lt;MsgType&gt;"/> class.
        /// </summary>
        public ClientMsgProtobuf()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgProtobuf&lt;MsgType&gt;"/> class.
        /// This is a reply constructor.
        /// </summary>
        /// <param name="origHdr">The message header of the original client message.</param>
        public ClientMsgProtobuf( MsgHdrProtoBuf origHdr )
            : this()
        {
            ProtoHeader.client_steam_id = origHdr.ProtoHeader.client_steam_id;
            ProtoHeader.job_id_target = origHdr.ProtoHeader.job_id_source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMsgProtobuf&lt;MsgType&gt;"/> class.
        /// This is a receive constructor.
        /// </summary>
        /// <param name="data">The data to construct the message from.</param>
        public ClientMsgProtobuf( byte[] data )
            : base( data )
        {
        }
    }

    /// <summary>
    /// Represents a protobuf game coordinator message.
    /// </summary>
    /// <typeparam name="MsgType">The message body type of the message.</typeparam>
    public class GCMsgProtobuf<MsgType> : GCMsg<MsgType, MsgGCHdrProtoBuf>
        where MsgType : IGCSerializableMessage, new()
    {
        /// <summary>
        /// Gets the protobuf header.
        /// </summary>
        public SteamKit2.GC.CMsgProtoBufHeader ProtoHeader
        {
            get { return Header.ProtoHeader; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GCMsgProtobuf&lt;MsgType&gt;"/> class.
        /// </summary>
        public GCMsgProtobuf()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GCMsgProtobuf&lt;MsgType&gt;"/> class.
        /// This a reply constructor.
        /// </summary>
        /// <param name="origHdr">The message header of the original game coordinator message.</param>
        public GCMsgProtobuf( MsgGCHdrProtoBuf origHdr )
            : this()
        {
            ProtoHeader.client_steam_id = origHdr.ProtoHeader.client_steam_id;
            ProtoHeader.job_id_target = origHdr.ProtoHeader.job_id_source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GCMsgProtobuf&lt;MsgType&gt;"/> class.
        /// This is a recieve constructor.
        /// </summary>
        /// <param name="data">The data to construct the message from.</param>
        public GCMsgProtobuf( byte[] data )
            : base( data )
        {
        }
    }
}
