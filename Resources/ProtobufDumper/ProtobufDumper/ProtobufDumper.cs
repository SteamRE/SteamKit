using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using google.protobuf;
using ProtoBuf;

// Protobuf descriptor documentation: https://protobuf.com/docs/descriptors

namespace ProtobufDumper
{
    class ProtobufDumper
    {
        public delegate void ProcessProtobuf( string name, string buffer );

        readonly List<FileDescriptorProto> protobufs;
        readonly Stack<string> messageNameStack;
        readonly Dictionary<string, ProtoNode> protobufMap;
        readonly Dictionary<string, ProtoTypeNode> protobufTypeMap;
        readonly SortedSet<string> optionImports = [];

        class ProtoNode
        {
            public string Name;
            public FileDescriptorProto Proto;
            public List<ProtoNode> Dependencies;
            public HashSet<FileDescriptorProto> AllPublicDependencies;
            public List<ProtoTypeNode> Types;
            public bool Defined;
        }

        class ProtoTypeNode
        {
            public string Name;
            public FileDescriptorProto Proto;
            public object Source;
            public bool Defined;
        }

        [ProtoContract]
        class ExtensionPlaceholder : IExtensible
        {
            IExtension extensionObject;

            IExtension IExtensible.GetExtensionObject( bool createIfMissing )
            {
                return Extensible.GetExtensionObject( ref extensionObject, createIfMissing );
            }
        }

        public ProtobufDumper( List<FileDescriptorProto> protobufs )
        {
            this.protobufs = protobufs;
            messageNameStack = new Stack<string>();
            protobufMap = [];
            protobufTypeMap = [];
        }

        ProtoTypeNode GetOrCreateTypeNode( string name, FileDescriptorProto proto = null, object source = null )
        {
            if ( !protobufTypeMap.TryGetValue( name, out var node ) )
            {
                node = new ProtoTypeNode()
                {
                    Name = name,
                    Proto = proto,
                    Source = source,
                    Defined = source != null
                };

                protobufTypeMap.Add( name, node );
            }
            else if ( source != null )
            {
                Debug.Assert( node.Defined == false );

                node.Proto = proto;
                node.Source = source;
                node.Defined = true;
            }

            return node;
        }

        public bool Analyze()
        {
            foreach ( var proto in protobufs )
            {
                var protoNode = new ProtoNode()
                {
                    Name = proto.name,
                    Proto = proto,
                    Dependencies = [],
                    AllPublicDependencies = [],
                    Types = [],
                    Defined = true
                };

                protobufMap.Add( proto.name, protoNode );

                foreach ( var extension in proto.extension )
                {
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, extension.name ), proto, extension ) );

