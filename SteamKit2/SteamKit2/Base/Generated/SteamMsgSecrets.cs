// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: steammessages_secrets.steamclient.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.Internal
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CKeyEscrow_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public byte[] rsa_oaep_sha_ticket
        {
            get => __pbn__rsa_oaep_sha_ticket;
            set => __pbn__rsa_oaep_sha_ticket = value;
        }
        public bool ShouldSerializersa_oaep_sha_ticket() => __pbn__rsa_oaep_sha_ticket != null;
        public void Resetrsa_oaep_sha_ticket() => __pbn__rsa_oaep_sha_ticket = null;
        private byte[] __pbn__rsa_oaep_sha_ticket;

        [global::ProtoBuf.ProtoMember(2)]
        public byte[] password
        {
            get => __pbn__password;
            set => __pbn__password = value;
        }
        public bool ShouldSerializepassword() => __pbn__password != null;
        public void Resetpassword() => __pbn__password = null;
        private byte[] __pbn__password;

        [global::ProtoBuf.ProtoMember(3)]
        [global::System.ComponentModel.DefaultValue(EKeyEscrowUsage.k_EKeyEscrowUsageStreamingDevice)]
        public EKeyEscrowUsage usage
        {
            get => __pbn__usage ?? EKeyEscrowUsage.k_EKeyEscrowUsageStreamingDevice;
            set => __pbn__usage = value;
        }
        public bool ShouldSerializeusage() => __pbn__usage != null;
        public void Resetusage() => __pbn__usage = null;
        private EKeyEscrowUsage? __pbn__usage;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue("")]
        public string device_name
        {
            get => __pbn__device_name ?? "";
            set => __pbn__device_name = value;
        }
        public bool ShouldSerializedevice_name() => __pbn__device_name != null;
        public void Resetdevice_name() => __pbn__device_name = null;
        private string __pbn__device_name;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CKeyEscrow_Ticket : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public byte[] password
        {
            get => __pbn__password;
            set => __pbn__password = value;
        }
        public bool ShouldSerializepassword() => __pbn__password != null;
        public void Resetpassword() => __pbn__password = null;
        private byte[] __pbn__password;

        [global::ProtoBuf.ProtoMember(2)]
        public ulong identifier
        {
            get => __pbn__identifier.GetValueOrDefault();
            set => __pbn__identifier = value;
        }
        public bool ShouldSerializeidentifier() => __pbn__identifier != null;
        public void Resetidentifier() => __pbn__identifier = null;
        private ulong? __pbn__identifier;

        [global::ProtoBuf.ProtoMember(3)]
        public byte[] payload
        {
            get => __pbn__payload;
            set => __pbn__payload = value;
        }
        public bool ShouldSerializepayload() => __pbn__payload != null;
        public void Resetpayload() => __pbn__payload = null;
        private byte[] __pbn__payload;

        [global::ProtoBuf.ProtoMember(4)]
        public uint timestamp
        {
            get => __pbn__timestamp.GetValueOrDefault();
            set => __pbn__timestamp = value;
        }
        public bool ShouldSerializetimestamp() => __pbn__timestamp != null;
        public void Resettimestamp() => __pbn__timestamp = null;
        private uint? __pbn__timestamp;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue(EKeyEscrowUsage.k_EKeyEscrowUsageStreamingDevice)]
        public EKeyEscrowUsage usage
        {
            get => __pbn__usage ?? EKeyEscrowUsage.k_EKeyEscrowUsageStreamingDevice;
            set => __pbn__usage = value;
        }
        public bool ShouldSerializeusage() => __pbn__usage != null;
        public void Resetusage() => __pbn__usage = null;
        private EKeyEscrowUsage? __pbn__usage;

        [global::ProtoBuf.ProtoMember(6)]
        [global::System.ComponentModel.DefaultValue("")]
        public string device_name
        {
            get => __pbn__device_name ?? "";
            set => __pbn__device_name = value;
        }
        public bool ShouldSerializedevice_name() => __pbn__device_name != null;
        public void Resetdevice_name() => __pbn__device_name = null;
        private string __pbn__device_name;

        [global::ProtoBuf.ProtoMember(7)]
        [global::System.ComponentModel.DefaultValue("")]
        public string device_model
        {
            get => __pbn__device_model ?? "";
            set => __pbn__device_model = value;
        }
        public bool ShouldSerializedevice_model() => __pbn__device_model != null;
        public void Resetdevice_model() => __pbn__device_model = null;
        private string __pbn__device_model;

        [global::ProtoBuf.ProtoMember(8)]
        [global::System.ComponentModel.DefaultValue("")]
        public string device_serial
        {
            get => __pbn__device_serial ?? "";
            set => __pbn__device_serial = value;
        }
        public bool ShouldSerializedevice_serial() => __pbn__device_serial != null;
        public void Resetdevice_serial() => __pbn__device_serial = null;
        private string __pbn__device_serial;

        [global::ProtoBuf.ProtoMember(9)]
        public uint device_provisioning_id
        {
            get => __pbn__device_provisioning_id.GetValueOrDefault();
            set => __pbn__device_provisioning_id = value;
        }
        public bool ShouldSerializedevice_provisioning_id() => __pbn__device_provisioning_id != null;
        public void Resetdevice_provisioning_id() => __pbn__device_provisioning_id = null;
        private uint? __pbn__device_provisioning_id;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CKeyEscrow_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public CKeyEscrow_Ticket ticket { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public enum EKeyEscrowUsage
    {
        k_EKeyEscrowUsageStreamingDevice = 0,
    }

    public class Secrets : SteamUnifiedMessages.UnifiedService
    {
        internal override string ServiceName { get; } = "Secrets";

        public AsyncJob<SteamUnifiedMessages.ServiceMsg<CKeyEscrow_Response>> KeyEscrow(CKeyEscrow_Request request)
        {
            return UnifiedMessages.SendMessage<CKeyEscrow_Request, CKeyEscrow_Response>( $"{SERVICE_NAME}.KeyEscrow#1", request );
        }

        internal override void HandleMsg( string methodName, IPacketMsg packetMsg )
        {
            switch ( methodName )
            {
                case "KeyEscrow":
                    UnifiedMessages.HandleServiceMsg<CKeyEscrow_Response>( packetMsg );
                    break;
            }
        }
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
