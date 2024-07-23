using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace SteamKit2
{
    internal static class StreamHelpers
    {
        public static short ReadInt16(this Stream stream)
        {
            Span<byte> data = stackalloc byte[sizeof(Int16)];

            stream.Read( data );
            return BitConverter.ToInt16( data );
        }

        public static ushort ReadUInt16(this Stream stream)
        {
            Span<byte> data = stackalloc byte[sizeof(UInt16)];

            stream.Read( data );
            return BitConverter.ToUInt16( data );
        }

        public static int ReadInt32(this Stream stream)
        {
            Span<byte> data = stackalloc byte[sizeof(Int32)];

            stream.Read( data );
            return BitConverter.ToInt32( data );
        }

        public static long ReadInt64(this Stream stream)
        {
            Span<byte> data = stackalloc byte[sizeof(Int64)];

            stream.Read( data );
            return BitConverter.ToInt64( data );
        }

        public static uint ReadUInt32(this Stream stream)
        {
            Span<byte> data = stackalloc byte[sizeof(UInt32)];

            stream.Read( data );
            return BitConverter.ToUInt32( data );
        }

        public static ulong ReadUInt64(this Stream stream)
        {
            Span<byte> data = stackalloc byte[sizeof(UInt64)];

            stream.Read( data );
            return BitConverter.ToUInt64( data );
        }

        public static float ReadFloat( this Stream stream )
        {
            Span<byte> data = stackalloc byte[sizeof(float)];

            stream.Read( data );
            return BitConverter.ToSingle( data );
        }

        public static string ReadNullTermString( this Stream stream, Encoding encoding )
        {
            if ( encoding == Encoding.UTF8 )
            {
                return ReadNullTermUtf8String( stream );
            }

            int characterSize = encoding.GetByteCount( "e" );

            using MemoryStream ms = new MemoryStream();

            while ( true )
            {
                byte[] data = new byte[ characterSize ];
                stream.Read( data, 0, characterSize );

                if ( encoding.GetString( data, 0, characterSize ) == "\0" )
                {
                    break;
                }

                ms.Write( data, 0, data.Length );
            }

            return encoding.GetString( ms.GetBuffer(), 0, ( int )ms.Length );
        }

        private static string ReadNullTermUtf8String( Stream stream )
        {
            var buffer = ArrayPool<byte>.Shared.Rent( 32 );

            try
            {
                var position = 0;

                do
                {
                    var b = stream.ReadByte();

                    if ( b <= 0 ) // null byte or stream ended
                    {
                        break;
                    }

                    if ( position >= buffer.Length )
                    {
                        var newBuffer = ArrayPool<byte>.Shared.Rent( buffer.Length * 2 );
                        Buffer.BlockCopy( buffer, 0, newBuffer, 0, buffer.Length );
                        ArrayPool<byte>.Shared.Return( buffer );
                        buffer = newBuffer;
                    }

                    buffer[ position++ ] = (byte)b;
                }
                while ( true );

                return Encoding.UTF8.GetString( buffer[ ..position ] );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( buffer );
            }
        }

        public static void WriteNullTermString( this Stream stream, string value, Encoding encoding )
        {
            var dataLength = encoding.GetByteCount( value );
            var data = new byte[ dataLength + 1 ];
            encoding.GetBytes( value, 0, value.Length, data, 0 );
            data[ dataLength ] = 0x00; // '\0'

            stream.Write( data, 0, data.Length );
        }

        public static int ReadAll( this Stream stream, Span<byte> buffer )
        {
            return stream.ReadAtLeast( buffer, minimumBytes: buffer.Length, throwOnEndOfStream: false );
        }
    }
}
