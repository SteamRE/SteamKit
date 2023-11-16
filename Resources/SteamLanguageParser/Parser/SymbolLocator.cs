using System;
using System.Text.RegularExpressions;

namespace SteamLanguageParser
{
    public class Symbol
    {
    }

    public class StrongSymbol : Symbol
    {
        public Node Class { get; private set; }
        public Node Prop { get; private set; }

        public StrongSymbol(Node classNode)
        {
            Class = classNode;
        }

        public StrongSymbol(Node classNode, Node prop)
        {
            Class = classNode;
            Prop = prop;
        }
    }

    public class WeakSymbol(string ident) : Symbol
    {
        public string Identifier { get; set; } = ident;
    }

    public partial class SymbolLocator
    {
        public const string identifierPattern =
            @"(?<identifier>[a-zA-Z0-9_:]*)";

        public const string fullIdentPattern =
            @"(?<class>[a-zA-Z0-9_]*?)::(?<name>[a-zA-Z0-9_]*)";

        // single level
        private static Node FindNode(Node tree, string symbol)
        {
            foreach (Node child in tree.ChildNodes)
            {
                if (child.Name == symbol)
                {
                    return child;
                }
            }

            return null;
        }

        public static Symbol LookupSymbol(Node tree, string identifier, bool strongonly)
        {
            Match ident = IdentifierRegex().Match(identifier);

            if (!ident.Success)
            {
                throw new Exception("Invalid identifier specified " + identifier);
            }

            if (!identifier.Contains("::"))
            {
                Node classNode = FindNode(tree, ident.Captures[0].Value);

                if (classNode == null)
                {
                    if (strongonly)
                    {
                        throw new Exception("Invalid weak symbol " + identifier);
                    }
                    else
                    {
                        return new WeakSymbol(identifier);
                    }
                }
                else
                {
                    return new StrongSymbol(classNode);
                }
            }
            else
            {
                ident = FullIdentRegex().Match(identifier);

                if (!ident.Success)
                {
                    throw new Exception("Couldn't parse full identifier");
                }

                Node classNode = FindNode(tree, ident.Groups["class"].Value) ?? throw new Exception("Invalid class in identifier " + identifier);
                Node propNode = FindNode(classNode, ident.Groups["name"].Value) ?? throw new Exception("Invalid property in identifier " + identifier);

                return new StrongSymbol(classNode, propNode);
            }

            throw new Exception("Invalid symbol");
        }

        [GeneratedRegex(identifierPattern, RegexOptions.Compiled)]
        private static partial Regex IdentifierRegex();

        [GeneratedRegex(fullIdentPattern, RegexOptions.Compiled)]
        private static partial Regex FullIdentRegex();
    }
}
