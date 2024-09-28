// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: service_userreviews.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.WebUI.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_GetFriendsRecommendedApp_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint appid
        {
            get => __pbn__appid.GetValueOrDefault();
            set => __pbn__appid = value;
        }
        public bool ShouldSerializeappid() => __pbn__appid != null;
        public void Resetappid() => __pbn__appid = null;
        private uint? __pbn__appid;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_GetFriendsRecommendedApp_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<uint> accountids_recommended { get; } = new global::System.Collections.Generic.List<uint>();

        [global::ProtoBuf.ProtoMember(3)]
        public global::System.Collections.Generic.List<uint> accountids_not_recommended { get; } = new global::System.Collections.Generic.List<uint>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_GetIndividualRecommendations_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<CUserReviews_GetIndividualRecommendations_Request_RecommendationRequest> requests { get; } = new global::System.Collections.Generic.List<CUserReviews_GetIndividualRecommendations_Request_RecommendationRequest>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_GetIndividualRecommendations_Request_RecommendationRequest : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong steamid
        {
            get => __pbn__steamid.GetValueOrDefault();
            set => __pbn__steamid = value;
        }
        public bool ShouldSerializesteamid() => __pbn__steamid != null;
        public void Resetsteamid() => __pbn__steamid = null;
        private ulong? __pbn__steamid;

        [global::ProtoBuf.ProtoMember(2)]
        public uint appid
        {
            get => __pbn__appid.GetValueOrDefault();
            set => __pbn__appid = value;
        }
        public bool ShouldSerializeappid() => __pbn__appid != null;
        public void Resetappid() => __pbn__appid = null;
        private uint? __pbn__appid;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_GetIndividualRecommendations_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<RecommendationDetails> recommendations { get; } = new global::System.Collections.Generic.List<RecommendationDetails>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_Recommendation_LoyaltyReaction : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint reaction_type
        {
            get => __pbn__reaction_type.GetValueOrDefault();
            set => __pbn__reaction_type = value;
        }
        public bool ShouldSerializereaction_type() => __pbn__reaction_type != null;
        public void Resetreaction_type() => __pbn__reaction_type = null;
        private uint? __pbn__reaction_type;

        [global::ProtoBuf.ProtoMember(2)]
        public uint count
        {
            get => __pbn__count.GetValueOrDefault();
            set => __pbn__count = value;
        }
        public bool ShouldSerializecount() => __pbn__count != null;
        public void Resetcount() => __pbn__count = null;
        private uint? __pbn__count;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_Update_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong recommendationid
        {
            get => __pbn__recommendationid.GetValueOrDefault();
            set => __pbn__recommendationid = value;
        }
        public bool ShouldSerializerecommendationid() => __pbn__recommendationid != null;
        public void Resetrecommendationid() => __pbn__recommendationid = null;
        private ulong? __pbn__recommendationid;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string review_text
        {
            get => __pbn__review_text ?? "";
            set => __pbn__review_text = value;
        }
        public bool ShouldSerializereview_text() => __pbn__review_text != null;
        public void Resetreview_text() => __pbn__review_text = null;
        private string __pbn__review_text;

        [global::ProtoBuf.ProtoMember(3)]
        public bool voted_up
        {
            get => __pbn__voted_up.GetValueOrDefault();
            set => __pbn__voted_up = value;
        }
        public bool ShouldSerializevoted_up() => __pbn__voted_up != null;
        public void Resetvoted_up() => __pbn__voted_up = null;
        private bool? __pbn__voted_up;

        [global::ProtoBuf.ProtoMember(4)]
        public bool is_public
        {
            get => __pbn__is_public.GetValueOrDefault();
            set => __pbn__is_public = value;
        }
        public bool ShouldSerializeis_public() => __pbn__is_public != null;
        public void Resetis_public() => __pbn__is_public = null;
        private bool? __pbn__is_public;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string language
        {
            get => __pbn__language ?? "";
            set => __pbn__language = value;
        }
        public bool ShouldSerializelanguage() => __pbn__language != null;
        public void Resetlanguage() => __pbn__language = null;
        private string __pbn__language;

        [global::ProtoBuf.ProtoMember(6)]
        public bool is_in_early_access
        {
            get => __pbn__is_in_early_access.GetValueOrDefault();
            set => __pbn__is_in_early_access = value;
        }
        public bool ShouldSerializeis_in_early_access() => __pbn__is_in_early_access != null;
        public void Resetis_in_early_access() => __pbn__is_in_early_access = null;
        private bool? __pbn__is_in_early_access;

        [global::ProtoBuf.ProtoMember(7)]
        public bool received_compensation
        {
            get => __pbn__received_compensation.GetValueOrDefault();
            set => __pbn__received_compensation = value;
        }
        public bool ShouldSerializereceived_compensation() => __pbn__received_compensation != null;
        public void Resetreceived_compensation() => __pbn__received_compensation = null;
        private bool? __pbn__received_compensation;

        [global::ProtoBuf.ProtoMember(8)]
        public bool comments_disabled
        {
            get => __pbn__comments_disabled.GetValueOrDefault();
            set => __pbn__comments_disabled = value;
        }
        public bool ShouldSerializecomments_disabled() => __pbn__comments_disabled != null;
        public void Resetcomments_disabled() => __pbn__comments_disabled = null;
        private bool? __pbn__comments_disabled;

        [global::ProtoBuf.ProtoMember(9)]
        public bool hide_in_steam_china
        {
            get => __pbn__hide_in_steam_china.GetValueOrDefault();
            set => __pbn__hide_in_steam_china = value;
        }
        public bool ShouldSerializehide_in_steam_china() => __pbn__hide_in_steam_china != null;
        public void Resethide_in_steam_china() => __pbn__hide_in_steam_china = null;
        private bool? __pbn__hide_in_steam_china;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CUserReviews_Update_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RecommendationDetails : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong recommendationid
        {
            get => __pbn__recommendationid.GetValueOrDefault();
            set => __pbn__recommendationid = value;
        }
        public bool ShouldSerializerecommendationid() => __pbn__recommendationid != null;
        public void Resetrecommendationid() => __pbn__recommendationid = null;
        private ulong? __pbn__recommendationid;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong steamid
        {
            get => __pbn__steamid.GetValueOrDefault();
            set => __pbn__steamid = value;
        }
        public bool ShouldSerializesteamid() => __pbn__steamid != null;
        public void Resetsteamid() => __pbn__steamid = null;
        private ulong? __pbn__steamid;

        [global::ProtoBuf.ProtoMember(3)]
        public uint appid
        {
            get => __pbn__appid.GetValueOrDefault();
            set => __pbn__appid = value;
        }
        public bool ShouldSerializeappid() => __pbn__appid != null;
        public void Resetappid() => __pbn__appid = null;
        private uint? __pbn__appid;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue("")]
        public string review
        {
            get => __pbn__review ?? "";
            set => __pbn__review = value;
        }
        public bool ShouldSerializereview() => __pbn__review != null;
        public void Resetreview() => __pbn__review = null;
        private string __pbn__review;

        [global::ProtoBuf.ProtoMember(5)]
        public uint time_created
        {
            get => __pbn__time_created.GetValueOrDefault();
            set => __pbn__time_created = value;
        }
        public bool ShouldSerializetime_created() => __pbn__time_created != null;
        public void Resettime_created() => __pbn__time_created = null;
        private uint? __pbn__time_created;

        [global::ProtoBuf.ProtoMember(6)]
        public uint time_updated
        {
            get => __pbn__time_updated.GetValueOrDefault();
            set => __pbn__time_updated = value;
        }
        public bool ShouldSerializetime_updated() => __pbn__time_updated != null;
        public void Resettime_updated() => __pbn__time_updated = null;
        private uint? __pbn__time_updated;

        [global::ProtoBuf.ProtoMember(7)]
        public uint votes_up
        {
            get => __pbn__votes_up.GetValueOrDefault();
            set => __pbn__votes_up = value;
        }
        public bool ShouldSerializevotes_up() => __pbn__votes_up != null;
        public void Resetvotes_up() => __pbn__votes_up = null;
        private uint? __pbn__votes_up;

        [global::ProtoBuf.ProtoMember(8)]
        public uint votes_down
        {
            get => __pbn__votes_down.GetValueOrDefault();
            set => __pbn__votes_down = value;
        }
        public bool ShouldSerializevotes_down() => __pbn__votes_down != null;
        public void Resetvotes_down() => __pbn__votes_down = null;
        private uint? __pbn__votes_down;

        [global::ProtoBuf.ProtoMember(9)]
        public float vote_score
        {
            get => __pbn__vote_score.GetValueOrDefault();
            set => __pbn__vote_score = value;
        }
        public bool ShouldSerializevote_score() => __pbn__vote_score != null;
        public void Resetvote_score() => __pbn__vote_score = null;
        private float? __pbn__vote_score;

        [global::ProtoBuf.ProtoMember(10)]
        [global::System.ComponentModel.DefaultValue("")]
        public string language
        {
            get => __pbn__language ?? "";
            set => __pbn__language = value;
        }
        public bool ShouldSerializelanguage() => __pbn__language != null;
        public void Resetlanguage() => __pbn__language = null;
        private string __pbn__language;

        [global::ProtoBuf.ProtoMember(11)]
        public uint comment_count
        {
            get => __pbn__comment_count.GetValueOrDefault();
            set => __pbn__comment_count = value;
        }
        public bool ShouldSerializecomment_count() => __pbn__comment_count != null;
        public void Resetcomment_count() => __pbn__comment_count = null;
        private uint? __pbn__comment_count;

        [global::ProtoBuf.ProtoMember(12)]
        public bool voted_up
        {
            get => __pbn__voted_up.GetValueOrDefault();
            set => __pbn__voted_up = value;
        }
        public bool ShouldSerializevoted_up() => __pbn__voted_up != null;
        public void Resetvoted_up() => __pbn__voted_up = null;
        private bool? __pbn__voted_up;

        [global::ProtoBuf.ProtoMember(13)]
        public bool is_public
        {
            get => __pbn__is_public.GetValueOrDefault();
            set => __pbn__is_public = value;
        }
        public bool ShouldSerializeis_public() => __pbn__is_public != null;
        public void Resetis_public() => __pbn__is_public = null;
        private bool? __pbn__is_public;

        [global::ProtoBuf.ProtoMember(14)]
        public bool moderator_hidden
        {
            get => __pbn__moderator_hidden.GetValueOrDefault();
            set => __pbn__moderator_hidden = value;
        }
        public bool ShouldSerializemoderator_hidden() => __pbn__moderator_hidden != null;
        public void Resetmoderator_hidden() => __pbn__moderator_hidden = null;
        private bool? __pbn__moderator_hidden;

        [global::ProtoBuf.ProtoMember(15)]
        public int flagged_by_developer
        {
            get => __pbn__flagged_by_developer.GetValueOrDefault();
            set => __pbn__flagged_by_developer = value;
        }
        public bool ShouldSerializeflagged_by_developer() => __pbn__flagged_by_developer != null;
        public void Resetflagged_by_developer() => __pbn__flagged_by_developer = null;
        private int? __pbn__flagged_by_developer;

        [global::ProtoBuf.ProtoMember(16)]
        public uint report_score
        {
            get => __pbn__report_score.GetValueOrDefault();
            set => __pbn__report_score = value;
        }
        public bool ShouldSerializereport_score() => __pbn__report_score != null;
        public void Resetreport_score() => __pbn__report_score = null;
        private uint? __pbn__report_score;

        [global::ProtoBuf.ProtoMember(17)]
        public ulong steamid_moderator
        {
            get => __pbn__steamid_moderator.GetValueOrDefault();
            set => __pbn__steamid_moderator = value;
        }
        public bool ShouldSerializesteamid_moderator() => __pbn__steamid_moderator != null;
        public void Resetsteamid_moderator() => __pbn__steamid_moderator = null;
        private ulong? __pbn__steamid_moderator;

        [global::ProtoBuf.ProtoMember(18)]
        public ulong steamid_developer
        {
            get => __pbn__steamid_developer.GetValueOrDefault();
            set => __pbn__steamid_developer = value;
        }
        public bool ShouldSerializesteamid_developer() => __pbn__steamid_developer != null;
        public void Resetsteamid_developer() => __pbn__steamid_developer = null;
        private ulong? __pbn__steamid_developer;

        [global::ProtoBuf.ProtoMember(19)]
        public ulong steamid_dev_responder
        {
            get => __pbn__steamid_dev_responder.GetValueOrDefault();
            set => __pbn__steamid_dev_responder = value;
        }
        public bool ShouldSerializesteamid_dev_responder() => __pbn__steamid_dev_responder != null;
        public void Resetsteamid_dev_responder() => __pbn__steamid_dev_responder = null;
        private ulong? __pbn__steamid_dev_responder;

        [global::ProtoBuf.ProtoMember(20)]
        [global::System.ComponentModel.DefaultValue("")]
        public string developer_response
        {
            get => __pbn__developer_response ?? "";
            set => __pbn__developer_response = value;
        }
        public bool ShouldSerializedeveloper_response() => __pbn__developer_response != null;
        public void Resetdeveloper_response() => __pbn__developer_response = null;
        private string __pbn__developer_response;

        [global::ProtoBuf.ProtoMember(21)]
        public uint time_developer_responded
        {
            get => __pbn__time_developer_responded.GetValueOrDefault();
            set => __pbn__time_developer_responded = value;
        }
        public bool ShouldSerializetime_developer_responded() => __pbn__time_developer_responded != null;
        public void Resettime_developer_responded() => __pbn__time_developer_responded = null;
        private uint? __pbn__time_developer_responded;

        [global::ProtoBuf.ProtoMember(22)]
        public bool developer_flag_cleared
        {
            get => __pbn__developer_flag_cleared.GetValueOrDefault();
            set => __pbn__developer_flag_cleared = value;
        }
        public bool ShouldSerializedeveloper_flag_cleared() => __pbn__developer_flag_cleared != null;
        public void Resetdeveloper_flag_cleared() => __pbn__developer_flag_cleared = null;
        private bool? __pbn__developer_flag_cleared;

        [global::ProtoBuf.ProtoMember(23)]
        public bool written_during_early_access
        {
            get => __pbn__written_during_early_access.GetValueOrDefault();
            set => __pbn__written_during_early_access = value;
        }
        public bool ShouldSerializewritten_during_early_access() => __pbn__written_during_early_access != null;
        public void Resetwritten_during_early_access() => __pbn__written_during_early_access = null;
        private bool? __pbn__written_during_early_access;

        [global::ProtoBuf.ProtoMember(24)]
        public uint votes_funny
        {
            get => __pbn__votes_funny.GetValueOrDefault();
            set => __pbn__votes_funny = value;
        }
        public bool ShouldSerializevotes_funny() => __pbn__votes_funny != null;
        public void Resetvotes_funny() => __pbn__votes_funny = null;
        private uint? __pbn__votes_funny;

        [global::ProtoBuf.ProtoMember(25)]
        public bool received_compensation
        {
            get => __pbn__received_compensation.GetValueOrDefault();
            set => __pbn__received_compensation = value;
        }
        public bool ShouldSerializereceived_compensation() => __pbn__received_compensation != null;
        public void Resetreceived_compensation() => __pbn__received_compensation = null;
        private bool? __pbn__received_compensation;

        [global::ProtoBuf.ProtoMember(26)]
        public bool unverified_purchase
        {
            get => __pbn__unverified_purchase.GetValueOrDefault();
            set => __pbn__unverified_purchase = value;
        }
        public bool ShouldSerializeunverified_purchase() => __pbn__unverified_purchase != null;
        public void Resetunverified_purchase() => __pbn__unverified_purchase = null;
        private bool? __pbn__unverified_purchase;

        [global::ProtoBuf.ProtoMember(27)]
        public global::System.Collections.Generic.List<int> review_qualities { get; } = new global::System.Collections.Generic.List<int>();

        [global::ProtoBuf.ProtoMember(28)]
        public float weighted_vote_score
        {
            get => __pbn__weighted_vote_score.GetValueOrDefault();
            set => __pbn__weighted_vote_score = value;
        }
        public bool ShouldSerializeweighted_vote_score() => __pbn__weighted_vote_score != null;
        public void Resetweighted_vote_score() => __pbn__weighted_vote_score = null;
        private float? __pbn__weighted_vote_score;

        [global::ProtoBuf.ProtoMember(29)]
        [global::System.ComponentModel.DefaultValue("")]
        public string moderation_note
        {
            get => __pbn__moderation_note ?? "";
            set => __pbn__moderation_note = value;
        }
        public bool ShouldSerializemoderation_note() => __pbn__moderation_note != null;
        public void Resetmoderation_note() => __pbn__moderation_note = null;
        private string __pbn__moderation_note;

        [global::ProtoBuf.ProtoMember(30)]
        public int payment_method
        {
            get => __pbn__payment_method.GetValueOrDefault();
            set => __pbn__payment_method = value;
        }
        public bool ShouldSerializepayment_method() => __pbn__payment_method != null;
        public void Resetpayment_method() => __pbn__payment_method = null;
        private int? __pbn__payment_method;

        [global::ProtoBuf.ProtoMember(31)]
        public int playtime_2weeks
        {
            get => __pbn__playtime_2weeks.GetValueOrDefault();
            set => __pbn__playtime_2weeks = value;
        }
        public bool ShouldSerializeplaytime_2weeks() => __pbn__playtime_2weeks != null;
        public void Resetplaytime_2weeks() => __pbn__playtime_2weeks = null;
        private int? __pbn__playtime_2weeks;

        [global::ProtoBuf.ProtoMember(32)]
        public int playtime_forever
        {
            get => __pbn__playtime_forever.GetValueOrDefault();
            set => __pbn__playtime_forever = value;
        }
        public bool ShouldSerializeplaytime_forever() => __pbn__playtime_forever != null;
        public void Resetplaytime_forever() => __pbn__playtime_forever = null;
        private int? __pbn__playtime_forever;

        [global::ProtoBuf.ProtoMember(33)]
        public int last_playtime
        {
            get => __pbn__last_playtime.GetValueOrDefault();
            set => __pbn__last_playtime = value;
        }
        public bool ShouldSerializelast_playtime() => __pbn__last_playtime != null;
        public void Resetlast_playtime() => __pbn__last_playtime = null;
        private int? __pbn__last_playtime;

        [global::ProtoBuf.ProtoMember(34)]
        public bool comments_disabled
        {
            get => __pbn__comments_disabled.GetValueOrDefault();
            set => __pbn__comments_disabled = value;
        }
        public bool ShouldSerializecomments_disabled() => __pbn__comments_disabled != null;
        public void Resetcomments_disabled() => __pbn__comments_disabled = null;
        private bool? __pbn__comments_disabled;

        [global::ProtoBuf.ProtoMember(35)]
        public int playtime_at_review
        {
            get => __pbn__playtime_at_review.GetValueOrDefault();
            set => __pbn__playtime_at_review = value;
        }
        public bool ShouldSerializeplaytime_at_review() => __pbn__playtime_at_review != null;
        public void Resetplaytime_at_review() => __pbn__playtime_at_review = null;
        private int? __pbn__playtime_at_review;

        [global::ProtoBuf.ProtoMember(36)]
        public bool approved_for_china
        {
            get => __pbn__approved_for_china.GetValueOrDefault();
            set => __pbn__approved_for_china = value;
        }
        public bool ShouldSerializeapproved_for_china() => __pbn__approved_for_china != null;
        public void Resetapproved_for_china() => __pbn__approved_for_china = null;
        private bool? __pbn__approved_for_china;

        [global::ProtoBuf.ProtoMember(37)]
        public int ban_check_result
        {
            get => __pbn__ban_check_result.GetValueOrDefault();
            set => __pbn__ban_check_result = value;
        }
        public bool ShouldSerializeban_check_result() => __pbn__ban_check_result != null;
        public void Resetban_check_result() => __pbn__ban_check_result = null;
        private int? __pbn__ban_check_result;

        [global::ProtoBuf.ProtoMember(38)]
        public bool refunded
        {
            get => __pbn__refunded.GetValueOrDefault();
            set => __pbn__refunded = value;
        }
        public bool ShouldSerializerefunded() => __pbn__refunded != null;
        public void Resetrefunded() => __pbn__refunded = null;
        private bool? __pbn__refunded;

        [global::ProtoBuf.ProtoMember(39)]
        public int account_score_spend
        {
            get => __pbn__account_score_spend.GetValueOrDefault();
            set => __pbn__account_score_spend = value;
        }
        public bool ShouldSerializeaccount_score_spend() => __pbn__account_score_spend != null;
        public void Resetaccount_score_spend() => __pbn__account_score_spend = null;
        private int? __pbn__account_score_spend;

        [global::ProtoBuf.ProtoMember(40)]
        public global::System.Collections.Generic.List<CUserReviews_Recommendation_LoyaltyReaction> reactions { get; } = new global::System.Collections.Generic.List<CUserReviews_Recommendation_LoyaltyReaction>();

        [global::ProtoBuf.ProtoMember(41)]
        [global::System.ComponentModel.DefaultValue("")]
        public string ipaddress
        {
            get => __pbn__ipaddress ?? "";
            set => __pbn__ipaddress = value;
        }
        public bool ShouldSerializeipaddress() => __pbn__ipaddress != null;
        public void Resetipaddress() => __pbn__ipaddress = null;
        private string __pbn__ipaddress;

        [global::ProtoBuf.ProtoMember(42)]
        public bool hidden_in_steam_china
        {
            get => __pbn__hidden_in_steam_china.GetValueOrDefault();
            set => __pbn__hidden_in_steam_china = value;
        }
        public bool ShouldSerializehidden_in_steam_china() => __pbn__hidden_in_steam_china != null;
        public void Resethidden_in_steam_china() => __pbn__hidden_in_steam_china = null;
        private bool? __pbn__hidden_in_steam_china;

        [global::ProtoBuf.ProtoMember(43)]
        [global::System.ComponentModel.DefaultValue("")]
        public string steam_china_location
        {
            get => __pbn__steam_china_location ?? "";
            set => __pbn__steam_china_location = value;
        }
        public bool ShouldSerializesteam_china_location() => __pbn__steam_china_location != null;
        public void Resetsteam_china_location() => __pbn__steam_china_location = null;
        private string __pbn__steam_china_location;

        [global::ProtoBuf.ProtoMember(44)]
        public uint category_ascii_pct
        {
            get => __pbn__category_ascii_pct.GetValueOrDefault();
            set => __pbn__category_ascii_pct = value;
        }
        public bool ShouldSerializecategory_ascii_pct() => __pbn__category_ascii_pct != null;
        public void Resetcategory_ascii_pct() => __pbn__category_ascii_pct = null;
        private uint? __pbn__category_ascii_pct;

        [global::ProtoBuf.ProtoMember(45)]
        public uint category_meme_pct
        {
            get => __pbn__category_meme_pct.GetValueOrDefault();
            set => __pbn__category_meme_pct = value;
        }
        public bool ShouldSerializecategory_meme_pct() => __pbn__category_meme_pct != null;
        public void Resetcategory_meme_pct() => __pbn__category_meme_pct = null;
        private uint? __pbn__category_meme_pct;

        [global::ProtoBuf.ProtoMember(46)]
        public uint category_offtopic_pct
        {
            get => __pbn__category_offtopic_pct.GetValueOrDefault();
            set => __pbn__category_offtopic_pct = value;
        }
        public bool ShouldSerializecategory_offtopic_pct() => __pbn__category_offtopic_pct != null;
        public void Resetcategory_offtopic_pct() => __pbn__category_offtopic_pct = null;
        private uint? __pbn__category_offtopic_pct;

        [global::ProtoBuf.ProtoMember(47)]
        public uint category_uninformative_pct
        {
            get => __pbn__category_uninformative_pct.GetValueOrDefault();
            set => __pbn__category_uninformative_pct = value;
        }
        public bool ShouldSerializecategory_uninformative_pct() => __pbn__category_uninformative_pct != null;
        public void Resetcategory_uninformative_pct() => __pbn__category_uninformative_pct = null;
        private uint? __pbn__category_uninformative_pct;

        [global::ProtoBuf.ProtoMember(48)]
        public uint category_votefarming_pct
        {
            get => __pbn__category_votefarming_pct.GetValueOrDefault();
            set => __pbn__category_votefarming_pct = value;
        }
        public bool ShouldSerializecategory_votefarming_pct() => __pbn__category_votefarming_pct != null;
        public void Resetcategory_votefarming_pct() => __pbn__category_votefarming_pct = null;
        private uint? __pbn__category_votefarming_pct;

        [global::ProtoBuf.ProtoMember(49)]
        public int deck_playtime_at_review
        {
            get => __pbn__deck_playtime_at_review.GetValueOrDefault();
            set => __pbn__deck_playtime_at_review = value;
        }
        public bool ShouldSerializedeck_playtime_at_review() => __pbn__deck_playtime_at_review != null;
        public void Resetdeck_playtime_at_review() => __pbn__deck_playtime_at_review = null;
        private int? __pbn__deck_playtime_at_review;

    }

    public class UserReviews : SteamUnifiedMessages.UnifiedService
    {
        public override string ServiceName { get; } = "UserReviews";

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CUserReviews_GetFriendsRecommendedApp_Response>> GetFriendsRecommendedApp(CUserReviews_GetFriendsRecommendedApp_Request request)
        {
            return UnifiedMessages.SendMessage<CUserReviews_GetFriendsRecommendedApp_Request, CUserReviews_GetFriendsRecommendedApp_Response>( "UserReviews.GetFriendsRecommendedApp#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CUserReviews_GetIndividualRecommendations_Response>> GetIndividualRecommendations(CUserReviews_GetIndividualRecommendations_Request request)
        {
            return UnifiedMessages.SendMessage<CUserReviews_GetIndividualRecommendations_Request, CUserReviews_GetIndividualRecommendations_Response>( "UserReviews.GetIndividualRecommendations#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CUserReviews_Update_Response>> Update(CUserReviews_Update_Request request)
        {
            return UnifiedMessages.SendMessage<CUserReviews_Update_Request, CUserReviews_Update_Response>( "UserReviews.Update#1", request );
        }

        public override void HandleMsg( string methodName, IPacketMsg packetMsg )
        {
            switch ( methodName )
            {
                case "GetFriendsRecommendedApp":
                    UnifiedMessages.HandleServiceMsg<CUserReviews_GetFriendsRecommendedApp_Response>( packetMsg );
                    break;
                case "GetIndividualRecommendations":
                    UnifiedMessages.HandleServiceMsg<CUserReviews_GetIndividualRecommendations_Response>( packetMsg );
                    break;
                case "Update":
                    UnifiedMessages.HandleServiceMsg<CUserReviews_Update_Response>( packetMsg );
                    break;
            }
        }
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
