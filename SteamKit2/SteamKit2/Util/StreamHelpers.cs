using System;
using System.IO;
using System.Text;

namespace SteamKit2
{
    internal static class StreamHelpers
    {
        static byte[] data = new byte[8];
        public static Int16 ReadInt16(this Stream stream)
        {
            stream.Read( data, 0, 2 );

            return BitConverter.ToInt16( data, 0 );
        }

        public static UInt16 ReadUInt16(this Stream stream)
        {
            stream.Read(data, 0, 2);

            return BitConverter.ToUInt16(data, 0);
        }

        public static Int32 ReadInt32(this Stream stream)
        {
            stream.Read( data, 0, 4 );

            return BitConverter.ToInt32( data, 0 );
        }

        public static UInt32 ReadUInt32(this Stream stream)
        {
            stream.Read(data, 0, 4);

            return BitConverter.ToUInt32(data, 0);
        }

        public static UInt64 ReadUInt64(this Stream stream)
        {
            stream.Read( data, 0, 8 );

            return BitConverter.ToUInt64( data, 0 );
        }

        public static float ReadFloat( this Stream stream )
        {
            stream.Read( data, 0, 4 );

            return BitConverter.ToSingle( data, 0 );
        }

        public static string ReadNullTermString( this Stream stream, Encoding encoding )
        {
            int characterSize = encoding.GetByteCount( "e" );

            using ( MemoryStream ms = new MemoryStream() )
            {

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

                return encoding.GetString( ms.ToArray() );
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

        private static byte[] bufferCache;

        public static byte[] ReadBytesCached( this Stream stream, int len )
        {
            if (bufferCache == null || bufferCache.Length < len)
                bufferCache = new byte[len];

            stream.Read( bufferCache, 0, len );

            return bufferCache;
        }

        static byte[] discardBuffer = new byte[2 << 12];

        public static void ReadAndDiscard(this Stream stream, int len)
        {
            while (len > discardBuffer.Length)
            {
                stream.Read(discardBuffer, 0, discardBuffer.Length);
                len -= discardBuffer.Length;
            }

            stream.Read(discardBuffer, 0, len);
        }
    }
}
