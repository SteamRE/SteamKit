using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace SteamKit2
{
    internal static class StreamHelpers
    {
        public static short ReadInt16(this Stream stream)
        {
            Span<byte> data = stackalloc byte[2];

            stream.Read( data );
            return BitConverter.ToInt16( data );
        }

        public static ushort ReadUInt16(this Stream stream)
        {
            Span<byte> data = stackalloc byte[2];

            stream.Read( data );
            return BitConverter.ToUInt16( data );
        }

        public static int ReadInt32(this Stream stream)
        {
            Span<byte> data = stackalloc byte[4];

            stream.Read( data );
            return BitConverter.ToInt32( data );
        }

        public static long ReadInt64(this Stream stream)
        {
            Span<byte> data = stackalloc byte[8];

            stream.Read( data );
            return BitConverter.ToInt64( data );
        }

        public static uint ReadUInt32(this Stream stream)
        {
            Span<byte> data = stackalloc byte[4];

            stream.Read( data );
            return BitConverter.ToUInt32( data );
        }

        public static ulong ReadUInt64(this Stream stream)
        {
            Span<byte> data = stackalloc byte[8];

            stream.Read( data );
            return BitConverter.ToUInt64( data );
        }

        public static float ReadFloat( this Stream stream )
        {
            Span<byte> data = stackalloc byte[4];

            stream.Read( data );
            return BitConverter.ToSingle( data );
        }

        public static string ReadNullTermString( this Stream stream, Encoding encoding )
        {
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

        public static void WriteNullTermString( this Stream stream, string value, Encoding encoding )
        {
            var dataLength = encoding.GetByteCount( value );
            var data = new byte[ dataLength + 1 ];
            encoding.GetBytes( value, 0, value.Length, data, 0 );
            data[ dataLength ] = 0x00; // '\0'

            stream.Write( data, 0, data.Length );
        }

        public static int ReadAll( this Stream stream, byte[] buffer )
        {
            int bytesRead;
            int totalRead = 0;
            while ( ( bytesRead = stream.Read( buffer, totalRead, buffer.Length - totalRead ) ) != 0 )
            {
                totalRead += bytesRead;
            }
            return totalRead;
        }
    }
}
