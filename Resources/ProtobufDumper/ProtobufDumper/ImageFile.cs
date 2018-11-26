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
    using ProtoExtensionDefs = Dictionary<string, List<Type>>;

    [AttributeUsage(AttributeTargets.All)]
    public class EnumProxyAttribute : Attribute
    {
        public EnumProxyAttribute(object defaultValue, string type)
        {
            DefaultValue = defaultValue;
            EnumType = type;
        }

        public object DefaultValue;
        public string EnumType;
    }

    class ImageFile
    {
        private static readonly ModuleBuilder moduleBuilder;

        static ImageFile()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ProtobufDumper"), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("JIT");
        }

        private string OutputDir { get; }
        private List<string> ProtoList { get; }

        private struct ProtoOutputData
        {
            public FileDescriptorProto file;
            public StringBuilder buffer;
        }

        private class ProtoInputData
        {
            public FileDescriptorProto proto;
            public ProtoExtensionDefs extensions = new ProtoExtensionDefs();
        }

        private readonly string FileName;

        private readonly List<ProtoOutputData> outputProtoDefs; 
        private readonly List<ProtoInputData> inputProtoDefs;

        private readonly Stack<string> messageNameStack;
        private readonly Dictionary<string, EnumDescriptorProto> enumLookup;
        private readonly Dictionary<string, int> enumLookupCount;
        private readonly List<string> deferredEnumTokens;

        private readonly Regex ProtoFileNameRegex;

        public ImageFile(string fileName, string output = null)
        {
            FileName = fileName;
            OutputDir = output ?? Path.GetFileNameWithoutExtension(fileName);
            ProtoList = new List<string>();

            outputProtoDefs = new List<ProtoOutputData>();
            inputProtoDefs = new List<ProtoInputData>();

            messageNameStack = new Stack<string>();
            enumLookup = new Dictionary<string, EnumDescriptorProto>();
            enumLookupCount = new Dictionary<string, int>();
            deferredEnumTokens = new List<string>();

            ProtoFileNameRegex = new Regex(@"^[a-zA-Z_0-9\\/.]+\.proto$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
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

            while (inputProtoDefs.Count > 0 && safeguard-- > 0)
            {
                foreach (var inputDef in inputProtoDefs.ToList())
                {
                    if (safeguard == 0 || !ShouldDeferProto(inputDef))
                    {
                        DoParseFile(inputDef);

                        inputProtoDefs.Remove(inputDef);
                    }
                }
            }

            FinalPassWriteFiles();
        }

        void ScanFile(Stream stream)
        {
            var characterSize = Encoding.ASCII.GetByteCount("e");
            const char marker = '\n';

            while (stream.Position < stream.Length)
            {
                var currentByte = stream.ReadByte();

                if (currentByte == marker)
                {
                    var nullSkip = false;
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

                    var strLen = data[1];

                    if (data.Length - 2 < strLen)
                    {
                        continue;
                    }

                    var protoName = Encoding.ASCII.GetString(data, 2, strLen);

                    if (!ProtoFileNameRegex.IsMatch(protoName))
                    {
                        continue;
                    }

                    if (inputProtoDefs.Any(x => x.proto.name == protoName))
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
            var messageName = string.Join(".", messageNameStack.ToArray().Reverse());
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
            return $"${messageName}_DEFAULT_VALUE$";
        }

        private static string GetEnumDescriptorTokenAt(string messageName, int index)
        {
            return $"${messageName}_{index}_VALUE$";
        }

        private string ResolveOrDeferEnumDefaultValue(string type)
        {
            var descName = GetDescriptorName(type);

            if (enumLookup.TryGetValue(descName, out var proto))
                return proto.value[0].name;

            var absType = '.' + descName;

            if(!deferredEnumTokens.Contains(absType))
                deferredEnumTokens.Add(absType);

            return GetEnumDescriptorTokenDefault(absType);
        }

        private string ResolveOrDeferEnumValueAt(string type, int index)
        {
            var descName = GetDescriptorName(type);

            if (enumLookup.TryGetValue(descName, out var proto))
                return proto.value[index].name;

            var absType = '.' + descName;

            if (!deferredEnumTokens.Contains(absType))
                deferredEnumTokens.Add(absType);

            return GetEnumDescriptorTokenAt(absType, index);
        }
 
        private bool HandleProto(string name, byte[] data)
        {
            Console.Write("{0}... ", name);

            var inputData = new ProtoInputData();

            if (Environment.GetCommandLineArgs().Contains("-dump", StringComparer.OrdinalIgnoreCase))
            {
                var fileName = Path.Combine(OutputDir, $"{name}.dump");
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
                using (var ms = new MemoryStream(data))
                    inputData.proto = Serializer.Deserialize<FileDescriptorProto>(ms);
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

            inputProtoDefs.Add(inputData);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK!");
            Console.ResetColor();

            return true;
        }

        private bool ShouldDeferProto(ProtoInputData inputData)
        {
            foreach (var dependency in inputData.proto.dependency)
            {
                if (!dependency.StartsWith("google", StringComparison.Ordinal) && !ProtoList.Contains(dependency))
                {
                    return true;
                }
            }

            return false;
        }

        private void DoParseFile(ProtoInputData inputData)
        {
            var sb = new StringBuilder();

            DumpFileDescriptor(inputData, sb);

            outputProtoDefs.Add(new ProtoOutputData { file = inputData.proto, buffer = sb });
            ProtoList.Add(inputData.proto.name);
        }

        private void FinalPassWriteFiles()
        {
            Console.WriteLine();

            foreach (var proto in outputProtoDefs)
            {
                Directory.CreateDirectory(OutputDir);
                var outputFile = Path.Combine(OutputDir, proto.file.name);

                Console.WriteLine("  ! Outputting proto to '{0}'", outputFile);
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                foreach(var type in deferredEnumTokens)
                {
                    proto.buffer.Replace(GetEnumDescriptorTokenDefault(type), ResolveOrDeferEnumDefaultValue(type));

                    for (var i = 0; i < enumLookupCount[GetDescriptorName(type)]; i++)
                    {
                        proto.buffer.Replace(GetEnumDescriptorTokenAt(type, i), ResolveOrDeferEnumValueAt(type, i));
                    }
                }

                File.WriteAllText(outputFile, proto.buffer.ToString());
            }
        }

        private static string GetLabel(FieldDescriptorProto.Label label)
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

        private static string GetType(FieldDescriptorProto.Type type)
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

        private static Type LookupBasicType(FieldDescriptorProto.Type type, out DataFormat format, out bool buildEnumProxy)
        {
            buildEnumProxy = false;
            format = DataFormat.Default;
            switch (type)
            {
                default:
                    return null;
                case FieldDescriptorProto.Type.TYPE_INT32:
                    return typeof(int);
                case FieldDescriptorProto.Type.TYPE_INT64:
                    return typeof(long);
                case FieldDescriptorProto.Type.TYPE_SINT32:
                    return typeof(int);
                case FieldDescriptorProto.Type.TYPE_SINT64:
                    return typeof(long);
                case FieldDescriptorProto.Type.TYPE_UINT32:
                    return typeof(uint);
                case FieldDescriptorProto.Type.TYPE_UINT64:
                    return typeof(ulong);
                case FieldDescriptorProto.Type.TYPE_STRING:
                    return typeof(string);
                case FieldDescriptorProto.Type.TYPE_BOOL:
                    return typeof(bool);
                case FieldDescriptorProto.Type.TYPE_BYTES:
                    return typeof(byte[]);
                case FieldDescriptorProto.Type.TYPE_DOUBLE:
                    return typeof(double);
                case FieldDescriptorProto.Type.TYPE_FLOAT:
                    return typeof(float);
                case FieldDescriptorProto.Type.TYPE_MESSAGE:
                    return typeof(string);
                case FieldDescriptorProto.Type.TYPE_FIXED32:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(int);
                    }
                case FieldDescriptorProto.Type.TYPE_FIXED64:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(long);
                    }
                case FieldDescriptorProto.Type.TYPE_SFIXED32:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(int);
                    }
                case FieldDescriptorProto.Type.TYPE_SFIXED64:
                    {
                        format = DataFormat.FixedSize;
                        return typeof(long);
                    }
                case FieldDescriptorProto.Type.TYPE_ENUM:
                    {
                        buildEnumProxy = true;
                        return typeof(int);
                    }
            }
        }

        private string ResolveType(FieldDescriptorProto field)
        {
            if (field.type == FieldDescriptorProto.Type.TYPE_ENUM || field.type == FieldDescriptorProto.Type.TYPE_MESSAGE)
            {
                return field.type_name;
            }
            
            return GetType(field.type);
        }

        private string GetValueForProp(dynamic propInfo, object options, bool field, out string name)
        {
            var protoMember = (ProtoMemberAttribute[])propInfo.GetCustomAttributes(typeof(ProtoMemberAttribute), false);
            var defaultValueList = (DefaultValueAttribute[])propInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            var enumProxyList = (EnumProxyAttribute[])propInfo.GetCustomAttributes(typeof(EnumProxyAttribute), false);

            name = null;

            if (protoMember.Length == 0)
                return null;

            name = protoMember[0].Name;
            object value = null;

            try
            {
                if (field)
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
                var defValue = defaultValueList[0].Value;
                if (defValue != null && defValue.Equals(value))
                    return null;
            }

            if (enumProxyList.Length > 0)
            {
                var enumProxy = enumProxyList[0];
                value = ResolveOrDeferEnumValueAt(enumProxy.EnumType, (int)value - 1);
            }
            else if (value is string)
            {
                if (string.IsNullOrEmpty((string)value))
                    return null;

                value = $"\"{value}\"";
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

        private Dictionary<string, string> DumpOptions(dynamic options, ProtoExtensionDefs protobufExtensions)
        {
            var options_kv = new Dictionary<string, string>();

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
            var extend = ((IExtensible)options).GetExtensionObject(false);
            var extensions = new List<Type>();

            if (extend != null && protobufExtensions.TryGetValue(options.GetType().FullName, out extensions))
            {
                foreach (var extension in extensions)
                {
                    var ms = extend.BeginQuery();

                    var deserialized = RuntimeTypeModel.Default.Deserialize(ms, null, extension);

                    foreach (var fieldInfo in extension.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        string name;
                        var value = GetValueForProp(fieldInfo, deserialized, true, out name);

                        if (value != null)
                            options_kv.Add(name, value);
                    }

                    extend.EndQuery(ms);
                }
            }

            return options_kv;
        }

        private string BuildDescriptorDeclaration(FieldDescriptorProto field, ProtoExtensionDefs extensions, bool emitFieldLabel = true)
        {
            PushDescriptorName(field);

            var type = ResolveType(field);
            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(field.default_value))
            {
                var default_value = field.default_value;

                if (field.type == FieldDescriptorProto.Type.TYPE_STRING)
                    default_value = $"\"{default_value}\"";

                options.Add("default", default_value);
            }
            else if (field.type == FieldDescriptorProto.Type.TYPE_ENUM && field.label != FieldDescriptorProto.Label.LABEL_REPEATED)
            {
                options.Add("default", ResolveOrDeferEnumDefaultValue(type));
            }

            var fieldOptions = DumpOptions(field.options, extensions);
            foreach (var pair in fieldOptions)
            {
                options[pair.Key] = pair.Value;
            }

            var parameters = string.Empty;
            if (options.Count > 0)
            {
                parameters = $" [{string.Join(", ", options.Select(kvp => $"{kvp.Key} = {kvp.Value}"))}]";
            }

            PopDescriptorName();

            var descriptorDeclarationBuilder = new StringBuilder();
            if (emitFieldLabel)
            {
                descriptorDeclarationBuilder.Append(GetLabel(field.label));
                descriptorDeclarationBuilder.Append(" ");
            }
            
            descriptorDeclarationBuilder.Append($"{type} {field.name} = {field.number}{parameters};");

            return descriptorDeclarationBuilder.ToString();
        }

        private void DumpExtensionDescriptor(IEnumerable<FieldDescriptorProto> fields, ProtoInputData inputData, StringBuilder sb, string levelspace)
        {
            foreach (var mapping in fields.GroupBy(x => x.extendee))
            {
                if (string.IsNullOrEmpty(mapping.Key))
                    throw new Exception("Empty extendee in extension, this should not be possible");

                if (mapping.Key.StartsWith(".google.protobuf", StringComparison.Ordinal))
                {
                    BuildExtension(mapping.Key.Substring(1), mapping.ToArray(), inputData);
                }

                sb.AppendLine($"{levelspace}extend {mapping.Key} {{");

                foreach (var field in mapping)
                {
                    sb.AppendLine($"{levelspace}\t{BuildDescriptorDeclaration(field, inputData.extensions)}");
                }

                sb.AppendLine($"{levelspace}}}");
                sb.AppendLine();
            }
        }

        private void DumpFileDescriptor(ProtoInputData inputData, StringBuilder sb)
        {
            if(!string.IsNullOrEmpty(inputData.proto.package))
                PushDescriptorName(inputData.proto);

            var marker = false;

            foreach (var dependency in inputData.proto.dependency)
            {
                sb.AppendLine($"import \"{dependency}\";");
                marker = true;
            }

            if (marker)
            {
                sb.AppendLine();
                marker = false;
            }

            if (!string.IsNullOrEmpty(inputData.proto.package))
            {
                sb.AppendLine($"package {inputData.proto.package};");
                marker = true;
            }

            if (marker)
            {
                sb.AppendLine();
                marker = false;
            }

            foreach (var option in DumpOptions(inputData.proto.options, inputData.extensions))
            {
                sb.AppendLine($"option {option.Key} = {option.Value};");
                marker = true;
            }

            if (marker)
            {
                sb.AppendLine();
            }

            DumpExtensionDescriptor(inputData.proto.extension, inputData, sb, string.Empty);

            foreach (var field in inputData.proto.enum_type)
            {
                DumpEnumDescriptor(field, inputData.extensions, sb, 0);
            }

            foreach (var proto in inputData.proto.message_type)
            {
                DumpDescriptor(proto, inputData, sb, 0);
            }

            foreach (var service in inputData.proto.service)
            {
                sb.AppendLine($"service {service.name} {{");

                foreach (var option in DumpOptions(service.options, inputData.extensions))
                {
                    sb.AppendLine($"\toption {option.Key} = {option.Value};");
                }

                foreach (var method in service.method)
                {
                    var declaration = $"\trpc {method.name} ({method.input_type}) returns ({method.output_type})";

                    var options = DumpOptions(method.options, inputData.extensions);

                    var parameters = string.Empty;
                    if (options.Count == 0)
                    {
                        sb.AppendLine($"{declaration};");
                    }
                    else
                    {
                        sb.AppendLine($"{declaration} {{");

                        foreach (var option in options)
                        {
                            sb.AppendLine($"\t\toption {option.Key} = {option.Value};");
                        }

                        sb.AppendLine("\t}");
                    }
                }

                sb.AppendLine("}");
            }

            if (!string.IsNullOrEmpty(inputData.proto.package))
                PopDescriptorName();
        }

        private void DumpDescriptor(DescriptorProto proto, ProtoInputData inputData, StringBuilder sb, int level)
        {
            PushDescriptorName(proto);

            var levelspace = new string('\t', level);

            sb.AppendLine($"{levelspace}message {proto.name} {{");

            foreach (var option in DumpOptions(proto.options, inputData.extensions))
            {
                sb.AppendLine($"{levelspace}\toption {option.Key} = {option.Value};");
            }

            foreach (var field in proto.nested_type)
            {
                DumpDescriptor(field, inputData, sb, level + 1);
            }

            DumpExtensionDescriptor(proto.extension, inputData, sb, $"{levelspace}\t");

            foreach (var field in proto.enum_type)
            {
                DumpEnumDescriptor(field, inputData.extensions, sb, level + 1);
            }

            foreach (var field in proto.field.Where(x => !x.oneof_indexSpecified))
            {
                var enumLookup = new List<EnumDescriptorProto>();

                enumLookup.AddRange( inputData.proto.enum_type ); // add global enums
                enumLookup.AddRange( proto.enum_type ); // add this message's nested enums

                sb.AppendLine($"{levelspace}\t{BuildDescriptorDeclaration(field, inputData.extensions)}");
            }

            for (var i = 0; i < proto.oneof_decl.Count; i++)
            {
                var oneof = proto.oneof_decl[i];
                var fields = proto.field.Where(x => x.oneof_indexSpecified && x.oneof_index == i).ToArray();

                sb.AppendLine($"{levelspace}\toneof {oneof.name} {{");

                foreach(var field in fields)
                {
                    sb.AppendLine($"{levelspace}\t\t{BuildDescriptorDeclaration(field, inputData.extensions, emitFieldLabel: false)}");
                }

                sb.AppendLine($"{levelspace}\t}}");
            }

            if (proto.extension_range.Count > 0)
                sb.AppendLine();

            foreach (var range in proto.extension_range)
            {
                var max = Convert.ToString( range.end );

                // http://code.google.com/apis/protocolbuffers/docs/proto.html#extensions
                // If your numbering convention might involve extensions having very large numbers as tags, you can specify
                // that your extension range goes up to the maximum possible field number using the max keyword:
                // max is 2^29 - 1, or 536,870,911. 
                if ( range.end >= 536870911 )
                {
                    max = "max";
                }

                sb.AppendLine($"{levelspace}\textensions {range.start} to {max};");
            }

            sb.AppendLine($"{levelspace}}}");
            sb.AppendLine();

            PopDescriptorName();
        }

        private void DumpEnumDescriptor(EnumDescriptorProto field, ProtoExtensionDefs extensions, StringBuilder sb, int level)
        {
            AddEnumDescriptorLookup(field);

            var levelspace = new string('\t', level);

            sb.AppendLine($"{levelspace}enum {field.name} {{");

            foreach (var option in DumpOptions(field.options, extensions))
            {
                sb.AppendLine($"{levelspace}\toption {option.Key} = {option.Value};");
            }

            foreach (var enumValue in field.value)
            {
                var options = DumpOptions(enumValue.options, extensions);

                var parameters = string.Empty;
                if (options.Count > 0)
                {
                    parameters = $" [{string.Join(", ", options.Select(kvp => $"{kvp.Key} = {kvp.Value}"))}]";
                }

                sb.AppendLine($"{levelspace}\t{enumValue.name} = {enumValue.number}{parameters};");
            }

            sb.AppendLine($"{levelspace}}}");
            sb.AppendLine();
        }

        // because of protobuf-net we're limited to parsing simple types at run-time as we can't parse the protobuf, but options shouldn't be too complex
        private void BuildExtension(string key, IEnumerable<FieldDescriptorProto> fields, ProtoInputData protoData)
        {
            var name = $"{key}Ext{Guid.NewGuid()}";
            var extension = moduleBuilder.DefineType(name, TypeAttributes.Class);

            var pcType = typeof(ProtoContractAttribute);
            var pcCtor = pcType.GetConstructor(Type.EmptyTypes);
            var pcBuilder = new CustomAttributeBuilder(pcCtor, Type.EmptyTypes);

            extension.SetCustomAttribute(pcBuilder);

            foreach (var field in fields)
            {
                var fieldType = LookupBasicType(field.type, out var format, out var buildEnumProxy);
                var fbuilder = extension.DefineField(field.name, fieldType, FieldAttributes.Public);

                object defaultValue = field.default_value;
                if (field.type == FieldDescriptorProto.Type.TYPE_ENUM)
                {
                    defaultValue = 0;
                }
                else if (string.IsNullOrEmpty(field.default_value))
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
                    var epType = typeof(EnumProxyAttribute);
                    var epCtor = epType.GetConstructor(new[] { typeof(object), typeof(string) });
                    var epBuilder = new CustomAttributeBuilder(epCtor, new object[] { field.default_value, field.type_name });

                    fbuilder.SetCustomAttribute(epBuilder);
                }

                var dvType = typeof(DefaultValueAttribute);
                var dvCtor = dvType.GetConstructor(new[] { typeof(object) });
                var dvBuilder = new CustomAttributeBuilder(dvCtor, new[] { defaultValue });

                fbuilder.SetCustomAttribute(dvBuilder);

                var pmType = typeof(ProtoMemberAttribute);
                var pmCtor = pmType.GetConstructor(new[] { typeof(int) });
                var pmBuilder = new CustomAttributeBuilder(pmCtor, new object[] { field.number }, 
                                                        new[] { pmType.GetProperty("Name"), pmType.GetProperty("DataFormat") },
                                                        new object[] {$"({field.name})", format } );


                fbuilder.SetCustomAttribute(pmBuilder);
            }

            var extensionType = extension.CreateType();

            if (!protoData.extensions.ContainsKey(key))
            {
                protoData.extensions[key] = new List<Type>();
            }

            protoData.extensions[key].Add(extensionType);

            // Update extensions known by protodefs that rely on us
            foreach (var dependent in inputProtoDefs.Where(x => x.proto.dependency.Contains(protoData.proto.name)))
            {
                if (!dependent.extensions.ContainsKey(key))
                {
                    dependent.extensions[key] = new List<Type>();
                }

                dependent.extensions[key].Add(extensionType);
            }
        }
    }
}
