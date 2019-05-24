using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;

namespace ProtobufGen
{
    public class SteamKitCSharpCodeGenerator : CSharpCodeGenerator
    {
        public SteamKitCSharpCodeGenerator()
            : base()
        {
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
            ctx.WriteLine( $"{GetAccess( GetAccess( service ) )} interface I{Escape( service.Name )}" ).WriteLine( "{" ).Indent();
        }

        protected override void WriteServiceFooter( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            ctx.Outdent().WriteLine( "}" ).WriteLine();
        }

        protected override void WriteServiceMethod( GeneratorContext ctx, MethodDescriptorProto method, ref object state )
        {
            var outputType = MakeRelativeName( ctx, method.OutputType );
            var inputType = MakeRelativeName( ctx, method.InputType );

            ctx.WriteLine( $"{Escape( outputType )} {Escape( method.Name )}({Escape( inputType )} request);" );
        }
    }
}
