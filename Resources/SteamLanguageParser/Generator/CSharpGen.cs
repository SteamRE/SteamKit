﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamLanguageParser
{
    class CSharpGen : ICodeGen
    {
        private static Dictionary<String, String> readerTypeMap = new Dictionary<String, String>
        {
            {"byte", "Byte"},
            {"short", "Int16"},
            {"ushort", "UInt16"},
            {"int", "Int32"},
            {"uint", "UInt32"},
            {"long", "Int64"},
            {"ulong", "UInt64"},
        };

        public void EmitNamespace(StringBuilder sb, bool end)
        {
            if (end)
            {
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine("using System;");
                sb.AppendLine("using System.IO;");
                sb.AppendLine( "using System.Runtime.InteropServices;" );
                sb.AppendLine();
                sb.AppendLine("namespace SteamKit2");
                sb.AppendLine("{");
            }
        }

        public void EmitSerialBase(StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "public interface ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tbyte[] Serialize();");
            sb.AppendLine(padding + "\tvoid Deserialize( MemoryStream ms );");
            sb.AppendLine(padding + "}");

            sb.AppendLine(padding + "public interface ISteamSerializableHeader : ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tvoid SetEMsg( EMsg msg );");
            sb.AppendLine(padding + "}");

            sb.AppendLine(padding + "public interface ISteamSerializableMessage : ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tEMsg GetEMsg();");
            sb.AppendLine(padding + "}");

            sb.AppendLine();
        }

        public string EmitType(Symbol sym)
        {
            if (sym is WeakSymbol)
            {
                WeakSymbol wsym = sym as WeakSymbol;

                return wsym.Identifier;
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
                EmitEnumNode(n as EnumNode, sb, level);
            }
        }

        private void EmitEnumNode(EnumNode enode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "public enum " + enode.Name);
            sb.AppendLine(padding + "{");

            string lastValue = "0";

            foreach (PropNode prop in enode.childNodes)
            {
                lastValue = EmitType(prop.Default);
                sb.AppendLine(padding + "\t" + prop.Name + " = " + lastValue + ",");
            }

            int maxint = Int32.Parse(lastValue) + 1;

            sb.AppendLine(padding + "\tMax = " + maxint + ",");
            sb.AppendLine(padding + "}");
        }

        private void EmitClassNode(ClassNode cnode, StringBuilder sb, int level)
        {
            EmitClassDef(cnode, sb, level, false);

            EmitClassIdentity(cnode, sb, level + 1);

            int baseSize = EmitClassProperties(cnode, sb, level + 1);
            EmitClassConstructor(cnode, sb, level + 1);

            EmitClassSerializer(cnode, sb, level + 1, baseSize);
            EmitClassDeserializer(cnode, sb, level + 1, baseSize);

            EmitClassDef(cnode, sb, level, true);
        }

        private void EmitClassDef(ClassNode cnode, StringBuilder sb, int level, bool end)
        {
            string padding = new String('\t', level);

            if (end)
            {
                sb.AppendLine(padding + "}");
                sb.AppendLine();
                return;
            }

            string parent = "ISteamSerializable";

            if (cnode.Ident != null)
            {
                parent = "ISteamSerializableMessage";
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                parent = "ISteamSerializableHeader";
            }

            if ( cnode.Name.Contains( "Hdr" ) )
            {
                sb.AppendLine( padding + "[StructLayout( LayoutKind.Sequential )]" );
            }

            sb.AppendLine(padding + "public class " + cnode.Name + " : " + parent);
            sb.AppendLine(padding + "{");
        }

        private void EmitClassIdentity(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            if (cnode.Ident != null)
            {
                sb.AppendLine(padding + "public EMsg GetEMsg() { return " + EmitType(cnode.Ident) + "; }");
                sb.AppendLine();
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                sb.AppendLine(padding + "public void SetEMsg( EMsg msg ) { this.Msg = msg; }");
                sb.AppendLine();
            }
        }

        private int EmitClassProperties(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);
            int baseClassSize = 0;

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "public " + EmitType(cnode.Parent) + " Header { get; set; }");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                string propName = GetUpperName(prop.Name);

                if (prop.Flags != null && prop.Flags == "const")
                {
                    sb.AppendLine(padding + "public static readonly " + typestr + " " + propName + " = " + EmitType(prop.Default) + ";");
                    continue;
                }

                int size = CodeGenerator.GetTypeSize(prop);
                baseClassSize += size;

                sb.AppendLine( padding + "// Static size: " + size);

                if (prop.Flags != null && prop.Flags == "steamidmarshal" && typestr == "ulong")
                {
                    sb.AppendLine( padding + "private " + typestr + " " + prop.Name + ";");
                    sb.AppendLine( padding + "public SteamID " + propName + " { get { return new SteamID( " + prop.Name + " ); } set { " + prop.Name + " = value.ConvertToUint64(); } }");
                }
                else
                {
                    int temp;
                    if (!String.IsNullOrEmpty(prop.FlagsOpt) && Int32.TryParse(prop.FlagsOpt, out temp))
                    {
                        typestr += "[]";
                    }

                    sb.AppendLine( padding + "public " + typestr + " " + propName + " { get; set; }");
                }
            }

            sb.AppendLine();

            return baseClassSize;
        }

        private void EmitClassConstructor(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "public " + cnode.Name + "()");
            sb.AppendLine(padding + "{");

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tHeader = new " + EmitType(cnode.Parent) + "();");
                sb.AppendLine(padding + "\tHeader.Msg = GetEMsg();");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                Symbol defsym = prop.Default;
                string defflags = prop.Flags;

                string symname = GetUpperName(prop.Name);
                string ctor = EmitType(defsym);

                if (defflags != null && defflags == "proto")
                {
                    ctor = "new " + EmitType(prop.Type) + "()";
                }
                else if (defsym == null)
                {
                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        ctor = "new " + EmitType(prop.Type) + "[" + CodeGenerator.GetTypeSize(prop) + "]";
                    }
                    else
                    {
                        ctor = "0";
                    }
                }
                if (defflags != null && defflags == "steamidmarshal")
                {
                    symname = prop.Name;
                }
                else if (defflags != null && defflags == "const")
                {
                    continue;
                }

                sb.AppendLine(padding + "\t" + symname + " = " + ctor + ";");
            }

            sb.AppendLine(padding + "}");
        }

        private void EmitClassSerializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "public byte[] Serialize()");
            sb.AppendLine(padding + "{");


            // first emit variable length members
            List<String> varLengthProps = new List<String>();
            List<String> openedStreams = new List<String>();
            varLengthProps.Add(baseSize.ToString());

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tbyte[] msHeader = Header.Serialize();");
                varLengthProps.Add("(int)msHeader.Length");
                openedStreams.Add("msHeader");

                sb.AppendLine();
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                if (size == 0)
                {
                    if (prop.Flags != null && prop.Flags == "proto")
                    {
                        sb.AppendLine(padding + "\tMemoryStream ms" + GetUpperName(prop.Name) + " = new MemoryStream();");
                        sb.AppendLine(padding + "\tProtoBuf.Serializer.Serialize<" + typestr + ">(ms" + GetUpperName(prop.Name) + ", " + GetUpperName(prop.Name) + ");");
  
                        if (prop.FlagsOpt != null)
                        {
                            sb.AppendLine(padding + "\t" + GetUpperName(prop.FlagsOpt) + " = (int)ms" + GetUpperName(prop.Name) + ".Length;");
                        }

                        //sb.AppendLine(padding + "\tms" + GetUpperName(prop.Name) + ".Seek( 0, SeekOrigin.Begin );");

                        if (baseSize == 0)
                        {
                            // early exit
                            sb.AppendLine(padding + "\treturn ms" + GetUpperName(prop.Name) + ".ToArray();");
                            sb.AppendLine(padding + "}");
                            return;
                        }
                    }
                    else
                    {
                        sb.AppendLine(padding + "\tMemoryStream ms" + GetUpperName(prop.Name) + " = " + GetUpperName(prop.Name) + ".serialize();");
                    }

                    varLengthProps.Add( "(int)ms" + GetUpperName(prop.Name) + ".Length" );
                    openedStreams.Add( "ms" + GetUpperName(prop.Name) );
                }
            }

            sb.AppendLine(padding + "\tByteBuffer bb = new ByteBuffer( " + String.Join(" + ", varLengthProps.ToArray()) + " );");
            sb.AppendLine();
            //sb.AppendLine(padding + "\tBinaryWriter writer = new BinaryWriter( msBuffer );");
            //sb.AppendLine();

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tmsHeader.CopyTo( msBuffer );");
            }

            // next emit writers
            foreach (PropNode prop in cnode.childNodes)
            {
                string typecast = "";
                string propName = GetUpperName(prop.Name);

                if (prop.Type is StrongSymbol && ((StrongSymbol)prop.Type).Class is EnumNode)
                {
                    EnumNode enode = ((StrongSymbol)prop.Type).Class as EnumNode;

                    if (enode.Type is WeakSymbol)
                        typecast = "(" + ((WeakSymbol)enode.Type).Identifier + ")";
                    else
                        typecast = "(int)";
                }

                if (prop.Flags != null)
                {
                    if (prop.Flags == "steamidmarshal")
                    {
                        propName = prop.Name;
                    }
                    else if (prop.Flags == "proto")
                    {
                        //sb.AppendLine(padding + "\tms" + propName + ".CopyTo( msBuffer );");
                        sb.AppendLine( padding + "\tbyte[] buff = ms" + propName + ".ToArray();" );
                        sb.AppendLine( padding + "\tbb.Append( buff );" );
                        continue;
                    }
                    else if (prop.Flags == "const")
                    {
                        continue;
                    }
                }

                if (prop.Flags == "protomask")
                {
                    propName = "MsgUtil.MakeMsg( " + propName + ", true )";
                }

                sb.AppendLine(padding + "\tbb.Append( " + typecast + propName + " );");
            }

            sb.AppendLine();

            foreach (String stream in openedStreams)
            {
                sb.AppendLine(padding + "\t" + stream + ".Close();");
            }

            //sb.AppendLine();
            //sb.AppendLine(padding + "\tmsBuffer.Seek( 0, SeekOrigin.Begin );");
            sb.AppendLine(padding + "\treturn bb.ToArray();");
            sb.AppendLine(padding + "}");
        }

        private void EmitClassSize( ClassNode cnode, StringBuilder sb, int level )
        {
        }

        private void EmitClassDeserializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "public void Deserialize( MemoryStream ms )");
            sb.AppendLine(padding + "{");

            if (baseSize > 0)
            {
                sb.AppendLine(padding + "\tBinaryReader br = new BinaryReader( ms );");
                sb.AppendLine();
            }

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tHeader.Deserialize( ms );");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                string defflags = prop.Flags;
                string symname = GetUpperName(prop.Name);

                if (defflags != null && defflags == "steamidmarshal")
                {
                    symname = prop.Name;
                }
                else if (defflags != null && defflags == "const")
                {
                    continue;
                }

                if (size == 0)
                {
                    if (prop.Flags != null && prop.Flags == "proto")
                    {
                        if (prop.FlagsOpt != null)
                        {
                            sb.AppendLine(padding + "\tusing( MemoryStream ms" + GetUpperName(prop.Name) + " = new MemoryStream( br.ReadBytes( " + GetUpperName(prop.FlagsOpt) + " ) ) )");
                            sb.AppendLine(padding + "\t\t" + GetUpperName(prop.Name) + " = ProtoBuf.Serializer.Deserialize<" + typestr + ">( ms" + GetUpperName(prop.Name) + " );");
                        }
                        else
                        {
                            sb.AppendLine(padding + "\t" + GetUpperName(prop.Name) + " = ProtoBuf.Serializer.Deserialize<" + typestr + ">( ms );");
                        }
                    }
                    else
                    {
                        sb.AppendLine(padding + "\t" + GetUpperName(prop.Name) + ".Deserialize( ms );");
                    }
                }
                else
                {
                    string typecast = "";
                    if (!readerTypeMap.ContainsKey(typestr))
                    {
                        typecast = "(" + typestr + ")";
                        typestr = CodeGenerator.GetTypeOfSize(size, SupportsUnsignedTypes());
                    }

                    string call = "br.Read" + readerTypeMap[typestr] + "()";

                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        call = "br.Read" + readerTypeMap[typestr] + "s( " + prop.FlagsOpt + " )";
                    }

                    if (prop.Flags == "protomask")
                    {
                        call = "MsgUtil.GetMsg( (uint)" + call + " )";
                    }

                    sb.AppendLine(padding + "\t" + symname + " = " + typecast + call + ";");
                }
            }


            sb.AppendLine(padding + "}");
        }

        public bool SupportsUnsignedTypes()
        {
            return true;
        }

        public bool SupportsNamespace()
        {
            return true;
        }

    }
}
