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
        readonly bool modelAllMessages;

        /// <param name="descriptorSet">The descriptor set the generated files come from.</param>
        /// <param name="modelClassName">
        /// The full name of a partial <c>[ProtoModel]</c> class to register the generated types on, or <c>null</c> to not register them.
        /// </param>
        /// <param name="modelAllMessages">
        /// Whether to register every top-level message of the file, instead of only the request and response types of its services.
        /// </param>
        public SteamKitCSharpCodeGenerator( FileDescriptorSet descriptorSet, string modelClassName, bool modelAllMessages )
        {
            this.descriptorSet = descriptorSet;
            this.modelClassName = modelClassName;
            this.modelAllMessages = modelAllMessages;
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
            var partial = modelClassName != null ? " partial" : "";
            ctx.WriteLine( $"{GetAccess( GetAccess( service ) )}{partial} class {Escape( service.Name )} : SteamUnifiedMessages.UnifiedService" )
                .WriteLine( "{" )
                .Indent()
                .WriteLine( $"public override string ServiceName {{ get; }} = \"{Escape( service.Name )}\";" )
                .WriteLine();
        }

        protected override void WriteServiceFooter( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            var services = service.Methods
                .Where( static serviceMethod => serviceMethod.OutputType.AsSpan()[ 1.. ] is not "NoResponse")
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
                .Where( static serviceMethod => serviceMethod.OutputType.AsSpan()[ 1.. ] is "NoResponse")
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
        }

        protected override void WriteServiceMethod( GeneratorContext ctx, MethodDescriptorProto method, ref object state )
        {
            var modelArgument = modelClassName != null ? ", Model" : "";

            if ( method.OutputType.AsSpan()[ 1.. ] is "NoResponse" )
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
                WriteModelTypes( ctx, file );
            }

            base.WriteFileFooter( ctx, file, ref state );
        }

        void WriteModelTypes( GeneratorContext ctx, FileDescriptorProto file )
        {
            var globalTypeNames = GetGlobalTypeNames( ctx );
            var typeNames = new List<string>();

            if ( modelAllMessages )
            {
                var protoPrefix = GetProtoPrefix( file );

                foreach ( var message in file.MessageTypes )
                {
                    AddModelType( typeNames, globalTypeNames, $"{protoPrefix}.{message.Name}" );
                }
            }

            if ( ctx.EmitServices )
            {
                foreach ( var service in file.Services )
                {
                    foreach ( var method in service.Methods )
                    {
                        AddModelType( typeNames, globalTypeNames, method.InputType );

                        if ( method.OutputType.AsSpan()[ 1.. ] is not "NoResponse" )
                        {
                            AddModelType( typeNames, globalTypeNames, method.OutputType );
                        }
                    }
                }
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
