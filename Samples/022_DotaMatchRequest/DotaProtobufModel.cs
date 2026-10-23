using ProtoBuf;
using ProtoBuf.Meta;
using SteamKit2.GC.Dota.Internal;

namespace DotaMatchRequest;

// SteamKitProtobuf.Model does not contain game coordinator messages, so we declare our own protobuf model
// that lists every GC message this sample sends or receives. The model is generated at compile time,
// which keeps the sample compatible with trimming and native AOT.
#pragma warning disable PBN9001 // ProtoModel and ProtoSerializable are experimental
[ProtoModel]
[ProtoSerializable( typeof( CMsgClientHello ) )]
[ProtoSerializable( typeof( CMsgClientWelcome ) )]
[ProtoSerializable( typeof( CMsgGCMatchDetailsRequest ) )]
[ProtoSerializable( typeof( CMsgGCMatchDetailsResponse ) )]
partial class DotaProtobufModel : TypeModel
{
}
#pragma warning restore PBN9001
