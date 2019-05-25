using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;

namespace ProtobufGen
{
    static class Program
    {
        public static int Main( string[] args )
        {
            var arguments = Parser.Default.ParseArguments<Options>( args );
            switch ( arguments.Tag )
            {
                case ParserResultType.Parsed:
                    var value = ( ( Parsed<Options> )arguments ).Value;
                    return Run( value );

                case ParserResultType.NotParsed:
                    // The library will automatically write help text. 
                    return -1;

                default:
                    // This should be unreachable.
                    return int.MinValue;
            }
        }

        static int Run(Options arguments)
        {
            var set = new FileDescriptorSet
            {
                DefaultPackage = arguments.Namespace
            };
            set.AddImportPath( Path.GetDirectoryName( arguments.ProtobufPath ) );

            var fileName = Path.GetFileName( arguments.ProtobufPath );
            if (!set.Add( fileName, includeInOutput: true ))
            {
                Console.Error.WriteLine( $"Could not find file '{fileName}'." );
            }

            set.Process();

            var errors = set.GetErrors();
            if ( errors.Length > 0 )
            {
                var errorsCount = 0;

                foreach ( var error in errors )
                {
                    Console.Error.WriteLine( $"{error.File} ({error.LineNumber}, {error.ColumnNumber}): {error.Message}" );

                    if (error.IsError)
                    {
                        errorsCount++;
                    }
                }

                if (errorsCount > 0)
                {
                    return errorsCount;
                }
            }

            var codegen = new SteamKitCSharpCodeGenerator();

            var options = new Dictionary<string, string>
            {
                [ "langver" ] = "7.0",
                [ "names" ] = "original",
            };

            var files = codegen.Generate( set, options: options );

            foreach (var file in files)
            {
                if (Path.GetFileNameWithoutExtension(file.Name) != Path.GetFileNameWithoutExtension( fileName ) )
                {
                    continue;
                }

                File.WriteAllText( arguments.Output, file.Text );
            }

            return 0;
        }
    }

    class Options
    {
        [Option("namespace")]
        public string Namespace { get; set; }

        [Option("proto", Required = true)]
        public string ProtobufPath { get; set; }

        [Option("output", Required = true)]
        public string Output { get; set; }
    }
}
