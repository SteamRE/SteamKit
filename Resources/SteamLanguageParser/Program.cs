using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamLanguageParser
{
    class Program
    {
        static void Main( string[] args )
        {
            string projectPath = Environment.GetEnvironmentVariable( "SteamRE" ) ?? args.SingleOrDefault();

            if ( !Directory.Exists( projectPath ))
            {
                throw new Exception( "Unable to find SteamRE project path, please specify the `SteamRE` environment variable" );
            }

            ParseFile( projectPath, Path.Combine( "Resources", "SteamLanguage" ), "steammsg.steamd", "SteamKit2", Path.Combine( "SteamKit2", "SteamKit2", "Base", "Generated"), "SteamLanguage", true, new CSharpGen(), "cs" );

            //ParseFile( projectPath, @"Resources\SteamLanguage", "steammsg.steamd", "SteamKit2", @"SteamKit2\ObjC\", "SteamLanguage", true, new ObjCInterfaceGen(), "h" );
            //ParseFile( projectPath, @"Resources\SteamLanguage", "steammsg.steamd", "SteamKit2", @"SteamKit2\ObjC\", "SteamLanguage", true, new ObjCImplementationGen(), "m" );

        }

        private static void ParseFile(string projectPath, string path, string file, string nspace, string outputPath, string outFile, bool supportsGC, ICodeGen codeGen, string fileNameSuffix)
        {
            string languagePath = Path.Combine( projectPath, path );

            Directory.SetCurrentDirectory( languagePath );
            Queue<Token> tokenList = LanguageParser.TokenizeString( File.ReadAllText( Path.Combine( languagePath, file ) ) );

            Node root = TokenAnalyzer.Analyze( tokenList );
            
            Node rootEnumNode = new Node();
            Node rootMessageNode = new Node();

            // this is now heavily c# based code
            // anyone looking to modify JavaGen or crate a different generator has their work cut out for them

            rootEnumNode.childNodes.AddRange( root.childNodes.Where( n => n is EnumNode ) );
            rootMessageNode.childNodes.AddRange( root.childNodes.Where( n => n is ClassNode ) );

            StringBuilder enumBuilder = new StringBuilder();
            StringBuilder messageBuilder = new StringBuilder();

            CodeGenerator.EmitCode( rootEnumNode, codeGen, enumBuilder, nspace, supportsGC, false );
            CodeGenerator.EmitCode(rootMessageNode, codeGen, messageBuilder, nspace + ".Internal", supportsGC, true);

            string outputEnumFile = Path.Combine( outputPath, outFile + "." + fileNameSuffix );
            string outputMessageFile = Path.Combine( outputPath, outFile + "Internal." + fileNameSuffix );

            File.WriteAllText( Path.Combine( projectPath, outputEnumFile ), enumBuilder.ToString() );
            File.WriteAllText( Path.Combine( projectPath, outputMessageFile ), messageBuilder.ToString() );
        }
    }
}
