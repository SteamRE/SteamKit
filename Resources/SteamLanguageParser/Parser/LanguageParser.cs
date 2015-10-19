using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamLanguageParser
{
    class Token
    {
        public string Name { get; }
        public string Value { get; }

        public TokenSourceInfo? Source { get; }

        public Token( string name, string value )
        {
            Name = name;
            Value = value;
        }

        public Token( string name, string value, TokenSourceInfo source )
            : this( name, value )
        {
            Source = source;
        }
    }

    struct TokenSourceInfo
    {
        public TokenSourceInfo(string fileName, int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
        {
            FileName = fileName;
            StartLineNumber = startLineNumber;
            StartColumnNumber = startColumnNumber;
            EndLineNumber = endLineNumber;
            EndColumnNumber = endColumnNumber;
        }

        public string FileName { get; }
        public int StartLineNumber { get; }
        public int StartColumnNumber { get; }
        public int EndLineNumber { get; }
        public int EndColumnNumber { get; }
    }
    
    class LanguageParser
    {
        public static string pattern =
        @"(?<whitespace>\s+)|" +
        @"(?<terminator>[;])|" +

        "[\"](?<string>.+?)[\"]|" + 

        @"\/\/(?<comment>.*)$|" +

        @"(?<identifier>-?[a-zA-Z_0-9][a-zA-Z0-9_:.]*)|" +
        @"[#](?<preprocess>[a-zA-Z]*)|" + 

        @"(?<operator>[{}<>\]=|])|" +
        @"(?<invalid>[^\s]+)";

        private static Regex regexPattern = new Regex( pattern, RegexOptions.Multiline | RegexOptions.Compiled );

        public static Queue<Token> TokenizeString( string buffer, string fileName = "" )
        {
            var bufferLines = buffer.Split( new[] { Environment.NewLine }, StringSplitOptions.None );

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

                        if ( groupName == "comment" )
                            continue; // don't create tokens for comments

                        int startLineNumber, startColumnNumber, endLineNumber, endColumnNumber;
                        CalculateTextOffset( bufferLines, match.Index, out startLineNumber, out startColumnNumber);
                        CalculateTextOffset( bufferLines, match.Index + match.Length, out endLineNumber, out endColumnNumber);

                        var tokenSource = new TokenSourceInfo( fileName, startLineNumber, startColumnNumber, endLineNumber, endColumnNumber );
                        var token = new Token( groupName, matchValue, tokenSource );

                        tokenList.Enqueue( token );
                    }
                    i++;
                }

            }

            return tokenList;
        }

        static void CalculateTextOffset( string[] textLines, int index, out int lineNumber, out int columnNumber )
        {
            int offset = 0;
            for ( lineNumber = 0; lineNumber < textLines.Length; lineNumber++ )
            {
                var currentLineLength = textLines[ lineNumber ].Length;
                if ( offset + currentLineLength >= index )
                {
                    break;
                }

                offset += currentLineLength + Environment.NewLine.Length;
            }

            if ( lineNumber == textLines.Length )
            {
                throw new ArgumentOutOfRangeException( "index must be less than the full text length when re-joined." );
            }

            lineNumber++; // Human line numbering starts from 1, even though it's the 0th line in the file programatically.
            columnNumber = index - offset + 1;
        }
    }
}
