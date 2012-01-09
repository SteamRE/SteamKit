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

            ParseFile( projectPath, @"Resources\SteamLanguage", "steammsg.steamd", "SteamKit2", @"SteamKit2\SteamKit2\Base\Generated\SteamLanguage.cs", true );

        }

        private static void ParseFile( string projectPath, string path, string file, string nspace, string outputFile, bool supportsGC )
        {
            string languagePath = Path.Combine( projectPath, path );

            Environment.CurrentDirectory = languagePath;
            Queue<Token> tokenList = LanguageParser.TokenizeString( File.ReadAllText( Path.Combine( languagePath, file ) ) );

            Node root = TokenAnalyzer.Analyze( tokenList );

            StringBuilder sb = new StringBuilder();

            //JavaGen cgen = new JavaGen();
            CSharpGen cgen = new CSharpGen();

            CodeGenerator.EmitCode( root, cgen, sb, nspace, supportsGC );
            File.WriteAllText( Path.Combine( projectPath, outputFile ), sb.ToString() );
        }
    }
}
