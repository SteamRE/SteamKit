using System;
using System.Linq;
using System.Reflection;

namespace ProtobufGen
{
    struct ITypeWrapper
    {
        public ITypeWrapper( object value )
        {
            Value = value;
        }

        public object Value { get; }

        public object GetParent() => IType_get_Parent.Invoke( Value, Array.Empty<object>() );

        internal static Type TypeOfIType { get; } = Type.GetType( "Google.Protobuf.Reflection.IType, protobuf-net.Reflection", throwOnError: true );

        static MethodInfo IType_get_Parent { get; } = GetITypeParentMember();

        static MethodInfo GetITypeParentMember()
        {
            var member = TypeOfIType.GetProperty( "Parent", BindingFlags.Public | BindingFlags.Instance );
            return member.GetMethod;
        }
    }

    static class ITypeExtensions
    {
        public static ITypeWrapper AsIType( this object o ) => new ITypeWrapper( o );
        public static bool IsIType( this object o ) => o != null && ITypeWrapper.TypeOfIType.IsAssignableFrom( o.GetType() );
    }
}
