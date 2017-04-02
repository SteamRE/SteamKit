using System;
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
            {"char", "Char"},
        };

        public void EmitNamespace(StringBuilder sb, bool end, string nspace)
        {
            if (end)
            {
                sb.AppendLine("}");
                sb.AppendLine( "#pragma warning restore 1591" );
                sb.AppendLine( "#pragma warning restore 0219" );
            }
            else
            {
                sb.AppendLine( "#pragma warning disable 1591" ); // this will hide "Missing XML comment for publicly visible type or member 'Type_or_Member'"
                sb.AppendLine( "#pragma warning disable 0219" ); // Warning CS0219: The variable `(variable)' is assigned but its value is never used
                sb.AppendLine("using System;");
                sb.AppendLine("using System.IO;");
                sb.AppendLine( "using System.Runtime.InteropServices;" );
                sb.AppendLine();
                sb.AppendLine( string.Format( "namespace {0}", nspace ) );
                sb.AppendLine("{");
            }
        }

        public void EmitSerialBase(StringBuilder sb, int level, bool supportsGC)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "public interface ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tvoid Serialize(Stream stream);");
            sb.AppendLine(padding + "\tvoid Deserialize( Stream stream );");
            sb.AppendLine(padding + "}");

            sb.AppendLine(padding + "public interface ISteamSerializableHeader : ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tvoid SetEMsg( EMsg msg );");
            sb.AppendLine(padding + "}");

            sb.AppendLine(padding + "public interface ISteamSerializableMessage : ISteamSerializable");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tEMsg GetEMsg();");
            sb.AppendLine(padding + "}");

            if ( supportsGC )
            {
                sb.AppendLine( padding + "public interface IGCSerializableHeader : ISteamSerializable" );
                sb.AppendLine( padding + "{" );
                sb.AppendLine( padding + "\tvoid SetEMsg( uint msg );" );
                sb.AppendLine( padding + "}" );

                sb.AppendLine( padding + "public interface IGCSerializableMessage : ISteamSerializable" );
                sb.AppendLine( padding + "{" );
                sb.AppendLine( padding + "\tuint GetEMsg();" );
                sb.AppendLine( padding + "}" );
            }

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
        public string EmitMultipleTypes( List<Symbol> syms, string operation = "|" )
        {
            var identList = syms.OfType<WeakSymbol>().Select( wsym => wsym.Identifier );
            return string.Join( " " + operation + " ", identList );
        }

        public string GetUpperName(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Remove(0, 1);
        }

        public void EmitNode(Node n, StringBuilder sb, int level)
        {
            if (n is ClassNode cn)
            {
                if (cn.Emit)
                {
                    EmitClassNode(cn, sb, level);
                }
            }
            else if (n is EnumNode en)
            {
                EmitEnumNode(en, sb, level);
            }
        }

        private void EmitEnumNode(EnumNode enode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            if ( enode.Flags == "flags" )
                sb.AppendLine( padding + "[Flags]" );

            if ( enode.Type != null )
            {
                sb.AppendLine( padding + "public enum " + enode.Name + " : " + EmitType( enode.Type ) );
            }
            else
            {
                sb.AppendLine( padding + "public enum " + enode.Name );
            }

            sb.AppendLine( padding + "{" );

            string lastValue = "0";

            foreach (PropNode prop in enode.childNodes)
            {
                if (!prop.Emit)
                {
                    continue;
                }

                lastValue = EmitMultipleTypes(prop.Default);

                if ( prop.Obsolete != null )
                {
                    if ( prop.Obsolete.Length > 0 )
                        sb.AppendLine( padding + "\t[Obsolete( \"" + prop.Obsolete + "\" )]" );
                    else
                        sb.AppendLine( padding + "\t[Obsolete]" );
                }
                sb.AppendLine(padding + "\t" + prop.Name + " = " + lastValue + ",");
            }

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
                if ( cnode.Name.Contains( "MsgGC" ) )
                {
                    parent = "IGCSerializableMessage";
                }
                else
                {
                    parent = "ISteamSerializableMessage";
                }
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                if ( cnode.Name.Contains( "MsgGC" ) )
                    parent = "IGCSerializableHeader";
                else
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
                var cnodeIdentAsStrongSymbol = cnode.Ident as StrongSymbol;
                var supressObsoletionWarning = false;

                if (cnodeIdentAsStrongSymbol != null)
                {
                    var propNode = cnodeIdentAsStrongSymbol.Prop as PropNode;
                    if (propNode != null && propNode.Obsolete != null)
                    {
                        supressObsoletionWarning = true;
                    }
                }

                if (supressObsoletionWarning)
                {
                    sb.AppendLine( padding + "#pragma warning disable 0612" );
                }

                if ( cnode.Name.Contains( "MsgGC" ) )
                {
                    sb.AppendLine( padding + "public uint GetEMsg() { return " + EmitType( cnode.Ident ) + "; }" );
                }
                else
                {
                    sb.AppendLine( padding + "public EMsg GetEMsg() { return " + EmitType( cnode.Ident ) + "; }" );
                }

                if (supressObsoletionWarning)
                {
                    sb.AppendLine(padding + "#pragma warning restore 0612");
                }

                sb.AppendLine();
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                if ( cnode.Name.Contains( "MsgGC" ) )
                {
                    if ( cnode.childNodes.Find( node => node.Name == "msg" ) != null )
                    {
                        sb.AppendLine( padding + "public void SetEMsg( uint msg ) { this.Msg = msg; }" );
                        sb.AppendLine();
                    }
                    else
                    {
                        // this is required for a gc header which doesn't have an emsg
                        sb.AppendLine( padding + "public void SetEMsg( uint msg ) { }" );
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine( padding + "public void SetEMsg( EMsg msg ) { this.Msg = msg; }" );
                    sb.AppendLine();
                }
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
                    sb.AppendLine(padding + "public static readonly " + typestr + " " + propName + " = " + EmitType(prop.Default.FirstOrDefault()) + ";");
                    continue;
                }

                int size = CodeGenerator.GetTypeSize(prop);
                baseClassSize += size;

                sb.AppendLine( padding + "// Static size: " + size);

                if (prop.Flags != null && prop.Flags == "steamidmarshal" && typestr == "ulong")
                {
                    sb.AppendLine( padding + string.Format( "private {0} {1};", typestr, prop.Name ) );
                    sb.AppendLine( padding + "public SteamID " + propName + " { get { return new SteamID( " + prop.Name + " ); } set { " + prop.Name + " = value.ConvertToUInt64(); } }");
                }
                else if ( prop.Flags != null && prop.Flags == "boolmarshal" && typestr == "byte" )
                {
                    sb.AppendLine( padding + string.Format( "private {0} {1};", typestr, prop.Name ) );
                    sb.AppendLine( padding + "public bool " + propName + " { get { return ( " + prop.Name + " == 1 ); } set { " + prop.Name + " = ( byte )( value ? 1 : 0 ); } }" );
                }
                else if ( prop.Flags != null && prop.Flags == "gameidmarshal" && typestr == "ulong" )
                {
                    sb.AppendLine( padding + string.Format( "private {0} {1};", typestr, prop.Name ) );
                    sb.AppendLine( padding + "public GameID " + propName + " { get { return new GameID( " + prop.Name + " ); } set { " + prop.Name + " = value.ToUInt64(); } }" );
                }
                else
                {
                    int temp;
                    if ( !String.IsNullOrEmpty( prop.FlagsOpt ) && Int32.TryParse( prop.FlagsOpt, out temp ) )
                    {
                        typestr += "[]";
                    }

                    sb.AppendLine( padding + "public " + typestr + " " + propName + " { get; set; }" );
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
                Symbol defsym = prop.Default.FirstOrDefault();
                string defflags = prop.Flags;

                string symname = GetUpperName(prop.Name);
                string ctor = EmitType(defsym);

                if (defflags != null && defflags == "proto")
                {
                    ctor = "new " + EmitType(prop.Type) + "()";
                }
                else if (defsym == null)
                {
                    if ( !String.IsNullOrEmpty( prop.FlagsOpt ) )
                    {
                        ctor = "new " + EmitType( prop.Type ) + "[" + CodeGenerator.GetTypeSize( prop ) + "]";
                    }
                    else
                    {
                        ctor = "0";
                    }
                }
                if (defflags != null && ( defflags == "steamidmarshal" || defflags == "gameidmarshal" || defflags == "boolmarshal" ))
                {
                    symname = prop.Name;
                }

                else if ( defflags != null && defflags == "const" )
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
            sb.AppendLine(padding + "public void Serialize(Stream stream)");
            sb.AppendLine(padding + "{");


            // first emit variable length members
            List<String> varLengthProps = new List<String>();
            List<String> openedStreams = new List<String>();
            varLengthProps.Add(baseSize.ToString());

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tHeader.Serialize(stream);");
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
                        if (baseSize == 0)
                        {
                            // early exit
                            sb.AppendLine(padding + "\tProtoBuf.Serializer.Serialize<" + typestr + ">(stream, " + GetUpperName(prop.Name) + ");");
                            sb.AppendLine(padding + "}");
                            return;
                        }

                        sb.AppendLine(padding + "\tMemoryStream ms" + GetUpperName(prop.Name) + " = new MemoryStream();");
                        sb.AppendLine(padding + "\tProtoBuf.Serializer.Serialize<" + typestr + ">(ms" + GetUpperName(prop.Name) + ", " + GetUpperName(prop.Name) + ");");
  
                        if (prop.FlagsOpt != null)
                        {
                            sb.AppendLine(padding + "\t" + GetUpperName(prop.FlagsOpt) + " = (int)ms" + GetUpperName(prop.Name) + ".Length;");
                        }

                        //sb.AppendLine(padding + "\tms" + GetUpperName(prop.Name) + ".Seek( 0, SeekOrigin.Begin );");
                    }
                    else
                    {
                        sb.AppendLine(padding + "\tMemoryStream ms" + GetUpperName(prop.Name) + " = " + GetUpperName(prop.Name) + ".serialize();");
                    }

                    varLengthProps.Add( "(int)ms" + GetUpperName(prop.Name) + ".Length" );
                    openedStreams.Add( "ms" + GetUpperName(prop.Name) );
                }
            }

            //sb.AppendLine(padding + "\tBinaryWriterEx bw = new BinaryWriterEx( stream );");
            //sb.AppendLine();
            sb.AppendLine(padding + "\tBinaryWriter bw = new BinaryWriter( stream );");
            sb.AppendLine();

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
                    if ( prop.Flags == "steamidmarshal" || prop.Flags == "gameidmarshal" || prop.Flags == "boolmarshal" )
                    {
                        propName = prop.Name;
                    }
                    else if ( prop.Flags == "proto" )
                    {
                        sb.AppendLine( padding + "\tbw.Write( ms" + propName + ".ToArray() );" );
                        continue;
                    }
                    else if ( prop.Flags == "const" )
                    {
                        continue;
                    }
                }

                if (prop.Flags == "protomask")
                {
                    propName = "MsgUtil.MakeMsg( " + propName + ", true )";
                }
                else if ( prop.Flags == "protomaskgc" )
                {
                    propName = "MsgUtil.MakeGCMsg( " + propName + ", true )";
                }

                sb.AppendLine(padding + "\tbw.Write( " + typecast + propName + " );");
            }

            sb.AppendLine();

            foreach (String stream in openedStreams)
            {
                sb.AppendLine(padding + "\t" + stream + ".Dispose();");
            }

            //sb.AppendLine();
            //sb.AppendLine(padding + "\tmsBuffer.Seek( 0, SeekOrigin.Begin );");
            sb.AppendLine(padding + "}");
        }

        private void EmitClassSize( ClassNode cnode, StringBuilder sb, int level )
        {
        }

        private void EmitClassDeserializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "public void Deserialize( Stream stream )");
            sb.AppendLine(padding + "{");

            if (baseSize > 0)
            {
                sb.AppendLine(padding + "\tBinaryReader br = new BinaryReader( stream );");
                sb.AppendLine();
            }

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\tHeader.Deserialize( stream );");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                string defflags = prop.Flags;
                string symname = GetUpperName(prop.Name);

                if ( defflags != null && ( defflags == "steamidmarshal" || defflags == "gameidmarshal" || defflags == "boolmarshal" ) )
                {
                    symname = prop.Name;
                }
                else if ( defflags != null && defflags == "const" )
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
                            sb.AppendLine(padding + "\t" + GetUpperName(prop.Name) + " = ProtoBuf.Serializer.Deserialize<" + typestr + ">( stream );");
                        }
                    }
                    else
                    {
                        sb.AppendLine(padding + "\t" + GetUpperName(prop.Name) + ".Deserialize( stream );");
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
                    else if ( prop.Flags == "protomaskgc" )
                    {
                        call = "MsgUtil.GetGCMsg( (uint)" + call + " )";
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
