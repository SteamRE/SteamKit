/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using ProtoBuf;
using ProtoBuf.Meta;

namespace SteamKit2.Internal
{
    /// <summary>
    /// Provides access to the protobuf model that SteamKit uses to serialize its messages.
    /// </summary>
    public static class SteamKitProtobuf
    {
        /// <summary>
        /// Gets the compile-time protobuf model for SteamKit's messages. It contains every message SteamKit sends or receives itself.
        /// Each generated unified service has a model of its own for its requests and responses, see
        /// <see cref="SteamUnifiedMessages.UnifiedService.Model"/>. It does not contain game coordinator messages;
        /// declare your own model with <c>[ProtoModel]</c> for those.
        /// </summary>
        public static TypeModel Model => SteamKitProtobufModel.Instance;
    }

    // Most serializable types are listed by [ProtoSerializable] attributes emitted into the generated protobuf files.
#pragma warning disable PBN9001
    [ProtoModel]
    [ProtoSerializable( typeof( BasicServerList ) )]
    [ProtoSerializable( typeof( BasicServerListProto ) )]
    [ProtoSerializable( typeof( ParentalSettings ) )]
    internal sealed partial class SteamKitProtobufModel : TypeModel
    {
    }
#pragma warning restore PBN9001
}
