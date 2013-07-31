using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamLanguageParser
{
    class ObjCInterfaceGen : ObjCGenBase
    {
        public override void EmitNamespace(StringBuilder sb, bool end, string nspace)
        {
            base.EmitNamespace(sb, end, nspace);
            if (!end)
            {
                if (nspace.EndsWith("Internal"))
                {
                    sb.AppendLine("#import \"SteamLanguage.h\"");
                    sb.AppendLine("#import <CRBoilerplate/CRBoilerplate.h>");
                }
                else
                {
                    sb.AppendLine("#import \"steammessages_base.pb.h\"");
                    sb.AppendLine("#import \"steammessages_clientserver.pb.h\"");
                }

                sb.AppendLine();
                sb.AppendLine("#ifndef _SK_EGCMSG");
                sb.AppendLine("#define _SK_EGCMSG");
                sb.AppendLine("typedef uint32_t EGCMsg; // Temp hack, not yet defined");
                sb.AppendLine("#endif");
                sb.AppendLine("@class CRDataReader; // Temp hack, not yet defined");
                sb.AppendLine();
            }
        }

        public override void EmitSerialBase(StringBuilder sb, int level, bool supportsGC)
        {
            string padding = new String('\t', level);


            sb.AppendLine(padding + "@protocol _SKSteamSerializable <NSObject>");
            sb.AppendLine();
            sb.AppendLine(padding + "- (void) serialize:(NSMutableData *)data;");
            sb.AppendLine(padding + "- (void) deserializeWithReader:(CRDataReader *)reader;");
            sb.AppendLine();
            sb.AppendLine(padding + "@end");
            sb.AppendLine();
            sb.AppendLine(padding + "@protocol _SKSteamSerializableHeader <_SKSteamSerializable>");
            sb.AppendLine();
            sb.AppendLine(padding + "- (void) setEMsg:(EMsg)msg;");
            sb.AppendLine();
            sb.AppendLine(padding + "@end");
            sb.AppendLine();
            sb.AppendLine(padding + "@protocol _SKSteamSerializableMessage <_SKSteamSerializable>");
            sb.AppendLine();
            sb.AppendLine(padding + "- (EMsg) eMsg;");
            sb.AppendLine();
            sb.AppendLine(padding + "@end");

            sb.AppendLine();
            sb.AppendLine(padding + "@protocol _SKGCSerializableHeader <_SKSteamSerializable>");
            sb.AppendLine();
            sb.AppendLine(padding + "- (void) setEMsg:(EGCMsg)msg;");
            sb.AppendLine();
            sb.AppendLine(padding + "@end");
            sb.AppendLine();
            sb.AppendLine(padding + "@protocol _SKGCSerializableMessage <_SKSteamSerializable>");
            sb.AppendLine();
            sb.AppendLine(padding + "- (EGCMsg) eMsg;");
            sb.AppendLine();
            sb.AppendLine(padding + "@end");

            sb.AppendLine();
        }

        public string GetUpperName(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Remove(0, 1);
        }

        protected override void EmitEnumNode(EnumNode enode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            if (enode.Flags == "flags")
            {
                sb.AppendLine(padding + "typedef NS_OPTIONS(NSUInteger, " + enode.Name + ") {");
            }
            else
            {
                sb.AppendLine(padding + "typedef NS_ENUM(NSUInteger, " + enode.Name + ") {");
            }

            string lastValue = "0";

            bool hasMax = false;

            foreach (PropNode prop in enode.childNodes)
            {
                lastValue = EmitType(prop.Default.FirstOrDefault());
                sb.AppendLine(padding + "\t" + enode.Name + prop.Name + " = " + lastValue + ",");
                if (prop.Name.Equals("Max", StringComparison.OrdinalIgnoreCase))
                {
                    hasMax = true;
                }
            }

            long maxlong = 0;

            if (lastValue.StartsWith("0x"))
                maxlong = Convert.ToInt64(lastValue.Substring(2, lastValue.Length - 2), 16);
            else
                maxlong = Int64.Parse(lastValue);

            maxlong++;

            if (!hasMax && enode.Flags != "flags")
            {
                sb.AppendLine(padding + "\t" + enode.Name + "Max = " + maxlong + ",");
            }
            sb.AppendLine(padding + "};");
            sb.AppendLine();
        }

        protected override void EmitClassNode(ClassNode cnode, StringBuilder sb, int level)
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
                sb.AppendLine(padding + "@end");
                sb.AppendLine();
                return;
            }

            string parent = "_SKSteamSerializable";

            if (cnode.Ident != null)
            {
                if (cnode.Name.Contains("MsgGC"))
                {
                    parent = "_SKGCSerializableMessage";
                }
                else
                {
                    parent = "_SKSteamSerializableMessage";
                }
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                if (cnode.Name.Contains("MsgGC"))
                    parent = "_SKGCSerializableHeader";
                else
                    parent = "_SKSteamSerializableHeader";
            }

            if (cnode.Name.Contains("Hdr"))
            {
                sb.AppendLine(padding + "//[StructLayout( LayoutKind.Sequential )]");
            }

            sb.AppendLine(padding + "@interface _SK" + cnode.Name + " : NSObject <" + parent + ">");
            sb.AppendLine();
        }

        private void EmitClassIdentity(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);

            if (cnode.Ident != null)
            {
                if (cnode.Name.Contains("MsgGC"))
                {
                    sb.AppendLine(padding + "- (EGCMsg) eMsg;");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(padding + "- (EMsg) eMsg;");
                    sb.AppendLine();
                }
            }
            else if (cnode.Name.Contains("Hdr"))
            {
                if (cnode.Name.Contains("MsgGC"))
                {
                    sb.AppendLine(padding + "- (void) setEMsg:(EGCMsg)msg;");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(padding + "- (void) setEMsg:(EMsg)msg;");
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
                sb.AppendLine(padding + "@property (nonatomic, readwrite) " + EmitType(cnode.Parent) + " header;");
            }

            foreach (PropNode prop in cnode.childNodes)
            {
                string typestr = EmitType(prop.Type);
                string propName = GetPropertyName(prop.Name);

                if (prop.Flags != null && prop.Flags == "const")
                {
                    sb.AppendLine(padding + "+ (" + typestr + ") " + GetUpperName(prop.Name) + ";");
                    continue;
                }

                int size = CodeGenerator.GetTypeSize(prop);
                baseClassSize += size;

                sb.AppendLine(padding + "// Static size: " + size);

                if (prop.Flags != null && prop.Flags == "steamidmarshal" && typestr == "ulong")
                {
                    sb.AppendLine(padding + "@property (nonatomic, readwrite) SKSteamID * " + propName + ";");
                }
                else if (prop.Flags != null && prop.Flags == "boolmarshal" && typestr == "byte")
                {
                    sb.AppendLine(padding + "@property (nonatomic, readwrite) bool " + propName + ";");
                }
                else if (prop.Flags != null && prop.Flags == "gameidmarshal" && typestr == "ulong")
                {
                    sb.AppendLine(padding + "@property (nonatomic, readwrite) SKGameID * " + propName + ";");
                }
                else if (prop.Flags != null && prop.Flags == "proto")
                {
                    sb.AppendLine(padding + "@property (nonatomic, strong, readwrite) " + GetLastPartNameFromDottedType(typestr) + " * " + propName + ";");
                }
                else
                {
                    int temp;
                    if (!String.IsNullOrEmpty(prop.FlagsOpt) && Int32.TryParse(prop.FlagsOpt, out temp))
                    {
                        typestr = "NSData *";
                    }

                    if (typestr.StartsWith("NS"))
                    {
                        sb.AppendLine(padding + "@property (nonatomic, strong, readwrite) " + typestr + " " + propName + ";");
                    }
                    else
                    {
                        sb.AppendLine(padding + "@property (nonatomic, readwrite) " + typestr + " " + propName + ";");
                    }
                }
            }

            sb.AppendLine();

            return baseClassSize;
        }

        private void EmitClassConstructor(ClassNode cnode, StringBuilder sb, int level)
        {
            string padding = new String('\t', level);
            sb.AppendLine(padding + "- (id) init;");
        }

        private void EmitClassSerializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "- (void) serialize:(NSMutableData *)data;");
        }

        private void EmitClassSize(ClassNode cnode, StringBuilder sb, int level)
        {
        }

        private void EmitClassDeserializer(ClassNode cnode, StringBuilder sb, int level, int baseSize)
        {
            string padding = new String('\t', level);

            sb.AppendLine();
            sb.AppendLine(padding + "- (void) deserializeWithReader:(CRDataReader *)reader;");
        }
    }
}
