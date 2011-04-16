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

            if ( args.Length == 0 )
            {
                Console.WriteLine( "No target specified." );
                return;
            }

            string target = args[ 0 ];

            ImageFile imgFile = new ImageFile( target );

            try
            {
                imgFile.Process();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "Unable to process file: {0}", ex.Message );
            }

            imgFile.Unload();
        }
    }
}
