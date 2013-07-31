using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamLanguageParser
{
    class JavaGen : ICodeGen
    {
        private static Dictionary<String, String> readerTypeMap = new Dictionary<String, String>
        {
            {"byte", ""},
            {"short", "Short"},
            {"int", "Int"},
            {"long", "Long"},
        };

        public void EmitNamespace(StringBuilder sb, bool end, string nspace)
        {
            if (end)
            {
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine("package net.steam3;");
                sb.AppendLine("import java.nio.*;");
                sb.AppendLine("import steamkit.steam3.SteamMessages.*;");
                sb.AppendLine("import steamkit.util.MsgUtil;");
                sb.AppendLine("import com.google.protobuf.InvalidProtocolBufferException;");

                sb.AppendLine();

                sb.AppendLine("public final class SteamLanguage {");
            }
        }

        public void EmitSerialBase(StringBuilder sb, int level, bool supportsGC)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "public interface ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tpublic ByteBuffer serialize();");
            sb.AppendLine(padding + "\tpublic void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException;");
            sb.AppendLine(padding + "}");

            sb.AppendLine(padding + "public interface ISteamSerializableHeader extends ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tpublic void SetEMsg( EMsg msg );");
            sb.AppendLine(padding + "}");

            sb.AppendLine(padding + "public interface ISteamSerializableMessage extends ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tpublic EMsg GetEMsg();");
            sb.AppendLine(padding + "}");

            sb.AppendLine();
        }

        public string EmitType(Symbol sym)
        {
            if (sym is WeakSymbol)
            {
                WeakSymbol wsym = sym as WeakSymbol;

                return CodeGenerator.GetUnsignedType(wsym.Identifier);
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
                lastValue = EmitType(prop.Default.FirstOrDefault());
                sb.AppendLine(padding + "\t" + prop.Name + "(" + lastValue + "),");
            }

            int maxint = Int32.Parse(lastValue) + 1;

            sb.AppendLine(padding + "\tMax(" + maxint + ");");

            sb.AppendLine();
            sb.AppendLine(padding + "\tprivate int code;");

            sb.AppendLine(padding + "\tprivate " + enode.Name + "( int c ) { code = c; }");

            sb.AppendLine(padding + "\tpublic int getCode() {");
            sb.AppendLine(padding + "\t\treturn code;");
            sb.AppendLine(padding + "\t}");
    
            sb.AppendLine(padding + "\tpublic static " + enode.Name + " lookup( int code ) {");
            sb.AppendLine(padding + "\t\tfor ( " + enode.Name + " x : values() ) {");
            sb.AppendLine(padding + "\t\t\tif( x.getCode() == code ) return x;");
            sb.AppendLine(padding + "\t\t}");
            sb.AppendLine(padding + "\t\treturn Invalid;");
            sb.AppendLine(padding + "\t}");

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

            sb.AppendLine(padding + "public static class " + cnode.Name + " implements " + parent);
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
                sb.AppendLine(padding + "public void SetEMsg( EMsg msg ) { this.msg = msg; }");
                sb.AppendLine();
            }
        }

        private int EmitClassProperties(ClassNode cnode, StringBuilder sb, int level)
        {
            StringBuilder sbAccessors = new StringBuilder();

            string padding = new String('\t', level);
            int baseClassSize = 0;

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "private " + EmitType(cnode.Parent) + " header;");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                string propName = prop.Name;

                if (prop.Flags != null && prop.Flags == "const")
                {
                    sb.AppendLine(padding + "public static final " + typestr + " " + propName + " = " + EmitType(prop.Default.FirstOrDefault()) + ";");
                    continue;
                }

                int size = CodeGenerator.GetTypeSize(prop);
                baseClassSize += size;

                sb.AppendLine( padding + "// Static size: " + size);

                int temp;
                if (!String.IsNullOrEmpty(prop.FlagsOpt) && Int32.TryParse(prop.FlagsOpt, out temp))
                {
                    typestr += "[]";
                }

                sb.AppendLine(padding + "private " + typestr + " " + propName + ";");

                string upname = GetUpperName(propName);
                sbAccessors.AppendLine(padding + "public " + typestr + " get" + upname + "() { return " + propName + "; }");
                sbAccessors.AppendLine(padding + "public void set" + upname + "( " + typestr + " value ) { " + propName + " = value; }");
            }

            sb.AppendLine();

            sb.Append(sbAccessors.ToString());
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
                Symbol defsym = prop.Default.FirstOrDefault();
                string defflags = prop.Flags;

                string symname = prop.Name;
                string ctor = EmitType(defsym);
                string proptype = EmitType(prop.Type);

                if (defflags != null && defflags == "proto")
                {
                    ctor = proptype + ".getDefaultInstance()";
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
                else if (proptype == "byte")
                {
                    ctor = "(byte)" + ctor;
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
            sb.AppendLine(padding + "public ByteBuffer serialize()");
            sb.AppendLine(padding + "{");


            // first emit variable length members
            List<String> varLengthProps = new List<String>();
            varLengthProps.Add(baseSize.ToString());

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tByteBuffer bufHeader = Header.serialize();");
                varLengthProps.Add("bufHeader.limit()");

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
                        sb.AppendLine(padding + "\tByteBuffer buf" + GetUpperName(prop.Name) + " = " + prop.Name + ".toByteString().asReadOnlyByteBuffer();");

                        if (prop.FlagsOpt != null)
                        {
                            sb.AppendLine(padding + "\t" + prop.FlagsOpt + " = buf" + GetUpperName(prop.Name) + ".limit();");
                        }

                        if (baseSize == 0)
                        {
                            // early exit
                            sb.AppendLine(padding + "\treturn buf" + GetUpperName(prop.Name) + ";");
                            sb.AppendLine(padding + "}");
                            return;
                        }

                    }
                    else
                    {
                        sb.AppendLine(padding + "\tByteBuffer buf" + GetUpperName(prop.Name) + " = " + GetUpperName(prop.Name) + ".serialize();");
                    }

                    varLengthProps.Add( "buf" + GetUpperName(prop.Name) + ".limit()" );
                }
            }

            sb.AppendLine(padding + "\tByteBuffer buffer = ByteBuffer.allocate( " + String.Join(" + ", varLengthProps.ToArray()) + " );");
            sb.AppendLine(padding + "\tbuffer.order( ByteOrder.LITTLE_ENDIAN );");
            sb.AppendLine();

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tbuffer.put( bufHeader );");
            }

            // next emit writers
            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                string typecast = "";
                string propName = prop.Name;
                int size = CodeGenerator.GetTypeSize(prop);

                if (prop.Type is StrongSymbol && ((StrongSymbol)prop.Type).Class is EnumNode)
                {
                    EnumNode enode = ((StrongSymbol)prop.Type).Class as EnumNode;

                    if (enode.Type is WeakSymbol)
                        typecast = "(" + ((WeakSymbol)enode.Type).Identifier + ")";
                    else
                        typecast = "(int)";

                    propName = propName + ".getCode()";
                }
                
                if (!readerTypeMap.ContainsKey(typestr))
                {
                    typestr = CodeGenerator.GetTypeOfSize(size, SupportsUnsignedTypes());
                }

                if (prop.Flags != null)
                {
                    if (prop.Flags == "steamidmarshal")
                    {

                    }
                    else if (prop.Flags == "proto")
                    {
                        sb.AppendLine(padding + "\tbuffer.put( buf" + GetUpperName(propName) + " );");
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

                sb.AppendLine(padding + "\tbuffer.put" + readerTypeMap[typestr] + "( " + typecast + propName + " );");
            }

            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine(padding + "\tbuffer.flip();");
            sb.AppendLine(padding + "\treturn buffer;");
            sb.AppendLine(padding + "}");
        }

        private void EmitClassDeserializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException");
            sb.AppendLine(padding + "{");

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\theader.deserialize( buffer );");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                string defflags = prop.Flags;
                string symname = prop.Name;

                if (defflags != null && defflags == "steamidmarshal")
                {
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
                            sb.AppendLine(padding + "\tbyte[] buf" + GetUpperName(prop.Name) + " = new byte[ " + prop.FlagsOpt + " ];");

                        }
                        else
                        {
                            sb.AppendLine(padding + "\tbyte[] buf" + GetUpperName(prop.Name) + " = new byte[ buffer.limit() - buffer.position() ];");
                        }

                        sb.AppendLine(padding + "\tbuffer.get( buf" + GetUpperName(prop.Name) + " );");
                        sb.AppendLine(padding + "\t" + prop.Name + " = " + typestr + ".parseFrom( buf" + GetUpperName(prop.Name) + " );");
                    }
                    else
                    {
                        sb.AppendLine(padding + "\t" + prop.Name + ".deserialize( ms );");
                    }
                }
                else
                {
                    string typecast = "";
                    string basetype = typestr;

                    if (!readerTypeMap.ContainsKey(typestr))
                    {
                        typecast = "(" + typestr + ")";
                        typestr = CodeGenerator.GetTypeOfSize(size, SupportsUnsignedTypes());
                    }

                    string call = "buffer.get" + readerTypeMap[typestr] + "()";

                    if (!String.IsNullOrEmpty(typecast))
                    {
                        call = basetype + ".lookup( " + call + " )";
                    }

                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        call = "new " + typestr + "[" + prop.FlagsOpt + "]";
                    }

                    if (prop.Flags == "protomask")
                    {
                        call = "MsgUtil.GetMsg( " + call + " )";
                    }

                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        // ctor creates array
                        sb.AppendLine(padding + "\tbuffer.get( " + symname + " );");
                    }
                    else
                    {
                        sb.AppendLine(padding + "\t" + symname + " = " + typecast + call + ";");
                    }
                }
            }

            sb.AppendLine(padding + "}");
        }

        public bool SupportsUnsignedTypes()
        {
            return false;
        }

        public bool SupportsNamespace()
        {
            return true;
        }

    }
}
