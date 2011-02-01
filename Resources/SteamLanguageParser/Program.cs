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
        static void Main(string[] args)
        {
            Queue<Token> tokenList = LanguageParser.TokenizeString( File.ReadAllText( @"G:\dev\SteamRE\Resources\SteamLanguage\steammsg.steamd" ) );

            Node root = TokenAnalyzer.Analyze( tokenList );

            StringBuilder sb = new StringBuilder();
            //JavaGen cgen = new JavaGen();
            CSharpGen cgen = new CSharpGen();

            CodeGenerator.EmitCode(root, cgen, sb);
            File.WriteAllText( @"G:\dev\SteamRE\SteamKit2\SteamKit2\Base\SteamLanguage.cs", sb.ToString() );
        }
    }
}
