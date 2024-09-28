// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: steammessages_oauth.steamworkssdk.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
namespace SteamKit2.Internal.Steamworks
{

    [global::ProtoBuf.ProtoContract()]
    public partial class COAuthToken_ImplicitGrantNoPrompt_Request : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string clientid
        {
            get => __pbn__clientid ?? "";
            set => __pbn__clientid = value;
        }
        public bool ShouldSerializeclientid() => __pbn__clientid != null;
        public void Resetclientid() => __pbn__clientid = null;
        private string __pbn__clientid;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class COAuthToken_ImplicitGrantNoPrompt_Response : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string access_token
        {
            get => __pbn__access_token ?? "";
            set => __pbn__access_token = value;
        }
        public bool ShouldSerializeaccess_token() => __pbn__access_token != null;
        public void Resetaccess_token() => __pbn__access_token = null;
        private string __pbn__access_token;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string redirect_uri
        {
            get => __pbn__redirect_uri ?? "";
            set => __pbn__redirect_uri = value;
        }
        public bool ShouldSerializeredirect_uri() => __pbn__redirect_uri != null;
        public void Resetredirect_uri() => __pbn__redirect_uri = null;
        private string __pbn__redirect_uri;

    }

    public class OAuthToken : SteamUnifiedMessages.UnifiedService
    {
        public override string ServiceName { get; } = "OAuthToken";

        public AsyncJob<SteamUnifiedMessages.ServiceMethodResponse<COAuthToken_ImplicitGrantNoPrompt_Response>> ImplicitGrantNoPrompt(COAuthToken_ImplicitGrantNoPrompt_Request request)
        {
            return UnifiedMessages.SendMessage<COAuthToken_ImplicitGrantNoPrompt_Request, COAuthToken_ImplicitGrantNoPrompt_Response>( "OAuthToken.ImplicitGrantNoPrompt#1", request );
        }

        public override void HandleMsg( string methodName, IPacketMsg packetMsg )
        {
            switch ( methodName )
            {
                case "ImplicitGrantNoPrompt":
                    UnifiedMessages.HandleServiceMsg<COAuthToken_ImplicitGrantNoPrompt_Response>( packetMsg );
                    break;
            }
        }
    }

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
