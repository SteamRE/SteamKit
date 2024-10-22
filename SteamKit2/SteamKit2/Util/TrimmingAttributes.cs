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
}
