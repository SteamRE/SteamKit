using System;
using System.Collections.Generic;
using System.IO;

namespace SteamLanguageParser
{
    public class Node
    {
        public List<Node> childNodes { get; private set; }
        public string Name { get; set; }

        public Node()
        {
            childNodes = new List<Node>();
        }
    }

    public class ClassNode : Node
    {
        public Symbol Ident { get; set; }
        public Symbol Parent { get; set; }
        public bool Emit { get; set; }
    }

    public class PropNode : Node
    {
        public string Flags { get; set; }
        public string FlagsOpt { get; set; }
        public Symbol Type { get; set; }
        public List<Symbol> Default { get; set; }
        public string Obsolete { get; set; }
        public string Removed { get; set; }
        public bool Emit { get; set; }

        public PropNode()
        {
            Default = new List<Symbol>();
            Emit = true;
        }
    }

    public class EnumNode : Node
    {
        public string Flags { get; set; }
        public Symbol Type { get; set; }
    }

    class TokenAnalyzer
    {
        public static Node Analyze( Queue<Token> tokens )
        {
            Node root = new Node();

            while (tokens.Count > 0)
            {
                Token cur = tokens.Dequeue();

                switch (cur.Name)
                {
                    case "EOF":
                        break;
                    case "preprocess":
                        Token text = Expect(tokens, "string");

                        if (cur.Value == "import")
                        {
                            Queue<Token> parentTokens = LanguageParser.TokenizeString( File.ReadAllText( text.Value ), text.Value );

                            Node newRoot = Analyze( parentTokens );

                            foreach (Node child in newRoot.childNodes)
                            {
                                root.childNodes.Add(child);
                            }
                        }
                        break;
                    case "identifier":
                        switch (cur.Value)
                        {
                            case "class":
                                {
                                    Token name = Expect(tokens, "identifier");
                                    Token ident = null, parent = null;

                                    Token op1 = Optional(tokens, "operator", "<");
                                    if (op1 != null)
                                    {
                                        ident = Expect(tokens, "identifier");
                                        Token op2 = Expect(tokens, "operator", ">");
                                    }

                                    Token expect = Optional(tokens, "identifier", "expects");
                                    if (expect != null)
                                    {
                                        parent = Expect(tokens, "identifier");
                                    }

                                    Token removed = Optional(tokens, "identifier", "removed");

                                    ClassNode cnode = new ClassNode();
                                    cnode.Name = name.Value;

                                    if (ident != null)
                                    {
                                        cnode.Ident = SymbolLocator.LookupSymbol(root, ident.Value, false);
                                    }

                                    if (parent != null)
                                    {
                                        //cnode.Parent = SymbolLocator.LookupSymbol(root, parent.Value, true);
                                    }

                                    if (removed != null)
                                    {
                                        cnode.Emit = false;
                                    }
                                    else
                                    {
                                        cnode.Emit = true;
                                    }

                                    root.childNodes.Add(cnode);
                                    ParseInnerScope(tokens, cnode, root);
                                }
                                break;
                            case "enum":
                                {
                                    Token name = Expect(tokens, "identifier");
                                    Token datatype = null;

                                    Token op1 = Optional(tokens, "operator", "<");
                                    if (op1 != null)
                                    {
                                        datatype = Expect(tokens, "identifier");
                                        Token op2 = Expect(tokens, "operator", ">");
                                    }

                                    Token flag = Optional( tokens, "identifier", "flags" );

                                    EnumNode enode = new EnumNode();
                                    enode.Name = name.Value;

                                    if ( flag != null )
                                    {
                                        enode.Flags = flag.Value;
                                    }

                                    if (datatype != null)
                                    {
                                        enode.Type = SymbolLocator.LookupSymbol(root, datatype.Value, false);
                                    }


                                    root.childNodes.Add(enode);
                                    ParseInnerScope(tokens, enode, root);
                                }
                                break;
                        }
                        break;
                }
            }

            return root;
        }

