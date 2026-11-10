using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;

namespace ProtobufGen
{
    class SteamKitCSharpCodeGenerator : CSharpCodeGenerator
    {
        readonly FileDescriptorSet descriptorSet;
        readonly string modelClassName;
        readonly bool serviceModels;
        Dictionary<string, string> globalTypeNames;

        /// <param name="descriptorSet">The descriptor set the generated files come from.</param>
        /// <param name="modelClassName">
        /// The full name of a partial <c>[ProtoModel]</c> class to register every top-level message of the file on,
        /// or <c>null</c> to not register them.
        /// </param>
        /// <param name="serviceModels">
        /// Whether to generate a <c>[ProtoModel]</c> for each service with its request and response types,
        /// and return it from the service's <c>Model</c>.
        /// </param>
        public SteamKitCSharpCodeGenerator( FileDescriptorSet descriptorSet, string modelClassName, bool serviceModels )
        {
            this.descriptorSet = descriptorSet;
            this.modelClassName = modelClassName;
            this.serviceModels = serviceModels;
        }

        protected override bool UseArray( FieldDescriptorProto field ) => false;

        protected override void WriteExtension( GeneratorContext ctx, FieldDescriptorProto field )
        {
        }

        protected override void WriteExtensionsHeader( GeneratorContext ctx, DescriptorProto message, ref object state )
        {
        }

        protected override void WriteExtensionsHeader( GeneratorContext ctx, FileDescriptorProto file, ref object state )
        {
        }

        protected override void WriteExtensionsFooter( GeneratorContext ctx, DescriptorProto message, ref object state )
        {
        }

        protected override void WriteExtensionsFooter( GeneratorContext ctx, FileDescriptorProto file, ref object state )
        {
        }

        /// <summary>
        /// Starts a service block
        /// </summary>
        protected override void WriteServiceHeader( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            state = service.Name;
            ctx.WriteLine( $"{GetAccess( GetAccess( service ) )} class {Escape( service.Name )} : SteamUnifiedMessages.UnifiedService" )
                .WriteLine( "{" )
                .Indent()
                .WriteLine( $"public override string ServiceName {{ get; }} = \"{Escape( service.Name )}\";" )
                .WriteLine();

            if ( serviceModels )
            {
                ctx.WriteLine( $"public override global::ProtoBuf.Meta.TypeModel Model => {GetServiceModelName( service )}.Instance;" )
                    .WriteLine();
            }
        }

        protected override void WriteServiceFooter( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            var services = service.Methods
                .Where( static serviceMethod => serviceMethod.OutputType.AsSpan( 1 ) is not "NoResponse")
                .ToList();

            ctx.WriteLine( "public override void HandleResponseMsg( string methodName, PacketClientMsgProtobuf packetMsg )" )
                .WriteLine( "{" );

            if ( services.Count != 0 )
            {
                ctx.Indent()
                    .WriteLine( "switch ( methodName )" )
                    .WriteLine( "{" )
                    .Indent();

                foreach ( var serviceMethod in services )
                {
                    ctx.WriteLine( $"case \"{Escape( serviceMethod.Name )}\":" )
                        .Indent()
                        .WriteLine( $"PostResponseMsg<{Escape( serviceMethod.OutputType[ 1.. ] )}>( packetMsg );" )
                        .WriteLine( "break;" )
                        .Outdent();
                }

                ctx.Outdent()
                    .WriteLine( "}" )
                    .Outdent();
            }

            ctx.WriteLine( "}" )
                .WriteLine();

            var notifications = service.Methods
                .Where( static serviceMethod => serviceMethod.OutputType.AsSpan( 1 ) is "NoResponse")
                .ToList();

            ctx.WriteLine( "public override void HandleNotificationMsg( string methodName, PacketClientMsgProtobuf packetMsg )" )
                .WriteLine( "{" );

            if ( notifications.Count != 0 )
            {
                ctx.Indent()
                    .WriteLine( "switch ( methodName )" )
                    .WriteLine( "{" )
                    .Indent();

                foreach ( var notificationMethod in notifications )
                {
                    ctx.WriteLine( $"case \"{Escape( notificationMethod.Name )}\":" )
                        .Indent()
                        .WriteLine( $"PostNotificationMsg<{Escape( notificationMethod.InputType[ 1.. ] )}>( packetMsg );" )
                        .WriteLine( "break;" )
                        .Outdent();
                }

                ctx.Outdent()
                    .WriteLine( "}" )
                    .Outdent();
            }

            ctx.WriteLine( "}" )
                .Outdent()
                .WriteLine( "}" )
                .WriteLine();

            if ( serviceModels )
            {
                WriteServiceModel( ctx, service );
            }
        }

        void WriteServiceModel( GeneratorContext ctx, ServiceDescriptorProto service )
        {
            globalTypeNames ??= GetGlobalTypeNames( ctx );
            var typeNames = new List<string>();

            foreach ( var method in service.Methods )
            {
                AddModelType( typeNames, globalTypeNames, method.InputType );

                if ( method.OutputType.AsSpan( 1 ) is not "NoResponse" )
                {
                    AddModelType( typeNames, globalTypeNames, method.OutputType );
                }
            }

            ctx.WriteLine( "#pragma warning disable PBN9001" )
                .WriteLine( "[global::ProtoBuf.ProtoModel]" );

            foreach ( var typeName in typeNames )
            {
                ctx.WriteLine( $"[global::ProtoBuf.ProtoSerializable(typeof({typeName}))]" );
            }

            ctx.WriteLine( $"internal sealed partial class {GetServiceModelName( service )} : global::ProtoBuf.Meta.TypeModel" )
                .WriteLine( "{" )
                .WriteLine( "}" )
                .WriteLine( "#pragma warning restore PBN9001" )
                .WriteLine();
        }

