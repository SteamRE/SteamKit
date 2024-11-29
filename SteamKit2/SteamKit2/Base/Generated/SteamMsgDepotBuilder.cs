// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: steammessages_depotbuilder.steamclient.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_InitDepotBuild_Request : global::ProtoBuf.IExtensible
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

        [global::ProtoBuf.ProtoMember(2)]
        public uint depotid
        {
            get => __pbn__depotid.GetValueOrDefault();
            set => __pbn__depotid = value;
        }
        public bool ShouldSerializedepotid() => __pbn__depotid != null;
        public void Resetdepotid() => __pbn__depotid = null;
        private uint? __pbn__depotid;

        [global::ProtoBuf.ProtoMember(3)]
        public ulong workshop_itemid
        {
            get => __pbn__workshop_itemid.GetValueOrDefault();
            set => __pbn__workshop_itemid = value;
        }
        public bool ShouldSerializeworkshop_itemid() => __pbn__workshop_itemid != null;
        public void Resetworkshop_itemid() => __pbn__workshop_itemid = null;
        private ulong? __pbn__workshop_itemid;

        [global::ProtoBuf.ProtoMember(4)]
        public bool for_local_cs
        {
            get => __pbn__for_local_cs.GetValueOrDefault();
            set => __pbn__for_local_cs = value;
        }
        public bool ShouldSerializefor_local_cs() => __pbn__for_local_cs != null;
        public void Resetfor_local_cs() => __pbn__for_local_cs = null;
        private bool? __pbn__for_local_cs;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string target_branch
        {
            get => __pbn__target_branch ?? "";
            set => __pbn__target_branch = value;
        }
        public bool ShouldSerializetarget_branch() => __pbn__target_branch != null;
        public void Resettarget_branch() => __pbn__target_branch = null;
        private string __pbn__target_branch;

        [global::ProtoBuf.ProtoMember(6)]
        public bool shader_depot
        {
            get => __pbn__shader_depot.GetValueOrDefault();
            set => __pbn__shader_depot = value;
        }
        public bool ShouldSerializeshader_depot() => __pbn__shader_depot != null;
        public void Resetshader_depot() => __pbn__shader_depot = null;
        private bool? __pbn__shader_depot;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_InitDepotBuild_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong baseline_manifestid
        {
            get => __pbn__baseline_manifestid.GetValueOrDefault();
            set => __pbn__baseline_manifestid = value;
        }
        public bool ShouldSerializebaseline_manifestid() => __pbn__baseline_manifestid != null;
        public void Resetbaseline_manifestid() => __pbn__baseline_manifestid = null;
        private ulong? __pbn__baseline_manifestid;

        [global::ProtoBuf.ProtoMember(2)]
        public uint chunk_size
        {
            get => __pbn__chunk_size.GetValueOrDefault();
            set => __pbn__chunk_size = value;
        }
        public bool ShouldSerializechunk_size() => __pbn__chunk_size != null;
        public void Resetchunk_size() => __pbn__chunk_size = null;
        private uint? __pbn__chunk_size;

        [global::ProtoBuf.ProtoMember(3)]
        public byte[] aes_key
        {
            get => __pbn__aes_key;
            set => __pbn__aes_key = value;
        }
        public bool ShouldSerializeaes_key() => __pbn__aes_key != null;
        public void Resetaes_key() => __pbn__aes_key = null;
        private byte[] __pbn__aes_key;

        [global::ProtoBuf.ProtoMember(4)]
        public byte[] rsa_key
        {
            get => __pbn__rsa_key;
            set => __pbn__rsa_key = value;
        }
        public bool ShouldSerializersa_key() => __pbn__rsa_key != null;
        public void Resetrsa_key() => __pbn__rsa_key = null;
        private byte[] __pbn__rsa_key;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string url_host
        {
            get => __pbn__url_host ?? "";
            set => __pbn__url_host = value;
        }
        public bool ShouldSerializeurl_host() => __pbn__url_host != null;
        public void Reseturl_host() => __pbn__url_host = null;
        private string __pbn__url_host;

        [global::ProtoBuf.ProtoMember(6)]
        public bool offset_detection_enabled
        {
            get => __pbn__offset_detection_enabled.GetValueOrDefault();
            set => __pbn__offset_detection_enabled = value;
        }
        public bool ShouldSerializeoffset_detection_enabled() => __pbn__offset_detection_enabled != null;
        public void Resetoffset_detection_enabled() => __pbn__offset_detection_enabled = null;
        private bool? __pbn__offset_detection_enabled;

        [global::ProtoBuf.ProtoMember(7)]
        public uint offset_detection_min_clean_chunk
        {
            get => __pbn__offset_detection_min_clean_chunk.GetValueOrDefault();
            set => __pbn__offset_detection_min_clean_chunk = value;
        }
        public bool ShouldSerializeoffset_detection_min_clean_chunk() => __pbn__offset_detection_min_clean_chunk != null;
        public void Resetoffset_detection_min_clean_chunk() => __pbn__offset_detection_min_clean_chunk = null;
        private uint? __pbn__offset_detection_min_clean_chunk;

        [global::ProtoBuf.ProtoMember(8)]
        public uint offset_detection_blast_radius_pre
        {
            get => __pbn__offset_detection_blast_radius_pre.GetValueOrDefault();
            set => __pbn__offset_detection_blast_radius_pre = value;
        }
        public bool ShouldSerializeoffset_detection_blast_radius_pre() => __pbn__offset_detection_blast_radius_pre != null;
        public void Resetoffset_detection_blast_radius_pre() => __pbn__offset_detection_blast_radius_pre = null;
        private uint? __pbn__offset_detection_blast_radius_pre;

        [global::ProtoBuf.ProtoMember(9)]
        public uint offset_detection_blast_radius_post
        {
            get => __pbn__offset_detection_blast_radius_post.GetValueOrDefault();
            set => __pbn__offset_detection_blast_radius_post = value;
        }
        public bool ShouldSerializeoffset_detection_blast_radius_post() => __pbn__offset_detection_blast_radius_post != null;
        public void Resetoffset_detection_blast_radius_post() => __pbn__offset_detection_blast_radius_post = null;
        private uint? __pbn__offset_detection_blast_radius_post;

        [global::ProtoBuf.ProtoMember(10)]
        public uint offset_detection_max_distance_pre
        {
            get => __pbn__offset_detection_max_distance_pre.GetValueOrDefault();
            set => __pbn__offset_detection_max_distance_pre = value;
        }
        public bool ShouldSerializeoffset_detection_max_distance_pre() => __pbn__offset_detection_max_distance_pre != null;
        public void Resetoffset_detection_max_distance_pre() => __pbn__offset_detection_max_distance_pre = null;
        private uint? __pbn__offset_detection_max_distance_pre;

        [global::ProtoBuf.ProtoMember(11)]
        public uint offset_detection_max_distance_post
        {
            get => __pbn__offset_detection_max_distance_post.GetValueOrDefault();
            set => __pbn__offset_detection_max_distance_post = value;
        }
        public bool ShouldSerializeoffset_detection_max_distance_post() => __pbn__offset_detection_max_distance_post != null;
        public void Resetoffset_detection_max_distance_post() => __pbn__offset_detection_max_distance_post = null;
        private uint? __pbn__offset_detection_max_distance_post;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_StartDepotUpload_Request : global::ProtoBuf.IExtensible
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

        [global::ProtoBuf.ProtoMember(2)]
        public uint depotid
        {
            get => __pbn__depotid.GetValueOrDefault();
            set => __pbn__depotid = value;
        }
        public bool ShouldSerializedepotid() => __pbn__depotid != null;
        public void Resetdepotid() => __pbn__depotid = null;
        private uint? __pbn__depotid;

        [global::ProtoBuf.ProtoMember(3)]
        public ulong workshop_itemid
        {
            get => __pbn__workshop_itemid.GetValueOrDefault();
            set => __pbn__workshop_itemid = value;
        }
        public bool ShouldSerializeworkshop_itemid() => __pbn__workshop_itemid != null;
        public void Resetworkshop_itemid() => __pbn__workshop_itemid = null;
        private ulong? __pbn__workshop_itemid;

        [global::ProtoBuf.ProtoMember(4)]
        public bool for_local_cs
        {
            get => __pbn__for_local_cs.GetValueOrDefault();
            set => __pbn__for_local_cs = value;
        }
        public bool ShouldSerializefor_local_cs() => __pbn__for_local_cs != null;
        public void Resetfor_local_cs() => __pbn__for_local_cs = null;
        private bool? __pbn__for_local_cs;

        [global::ProtoBuf.ProtoMember(5)]
        public ulong baseline_manifestid
        {
            get => __pbn__baseline_manifestid.GetValueOrDefault();
            set => __pbn__baseline_manifestid = value;
        }
        public bool ShouldSerializebaseline_manifestid() => __pbn__baseline_manifestid != null;
        public void Resetbaseline_manifestid() => __pbn__baseline_manifestid = null;
        private ulong? __pbn__baseline_manifestid;

        [global::ProtoBuf.ProtoMember(6)]
        public uint manifest_size
        {
            get => __pbn__manifest_size.GetValueOrDefault();
            set => __pbn__manifest_size = value;
        }
        public bool ShouldSerializemanifest_size() => __pbn__manifest_size != null;
        public void Resetmanifest_size() => __pbn__manifest_size = null;
        private uint? __pbn__manifest_size;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_StartDepotUpload_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong depot_build_handle
        {
            get => __pbn__depot_build_handle.GetValueOrDefault();
            set => __pbn__depot_build_handle = value;
        }
        public bool ShouldSerializedepot_build_handle() => __pbn__depot_build_handle != null;
        public void Resetdepot_build_handle() => __pbn__depot_build_handle = null;
        private ulong? __pbn__depot_build_handle;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_GetMissingDepotChunks_Request : global::ProtoBuf.IExtensible
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

        [global::ProtoBuf.ProtoMember(2)]
        public ulong depot_build_handle
        {
            get => __pbn__depot_build_handle.GetValueOrDefault();
            set => __pbn__depot_build_handle = value;
        }
        public bool ShouldSerializedepot_build_handle() => __pbn__depot_build_handle != null;
        public void Resetdepot_build_handle() => __pbn__depot_build_handle = null;
        private ulong? __pbn__depot_build_handle;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_GetMissingDepotChunks_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<Chunks> missing_chunks { get; } = new global::System.Collections.Generic.List<Chunks>();

        [global::ProtoBuf.ProtoMember(2)]
        public uint total_missing_chunks
        {
            get => __pbn__total_missing_chunks.GetValueOrDefault();
            set => __pbn__total_missing_chunks = value;
        }
        public bool ShouldSerializetotal_missing_chunks() => __pbn__total_missing_chunks != null;
        public void Resettotal_missing_chunks() => __pbn__total_missing_chunks = null;
        private uint? __pbn__total_missing_chunks;

        [global::ProtoBuf.ProtoMember(3)]
        public ulong total_missing_bytes
        {
            get => __pbn__total_missing_bytes.GetValueOrDefault();
            set => __pbn__total_missing_bytes = value;
        }
        public bool ShouldSerializetotal_missing_bytes() => __pbn__total_missing_bytes != null;
        public void Resettotal_missing_bytes() => __pbn__total_missing_bytes = null;
        private ulong? __pbn__total_missing_bytes;

        [global::ProtoBuf.ProtoContract()]
        public partial class Chunks : global::ProtoBuf.IExtensible
        {
            private global::ProtoBuf.IExtension __pbn__extensionData;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [global::ProtoBuf.ProtoMember(1)]
            public byte[] sha
            {
                get => __pbn__sha;
                set => __pbn__sha = value;
            }
            public bool ShouldSerializesha() => __pbn__sha != null;
            public void Resetsha() => __pbn__sha = null;
            private byte[] __pbn__sha;

        }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_FinishDepotUpload_Request : global::ProtoBuf.IExtensible
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

        [global::ProtoBuf.ProtoMember(2)]
        public ulong depot_build_handle
        {
            get => __pbn__depot_build_handle.GetValueOrDefault();
            set => __pbn__depot_build_handle = value;
        }
        public bool ShouldSerializedepot_build_handle() => __pbn__depot_build_handle != null;
        public void Resetdepot_build_handle() => __pbn__depot_build_handle = null;
        private ulong? __pbn__depot_build_handle;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_FinishDepotUpload_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong manifestid
        {
            get => __pbn__manifestid.GetValueOrDefault();
            set => __pbn__manifestid = value;
        }
        public bool ShouldSerializemanifestid() => __pbn__manifestid != null;
        public void Resetmanifestid() => __pbn__manifestid = null;
        private ulong? __pbn__manifestid;

        [global::ProtoBuf.ProtoMember(2)]
        public bool prev_reused
        {
            get => __pbn__prev_reused.GetValueOrDefault();
            set => __pbn__prev_reused = value;
        }
        public bool ShouldSerializeprev_reused() => __pbn__prev_reused != null;
        public void Resetprev_reused() => __pbn__prev_reused = null;
        private bool? __pbn__prev_reused;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_CommitAppBuild_Request : global::ProtoBuf.IExtensible
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

        [global::ProtoBuf.ProtoMember(2)]
        public global::System.Collections.Generic.List<Depots> depot_manifests { get; } = new global::System.Collections.Generic.List<Depots>();

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue("")]
        public string build_notes
        {
            get => __pbn__build_notes ?? "";
            set => __pbn__build_notes = value;
        }
        public bool ShouldSerializebuild_notes() => __pbn__build_notes != null;
        public void Resetbuild_notes() => __pbn__build_notes = null;
        private string __pbn__build_notes;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string live_branch
        {
            get => __pbn__live_branch ?? "";
            set => __pbn__live_branch = value;
        }
        public bool ShouldSerializelive_branch() => __pbn__live_branch != null;
        public void Resetlive_branch() => __pbn__live_branch = null;
        private string __pbn__live_branch;

        [global::ProtoBuf.ProtoContract()]
        public partial class Depots : global::ProtoBuf.IExtensible
        {
            private global::ProtoBuf.IExtension __pbn__extensionData;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [global::ProtoBuf.ProtoMember(1)]
            public uint depotid
            {
                get => __pbn__depotid.GetValueOrDefault();
                set => __pbn__depotid = value;
            }
            public bool ShouldSerializedepotid() => __pbn__depotid != null;
            public void Resetdepotid() => __pbn__depotid = null;
            private uint? __pbn__depotid;

            [global::ProtoBuf.ProtoMember(2)]
            public ulong manifestid
            {
                get => __pbn__manifestid.GetValueOrDefault();
                set => __pbn__manifestid = value;
            }
            public bool ShouldSerializemanifestid() => __pbn__manifestid != null;
            public void Resetmanifestid() => __pbn__manifestid = null;
            private ulong? __pbn__manifestid;

        }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_CommitAppBuild_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint buildid
        {
            get => __pbn__buildid.GetValueOrDefault();
            set => __pbn__buildid = value;
        }
        public bool ShouldSerializebuildid() => __pbn__buildid != null;
        public void Resetbuildid() => __pbn__buildid = null;
        private uint? __pbn__buildid;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_SignInstallScript_Request : global::ProtoBuf.IExtensible
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

        [global::ProtoBuf.ProtoMember(2)]
        public uint depotid
        {
            get => __pbn__depotid.GetValueOrDefault();
            set => __pbn__depotid = value;
        }
        public bool ShouldSerializedepotid() => __pbn__depotid != null;
        public void Resetdepotid() => __pbn__depotid = null;
        private uint? __pbn__depotid;

        [global::ProtoBuf.ProtoMember(3)]
        [global::System.ComponentModel.DefaultValue("")]
        public string install_script
        {
            get => __pbn__install_script ?? "";
            set => __pbn__install_script = value;
        }
        public bool ShouldSerializeinstall_script() => __pbn__install_script != null;
        public void Resetinstall_script() => __pbn__install_script = null;
        private string __pbn__install_script;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CContentBuilder_SignInstallScript_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string signed_install_script
        {
            get => __pbn__signed_install_script ?? "";
            set => __pbn__signed_install_script = value;
        }
        public bool ShouldSerializesigned_install_script() => __pbn__signed_install_script != null;
        public void Resetsigned_install_script() => __pbn__signed_install_script = null;
        private string __pbn__signed_install_script;

    }

    public class ContentBuilder : SteamUnifiedMessages.UnifiedService
    {
        public override string ServiceName { get; } = "ContentBuilder";

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CContentBuilder_InitDepotBuild_Response>> InitDepotBuild( CContentBuilder_InitDepotBuild_Request request )
        {
            return UnifiedMessages.SendMessage<CContentBuilder_InitDepotBuild_Request, CContentBuilder_InitDepotBuild_Response>( "ContentBuilder.InitDepotBuild#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CContentBuilder_StartDepotUpload_Response>> StartDepotUpload( CContentBuilder_StartDepotUpload_Request request )
        {
            return UnifiedMessages.SendMessage<CContentBuilder_StartDepotUpload_Request, CContentBuilder_StartDepotUpload_Response>( "ContentBuilder.StartDepotUpload#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CContentBuilder_GetMissingDepotChunks_Response>> GetMissingDepotChunks( CContentBuilder_GetMissingDepotChunks_Request request )
        {
            return UnifiedMessages.SendMessage<CContentBuilder_GetMissingDepotChunks_Request, CContentBuilder_GetMissingDepotChunks_Response>( "ContentBuilder.GetMissingDepotChunks#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CContentBuilder_FinishDepotUpload_Response>> FinishDepotUpload( CContentBuilder_FinishDepotUpload_Request request )
        {
            return UnifiedMessages.SendMessage<CContentBuilder_FinishDepotUpload_Request, CContentBuilder_FinishDepotUpload_Response>( "ContentBuilder.FinishDepotUpload#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CContentBuilder_CommitAppBuild_Response>> CommitAppBuild( CContentBuilder_CommitAppBuild_Request request )
        {
            return UnifiedMessages.SendMessage<CContentBuilder_CommitAppBuild_Request, CContentBuilder_CommitAppBuild_Response>( "ContentBuilder.CommitAppBuild#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CContentBuilder_SignInstallScript_Response>> SignInstallScript( CContentBuilder_SignInstallScript_Request request )
        {
            return UnifiedMessages.SendMessage<CContentBuilder_SignInstallScript_Request, CContentBuilder_SignInstallScript_Response>( "ContentBuilder.SignInstallScript#1", request );
        }

        public override void HandleResponseMsg( string methodName, PacketClientMsgProtobuf packetMsg )
        {
            switch ( methodName )
            {
                case "InitDepotBuild":
                    PostResponseMsg<CContentBuilder_InitDepotBuild_Response>( packetMsg );
                    break;
                case "StartDepotUpload":
                    PostResponseMsg<CContentBuilder_StartDepotUpload_Response>( packetMsg );
                    break;
                case "GetMissingDepotChunks":
                    PostResponseMsg<CContentBuilder_GetMissingDepotChunks_Response>( packetMsg );
                    break;
                case "FinishDepotUpload":
                    PostResponseMsg<CContentBuilder_FinishDepotUpload_Response>( packetMsg );
                    break;
                case "CommitAppBuild":
                    PostResponseMsg<CContentBuilder_CommitAppBuild_Response>( packetMsg );
                    break;
                case "SignInstallScript":
                    PostResponseMsg<CContentBuilder_SignInstallScript_Response>( packetMsg );
                    break;
            }
        }

        public override void HandleNotificationMsg( string methodName, PacketClientMsgProtobuf packetMsg )
        {
        }
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
