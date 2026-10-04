using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProtobufDumper
{
    partial class ExecutableScanner
    {
        [GeneratedRegex( @"^[a-zA-Z_0-9\\/.]+\.proto$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant )]
        private static partial Regex ProtoFileNameRegex();

        public delegate bool ProcessCandidate( string name, Stream buffer, out long bytesConsumed );

        public static void ScanFile( string fileName, ProcessCandidate processCandidate )
        {
            ScanFile( File.ReadAllBytes( fileName ), processCandidate );
        }

        static void ScanFile( byte[] data, ProcessCandidate processCandidate )
        {
            const byte markerStart = 0x0A;

            var i = 0;
            while ( i < data.Length - 1 )
            {
                i = Array.IndexOf( data, markerStart, i );

                if ( i == -1 || i >= data.Length - 1 )
                {
                    break;
                }

                // The name length is a varint, names of 128 characters or more take two bytes
                var expectedLength = data[ i + 1 ] & 0x7F;
                var markerLength = 2;

                if ( ( data[ i + 1 ] & 0x80 ) != 0 )
                {
                    if ( i + 2 >= data.Length || ( data[ i + 2 ] & 0x80 ) != 0 )
                    {
                        i++;
                        continue;
                    }

                    expectedLength |= data[ i + 2 ] << 7;
                    markerLength = 3;
                }

                if ( i + markerLength + expectedLength > data.Length )
                {
                    i++;
                    continue;
                }

                var protoName = Encoding.ASCII.GetString( data, i + markerLength, expectedLength );

                if ( !protoName.EndsWith( ".proto", StringComparison.OrdinalIgnoreCase ) )
                {
                    i++;
                    continue;
                }

                if ( !ProtoFileNameRegex().IsMatch( protoName ) )
                {
                    Console.WriteLine( $"Skipping potentially valid '{protoName}'" );
                    i++;
                    continue;
                }

                using var buffer = new MemoryStream( data, i, data.Length - i );
                if ( !processCandidate( protoName, buffer, out var bytesConsumed ) )
                {
                    i++;
                    continue;
                }

                i += ( int )Math.Max( 1, bytesConsumed );
            }
        }
    }
}
