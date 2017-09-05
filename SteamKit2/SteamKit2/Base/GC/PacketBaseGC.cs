/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.IO;
using SteamKit2.Internal;

namespace SteamKit2.GC
{
    /// <summary>
    /// Represents a simple unified interface into game coordinator messages recieved from the network.
    /// This is contrasted with <see cref="IClientGCMsg"/> in that this interface is packet body agnostic
    /// and only allows simple access into the header. This interface is also immutable, and the underlying
    /// data cannot be modified.
    /// </summary>
    public interface IPacketGCMsg
    {
        /// <summary>
        /// Gets a value indicating whether this packet message is protobuf backed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        bool IsProto { get; }
        /// <summary>
        /// Gets the network message type of this packet message.
        /// </summary>
        /// <value>
        /// The message type.
        /// </value>
        uint MsgType { get; }

        /// <summary>
        /// Gets the target job id for this packet message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        JobID TargetJobID { get; }
        /// <summary>
        /// Gets the source job id for this packet message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        JobID SourceJobID { get; }

        /// <summary>
        /// Gets the underlying data that represents this client message.
        /// </summary>
        /// <returns>The data.</returns>
        byte[] GetData();
    }

    static class IPacketGCMsgExtensions
    {
        public static uint GetMsgTypeWithNullCheck( this IPacketGCMsg msg, string name )
        {
            if ( msg == null )
            {
                throw new ArgumentNullException( name );
            }

            return msg.MsgType;
        }
    }


    /// <summary>
    /// Represents a protobuf backed packet message.
    /// </summary>
    public sealed class PacketClientGCMsgProtobuf : IPacketGCMsg
    {
        /// <summary>
        /// Gets a value indicating whether this packet message is protobuf backed.
        /// This type of message is always protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public bool IsProto => true;
        /// <summary>
        /// Gets the network message type of this packet message.
        /// </summary>
        /// <value>
        /// The message type.
        /// </value>
        public uint MsgType { get; }

        /// <summary>
        /// Gets the target job id for this packet message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public JobID TargetJobID { get; }
        /// <summary>
        /// Gets the source job id for this packet message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public JobID SourceJobID { get; }


        readonly byte[] payload;


        /// <summary>
        /// Initializes a new instance of the <see cref="PacketClientGCMsgProtobuf"/> class.
        /// </summary>
        /// <param name="eMsg">The network message type for this packet message.</param>
        /// <param name="data">The data.</param>
        public PacketClientGCMsgProtobuf( uint eMsg, byte[] data )
        {
            if ( data == null )
            {
                throw new ArgumentNullException( nameof(data) );
            }

            MsgType = eMsg;
            payload = data;

            MsgGCHdrProtoBuf protobufHeader = new MsgGCHdrProtoBuf();

            // we need to pull out the job ids, so we deserialize the protobuf header
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                protobufHeader.Deserialize( ms );
            }

            TargetJobID = protobufHeader.Proto.job_id_target;
            SourceJobID = protobufHeader.Proto.job_id_source;
        }


        /// <summary>
        /// Gets the underlying data that represents this client message.
        /// </summary>
        /// <returns>The data.</returns>
        public byte[] GetData()
        {
            return payload;
        }
    }

    /// <summary>
    /// Represents a packet message with extended header information.
    /// </summary>
    public sealed class PacketClientGCMsg : IPacketGCMsg
    {
        /// <summary>
        /// Gets a value indicating whether this packet message is protobuf backed.
        /// This type of message is never protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public bool IsProto => false;
        /// <summary>
        /// Gets the network message type of this packet message.
        /// </summary>
        /// <value>
        /// The message type.
        /// </value>
        public uint MsgType { get; }

        /// <summary>
        /// Gets the target job id for this packet message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public JobID TargetJobID { get; }
        /// <summary>
        /// Gets the source job id for this packet message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public JobID SourceJobID { get; }

        byte[] payload;


        /// <summary>
        /// Initializes a new instance of the <see cref="PacketClientGCMsg"/> class.
        /// </summary>
        /// <param name="eMsg">The network message type for this packet message.</param>
        /// <param name="data">The data.</param>
        public PacketClientGCMsg( uint eMsg, byte[] data )
        {
            if ( data == null )
            {
                throw new ArgumentNullException( nameof(data) );
            }

            MsgType = eMsg;
            payload = data;

            MsgGCHdr gcHdr = new MsgGCHdr();

            // deserialize the gc header to get our hands on the job ids
            using ( MemoryStream ms = new MemoryStream( data ) )
            {
                gcHdr.Deserialize( ms );
            }

            TargetJobID = gcHdr.TargetJobID;
            SourceJobID = gcHdr.SourceJobID;
        }


        /// <summary>
        /// Gets the underlying data that represents this packet message.
        /// </summary>
        /// <returns>The data.</returns>
        public byte[] GetData()
        {
            return payload;
        }
    }
}
