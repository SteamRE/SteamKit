/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using SteamKit2.Internal;

namespace SteamKit2.GC
{
    /// <summary>
    /// Represents a unified interface into client messages.
    /// </summary>
    public interface IClientGCMsg
    {
        /// <summary>
        /// Gets a value indicating whether this client message is protobuf backed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        bool IsProto { get; }
        /// <summary>
        /// Gets the network message type of this client message.
        /// </summary>
        /// <value>
        /// The message type.
        /// </value>
        uint MsgType { get; }

        /// <summary>
        /// Gets or sets the target job id for this client message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        JobID TargetJobID { get; set; }
        /// <summary>
        /// Gets or sets the source job id for this client message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        JobID SourceJobID { get; set; }

        /// <summary>
        /// Serializes this client message instance to a byte array.
        /// </summary>
        /// <returns>Data representing a client message.</returns>
        byte[] Serialize();
        /// <summary>
        /// Initializes this client message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a client message.</param>
        void Deserialize( byte[] data );
    }

    /// <summary>
    /// This is the abstract base class for all available game coordinator messages.
    /// It's used to maintain packet payloads and provide a header for all gc messages.
    /// </summary>
    /// <typeparam name="THeader">The header type for this gc message.</typeparam>
    public abstract class GCMsgBase<THeader> : MsgBase, IClientGCMsg
        where THeader : IGCSerializableHeader, new()
    {
        /// <summary>
        /// Gets a value indicating whether this gc message is protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsProto { get; }
        /// <summary>
        /// Gets the network message type of this gc message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public abstract uint MsgType { get; }

        /// <summary>
        /// Gets or sets the target job id for this gc message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public abstract JobID TargetJobID { get; set; }
        /// <summary>
        /// Gets or sets the source job id for this gc message.
        /// </summary>
        /// <value>
        /// The source job id.
        /// </value>
        public abstract JobID SourceJobID { get; set; }


        /// <summary>
        /// Gets the header for this message type. 
        /// </summary>
        public THeader Header { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="GCMsgBase&lt;HdrType&gt;"/> class.
        /// </summary>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public GCMsgBase( int payloadReserve = 0 )
            : base( payloadReserve )
        {
            Header = new THeader();
        }


        /// <summary>
        /// Serializes this gc message instance to a byte array.
        /// </summary>
        /// <returns>
        /// Data representing a gc message.
        /// </returns>
        public abstract byte[] Serialize();
        /// <summary>
        /// Initializes this gc message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a gc message.</param>
        public abstract void Deserialize( byte[] data );

    }
}
