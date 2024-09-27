using System.Linq;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;

namespace ProtobufGen
{
    class SteamKitCSharpCodeGenerator : CSharpCodeGenerator
    {
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
        }

        protected override void WriteServiceFooter( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            ctx.WriteLine( "public override void HandleMsg( string methodName, IPacketMsg packetMsg )" )
                .WriteLine( "{" );

                var methods = service.Methods
                    .Where( static serviceMethod => serviceMethod.OutputType[ 1.. ] != "NoResponse")
                    .ToList();

                if ( methods.Count != 0 )
                {
                    ctx.Indent()
                        .WriteLine( "switch ( methodName )" )
                        .WriteLine( "{" )
                        .Indent();

                    foreach ( var serviceMethod in methods )
                    {
                        ctx.WriteLine( $"case \"{Escape( serviceMethod.Name )}\":" )
                            .Indent()
                            .WriteLine( $"UnifiedMessages.HandleServiceMsg<{Escape( serviceMethod.OutputType[ 1.. ] )}>( packetMsg );" )
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
            if ( method.OutputType[ 1.. ] == "NoResponse" )
            {
                ctx.WriteLine( $"public void {Escape( method.Name )}({Escape( MakeRelativeName( ctx, method.InputType ) )} request)" )
                    .WriteLine( "{" )
                    .Indent()
                    .WriteLine( $"UnifiedMessages.SendNotification<{Escape( MakeRelativeName( ctx, method.InputType ) )}>( \"{Escape( state as string )}.{Escape( method.Name )}#1\", request );" )
                    .Outdent()
                    .WriteLine( "}" )
                    .WriteLine();
            }
            else
            {
                ctx.WriteLine( $"public AsyncJob<SteamUnifiedMessages.ServiceMsg<{Escape( method.OutputType[ 1.. ] )}>> {Escape( method.Name )}({Escape( MakeRelativeName( ctx, method.InputType ) )} request)" )
                    .WriteLine( "{" )
                    .Indent()
                    .WriteLine( $"return UnifiedMessages.SendMessage<{Escape( MakeRelativeName( ctx, method.InputType ) )}, {Escape( method.OutputType[1..] )}>( \"{Escape( state as string )}.{Escape( method.Name )}#1\", request );" )
                    .Outdent()
                    .WriteLine( "}" )
                    .WriteLine();
            }
        }
    }
}
