using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using google.protobuf;
using ProtoBuf;

namespace ProtobufDumper
{
    class ProtobufDumper
    {
        public delegate void ProcessProtobuf( string name, string buffer );

        readonly List<FileDescriptorProto> protobufs;
        readonly Stack<string> messageNameStack;
        readonly Dictionary<string, ProtoNode> protobufMap;
        readonly Dictionary<string, ProtoTypeNode> protobufTypeMap;

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
            protobufMap = new Dictionary<string, ProtoNode>();
            protobufTypeMap = new Dictionary<string, ProtoTypeNode>();
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
                    Dependencies = new List<ProtoNode>(),
                    AllPublicDependencies = new HashSet<FileDescriptorProto>(),
                    Types = new List<ProtoTypeNode>(),
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
                                Dependencies = new List<ProtoNode>(),
                                AllPublicDependencies = new HashSet<FileDescriptorProto>(),
                                Types = new List<ProtoTypeNode>(),
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
                    var depend = protobufMap[ dep ];
                    set.Add( depend.Proto );
                    RecursiveAddPublicDependencies( set, depend, depth + 1 );
                }
            }
            else
            {
                foreach ( var idx in node.Proto.public_dependency )
                {
                    var depend = protobufMap[ node.Proto.dependency[ idx ] ];
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

            for ( var i = 0; i < proto.dependency.Count; i++ )
            {
                var dependency = proto.dependency[ i ];
                var modifier = proto.public_dependency.Contains( i ) ? "public " : "";
                sb.AppendLine( $"import {modifier}\"{dependency}\";" );
                marker = true;
            }

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

            if ( !string.IsNullOrEmpty( proto.package ) )
                PopDescriptorName();
        }

        Dictionary<string, string> DumpOptions( FileDescriptorProto source, FileOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );
            if ( options.ShouldSerializeoptimize_for() )
                optionsKv.Add( "optimize_for", $"{options.optimize_for}" );
            if ( options.ShouldSerializecc_generic_services() )
                optionsKv.Add( "cc_generic_services", options.cc_generic_services ? "true" : "false" );
            if ( options.ShouldSerializego_package() )
                optionsKv.Add( "go_package", $"\"{options.go_package}\"" );
            if ( options.ShouldSerializejava_package() )
                optionsKv.Add( "java_package", $"\"{options.java_package}\"" );
            if ( options.ShouldSerializejava_outer_classname() )
                optionsKv.Add( "java_outer_classname", $"\"{options.java_outer_classname}\"" );
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

            DumpOptionsMatching( source, ".google.protobuf.FileOptions", options, optionsKv );

            return optionsKv;
        }

        Dictionary<string, string> DumpOptions( FileDescriptorProto source, FieldOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

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
            if ( options.ShouldSerializeexperimental_map_key() )
                optionsKv.Add( "experimental_map_key", $"\"{options.experimental_map_key}\"" );

            DumpOptionsMatching( source, ".google.protobuf.FieldOptions", options, optionsKv );

            return optionsKv;
        }

        Dictionary<string, string> DumpOptions( FileDescriptorProto source, MessageOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

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

        Dictionary<string, string> DumpOptions( FileDescriptorProto source, EnumOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializeallow_alias() )
                optionsKv.Add( "allow_alias", options.allow_alias ? "true" : "false" );
            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.EnumOptions", options, optionsKv );

            return optionsKv;
        }

        Dictionary<string, string> DumpOptions( FileDescriptorProto source, EnumValueOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.EnumValueOptions", options, optionsKv );

            return optionsKv;
        }


        Dictionary<string, string> DumpOptions( FileDescriptorProto source, ServiceOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.ServiceOptions", options, optionsKv );

            return optionsKv;
        }

        Dictionary<string, string> DumpOptions( FileDescriptorProto source, MethodOptions options )
        {
            var optionsKv = new Dictionary<string, string>();

            if ( options == null )
                return optionsKv;

            if ( options.ShouldSerializedeprecated() )
                optionsKv.Add( "deprecated", options.deprecated ? "true" : "false" );

            DumpOptionsMatching( source, ".google.protobuf.MethodOptions", options, optionsKv );

            return optionsKv;
        }

        void DumpOptionsFieldRecursive( FieldDescriptorProto field, IExtensible options, Dictionary<string, string> optionsKv, string path )
        {
            string key = string.IsNullOrEmpty( path ) ? $"({field.name})" : $"{path}.{field.name}";

            if ( IsNamedType( field.type ) && !string.IsNullOrEmpty( field.type_name ) )
            {
                var fieldData = protobufTypeMap[ field.type_name ].Source;

                if ( fieldData is EnumDescriptorProto enumProto )
                {
                    if ( Extensible.TryGetValue( options, field.number, out int idx ) )
                    {
                        var value = enumProto.value.Find( x => x.number == idx );

                        optionsKv.Add( key, value.name );
                    }
                }
                else if ( fieldData is DescriptorProto messageProto )
                {
                    ExtensionPlaceholder extension = Extensible.GetValue<ExtensionPlaceholder>( options, field.number );

                    if ( extension != null )
                    {
                        foreach ( var subField in messageProto.field )
                        {
                            DumpOptionsFieldRecursive( subField, extension, optionsKv, key );
                        }
                    }
                }
            }
            else
            {
                if ( ExtractType( options, field, out var value ) )
                {
                    optionsKv.Add( key, value );
                }
            }
        }

        void DumpOptionsMatching( FileDescriptorProto source, string typeName, IExtensible options, Dictionary<string, string> optionsKv )
        {
            var dependencies = new HashSet<FileDescriptorProto>( protobufMap[ source.name ].AllPublicDependencies );
            dependencies.Add( source );

            foreach ( var type in protobufTypeMap )
            {
                if ( dependencies.Contains( type.Value.Proto ) && type.Value.Source is FieldDescriptorProto field )
                {
                    if ( !string.IsNullOrEmpty( field.extendee ) && field.extendee == typeName )
                    {
                        DumpOptionsFieldRecursive( field, options, optionsKv, null );
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

            foreach ( var field in proto.nested_type )
            {
                DumpDescriptor( source, field, sb, level + 1, ref innerMarker );
            }

            foreach ( var field in proto.enum_type )
            {
                DumpEnumDescriptor( source, field, sb, level + 1, ref innerMarker );
            }

            var rootFields = proto.field.Where( x => !x.ShouldSerializeoneof_index() ).ToList();

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

                AppendHeadingSpace( sb, ref innerMarker );
                sb.AppendLine( $"{levelspace}\toneof {oneof.name} {{" );

                foreach ( var field in fields )
                {
                    sb.AppendLine(
                        $"{levelspace}\t\t{BuildDescriptorDeclaration( source, field, emitFieldLabel: false )}" );
                }

                sb.AppendLine( $"{levelspace}\t}}" );
                innerMarker = true;
            }

            foreach ( var range in proto.extension_range )
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

                AppendHeadingSpace( sb, ref innerMarker );
                sb.AppendLine( $"{levelspace}\textensions {range.start} to {max};" );
            }

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

            sb.AppendLine( $"{levelspace}}}" );
            marker = true;
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
                var declaration = $"\trpc {method.name} ({method.input_type}) returns ({method.output_type})";

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
            var options = new Dictionary<string, string>();

            if ( !string.IsNullOrEmpty( field.default_value ) )
            {
                var defaultValue = field.default_value;

                if ( field.type == FieldDescriptorProto.Type.TYPE_STRING )
                    defaultValue = $"\"{defaultValue}\"";

                options.Add( "default", defaultValue );
            }
            else if ( field.type == FieldDescriptorProto.Type.TYPE_ENUM && field.label != FieldDescriptorProto.Label.LABEL_REPEATED )
            {
                var lookup = protobufTypeMap[ field.type_name ];

                if ( lookup.Source is EnumDescriptorProto enumDescriptor && enumDescriptor.value.Count > 0 )
                    options.Add( "default", enumDescriptor.value[ 0 ].name );
            }

            var fieldOptions = DumpOptions( source, field.options );
            foreach ( var pair in fieldOptions )
            {
                options[ pair.Key ] = pair.Value;
            }

            var parameters = string.Empty;
            if ( options.Count > 0 )
            {
                parameters = $" [{string.Join( ", ", options.Select( kvp => $"{kvp.Key} = {kvp.Value}" ) )}]";
            }

            PopDescriptorName();

            var descriptorDeclarationBuilder = new StringBuilder();
            if ( emitFieldLabel )
            {
                descriptorDeclarationBuilder.Append( GetLabel( field.label ) );
                descriptorDeclarationBuilder.Append( " " );
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

        static string GetType( FieldDescriptorProto.Type type )
        {
            switch ( type )
            {
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
                default:
                    return type.ToString();
            }
        }

        static bool ExtractType( IExtensible data, FieldDescriptorProto field, out string value )
        {
            switch ( field.type )
            {
                case FieldDescriptorProto.Type.TYPE_INT32:
                case FieldDescriptorProto.Type.TYPE_UINT32:
                case FieldDescriptorProto.Type.TYPE_FIXED32:
                    if ( Extensible.TryGetValue( data, field.number, out uint int32 ) )
                    {
                        value = Convert.ToString( int32 );
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_INT64:
                case FieldDescriptorProto.Type.TYPE_UINT64:
                case FieldDescriptorProto.Type.TYPE_FIXED64:
                    if ( Extensible.TryGetValue( data, field.number, out ulong int64 ) )
                    {
                        value = Convert.ToString( int64 );
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_SINT32:
                case FieldDescriptorProto.Type.TYPE_SFIXED32:
                    if ( Extensible.TryGetValue( data, field.number, out int sint32 ) )
                    {
                        value = Convert.ToString( sint32 );
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_SINT64:
                case FieldDescriptorProto.Type.TYPE_SFIXED64:
                    if ( Extensible.TryGetValue( data, field.number, out long sint64 ) )
                    {
                        value = Convert.ToString( sint64 );
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_STRING:
                    if ( Extensible.TryGetValue( data, field.number, out string str ) )
                    {
                        value = $"\"{str}\"";
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_BOOL:
                    if ( Extensible.TryGetValue( data, field.number, out bool boolean ) )
                    {
                        value = boolean ? "true" : "false";
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_BYTES:
                    if ( Extensible.TryGetValue( data, field.number, out byte[] bytes ) )
                    {
                        value = Convert.ToString( bytes );
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_DOUBLE:
                    if ( Extensible.TryGetValue( data, field.number, out double dbl ) )
                    {
                        value = Convert.ToString( dbl, CultureInfo.InvariantCulture );
                        return true;
                    }
                    break;
                case FieldDescriptorProto.Type.TYPE_FLOAT:
                    if ( Extensible.TryGetValue( data, field.number, out float flt ) )
                    {
                        value = Convert.ToString( flt, CultureInfo.InvariantCulture );
                        return true;
                    }
                    break;
                default:
                    value = null;
                    return false;
            }

            value = null;
            return false;
        }

        static string ResolveType( FieldDescriptorProto field )
        {
            if ( IsNamedType( field.type ) )
            {
                return field.type_name;
            }

            return GetType( field.type );
        }

        void AppendHeadingSpace( StringBuilder sb, ref bool marker )
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
