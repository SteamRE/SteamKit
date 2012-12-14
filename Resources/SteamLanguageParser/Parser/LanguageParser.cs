using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SteamLanguageParser
{
    class Token
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public Token(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    class LanguageParser
    {
        public static string pattern =
        @"(?<whitespace>\s+)|" +
        @"(?<terminator>[;])|" +

        "[\"](?<string>.+?)[\"]|" + 

        @"(?<identifier>-?[a-zA-Z_0-9][a-zA-Z0-9_:.]*)|" +
        @"[#](?<preprocess>[a-zA-Z]*)|" + 

        @"(?<operator>[{}<>\]=])|" +
        @"(?<invalid>[^\s]+)";

        private static Regex regexPattern = new Regex(LanguageParser.pattern, RegexOptions.Singleline | RegexOptions.Compiled);

        public static Queue<Token> TokenizeString(string buffer)
        {
            MatchCollection matches = regexPattern.Matches( buffer );

            Queue<Token> tokenList = new Queue<Token>();
            foreach ( Match match in matches )
            {
                int i = 0;
                foreach ( Group group in match.Groups )
                {
                    string matchValue = group.Value;
                    bool success = group.Success;

                    if ( success && i > 1 )
                    {
                        string groupName = regexPattern.GroupNameFromNumber( i );
                        tokenList.Enqueue( new Token( groupName, matchValue ) );
                    }
                    i++;
                }

            }

            return tokenList;
        }
    }
}
