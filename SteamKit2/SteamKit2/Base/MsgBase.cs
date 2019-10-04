/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.IO;
using System.Text;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// Represents a unified interface into client messages.
    /// </summary>
    public interface IClientMsg
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
        EMsg MsgType { get; }

        /// <summary>
        /// Gets or sets the session id for this client message.
        /// </summary>
        /// <value>
        /// The session id.
        /// </value>
        int SessionID { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="SteamID"/> for this client message.
        /// </summary>
        /// <value>
        /// The <see cref="SteamID"/>.
        /// </value>
        SteamID? SteamID { get; set; }

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
    /// This class provides a payload backing to client messages.
    /// </summary>
    public abstract class MsgBase
    {
        /// <summary>
        /// Returns a <see cref="System.IO.MemoryStream"/> which is the backing stream for client message payload data.
        /// </summary>
        public MemoryStream Payload { get; }


        readonly BinaryReader reader;
        readonly BinaryWriter writer;


        /// <summary>
        /// Initializes a new instance of the <see cref="MsgBase"/> class.
        /// </summary>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public MsgBase( int payloadReserve = 0 )
        {
            Payload = new MemoryStream( payloadReserve );
            reader = new BinaryReader( Payload );
            writer = new BinaryWriter( Payload );
        }


        /// <summary>
        /// Seeks within the payload to the specified offset.
        /// </summary>
        /// <param name="offset">The offset in the payload to seek to.</param>
        /// <param name="loc">The origin to seek from.</param>
        /// <returns>The new position within the stream, calculated by combining the initial reference point and the offset.</returns>
        public long Seek( long offset, SeekOrigin loc )
        {
            return Payload.Seek( offset, loc );
        }

        /// <summary>
        /// Writes a single unsigned byte to the message payload.
        /// </summary>
        /// <param name="data">The unsigned byte.</param>
        public void Write( byte data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single signed byte to the message payload.
        /// </summary>
        /// <param name="data">The signed byte.</param>
        public void Write( sbyte data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes the specified byte array to the message payload.
        /// </summary>
        /// <param name="data">The byte array.</param>
        public void Write( byte[] data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single 16bit short to the message payload.
        /// </summary>
        /// <param name="data">The short.</param>
        public void Write( short data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single unsigned 16bit short to the message payload.
        /// </summary>
        /// <param name="data">The unsigned short.</param>
        public void Write( ushort data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single 32bit integer to the message payload.
        /// </summary>
        /// <param name="data">The integer.</param>
        public void Write( int data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single unsigned 32bit integer to the message payload.
        /// </summary>
        /// <param name="data">The unsigned integer.</param>
        public void Write( uint data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single 64bit long to the message payload.
        /// </summary>
        /// <param name="data">The long.</param>
        public void Write( long data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single unsigned 64bit long to the message payload.
        /// </summary>
        /// <param name="data">The unsigned long.</param>
        public void Write( ulong data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single 32bit float to the message payload.
        /// </summary>
        /// <param name="data">The float.</param>
        public void Write( float data )
        {
            writer.Write( data );
        }
        /// <summary>
        /// Writes a single 64bit double to the message payload.
        /// </summary>
        /// <param name="data">The double.</param>
        public void Write( double data )
        {
            writer.Write( data );
        }

        /// <summary>
        /// Writes the specified string to the message payload using default encoding.
        /// This function does not write a terminating null character.
        /// </summary>
        /// <param name="data">The string to write.</param>
        public void Write( string data )
        {
            Write( data, Encoding.GetEncoding( 0 ) );
        }
        /// <summary>
        /// Writes the specified string to the message payload using the specified encoding.
        /// This function does not write a terminating null character.
        /// </summary>
        /// <param name="data">The string to write.</param>
        /// <param name="encoding">The encoding to use.</param>
        public void Write( string data, Encoding encoding )
        {
            if ( data == null )
            {
                return;
            }

            if ( encoding == null )
            {
                throw new ArgumentNullException( nameof(encoding) );
            }

            Write( encoding.GetBytes( data ) );
        }

        /// <summary>
        /// Writes the secified string and a null terminator to the message payload using default encoding.
        /// </summary>
        /// <param name="data">The string to write.</param>
        public void WriteNullTermString( string data )
        {
            WriteNullTermString( data, Encoding.GetEncoding( 0 ) );
        }
        /// <summary>
        /// Writes the specified string and a null terminator to the message payload using the specified encoding.
        /// </summary>
        /// <param name="data">The string to write.</param>
        /// <param name="encoding">The encoding to use.</param>
        public void WriteNullTermString( string data, Encoding encoding )
        {
            Write( data, encoding );
            Write( encoding.GetBytes( "\0" ) );
        }

        /// <summary>
        /// Reads a single signed byte from the message payload.
        /// </summary>
        /// <returns>The signed byte.</returns>
        public sbyte ReadInt8()
        {
            return reader.ReadSByte();
        }
        /// <summary>
        /// Reads a single signed byte from the message payload.
        /// </summary>
        /// <returns>The signed byte.</returns>
        public sbyte ReadSByte()
        {
            return reader.ReadSByte();
        }
        /// <summary>
        /// Reads a single unsigned byte from the message payload.
        /// </summary>
        /// <returns>The unsigned byte.</returns>
        public byte ReadUInt8()
        {
            return reader.ReadByte();
        }
        /// <summary>
        /// Reads a single unsigned byte from the message payload.
        /// </summary>
        /// <returns>The unsigned byte.</returns>
        public byte ReadByte()
        {
            return reader.ReadByte();
        }
        /// <summary>
        /// Reads a number of bytes from the message payload.
        /// </summary>
        /// <param name="numBytes">The number of bytes to read.</param>
        /// <returns>The data.</returns>
        public byte[] ReadBytes( int numBytes )
        {
            return reader.ReadBytes( numBytes );
        }
        /// <summary>
        /// Reads a single 16bit short from the message payload.
        /// </summary>
        /// <returns>The short.</returns>
        public short ReadInt16()
        {
            return reader.ReadInt16();
        }
        /// <summary>
        /// Reads a single 16bit short from the message payload.
        /// </summary>
        /// <returns>The short.</returns>
        public short ReadShort()
        {
            return reader.ReadInt16();
        }
        /// <summary>
        /// Reads a single unsigned 16bit short from the message payload.
        /// </summary>
        /// <returns>The unsigned short.</returns>
        public ushort ReadUInt16()
        {
            return reader.ReadUInt16();
        }
        /// <summary>
        /// Reads a single unsigned 16bit short from the message payload.
        /// </summary>
        /// <returns>The unsigned short.</returns>
        public ushort ReadUShort()
        {
            return reader.ReadUInt16();
        }
        /// <summary>
        /// Reads a single 32bit integer from the message payload.
        /// </summary>
        /// <returns>The integer.</returns>
        public int ReadInt32()
        {
            return reader.ReadInt32();
        }
        /// <summary>
        /// Reads a single 32bit integer from the message payload.
        /// </summary>
        /// <returns>The integer.</returns>
        public int ReadInt()
        {
            return reader.ReadInt32();
        }
        /// <summary>
        /// Reads a single unsigned 32bit integer from the message payload.
        /// </summary>
        /// <returns>The unsigned integer.</returns>
        public uint ReadUInt32()
        {
            return reader.ReadUInt32();
        }
        /// <summary>
        /// Reads a single unsigned 32bit integer from the message payload.
        /// </summary>
        /// <returns>The unsigned integer.</returns>
        public uint ReadUInt()
        {
            return reader.ReadUInt32();
        }
        /// <summary>
        /// Reads a single 64bit long from the message payload.
        /// </summary>
        /// <returns>The long.</returns>
        public long ReadInt64()
        {
            return reader.ReadInt64();
        }
        /// <summary>
        /// Reads a single 64bit long from the message payload.
        /// </summary>
        /// <returns>The long.</returns>
        public long ReadLong()
        {
            return reader.ReadInt64();
        }
        /// <summary>
        /// Reads a single unsigned 64bit long from the message payload.
        /// </summary>
        /// <returns>The unsigned long.</returns>
        public ulong ReadUInt64()
        {
            return reader.ReadUInt64();
        }
        /// <summary>
        /// Reads a single unsigned 64bit long from the message payload.
        /// </summary>
        /// <returns>The unsigned long.</returns>
        public ulong ReadULong()
        {
            return reader.ReadUInt64();
        }
        /// <summary>
        /// Reads a single 32bit float from the message payload.
        /// </summary>
        /// <returns>The float.</returns>
        public float ReadSingle()
        {
            return reader.ReadSingle();
        }
        /// <summary>
        /// Reads a single 32bit float from the message payload.
        /// </summary>
        /// <returns>The float.</returns>
        public float ReadFloat()
        {
            return reader.ReadSingle();
        }
        /// <summary>
        /// Reads a single 64bit double from the message payload.
        /// </summary>
        /// <returns>The double.</returns>
        public double ReadDouble()
        {
            return reader.ReadDouble();
        }

        /// <summary>
        /// Reads a null terminated string from the message payload with the default encoding.
        /// </summary>
        /// <returns>The string.</returns>
        public string ReadNullTermString()
        {
            return ReadNullTermString( Encoding.GetEncoding( 0 ) );
        }
        /// <summary>
        /// Reads a null terminated string from the message payload with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use.</param>
        /// /// <returns>The string.</returns>
        public string ReadNullTermString( Encoding encoding )
        {
            if ( encoding == null )
            {
                throw new ArgumentNullException( nameof(encoding) );
            }

            return Payload.ReadNullTermString( encoding );
        }

    }

    /// <summary>
    /// This is the abstract base class for all available client messages.
    /// It's used to maintain packet payloads and provide a header for all client messages.
    /// </summary>
    /// <typeparam name="THeader">The header type for this client message.</typeparam>
    public abstract class MsgBase<THeader> : MsgBase, IClientMsg
        where THeader : ISteamSerializableHeader, new()
    {
        /// <summary>
        /// Gets a value indicating whether this client message is protobuf backed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is protobuf backed; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsProto { get; }
        /// <summary>
        /// Gets the network message type of this client message.
        /// </summary>
        /// <value>
        /// The network message type.
        /// </value>
        public abstract EMsg MsgType { get; }

        /// <summary>
        /// Gets or sets the session id for this client message.
        /// </summary>
        /// <value>
        /// The session id.
        /// </value>
        public abstract int SessionID { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="SteamID"/> for this client message.
        /// </summary>
        /// <value>
        /// The <see cref="SteamID"/>.
        /// </value>
        public abstract SteamID? SteamID { get; set; }

        /// <summary>
        /// Gets or sets the target job id for this client message.
        /// </summary>
        /// <value>
        /// The target job id.
        /// </value>
        public abstract JobID TargetJobID { get; set; }
        /// <summary>
        /// Gets or sets the source job id for this client message.
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
        /// Initializes a new instance of the <see cref="MsgBase&lt;HdrType&gt;"/> class.
        /// </summary>
        /// <param name="payloadReserve">The number of bytes to initialize the payload capacity to.</param>
        public MsgBase( int payloadReserve = 0 )
            : base( payloadReserve )
        {
            Header = new THeader();
        }


        /// <summary>
        /// Serializes this client message instance to a byte array.
        /// </summary>
        /// <returns>
        /// Data representing a client message.
        /// </returns>
        public abstract byte[] Serialize();
        /// <summary>
        /// Initializes this client message by deserializing the specified data.
        /// </summary>
        /// <param name="data">The data representing a client message.</param>
        public abstract void Deserialize( byte[] data );

    }
}
