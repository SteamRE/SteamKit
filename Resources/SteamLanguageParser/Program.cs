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
            string projectPath = Environment.GetEnvironmentVariable("SteamRE");

            if (!Directory.Exists(projectPath))
            {
                throw new Exception("Unable to find SteamRE project path, please specify the `SteamRE` environment variable");
            }

            string languagePath = Path.Combine(projectPath, @"Resources\SteamLanguage");

            Environment.CurrentDirectory = languagePath;
            Queue<Token> tokenList = LanguageParser.TokenizeString(File.ReadAllText(Path.Combine(languagePath, "steammsg.steamd")));

            Node root = TokenAnalyzer.Analyze( tokenList );

            StringBuilder sb = new StringBuilder();
            //JavaGen cgen = new JavaGen();
            CSharpGen cgen = new CSharpGen();

            CodeGenerator.EmitCode(root, cgen, sb);
            File.WriteAllText(Path.Combine(projectPath, @"SteamKit2\SteamKit2\Base\SteamLanguage.cs"), sb.ToString());
        }
    }
}
