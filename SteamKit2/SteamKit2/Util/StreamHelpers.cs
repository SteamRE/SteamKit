using System;
using System.Buffers;
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
            var nullTermStride = encoding switch {
                _ when encoding == Encoding.UTF8 => 1,
                _ => encoding.GetByteCount( "e" ),
            };

            var buffer = ArrayPool<byte>.Shared.Rent( 32 );

            try
            {
                var position = 0;

                do
                {
                    var b = stream.ReadByte();

                    if ( b == -1 )
                    {
                        break;
                    }

                    if ( b == 0 && position % nullTermStride == 0 )
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

                return encoding.GetString( buffer[ ..position ] );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( buffer );
            }
        }

        public static void WriteNullTermString( this Stream stream, string value, Encoding encoding )
        {
            const string NullTerm = "\0";
            value ??= string.Empty;

            var stringByteCount = encoding.GetByteCount( value );
            var nullTermByteCount = encoding.GetByteCount( NullTerm );
            var totalByteCount = stringByteCount + nullTermByteCount;

            var isLargeBuffer = totalByteCount > 256;
            var rented = isLargeBuffer ? ArrayPool<byte>.Shared.Rent( totalByteCount ) : null;

            try
            {
                Span<byte> encodedSpan = isLargeBuffer ? rented.AsSpan( 0, totalByteCount ) : stackalloc byte[ totalByteCount ];

                encoding.GetBytes( value.AsSpan(), encodedSpan[ ..stringByteCount ] );
                encoding.GetBytes( NullTerm.AsSpan(), encodedSpan[ stringByteCount.. ] );

                stream.Write( encodedSpan );
            }
            finally
            {
                if ( rented != null )
                {
                    ArrayPool<byte>.Shared.Return( rented );
                }
            }
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
