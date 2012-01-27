using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SteamLanguageParser
{
    class Program
    {
        static void Main( string[] args )
        {
            string projectPath = Environment.GetEnvironmentVariable( "SteamRE" );

            if ( !Directory.Exists( projectPath ) )
            {
                throw new Exception( "Unable to find SteamRE project path, please specify the `SteamRE` environment variable" );
            }

            ParseFile( projectPath, @"Resources\SteamLanguage", "steammsg.steamd", "SteamKit2", @"SteamKit2\SteamKit2\Base\Generated\", "SteamLanguage", true );

        }

        private static void ParseFile( string projectPath, string path, string file, string nspace, string outputPath, string outFile, bool supportsGC )
        {
            string languagePath = Path.Combine( projectPath, path );

            Environment.CurrentDirectory = languagePath;
            Queue<Token> tokenList = LanguageParser.TokenizeString( File.ReadAllText( Path.Combine( languagePath, file ) ) );

            Node root = TokenAnalyzer.Analyze( tokenList );

            //JavaGen cgen = new JavaGen();
            CSharpGen cgen = new CSharpGen();

            Node rootEnumNode = new Node();
            Node rootMessageNode = new Node();

            // this is now heavily c# based code
            // anyone looking to modify JavaGen or crate a different generator has their work cut out for them

            rootEnumNode.childNodes.AddRange( root.childNodes.Where( n => n is EnumNode ) );
            rootMessageNode.childNodes.AddRange( root.childNodes.Where( n => n is ClassNode ) );

            StringBuilder enumBuilder = new StringBuilder();
            StringBuilder messageBuilder = new StringBuilder();

            CodeGenerator.EmitCode( rootEnumNode, cgen, enumBuilder, nspace, supportsGC, false );
            CodeGenerator.EmitCode( rootMessageNode, cgen, messageBuilder, nspace + ".Internal", supportsGC, true );

            string outputEnumFile = Path.Combine( outputPath, outFile + ".cs" );
            string outputMessageFile = Path.Combine( outputPath, outFile + "Internal.cs" );

            File.WriteAllText( Path.Combine( projectPath, outputEnumFile ), enumBuilder.ToString() );
            File.WriteAllText( Path.Combine( projectPath, outputMessageFile ), messageBuilder.ToString() );
        }
    }
}
