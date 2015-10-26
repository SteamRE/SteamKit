using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using google.protobuf;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ProtobufDumper
{
    [AttributeUsage(AttributeTargets.All)]
    public class EnumProxyAttribute : Attribute
    {
        public EnumProxyAttribute(object defaultValue, string type)
        {
            this.DefaultValue = defaultValue;
            this.EnumType = type;
        }

        public object DefaultValue;
        public String EnumType;
    }

    class ImageFile
    {
        private static ModuleBuilder moduleBuilder;

        static ImageFile()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ProtobufDumper"), AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("JIT", true);
        }

        public string OutputDir { get; private set; }
        public List<string> ProtoList { get; private set; }

        struct ProtoData
        {
            public FileDescriptorProto file;
            public StringBuilder buffer;
        }

        private string FileName;

        private List<ProtoData> FinalProtoDefinition; 

        private List<FileDescriptorProto> deferredProtos;

        private Dictionary<string, List<Type>> protobufExtensions;

        private Stack<string> messageNameStack;
        private Dictionary<string, EnumDescriptorProto> enumLookup;
        private Dictionary<string, int> enumLookupCount;
        private List<string> deferredEnumTokens;

        private Regex ProtoFileNameRegex;

        public ImageFile(string fileName, string output = null)
        {
            this.FileName = fileName;
            this.OutputDir = output ?? Path.GetFileNameWithoutExtension(fileName);
            this.ProtoList = new List<string>();

            this.FinalProtoDefinition = new List<ProtoData>();

            this.deferredProtos = new List<FileDescriptorProto>();

            this.protobufExtensions = new Dictionary<string, List<Type>>();

            this.messageNameStack = new Stack<string>();
            this.enumLookup = new Dictionary<string, EnumDescriptorProto>();
            this.enumLookupCount = new Dictionary<string, int>();
            this.deferredEnumTokens = new List<string>();

            this.ProtoFileNameRegex = new Regex(@"^[a-zA-Z_0-9\\/.]+\.proto$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        }

        public void Process()
        {
            Console.WriteLine("Loading binary '{0}'...", FileName);

            var rawData = File.ReadAllBytes(FileName);

            using (var ms = new MemoryStream(rawData))
            {
                ScanFile(ms);
            }

            var safeguard = 10;

            while (deferredProtos.Count > 0 && safeguard-- > 0)
            {
                foreach (var proto in deferredProtos.ToList())
                {
                    if (safeguard == 0 || !ShouldDeferProto(proto))
                    {
                        DoParseFile(proto);

                        deferredProtos.Remove(proto);
                    }
                }
            }

            FinalPassWriteFiles();
        }

        void ScanFile(Stream stream)
        {
            int characterSize = Encoding.ASCII.GetByteCount("e");
            const char marker = '\n';

            while (stream.Position < stream.Length)
            {
                var currentByte = stream.ReadByte();

                if (currentByte == marker)
                {
                    bool nullSkip = false;
                    byte[] data = null;

                continueScanning:
                    stream.Position--; // Return back to the marker we just read

                    using (var ms = new MemoryStream())
                    {
                        if (data != null)
                        {
                            ms.Write(data, 0, data.Length);
                        }

                        while (true)
                        {
                            var data2 = new byte[characterSize];
                            stream.Read(data2, 0, characterSize);

                            if (Encoding.ASCII.GetString(data2, 0, characterSize) == "\0")
                            {
                                if (!nullSkip)
                                {
                                    break;
                                }

                                nullSkip = false;
                            }

                            ms.Write(data2, 0, data2.Length);
                        }

                        data = ms.ToArray();
                    }

                    if (data.Length < 2)
                    {
                        continue;
                    }

                    byte strLen = data[1];

                    if (data.Length - 2 < strLen)
                    {
                        continue;
                    }

                    string protoName = Encoding.ASCII.GetString(data, 2, strLen);

                    if (!ProtoFileNameRegex.IsMatch(protoName))
                    {
                        continue;
                    }

                    if (deferredProtos.Any(x => x.name == protoName))
                    {
                        continue;
                    }

                    if (!HandleProto(protoName, data))
                    {
                        nullSkip = true;

                        goto continueScanning;
                    }
                }
            }
        }

        private void PushDescriptorName(FileDescriptorProto file)
        {
            messageNameStack.Push(file.package);
        }

        private void PushDescriptorName(DescriptorProto proto)
        {
            messageNameStack.Push(proto.name);
        }

        private void PushDescriptorName(FieldDescriptorProto field)
        {
            messageNameStack.Push(field.name);
        }

        private string GetDescriptorName(string descriptor)
        {
            if (descriptor[0] == '.')
            {
                return descriptor.Substring(1);
            }

            messageNameStack.Push(descriptor);
            string messageName = String.Join(".", messageNameStack.ToArray().Reverse());
            messageNameStack.Pop();

            return messageName;
        }

        private void AddEnumDescriptorLookup(EnumDescriptorProto enumdesc)
        {
            enumLookup[GetDescriptorName(enumdesc.name)] = enumdesc;
            enumLookupCount[GetDescriptorName(enumdesc.name)] = enumdesc.value.Count;
        }

        private void PopDescriptorName()
        {
            messageNameStack.Pop();
        }

        private static string GetEnumDescriptorTokenDefault(string messageName)
        {
            return String.Format("${0}_DEFAULT_VALUE$", messageName);
        }

        private static string GetEnumDescriptorTokenAt(string messageName, int index)
        {
            return String.Format("${0}_{1}_VALUE$", messageName, index);
        }

        private string ResolveOrDeferEnumDefaultValue(string type)
        {
            EnumDescriptorProto proto;
            string descName = GetDescriptorName(type);

            if (enumLookup.TryGetValue(descName, out proto))
                return proto.value[0].name;
            else
            {
                string absType = '.' + descName;

                if(!deferredEnumTokens.Contains(absType))
                    deferredEnumTokens.Add(absType);

                return GetEnumDescriptorTokenDefault(absType);
            }
        }

        private string ResolveOrDeferEnumValueAt(string type, int index)
        {
            EnumDescriptorProto proto;
            string descName = GetDescriptorName(type);

            if (enumLookup.TryGetValue(descName, out proto))
                return proto.value[index].name;
            else
            {
                string absType = '.' + descName;

                if (!deferredEnumTokens.Contains(absType))
                    deferredEnumTokens.Add(absType);

                return GetEnumDescriptorTokenAt(absType, index);
            }
        }
 
        private bool HandleProto(string name, byte[] data)
        {
            Console.Write("{0}... ", name);

            FileDescriptorProto set = null;

            if (Environment.GetCommandLineArgs().Contains("-dump", StringComparer.OrdinalIgnoreCase))
            {
                string fileName = Path.Combine(OutputDir, name + ".dump");
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                Console.WriteLine("  ! Dumping to '{0}'!", fileName);

                try
                {
                    File.WriteAllBytes(fileName, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to dump: {0}", ex.Message);
                }
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                    set = Serializer.Deserialize<FileDescriptorProto>(ms);
            }
            catch (EndOfStreamException ex)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("needs rescan: {0}", ex.Message);
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("is invalid: {0}", ex.Message);
                Console.ResetColor();
                return true;
            }

            deferredProtos.Add(set);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK!");
            Console.ResetColor();

            return true;
        }

        private bool ShouldDeferProto(FileDescriptorProto set)
        {
            bool defer = false;
            foreach (string dependency in set.dependency)
            {
                if (!dependency.StartsWith("google", StringComparison.Ordinal) && !ProtoList.Contains(dependency))
                {
                    defer = true;
                    break;
                }
            }

            return defer;
        }

        private void DoParseFile(FileDescriptorProto set)
        {
            StringBuilder sb = new StringBuilder();

            DumpFileDescriptor(set, sb);

            FinalProtoDefinition.Add(new ProtoData { file = set, buffer = sb });
            ProtoList.Add(set.name);
        }

        private void FinalPassWriteFiles()
        {
            Console.WriteLine();

            foreach (var proto in FinalProtoDefinition)
            {
                Directory.CreateDirectory(OutputDir);
                string outputFile = Path.Combine(OutputDir, proto.file.name);

                Console.WriteLine("  ! Outputting proto to '{0}'", outputFile);
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                foreach(string type in deferredEnumTokens)
                {
                    proto.buffer.Replace(GetEnumDescriptorTokenDefault(type), ResolveOrDeferEnumDefaultValue(type));

                    for (int i = 0; i < enumLookupCount[GetDescriptorName(type)]; i++)
                    {
                        proto.buffer.Replace(GetEnumDescriptorTokenAt(type, i), ResolveOrDeferEnumValueAt(type, i));
                    }
                }

                File.WriteAllText(outputFile, proto.buffer.ToString());
            }
        }

        private static String GetLabel(FieldDescriptorProto.Label label)
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

        private static String GetType(FieldDescriptorProto.Type type)
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
                case FieldDescriptorProto.Type.TYPE_SFIXED32:
                    return "sfixed32";
                case FieldDescriptorProto.Type.TYPE_SFIXED64:
                    return "sfixed64";
            }
        }

        public static Type LookupBasicType(FieldDescriptorProto.Type type, out DataFormat format, out bool buildEnumProxy)
        {
            buildEnumProxy = false;
            format = DataFormat.Default;
            switch (type)
            {
                default:
                    return null;
                case FieldDescriptorProto.Type.TYPE_INT32:
                    return typeof(Int32);
                case FieldDescriptorProto.Type.TYPE_INT64:
                    return typeof(Int64);
                case FieldDescriptorProto.Type.TYPE_SINT32:
                    return typeof(Int32);
                case FieldDescriptorProto.Type.TYPE_SINT64:
                    return typeof(Int64);
                case FieldDescriptorProto.Type.TYPE_UINT32:
                    return typeof(UInt32);
                case FieldDescriptorProto.Type.TYPE_UINT64:
                    return typeof(UInt64);
                case FieldDescriptorProto.Type.TYPE_STRING:
                    return typeof(String);
                case FieldDescriptorProto.Type.TYPE_BOOL:
                    return typeof(Boolean);
                case FieldDescriptorProto.Type.TYPE_BYTES:
                    return typeof(byte[]);
                case FieldDescriptorProto.Type.TYPE_DOUBLE:
                    return typeof(Double);
                case FieldDescriptorProto.Type.TYPE_FLOAT:
                    return typeof(float);
                case FieldDescriptorProto.Type.TYPE_MESSAGE:
                    return typeof(string);
                case FieldDescriptorProto.Type.TYPE_FIXED32:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(Int32);
                    }
                case FieldDescriptorProto.Type.TYPE_FIXED64:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(Int64);
                    }
                case FieldDescriptorProto.Type.TYPE_SFIXED32:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(Int32);
                    }
                case FieldDescriptorProto.Type.TYPE_SFIXED64:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(Int64);
                    }
                case FieldDescriptorProto.Type.TYPE_ENUM:
                    {
                        buildEnumProxy = true;
                        return typeof(Int32);
                    }
            }
        }

        private String ResolveType(FieldDescriptorProto field)
        {
            if (field.type == FieldDescriptorProto.Type.TYPE_MESSAGE)
            {
                return field.type_name;
            }
            else if (field.type == FieldDescriptorProto.Type.TYPE_ENUM)
            {
                return field.type_name;
            }

            return GetType(field.type);
        }

        private string GetValueForProp(dynamic propInfo, object options, bool field, out string name)
        {
            ProtoMemberAttribute[] protoMember = (ProtoMemberAttribute[])propInfo.GetCustomAttributes(typeof(ProtoMemberAttribute), false);
            DefaultValueAttribute[] defaultValueList = (DefaultValueAttribute[])propInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            EnumProxyAttribute[] enumProxyList = (EnumProxyAttribute[])propInfo.GetCustomAttributes(typeof(EnumProxyAttribute), false);

            name = null;

            if (protoMember.Length == 0)
                return null;

            name = protoMember[0].Name;
            object value = null;

            try
            {
                if(field)
                    value = propInfo.GetValue(options);
                else
                    value = propInfo.GetValue(options, null);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to dump option '{0}': {1}", name, ex.Message);
                Console.ResetColor();
                return null;
            }

            if (value == null)
                return null;

            if (defaultValueList.Length > 0)
            {
                object defValue = defaultValueList[0].Value;
                if (defValue != null && defValue.Equals(value))
                    return null;
            }

            if (enumProxyList.Length > 0)
            {
                EnumProxyAttribute enumProxy = enumProxyList[0];
                value = ResolveOrDeferEnumValueAt(enumProxy.EnumType, (int)value - 1);
            }
            else if (value is string)
            {
                if (string.IsNullOrEmpty((string)value))
                    return null;

                value = string.Format("\"{0}\"", value);
            }
            else if (value is bool)
            {
                value = value.ToString().ToLower();
            }
            else if (value is IEnumerable)
            {
                var enumerableValue = ((IEnumerable)value).Cast<object>();
                if (!enumerableValue.Any())
                {
                    return null;
                }
            }

            return Convert.ToString(value);
        }

        private Dictionary<string, string> DumpOptions(dynamic options)
        {
            Dictionary<string, string> options_kv = new Dictionary<string, string>();

            if (options == null)
                return options_kv;

            var optionsType = options.GetType();

            // generate precompiled options
            var propertySearchBindingFlags = BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo propInfo in optionsType.GetProperties(propertySearchBindingFlags))
            {
                var propertySpecifiedSuffix = "Specified";
                var propName = propInfo.Name;
                if (propName.EndsWith(propertySpecifiedSuffix))
                {
                    continue;
                }

                var specifiedProp = optionsType.GetProperty(propName + propertySpecifiedSuffix, propertySearchBindingFlags);
                if (specifiedProp != null)
                {
                    var isSpecified = specifiedProp.GetValue(options);
                    if (!isSpecified)
                    {
                        continue;
                    }
                }

                string name;
                string value = GetValueForProp(propInfo, options, false, out name); 

                if(value != null)
                    options_kv.Add(name, value);
            }

            // generate reflected options (extensions to this type)
            IExtension extend = ((IExtensible)options).GetExtensionObject(false);
            List<Type> extensions = new List<Type>();

            if (extend != null && protobufExtensions.TryGetValue(options.GetType().FullName, out extensions))
            {
                foreach (var extension in extensions)
                {
                    Stream ms = extend.BeginQuery();

                    object deserialized = RuntimeTypeModel.Default.Deserialize(ms, null, extension);

                    foreach (var fieldInfo in extension.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        string name;
                        string value = GetValueForProp(fieldInfo, deserialized, true, out name);

                        if (value != null)
                            options_kv.Add(name, value);
                    }

                    extend.EndQuery(ms);
                }
            }

            return options_kv;
        }

        private string BuildDescriptorDeclaration(FieldDescriptorProto field, bool emitFieldLabel = true)
        {
            PushDescriptorName(field);

            string type = ResolveType(field);
            Dictionary<string, string> options = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(field.default_value))
            {
                string default_value = field.default_value;

                if (field.type == FieldDescriptorProto.Type.TYPE_STRING)
                    default_value = String.Format("\"{0}\"", default_value);

                options.Add("default", default_value);
            }
            else if (field.type == FieldDescriptorProto.Type.TYPE_ENUM && field.label != FieldDescriptorProto.Label.LABEL_REPEATED)
            {
                options.Add("default", ResolveOrDeferEnumDefaultValue(type));
            }

            Dictionary<string, string> fieldOptions = DumpOptions(field.options);
            foreach (var pair in fieldOptions)
            {
                options[pair.Key] = pair.Value;
            }

            string parameters = String.Empty;
            if (options.Count > 0)
            {
                parameters = " [" + String.Join(", ", options.Select(kvp => String.Format("{0} = {1}", kvp.Key, kvp.Value))) + "]";
            }

            PopDescriptorName();

            var descriptorDeclarationBuilder = new StringBuilder();
            if (emitFieldLabel)
            {
                descriptorDeclarationBuilder.Append(GetLabel(field.label));
                descriptorDeclarationBuilder.Append(" ");
            }

            descriptorDeclarationBuilder.AppendFormat("{0} {1} = {2}{3};", type, field.name, field.number, parameters);

            return descriptorDeclarationBuilder.ToString();
        }

        private void DumpExtensionDescriptor(List<FieldDescriptorProto> fields, StringBuilder sb, string levelspace)
        {
            foreach (var mapping in fields.GroupBy(x => { return x.extendee; }))
            {
                if (String.IsNullOrEmpty(mapping.Key))
                    throw new Exception("Empty extendee in extension, this should not be possible");

                if (mapping.Key.StartsWith(".google.protobuf", StringComparison.Ordinal))
                {
                    BuildExtension(mapping.Key.Substring(1), mapping.ToArray());
                }

                sb.AppendLine(levelspace + "extend " + mapping.Key + " {");

                foreach (FieldDescriptorProto field in mapping)
                {
                    sb.AppendLine(levelspace + "\t" + BuildDescriptorDeclaration(field));
                }

                sb.AppendLine(levelspace + "}");
                sb.AppendLine();
            }
        }

        private void DumpFileDescriptor(FileDescriptorProto set, StringBuilder sb)
        {
            if(!String.IsNullOrEmpty(set.package))
                PushDescriptorName(set);

            bool marker = false;

            foreach (string dependency in set.dependency)
            {
                sb.AppendLine("import \"" + dependency + "\";");
                marker = true;
            }

            if (marker)
            {
                sb.AppendLine();
                marker = false;
            }

            if (!string.IsNullOrEmpty(set.package))
            {
                sb.AppendLine("package " + set.package + ";");
                marker = true;
            }

            if (marker)
            {
                sb.AppendLine();
                marker = false;
            }

            foreach (var option in DumpOptions(set.options))
            {
                sb.AppendLine("option " + option.Key + " = " + option.Value + ";");
                marker = true;
            }

            if (marker)
            {
                sb.AppendLine();
                marker = false;
            }

            DumpExtensionDescriptor(set.extension, sb, String.Empty);

            foreach (EnumDescriptorProto field in set.enum_type)
            {
                DumpEnumDescriptor(field, sb, 0);
            }

            foreach (DescriptorProto proto in set.message_type)
            {
                DumpDescriptor(proto, set, sb, 0);
            }

            foreach (ServiceDescriptorProto service in set.service)
            {
                sb.AppendLine("service " + service.name + " {");

                foreach (var option in DumpOptions(service.options))
                {
                    sb.AppendLine("\toption " + option.Key + " = " + option.Value + ";");
                }

                foreach (MethodDescriptorProto method in service.method)
                {
                    string declaration = "\trpc " + method.name + " (" + method.input_type + ") returns (" + method.output_type + ")";

                    Dictionary<string, string> options = DumpOptions(method.options);

                    string parameters = String.Empty;
                    if (options.Count == 0)
                    {
                        sb.AppendLine(declaration + ";");
                    }
                    else
                    {
                        sb.AppendLine(declaration + " {");

                        foreach (var option in options)
                        {
                            sb.AppendLine("\t\toption " + option.Key + " = " + option.Value + ";");
                        }

                        sb.AppendLine("\t}");
                    }
                }

                sb.AppendLine("}");
            }

            if (!String.IsNullOrEmpty(set.package))
                PopDescriptorName();
        }

        private void DumpDescriptor(DescriptorProto proto, FileDescriptorProto set, StringBuilder sb, int level)
        {
            PushDescriptorName(proto);

            string levelspace = new String('\t', level);

            sb.AppendLine(levelspace + "message " + proto.name + " {");

            foreach (var option in DumpOptions(proto.options))
            {
                sb.AppendLine(levelspace + "\toption " + option.Key + " = " + option.Value + ";");
            }

            foreach (DescriptorProto field in proto.nested_type)
            {
                DumpDescriptor(field, set, sb, level + 1);
            }

            DumpExtensionDescriptor(proto.extension, sb, levelspace + '\t');

            foreach (EnumDescriptorProto field in proto.enum_type)
            {
                DumpEnumDescriptor(field, sb, level + 1);
            }

            foreach (FieldDescriptorProto field in proto.field.Where(x => !x.oneof_indexSpecified))
            {
                var enumLookup = new List<EnumDescriptorProto>();

                enumLookup.AddRange( set.enum_type ); // add global enums
                enumLookup.AddRange( proto.enum_type ); // add this message's nested enums

                sb.AppendLine(levelspace + "\t" + BuildDescriptorDeclaration(field));
            }

            for (int i = 0; i < proto.oneof_decl.Count; i++)
            {
                var oneof = proto.oneof_decl[i];
                var fields = proto.field.Where(x => x.oneof_indexSpecified && x.oneof_index == i).ToArray();

                sb.AppendLine(levelspace + "\toneof " + oneof.name + " {");

                foreach(var field in fields)
                {
                    sb.AppendLine(levelspace + "\t\t" + BuildDescriptorDeclaration(field, emitFieldLabel: false));
                }

                sb.AppendLine(levelspace + "\t}");
            }

            if (proto.extension_range.Count > 0)
                sb.AppendLine();

            foreach (DescriptorProto.ExtensionRange range in proto.extension_range)
            {
                string max = Convert.ToString( range.end );

                // http://code.google.com/apis/protocolbuffers/docs/proto.html#extensions
                // If your numbering convention might involve extensions having very large numbers as tags, you can specify
                // that your extension range goes up to the maximum possible field number using the max keyword:
                // max is 2^29 - 1, or 536,870,911. 
                if ( range.end >= 536870911 )
                {
                    max = "max";
                }

                sb.AppendLine(levelspace + "\textensions " + range.start + " to " + max + ";");
            }

            sb.AppendLine(levelspace + "}");
            sb.AppendLine();

            PopDescriptorName();
        }

        private void DumpEnumDescriptor(EnumDescriptorProto field, StringBuilder sb, int level)
        {
            AddEnumDescriptorLookup(field);

            string levelspace = new String('\t', level);

            sb.AppendLine(levelspace + "enum " + field.name + " {");

            foreach (var option in DumpOptions(field.options))
            {
                sb.AppendLine(levelspace + "\toption " + option.Key + " = " + option.Value + ";");
            }

            foreach (EnumValueDescriptorProto enumValue in field.value)
            {
                Dictionary<string, string> options = DumpOptions(enumValue.options);

                string parameters = String.Empty;
                if (options.Count > 0)
                {
                    parameters = " [" + String.Join(", ", options.Select(kvp => String.Format("{0} = {1}", kvp.Key, kvp.Value))) + "]";
                }

                sb.AppendLine(levelspace + "\t" + enumValue.name + " = " + enumValue.number + parameters + ";");
            }

            sb.AppendLine(levelspace + "}");
            sb.AppendLine();
        }

        // because of protobuf-net we're limited to parsing simple types at run-time as we can't parse the protobuf, but options shouldn't be too complex
        private void BuildExtension(string key, FieldDescriptorProto[] fields)
        {
            string name = key + "Ext" + Guid.NewGuid().ToString();
            TypeBuilder extension = moduleBuilder.DefineType(name, TypeAttributes.Class);

            Type pcType = typeof(ProtoContractAttribute);
            ConstructorInfo pcCtor = pcType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder pcBuilder = new CustomAttributeBuilder(pcCtor,Type.EmptyTypes);

            extension.SetCustomAttribute(pcBuilder);

            foreach (var field in fields)
            {
                DataFormat format;
                bool buildEnumProxy;
                Type fieldType = ImageFile.LookupBasicType(field.type, out format, out buildEnumProxy);

                FieldBuilder fbuilder = extension.DefineField(field.name, fieldType, FieldAttributes.Public);

                object defaultValue = field.default_value;
                if (field.type == FieldDescriptorProto.Type.TYPE_ENUM)
                {
                    defaultValue = 0;
                }
                else if (String.IsNullOrEmpty(field.default_value))
                {
                    if (field.type == FieldDescriptorProto.Type.TYPE_STRING)
                        defaultValue = "";
                    else
                        defaultValue = Activator.CreateInstance(fieldType);
                }
                else
                {
                    try
                    {
                        defaultValue = Convert.ChangeType(field.default_value, fieldType);
                    }
                    catch (FormatException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Constructor for extension had bad format: {0}", key);
                        Console.ResetColor();
                        return;
                    }
                }


                if (buildEnumProxy)
                {
                    Type epType = typeof(EnumProxyAttribute);
                    ConstructorInfo epCtor = epType.GetConstructor(new Type[] { typeof(object), typeof(string) });
                    CustomAttributeBuilder epBuilder = new CustomAttributeBuilder(epCtor, new object[] { field.default_value, field.type_name });

                    fbuilder.SetCustomAttribute(epBuilder);
                }

                Type dvType = typeof(DefaultValueAttribute);
                ConstructorInfo dvCtor = dvType.GetConstructor(new Type[] { typeof(object) });
                CustomAttributeBuilder dvBuilder = new CustomAttributeBuilder(dvCtor, new object[] { defaultValue });

                fbuilder.SetCustomAttribute(dvBuilder);

                Type pmType = typeof(ProtoMemberAttribute);
                ConstructorInfo pmCtor = pmType.GetConstructor(new Type[] { typeof(int) });
                CustomAttributeBuilder pmBuilder = new CustomAttributeBuilder(pmCtor, new object[] { field.number }, 
                                                        new PropertyInfo[] { pmType.GetProperty("Name"), pmType.GetProperty("DataFormat") },
                                                        new object[] {  "(" + field.name + ")", format } );


                fbuilder.SetCustomAttribute(pmBuilder);
            }

            Type extensionType = extension.CreateType();

            if (!this.protobufExtensions.ContainsKey(key))
            {
                this.protobufExtensions[key] = new List<Type>();
            }

            this.protobufExtensions[key].Add(extensionType);
        }
    }
}
