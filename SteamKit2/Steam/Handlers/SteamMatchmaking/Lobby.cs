using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamMatchmaking
    {
        /// <summary>
        /// Represents a Steam lobby.
        /// </summary>
        public sealed class Lobby
        {
            /// <summary>
            /// The lobby filter base class.
            /// </summary>
            public abstract class Filter
            {
                /// <summary>
                /// The type of filter.
                /// </summary>
                public ELobbyFilterType FilterType { get; }

                /// <summary>
                /// The metadata key this filter pertains to. Under certain circumstances e.g. a distance
                /// filter, this will be an empty string.
                /// </summary>
                public string Key { get; }

                /// <summary>
                /// The comparison method used by this filter.
                /// </summary>
                public ELobbyComparison Comparison { get; }

                /// <summary>
                /// Base constructor for all filter sub-classes.
                /// </summary>
                /// <param name="filterType">The type of filter.</param>
                /// <param name="key">The metadata key this filter pertains to.</param>
                /// <param name="comparison">The comparison method used by this filter.</param>
                protected Filter( ELobbyFilterType filterType, string key, ELobbyComparison comparison )
                {
                    FilterType = filterType;
                    Key = key;
                    Comparison = comparison;
                }

                /// <summary>
                /// Serializes the filter into a representation used internally by SteamMatchmaking.
                /// </summary>
                /// <returns>A protobuf serializable representation of this filter.</returns>
                public virtual CMsgClientMMSGetLobbyList.Filter Serialize()
                {
                    var filter = new CMsgClientMMSGetLobbyList.Filter();
                    filter.filter_type = ( int )FilterType;
                    filter.key = Key;
                    filter.comparision = ( int )Comparison;
                    return filter;
                }
            }

            /// <summary>
            /// Can be used to filter lobbies geographically (based on IP according to Steam's IP database).
            /// </summary>
            public sealed class DistanceFilter : Filter
            {
                /// <summary>
                /// Steam distance filter value.
                /// </summary>
                public ELobbyDistanceFilter Value { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="DistanceFilter"/> class.
                /// </summary>
                /// <param name="value">Steam distance filter value.</param>
                public DistanceFilter( ELobbyDistanceFilter value ) : base( ELobbyFilterType.Distance, "", ELobbyComparison.Equal )
                {
                    Value = value;
                }

                /// <summary>
                /// Serializes the distance filter into a representation used internally by SteamMatchmaking.
                /// </summary>
                /// <returns>A protobuf serializable representation of this filter.</returns>
                public override CMsgClientMMSGetLobbyList.Filter Serialize()
                {
                    var filter = base.Serialize();
                    filter.value = ( ( int )Value ).ToString();
                    return filter;
                }
            }

            /// <summary>
            /// Can be used to filter lobbies with a metadata value closest to the specified value. Multiple
            /// near filters can be specified, with former filters taking precedence over latter filters.
            /// </summary>
            public sealed class NearValueFilter : Filter
            {
                /// <summary>
                /// Integer value that lobbies' metadata value should be close to.
                /// </summary>
                public int Value { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="NearValueFilter"/> class.
                /// </summary>
                /// <param name="key">The metadata key this filter pertains to.</param>
                /// <param name="value">Integer value to compare against.</param>
                public NearValueFilter( string key, int value ) : base( ELobbyFilterType.NearValue, key, ELobbyComparison.Equal )
                {
                    Value = value;
                }

                /// <summary>
                /// Serializes the slots available filter into a representation used internally by SteamMatchmaking.
                /// </summary>
                /// <returns>A protobuf serializable representation of this filter.</returns>
                public override CMsgClientMMSGetLobbyList.Filter Serialize()
                {
                    var filter = base.Serialize();
                    filter.value = Value.ToString();
                    return filter;
                }
            }

            /// <summary>
            /// Can be used to filter lobbies by comparing an integer against a value in each lobby's metadata.
            /// </summary>
            public sealed class NumericalFilter : Filter
            {
                /// <summary>
                /// Integer value to compare against.
                /// </summary>
                public int Value { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="NumericalFilter"/> class.
                /// </summary>
                /// <param name="key">The metadata key this filter pertains to.</param>
                /// <param name="comparison">The comparison method used by this filter.</param>
                /// <param name="value">Integer value to compare against.</param>
                public NumericalFilter( string key, ELobbyComparison comparison, int value ) : base( ELobbyFilterType.Numerical, key, comparison )
                {
                    Value = value;
                }

                /// <summary>
                /// Serializes the numerical filter into a representation used internally by SteamMatchmaking.
                /// </summary>
                /// <returns>A protobuf serializable representation of this filter.</returns>
                public override CMsgClientMMSGetLobbyList.Filter Serialize()
                {
                    var filter = base.Serialize();
                    filter.value = Value.ToString();
                    return filter;
                }
            }

            /// <summary>
            /// Can be used to filter lobbies by minimum number of slots available.
            /// </summary>
            public sealed class SlotsAvailableFilter : Filter
            {
                /// <summary>
                /// Minumum number of slots available in the lobby.
                /// </summary>
                public int SlotsAvailable { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="SlotsAvailableFilter"/> class.
                /// </summary>
                /// <param name="slotsAvailable">Integer value to compare against.</param>
                public SlotsAvailableFilter( int slotsAvailable ) : base( ELobbyFilterType.SlotsAvailable, "", ELobbyComparison.Equal )
                {
                    SlotsAvailable = slotsAvailable;
                }

                /// <summary>
                /// Serializes the slots available filter into a representation used internally by SteamMatchmaking.
                /// </summary>
                /// <returns>A protobuf serializable representation of this filter.</returns>
                public override CMsgClientMMSGetLobbyList.Filter Serialize()
                {
                    var filter = base.Serialize();
                    filter.value = SlotsAvailable.ToString();
                    return filter;
                }
            }

            /// <summary>
            /// Can be used to filter lobbies by comparing a string against a value in each lobby's metadata.
            /// </summary>
            public sealed class StringFilter : Filter
            {
                /// <summary>
                /// String value to compare against.
                /// </summary>
                public string Value { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="StringFilter"/> class.
                /// </summary>
                /// <param name="key">The metadata key this filter pertains to.</param>
                /// <param name="comparison">The comparison method used by this filter.</param>
                /// <param name="value">String value to compare against.</param>
                public StringFilter( string key, ELobbyComparison comparison, string value ) : base( ELobbyFilterType.String, key, comparison )
                {
                    Value = value;
                }

                /// <summary>
                /// Serializes the string filter into a representation used internally by SteamMatchmaking.
                /// </summary>
                /// <returns>A protobuf serializable representation of this filter.</returns>
                public override CMsgClientMMSGetLobbyList.Filter Serialize()
                {
                    var filter = base.Serialize();
                    filter.value = Value;
                    return filter;
                }
            }

            /// <summary>
            /// Represents a Steam user within a lobby.
            /// </summary>
            public sealed class Member
            {
                /// <summary>
                /// SteamID of the lobby member.
                /// </summary>
                public SteamID SteamID { get; }

                /// <summary>
                /// Steam persona of the lobby member.
                /// </summary>
                public string PersonaName { get; }

                /// <summary>
                /// Metadata attached to the lobby member.
                /// </summary>
                public IReadOnlyDictionary<string, string> Metadata { get; }

                internal Member( SteamID steamId, string personaName, IReadOnlyDictionary<string, string>? metadata = null )
                {
                    SteamID = steamId;
                    PersonaName = personaName;
                    Metadata = metadata ?? EmptyMetadata;
                }

                /// <summary>
                /// Checks to see if this lobby member is equal to another. Only the SteamID of the lobby member is taken into account.
                /// </summary>
                /// <param name="obj"></param>
                /// <returns>true, if obj is <see cref="Member"/> with a matching SteamID. Otherwise, false.</returns>
                public override bool Equals( object obj )
                {
                    if ( obj is Member member )
                    {
                        return SteamID.Equals( member.SteamID );
                    }

                    return false;
                }

                /// <summary>
                /// Hash code of the lobby member. Only the SteamID of the lobby member is taken into account.
                /// </summary>
                /// <returns>The hash code of this lobby member.</returns>
                public override int GetHashCode()
                {
                    return SteamID.GetHashCode();
                }
            }

            /// <summary>
            /// SteamID of the lobby.
            /// </summary>
            public SteamID SteamID { get; }

            /// <summary>
            /// The type of the lobby.
            /// </summary>
            public ELobbyType LobbyType { get; }

            /// <summary>
            /// The lobby's flags.
            /// </summary>
            public int LobbyFlags { get; }

            /// <summary>
            /// The SteamID of the lobby's owner. Please keep in mind that Steam does not provide lobby
            /// owner details for lobbies returned in a lobby list. As such, lobbies that have been
            /// obtained/updated as a result of calling <see cref="SteamMatchmaking.GetLobbyList"/>
            /// may have a null (or non-null but state) owner.
            /// </summary>
            public SteamID? OwnerSteamID { get; }

            /// <summary>
            /// The metadata of the lobby; string key-value pairs.
            /// </summary>
            public IReadOnlyDictionary<string, string> Metadata { get; }

            /// <summary>
            /// The maximum number of members that can occupy the lobby.
            /// </summary>
            public int MaxMembers { get; }

            /// <summary>
            /// The number of members that are currently occupying the lobby.
            /// </summary>
            public int NumMembers { get; }

            /// <summary>
            /// A list of lobby members. This will only be populated for the user's current lobby.
            /// </summary>
            public IReadOnlyList<Member> Members { get; }

            /// <summary>
            /// The distance of the lobby.
            /// </summary>
            public float? Distance { get; }

            /// <summary>
            /// The weight of the lobby.
            /// </summary>
            public long? Weight { get; }

            static readonly ReadOnlyDictionary<string, string> EmptyMetadata =
                new ReadOnlyDictionary<string, string>( new Dictionary<string, string>() );

            static readonly IReadOnlyList<Member> EmptyMembers = Array.AsReadOnly(Array.Empty<Member>());

            internal Lobby( SteamID steamId, ELobbyType lobbyType, int lobbyFlags, SteamID? ownerSteamId, IReadOnlyDictionary<string, string>? metadata,
                int maxMembers, int numMembers, IReadOnlyList<Member>? members, float? distance, long? weight )
            {
                SteamID = steamId;
                LobbyType = lobbyType;
                LobbyFlags = lobbyFlags;
                OwnerSteamID = ownerSteamId;
                Metadata = metadata ?? EmptyMetadata;
                MaxMembers = maxMembers;
                NumMembers = numMembers;
                Members = members ?? EmptyMembers;
                Distance = distance;
                Weight = weight;
            }

            internal static byte[] EncodeMetadata( IReadOnlyDictionary<string, string>? metadata )
            {
                var keyValue = new KeyValue( "" );

                if ( metadata != null )
                {
                    foreach ( var entry in metadata )
                    {
                        keyValue[ entry.Key ] = new KeyValue( null, entry.Value );
                    }
                }

                using ( var ms = new MemoryStream() )
                {
                    keyValue.SaveToStream( ms, true );
                    return ms.ToArray();
                }
            }

            internal static ReadOnlyDictionary<string, string> DecodeMetadata( byte[]? buffer )
            {
                if ( buffer == null || buffer.Length == 0 )
                {
                    return EmptyMetadata;
                }

                var keyValue = new KeyValue();

                using ( var ms = new MemoryStream( buffer ) )
                {
                    if ( !keyValue.TryReadAsBinary( ms ) )
                    {
                        throw new FormatException( "Lobby metadata is of an unexpected format" );
                    }
                }

                var metadata = new Dictionary<string, string>();

                foreach ( var value in keyValue.Children )
                {
                    if (value.Name is null || value.Value is null)
                    {
                        continue;
                    }

                    metadata[ value.Name ] = value.Value;
                }

                return new ReadOnlyDictionary<string, string>( metadata );
            }
        }
    }
}
