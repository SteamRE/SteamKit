using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using google.protobuf;
using ProtoBuf;
using System.Reflection;

namespace ProtobufDumper
{
    class ImageFile
    {
        string fileName;

        IntPtr hModule;
        uint loadAddr;

        public string OutputDir { get; private set; }
        public List<string> ProtoList { get; private set; }

        public ImageFile( string fileName, string output = null )
        {
            this.fileName = fileName;
            this.OutputDir = output ?? Path.GetFileNameWithoutExtension( this.fileName );
            this.ProtoList = new List<string>();
        }

        public void Process()
        {
            Console.WriteLine( "Loading image '{0}'...", fileName );

            hModule = Native.LoadLibraryEx(
                this.fileName,
                IntPtr.Zero,
                Native.LoadLibraryFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE
            );

            if ( hModule == IntPtr.Zero )
                throw new Win32Exception( Marshal.GetLastWin32Error(), "Unable to load image!" );

            // LOAD_LIBRARY_AS_IMAGE_RESOURCE returns ( HMODULE | 2 )
            loadAddr = ( uint )( hModule.ToInt32() & ~2 );

            Console.WriteLine( "Loaded at 0x{0:X2}!", loadAddr );

            var dosHeader = Native.PtrToStruct<Native.IMAGE_DOS_HEADER>( loadAddr );

            if ( dosHeader.e_magic != Native.IMAGE_DOS_SIGNATURE )
                throw new InvalidOperationException( "Target image has invalid DOS signature!" );

            var peHeader = Native.PtrToStruct<Native.IMAGE_NT_HEADERS>( ( uint )( loadAddr + dosHeader.e_lfanew ) );

            if ( peHeader.Signature != Native.IMAGE_NT_SIGNATURE )
                throw new InvalidOperationException( "Target image has invalid PE signature!" );

            int numSections = peHeader.FileHeader.NumberOfSections;
            int sizeOfNtHeaders = Marshal.SizeOf( peHeader );
            uint baseSectionsAddr = ( uint )( loadAddr + dosHeader.e_lfanew + sizeOfNtHeaders );

            Console.WriteLine( "# of sections: {0}", numSections );

            var sectionHeaders = new Native.IMAGE_SECTION_HEADER[ numSections ];

            for ( int x = 0 ; x < sectionHeaders.Length ; ++x )
            {
                uint baseAddr = ( uint )( baseSectionsAddr + ( x * Marshal.SizeOf( sectionHeaders[ x ] ) ) );

                var sectionHdr = Native.PtrToStruct<Native.IMAGE_SECTION_HEADER>( baseAddr );

                var searchFlags =
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_MEM_READ |
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_CNT_INITIALIZED_DATA;

                var excludeFlags =
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_MEM_WRITE |
                    Native.IMAGE_SECTION_HEADER.CharacteristicFlags.IMAGE_SCN_MEM_DISCARDABLE;

                if ( ( sectionHdr.Characteristics & searchFlags ) != searchFlags )
                {
                    Console.WriteLine( "\nSection '{0}' skipped: not an initialized read section.", sectionHdr.Name );
                    continue;
                }

                if ( ( sectionHdr.Characteristics & excludeFlags ) != 0 )
                {
                    Console.WriteLine( "\nSection '{0}' skipped: not a non-discardable readonly section.", sectionHdr.Name );
                    continue;
                }

                ScanSection( sectionHdr );
            }
        }