        private static void ParseInnerScope(Queue<Token> tokens, Node parent, Node root)
        {
            Token scope1 = Expect(tokens, "operator", "{");
            Token scope2 = Optional(tokens, "operator", "}");

            while (scope2 == null)
            {
                PropNode pnode = new PropNode();

                Token t1 = tokens.Dequeue();

                Token t1op1 = Optional(tokens, "operator", "<");
                Token flagop = null;

                if (t1op1 != null)
                {
                    flagop = Expect(tokens, "identifier");
                    Token t1op2 = Expect(tokens, "operator", ">");

                    pnode.FlagsOpt = flagop.Value;
                }

                Token t2 = Optional(tokens, "identifier");
                Token t3 = Optional(tokens, "identifier");

                if (t3 != null)
                {
                    pnode.Name = t3.Value;
                    pnode.Type = SymbolLocator.LookupSymbol(root, t2.Value, false);
                    pnode.Flags = t1.Value;
                }
                else if (t2 != null)
                {
                    pnode.Name = t2.Value;
                    pnode.Type = SymbolLocator.LookupSymbol(root, t1.Value, false);
                }
                else
                {
                    pnode.Name = t1.Value;
                }

                Token defop = Optional(tokens, "operator", "=");

                if ( defop != null )
                {
                    while ( true )
                    {
                        Token value = tokens.Dequeue();
                        pnode.Default.Add( SymbolLocator.LookupSymbol( root, value.Value, false ) );

                        if ( Optional( tokens, "operator", "|" ) != null )
                            continue;

                        Expect( tokens, "terminator", ";" );
                        break;
                    }
                }
                else
                {
                    Expect( tokens, "terminator", ";" );
                }

                Token obsolete = Optional( tokens, "identifier", "obsolete" );
                if ( obsolete != null )
                {
                    // Obsolete identifiers are output when generating the language, but include a warning
                    pnode.Obsolete = "";

                    Token obsoleteReason = Optional( tokens, "string" );

                    if ( obsoleteReason != null )
                        pnode.Obsolete = obsoleteReason.Value;
                }

                Token removed = Optional( tokens, "identifier", "removed" );
                if ( removed != null )
                {
                    // Removed identifiers are not output when generating the language
                    pnode.Emit = false;

                    // Consume and record the removed reason so it's available in the node graph
                    pnode.Removed = "";

                    Token removedReason = Optional( tokens, "string" );

                    if ( removedReason != null )
                        pnode.Removed = removedReason.Value;
                }

                parent.childNodes.Add(pnode);

                scope2 = Optional(tokens, "operator", "}");
            }
        }

        private static Token Expect(Queue<Token> tokens, string name)
        {
            Token peek = tokens.Peek();

            if (peek == null)
            {
                return new Token("EOF", "");
            }

            if(peek.Name != name)
            {
                throw new Exception("Expecting " + name);
            }

            return tokens.Dequeue();
        }

        private static Token Expect(Queue<Token> tokens, string name, string value)
        {
            Token peek = tokens.Peek();

            if (peek == null)
            {
                return new Token("EOF", "");
            }

            if (peek.Name != name || peek.Value != value)
            {
                if (peek.Source.HasValue)
                {
                    var source = peek.Source.Value;
                    throw new Exception( $"Expecting {name} '{value}', but got '{peek.Value}' at {source.FileName} {source.StartLineNumber},{source.StartColumnNumber}-{source.EndLineNumber},{source.EndColumnNumber}" );
                }
                else
                {
                    throw new Exception("Expecting " + name + " '" + value + "', but got '" + peek.Value + "'");
                }
            }

            return tokens.Dequeue();
        }

        private static Token Optional(Queue<Token> tokens, string name)
        {
            Token peek = tokens.Peek();

            if (peek == null)
            {
                return new Token("EOF", "");
            }

            if (peek.Name != name)
            {
                return null;
            }

            return tokens.Dequeue();
        }

        private static Token Optional(Queue<Token> tokens, string name, string value)
        {
            Token peek = tokens.Peek();

            if (peek == null)
            {
                return new Token("EOF", "");
            }

            if (peek.Name != name || peek.Value != value)
            {
                return null;
            }

            return tokens.Dequeue();
        }
    }
}
