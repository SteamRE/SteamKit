/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepotDownloader2
{
    static class Log
    {
        public static void WriteLine( string format, params object[] args )
        {
            Console.WriteLine( format, args );
        }
        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void WriteVerbose( string format, params object[] args )
        {
            if ( !Options.Verbose )
                return;

            WriteLine( format, args );
        }
    }
}
