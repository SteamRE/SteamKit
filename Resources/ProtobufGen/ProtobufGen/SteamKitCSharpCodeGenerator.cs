using System;
using System.Collections.Generic;
using System.Text;
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

        protected override void WriteExtension( GeneratorContext ctx, FieldDescriptorProto field )
        {
            return;
        }

        protected override void WriteExtensionsHeader( GeneratorContext ctx, DescriptorProto message, ref object state )
        {
            return;
        }

        protected override void WriteExtensionsHeader( GeneratorContext ctx, FileDescriptorProto file, ref object state )
        {
            return;
        }

        protected override void WriteExtensionsFooter( GeneratorContext ctx, DescriptorProto message, ref object state )
        {
            return;
        }

        protected override void WriteExtensionsFooter( GeneratorContext ctx, FileDescriptorProto file, ref object state )
        {
            return;
        }

        /// <summary>
        /// Starts a service block
        /// </summary>
        protected override void WriteServiceHeader( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            var name = service.Name;
            ctx.WriteLine( $"{GetAccess( GetAccess( service ) )} interface I{Escape( name )}" ).WriteLine( "{" ).Indent();
        }

        protected override void WriteServiceFooter( GeneratorContext ctx, ServiceDescriptorProto service, ref object state )
        {
            ctx.Outdent().WriteLine( "}" ).WriteLine();
        }

        protected override void WriteServiceMethod( GeneratorContext ctx, MethodDescriptorProto method, ref object state )
        {
            string MakeRelativeName( string typeName )
            {
                var target = ctx.TryFind<DescriptorProto>( typeName );
                if ( target != null )
                {
                    var declaringType = target.AsIType().GetParent();

                    if ( declaringType.IsIType() )
                    {
                        var name = FindNameFromCommonAncestor( declaringType.AsIType(), target.AsIType(), ctx.NameNormalizer );
                        if ( !string.IsNullOrWhiteSpace( name ) )
                        {
                            return name;
                        }
                    }
                }
                return Escape( typeName );
            }

            var outputType = MakeRelativeName( method.OutputType );
            var inputType = MakeRelativeName( method.InputType );

            ctx.WriteLine( $"{Escape( outputType )} {Escape( method.Name )}({Escape( inputType )} request);" );
        }

        protected Access GetAccess( ServiceDescriptorProto obj )
            => NullIfInherit( obj?.Options?.GetOptions()?.Access ) ?? Access.Public;

        //
        // Copied from base class
        //
        private static Access? NullIfInherit( Access? access )
            => access == Access.Inherit ? null : access;

        //
        // Copied from base class with reflection hacks to access IType.
        //
        private string FindNameFromCommonAncestor( ITypeWrapper declaring, ITypeWrapper target, NameNormalizer normalizer )
        {
            // trivial case; asking for self, or asking for immediate child
            if ( ReferenceEquals( declaring.Value, target.Value ) || ReferenceEquals( declaring.Value, target.GetParent() ) )
            {
                if ( target.Value is DescriptorProto message ) return Escape( normalizer.GetName( message ) );
                if ( target.Value is EnumDescriptorProto @enum ) return Escape( normalizer.GetName( @enum ) );
                return null;
            }

            var origTarget = target;
            var xStack = new Stack<ITypeWrapper>();

            while ( declaring.Value != null )
            {
                xStack.Push( declaring );
                declaring = declaring.GetParent().AsIType();
            }
            var yStack = new Stack<ITypeWrapper>();

            while ( target.Value != null )
            {
                yStack.Push( target );
                target = target.GetParent().AsIType();
            }
            int lim = Math.Min( xStack.Count, yStack.Count );
            for ( int i = 0; i < lim; i++ )
            {
                declaring = xStack.Peek();
                target = yStack.Pop();
                if ( !ReferenceEquals( target.Value, declaring.Value ) )
                {
                    // special-case: if both are the package (file), and they have the same namespace: we're OK
                    if ( target.Value is FileDescriptorProto && declaring.Value is FileDescriptorProto
                        && normalizer.GetName( ( FileDescriptorProto )declaring.Value ) == normalizer.GetName( ( FileDescriptorProto )target.Value ) )
                    {
                        // that's fine, keep going
                    }
                    else
                    {
                        // put it back
                        yStack.Push( target );
                        break;
                    }
                }
            }
            // if we used everything, then the target is an ancestor-or-self
            if ( yStack.Count == 0 )
            {
                target = origTarget;
                if ( target.Value is DescriptorProto message ) return Escape( normalizer.GetName( message ) );
                if ( target.Value is EnumDescriptorProto @enum ) return Escape( normalizer.GetName( @enum ) );
                return null;
            }

            var sb = new StringBuilder();
            while ( yStack.Count != 0 )
            {
                target = yStack.Pop();

                string nextName;
                if ( target.Value is FileDescriptorProto file ) nextName = normalizer.GetName( file );
                else if ( target.Value is DescriptorProto message ) nextName = normalizer.GetName( message );
                else if ( target.Value is EnumDescriptorProto @enum ) nextName = normalizer.GetName( @enum );
                else return null;

                if ( !string.IsNullOrWhiteSpace( nextName ) )
                {
                    if ( sb.Length == 0 && target.Value is FileDescriptorProto ) sb.Append( "global::" );
                    else if ( sb.Length != 0 ) sb.Append( '.' );
                    sb.Append( Escape( nextName ) );
                }
            }
            return sb.ToString();
        }
    }
}
