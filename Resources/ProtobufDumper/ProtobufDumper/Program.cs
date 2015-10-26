using System;

namespace ProtobufDumper
{
    class Program
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

            string target = args[ 0 ];
            string output = ( ( args.Length > 1 ) ? args[ 1 ] : null );

            ImageFile imgFile = new ImageFile( target, output );

            try
            {
                imgFile.Process();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "Unable to process file: {0}", ex.Message );
                Console.WriteLine( ex.StackTrace );
                Environment.ExitCode = -1;
            }
        }
    }
}
