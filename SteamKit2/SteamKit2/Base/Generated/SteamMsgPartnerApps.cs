// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: steammessages_partnerapps.steamclient.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_RequestUploadToken_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string filename
        {
            get => __pbn__filename ?? "";
            set => __pbn__filename = value;
        }
        public bool ShouldSerializefilename() => __pbn__filename != null;
        public void Resetfilename() => __pbn__filename = null;
        private string __pbn__filename;

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
    public partial class CPartnerApps_RequestUploadToken_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong upload_token
        {
            get => __pbn__upload_token.GetValueOrDefault();
            set => __pbn__upload_token = value;
        }
        public bool ShouldSerializeupload_token() => __pbn__upload_token != null;
        public void Resetupload_token() => __pbn__upload_token = null;
        private ulong? __pbn__upload_token;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string location
        {
            get => __pbn__location ?? "";
            set => __pbn__location = value;
        }
        public bool ShouldSerializelocation() => __pbn__location != null;
        public void Resetlocation() => __pbn__location = null;
        private string __pbn__location;

        [global::ProtoBuf.ProtoMember(3)]
        public ulong routing_id
        {
            get => __pbn__routing_id.GetValueOrDefault();
            set => __pbn__routing_id = value;
        }
        public bool ShouldSerializerouting_id() => __pbn__routing_id != null;
        public void Resetrouting_id() => __pbn__routing_id = null;
        private ulong? __pbn__routing_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUpload_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong upload_token
        {
            get => __pbn__upload_token.GetValueOrDefault();
            set => __pbn__upload_token = value;
        }
        public bool ShouldSerializeupload_token() => __pbn__upload_token != null;
        public void Resetupload_token() => __pbn__upload_token = null;
        private ulong? __pbn__upload_token;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong routing_id
        {
            get => __pbn__routing_id.GetValueOrDefault();
            set => __pbn__routing_id = value;
        }
        public bool ShouldSerializerouting_id() => __pbn__routing_id != null;
        public void Resetrouting_id() => __pbn__routing_id = null;
        private ulong? __pbn__routing_id;

        [global::ProtoBuf.ProtoMember(3)]
        public uint app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private uint? __pbn__app_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUploadKVSign_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string signed_installscript
        {
            get => __pbn__signed_installscript ?? "";
            set => __pbn__signed_installscript = value;
        }
        public bool ShouldSerializesigned_installscript() => __pbn__signed_installscript != null;
        public void Resetsigned_installscript() => __pbn__signed_installscript = null;
        private string __pbn__signed_installscript;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUploadLegacyDRM_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong upload_token
        {
            get => __pbn__upload_token.GetValueOrDefault();
            set => __pbn__upload_token = value;
        }
        public bool ShouldSerializeupload_token() => __pbn__upload_token != null;
        public void Resetupload_token() => __pbn__upload_token = null;
        private ulong? __pbn__upload_token;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong routing_id
        {
            get => __pbn__routing_id.GetValueOrDefault();
            set => __pbn__routing_id = value;
        }
        public bool ShouldSerializerouting_id() => __pbn__routing_id != null;
        public void Resetrouting_id() => __pbn__routing_id = null;
        private ulong? __pbn__routing_id;

        [global::ProtoBuf.ProtoMember(3)]
        public uint app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private uint? __pbn__app_id;

        [global::ProtoBuf.ProtoMember(4)]
        public uint flags
        {
            get => __pbn__flags.GetValueOrDefault();
            set => __pbn__flags = value;
        }
        public bool ShouldSerializeflags() => __pbn__flags != null;
        public void Resetflags() => __pbn__flags = null;
        private uint? __pbn__flags;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string tool_name
        {
            get => __pbn__tool_name ?? "";
            set => __pbn__tool_name = value;
        }
        public bool ShouldSerializetool_name() => __pbn__tool_name != null;
        public void Resettool_name() => __pbn__tool_name = null;
        private string __pbn__tool_name;

        [global::ProtoBuf.ProtoMember(6)]
        [global::System.ComponentModel.DefaultValue(false)]
        public bool use_cloud
        {
            get => __pbn__use_cloud ?? false;
            set => __pbn__use_cloud = value;
        }
        public bool ShouldSerializeuse_cloud() => __pbn__use_cloud != null;
        public void Resetuse_cloud() => __pbn__use_cloud = null;
        private bool? __pbn__use_cloud;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUploadLegacyDRM_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string file_id
        {
            get => __pbn__file_id ?? "";
            set => __pbn__file_id = value;
        }
        public bool ShouldSerializefile_id() => __pbn__file_id != null;
        public void Resetfile_id() => __pbn__file_id = null;
        private string __pbn__file_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUpload_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUploadDepot_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong upload_token
        {
            get => __pbn__upload_token.GetValueOrDefault();
            set => __pbn__upload_token = value;
        }
        public bool ShouldSerializeupload_token() => __pbn__upload_token != null;
        public void Resetupload_token() => __pbn__upload_token = null;
        private ulong? __pbn__upload_token;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong routing_id
        {
            get => __pbn__routing_id.GetValueOrDefault();
            set => __pbn__routing_id = value;
        }
        public bool ShouldSerializerouting_id() => __pbn__routing_id != null;
        public void Resetrouting_id() => __pbn__routing_id = null;
        private ulong? __pbn__routing_id;

        [global::ProtoBuf.ProtoMember(3)]
        public uint app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private uint? __pbn__app_id;

        [global::ProtoBuf.ProtoMember(4)]
        public uint depot_id
        {
            get => __pbn__depot_id.GetValueOrDefault();
            set => __pbn__depot_id = value;
        }
        public bool ShouldSerializedepot_id() => __pbn__depot_id != null;
        public void Resetdepot_id() => __pbn__depot_id = null;
        private uint? __pbn__depot_id;

        [global::ProtoBuf.ProtoMember(5)]
        public uint build_flags
        {
            get => __pbn__build_flags.GetValueOrDefault();
            set => __pbn__build_flags = value;
        }
        public bool ShouldSerializebuild_flags() => __pbn__build_flags != null;
        public void Resetbuild_flags() => __pbn__build_flags = null;
        private uint? __pbn__build_flags;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FinishUploadDepot_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong build_routing_id
        {
            get => __pbn__build_routing_id.GetValueOrDefault();
            set => __pbn__build_routing_id = value;
        }
        public bool ShouldSerializebuild_routing_id() => __pbn__build_routing_id != null;
        public void Resetbuild_routing_id() => __pbn__build_routing_id = null;
        private ulong? __pbn__build_routing_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_GetDepotBuildResult_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong upload_token
        {
            get => __pbn__upload_token.GetValueOrDefault();
            set => __pbn__upload_token = value;
        }
        public bool ShouldSerializeupload_token() => __pbn__upload_token != null;
        public void Resetupload_token() => __pbn__upload_token = null;
        private ulong? __pbn__upload_token;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong routing_id
        {
            get => __pbn__routing_id.GetValueOrDefault();
            set => __pbn__routing_id = value;
        }
        public bool ShouldSerializerouting_id() => __pbn__routing_id != null;
        public void Resetrouting_id() => __pbn__routing_id = null;
        private ulong? __pbn__routing_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_GetDepotBuildResult_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public ulong manifest_id
        {
            get => __pbn__manifest_id.GetValueOrDefault();
            set => __pbn__manifest_id = value;
        }
        public bool ShouldSerializemanifest_id() => __pbn__manifest_id != null;
        public void Resetmanifest_id() => __pbn__manifest_id = null;
        private ulong? __pbn__manifest_id;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string error_msg
        {
            get => __pbn__error_msg ?? "";
            set => __pbn__error_msg = value;
        }
        public bool ShouldSerializeerror_msg() => __pbn__error_msg != null;
        public void Reseterror_msg() => __pbn__error_msg = null;
        private string __pbn__error_msg;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FindDRMUploads_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public int app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private int? __pbn__app_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_ExistingDRMUpload : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string file_id
        {
            get => __pbn__file_id ?? "";
            set => __pbn__file_id = value;
        }
        public bool ShouldSerializefile_id() => __pbn__file_id != null;
        public void Resetfile_id() => __pbn__file_id = null;
        private string __pbn__file_id;

        [global::ProtoBuf.ProtoMember(2)]
        public uint app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private uint? __pbn__app_id;

        [global::ProtoBuf.ProtoMember(3)]
        public int actor_id
        {
            get => __pbn__actor_id.GetValueOrDefault();
            set => __pbn__actor_id = value;
        }
        public bool ShouldSerializeactor_id() => __pbn__actor_id != null;
        public void Resetactor_id() => __pbn__actor_id = null;
        private int? __pbn__actor_id;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string supplied_name
        {
            get => __pbn__supplied_name ?? "";
            set => __pbn__supplied_name = value;
        }
        public bool ShouldSerializesupplied_name() => __pbn__supplied_name != null;
        public void Resetsupplied_name() => __pbn__supplied_name = null;
        private string __pbn__supplied_name;

        [global::ProtoBuf.ProtoMember(6)]
        public uint flags
        {
            get => __pbn__flags.GetValueOrDefault();
            set => __pbn__flags = value;
        }
        public bool ShouldSerializeflags() => __pbn__flags != null;
        public void Resetflags() => __pbn__flags = null;
        private uint? __pbn__flags;

        [global::ProtoBuf.ProtoMember(7)]
        [global::System.ComponentModel.DefaultValue("")]
        public string mod_type
        {
            get => __pbn__mod_type ?? "";
            set => __pbn__mod_type = value;
        }
        public bool ShouldSerializemod_type() => __pbn__mod_type != null;
        public void Resetmod_type() => __pbn__mod_type = null;
        private string __pbn__mod_type;

        [global::ProtoBuf.ProtoMember(8, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public uint timestamp
        {
            get => __pbn__timestamp.GetValueOrDefault();
            set => __pbn__timestamp = value;
        }
        public bool ShouldSerializetimestamp() => __pbn__timestamp != null;
        public void Resettimestamp() => __pbn__timestamp = null;
        private uint? __pbn__timestamp;

        [global::ProtoBuf.ProtoMember(9)]
        [global::System.ComponentModel.DefaultValue("")]
        public string orig_file_id
        {
            get => __pbn__orig_file_id ?? "";
            set => __pbn__orig_file_id = value;
        }
        public bool ShouldSerializeorig_file_id() => __pbn__orig_file_id != null;
        public void Resetorig_file_id() => __pbn__orig_file_id = null;
        private string __pbn__orig_file_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_FindDRMUploads_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<CPartnerApps_ExistingDRMUpload> uploads { get; } = new global::System.Collections.Generic.List<CPartnerApps_ExistingDRMUpload>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_Download_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string file_id
        {
            get => __pbn__file_id ?? "";
            set => __pbn__file_id = value;
        }
        public bool ShouldSerializefile_id() => __pbn__file_id != null;
        public void Resetfile_id() => __pbn__file_id = null;
        private string __pbn__file_id;

        [global::ProtoBuf.ProtoMember(2)]
        public int app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private int? __pbn__app_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CPartnerApps_Download_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string download_url
        {
            get => __pbn__download_url ?? "";
            set => __pbn__download_url = value;
        }
        public bool ShouldSerializedownload_url() => __pbn__download_url != null;
        public void Resetdownload_url() => __pbn__download_url = null;
        private string __pbn__download_url;

        [global::ProtoBuf.ProtoMember(2)]
        public int app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private int? __pbn__app_id;

    }

    public class PartnerApps : SteamUnifiedMessages.UnifiedService
    {

        const string SERVICE_NAME = "PartnerApps";

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_RequestUploadToken_Response>> RequestKVSignUploadToken(CPartnerApps_RequestUploadToken_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_RequestUploadToken_Request, CPartnerApps_RequestUploadToken_Response>( $"{SERVICE_NAME}.RequestKVSignUploadToken#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_RequestUploadToken_Response>> RequestDRMUploadToken(CPartnerApps_RequestUploadToken_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_RequestUploadToken_Request, CPartnerApps_RequestUploadToken_Response>( $"{SERVICE_NAME}.RequestDRMUploadToken#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_RequestUploadToken_Response>> RequestCEGUploadToken(CPartnerApps_RequestUploadToken_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_RequestUploadToken_Request, CPartnerApps_RequestUploadToken_Response>( $"{SERVICE_NAME}.RequestCEGUploadToken#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_RequestUploadToken_Response>> RequestDepotUploadToken(CPartnerApps_RequestUploadToken_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_RequestUploadToken_Request, CPartnerApps_RequestUploadToken_Response>( $"{SERVICE_NAME}.RequestDepotUploadToken#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_FinishUploadKVSign_Response>> FinishUploadKVSign(CPartnerApps_FinishUpload_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_FinishUpload_Request, CPartnerApps_FinishUploadKVSign_Response>( $"{SERVICE_NAME}.FinishUploadKVSign#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_FinishUploadLegacyDRM_Response>> FinishUploadDRMUpload(CPartnerApps_FinishUploadLegacyDRM_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_FinishUploadLegacyDRM_Request, CPartnerApps_FinishUploadLegacyDRM_Response>( $"{SERVICE_NAME}.FinishUploadDRMUpload#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_FinishUpload_Response>> FinishUploadCEGUpload(CPartnerApps_FinishUpload_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_FinishUpload_Request, CPartnerApps_FinishUpload_Response>( $"{SERVICE_NAME}.FinishUploadCEGUpload#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_FinishUploadDepot_Response>> FinishUploadDepotUpload(CPartnerApps_FinishUploadDepot_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_FinishUploadDepot_Request, CPartnerApps_FinishUploadDepot_Response>( $"{SERVICE_NAME}.FinishUploadDepotUpload#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_GetDepotBuildResult_Response>> GetDepotBuildResult(CPartnerApps_GetDepotBuildResult_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_GetDepotBuildResult_Request, CPartnerApps_GetDepotBuildResult_Response>( $"{SERVICE_NAME}.GetDepotBuildResult#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_FindDRMUploads_Response>> FindDRMUploads(CPartnerApps_FindDRMUploads_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_FindDRMUploads_Request, CPartnerApps_FindDRMUploads_Response>( $"{SERVICE_NAME}.FindDRMUploads#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CPartnerApps_Download_Response>> Download(CPartnerApps_Download_Request request)
        {
            return UnifiedMessages.SendMessage<CPartnerApps_Download_Request, CPartnerApps_Download_Response>( $"{SERVICE_NAME}.Download#1", request );
        }

        internal override void HandleMsg( IPacketMsg packetMsg )
        {
            if (!SteamUnifiedMessages.CanHandleMsg( packetMsg, SERVICE_NAME, out var methodName ))
                return;

            switch ( methodName )
            {
                case "RequestKVSignUploadToken":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_RequestUploadToken_Response>( packetMsg );
                    break;
                case "RequestDRMUploadToken":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_RequestUploadToken_Response>( packetMsg );
                    break;
                case "RequestCEGUploadToken":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_RequestUploadToken_Response>( packetMsg );
                    break;
                case "RequestDepotUploadToken":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_RequestUploadToken_Response>( packetMsg );
                    break;
                case "FinishUploadKVSign":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_FinishUploadKVSign_Response>( packetMsg );
                    break;
                case "FinishUploadDRMUpload":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_FinishUploadLegacyDRM_Response>( packetMsg );
                    break;
                case "FinishUploadCEGUpload":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_FinishUpload_Response>( packetMsg );
                    break;
                case "FinishUploadDepotUpload":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_FinishUploadDepot_Response>( packetMsg );
                    break;
                case "GetDepotBuildResult":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_GetDepotBuildResult_Response>( packetMsg );
                    break;
                case "FindDRMUploads":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_FindDRMUploads_Response>( packetMsg );
                    break;
                case "Download":
                    UnifiedMessages.HandleServiceMsg<CPartnerApps_Download_Response>( packetMsg );
                    break;
            }
        }
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
