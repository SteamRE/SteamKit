// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: steammessages_datapublisher.steamclient.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_ClientContentCorruptionReport_Notification : global::ProtoBuf.IExtensible
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
        public string download_source
        {
            get => __pbn__download_source ?? "";
            set => __pbn__download_source = value;
        }
        public bool ShouldSerializedownload_source() => __pbn__download_source != null;
        public void Resetdownload_source() => __pbn__download_source = null;
        private string __pbn__download_source;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue("")]
        public string objectid
        {
            get => __pbn__objectid ?? "";
            set => __pbn__objectid = value;
        }
        public bool ShouldSerializeobjectid() => __pbn__objectid != null;
        public void Resetobjectid() => __pbn__objectid = null;
        private string __pbn__objectid;

        [global::ProtoBuf.ProtoMember(5)]
        public uint cellid
        {
            get => __pbn__cellid.GetValueOrDefault();
            set => __pbn__cellid = value;
        }
        public bool ShouldSerializecellid() => __pbn__cellid != null;
        public void Resetcellid() => __pbn__cellid = null;
        private uint? __pbn__cellid;

        [global::ProtoBuf.ProtoMember(6)]
        public bool is_manifest
        {
            get => __pbn__is_manifest.GetValueOrDefault();
            set => __pbn__is_manifest = value;
        }
        public bool ShouldSerializeis_manifest() => __pbn__is_manifest != null;
        public void Resetis_manifest() => __pbn__is_manifest = null;
        private bool? __pbn__is_manifest;

        [global::ProtoBuf.ProtoMember(7)]
        public ulong object_size
        {
            get => __pbn__object_size.GetValueOrDefault();
            set => __pbn__object_size = value;
        }
        public bool ShouldSerializeobject_size() => __pbn__object_size != null;
        public void Resetobject_size() => __pbn__object_size = null;
        private ulong? __pbn__object_size;

        [global::ProtoBuf.ProtoMember(8)]
        public uint corruption_type
        {
            get => __pbn__corruption_type.GetValueOrDefault();
            set => __pbn__corruption_type = value;
        }
        public bool ShouldSerializecorruption_type() => __pbn__corruption_type != null;
        public void Resetcorruption_type() => __pbn__corruption_type = null;
        private uint? __pbn__corruption_type;

        [global::ProtoBuf.ProtoMember(9)]
        public bool used_https
        {
            get => __pbn__used_https.GetValueOrDefault();
            set => __pbn__used_https = value;
        }
        public bool ShouldSerializeused_https() => __pbn__used_https != null;
        public void Resetused_https() => __pbn__used_https = null;
        private bool? __pbn__used_https;

        [global::ProtoBuf.ProtoMember(10)]
        public bool oc_proxy_detected
        {
            get => __pbn__oc_proxy_detected.GetValueOrDefault();
            set => __pbn__oc_proxy_detected = value;
        }
        public bool ShouldSerializeoc_proxy_detected() => __pbn__oc_proxy_detected != null;
        public void Resetoc_proxy_detected() => __pbn__oc_proxy_detected = null;
        private bool? __pbn__oc_proxy_detected;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_ClientUpdateAppJob_Notification : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint app_id
        {
            get => __pbn__app_id.GetValueOrDefault();
            set => __pbn__app_id = value;
        }
        public bool ShouldSerializeapp_id() => __pbn__app_id != null;
        public void Resetapp_id() => __pbn__app_id = null;
        private uint? __pbn__app_id;

        [global::ProtoBuf.ProtoMember(2)]
        public global::System.Collections.Generic.List<uint> depot_ids { get; } = new global::System.Collections.Generic.List<uint>();

        [global::ProtoBuf.ProtoMember(3)]
        public uint app_state
        {
            get => __pbn__app_state.GetValueOrDefault();
            set => __pbn__app_state = value;
        }
        public bool ShouldSerializeapp_state() => __pbn__app_state != null;
        public void Resetapp_state() => __pbn__app_state = null;
        private uint? __pbn__app_state;

        [global::ProtoBuf.ProtoMember(4)]
        public uint job_app_error
        {
            get => __pbn__job_app_error.GetValueOrDefault();
            set => __pbn__job_app_error = value;
        }
        public bool ShouldSerializejob_app_error() => __pbn__job_app_error != null;
        public void Resetjob_app_error() => __pbn__job_app_error = null;
        private uint? __pbn__job_app_error;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue("")]
        public string error_details
        {
            get => __pbn__error_details ?? "";
            set => __pbn__error_details = value;
        }
        public bool ShouldSerializeerror_details() => __pbn__error_details != null;
        public void Reseterror_details() => __pbn__error_details = null;
        private string __pbn__error_details;

        [global::ProtoBuf.ProtoMember(6)]
        public uint job_duration
        {
            get => __pbn__job_duration.GetValueOrDefault();
            set => __pbn__job_duration = value;
        }
        public bool ShouldSerializejob_duration() => __pbn__job_duration != null;
        public void Resetjob_duration() => __pbn__job_duration = null;
        private uint? __pbn__job_duration;

        [global::ProtoBuf.ProtoMember(7)]
        public uint files_validation_failed
        {
            get => __pbn__files_validation_failed.GetValueOrDefault();
            set => __pbn__files_validation_failed = value;
        }
        public bool ShouldSerializefiles_validation_failed() => __pbn__files_validation_failed != null;
        public void Resetfiles_validation_failed() => __pbn__files_validation_failed = null;
        private uint? __pbn__files_validation_failed;

        [global::ProtoBuf.ProtoMember(8)]
        public ulong job_bytes_downloaded
        {
            get => __pbn__job_bytes_downloaded.GetValueOrDefault();
            set => __pbn__job_bytes_downloaded = value;
        }
        public bool ShouldSerializejob_bytes_downloaded() => __pbn__job_bytes_downloaded != null;
        public void Resetjob_bytes_downloaded() => __pbn__job_bytes_downloaded = null;
        private ulong? __pbn__job_bytes_downloaded;

        [global::ProtoBuf.ProtoMember(9)]
        public ulong job_bytes_staged
        {
            get => __pbn__job_bytes_staged.GetValueOrDefault();
            set => __pbn__job_bytes_staged = value;
        }
        public bool ShouldSerializejob_bytes_staged() => __pbn__job_bytes_staged != null;
        public void Resetjob_bytes_staged() => __pbn__job_bytes_staged = null;
        private ulong? __pbn__job_bytes_staged;

        [global::ProtoBuf.ProtoMember(10)]
        public ulong bytes_comitted
        {
            get => __pbn__bytes_comitted.GetValueOrDefault();
            set => __pbn__bytes_comitted = value;
        }
        public bool ShouldSerializebytes_comitted() => __pbn__bytes_comitted != null;
        public void Resetbytes_comitted() => __pbn__bytes_comitted = null;
        private ulong? __pbn__bytes_comitted;

        [global::ProtoBuf.ProtoMember(11)]
        public uint start_app_state
        {
            get => __pbn__start_app_state.GetValueOrDefault();
            set => __pbn__start_app_state = value;
        }
        public bool ShouldSerializestart_app_state() => __pbn__start_app_state != null;
        public void Resetstart_app_state() => __pbn__start_app_state = null;
        private uint? __pbn__start_app_state;

        [global::ProtoBuf.ProtoMember(12, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong stats_machine_id
        {
            get => __pbn__stats_machine_id.GetValueOrDefault();
            set => __pbn__stats_machine_id = value;
        }
        public bool ShouldSerializestats_machine_id() => __pbn__stats_machine_id != null;
        public void Resetstats_machine_id() => __pbn__stats_machine_id = null;
        private ulong? __pbn__stats_machine_id;

        [global::ProtoBuf.ProtoMember(13)]
        [global::System.ComponentModel.DefaultValue("")]
        public string branch_name
        {
            get => __pbn__branch_name ?? "";
            set => __pbn__branch_name = value;
        }
        public bool ShouldSerializebranch_name() => __pbn__branch_name != null;
        public void Resetbranch_name() => __pbn__branch_name = null;
        private string __pbn__branch_name;

        [global::ProtoBuf.ProtoMember(14)]
        public ulong total_bytes_downloaded
        {
            get => __pbn__total_bytes_downloaded.GetValueOrDefault();
            set => __pbn__total_bytes_downloaded = value;
        }
        public bool ShouldSerializetotal_bytes_downloaded() => __pbn__total_bytes_downloaded != null;
        public void Resettotal_bytes_downloaded() => __pbn__total_bytes_downloaded = null;
        private ulong? __pbn__total_bytes_downloaded;

        [global::ProtoBuf.ProtoMember(15)]
        public ulong total_bytes_staged
        {
            get => __pbn__total_bytes_staged.GetValueOrDefault();
            set => __pbn__total_bytes_staged = value;
        }
        public bool ShouldSerializetotal_bytes_staged() => __pbn__total_bytes_staged != null;
        public void Resettotal_bytes_staged() => __pbn__total_bytes_staged = null;
        private ulong? __pbn__total_bytes_staged;

        [global::ProtoBuf.ProtoMember(16)]
        public ulong total_bytes_restored
        {
            get => __pbn__total_bytes_restored.GetValueOrDefault();
            set => __pbn__total_bytes_restored = value;
        }
        public bool ShouldSerializetotal_bytes_restored() => __pbn__total_bytes_restored != null;
        public void Resettotal_bytes_restored() => __pbn__total_bytes_restored = null;
        private ulong? __pbn__total_bytes_restored;

        [global::ProtoBuf.ProtoMember(17)]
        public bool is_borrowed
        {
            get => __pbn__is_borrowed.GetValueOrDefault();
            set => __pbn__is_borrowed = value;
        }
        public bool ShouldSerializeis_borrowed() => __pbn__is_borrowed != null;
        public void Resetis_borrowed() => __pbn__is_borrowed = null;
        private bool? __pbn__is_borrowed;

        [global::ProtoBuf.ProtoMember(18)]
        public bool is_free_weekend
        {
            get => __pbn__is_free_weekend.GetValueOrDefault();
            set => __pbn__is_free_weekend = value;
        }
        public bool ShouldSerializeis_free_weekend() => __pbn__is_free_weekend != null;
        public void Resetis_free_weekend() => __pbn__is_free_weekend = null;
        private bool? __pbn__is_free_weekend;

        [global::ProtoBuf.ProtoMember(20)]
        public ulong total_bytes_patched
        {
            get => __pbn__total_bytes_patched.GetValueOrDefault();
            set => __pbn__total_bytes_patched = value;
        }
        public bool ShouldSerializetotal_bytes_patched() => __pbn__total_bytes_patched != null;
        public void Resettotal_bytes_patched() => __pbn__total_bytes_patched = null;
        private ulong? __pbn__total_bytes_patched;

        [global::ProtoBuf.ProtoMember(21)]
        public ulong total_bytes_saved
        {
            get => __pbn__total_bytes_saved.GetValueOrDefault();
            set => __pbn__total_bytes_saved = value;
        }
        public bool ShouldSerializetotal_bytes_saved() => __pbn__total_bytes_saved != null;
        public void Resettotal_bytes_saved() => __pbn__total_bytes_saved = null;
        private ulong? __pbn__total_bytes_saved;

        [global::ProtoBuf.ProtoMember(22)]
        public uint cell_id
        {
            get => __pbn__cell_id.GetValueOrDefault();
            set => __pbn__cell_id = value;
        }
        public bool ShouldSerializecell_id() => __pbn__cell_id != null;
        public void Resetcell_id() => __pbn__cell_id = null;
        private uint? __pbn__cell_id;

        [global::ProtoBuf.ProtoMember(23)]
        public bool is_workshop
        {
            get => __pbn__is_workshop.GetValueOrDefault();
            set => __pbn__is_workshop = value;
        }
        public bool ShouldSerializeis_workshop() => __pbn__is_workshop != null;
        public void Resetis_workshop() => __pbn__is_workshop = null;
        private bool? __pbn__is_workshop;

        [global::ProtoBuf.ProtoMember(24)]
        public bool is_shader
        {
            get => __pbn__is_shader.GetValueOrDefault();
            set => __pbn__is_shader = value;
        }
        public bool ShouldSerializeis_shader() => __pbn__is_shader != null;
        public void Resetis_shader() => __pbn__is_shader = null;
        private bool? __pbn__is_shader;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_GetVRDeviceInfo_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint month_count
        {
            get => __pbn__month_count.GetValueOrDefault();
            set => __pbn__month_count = value;
        }
        public bool ShouldSerializemonth_count() => __pbn__month_count != null;
        public void Resetmonth_count() => __pbn__month_count = null;
        private uint? __pbn__month_count;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_GetVRDeviceInfo_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<Device> device { get; } = new global::System.Collections.Generic.List<Device>();

        [global::ProtoBuf.ProtoContract()]
        public partial class Device : global::ProtoBuf.IExtensible
        {
            private global::ProtoBuf.IExtension __pbn__extensionData;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [global::ProtoBuf.ProtoMember(1)]
            [global::System.ComponentModel.DefaultValue("")]
            public string name
            {
                get => __pbn__name ?? "";
                set => __pbn__name = value;
            }
            public bool ShouldSerializename() => __pbn__name != null;
            public void Resetname() => __pbn__name = null;
            private string __pbn__name;

            [global::ProtoBuf.ProtoMember(2)]
            public uint @ref
            {
                get => __pbn__ref.GetValueOrDefault();
                set => __pbn__ref = value;
            }
            public bool ShouldSerializeref() => __pbn__ref != null;
            public void Resetref() => __pbn__ref = null;
            private uint? __pbn__ref;

            [global::ProtoBuf.ProtoMember(3)]
            public uint aggregation_ref
            {
                get => __pbn__aggregation_ref.GetValueOrDefault();
                set => __pbn__aggregation_ref = value;
            }
            public bool ShouldSerializeaggregation_ref() => __pbn__aggregation_ref != null;
            public void Resetaggregation_ref() => __pbn__aggregation_ref = null;
            private uint? __pbn__aggregation_ref;

            [global::ProtoBuf.ProtoMember(4)]
            public uint total
            {
                get => __pbn__total.GetValueOrDefault();
                set => __pbn__total = value;
            }
            public bool ShouldSerializetotal() => __pbn__total != null;
            public void Resettotal() => __pbn__total = null;
            private uint? __pbn__total;

            [global::ProtoBuf.ProtoMember(5)]
            [global::System.ComponentModel.DefaultValue("")]
            public string driver
            {
                get => __pbn__driver ?? "";
                set => __pbn__driver = value;
            }
            public bool ShouldSerializedriver() => __pbn__driver != null;
            public void Resetdriver() => __pbn__driver = null;
            private string __pbn__driver;

            [global::ProtoBuf.ProtoMember(6)]
            public int device_class
            {
                get => __pbn__device_class.GetValueOrDefault();
                set => __pbn__device_class = value;
            }
            public bool ShouldSerializedevice_class() => __pbn__device_class != null;
            public void Resetdevice_class() => __pbn__device_class = null;
            private int? __pbn__device_class;

        }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_SetVRDeviceInfoAggregationReference_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint @ref
        {
            get => __pbn__ref.GetValueOrDefault();
            set => __pbn__ref = value;
        }
        public bool ShouldSerializeref() => __pbn__ref != null;
        public void Resetref() => __pbn__ref = null;
        private uint? __pbn__ref;

        [global::ProtoBuf.ProtoMember(2)]
        public uint aggregation_ref
        {
            get => __pbn__aggregation_ref.GetValueOrDefault();
            set => __pbn__aggregation_ref = value;
        }
        public bool ShouldSerializeaggregation_ref() => __pbn__aggregation_ref != null;
        public void Resetaggregation_ref() => __pbn__aggregation_ref = null;
        private uint? __pbn__aggregation_ref;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_SetVRDeviceInfoAggregationReference_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint result
        {
            get => __pbn__result.GetValueOrDefault();
            set => __pbn__result = value;
        }
        public bool ShouldSerializeresult() => __pbn__result != null;
        public void Resetresult() => __pbn__result = null;
        private uint? __pbn__result;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_AddVRDeviceInfo_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string manufacturer
        {
            get => __pbn__manufacturer ?? "";
            set => __pbn__manufacturer = value;
        }
        public bool ShouldSerializemanufacturer() => __pbn__manufacturer != null;
        public void Resetmanufacturer() => __pbn__manufacturer = null;
        private string __pbn__manufacturer;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string model
        {
            get => __pbn__model ?? "";
            set => __pbn__model = value;
        }
        public bool ShouldSerializemodel() => __pbn__model != null;
        public void Resetmodel() => __pbn__model = null;
        private string __pbn__model;

        [global::ProtoBuf.ProtoMember(3)]
        [global::System.ComponentModel.DefaultValue("")]
        public string driver
        {
            get => __pbn__driver ?? "";
            set => __pbn__driver = value;
        }
        public bool ShouldSerializedriver() => __pbn__driver != null;
        public void Resetdriver() => __pbn__driver = null;
        private string __pbn__driver;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue("")]
        public string controller_type
        {
            get => __pbn__controller_type ?? "";
            set => __pbn__controller_type = value;
        }
        public bool ShouldSerializecontroller_type() => __pbn__controller_type != null;
        public void Resetcontroller_type() => __pbn__controller_type = null;
        private string __pbn__controller_type;

        [global::ProtoBuf.ProtoMember(5)]
        public int device_class
        {
            get => __pbn__device_class.GetValueOrDefault();
            set => __pbn__device_class = value;
        }
        public bool ShouldSerializedevice_class() => __pbn__device_class != null;
        public void Resetdevice_class() => __pbn__device_class = null;
        private int? __pbn__device_class;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CDataPublisher_AddVRDeviceInfo_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint result
        {
            get => __pbn__result.GetValueOrDefault();
            set => __pbn__result = value;
        }
        public bool ShouldSerializeresult() => __pbn__result != null;
        public void Resetresult() => __pbn__result = null;
        private uint? __pbn__result;

        [global::ProtoBuf.ProtoMember(2)]
        public uint @ref
        {
            get => __pbn__ref.GetValueOrDefault();
            set => __pbn__ref = value;
        }
        public bool ShouldSerializeref() => __pbn__ref != null;
        public void Resetref() => __pbn__ref = null;
        private uint? __pbn__ref;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CValveHWSurvey_GetSurveySchedule_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string surveydatetoken
        {
            get => __pbn__surveydatetoken ?? "";
            set => __pbn__surveydatetoken = value;
        }
        public bool ShouldSerializesurveydatetoken() => __pbn__surveydatetoken != null;
        public void Resetsurveydatetoken() => __pbn__surveydatetoken = null;
        private string __pbn__surveydatetoken;

        [global::ProtoBuf.ProtoMember(2, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong surveydatetokenversion
        {
            get => __pbn__surveydatetokenversion.GetValueOrDefault();
            set => __pbn__surveydatetokenversion = value;
        }
        public bool ShouldSerializesurveydatetokenversion() => __pbn__surveydatetokenversion != null;
        public void Resetsurveydatetokenversion() => __pbn__surveydatetokenversion = null;
        private ulong? __pbn__surveydatetokenversion;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CValveHWSurvey_GetSurveySchedule_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint surveydatetoken
        {
            get => __pbn__surveydatetoken.GetValueOrDefault();
            set => __pbn__surveydatetoken = value;
        }
        public bool ShouldSerializesurveydatetoken() => __pbn__surveydatetoken != null;
        public void Resetsurveydatetoken() => __pbn__surveydatetoken = null;
        private uint? __pbn__surveydatetoken;

        [global::ProtoBuf.ProtoMember(2, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong surveydatetokenversion
        {
            get => __pbn__surveydatetokenversion.GetValueOrDefault();
            set => __pbn__surveydatetokenversion = value;
        }
        public bool ShouldSerializesurveydatetokenversion() => __pbn__surveydatetokenversion != null;
        public void Resetsurveydatetokenversion() => __pbn__surveydatetokenversion = null;
        private ulong? __pbn__surveydatetokenversion;

    }

    public class DataPublisher : SteamUnifiedMessages.UnifiedService
    {
        public override string ServiceName { get; } = "DataPublisher";

        public void ClientContentCorruptionReport(CDataPublisher_ClientContentCorruptionReport_Notification request)
        {
            UnifiedMessages.SendNotification<CDataPublisher_ClientContentCorruptionReport_Notification>( "DataPublisher.ClientContentCorruptionReport#1", request );
        }

        public void ClientUpdateAppJobReport(CDataPublisher_ClientUpdateAppJob_Notification request)
        {
            UnifiedMessages.SendNotification<CDataPublisher_ClientUpdateAppJob_Notification>( "DataPublisher.ClientUpdateAppJobReport#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CDataPublisher_GetVRDeviceInfo_Response>> GetVRDeviceInfo(CDataPublisher_GetVRDeviceInfo_Request request)
        {
            return UnifiedMessages.SendMessage<CDataPublisher_GetVRDeviceInfo_Request, CDataPublisher_GetVRDeviceInfo_Response>( "DataPublisher.GetVRDeviceInfo#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CDataPublisher_SetVRDeviceInfoAggregationReference_Response>> SetVRDeviceInfoAggregationReference(CDataPublisher_SetVRDeviceInfoAggregationReference_Request request)
        {
            return UnifiedMessages.SendMessage<CDataPublisher_SetVRDeviceInfoAggregationReference_Request, CDataPublisher_SetVRDeviceInfoAggregationReference_Response>( "DataPublisher.SetVRDeviceInfoAggregationReference#1", request );
        }

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CDataPublisher_AddVRDeviceInfo_Response>> AddVRDeviceInfo(CDataPublisher_AddVRDeviceInfo_Request request)
        {
            return UnifiedMessages.SendMessage<CDataPublisher_AddVRDeviceInfo_Request, CDataPublisher_AddVRDeviceInfo_Response>( "DataPublisher.AddVRDeviceInfo#1", request );
        }

        public override void HandleMsg( string methodName, IPacketMsg packetMsg )
        {
            switch ( methodName )
            {
                case "GetVRDeviceInfo":
                    UnifiedMessages.HandleServiceMsg<CDataPublisher_GetVRDeviceInfo_Response>( packetMsg );
                    break;
                case "SetVRDeviceInfoAggregationReference":
                    UnifiedMessages.HandleServiceMsg<CDataPublisher_SetVRDeviceInfoAggregationReference_Response>( packetMsg );
                    break;
                case "AddVRDeviceInfo":
                    UnifiedMessages.HandleServiceMsg<CDataPublisher_AddVRDeviceInfo_Response>( packetMsg );
                    break;
            }
        }
    }

    public class ValveHWSurvey : SteamUnifiedMessages.UnifiedService
    {
        public override string ServiceName { get; } = "ValveHWSurvey";

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<CValveHWSurvey_GetSurveySchedule_Response>> GetSurveySchedule(CValveHWSurvey_GetSurveySchedule_Request request)
        {
            return UnifiedMessages.SendMessage<CValveHWSurvey_GetSurveySchedule_Request, CValveHWSurvey_GetSurveySchedule_Response>( "ValveHWSurvey.GetSurveySchedule#1", request );
        }

        public override void HandleMsg( string methodName, IPacketMsg packetMsg )
        {
            switch ( methodName )
            {
                case "GetSurveySchedule":
                    UnifiedMessages.HandleServiceMsg<CValveHWSurvey_GetSurveySchedule_Response>( packetMsg );
                    break;
            }
        }
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