        string GetServiceModelName( ServiceDescriptorProto service ) => Escape( $"{service.Name}ProtobufModel" );

        protected override void WriteServiceMethod( GeneratorContext ctx, MethodDescriptorProto method, ref object state )
        {
            var modelArgument = serviceModels ? ", Model" : "";

            if ( method.OutputType.AsSpan( 1 ) is "NoResponse" )
            {
                ctx.WriteLine( $"public void {Escape( method.Name )}({Escape( MakeRelativeName( ctx, method.InputType ) )} request )" )
                    .WriteLine( "{" )
                    .Indent()
                    .WriteLine( $"UnifiedMessages.SendNotification<{Escape( MakeRelativeName( ctx, method.InputType ) )}>( \"{Escape( state as string )}.{Escape( method.Name )}#1\", request{modelArgument} );" )
                    .Outdent()
                    .WriteLine( "}" )
                    .WriteLine();
            }
            else
            {
                ctx.WriteLine( $"public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<{Escape( method.OutputType[ 1.. ] )}>> {Escape( method.Name )}( {Escape( MakeRelativeName( ctx, method.InputType ) )} request )" )
                    .WriteLine( "{" )
                    .Indent()
                    .WriteLine( $"return UnifiedMessages.SendMessage<{Escape( MakeRelativeName( ctx, method.InputType ) )}, {Escape( method.OutputType[1..] )}>( \"{Escape( state as string )}.{Escape( method.Name )}#1\", request{modelArgument} );" )
                    .Outdent()
                    .WriteLine( "}" )
                    .WriteLine();
            }
        }

        protected override void WriteFileFooter( GeneratorContext ctx, FileDescriptorProto file, ref object state )
        {
            if ( modelClassName != null )
            {
                WriteModelMessages( ctx, file );
            }

            base.WriteFileFooter( ctx, file, ref state );
        }

        void WriteModelMessages( GeneratorContext ctx, FileDescriptorProto file )
        {
            globalTypeNames ??= GetGlobalTypeNames( ctx );
            var typeNames = new List<string>();
            var protoPrefix = GetProtoPrefix( file );

            foreach ( var message in file.MessageTypes )
            {
                AddModelType( typeNames, globalTypeNames, $"{protoPrefix}.{message.Name}" );
            }

            if ( typeNames.Count == 0 )
            {
                return;
            }

            var lastDot = modelClassName.LastIndexOf( '.' );
            var modelNamespace = modelClassName[ ..lastDot ];
            var modelName = modelClassName[ ( lastDot + 1 ).. ];

            ctx.WriteLine( "#pragma warning disable PBN9001" )
                .WriteLine( $"namespace {modelNamespace}" )
                .WriteLine( "{" )
                .Indent();

            foreach ( var typeName in typeNames )
            {
                ctx.WriteLine( $"[global::ProtoBuf.ProtoSerializable(typeof({typeName}))]" );
            }

            ctx.WriteLine( $"partial class {modelName}" )
                .WriteLine( "{" )
                .WriteLine( "}" )
                .Outdent()
                .WriteLine( "}" )
                .WriteLine( "#pragma warning restore PBN9001" )
                .WriteLine();
        }

        static void AddModelType( List<string> typeNames, Dictionary<string, string> globalTypeNames, string protoTypeName )
        {
            if ( !globalTypeNames.TryGetValue( protoTypeName, out var typeName ) )
            {
                throw new InvalidOperationException( $"Could not find message type '{protoTypeName}'." );
            }

            if ( !typeNames.Contains( typeName ) )
            {
                typeNames.Add( typeName );
            }
        }

        static string GetProtoPrefix( FileDescriptorProto file ) => string.IsNullOrEmpty( file.Package ) ? "" : $".{file.Package}";

        /// <summary>
        /// Maps the fully qualified protobuf name of every message in the descriptor set to its C# name.
        /// </summary>
        Dictionary<string, string> GetGlobalTypeNames( GeneratorContext ctx )
        {
            var globalTypeNames = new Dictionary<string, string>();

            foreach ( var file in descriptorSet.Files )
            {
                var protoPrefix = GetProtoPrefix( file );
                var fileNamespace = ctx.NameNormalizer.GetName( file );

                foreach ( var message in file.MessageTypes )
                {
                    var @namespace = message.Options?.GetOptions()?.Namespace;

                    if ( string.IsNullOrWhiteSpace( @namespace ) )
                    {
                        @namespace = fileNamespace;
                    }

                    var typePrefix = string.IsNullOrWhiteSpace( @namespace ) ? "global::" : $"global::{@namespace}.";

                    AddGlobalTypeNames( ctx, globalTypeNames, message, protoPrefix, typePrefix );
                }
            }

            return globalTypeNames;
        }

        void AddGlobalTypeNames( GeneratorContext ctx, Dictionary<string, string> globalTypeNames, DescriptorProto message, string protoPrefix, string typePrefix )
        {
            var protoName = $"{protoPrefix}.{message.Name}";
            var typeName = typePrefix + Escape( ctx.NameNormalizer.GetName( message ) );

            globalTypeNames[ protoName ] = typeName;

            foreach ( var nestedMessage in message.NestedTypes )
            {
                AddGlobalTypeNames( ctx, globalTypeNames, nestedMessage, protoName, $"{typeName}." );
            }
        }
    }
}
