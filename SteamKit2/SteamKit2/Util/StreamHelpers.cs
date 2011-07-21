using System;
using System.IO;
using System.Text;

namespace SteamKit2
{
    internal static class StreamHelpers
    {
        internal static Int16 ReadInt16(this Stream stream)
        {
            byte[] data = new byte[ 2 ];
            stream.Read( data, 0, data.Length );

            return BitConverter.ToInt16( data, 0 );
        }

        internal static Int32 ReadInt32(this Stream stream)
        {
            byte[] data = new byte[ 4 ];
            stream.Read( data, 0, data.Length );

            return BitConverter.ToInt32( data, 0 );
        }

        internal static UInt64 ReadUInt64(this Stream stream)
        {
            byte[] data = new byte[ 8 ];
            stream.Read( data, 0, data.Length );

            return BitConverter.ToUInt64( data, 0 );
        }

        internal static float ReadFloat( this Stream stream )
        {
            byte[] data = new byte[ 4 ];
            stream.Read( data, 0, data.Length );

            return BitConverter.ToSingle( data, 0 );
        }

        internal static string ReadNullTermString( this Stream stream, Encoding encoding )
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

        internal static byte[] ReadBytes( this Stream stream, int len )
        {
            byte[] data = new byte[ len ];

            stream.Read( data, 0, len );

            return data;
        }
    }
}
