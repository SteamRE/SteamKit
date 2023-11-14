using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteamLanguageParser
{
    class Token(string name, string value)
    {
        public string Name { get; } = name;
        public string Value { get; } = value;

        public TokenSourceInfo? Source { get; }

        public Token(string name, string value, TokenSourceInfo source)
            : this(name, value)
        {
            Source = source;
        }
    }

    readonly struct TokenSourceInfo(string fileName, int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
    {
        public string FileName { get; } = fileName;
        public int StartLineNumber { get; } = startLineNumber;
        public int StartColumnNumber { get; } = startColumnNumber;
        public int EndLineNumber { get; } = endLineNumber;
        public int EndColumnNumber { get; } = endColumnNumber;
    }

    partial class LanguageParser
    {
        [GeneratedRegex(
            @"(?<whitespace>\s+)|" +
            @"(?<terminator>[;])|" +

            "[\"](?<string>.+?)[\"]|" +

            @"\/\/(?<comment>.*)$|" +

            @"(?<identifier>-?[a-zA-Z_0-9][a-zA-Z0-9_:.]*)|" +
            @"[#](?<preprocess>[a-zA-Z]*)|" +

            @"(?<operator>[{}<>\]=|])|" +
            @"(?<invalid>[^\s]+)",
            RegexOptions.Multiline)]
        private static partial Regex RegexPattern();

        public static Queue<Token> TokenizeString(string buffer, string fileName = "")
        {
            var regexPattern = RegexPattern();
            var bufferLines = buffer.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            MatchCollection matches = regexPattern.Matches(buffer);

            Queue<Token> tokenList = new Queue<Token>();
            foreach (Match match in matches.Cast<Match>())
            {
                int i = 0;
                foreach (Group group in match.Groups.Cast<Group>())
                {
                    string matchValue = group.Value;
                    bool success = group.Success;

                    if (success && i > 1)
                    {
                        string groupName = regexPattern.GroupNameFromNumber(i);

                        if (groupName == "comment")
                        {
                            continue; // don't create tokens for comments
                        }

                        CalculateTextOffset(bufferLines, match.Index, out var startLineNumber, out var startColumnNumber);
                        CalculateTextOffset(bufferLines, match.Index + match.Length, out var endLineNumber, out var endColumnNumber);

                        var tokenSource = new TokenSourceInfo(fileName, startLineNumber, startColumnNumber, endLineNumber, endColumnNumber);
                        var token = new Token(groupName, matchValue, tokenSource);

                        tokenList.Enqueue(token);
                    }
                    i++;
                }

            }

            return tokenList;
        }

        static void CalculateTextOffset(string[] textLines, int index, out int lineNumber, out int columnNumber)
        {
            int offset = 0;
            for (lineNumber = 0; lineNumber < textLines.Length; lineNumber++)
            {
                var currentLineLength = textLines[lineNumber].Length;
                if (offset + currentLineLength >= index)
                {
                    break;
                }

                offset += currentLineLength + Environment.NewLine.Length;
            }

            ArgumentOutOfRangeException.ThrowIfEqual(lineNumber, textLines.Length);

            lineNumber++; // Human line numbering starts from 1, even though it's the 0th line in the file programatically.
            columnNumber = index - offset + 1;
        }
    }
}
