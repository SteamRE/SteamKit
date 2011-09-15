using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace ProtobufDumper
{
    class Program
    {
        static unsafe void Main( string[] args )
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
                Environment.ExitCode = -1;
            }
            finally
            {
                imgFile.Unload();
            }
        }
    }
}
