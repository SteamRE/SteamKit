namespace System.Diagnostics.CodeAnalysis
{
    static class Trimming
    {
        // BUG BUG BUG: https://github.com/mono/linker/issues/2185
        // Ideally we should be able to list the same set at protobuf-net (all public-and non-public constructors, fields, method, and properties).
        // However since we have nested types, we also need to set public and non-public nested types.
        // In practice (at least as of .NET 6 Preview 6), the properties of the nested types get linked away. The implementation of the property getter
        // and setter is replaced with `throw new NotSupportedException("Linked away")`, which makes me wonder what on earth the linker was thinking, to
        // keep the type but not the implementation of any of its members. This then manifests itself at runtime in a trimmed application as:
        // Unhandled 'InvalidOperationException' exception from 'SteamFriends' handler: 'Cannot apply changes to property SteamKit2.Internal.CMsgClientFriendsList+Friend.ulfriendid'
        public const DynamicallyAccessedMemberTypes ForProtobufNet = DynamicallyAccessedMemberTypes.All;
    }

#if !NET5_0_OR_GREATER

    [Flags]
    internal enum DynamicallyAccessedMemberTypes
    {
        None = 0,
        PublicParameterlessConstructor = 0x0001,
        PublicConstructors = 0x0002 | PublicParameterlessConstructor,
        NonPublicConstructors = 0x0004,
        PublicMethods = 0x0008,
        NonPublicMethods = 0x0010,
        PublicFields = 0x0020,
        NonPublicFields = 0x0040,
        PublicNestedTypes = 0x0080,
        NonPublicNestedTypes = 0x0100,
        PublicProperties = 0x0200,
        NonPublicProperties = 0x0400,
        PublicEvents = 0x0800,
        NonPublicEvents = 0x1000,
        Interfaces = 0x2000,
        All = ~None
    }

    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false )]
    internal sealed class DynamicallyAccessedMembersAttribute : Attribute
    {
        public DynamicallyAccessedMembersAttribute( DynamicallyAccessedMemberTypes memberTypes )
        {
        }

        public DynamicallyAccessedMemberTypes MemberTypes { get; }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    internal sealed class DynamicDependencyAttribute : Attribute
    {
        public DynamicDependencyAttribute(string memberSignature )
        {
            MemberSignature = memberSignature;
        }

        public DynamicDependencyAttribute( DynamicallyAccessedMemberTypes memberTypes, Type type )
        {
            MemberTypes = memberTypes;
            Type = type;
        }

        public DynamicDependencyAttribute(string memberSignature, Type type)
        {
            MemberSignature = memberSignature;
            Type = type;
        }

        public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
        {
            MemberTypes = memberTypes;
            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
        {
            MemberSignature = memberSignature;
            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public string? AssemblyName { get; }
        public string? Condition { get; set; }
        public string? MemberSignature { get; }
        public DynamicallyAccessedMemberTypes MemberTypes { get; }
        public Type? Type { get; }
        public string? TypeName { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
    internal sealed class RequiresUnreferencedCodeAttribute : Attribute
    {
        public RequiresUnreferencedCodeAttribute(string message)
        {
            Message = message;
        }
 
        public string Message { get; }
        public string? Url { get; set; }
    }
#endif
}
