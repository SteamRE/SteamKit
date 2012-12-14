using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamLanguageParser
{
    interface ICodeGen
    {
        void EmitNamespace(StringBuilder sb, bool end, string nspace);
        void EmitSerialBase(StringBuilder sb, int level, bool supportsGC);
        void EmitNode(Node n, StringBuilder sb, int level);

        bool SupportsNamespace();
        bool SupportsUnsignedTypes();
    }

    class CodeGenerator
    {
        private class TypeInfo
        {
            public int Size { get; private set; }
            public bool Signed { get; private set; }
            public string SignedType { get; private set; }

            public TypeInfo(int size, string unsigned)
            {
                Size = size;
                Signed = false;
                SignedType = unsigned;
            }

            public TypeInfo(int size)
            {
                Size = size;
                Signed = true;
            }
        }

        private static string defaultType = "uint";
        private static Dictionary<String, TypeInfo> weakTypeMap = new Dictionary<String, TypeInfo>
        {
            {"byte", new TypeInfo(1)},
            {"short", new TypeInfo(2)},
            {"ushort", new TypeInfo(2, "short")},
            {"int", new TypeInfo(4)},
            {"uint", new TypeInfo(4, "int")},
            {"long", new TypeInfo(8)},
            {"ulong", new TypeInfo(8, "long")},
        };

        public static string GetUnsignedType(string type)
        {
            if (weakTypeMap.ContainsKey(type) && !weakTypeMap[type].Signed)
                return weakTypeMap[type].SignedType;

            return type;
        }

        public static string GetTypeOfSize(int size, bool unsigned)
        {
            foreach (string key in weakTypeMap.Keys)
            {
                if (weakTypeMap[key].Size == size)
                {
                    if (unsigned && weakTypeMap[key].Signed == false)
                        return key;
                    else if (weakTypeMap[key].Signed)
                        return key;
                    else if (!weakTypeMap[key].Signed)
                        return weakTypeMap[key].SignedType;
                }
            }

            return "bad";
        }

        public static int GetTypeSize(PropNode prop)
        {
            Symbol sym = prop.Type;

            // no static size for proto
            if (prop.Flags != null && prop.Flags == "proto")
            {
                return 0;
            }

            if (sym is WeakSymbol)
            {
                WeakSymbol wsym = sym as WeakSymbol;
                string key = wsym.Identifier;

                if (!weakTypeMap.ContainsKey(key))
                {
                    key = defaultType;
                }

                if (!String.IsNullOrEmpty(prop.FlagsOpt))
                {
                    return Int32.Parse(prop.FlagsOpt);
                }

                return weakTypeMap[key].Size;
            }
            else if (sym is StrongSymbol)
            {
                StrongSymbol ssym = sym as StrongSymbol;

                if (ssym.Class is EnumNode)
                {
                    EnumNode enode = ssym.Class as EnumNode;

                    if (enode.Type is WeakSymbol)
                        return weakTypeMap[((WeakSymbol)enode.Type).Identifier].Size;
                    else
                        return weakTypeMap[defaultType].Size;
                }
            }

            return 0;
        }

        public static void EmitCode(Node root, ICodeGen gen, StringBuilder sb, string nspace, bool supportsGC, bool internalFile )
        {
            gen.EmitNamespace(sb, false, nspace);

            int level = 0;
            if (gen.SupportsNamespace())
                level = 1;

            if ( internalFile )
                gen.EmitSerialBase( sb, level, supportsGC );

            foreach (Node n in root.childNodes)
            {
                gen.EmitNode(n, sb, level);
            }

            gen.EmitNamespace(sb, true, nspace);
        }

    }
}
