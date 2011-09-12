using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ProtoBuf;
using ProtoBuf.Meta;
using google.protobuf;

namespace ProtobufDumper
{
    class ImageFile
    {
        private static ModuleBuilder moduleBuilder;

        static ImageFile()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ProtobufDumper"), AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("JIT", true);
        }


        string fileName;

        IntPtr hModule;
        uint loadAddr;

        public string OutputDir { get; private set; }
        public List<string> ProtoList { get; private set; }

        private List<FileDescriptorProto> deferredProtos;

        private Dictionary<string, List<Type>> protobufExtensions;


        public ImageFile(string fileName, string output = null)
        {
            this.fileName = fileName;
            this.OutputDir = output ?? Path.GetFileNameWithoutExtension(this.fileName);
            this.ProtoList = new List<string>();

            this.deferredProtos = new List<FileDescriptorProto>();

            this.protobufExtensions = new Dictionary<string, List<Type>>();
        }

        public void Process()
        {
            Console.WriteLine("Loading image '{0}'...", fileName);

            hModule = Native.LoadLibraryEx(
                this.fileName,
                IntPtr.Zero,
                Native.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE
            );

            if (hModule == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to load image!");

            // LOAD_LIBRARY_AS_IMAGE_RESOURCE returns ( HMODULE | 2 )
            loadAddr = (uint)(hModule.ToInt32() & ~2);

            Console.WriteLine("Loaded at 0x{0:X2}!", loadAddr);

            var dosHeader = Native.PtrToStruct<Native.IMAGE_DOS_HEADER>(loadAddr);

            if (dosHeader.e_magic != Native.IMAGE_DOS_SIGNATURE)
                throw new InvalidOperationException("Target image has invalid DOS signature!");

            var peHeader = Native.PtrToStruct<Native.IMAGE_NT_HEADERS>((uint)(loadAddr + dosHeader.e_lfanew));

            if (peHeader.Signature != Native.IMAGE_NT_SIGNATURE)
                throw new InvalidOperationException("Target image has invalid PE signature!");

            int numSections = peHeader.FileHeader.NumberOfSections;
            int sizeOfNtHeaders = Marshal.SizeOf(peHeader);
            uint baseSectionsAddr = (uint)(loadAddr + dosHeader.e_lfanew + sizeOfNtHeaders);

            Console.WriteLine("# of sections: {0}", numSections);

            var sectionHeaders = new Native.IMAGE_SECTION_HEADER[numSections];

            for (int x = 0; x < sectionHeaders.Length; ++x)
            {
                uint baseAddr = (uint)(baseSectionsAddr + (x * Marshal.SizeOf(sectionHeaders[x])));

                var sectionHdr = Native.PtrToStruct<Native.IMAGE_SECTION_HEADER>(baseAddr);

                var searchFlags =
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_MEM_READ |
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_CNT_INITIALIZED_DATA;

                var excludeFlags =
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_MEM_WRITE |
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_MEM_DISCARDABLE;

                if ((sectionHdr.Characteristics & searchFlags) != searchFlags)
                {
                    Console.WriteLine("\nSection '{0}' skipped: not an initialized read section.", sectionHdr.Name);
                    continue;
                }

                if ((sectionHdr.Characteristics & excludeFlags) != 0)
                {
                    Console.WriteLine("\nSection '{0}' skipped: not a non-discardable readonly section.", sectionHdr.Name);
                    continue;
                }

                ScanSection(sectionHdr);
            }
        }

        unsafe void ScanSection(Native.IMAGE_SECTION_HEADER sectionHdr)
        {
            uint sectionDataAddr = loadAddr + sectionHdr.PointerToRawData;

            Console.WriteLine("\nScanning section '{0}' at 0x{1:X2}...\n", sectionHdr.Name, sectionDataAddr);

            byte* dataPtr = (byte*)(sectionDataAddr);
            byte* endPtr = (byte*)(dataPtr + sectionHdr.SizeOfRawData);

            while (dataPtr < endPtr)
            {

                if (*dataPtr == 0x0A)
                {
                    byte* originalPtr = dataPtr;
                    int nullskiplevel = 0;

                rescan:
                    int t = nullskiplevel;
                    dataPtr = originalPtr;

                    byte[] data = null;

                    using (var ms = new MemoryStream())
                    using (var bw = new BinaryWriter(ms))
                    {
                        for (; *(short*)dataPtr != 0 || t-- > 0; dataPtr++)
                        {
                            bw.Write(*dataPtr);
                        }

                        bw.Write((byte)0);

                        data = ms.ToArray();
                    }


                    dataPtr++;

                    if (data.Length < 2)
                    {
                        dataPtr = originalPtr + 1;
                        continue;
                    }

                    byte strLen = data[1];

                    if (data.Length - 2 < strLen)
                    {
                        dataPtr = originalPtr + 1;
                        continue;
                    }

                    string protoName = Encoding.ASCII.GetString(data, 2, strLen);

                    if (!protoName.EndsWith(".proto"))
                    {
                        dataPtr = originalPtr + 1;
                        continue;
                    }

                    if (!HandleProto(protoName, data))
                    {
                        nullskiplevel++;

                        goto rescan;
                    }
                }
                else
                {
                    dataPtr++;
                }
            }
        }

        public void Unload()
        {
            if (hModule == IntPtr.Zero)
                return;

            Native.FreeLibrary(hModule);
        }

        private bool HandleProto(string name, byte[] data)
        {
            if (name == "google/protobuf/descriptor.proto")
                return true;

            Console.WriteLine("Found protobuf candidate '{0}'!", name);

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
                    return true;
                }
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                    set = Serializer.Deserialize<FileDescriptorProto>(ms);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(EndOfStreamException))
                {
                    Console.WriteLine("'{0}' needs rescan: {1}\n", name, ex.Message);
                    return false;
                }

