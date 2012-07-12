/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// Helper class that allows writing binary data to a stream.
    /// </summary>
    public class BinaryWriterEx : BinaryWriter
    {
        /// <summary>
        /// Gets or sets a value indicating whether to swap endianness when writing basic types.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to swap endianness when writing basic types; otherwise, <c>false</c>.
        /// </value>
        public bool SwapEndianness { get; set; }

        /// <summary>
        /// gets the length in bytes of the stream.
        /// </summary>
        public long Length { get { return BaseStream.Length; } }
        /// <summary>
        /// Gets the position within the current stream.
        /// </summary>
        public long Position { get { return BaseStream.Position; } }


        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryWriterEx"/> class.
        /// </summary>
        /// <param name="swapEndianness">if set to <c>true</c>, perform an endian swap when writing basic types.</param>
        public BinaryWriterEx( bool swapEndianness = false)
            : this( new MemoryStream(), swapEndianness )
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryWriterEx"/> class.
        /// </summary>
        /// <param name="capacity">The initial size of the internal array in bytes.</param>
        /// <param name="swapEndianness">if set to <c>true</c>, perform an endian swap when writing basic types.</param>
        public BinaryWriterEx( int capacity, bool swapEndianness = false )
            : this( new MemoryStream( capacity ), swapEndianness )
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryWriterEx"/> class.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="swapEndianness">if set to <c>true</c>, perform an endian swap when writing basic types.</param>
        public BinaryWriterEx( Stream stream, bool swapEndianness = false )
            : base( stream )
        {
            this.SwapEndianness = swapEndianness;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.BinaryWriterEx"/> to <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="bw">The <see cref="SteamKit2.BinaryWriterEx"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Stream( BinaryWriterEx bw )
        {
            return bw.BaseStream;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.BinaryWriter"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose( bool disposing )
        {
            //Stop BinaryWriter from closing the stream
            base.Dispose( false );
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            if ( OutStream != null )
                OutStream.Dispose();

            OutStream = new MemoryStream();
        }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.
        /// </summary>
        /// <param name="value">The two-byte signed integer to write.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        public override void Write( short value )
        {
            if ( SwapEndianness )
                value = IPAddress.HostToNetworkOrder( value );

            base.Write( value );
        }
        /// <summary>
        /// Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        public override void Write( int value )
        {
            if ( SwapEndianness )
                value = IPAddress.HostToNetworkOrder( value );

            base.Write( value );
        }
        /// <summary>
        /// Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte signed integer to write.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        public override void Write( long value )
        {
            if ( SwapEndianness )
                value = IPAddress.HostToNetworkOrder( value );

            base.Write( value );
        }
        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.
        /// </summary>
        /// <param name="value">The two-byte unsigned integer to write.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        public override void Write( ushort value )
        {
            Write( ( short )value );
        }
        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte unsigned integer to write.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        public override void Write( uint value )
        {
            Write( ( int )value );
        }
        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        public override void Write( ulong value )
        {
            Write( ( long )value );
        }


        /// <summary>
        /// Attempts to marshal the given type and data into a byte array, and write it to the stream.
        /// </summary>
        /// <param name="dataType">The <see cref="Type"/> of the data.</param>
        /// <param name="data">The object that will be marshaled into the stream.</param>
        public void Write( Type dataType, object data )
        {
            int dataLen = Marshal.SizeOf( dataType );
            IntPtr dataBlock = Marshal.AllocHGlobal( dataLen );

            Marshal.StructureToPtr( data, dataBlock, true );

            byte[] byteData = new byte[ dataLen ];

            Marshal.Copy( dataBlock, byteData, 0, dataLen );

            Marshal.FreeHGlobal( dataBlock );

            if ( SwapEndianness )
                Array.Reverse( byteData );

            Write( byteData );
        }

        /// <summary>
        /// Attempts to marshal the given type and data into a byte array, and write it to the stream.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the data.</typeparam>
        /// <param name="data">The object that will be marshaled into the stream.</param>
        public void WriteType<T>( T data )
            where T : struct
        {
            Write( typeof( T ), data );
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the <see cref="Position"/> property.
        /// </summary>
        /// <returns>A new byte array.</returns>
        public byte[] ToArray()
        {
            var ms = BaseStream as MemoryStream;

            if ( ms != null )
                return ms.ToArray();

            return null;
        }

        /// <summary>
        /// Writes the specified string to the stream using default encoding.
        /// This function does not write a terminating null character.
        /// </summary>
        /// <param name="data">The string to write.</param>
        public new void Write( string data )
        {
            Write( data, Encoding.Default );
        }
        /// <summary>
        /// Writes the specified string to the stream using the specified encoding.
        /// This function does not write a terminating null character.
        /// </summary>
        /// <param name="data">The string to write.</param>
        /// <param name="encoding">The encoding to use when writing the string.</param>
        public void Write( string data, Encoding encoding )
        {
            if ( data == null )
                return;

            Write( encoding.GetBytes( data ) );
        }

        /// <summary>
        /// Writes the secified string and a null terminator to the stream using default encoding.
        /// </summary>
        /// <param name="data">The string to write.</param>
        public void WriteNullTermString( string data )
        {
            WriteNullTermString( data, Encoding.Default );
        }
        /// <summary>
        /// Writes the specified string and a null terminator to the stream using the specified encoding.
        /// </summary>
        /// <param name="data">The string to write.</param>
        /// <param name="encoding">The encoding to use when writing the string.</param>
        public void WriteNullTermString( string data, Encoding encoding )
        {
            Write( data, encoding );
            Write( encoding.GetBytes( "\0" ) );
        }
    }
}