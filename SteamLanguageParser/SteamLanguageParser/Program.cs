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
            Queue<Token> tokenList = LanguageParser.TokenizeString( File.ReadAllText( @"steammsg.steamd" ) );

            Node root = TokenAnalyzer.Analyze( tokenList );

            StringBuilder sb = new StringBuilder();
            JavaGen cgen = new JavaGen();

            CodeGenerator.EmitCode(root, cgen, sb);
            File.WriteAllText(@"C:\steamre\test.java", sb.ToString());
        }
    }
}
