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

            var namedArgs = new List<String>();
            var unnamedArgs = new List<String>();

            foreach ( var arg in args )
            {
                if ( arg.StartsWith( '-' ) )
                    namedArgs.Add( arg );
                else
                    unnamedArgs.Add( arg );
            }

            if ( unnamedArgs.Count == 0 )
            {
                Console.WriteLine( "No target specified." );

                Environment.ExitCode = -1;
                return;
            }

            var hasDumpCandidates = namedArgs.Contains( "-dump", StringComparer.OrdinalIgnoreCase );

            var targets = new List<String>();
            string output = null;

            for ( var i = 0; i < unnamedArgs.Count; i++ )
            {
                var exists = File.Exists( unnamedArgs[ i ] );

                if ( i == 0 || i < unnamedArgs.Count - 1 )
                {
                    targets.Add( unnamedArgs[ i ] );

                    if ( exists ) continue;

                    Console.WriteLine( "Could not find file {0}", unnamedArgs[ i ] );

                    Environment.ExitCode = -1;
                    return;
                }
                else
                {
                    output = unnamedArgs[ i ];

                    if ( !exists ) continue;

                    Console.WriteLine( "Output directory path is not valid {0}", unnamedArgs[ i ] );

                    Environment.ExitCode = -1;
                    return;
                }
            }

            var outputDir = output ?? Path.GetFileNameWithoutExtension( targets[ 0 ] );

            var collector = new ProtobufCollector();

            foreach ( var target in targets )
            {
                Console.WriteLine( "Loading binary '{0}'...", target );

                ExecutableScanner.ScanFile( target, ( name, buffer ) =>
                {
                    if ( collector.Candidates.Find( c => c.name == name ) != null ) return true;

                    Console.Write( "{0}... ", name );

                    var complete = collector.TryParseCandidate( name, buffer, out var result, out var error );

                    switch ( result )
                    {
                        case ProtobufCollector.CandidateResult.OK:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine( "OK!" );
                            Console.ResetColor();
                            break;

                        case ProtobufCollector.CandidateResult.Rescan:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine( "needs rescan: {0}", error.Message );
                            Console.ResetColor();
                            break;

                        default:
                        case ProtobufCollector.CandidateResult.Invalid:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine( "is invalid: {0}", error.Message );
                            Console.ResetColor();
                            break;
                    }

                    if ( complete && hasDumpCandidates )
                    {
                        var fileName = Path.Combine( outputDir, $"{name}.dump" );
                        Directory.CreateDirectory( Path.GetDirectoryName( fileName ) );

                        Console.WriteLine( "  ! Dumping to '{0}'!", fileName );

                        try
                        {
                            using ( var file = File.OpenWrite( fileName ) )
                            {
                                buffer.Seek( 0, SeekOrigin.Begin );
                                file.SetLength( buffer.Length );
                                buffer.CopyTo( file );
                            }
                        }
                        catch ( Exception ex )
                        {
                            Console.WriteLine( "Unable to dump: {0}", ex.Message );
                        }
                    }

                    return complete;
                } );
            }

            var dumper = new ProtobufDumper( collector.Candidates );

            if ( dumper.Analyze() )
            {
                dumper.DumpFiles( ( name, buffer ) =>
                {
                    var outputFile = Path.Combine( outputDir, name );

                    Console.WriteLine( "  ! Outputting proto to '{0}'", outputFile );
                    Directory.CreateDirectory( Path.GetDirectoryName( outputFile ) );

                    File.WriteAllText( outputFile, buffer.ToString() );
                } );
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( "Dump failed. Not all dependencies and types were found." );
                Console.ResetColor();

                Environment.ExitCode = -1;
            }
        }
    }
}
