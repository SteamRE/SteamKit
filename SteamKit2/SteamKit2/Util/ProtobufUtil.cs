/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace SteamKit2
{
    static class ProtobufUtil
    {
        /// <summary>
        /// The members protobuf-net accesses on a contract type.
        /// </summary>
        /// <remarks>
        /// protobuf-net's own annotation also lists nested types, but it never reflects over them. Keeping nested types
        /// keeps them with all their members, so a nested enum keeps <see cref="Enum.GetValues(Type)"/> and native AOT
        /// warns about it wherever a message type with a nested enum is used. Generic code annotated with this
        /// calls protobuf-net through the methods of this class, which bridge the two annotations.
        /// </remarks>
        public const DynamicallyAccessedMemberTypes ContractMembers =
            DynamicallyAccessedMemberTypes.PublicConstructors |
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.NonPublicMethods |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.NonPublicFields |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties;

        const string NestedTypesJustification = "protobuf-net never reflects over the nested types of a contract type; ContractMembers keeps every other member its annotation asks for.";

        public const string RequiresModelMessage = "Without a TypeModel, messages are serialized with protobuf-net's reflection-based RuntimeTypeModel, which is not compatible with trimming or native AOT. Use an overload that takes a TypeModel, passing SteamKitProtobuf.Model or your own [ProtoModel].";

        /// <summary>
        /// Serializes a message to the stream.
        /// </summary>
        [UnconditionalSuppressMessage( "Trimming", "IL2091", Justification = NestedTypesJustification )]
        public static void Serialize<[DynamicallyAccessedMembers( ContractMembers )] T>( TypeModel model, Stream stream, T value )
            => model.Serialize( stream, value );

        /// <summary>
        /// Deserializes a message from the rest of the stream.
        /// </summary>
        [UnconditionalSuppressMessage( "Trimming", "IL2091", Justification = NestedTypesJustification )]
        public static T Deserialize<[DynamicallyAccessedMembers( ContractMembers )] T>( TypeModel model, Stream stream )
            => model.Deserialize<T>( stream );

        /// <summary>
        /// Deserializes a message that occupies exactly <paramref name="length"/> bytes of the stream.
        /// </summary>
        [UnconditionalSuppressMessage( "Trimming", "IL2091", Justification = NestedTypesJustification )]
        public static T Deserialize<[DynamicallyAccessedMembers( ContractMembers )] T>( TypeModel model, Stream stream, int length )
        {
            ArgumentOutOfRangeException.ThrowIfNegative( length );

            using var state = ProtoReader.State.Create( stream, model, userState: null, length );
            return state.DeserializeRoot<T>();
        }
    }
}
