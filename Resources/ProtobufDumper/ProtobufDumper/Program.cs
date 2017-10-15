using System;

namespace ProtobufDumper
{
    static class Program
    {
        static void Main( string[] args )
        {
            Environment.ExitCode = 0;

            if ( args.Length == 0 )
            {
                Console.WriteLine( "No target specified." );

                Environment.ExitCode = -1;
                return;
            }

            var target = args[ 0 ];
            var output = args.Length > 1 ? args[ 1 ] : null;

            var imgFile = new ImageFile( target, output );
            imgFile.Process();
        }
    }
}
