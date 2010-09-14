using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using google.protobuf;
using ProtoBuf;
using System.IO;
using System.Diagnostics;

namespace protbuf_deserialize
{
    class Program
    {
        public static String GetLabel(FieldDescriptorProto.Label label)
        {
            switch (label)
            {
                default:
                case FieldDescriptorProto.Label.LABEL_OPTIONAL:
                    return "optional";
                case FieldDescriptorProto.Label.LABEL_REQUIRED:
                    return "required";
                case FieldDescriptorProto.Label.LABEL_REPEATED:
                    return "repeated";
            }
        }

        public static String GetType(FieldDescriptorProto.Type type)
        {
            switch (type)
            {
                default:
                    return type.ToString();
                case FieldDescriptorProto.Type.TYPE_INT32:
                    return "int32";
                case FieldDescriptorProto.Type.TYPE_INT64:
                    return "int64";
                case FieldDescriptorProto.Type.TYPE_SINT32:
                    return "sint32";
                case FieldDescriptorProto.Type.TYPE_SINT64:
                    return "sint64";
                case FieldDescriptorProto.Type.TYPE_UINT32:
                    return "uint32";
                case FieldDescriptorProto.Type.TYPE_UINT64:
                    return "uint64";
                case FieldDescriptorProto.Type.TYPE_STRING:
                    return "string";
                case FieldDescriptorProto.Type.TYPE_BOOL:
                    return "bool";
                case FieldDescriptorProto.Type.TYPE_BYTES:
                    return "bytes";
                case FieldDescriptorProto.Type.TYPE_DOUBLE:
                    return "double";
                case FieldDescriptorProto.Type.TYPE_ENUM:
                    return "enum";
                case FieldDescriptorProto.Type.TYPE_FLOAT:
                    return "float";
                case FieldDescriptorProto.Type.TYPE_GROUP:
                    return "GROUP";
                case FieldDescriptorProto.Type.TYPE_MESSAGE:
                    return "message";
                case FieldDescriptorProto.Type.TYPE_FIXED32:
                    return "fixed32";
                case FieldDescriptorProto.Type.TYPE_FIXED64:
                    return "fixed64";
            }
        }

        public static void DumpDescriptor(DescriptorProto proto, StringBuilder sb, int level )
        {
            string levelspace = new String('\t', level);

            sb.AppendLine(levelspace + "message " + proto.name + " {");

            foreach (DescriptorProto field in proto.nested_type)
            {
                DumpDescriptor(field, sb, level + 1 );
            }

            foreach (FieldDescriptorProto field in proto.extension)
            {
                sb.AppendLine(levelspace + "\t" + GetLabel(field.label) + " " + GetType(field.type) + " " + field.name);
            }

            foreach (EnumDescriptorProto field in proto.enum_type)
            {
                sb.AppendLine(levelspace + "\tENUM " + field.name);
            }

            foreach (FieldDescriptorProto field in proto.field)
            {
                string type = GetType(field.type);

                if (type.Equals("message"))
                {
                    type = field.type_name;
                }

                string parameters = "";

                if (!String.IsNullOrWhiteSpace(field.default_value))
                {
                    parameters += " [default = " + field.default_value + "]";
                }

                sb.AppendLine(levelspace + "\t" + GetLabel(field.label) + " " + type  + " " + field.name + " = " + field.number + parameters + ";");
            }


            sb.AppendLine(levelspace + "}");
            sb.AppendLine();
        }

        static void Main(string[] args)
        {
            FileDescriptorProto set;
            using (var file = File.OpenRead(@"C:\steamre\steamstruct.bin"))
            {
                set = Serializer.Deserialize<FileDescriptorProto>(file);

                StringBuilder sb = new StringBuilder();

                foreach (string dependency in set.dependency)
                {
                    sb.AppendLine("import \"" + dependency + "\";");
                }

                if (set.dependency.Count > 0)
                {
                    sb.AppendLine();
                }
                
                foreach( DescriptorProto proto in set.message_type )
                {
                    DumpDescriptor(proto, sb, 0);
                }

                File.WriteAllText(@"C:\steamre\" + set.name, sb.ToString());

            }
        }
    }
}
