using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProtobufDumper
{
    class ExecutableScanner
    {
        static readonly Regex ProtoFileNameRegex = new Regex( @"^[a-zA-Z_0-9\\/.]+\.proto$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        public delegate bool ProcessCandidate( string name, ReadOnlySpan<byte> buffer );

        public static void ScanFile( string fileName, ProcessCandidate processCandidate )
        {
            ScanFile( File.ReadAllBytes( fileName ), processCandidate );
        }

        static void ScanFile( byte[] data, ProcessCandidate processCandidate )
        {
            const char markerStart = '\n';
            const int markerLength = 2;

            var scanSkipOffset = 0;

            for ( var i = 0; i < data.Length - 1; i++ )
            {
                var currentByte = data[ i ];
                var expectedLength = data[ i + 1 ];

                if ( currentByte != markerStart ) continue;

                var y = i + scanSkipOffset;
                for ( ; y < data.Length; y++ )
                {
                    if ( data[ y ] == 0 ) break;
                }

                if ( y == data.Length ) continue;

                var length = y - i;

                if ( length < markerLength || length - 2 < expectedLength ) continue;

                var bufferSpan = new ReadOnlySpan<byte>( data, i, length );
                var nameSpan = bufferSpan.Slice( 2, expectedLength );

                var protoName = Encoding.ASCII.GetString( nameSpan );

                if ( !ProtoFileNameRegex.IsMatch( protoName ) ) continue;

                if ( !processCandidate( protoName, bufferSpan ) )
                {
                    scanSkipOffset = length + 1;
                    i -= 1;
                }
                else
                {
                    i = y;
                    scanSkipOffset = 0;
                }
            }
        }
    }
}