        unsafe void ScanSection( Native.IMAGE_SECTION_HEADER sectionHdr )
        {
            uint sectionDataAddr = loadAddr + sectionHdr.PointerToRawData;

            Console.WriteLine( "\nScanning section '{0}' at 0x{1:X2}...\n", sectionHdr.Name, sectionDataAddr );

            byte* dataPtr = ( byte* )( sectionDataAddr );
            byte* endPtr = ( byte* )( dataPtr + sectionHdr.SizeOfRawData );

            while ( dataPtr < endPtr )
            {

                if ( *dataPtr == 0x0A )
                {
                    byte* originalPtr = dataPtr;
                    int nullskiplevel = 0;

                rescan:
                    int t = nullskiplevel;
                    dataPtr = originalPtr;

                    byte[] data = null;

                    using ( var ms = new MemoryStream() )
                    using ( var bw = new BinaryWriter( ms ) )
                    {
                        for ( ; *( short* )dataPtr != 0 || t-- > 0 ; dataPtr++ )
                        {
                            bw.Write( *dataPtr );
                        }

                        bw.Write( (byte)0 );

                        data = ms.ToArray();
                    }


                    dataPtr++;

                    if ( data.Length < 2 )
                    {
                        dataPtr = originalPtr + 1;
                        continue;
                    }

                    byte strLen = data[ 1 ];

                    if ( data.Length - 2 < strLen )
                    {
                        dataPtr = originalPtr + 1;
                        continue;
                    }

                    string protoName = Encoding.ASCII.GetString( data, 2, strLen );

                    if ( !protoName.EndsWith( ".proto" ) )
                    {
                        dataPtr = originalPtr + 1;
                        continue;
                    }

                    if ( !HandleProto( protoName, data ) )
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
            if ( hModule == IntPtr.Zero )
                return;

            Native.FreeLibrary( hModule );
        }

        private bool HandleProto( string name, byte[] data )
        {
            if ( name == "google/protobuf/descriptor.proto" )
                return true;

            Console.WriteLine( "Found protobuf candidate '{0}'!", name );

            FileDescriptorProto set = null;

            if ( Environment.GetCommandLineArgs().Contains( "-dump", StringComparer.OrdinalIgnoreCase ) )
            {
                string fileName = Path.Combine( OutputDir, name + ".dump" );
                Directory.CreateDirectory( Path.GetDirectoryName( fileName ) );

                Console.WriteLine( "  ! Dumping to '{0}'!", fileName );

                try
                {
                    File.WriteAllBytes( fileName, data );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Unable to dump: {0}", ex.Message );
                    return true;
                }

            }

            try
            {
                using ( MemoryStream ms = new MemoryStream( data ) )
                    set = Serializer.Deserialize<FileDescriptorProto>( ms );
            }
            catch ( Exception ex )
            {
                if (ex.GetType() == typeof(EndOfStreamException))
                {
                    Console.WriteLine("'{0}' needs rescan: {1}\n", name, ex.Message);
                    return false;
                }

                Console.WriteLine("'{0}' was invalid: {1}\n", name, ex.Message);
                return true;
            }

            StringBuilder sb = new StringBuilder();

            if ( !string.IsNullOrEmpty( set.package ) )
            {
                sb.AppendLine( "package " + set.package + ";" );
                sb.AppendLine();
            }

            // disabling this because our protobuf compiler doesn't support it
            // DumpOptions( set.options, sb );

            foreach ( string dependency in set.dependency )
            {
                sb.AppendLine( "import \"" + dependency + "\";" );
            }

            sb.AppendLine();

            DumpExtensionDescriptor(set.extension, sb, String.Empty);

            foreach ( DescriptorProto proto in set.message_type )
            {
                DumpDescriptor(proto, sb, 0);
            }

            foreach ( ServiceDescriptorProto service in set.service )
            {
                sb.AppendLine( "service " + service.name + " {" );

                foreach ( MethodDescriptorProto method in service.method )
                {
                    sb.AppendLine( "\trpc " + method.name + " (" + method.input_type + ") returns (" + method.output_type + ");" );
                }

                sb.AppendLine( "}" );
            }

            Directory.CreateDirectory( OutputDir );
            string outputFile = Path.Combine( OutputDir, set.name );

            Console.WriteLine( "  ! Outputting proto to '{0}'\n", outputFile );
            Directory.CreateDirectory( Path.GetDirectoryName( outputFile ) );
            File.WriteAllText( outputFile, sb.ToString() );

            ProtoList.Add( outputFile );

            return true;
        }



        private static String GetLabel( FieldDescriptorProto.Label label )
        {
            switch ( label )
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

        private static String GetType( FieldDescriptorProto.Type type )
        {
            switch ( type )
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

        private static String ResolveType( FieldDescriptorProto field )
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

        private static void DumpOptions( google.protobuf.FileOptions fileOptions, StringBuilder sb )
        {
            foreach ( PropertyInfo propInfo in fileOptions.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                string name = propInfo.Name;
                object value = null;

                DefaultValueAttribute[] defaultValueList = ( DefaultValueAttribute[] )propInfo.GetCustomAttributes( typeof( DefaultValueAttribute ), false );

                if ( defaultValueList.Length == 0 || propInfo.GetCustomAttributes( typeof( ProtoMemberAttribute ), false ).Length == 0 )
                {
                    continue;
                }

                var defValue = defaultValueList[ 0 ];

                try
                {
                    value = propInfo.GetValue( fileOptions, null );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "Unable to dump option '{0}': {1}", name, ex.Message );
                    continue;
                }

                if ( defValue.Value != null && defValue.Value.Equals( value ) )
                {
                    continue;
                }

                if ( value is string )
                {
                    if ( string.IsNullOrEmpty( ( string )value ) )
                    {
                        continue;
                    }

                    value = string.Format( "\"{0}\"", value );
                }

                if ( value is bool )
                {
                    value = value.ToString().ToLower();
                }


                sb.AppendLine( "option " + name + " = " + value + ";" );
            }

            sb.AppendLine();
        }

        private static string BuildDescriptorDeclaration( FieldDescriptorProto field, List<EnumDescriptorProto> enum_lookup )
        {
            string type = ResolveType(field);
            string parameters = "";

            if ( !String.IsNullOrEmpty(field.default_value) )
            {
                parameters = " [default = " + field.default_value + "]";
            }
            else if ( field.type == FieldDescriptorProto.Type.TYPE_ENUM && enum_lookup != null )
            {
                var potEnum = enum_lookup.Find(protoEnum => type.EndsWith(protoEnum.name));

                if (potEnum != null)
                {
                    parameters = " [default = " + potEnum.value[0].name + "]";
                }
            }

            return GetLabel(field.label) + " " + type + " " + field.name + " = " + field.number + parameters + ";";
        }

        private static void DumpExtensionDescriptor( List<FieldDescriptorProto> fields, StringBuilder sb, string levelspace )
        {
            foreach (var mapping in fields.GroupBy(x => { return x.extendee; }))
            {
                if (String.IsNullOrEmpty(mapping.Key))
                    throw new Exception("Empty extendee in extension, this should not be possible");

                sb.AppendLine(levelspace + "extend " + mapping.Key + " {");

                foreach (FieldDescriptorProto field in mapping)
                {
                    sb.AppendLine(levelspace + "\t" + BuildDescriptorDeclaration(field, null));
                }

                sb.AppendLine(levelspace + "}");
                sb.AppendLine();
            }
        }

        private static void DumpDescriptor( DescriptorProto proto, StringBuilder sb, int level )
        {
            string levelspace = new String( '\t', level );

            sb.AppendLine( levelspace + "message " + proto.name + " {" );

            foreach ( DescriptorProto field in proto.nested_type )
            {
                DumpDescriptor( field, sb, level + 1 );
            }

            DumpExtensionDescriptor( proto.extension, sb, levelspace + '\t' );

            foreach ( EnumDescriptorProto field in proto.enum_type )
            {
                sb.AppendLine( levelspace + "\tenum " + field.name + " {" );

                foreach ( EnumValueDescriptorProto enumValue in field.value )
                {
                    sb.AppendLine( levelspace + "\t\t" + enumValue.name + " = " + enumValue.number + ";" );
                }
        
                sb.AppendLine( levelspace + "\t}" );
                sb.AppendLine();
            }

            foreach ( FieldDescriptorProto field in proto.field )
            {
                sb.AppendLine( levelspace + "\t" + BuildDescriptorDeclaration(field, proto.enum_type) );
            }

            if (proto.extension_range.Count > 0)
                sb.AppendLine();

            foreach ( DescriptorProto.ExtensionRange range in proto.extension_range )
            {
                sb.AppendLine(levelspace + "\textensions " + range.start + " to " + range.end + ";" );
            }

            sb.AppendLine( levelspace + "}" );
            sb.AppendLine();
        }
    }
}
