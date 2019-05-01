using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            var targets = new List<String>();
            string output = null;

            for ( var i = 0; i < args.Length; i++ )
            {
                if ( i == 0 || i < args.Length - 1 )
                {
                    targets.Add(args[i]);
                }
                else
                {
                    output = args[ i ];
                }
            }

            var outputDir = output ?? Path.GetFileNameWithoutExtension( targets[0] );

            ProtobufCollector collector = new ProtobufCollector();

            foreach ( var target in targets )
            {
                ExecutableScanner.ScanFile( target, ( name, buffer ) =>
                {
                    if ( Environment.GetCommandLineArgs().Contains( "-dump", StringComparer.OrdinalIgnoreCase ) )
                    {
                        Directory.CreateDirectory( outputDir );
                        var fileName = Path.Combine( outputDir, $"{name}.dump" );
                        Directory.CreateDirectory( Path.GetDirectoryName( fileName ) );

                        Console.WriteLine( "  ! Dumping to '{0}'!", fileName );

                        try
                        {
                            File.WriteAllBytes( fileName, buffer );
                        }
                        catch ( Exception ex )
                        {
                            Console.WriteLine( "Unable to dump: {0}", ex.Message );
                        }
                    }

                    return collector.CollectCandidate( name, buffer );
                } );
            }

            ProtobufDumper dumper = new ProtobufDumper(collector.Candidates);

            if ( dumper.Analyze() )
            {
                dumper.DumpFiles( ( name, buffer ) =>
                {
                    Directory.CreateDirectory( outputDir );
                    var outputFile = Path.Combine( outputDir, name );

                    Console.WriteLine( "  ! Outputting proto to '{0}'", outputFile );
                    Directory.CreateDirectory( Path.GetDirectoryName( outputFile ) );

                    File.WriteAllText( outputFile, buffer.ToString() );
                } );
            }
        }
    }
}
