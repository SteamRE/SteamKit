using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Steam3Lib
{
    public class ByteBuffer
    {
        MemoryStream buffer;

        public ByteBuffer()
        {
            buffer = new MemoryStream();
        }
        public ByteBuffer( int capacity )
        {
            buffer = new MemoryStream( capacity );
        }
        public ByteBuffer( IEnumerable<byte> collection )
        {
            buffer = new MemoryStream( ( byte[] )collection );
        }


        public void Append( Type dataType, object data )
        {
            int dataLen = Marshal.SizeOf( dataType );
            IntPtr dataBlock = Marshal.AllocHGlobal( dataLen );

            Marshal.StructureToPtr( data, dataBlock, true );

            byte[] byteData = new byte[ dataLen ];

            Marshal.Copy( dataBlock, byteData, 0, dataLen );

            Marshal.FreeHGlobal( dataBlock );

            Append( byteData );
        }
        public void Append<T>( T data ) where T : struct
        {
            Append( typeof( T ), data );
        }


        public void Append( SByte data )
        {
            Append<SByte>( data );
        }
        public void Append( Byte data )
        {
            Append<Byte>( data );
        }

        public void Append( Byte[] data )
        {
            buffer.Write( data, 0, data.Length );
        }

        public void Append( Boolean data )
        {
            Append<Boolean>( data );
        }

        public void Append( Char data )
        {
            Append<Char>( data );
        }

        public void Append( Int16 data )
        {
            Append<Int16>( data );
        }
        public void Append( Int32 data )
        {
            Append<Int32>( data );
        }
        public void Append( Int64 data )
        {
            Append<Int64>( data );
        }

        public void Append( UInt16 data )
        {
            Append<UInt16>( data );
        }
        public void Append( UInt32 data )
        {
            Append<UInt32>( data );
        }
        public void Append( UInt64 data )
        {
            Append<UInt64>( data );
        }

        public void Append( Single data )
        {
            Append<Single>( data );
        }
        public void Append( Double data )
        {
            Append<Double>( data );
        }

        public void Append( string data )
        {
            Append( data, Encoding.Default );
        }
        public void Append( string data, Encoding encoding )
        {
            if ( data == null )
                return;

            byte[] strBuffer = encoding.GetBytes( data );
            buffer.Write( strBuffer, 0, strBuffer.Length );
        }

        public void AppendNullTermString( string data )
        {
            AppendNullTermString( data, Encoding.Default );
        }
        public void AppendNullTermString( string data, Encoding encoding )
        {
            Append( data, encoding );
            Append<Byte>( 0 );
        }


        public byte[] ToArray()
        {
            return buffer.ToArray();
        }
    }
}
