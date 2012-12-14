using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamLanguageParser {

    public abstract class ObjCGenBase : ICodeGen
    {
        protected static Dictionary<String, String> readerTypeMap = new Dictionary<String, String>
        {
            {"byte", "UInt8"},
            {"short", "Int16"},
            {"ushort", "UInt16"},
            {"int", "Int32"},
            {"uint", "UInt32"},
            {"long", "Int64"},
            {"ulong", "UInt64"},
            {"char", "UInt8"},
            {"uint8_t", "UInt8"},
            {"int16_t", "Int16"},
            {"uint16_t", "UInt16"},
            {"int32_t", "Int32"},
            {"uint32_t", "UInt32"},
            {"int64_t", "Int64"},
            {"uint64_t", "UInt64"},
            {"NSData", "DataOfLength"},
        };

        protected static Dictionary<String, String> weakTypeMap = new Dictionary<String, String>
        {
            {"byte", "uint8_t"},
            {"short", "int16_t"},
            {"ushort", "uint16_t"},
            {"int", "int32_t"},
            {"uint", "uint32_t"},
            {"long", "int64_t"},
            {"ulong", "uint64_t"},
        };

        public virtual void EmitNamespace(StringBuilder sb, bool end, string nspace) {
            if (!end) {
                sb.AppendLine("//");
                sb.AppendLine("// This file is subject to the software licence as defined in");
                sb.AppendLine("// the file 'LICENCE.txt' included in this source code package.");
                sb.AppendLine("//");
                sb.AppendLine("// This file was automatically generated. Do not edit it.");
                sb.AppendLine("//");
                sb.AppendLine();
            }
        }

        public virtual string EmitType(Symbol sym)
        {
            if (sym is WeakSymbol)
            {
                WeakSymbol wsym = sym as WeakSymbol;
                string identifier = wsym.Identifier;

                if (weakTypeMap.ContainsKey(identifier))
                {
                    return weakTypeMap[identifier];
                }
                else if(identifier.StartsWith("CMsg"))
                {
                    return identifier + " *";
                }
                else if (identifier == "ulong.MaxValue")
                {
                    return "ULLONG_MAX";
                } 
                else
                {
                    return identifier;
                }
            } else if (sym is StrongSymbol)
            {
                StrongSymbol ssym = sym as StrongSymbol;

                if (ssym.Prop == null)
                {
                    return ssym.Class.Name;
                }
                else
                {
                    if (ssym.Class.Name.StartsWith("E"))
                    {
                        return ssym.Class.Name + ssym.Prop.Name;
                    }
                    else
                    {
                        return "[_SK" + ssym.Class.Name + " " + ssym.Prop.Name + "]";
                    }
                }
            }

            return "INVALID";
        }

        public virtual void EmitSerialBase(StringBuilder sb, int level, bool supportsGC) { }

        public void EmitNode(Node n, StringBuilder sb, int level)
        {
            if (n is ClassNode)
            {
                EmitClassNode(n as ClassNode, sb, level);
            }
            else if (n is EnumNode)
            {
                EmitEnumNode(n as EnumNode, sb, level);
            }
        }

        protected virtual void EmitClassNode(ClassNode n, StringBuilder sb, int level) { }
        protected virtual void EmitEnumNode(EnumNode n, StringBuilder sb, int level) { }

        public bool SupportsNamespace() { return false; }
        public bool SupportsUnsignedTypes() { return true;  }

        protected string GetPropertyName(string varName)
        {
            return varName.Substring(0, 1).ToLower() + varName.Substring(1);
        }

        protected static string GetLastPartNameFromDottedType(string name)
        {
            return name.Split('.').Last();
        }
    }
}
