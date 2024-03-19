// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: service_cloudconfigstore.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.WebUI.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_Change_Notification : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(2)]
        public global::System.Collections.Generic.List<CCloudConfigStore_NamespaceVersion> versions { get; } = new global::System.Collections.Generic.List<CCloudConfigStore_NamespaceVersion>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_Download_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<CCloudConfigStore_NamespaceVersion> versions { get; } = new global::System.Collections.Generic.List<CCloudConfigStore_NamespaceVersion>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_Download_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<CCloudConfigStore_NamespaceData> data { get; } = new global::System.Collections.Generic.List<CCloudConfigStore_NamespaceData>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_Entry : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string key
        {
            get => __pbn__key ?? "";
            set => __pbn__key = value;
        }
        public bool ShouldSerializekey() => __pbn__key != null;
        public void Resetkey() => __pbn__key = null;
        private string __pbn__key;

        [global::ProtoBuf.ProtoMember(2)]
        public bool is_deleted
        {
            get => __pbn__is_deleted.GetValueOrDefault();
            set => __pbn__is_deleted = value;
        }
        public bool ShouldSerializeis_deleted() => __pbn__is_deleted != null;
        public void Resetis_deleted() => __pbn__is_deleted = null;
        private bool? __pbn__is_deleted;

        [global::ProtoBuf.ProtoMember(3)]
        [global::System.ComponentModel.DefaultValue("")]
        public string value
        {
            get => __pbn__value ?? "";
            set => __pbn__value = value;
        }
        public bool ShouldSerializevalue() => __pbn__value != null;
        public void Resetvalue() => __pbn__value = null;
        private string __pbn__value;

        [global::ProtoBuf.ProtoMember(4, DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public uint timestamp
        {
            get => __pbn__timestamp.GetValueOrDefault();
            set => __pbn__timestamp = value;
        }
        public bool ShouldSerializetimestamp() => __pbn__timestamp != null;
        public void Resettimestamp() => __pbn__timestamp = null;
        private uint? __pbn__timestamp;

        [global::ProtoBuf.ProtoMember(5)]
        public ulong version
        {
            get => __pbn__version.GetValueOrDefault();
            set => __pbn__version = value;
        }
        public bool ShouldSerializeversion() => __pbn__version != null;
        public void Resetversion() => __pbn__version = null;
        private ulong? __pbn__version;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_NamespaceData : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint enamespace
        {
            get => __pbn__enamespace.GetValueOrDefault();
            set => __pbn__enamespace = value;
        }
        public bool ShouldSerializeenamespace() => __pbn__enamespace != null;
        public void Resetenamespace() => __pbn__enamespace = null;
        private uint? __pbn__enamespace;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong version
        {
            get => __pbn__version.GetValueOrDefault();
            set => __pbn__version = value;
        }
        public bool ShouldSerializeversion() => __pbn__version != null;
        public void Resetversion() => __pbn__version = null;
        private ulong? __pbn__version;

        [global::ProtoBuf.ProtoMember(3)]
        public global::System.Collections.Generic.List<CCloudConfigStore_Entry> entries { get; } = new global::System.Collections.Generic.List<CCloudConfigStore_Entry>();

        [global::ProtoBuf.ProtoMember(4)]
        public ulong horizon
        {
            get => __pbn__horizon.GetValueOrDefault();
            set => __pbn__horizon = value;
        }
        public bool ShouldSerializehorizon() => __pbn__horizon != null;
        public void Resethorizon() => __pbn__horizon = null;
        private ulong? __pbn__horizon;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_NamespaceVersion : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public uint enamespace
        {
            get => __pbn__enamespace.GetValueOrDefault();
            set => __pbn__enamespace = value;
        }
        public bool ShouldSerializeenamespace() => __pbn__enamespace != null;
        public void Resetenamespace() => __pbn__enamespace = null;
        private uint? __pbn__enamespace;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong version
        {
            get => __pbn__version.GetValueOrDefault();
            set => __pbn__version = value;
        }
        public bool ShouldSerializeversion() => __pbn__version != null;
        public void Resetversion() => __pbn__version = null;
        private ulong? __pbn__version;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_Upload_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<CCloudConfigStore_NamespaceData> data { get; } = new global::System.Collections.Generic.List<CCloudConfigStore_NamespaceData>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CCloudConfigStore_Upload_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public global::System.Collections.Generic.List<CCloudConfigStore_NamespaceVersion> versions { get; } = new global::System.Collections.Generic.List<CCloudConfigStore_NamespaceVersion>();

    }

    public interface ICloudConfigStore
    {
        CCloudConfigStore_Download_Response Download(CCloudConfigStore_Download_Request request);
        CCloudConfigStore_Upload_Response Upload(CCloudConfigStore_Upload_Request request);
    }

    public interface ICloudConfigStoreClient
    {
        NoResponse NotifyChange(CCloudConfigStore_Change_Notification request);
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