                Console.WriteLine("'{0}' was invalid: {1}\n", name, ex.Message);
                return true;
            }

            if (ShouldDeferProto(set))
            {
                Console.WriteLine("  ! Deferring parsing proto '{0}'\n", set.name);

                deferredProtos.Add(set);
            }
            else
            {
                DoParseFile(set);
            }

            return true;
        }

        private bool ShouldDeferProto(FileDescriptorProto set)
        {
            bool defer = false;
            foreach (string dependency in set.dependency)
            {
                if (!dependency.StartsWith("google") && !ProtoList.Contains(dependency))
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

            Directory.CreateDirectory(OutputDir);
            string outputFile = Path.Combine(OutputDir, set.name);

            Console.WriteLine("  ! Outputting proto to '{0}'\n", outputFile);
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            File.WriteAllText(outputFile, sb.ToString());

            ProtoList.Add(set.name);

            List<FileDescriptorProto> protosToRender = new List<FileDescriptorProto>();
            do
            {
                protosToRender.Clear();

                foreach (var proto in deferredProtos)
                {
                    if (!ShouldDeferProto(proto))
                        protosToRender.Add(proto);
                }

                foreach (var proto in protosToRender)
                {
                    deferredProtos.Remove(proto);
                    DoParseFile(proto);
                }
            }
            while (protosToRender.Count != 0);
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

        public static Type LookupBasicType(FieldDescriptorProto.Type type, out DataFormat format)
        {
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

            name = null;

            if (defaultValueList.Length == 0 || protoMember.Length == 0)
                return null;

            name = protoMember[0].Name;
            object value = null;

            var defValue = defaultValueList[0];

            try
            {
                if(field)
                    value = propInfo.GetValue(options);
                else
                    value = propInfo.GetValue(options, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to dump option '{0}': {1}", name, ex.Message);
                return null;
            }

            if (defValue.Value != null && defValue.Value.Equals(value))
                return null;

            if (value is string)
            {
                if (string.IsNullOrEmpty((string)value))
                    return null;

                value = string.Format("\"{0}\"", value);
            }
            else if (value is bool)
                value = value.ToString().ToLower();

            return Convert.ToString(value);
        }

        private Dictionary<string, string> DumpOptions(dynamic options)
        {
            Dictionary<string, string> options_kv = new Dictionary<string, string>();

            if (options == null)
                return options_kv;

            // generate precompiled options
            foreach (PropertyInfo propInfo in options.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
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

        private string BuildDescriptorDeclaration(FieldDescriptorProto field, List<EnumDescriptorProto> enum_lookup)
        {
            string type = ResolveType(field);
            Dictionary<string, string> options = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(field.default_value))
            {
                string default_value = field.default_value;

                if (field.type == FieldDescriptorProto.Type.TYPE_STRING)
                    default_value = String.Format("\"{0}\"", default_value);

                options.Add("default", default_value);
            }
            else if (field.type == FieldDescriptorProto.Type.TYPE_ENUM && enum_lookup != null)
            {
                var potEnum = enum_lookup.Find(protoEnum => type.EndsWith(protoEnum.name));

                if (potEnum != null)
                    options.Add("default", potEnum.value[0].name);
            }

            options.Concat(DumpOptions(field.options));

            string parameters = String.Empty;
            if (options.Count > 0)
            {
                parameters = "[" + String.Join(", ", options.Select(kvp => String.Format("{0} = {1}", kvp.Key, kvp.Value))) + "]";
            }

            return GetLabel(field.label) + " " + type + " " + field.name + " = " + field.number + parameters + ";";
        }

        private void DumpExtensionDescriptor(List<FieldDescriptorProto> fields, StringBuilder sb, string levelspace)
        {
            foreach (var mapping in fields.GroupBy(x => { return x.extendee; }))
            {
                if (String.IsNullOrEmpty(mapping.Key))
                    throw new Exception("Empty extendee in extension, this should not be possible");

                if (mapping.Key.StartsWith(".google.protobuf"))
                {
                    BuildExtension(mapping.Key.Substring(1), mapping.ToArray());
                }

                sb.AppendLine(levelspace + "extend " + mapping.Key + " {");

                foreach (FieldDescriptorProto field in mapping)
                {
                    sb.AppendLine(levelspace + "\t" + BuildDescriptorDeclaration(field, null));
                }

                sb.AppendLine(levelspace + "}");
                sb.AppendLine();
            }
        }

        private void DumpFileDescriptor(FileDescriptorProto set, StringBuilder sb)
        {
            foreach (string dependency in set.dependency)
            {
                sb.AppendLine("import \"" + dependency + "\";");
            }

            if (!string.IsNullOrEmpty(set.package))
            {
                sb.AppendLine("package " + set.package + ";");
                sb.AppendLine();
            }

            foreach (var option in DumpOptions(set.options))
            {
                sb.AppendLine("option " + option.Key + " = " + option.Value + ";");
            }

            sb.AppendLine();

            DumpExtensionDescriptor(set.extension, sb, String.Empty);

            foreach (DescriptorProto proto in set.message_type)
            {
                DumpDescriptor(proto, sb, 0);
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
        }

        private void DumpDescriptor(DescriptorProto proto, StringBuilder sb, int level)
        {
            string levelspace = new String('\t', level);

            sb.AppendLine(levelspace + "message " + proto.name + " {");

            foreach (var option in DumpOptions(proto.options))
            {
                sb.AppendLine(levelspace + "\toption " + option.Key + " = " + option.Value + ";");
            }

            foreach (DescriptorProto field in proto.nested_type)
            {
                DumpDescriptor(field, sb, level + 1);
            }

            DumpExtensionDescriptor(proto.extension, sb, levelspace + '\t');

            foreach (EnumDescriptorProto field in proto.enum_type)
            {
                sb.AppendLine(levelspace + "\tenum " + field.name + " {");

                foreach (var option in DumpOptions(field.options))
                {
                    sb.AppendLine(levelspace + "\t\toption " + option.Key + " = " + option.Value + ";");
                }

                foreach (EnumValueDescriptorProto enumValue in field.value)
                {
                    Dictionary<string, string> options = DumpOptions(enumValue.options);

                    string parameters = String.Empty;
                    if (options.Count > 0)
                    {
                        parameters = "[" + String.Join(", ", options.Select(kvp => String.Format("{0} = {1}", kvp.Key, kvp.Value))) + "]";
                    }

                    sb.AppendLine(levelspace + "\t\t" + enumValue.name + " = " + enumValue.number + parameters + ";");
                }

                sb.AppendLine(levelspace + "\t}");
                sb.AppendLine();
            }

            foreach (FieldDescriptorProto field in proto.field)
            {
                sb.AppendLine(levelspace + "\t" + BuildDescriptorDeclaration(field, proto.enum_type));
            }

            if (proto.extension_range.Count > 0)
                sb.AppendLine();

            foreach (DescriptorProto.ExtensionRange range in proto.extension_range)
            {
                sb.AppendLine(levelspace + "\textensions " + range.start + " to " + range.end + ";");
            }

            sb.AppendLine(levelspace + "}");
            sb.AppendLine();
        }

        // because of protobuf-net we're limited to parsing simple types at run-time as we can't parse the protobuf, but options shouldn't be too complex
        private void BuildExtension(string key, FieldDescriptorProto[] fields)
        {
            TypeBuilder extension = moduleBuilder.DefineType(key + "Ext", TypeAttributes.Class);

            Type pcType = typeof(ProtoContractAttribute);
            ConstructorInfo pcCtor = pcType.GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder pcBuilder = new CustomAttributeBuilder(pcCtor,Type.EmptyTypes);

            extension.SetCustomAttribute(pcBuilder);

            foreach (var field in fields)
            {
                DataFormat format;
                Type fieldType = ImageFile.LookupBasicType(field.type, out format);

                FieldBuilder fbuilder = extension.DefineField(field.name, fieldType, FieldAttributes.Public);

                object defaultValue;
                if (String.IsNullOrEmpty(field.default_value))
                {
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
                        Console.WriteLine("Constructor for extension had bad format: {0}", key);
                        return;
                    }
                }

                Type dvType = typeof(ProtoDefaultValueAttribute);
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
