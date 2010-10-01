using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class WeakSymbol : Symbol
    {
        public string Identifier { get; set; }

        public WeakSymbol(string ident)
        {
            Identifier = ident;
        }
    }

    public class SymbolLocator
    {
        public static string identifierPattern =
            @"(?<identifier>[a-zA-Z0-9_:]*)";

        public static string fullIdentPattern =
            @"(?<class>[a-zA-Z0-9_]*?)::(?<name>[a-zA-Z0-9_]*)";

        private static Regex identifierRegex = new Regex(identifierPattern, RegexOptions.Compiled);
        private static Regex fullIdentRegex = new Regex(fullIdentPattern, RegexOptions.Compiled);

        // single level
        private static Node FindNode(Node tree, string symbol)
        {
            foreach (Node child in tree.childNodes)
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
            Match ident = identifierRegex.Match(identifier);

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
                ident = fullIdentRegex.Match(identifier);

                if (!ident.Success)
                {
                    throw new Exception("Couldn't parse full identifier");
                }

                Node classNode = FindNode(tree, ident.Groups["class"].Value);

                if (classNode == null)
                {
                    throw new Exception("Invalid class in identifier " + identifier);
                }

                Node propNode = FindNode(classNode, ident.Groups["name"].Value);

                if (propNode == null)
                {
                    throw new Exception("Invalid property in identifier " + identifier);
                }

                return new StrongSymbol(classNode, propNode);
            }
            
            throw new Exception("Invalid symbol");
        }
 
    }
}
