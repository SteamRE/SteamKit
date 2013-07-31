using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamLanguageParser
{
    class ObjCImplementationGen : ObjCGenBase
    {
        public override void EmitNamespace(StringBuilder sb, bool end, string nspace)
        {
            if (!end)
            {
                sb.AppendLine("#import \"SteamLanguageInternal.h\"");
                sb.AppendLine("#import \"_SKMsgUtil.h\"");
                sb.AppendLine("#import <CRBoilerplate/CRBoilerplate.h>");
                sb.AppendLine();
            }
        }

        public string GetUpperName(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Remove(0, 1);
        }

        protected override void EmitEnumNode(EnumNode enode, StringBuilder sb, int level)
        {
            // Do nothing. Enums are already defined in the @interface generator.
        }

        protected override void EmitClassNode(ClassNode cnode, StringBuilder sb, int level)
        {
            EmitClassDef(cnode, sb, level, false);

            EmitClassIdentity(cnode, sb, level + 1);

            int baseSize = EmitClassProperties(cnode, sb, level + 1);
            EmitClassConstructor(cnode, sb, level + 1);
            EmitClassDestructor(cnode, sb, level + 1);

            EmitClassSerializer(cnode, sb, level + 1, baseSize);
            EmitClassDeserializer(cnode, sb, level + 1, baseSize);

            EmitClassDef(cnode, sb, level, true);
        }

        private void EmitClassDef(ClassNode cnode, StringBuilder sb, int level, bool end)
        {
            string padding = new String('\t', level);

            if (end)
            {
                sb.AppendLine(padding + "@end");
                sb.AppendLine();
                return;
            }


            if (cnode.Name.Contains("Hdr"))
            {
                sb.AppendLine(padding + "//[StructLayout( LayoutKind.Sequential )]");
            }

            sb.AppendLine(padding + "@implementation _SK" + cnode.Name);
            sb.AppendLine();
        }

        private void EmitClassIdentity(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            if (cnode.Ident != null)
            {
                if (cnode.Name.Contains("MsgGC"))
                {
                    sb.AppendLine(padding + "- (EGCMsg) eMsg");
                    sb.AppendLine(padding + "{");
                    sb.AppendLine(padding + "\treturn " + EmitType(cnode.Ident) + ";");
                    sb.AppendLine(padding + "}");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(padding + "- (EMsg) eMsg");
                    sb.AppendLine(padding + "{");
                    sb.AppendLine(padding + "\treturn " + EmitType(cnode.Ident) + ";");
                    sb.AppendLine(padding + "}");
                    sb.AppendLine();
                }
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                if (cnode.Name.Contains("MsgGC"))
                {
                    if (cnode.childNodes.Find(node => node.Name == "msg") != null)
                    {
                        sb.AppendLine(padding + "- (void) setEMsg:(EGCMsg)_msg");
                        sb.AppendLine(padding + "{");
                        sb.AppendLine(padding + "\tself.msg = _msg;");
                        sb.AppendLine(padding + "}");
                        sb.AppendLine();
                    }
                    else
                    {
                        // this is required for a gc header which doesn't have an emsg
                        sb.AppendLine(padding + "- (void) setEMsg:(EGCMsg)msg { }");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine(padding + "- (void) setEMsg:(EMsg)_msg");
                    sb.AppendLine(padding + "{");
                    sb.AppendLine(padding + "\tself.msg = _msg;");
                    sb.AppendLine(padding + "}");
                    sb.AppendLine();
                }
            }
        }

        private int EmitClassProperties(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);
            int baseClassSize = 0;

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                string propName = GetPropertyName(prop.Name);

                if (prop.Flags != null && prop.Flags == "const")
                {
                    sb.AppendLine(padding + "+ (" + typestr + ") " + GetUpperName(propName));
                    sb.AppendLine(padding + "{");
                    sb.AppendLine(padding + "\t return " + EmitType(prop.Default.FirstOrDefault()) + ";");
                    sb.AppendLine(padding + "}");
                    continue;
                }

                int size = CodeGenerator.GetTypeSize(prop);
                baseClassSize += size;

                sb.AppendLine(padding + "// Static size: " + size);

                if (prop.Flags != null && prop.Flags == "steamidmarshal" && typestr == "ulong")
                {
                    sb.AppendLine(padding + string.Format("private {0} {1};", typestr, prop.Name));
                    sb.AppendLine(padding + "public SteamID " + propName + " { get { return new SteamID( " + prop.Name + " ); } set { " + prop.Name + " = value.ConvertToUint64(); } }");
                }
                else if (prop.Flags != null && prop.Flags == "boolmarshal" && typestr == "byte")
                {
                    sb.AppendLine(padding + string.Format("private {0} {1};", typestr, prop.Name));
                    sb.AppendLine(padding + "public bool " + propName + " { get { return ( " + prop.Name + " == 1 ); } set { " + prop.Name + " = ( byte )( value ? 1 : 0 ); } }");
                }
                else if (prop.Flags != null && prop.Flags == "gameidmarshal" && typestr == "ulong")
                {
                    sb.AppendLine(padding + string.Format("private {0} {1};", typestr, prop.Name));
                    sb.AppendLine(padding + "public GameID " + propName + " { get { return new GameID( " + prop.Name + " ); } set { " + prop.Name + " = value.ToUint64(); } }");
                }
                else
                {
                    int temp;
                    if (!String.IsNullOrEmpty(prop.FlagsOpt) && Int32.TryParse(prop.FlagsOpt, out temp))
                    {
                        typestr += "[]";
                    }

                    sb.AppendLine(padding + "@synthesize " + propName + ";");
                }
            }

            sb.AppendLine();

            return baseClassSize;
        }

        private void EmitClassConstructor(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "- (id) init");
            sb.AppendLine(padding + "{");
            sb.AppendLine(padding + "\tself = [super init];");
            sb.AppendLine(padding + "\tif (self) {");

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\t\tself.header = new " + EmitType(cnode.Parent) + "();");
                sb.AppendLine(padding + "\t\t[self.header setMsg:[self eMsg]];");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                Symbol defsym = prop.Default.FirstOrDefault();
                string defflags = prop.Flags;

                string symname = GetUpperName(prop.Name);
                string ctor = EmitType(defsym);

                if (defflags != null && defflags == "proto")
                {
                    ctor = "nil";
                }
                else if (defsym == null)
                {
                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        ctor = "nil";
                    }
                    else if (prop.Type as StrongSymbol != null && ((StrongSymbol)prop.Type).Class.Name.StartsWith("E"))
                    {
                        ctor = "(" + ((StrongSymbol)prop.Type).Class.Name + ") 0";
                    }
                    else
                    {
                        ctor = "0";
                    }
                }
                if (defflags != null && (defflags == "steamidmarshal" || defflags == "gameidmarshal" || defflags == "boolmarshal"))
                {
                    symname = prop.Name;
                }
                else if (defflags != null && defflags == "const")
                {
                    continue;
                }

                sb.AppendLine(padding + "\t\tself." + GetPropertyName(symname) + " = " + ctor + ";");
            }

            sb.AppendLine(padding + "\t}");
            sb.AppendLine(padding + "\treturn self;");
            sb.AppendLine(padding + "}");
        }

        private void EmitClassDestructor(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            sb.AppendLine(padding + "- (void) dealloc");
            sb.AppendLine(padding + "{");

            foreach (PropNode prop in cnode.childNodes)
            {
                Symbol defsym = prop.Default.FirstOrDefault();
                string defflags = prop.Flags;

                string symname = GetUpperName(prop.Name);
                string ctor = EmitType(defsym);

                if (defflags != null && defflags == "proto")
                {
                    //sb.AppendLine(padding + "\tdelete " + GetPropertyName(symname) + ";");
                }
            }

            sb.AppendLine(padding + "}");
        }

        private void EmitClassSerializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "- (void) serialize:(NSMutableData *)_data");
            sb.AppendLine(padding + "{");
            sb.AppendLine();

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "\t[self.header serialize:data];");
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
                        //sb.AppendLine(padding + "\t[_data appendProtobuf:" + GetPropertyName(prop.Name) + "];");
                    }
                    else
                    {
                        sb.AppendLine(padding + "\tMemoryStream ms" + GetUpperName(prop.Name) + " = " + GetUpperName(prop.Name) + ".serialize();");
                    }
                }
            }

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
                    if (prop.Flags == "steamidmarshal" || prop.Flags == "gameidmarshal" || prop.Flags == "boolmarshal")
                    {
                        propName = prop.Name;
                    }
                    else if (prop.Flags == "proto")
                    {
                        sb.AppendLine(padding + "\t[_data appendData:[" + GetPropertyName(propName) + " data]];");
                        continue;
                    }
                    else if (prop.Flags == "const")
                    {
                        continue;
                    }
                }

                if (prop.Flags == "protomask")
                {
                    if (prop.Default.FirstOrDefault() is StrongSymbol && (prop.Default.FirstOrDefault() as StrongSymbol).Class.Name == "EGCMsg")
                        propName = "[_SKMsgUtil makeGCMsg:" + GetPropertyName(propName) + " isProtobuf:YES]";
                    else
                        propName = "[_SKMsgUtil makeMsg:" + GetPropertyName(propName) + " isProtobuf:YES]";
                }

                int size = CodeGenerator.GetTypeSize(prop);
                string typestr = CodeGenerator.GetTypeOfSize(size, SupportsUnsignedTypes());

                if (!String.IsNullOrEmpty(prop.FlagsOpt))
                {
                    sb.AppendLine(padding + "\t[_data appendData:" + GetPropertyName(propName) + "];");
                }
                else
                {
                    sb.AppendLine(padding + "\t[_data cr_append" + readerTypeMap[typestr] + ":" + GetPropertyName(propName) + "];");
                }

                //sb.AppendLine(padding + "\tbw.Write( " + typecast + propName + " );");
            }

            sb.AppendLine();

            //sb.AppendLine();
            //sb.AppendLine(padding + "\tmsBuffer.Seek( 0, SeekOrigin.Begin );");
            sb.AppendLine(padding + "}");
        }

        private void EmitClassSize(ClassNode cnode, StringBuilder sb, int level)
        {
        }

        private void EmitClassDeserializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "- (void) deserializeWithReader:(CRDataReader *)reader");
            sb.AppendLine(padding + "{");

            if (cnode.Parent != null)
            {
                sb.AppendLine(padding + "[self.header deserialize:reader]");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                int size = CodeGenerator.GetTypeSize(prop);

                string defflags = prop.Flags;
                string symname = GetUpperName(prop.Name);

                if (defflags != null && (defflags == "steamidmarshal" || defflags == "gameidmarshal" || defflags == "boolmarshal"))
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
                            sb.AppendLine(padding + "\tNSData * " + GetPropertyName(prop.Name) + "Data = [reader readDataOfLength:" + prop.FlagsOpt + "];");
                            sb.AppendLine(padding + "\tself." + GetPropertyName(prop.Name) + " = [" + GetLastPartNameFromDottedType(EmitType(prop.Type)).Trim(' ', '*') + " parseFromData:" + GetPropertyName(prop.Name) + "Data];");
                        }
                        else
                        {
                            sb.AppendLine(padding + "\tNSData * " + GetPropertyName(prop.Name) + "Data = [reader remainingData];");
                            sb.AppendLine(padding + "\t" + GetPropertyName(prop.Name) + " [" + GetLastPartNameFromDottedType(EmitType(prop.Type)).Trim(' ', '*') + " parseFromData:" + GetPropertyName(prop.Name) + "Data];");
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

                    string call;
                    if (!String.IsNullOrEmpty(prop.FlagsOpt))
                    {
                        call = "[reader readDataOfLength:" + prop.FlagsOpt + "]";
                    }
                    else
                    {
                        call = "[reader read" + readerTypeMap[typestr] + "]";
                    }

                    if (prop.Flags == "protomask")
                    {
                        if (prop.Default.FirstOrDefault() is StrongSymbol && (prop.Default.FirstOrDefault() as StrongSymbol).Class.Name == "EGCMsg")
                            call = "[_SKMsgUtil getGCMsg:(uint32_t)" + call + "]";
                        else
                            call = "[_SKMsgUtil getMsg:(uint32_t)" + call + "]";
                    }

                    sb.AppendLine(padding + "\tself." + GetPropertyName(symname) + " = " + typecast + call + ";");
                }
            }


            sb.AppendLine(padding + "}");
        }
    }
}
