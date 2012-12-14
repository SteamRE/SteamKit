using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    public class PropNode : Node
    {
        public string Flags { get; set; }
        public string FlagsOpt { get; set; }
        public Symbol Type { get; set; }
        public Symbol Default { get; set; }
    }

    public class EnumNode : Node
    {
        public string Flags { get; set; }
        public Symbol Type { get; set; }
    }

    class TokenAnalyzer
    {
        public static Node Analyze(Queue<Token> tokens)
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
                            Queue<Token> parentTokens = LanguageParser.TokenizeString(File.ReadAllText(text.Value));

                            Node newRoot = Analyze(parentTokens);

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

                                    Token flag = Optional(tokens, "identifier");

                                    EnumNode enode = new EnumNode();
                                    enode.Name = name.Value;

                                    if (flag != null)
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

                if (defop != null)
                {
                    Token value = tokens.Dequeue();
                    pnode.Default = SymbolLocator.LookupSymbol(root, value.Value, false);
                }

                Expect(tokens, "terminator", ";");

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

            if (peek.Name != name)
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
                throw new Exception("Expecting " + name);
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
