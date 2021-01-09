// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: gc.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.GC.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CMsgProtoBufHeader : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong client_steam_id
        {
            get => __pbn__client_steam_id.GetValueOrDefault();
            set => __pbn__client_steam_id = value;
        }
        public bool ShouldSerializeclient_steam_id() => __pbn__client_steam_id != null;
        public void Resetclient_steam_id() => __pbn__client_steam_id = null;
        private ulong? __pbn__client_steam_id;

        [global::ProtoBuf.ProtoMember(2)]
        public int client_session_id
        {
            get => __pbn__client_session_id.GetValueOrDefault();
            set => __pbn__client_session_id = value;
        }
        public bool ShouldSerializeclient_session_id() => __pbn__client_session_id != null;
        public void Resetclient_session_id() => __pbn__client_session_id = null;
        private int? __pbn__client_session_id;

        [global::ProtoBuf.ProtoMember(3)]
        public uint source_app_id
        {
            get => __pbn__source_app_id.GetValueOrDefault();
            set => __pbn__source_app_id = value;
        }
        public bool ShouldSerializesource_app_id() => __pbn__source_app_id != null;
        public void Resetsource_app_id() => __pbn__source_app_id = null;
        private uint? __pbn__source_app_id;

        [global::ProtoBuf.ProtoMember(10, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        [global::System.ComponentModel.DefaultValue(typeof(ulong), "18446744073709551615")]
        public ulong job_id_source
        {
            get => __pbn__job_id_source ?? 18446744073709551615;
            set => __pbn__job_id_source = value;
        }
        public bool ShouldSerializejob_id_source() => __pbn__job_id_source != null;
        public void Resetjob_id_source() => __pbn__job_id_source = null;
        private ulong? __pbn__job_id_source;

        [global::ProtoBuf.ProtoMember(11, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        [global::System.ComponentModel.DefaultValue(typeof(ulong), "18446744073709551615")]
        public ulong job_id_target
        {
            get => __pbn__job_id_target ?? 18446744073709551615;
            set => __pbn__job_id_target = value;
        }
        public bool ShouldSerializejob_id_target() => __pbn__job_id_target != null;
        public void Resetjob_id_target() => __pbn__job_id_target = null;
        private ulong? __pbn__job_id_target;

        [global::ProtoBuf.ProtoMember(12)]
        [global::System.ComponentModel.DefaultValue("")]
        public string target_job_name
        {
            get => __pbn__target_job_name ?? "";
            set => __pbn__target_job_name = value;
        }
        public bool ShouldSerializetarget_job_name() => __pbn__target_job_name != null;
        public void Resettarget_job_name() => __pbn__target_job_name = null;
        private string __pbn__target_job_name;

        [global::ProtoBuf.ProtoMember(13)]
        [global::System.ComponentModel.DefaultValue(2)]
        public int eresult
        {
            get => __pbn__eresult ?? 2;
            set => __pbn__eresult = value;
        }
        public bool ShouldSerializeeresult() => __pbn__eresult != null;
        public void Reseteresult() => __pbn__eresult = null;
        private int? __pbn__eresult;

        [global::ProtoBuf.ProtoMember(14)]
        [global::System.ComponentModel.DefaultValue("")]
        public string error_message
        {
            get => __pbn__error_message ?? "";
            set => __pbn__error_message = value;
        }
        public bool ShouldSerializeerror_message() => __pbn__error_message != null;
        public void Reseterror_message() => __pbn__error_message = null;
        private string __pbn__error_message;

        [global::ProtoBuf.ProtoMember(15)]
        public uint ip
        {
            get => __pbn__ip.GetValueOrDefault();
            set => __pbn__ip = value;
        }
        public bool ShouldSerializeip() => __pbn__ip != null;
        public void Resetip() => __pbn__ip = null;
        private uint? __pbn__ip;

        [global::ProtoBuf.ProtoMember(200)]
        [global::System.ComponentModel.DefaultValue(GCProtoBufMsgSrc.GCProtoBufMsgSrc_Unspecified)]
        public GCProtoBufMsgSrc gc_msg_src
        {
            get => __pbn__gc_msg_src ?? GCProtoBufMsgSrc.GCProtoBufMsgSrc_Unspecified;
            set => __pbn__gc_msg_src = value;
        }
        public bool ShouldSerializegc_msg_src() => __pbn__gc_msg_src != null;
        public void Resetgc_msg_src() => __pbn__gc_msg_src = null;
        private GCProtoBufMsgSrc? __pbn__gc_msg_src;

        [global::ProtoBuf.ProtoMember(201)]
        public uint gc_dir_index_source
        {
            get => __pbn__gc_dir_index_source.GetValueOrDefault();
            set => __pbn__gc_dir_index_source = value;
        }
        public bool ShouldSerializegc_dir_index_source() => __pbn__gc_dir_index_source != null;
        public void Resetgc_dir_index_source() => __pbn__gc_dir_index_source = null;
        private uint? __pbn__gc_dir_index_source;

    }

    [global::ProtoBuf.ProtoContract()]
    public enum GCProtoBufMsgSrc
    {
        GCProtoBufMsgSrc_Unspecified = 0,
        GCProtoBufMsgSrc_FromSystem = 1,
        GCProtoBufMsgSrc_FromSteamID = 2,
        GCProtoBufMsgSrc_FromGC = 3,
        GCProtoBufMsgSrc_ReplySystem = 4,
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
