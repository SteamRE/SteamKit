using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamLib
{
    /// <summary>
    /// Helper data manip class that allows reading binary data from a byte array.
    /// </summary>
    public class DataStream : Stream
    {

        /// <summary>
        /// Gets or sets the data represented by this instance.
        /// </summary>
        /// <value>The data.</value>
        public byte[] Data { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataStream"/> class.
        /// </summary>
        public DataStream()
        {
            Data = new byte[ 0 ];
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataStream"/> class.
        /// </summary>
        /// <param name="data">The data to wrap.</param>
        public DataStream( byte[] data )
        {
            this.Data = data;
        }


        /// <summary>
        /// Reads the specified object type, and seeks ahead the size of the object.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>An instance of the object.</returns>
        public object Read( Type objectType )
        {
            int dataLen = Marshal.SizeOf( objectType );
            IntPtr dataBlock = Marshal.AllocHGlobal( dataLen );

            Marshal.Copy( Data, ( int )Position, dataBlock, dataLen );

            object type = Marshal.PtrToStructure( dataBlock, objectType );

            Marshal.FreeHGlobal( dataBlock );

            Position += dataLen;

            return type;
        }
        /// <summary>
        /// Reads this instance, and seeks forward the length of the object.
        /// </summary>
        /// <typeparam name="T">The object type to read.</typeparam>
        /// <returns>An instance of the object.</returns>
        public T Read<T>() where T : struct
        {
            return ( T )Read( typeof( T ) );
        }

        /// <summary>
        /// Reads a signed byte from the stream, and seeks ahead 1 byte.
        /// </summary>
        /// <returns>the </returns>
        public SByte ReadSByte()
        {
            return Read<SByte>();
        }
        /// <summary>
        /// Reads a unsigned byte from the stream, and seeks ahead 1 byte.
        /// </summary>
        /// <returns></returns>
        public new Byte ReadByte()
        {
            return Read<Byte>();
        }

        /// <summary>
        /// Reads an unsigned byte from the stream, and seeks ahead 1 byte.
        /// </summary>
        /// <returns></returns>
        public Byte ReadUByte()
        {
            return Read<Byte>();
        }
        /// <summary>
        /// Reads an unsigned byte from the stream, but does not seek.
        /// </summary>
        /// <returns></returns>
        public Byte PeekByte()
        {
            Byte byteData = ReadByte();
            Position -= 1;

            return byteData;
        }

        /// <summary>
        /// Reads a number of bytes from the stream.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns></returns>
        public byte[] ReadBytes( int count )
        {
            byte[] returnBuff = new byte[ count ];
            Array.Copy( Data, Position, returnBuff, 0, count );

            Position += count;
            return returnBuff;
        }
        public byte[] ReadBytes( uint count )
        {
            return this.ReadBytes( ( int )count );
        }

        /// <summary>
        /// Reads a boolean value from the stream.
        /// </summary>
        /// <returns></returns>
        public Boolean ReadBool()
        {
            return Read<Boolean>();
        }

        /// <summary>
        /// Reads a character from the stream.
        /// </summary>
        /// <returns></returns>
        public Char ReadChar()
        {
            return Read<Char>();
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the stream.
        /// </summary>
        /// <returns></returns>
        public Int16 ReadInt16()
        {
            return Read<Int16>();
        }
        /// <summary>
        /// Reads a 32-bit signed integer from the stream.
        /// </summary>
        /// <returns></returns>
        public Int32 ReadInt32()
        {
            return Read<Int32>();
        }
        /// <summary>
        /// Reads a 64-bit signed integer from the stream.
        /// </summary>
        /// <returns></returns>
        public Int64 ReadInt64()
        {
            return Read<Int64>();
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the stream.
        /// </summary>
        /// <returns></returns>
        public UInt16 ReadUInt16()
        {
            return Read<UInt16>();
        }
        /// <summary>
        /// Reads a 32-bit unsigned integer from the stream.
        /// </summary>
        /// <returns></returns>
        public UInt32 ReadUInt32()
        {
            return Read<UInt32>();
        }
        /// <summary>
        /// Reads a 64-bit unsigned integer from the stream.
        /// </summary>
        /// <returns></returns>
        public UInt64 ReadUInt64()
        {
            return Read<UInt64>();
        }

        /// <summary>
        /// Reads a 32-bit floating point number.
        /// </summary>
        /// <returns></returns>
        public Single ReadSingle()
        {
            return Read<Single>();
        }
        /// <summary>
        /// Reads a 32-bit floating point number.
        /// </summary>
        /// <returns></returns>
        public Single ReadFloat()
        {
            return ReadSingle();
        }

        /// <summary>
        /// Reads a 64-bit floating point number.
        /// </summary>
        /// <returns></returns>
        public Double ReadDouble()
        {
            return Read<Double>();
        }


        /// <summary>
        /// Sizes the remaining size.
        /// </summary>
        /// <returns></returns>
        public long SizeRemaining()
        {
            return Length - Position;
        }


        /// <summary>
        /// Reads a null terminated string from the stream.
        /// </summary>
        /// <returns></returns>
        public string ReadNullTermString()
        {
            return ReadNullTermString( Encoding.Default );
        }
        /// <summary>
        /// Reads a null terminated string from the stream with a specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns></returns>
        public string ReadNullTermString( Encoding encoding )
        {
            long start = Position;
            int length = 0;

            for ( long x = start ; ( Data[ x ] != 0 ) ; ++length )
                x++;


            byte[] stringBlob = new byte[ length ];

            Array.Copy( Data, start, stringBlob, 0, length );

            string returnString = encoding.GetString( stringBlob );

            length++;
            Position += length;

            return returnString;
        }

        /// <summary>
        /// Reads a string of a certain length from the stream.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns></returns>
        public string ReadString( int length )
        {
            return ReadString( length, Encoding.Default );
        }
        /// <summary>
        /// Reads a string of a certain length from the steam with a specified encoding.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns></returns>
        public string ReadString( int length, Encoding encoding )
        {
            byte[] stringBlob = new byte[ length ];

            Array.Copy( Data, Position, stringBlob, 0, length );

            string returnString = encoding.GetString( stringBlob );

            Position += length;

            return returnString;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return true; }
        }
        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return true; }
        }
        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            // nop
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get { return Data.Length; }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get;
            set;
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read( byte[] buffer, int offset, int count )
        {
            Array.Copy( Data, Position, buffer, offset, count );
            return count;
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek( long offset, SeekOrigin origin )
        {
            switch ( origin )
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return Position;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength( long value )
        {
            throw new NotSupportedException( "Setting stream length is not supported, set the data instead." );
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException( "Writing is not supported." );
        }
    }
}