using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLanguageParser
{
    class JavaScriptGen : ICodeGen
    {
        private static Dictionary<String, String> readerTypeMap = new Dictionary<String, String>
        {
            {"byte", "UInt8"},
            {"short", "Int16LE"},
            {"ushort", "UInt16LE"},
            {"int", "Int32LE"},
            {"uint", "UInt32LE"},
            {"long", "Int64LE"},
            {"ulong", "UInt64LE"},
            //{"char", "Char"},
        };

        public void EmitNamespace(StringBuilder sb, bool end, string nspace)
        {
            if (end)
                return;

            sb.AppendLine("var Steam = require('../steam_client');");
            sb.AppendLine("var EMsg = Steam.EMsg;");
            sb.AppendLine("var EUdpPacketType = Steam.EUdpPacketType;");
            sb.AppendLine("var EUniverse = Steam.EUniverse;");
            sb.AppendLine("var EResult = Steam.EResult;");
            sb.AppendLine();
            sb.AppendLine("var protoMask = 0x80000000;");
            sb.AppendLine("require('ref');");
        }

        public void EmitSerialBase(StringBuilder sb, int level, bool supportsGC)
        {
        }

        public string EmitType(Symbol sym)
        {
            if (sym is WeakSymbol)
            {
                WeakSymbol wsym = sym as WeakSymbol;

                return wsym.Identifier.Replace("SteamKit2", "Steam").Replace("ulong.MaxValue", "'18446744073709551615'");
            }
            else if (sym is StrongSymbol)
            {
                StrongSymbol ssym = sym as StrongSymbol;

                if (ssym.Prop == null)
                {
                    return ssym.Class.Name;
                }
                else
                {
                    return ssym.Class.Name + "." + ssym.Prop.Name;
                }
            }

            return "INVALID";
        }

        public string GetUpperName(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Remove(0, 1);
        }

        public void EmitNode(Node n, StringBuilder sb, int level)
        {
            if (n is ClassNode)
            {
                EmitClassNode(n as ClassNode, sb, level);
            }
            else if (n is EnumNode)
            {
                EmitEnumNode(n as EnumNode, sb);
            }
        }

        private void EmitEnumNode(EnumNode enode, StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("Steam." + enode.Name + " = {");

            string lastValue = "0";

            foreach (PropNode prop in enode.childNodes)
            {
                lastValue = EmitType(prop.Default);
                sb.Append("  " + prop.Name + ": " + lastValue + ",");
                if (prop.Obsolete != null)
                {
                    if (prop.Obsolete.Length > 0)
                        sb.Append(" // obsolete - " + prop.Obsolete);
                    else
                        sb.Append(" // obsolete");
                }
                sb.AppendLine();
            }

            sb.AppendLine("};");
        }

        private void EmitClassNode(ClassNode cnode, StringBuilder sb, int level)
        {
            EmitClassDef(cnode, sb, level, false);

            int baseSize = EmitClassProperties(cnode, sb, level + 1);

            EmitClassSerializer(cnode, sb, level + 1, baseSize);
            EmitClassParser(cnode, sb, level + 1, baseSize);

            EmitClassDef(cnode, sb, level, true);
        }

        private void EmitClassDef(ClassNode cnode, StringBuilder sb, int level, bool end)
        {
            string padding = new String(' ', level * 2);

            if (end)
            {
                sb.AppendLine(padding + "};");
                sb.AppendLine();
                sb.AppendLine("Steam.Internal." + cnode.Name + " = " + cnode.Name + ";");
                return;
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(padding + "var " + cnode.Name + " = {");
        }

        private int EmitClassProperties(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String(' ', level * 2);
            int baseClassSize = 0;

            //if (cnode.Parent != null)
            //{
            //    sb.AppendLine(padding + "public " + EmitType(cnode.Parent) + " Header { get; set; }");
            //}

            foreach (PropNode prop in cnode.childNodes)
            {
                string propName = prop.Name;

                if (prop.Flags != null && prop.Flags == "const")
                {
                    sb.AppendLine(padding + propName + ": " + EmitType(prop.Default) + ",");
                    sb.AppendLine();
                    continue;
                }

                int size = CodeGenerator.GetTypeSize(prop);
                baseClassSize += size;
            }

            sb.AppendLine(padding + "baseSize: " + baseClassSize + ",");
            sb.AppendLine();
            return baseClassSize;
        }

        private void EmitClassSerializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String(' ', level * 2);

            //sb.AppendLine();
            sb.AppendLine(padding + "serialize: function(object) {");


            // first emit variable length members
            List<String> varLengthProps = new List<String>();
            varLengthProps.Add(baseSize.ToString());

            //if (cnode.Parent != null)
            //{
            //    sb.AppendLine(padding + "\tHeader.Serialize(stream);");
            //    varLengthProps.Add("(int)msHeader.Length");
            //    openedStreams.Add("msHeader");

            //    sb.AppendLine();
            //}

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                if (size == 0)
                {
                    if (prop.Flags != null && prop.Flags == "proto")
                    {
                        if (baseSize == 0)
                        {
                            // early exit
                            sb.AppendLine(padding + "  return " + typestr + ".serialize(object." + prop.Name + " || {});");
                            sb.AppendLine(padding + "},");
                            return;
                        }

                        sb.AppendLine(padding + "  var buf" + GetUpperName(prop.Name) + " = " + typestr + ".serialize(object." + prop.Name + " || {});");

                        if (prop.FlagsOpt != null)
                        {
                            sb.AppendLine(padding + "  object." + prop.FlagsOpt + " = buf" + GetUpperName(prop.Name) + ".length;");
                        }
                    }
                    else
                    {
                        //sb.AppendLine(padding + "\tMemoryStream ms" + GetUpperName(prop.Name) + " = " + GetUpperName(prop.Name) + ".serialize();");
                    }

                    varLengthProps.Add("buf" + GetUpperName(prop.Name) + ".length");
                }
            }

            sb.AppendLine(padding + "  var buffer = new Buffer(" + String.Join(" + ", varLengthProps.ToArray()) + ");");
            sb.AppendLine();

            if (cnode.Parent != null)
            {
                //sb.AppendLine(padding + "\tmsHeader.CopyTo( msBuffer );");
            }

            // next emit writers
            var offset = 0;
            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                string propName = "object." + prop.Name;

                if (prop.Flags != null)
                {
                    if (prop.Flags == "proto")
                    {
                        sb.AppendLine(padding + "  buf" + GetUpperName(prop.Name) + ".copy(buffer, " + offset + ");");
                        continue;
                    }
                    else if (prop.Flags == "const")
                    {
                        continue;
                    }
                }

                if (prop.Flags == "protomask" || prop.Flags == "protomaskgc")
                {
                    propName = propName + " | protoMask";
                }

                if (!readerTypeMap.ContainsKey(typestr))
                {
                    typestr = CodeGenerator.GetTypeOfSize(size, SupportsUnsignedTypes());
                }

                Symbol defsym = prop.Default;
                string ctor = EmitType(defsym);

                if (defsym == null)
                {
                    ctor = "0";
                }

                string call = "buffer.write" + readerTypeMap[typestr] + "(" + propName + " || " + ctor + ", " + offset + ");";

                if (!String.IsNullOrEmpty(prop.FlagsOpt))
                {
                    call = propName + " && " + propName + ".copy(buffer, " + offset + ");";
                }

                sb.AppendLine(padding + "  " + call);
                offset += size;
            }

            sb.AppendLine();
            sb.AppendLine(padding + "  return buffer;");
            sb.AppendLine(padding + "},");
        }

        private void EmitClassParser(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String(' ', level * 2);

            sb.AppendLine();
            sb.AppendLine(padding + "parse: function(buffer) {");

            sb.AppendLine(padding + "  var object = {};");
            sb.AppendLine();

            if (cnode.Parent != null)
            {
                //sb.AppendLine(padding + "\tHeader.Deserialize( stream );");
            }

            var offset = 0;
            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                string defflags = prop.Flags;
                string symname = "object." + prop.Name;

                if (defflags != null && defflags == "const")
                {
                    continue;
                }

                if (size == 0)
                {
                    if (prop.Flags != null && prop.Flags == "proto")
                    {
                        if (prop.FlagsOpt != null)
                        {
                            sb.AppendLine(padding + "  object." + prop.Name + " = " + typestr + ".parse(buffer.slice(" + offset + ", " + offset + " + object." + prop.FlagsOpt + "));");
                        }
                        else
                        {
                            //sb.AppendLine(padding + "\t" + GetUpperName(prop.Name) + " = ProtoBuf.Serializer.Deserialize<" + typestr + ">( stream );");
                        }
                    }
                    else
                    {
                        //sb.AppendLine(padding + "\t" + GetUpperName(prop.Name) + ".Deserialize( stream );");
                    }
                }
                else
                {
                    if (!readerTypeMap.ContainsKey(typestr))
                    {
                        typestr = CodeGenerator.GetTypeOfSize(size, SupportsUnsignedTypes());
                    }

                    string call = "buffer.read" + readerTypeMap[typestr] + "(" + offset +")";

                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        call = "buffer.slice(" + offset + ", " + offset + " + " + prop.FlagsOpt + ")";
                    }

                    if (prop.Flags == "protomask" || prop.Flags == "protomaskgc")
                    {
                        call = call + " & ~protoMask";
                    }

                    sb.AppendLine(padding + "  " + symname + " = " + call + ";");
                }

                offset += size;
            }

            sb.AppendLine();
            sb.AppendLine(padding + "  return object;");
            sb.AppendLine(padding + "}");
        }

        public bool SupportsUnsignedTypes()
        {
            return true;
        }

        public bool SupportsNamespace()
        {
            return false;
        }

    }
}