                    if ( IsNamedType( extension.type ) && !string.IsNullOrEmpty( extension.type_name ) )
                        protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, extension.type_name ) ) );

                    if ( !string.IsNullOrEmpty( extension.extendee ) )
                        protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, extension.extendee ) ) );
                }

                foreach ( var enumType in proto.enum_type )
                {
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, enumType.name ), proto, enumType ) );
                }

                foreach ( var messageType in proto.message_type )
                {
                    RecursiveAnalyzeMessageDescriptor( messageType, protoNode, proto.package );
                }

                foreach ( var service in proto.service )
                {
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, service.name ), proto, service ) );

                    foreach ( var method in service.method )
                    {
                        if ( !string.IsNullOrEmpty( method.input_type ) )
                            protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, method.input_type ) ) );

                        if ( !string.IsNullOrEmpty( method.output_type ) )
                            protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( proto.package, method.output_type ) ) );
                    }
                }
            }

            // inspect file dependencies
            var missingDependencies = new List<ProtoNode>();

            foreach ( var pair in protobufMap )
            {
                foreach ( var dependency in pair.Value.Proto.dependency )
                {
                    if ( dependency.StartsWith( "google", StringComparison.Ordinal ) ) continue;

                    if ( protobufMap.TryGetValue( dependency, out var depends ) )
                    {
                        pair.Value.Dependencies.Add( depends );
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine( "Unknown dependency: {0} for {1}", dependency, pair.Value.Proto.name );
                        Console.ResetColor();

                        var missing = missingDependencies.Find( x => x.Name == dependency );
                        if ( missing == null )
                        {
                            missing = new ProtoNode()
                            {
                                Name = dependency,
                                Proto = null,
                                Dependencies = [],
                                AllPublicDependencies = [],
                                Types = [],
                                Defined = false
                            };
                            missingDependencies.Add( missing );
                        }

                        pair.Value.Dependencies.Add( missing );
                    }
                }
            }

            foreach ( var depend in missingDependencies )
            {
                protobufMap.Add( depend.Name, depend );
            }

            foreach ( var pair in protobufMap )
            {
                var undefinedFiles = pair.Value.Dependencies.Where( x => !x.Defined ).ToList();

                if ( undefinedFiles.Count > 0 )
                {
                    Console.WriteLine( "Not all dependencies were found for {0}", pair.Key );

                    foreach ( var file in undefinedFiles )
                    {
                        var x = protobufMap[ file.Name ];
                        Console.WriteLine( "Dependency not found: {0}", file.Name );
                    }

                    return false;
                }

                var undefinedTypes = pair.Value.Types.Where( x => !x.Defined ).ToList();

                if ( undefinedTypes.Count > 0 )
                {
                    Console.WriteLine( "Not all types were resolved for {0}", pair.Key );

                    foreach ( var type in undefinedTypes )
                    {
                        var x = protobufTypeMap[ type.Name ];
                        Console.WriteLine( "Type not found: {0}", type.Name );
                    }

                    return false;
                }

                // build the list of all publicly accessible types from each file
                RecursiveAddPublicDependencies( pair.Value.AllPublicDependencies, pair.Value, 0 );
            }

            return true;
        }

        void RecursiveAddPublicDependencies( HashSet<FileDescriptorProto> set, ProtoNode node, int depth )
        {
            if ( depth == 0 )
            {
                foreach ( var dep in node.Proto.dependency )
                {
                    // Google dependencies are not required to be in the binary
                    if ( !protobufMap.TryGetValue( dep, out var depend ) || !depend.Defined )
                        continue;

                    set.Add( depend.Proto );
                    RecursiveAddPublicDependencies( set, depend, depth + 1 );
                }
            }
            else
            {
                foreach ( var idx in node.Proto.public_dependency )
                {
                    if ( !protobufMap.TryGetValue( node.Proto.dependency[ idx ], out var depend ) || !depend.Defined )
                        continue;

                    set.Add( depend.Proto );
                    RecursiveAddPublicDependencies( set, depend, depth + 1 );
                }
            }
        }

        void RecursiveAnalyzeMessageDescriptor( DescriptorProto messageType, ProtoNode protoNode, string packagePath )
        {
            protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( packagePath, messageType.name ), protoNode.Proto, messageType ) );

            foreach ( var extension in messageType.extension )
            {
                protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( GetPackagePath( packagePath, messageType.name ), extension.name ),
                    protoNode.Proto, extension ) );

                if ( IsNamedType( extension.type ) && !string.IsNullOrEmpty( extension.type_name ) )
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( packagePath, extension.type_name ) ) );

                if ( !string.IsNullOrEmpty( extension.extendee ) )
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( packagePath, extension.extendee ) ) );
            }

            foreach ( var enumType in messageType.enum_type )
            {
                protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( GetPackagePath( packagePath, messageType.name ), enumType.name ),
                    protoNode.Proto, enumType ) );
            }

            foreach ( var field in messageType.field )
            {
                if ( IsNamedType( field.type ) && !string.IsNullOrEmpty( field.type_name ) )
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( packagePath, field.type_name ) ) );

                if ( !string.IsNullOrEmpty( field.extendee ) )
                    protoNode.Types.Add( GetOrCreateTypeNode( GetPackagePath( packagePath, field.extendee ) ) );
            }

            foreach ( var nested in messageType.nested_type )
            {
                RecursiveAnalyzeMessageDescriptor( nested, protoNode, GetPackagePath( packagePath, messageType.name ) );
            }
        }

        public void DumpFiles( ProcessProtobuf callback )
        {
            foreach ( var proto in protobufs )
            {
                var sb = new StringBuilder();
                DumpFileDescriptor( proto, sb );

                callback( proto.name, sb.ToString() );
            }
        }

        void DumpFileDescriptor( FileDescriptorProto proto, StringBuilder sb )
        {
            if ( !string.IsNullOrEmpty( proto.package ) )
                PushDescriptorName( proto );

            var marker = false;

            if ( !string.IsNullOrEmpty( proto.syntax ) )
            {
                AppendHeadingSpace( sb, ref marker );
                sb.AppendLine( $"syntax = {Util.ToLiteral( proto.syntax )};" );
                marker = true;
            }

            for ( var i = 0; i < proto.dependency.Count; i++ )
            {
                var dependency = proto.dependency[ i ];
                var modifier = string.Empty;

                if ( proto.public_dependency.Contains( i ) )
                {
                    modifier = "public ";
                }
                else if ( proto.weak_dependency.Contains( i ) )
                {
                    modifier = "weak ";
                }

                sb.AppendLine( $"import {modifier}\"{dependency}\";" );
                marker = true;
            }

            var importsEnd = sb.Length;
            var hasHeading = marker;
            optionImports.Clear();

            if ( !string.IsNullOrEmpty( proto.package ) )
            {
                AppendHeadingSpace( sb, ref marker );
                sb.AppendLine( $"package {proto.package};" );
                marker = true;
            }

            var options = DumpOptions( proto, proto.options );

            foreach ( var option in options )
            {
                AppendHeadingSpace( sb, ref marker );
                sb.AppendLine( $"option {option.Key} = {option.Value};" );
            }

            if ( options.Count > 0 )
            {
                marker = true;
            }

            DumpExtensionDescriptors( proto, proto.extension, sb, 0, ref marker );

            foreach ( var field in proto.enum_type )
            {
                DumpEnumDescriptor( proto, field, sb, 0, ref marker );
            }

            foreach ( var message in proto.message_type )
            {
                DumpDescriptor( proto, message, sb, 0, ref marker );
            }

            foreach ( var service in proto.service )
            {
                DumpService( proto, service, sb, ref marker );
            }

            // Valve's protoc has the options of its descriptor.proto built in, so the binary doesn't list the import,
            // but any other protoc only knows them when the file imports it
            if ( optionImports.Count > 0 )
            {
                var imports = new StringBuilder();

                foreach ( var import in optionImports )
                {
                    imports.AppendLine( $"import \"{import}\";" );
                }

                if ( !hasHeading && importsEnd < sb.Length )
                {
                    imports.AppendLine();
                }

                sb.Insert( importsEnd, imports.ToString() );
            }

            if ( !string.IsNullOrEmpty( proto.package ) )
                PopDescriptorName();
        }

        OptionList DumpOptions( FileDescriptorProto source, FileOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );
            if ( options.ShouldSerializeoptimize_for() )
                optionsKv.Add( "optimize_for", $"{options.optimize_for}" );
            if ( options.ShouldSerializecc_generic_services() )
                optionsKv.Add( "cc_generic_services", options.cc_generic_services ? "true" : "false" );
            if ( options.ShouldSerializecc_enable_arenas() )
                optionsKv.Add( "cc_enable_arenas", options.cc_enable_arenas ? "true" : "false" );
            if ( options.ShouldSerializego_package() )
                optionsKv.Add( "go_package", Util.ToLiteral( options.go_package ) );
            if ( options.ShouldSerializejava_package() )
                optionsKv.Add( "java_package", Util.ToLiteral( options.java_package ) );
            if ( options.ShouldSerializejava_outer_classname() )
                optionsKv.Add( "java_outer_classname", Util.ToLiteral( options.java_outer_classname ) );
            if ( options.ShouldSerializejava_generate_equals_and_hash() )
                optionsKv.Add( "java_generate_equals_and_hash", options.java_generate_equals_and_hash ? "true" : "false" );
            if ( options.ShouldSerializejava_generic_services() )
                optionsKv.Add( "java_generic_services", options.java_generic_services ? "true" : "false" );
            if ( options.ShouldSerializejava_multiple_files() )
                optionsKv.Add( "java_multiple_files", options.java_multiple_files ? "true" : "false" );
            if ( options.ShouldSerializejava_string_check_utf8() )
                optionsKv.Add( "java_string_check_utf8", options.java_string_check_utf8 ? "true" : "false" );
            if ( options.ShouldSerializepy_generic_services() )
                optionsKv.Add( "py_generic_services", options.py_generic_services ? "true" : "false" );
            if ( options.ShouldSerializeruby_package() )
                optionsKv.Add( "ruby_package", Util.ToLiteral( options.ruby_package ) );
            if ( options.ShouldSerializeobjc_class_prefix() )
                optionsKv.Add( "objc_class_prefix", Util.ToLiteral( options.objc_class_prefix ) );
            if ( options.ShouldSerializecsharp_namespace() )
                optionsKv.Add( "csharp_namespace", Util.ToLiteral( options.csharp_namespace ) );
            if ( options.ShouldSerializeswift_prefix() )
                optionsKv.Add( "swift_prefix", Util.ToLiteral( options.swift_prefix ) );
            if ( options.ShouldSerializephp_generic_services() )
                optionsKv.Add( "php_generic_services", options.php_generic_services ? "true" : "false" );
            if ( options.ShouldSerializephp_class_prefix() )
                optionsKv.Add( "php_class_prefix", Util.ToLiteral( options.php_class_prefix ) );
            if ( options.ShouldSerializephp_namespace() )
                optionsKv.Add( "php_namespace", Util.ToLiteral( options.php_namespace ) );
            if ( options.ShouldSerializephp_metadata_namespace() )
                optionsKv.Add( "php_metadata_namespace", Util.ToLiteral( options.php_metadata_namespace ) );

            DumpOptionsMatching( source, ".google.protobuf.FileOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, FieldOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializectype() )
                optionsKv.Add( "ctype", $"{options.ctype}" );
            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );
            if ( options.ShouldSerializelazy() )
                optionsKv.Add( "lazy", options.lazy ? "true" : "false" );
            if ( options.ShouldSerializepacked() )
                optionsKv.Add( "packed", options.packed ? "true" : "false" );
            if ( options.ShouldSerializeweak() )
                optionsKv.Add( "weak", options.weak ? "true" : "false" );
            if ( options.ShouldSerializejstype() )
                optionsKv.Add( "jstype", $"{options.jstype}" );

            DumpOptionsMatching( source, ".google.protobuf.FieldOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, MessageOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializemessage_set_wire_format() )
                optionsKv.Add( "message_set_wire_format", options.message_set_wire_format ? "true" : "false" );
            if ( options.ShouldSerializeno_standard_descriptor_accessor() )
                optionsKv.Add( "no_standard_descriptor_accessor", options.no_standard_descriptor_accessor ? "true" : "false" );
            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.MessageOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, EnumOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializeallow_alias() )
                optionsKv.Add( "allow_alias", options.allow_alias ? "true" : "false" );
            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.EnumOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, EnumValueOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.EnumValueOptions", options, optionsKv );

            return optionsKv;
        }


        OptionList DumpOptions( FileDescriptorProto source, ServiceOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.ServiceOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, MethodOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );
            if ( options.ShouldSerializeidempotency_level() )
                optionsKv.Add( "idempotency_level", $"{options.idempotency_level}" );

            DumpOptionsMatching( source, ".google.protobuf.MethodOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, OneofOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            DumpOptionsMatching( source, ".google.protobuf.OneofOptions", options, optionsKv );

            return optionsKv;
        }

        OptionList DumpOptions( FileDescriptorProto source, ExtensionRangeOptions options )
        {
            var optionsKv = new OptionList();

            if ( options == null )
                return optionsKv;

            DumpOptionsMatching( source, ".google.protobuf.ExtensionRangeOptions", options, optionsKv );

            return optionsKv;
        }

        void DumpOptionsFieldRecursive( FieldDescriptorProto field, IExtensible options, OptionList optionsKv, string key )
        {
            // A message that is not repeated is written field by field, a repeated one is written as an aggregate value per element
            if ( field.type == FieldDescriptorProto.Type.TYPE_MESSAGE && field.label != FieldDescriptorProto.Label.LABEL_REPEATED )
            {
                var extension = Extensible.GetValue<ExtensionPlaceholder>( options, field.number );

                if ( extension == null )
                    return;

                var count = optionsKv.Count;

                foreach ( var subField in ( ( DescriptorProto )protobufTypeMap[ field.type_name ].Source ).field )
                {
                    DumpOptionsFieldRecursive( subField, extension, optionsKv, $"{key}.{subField.name}" );
                }

                if ( optionsKv.Count == count )
                    optionsKv.Add( key, "{}" );

                return;
            }

            foreach ( var value in GetOptionValues( options, field ) )
            {
                optionsKv.Add( key, value );
            }
        }

        // Returns the values of a field in an options message, a field that is not repeated keeps its last value
        List<string> GetOptionValues( IExtensible data, FieldDescriptorProto field )
        {
            var values = field.type switch
            {
                FieldDescriptorProto.Type.TYPE_INT32 => GetValues<int>( data, field, DataFormat.Default ),
                // protobuf-net does not decode zigzag for extension values
                FieldDescriptorProto.Type.TYPE_SINT32 => GetValues<uint>( data, field, DataFormat.Default, x => Convert.ToString( ( int )( x >> 1 ) ^ -( int )( x & 1 ), CultureInfo.InvariantCulture ) ),
                FieldDescriptorProto.Type.TYPE_SFIXED32 => GetValues<int>( data, field, DataFormat.FixedSize ),
                FieldDescriptorProto.Type.TYPE_UINT32 => GetValues<uint>( data, field, DataFormat.Default ),
                FieldDescriptorProto.Type.TYPE_FIXED32 => GetValues<uint>( data, field, DataFormat.FixedSize ),
                FieldDescriptorProto.Type.TYPE_INT64 => GetValues<long>( data, field, DataFormat.Default ),
                FieldDescriptorProto.Type.TYPE_SINT64 => GetValues<ulong>( data, field, DataFormat.Default, x => Convert.ToString( ( long )( x >> 1 ) ^ -( long )( x & 1 ), CultureInfo.InvariantCulture ) ),
                FieldDescriptorProto.Type.TYPE_SFIXED64 => GetValues<long>( data, field, DataFormat.FixedSize ),
                FieldDescriptorProto.Type.TYPE_UINT64 => GetValues<ulong>( data, field, DataFormat.Default ),
                FieldDescriptorProto.Type.TYPE_FIXED64 => GetValues<ulong>( data, field, DataFormat.FixedSize ),
                FieldDescriptorProto.Type.TYPE_BOOL => GetValues<bool>( data, field, DataFormat.Default, x => x ? "true" : "false" ),
                FieldDescriptorProto.Type.TYPE_FLOAT => GetValues<float>( data, field, DataFormat.Default, Util.ToLiteral ),
                FieldDescriptorProto.Type.TYPE_DOUBLE => GetValues<double>( data, field, DataFormat.Default, Util.ToLiteral ),
                FieldDescriptorProto.Type.TYPE_STRING => GetValues<string>( data, field, DataFormat.Default, Util.ToLiteral ),
                FieldDescriptorProto.Type.TYPE_BYTES => GetValues<byte[]>( data, field, DataFormat.Default, Util.ToLiteral ),
                FieldDescriptorProto.Type.TYPE_ENUM => GetValues<int>( data, field, DataFormat.Default, x => GetEnumValueName( field, x ) ),
                FieldDescriptorProto.Type.TYPE_MESSAGE => Extensible.GetValues<ExtensionPlaceholder>( data, field.number )
                    .Select( x => BuildAggregateValue( ( DescriptorProto )protobufTypeMap[ field.type_name ].Source, x ) ).ToList(),
                _ => [],
            };

            if ( field.label != FieldDescriptorProto.Label.LABEL_REPEATED && values.Count > 1 )
                values.RemoveRange( 0, values.Count - 1 );

            return values;
        }

        static List<string> GetValues<T>( IExtensible data, FieldDescriptorProto field, DataFormat format, Func<T, string> toString = null )
        {
            toString ??= x => Convert.ToString( x, CultureInfo.InvariantCulture );

            return Extensible.GetValues<T>( data, field.number, format ).Select( toString ).ToList();
        }

        string GetEnumValueName( FieldDescriptorProto field, int number )
        {
            var enumProto = ( EnumDescriptorProto )protobufTypeMap[ field.type_name ].Source;

            // Unknown values can only be written as their number
            return enumProto.value.Find( x => x.number == number )?.name ?? Convert.ToString( number, CultureInfo.InvariantCulture );
        }

        string BuildAggregateValue( DescriptorProto messageProto, IExtensible data )
        {
            var fields = new List<string>();

            foreach ( var subField in messageProto.field )
            {
                foreach ( var value in GetOptionValues( data, subField ) )
                {
                    fields.Add( subField.type == FieldDescriptorProto.Type.TYPE_MESSAGE ? $"{subField.name} {value}" : $"{subField.name}: {value}" );
                }
            }

            return fields.Count == 0 ? "{}" : $"{{ {string.Join( " ", fields )} }}";
        }

        void DumpOptionsMatching( FileDescriptorProto source, string typeName, IExtensible options, OptionList optionsKv )
        {
            var dependencies = new HashSet<FileDescriptorProto>( protobufMap[ source.name ].AllPublicDependencies )
            {
                source
            };

            // Options that the binary's descriptor.proto has but Descriptor.cs does not, such as the ones Valve added,
            // are left over as unknown fields. The ones Descriptor.cs has were read into its properties.
            if ( protobufTypeMap.TryGetValue( typeName, out var optionsType ) && optionsType.Source is DescriptorProto optionsProto )
            {
                var count = optionsKv.Count;

                foreach ( var field in optionsProto.field )
                {
                    if ( field.name != "uninterpreted_option" )
                        DumpOptionsFieldRecursive( field, options, optionsKv, field.name );
                }

                if ( optionsKv.Count > count && !dependencies.Contains( optionsType.Proto ) )
                    optionImports.Add( optionsType.Proto.name );
            }

            foreach ( var type in protobufTypeMap )
            {
                if ( dependencies.Contains( type.Value.Proto ) && type.Value.Source is FieldDescriptorProto field )
                {
                    if ( !string.IsNullOrEmpty( field.extendee ) && field.extendee == typeName )
                    {
                        // The full name, extensions can be declared inside a message or a package
                        DumpOptionsFieldRecursive( field, options, optionsKv, $"({type.Key.TrimStart( '.' )})" );
                    }
                }
            }
        }

        void DumpExtensionDescriptors( FileDescriptorProto source, IEnumerable<FieldDescriptorProto> fields, StringBuilder sb, int level, ref bool marker )
        {
            var levelspace = new string( '\t', level );

            foreach ( var mapping in fields.GroupBy( x => x.extendee ) )
            {
                if ( string.IsNullOrEmpty( mapping.Key ) )
                    throw new Exception( "Empty extendee in extension, this should not be possible" );

                AppendHeadingSpace( sb, ref marker );
                sb.AppendLine( $"{levelspace}extend {mapping.Key} {{" );

                foreach ( var field in mapping )
                {
                    sb.AppendLine( $"{levelspace}\t{BuildDescriptorDeclaration( source, field )}" );
                }

                sb.AppendLine( $"{levelspace}}}" );
                marker = true;
            }
        }

        void DumpDescriptor( FileDescriptorProto source, DescriptorProto proto, StringBuilder sb, int level, ref bool marker )
        {
            PushDescriptorName( proto );

            var levelspace = new string( '\t', level );
            var innerMarker = false;

            AppendHeadingSpace( sb, ref marker );
            sb.AppendLine( $"{levelspace}message {proto.name} {{" );

            var options = DumpOptions( source, proto.options );

            foreach ( var option in options )
            {
                AppendHeadingSpace( sb, ref innerMarker );
                sb.AppendLine( $"{levelspace}\toption {option.Key} = {option.Value};" );
            }

            if ( options.Count > 0 )
            {
                innerMarker = true;
            }

            if ( proto.extension.Count > 0 )
            {
                DumpExtensionDescriptors( source, proto.extension, sb, level + 1, ref innerMarker );
            }

            // Map entries are written as the map fields that use them
            foreach ( var field in proto.nested_type.Where( x => x.options?.map_entry != true ) )
            {
                DumpDescriptor( source, field, sb, level + 1, ref innerMarker );
            }

            foreach ( var field in proto.enum_type )
            {
                DumpEnumDescriptor( source, field, sb, level + 1, ref innerMarker );
            }

            // Proto3 optional fields are each in a oneof of their own that the compiler made
            var rootFields = proto.field.Where( x => !x.ShouldSerializeoneof_index() || x.proto3_optional ).ToList();

            foreach ( var field in rootFields )
            {
                AppendHeadingSpace( sb, ref innerMarker );
                sb.AppendLine( $"{levelspace}\t{BuildDescriptorDeclaration( source, field )}" );
            }

            if ( rootFields.Count > 0 )
            {
                innerMarker = true;
            }

            for ( var i = 0; i < proto.oneof_decl.Count; i++ )
            {
                var oneof = proto.oneof_decl[ i ];
                var fields = proto.field.Where( x => x.ShouldSerializeoneof_index() && x.oneof_index == i ).ToArray();

                if ( fields.Any( x => x.proto3_optional ) )
                    continue;

                AppendHeadingSpace( sb, ref innerMarker );
                sb.AppendLine( $"{levelspace}\toneof {oneof.name} {{" );

                foreach ( var option in DumpOptions( source, oneof.options ) )
                {
                    sb.AppendLine( $"{levelspace}\t\toption {option.Key} = {option.Value};" );
                }

                foreach ( var field in fields )
                {
                    sb.AppendLine(
                        $"{levelspace}\t\t{BuildDescriptorDeclaration( source, field, emitFieldLabel: false )}" );
                }

                sb.AppendLine( $"{levelspace}\t}}" );
                innerMarker = true;
            }

            // The ends of extension and reserved ranges in messages are exclusive
            foreach ( var range in proto.extension_range )
            {
                AppendHeadingSpace( sb, ref innerMarker );
                var rangeOptions = DumpOptions( source, range.options );
                var parameters = rangeOptions.Count > 0 ? $" [{string.Join( ", ", rangeOptions.Select( kvp => $"{kvp.Key} = {kvp.Value}" ) )}]" : string.Empty;

                sb.AppendLine( $"{levelspace}\textensions {FormatRange( range.start, range.end - 1, MaxFieldNumber )}{parameters};" );
            }

            DumpReserved( proto.reserved_range.Select( x => FormatRange( x.start, x.end - 1, MaxFieldNumber ) ), proto.reserved_name, sb, levelspace, ref innerMarker );

            sb.AppendLine( $"{levelspace}}}" );
            marker = true;

            PopDescriptorName();
        }

        void DumpEnumDescriptor( FileDescriptorProto source, EnumDescriptorProto field, StringBuilder sb, int level, ref bool marker )
        {
            var levelspace = new string( '\t', level );

            AppendHeadingSpace( sb, ref marker );
            sb.AppendLine( $"{levelspace}enum {field.name} {{" );

            foreach ( var option in DumpOptions( source, field.options ) )
            {
                sb.AppendLine( $"{levelspace}\toption {option.Key} = {option.Value};" );
            }

            foreach ( var enumValue in field.value )
            {
                var options = DumpOptions( source, enumValue.options );

                var parameters = string.Empty;
                if ( options.Count > 0 )
                {
                    parameters = $" [{string.Join( ", ", options.Select( kvp => $"{kvp.Key} = {kvp.Value}" ) )}]";
                }

                sb.AppendLine( $"{levelspace}\t{enumValue.name} = {enumValue.number}{parameters};" );
            }

            // The ends of reserved ranges in enums are inclusive
            var innerMarker = false;
            DumpReserved( field.reserved_range.Select( x => FormatRange( x.start, x.end, int.MaxValue ) ), field.reserved_name, sb, levelspace, ref innerMarker );

            sb.AppendLine( $"{levelspace}}}" );
            marker = true;
        }

        const int MaxFieldNumber = 536870911;

        static string FormatRange( int start, int end, int max )
        {
            if ( start == end )
                return $"{start}";

            return end >= max ? $"{start} to max" : $"{start} to {end}";
        }

        static void DumpReserved( IEnumerable<string> ranges, List<string> names, StringBuilder sb, string levelspace, ref bool marker )
        {
            var rangeList = ranges.ToList();

            if ( rangeList.Count > 0 )
            {
                AppendHeadingSpace( sb, ref marker );
                sb.AppendLine( $"{levelspace}\treserved {string.Join( ", ", rangeList )};" );
            }

            if ( names.Count > 0 )
            {
                AppendHeadingSpace( sb, ref marker );
                sb.AppendLine( $"{levelspace}\treserved {string.Join( ", ", names.Select( Util.ToLiteral ) )};" );
            }
        }

        void DumpService( FileDescriptorProto source, ServiceDescriptorProto service, StringBuilder sb, ref bool marker )
        {
            var innerMarker = false;

            AppendHeadingSpace( sb, ref marker );
            sb.AppendLine( $"service {service.name} {{" );

            var rootOptions = DumpOptions( source, service.options );

            foreach ( var option in rootOptions )
            {
                sb.AppendLine( $"\toption {option.Key} = {option.Value};" );
            }

            if ( rootOptions.Count > 0 )
            {
                innerMarker = true;
            }

            foreach ( var method in service.method )
            {
                var declaration = $"\trpc {method.name} ({( method.client_streaming ? "stream " : "" )}{method.input_type}) returns ({( method.server_streaming ? "stream " : "" )}{method.output_type})";

                var options = DumpOptions( source, method.options );

                AppendHeadingSpace( sb, ref innerMarker );

                if ( options.Count == 0 )
                {
                    sb.AppendLine( $"{declaration};" );
                }
                else
                {
                    sb.AppendLine( $"{declaration} {{" );

                    foreach ( var option in options )
                    {
                        sb.AppendLine( $"\t\toption {option.Key} = {option.Value};" );
                    }

                    sb.AppendLine( "\t}" );
                    innerMarker = true;
                }
            }

            sb.AppendLine( "}" );
            marker = true;
        }

        string BuildDescriptorDeclaration( FileDescriptorProto source, FieldDescriptorProto field, bool emitFieldLabel = true )
        {
            PushDescriptorName( field );

            var type = ResolveType( field );
            var options = new OptionList();
            var isProto2 = string.IsNullOrEmpty( source.syntax ) || source.syntax == "proto2";

            if ( field.ShouldSerializedefault_value() )
            {
                var defaultValue = field.default_value;

                // Strings are stored as is, bytes are already escaped
                if ( field.type == FieldDescriptorProto.Type.TYPE_STRING )
                    defaultValue = Util.ToLiteral( defaultValue );
                else if ( field.type == FieldDescriptorProto.Type.TYPE_BYTES )
                    defaultValue = $"\"{defaultValue}\"";

                options.Add( "default", defaultValue );
            }
            else if ( isProto2 && field.type == FieldDescriptorProto.Type.TYPE_ENUM && field.label != FieldDescriptorProto.Label.LABEL_REPEATED )
            {
                var lookup = protobufTypeMap[ field.type_name ];

                if ( lookup.Source is EnumDescriptorProto enumDescriptor && enumDescriptor.value.Count > 0 )
                    options.Add( "default", enumDescriptor.value[ 0 ].name );
            }

            if ( !string.IsNullOrEmpty( field.json_name ) )
            {
                options.Add( "json_name", Util.ToLiteral( field.json_name ) );
            }

            options.AddRange( DumpOptions( source, field.options ) );

            var parameters = string.Empty;
            if ( options.Count > 0 )
            {
                parameters = $" [{string.Join( ", ", options.Select( kvp => $"{kvp.Key} = {kvp.Value}" ) )}]";
            }

            PopDescriptorName();

            string label = null;

            if ( field.label == FieldDescriptorProto.Label.LABEL_REPEATED && field.type == FieldDescriptorProto.Type.TYPE_MESSAGE
                && protobufTypeMap[ field.type_name ].Source is DescriptorProto { options.map_entry: true } mapEntry )
            {
                var key = mapEntry.field.Find( x => x.number == 1 );
                var value = mapEntry.field.Find( x => x.number == 2 );

                type = $"map<{ResolveType( key )}, {ResolveType( value )}>";
            }
            else if ( emitFieldLabel )
            {
                // Fields in proto3 have no label unless they are repeated or track presence
                label = isProto2 || field.label == FieldDescriptorProto.Label.LABEL_REPEATED || field.proto3_optional ? GetLabel( field.label ) : null;
            }

            var descriptorDeclarationBuilder = new StringBuilder();
            if ( label != null )
            {
                descriptorDeclarationBuilder.Append( label );
                descriptorDeclarationBuilder.Append( ' ' );
            }

            descriptorDeclarationBuilder.Append( $"{type} {field.name} = {field.number}{parameters};" );

            return descriptorDeclarationBuilder.ToString();
        }

        static bool IsNamedType( FieldDescriptorProto.Type type )
        {
            return type == FieldDescriptorProto.Type.TYPE_MESSAGE || type == FieldDescriptorProto.Type.TYPE_ENUM;
        }

        static string GetPackagePath( string package, string name )
        {
            package = package.Length == 0 || package.StartsWith( '.' ) ? package : $".{package}";
            return name.StartsWith( '.' ) ? name : $"{package}.{name}";
        }

        static string GetLabel( FieldDescriptorProto.Label label )
        {
            return label switch
            {
                FieldDescriptorProto.Label.LABEL_REQUIRED => "required",
                FieldDescriptorProto.Label.LABEL_REPEATED => "repeated",
                _ => "optional",
            };
        }

        static string GetType( FieldDescriptorProto.Type type )
        {
            return type switch
            {
                FieldDescriptorProto.Type.TYPE_INT32 => "int32",
                FieldDescriptorProto.Type.TYPE_INT64 => "int64",
                FieldDescriptorProto.Type.TYPE_SINT32 => "sint32",
                FieldDescriptorProto.Type.TYPE_SINT64 => "sint64",
                FieldDescriptorProto.Type.TYPE_UINT32 => "uint32",
                FieldDescriptorProto.Type.TYPE_UINT64 => "uint64",
                FieldDescriptorProto.Type.TYPE_STRING => "string",
                FieldDescriptorProto.Type.TYPE_BOOL => "bool",
                FieldDescriptorProto.Type.TYPE_BYTES => "bytes",
                FieldDescriptorProto.Type.TYPE_DOUBLE => "double",
                FieldDescriptorProto.Type.TYPE_ENUM => "enum",
                FieldDescriptorProto.Type.TYPE_FLOAT => "float",
                FieldDescriptorProto.Type.TYPE_GROUP => "GROUP",
                FieldDescriptorProto.Type.TYPE_MESSAGE => "message",
                FieldDescriptorProto.Type.TYPE_FIXED32 => "fixed32",
                FieldDescriptorProto.Type.TYPE_FIXED64 => "fixed64",
                FieldDescriptorProto.Type.TYPE_SFIXED32 => "sfixed32",
                FieldDescriptorProto.Type.TYPE_SFIXED64 => "sfixed64",
                _ => type.ToString(),
            };
        }

        static string ResolveType( FieldDescriptorProto field )
        {
            if ( IsNamedType( field.type ) )
            {
                return field.type_name;
            }

            return GetType( field.type );
        }

        static void AppendHeadingSpace( StringBuilder sb, ref bool marker )
        {
            if ( marker )
            {
                sb.AppendLine();
                marker = false;
            }
        }

        void PushDescriptorName( FileDescriptorProto file )
        {
            messageNameStack.Push( file.package );
        }

        void PushDescriptorName( DescriptorProto proto )
        {
            messageNameStack.Push( proto.name );
        }

        void PushDescriptorName( FieldDescriptorProto field )
        {
            messageNameStack.Push( field.name );
        }

        void PopDescriptorName()
        {
            messageNameStack.Pop();
        }
    }
}
