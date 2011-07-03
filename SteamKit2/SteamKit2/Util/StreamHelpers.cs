using System;
using System.IO;
using System.Text;

namespace SteamKit2
{
    internal static class StreamHelpers
    {
        internal static Int16 ReadInt16(this Stream stream)
        {
            return (Int16)(stream.ReadByte() | stream.ReadByte() << 8);
        }

        internal static Int32 ReadInt32(this Stream stream)
        {
            return (Int32)(stream.ReadByte() | stream.ReadByte() << 8 | 
                           stream.ReadByte() << 16 | stream.ReadByte() << 24);
        }

        internal static UInt64 ReadUInt64(this Stream stream)
        {
            return (UInt64)(stream.ReadByte() | stream.ReadByte() << 8 |
               stream.ReadByte() << 16 | stream.ReadByte() << 24 |
               stream.ReadByte() << 32 | stream.ReadByte() << 40 |
               stream.ReadByte() << 48 | stream.ReadByte() << 56);
        }

        internal static float ReadFloat( this Stream stream )
        {
            return ( float )stream.ReadInt32();
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
