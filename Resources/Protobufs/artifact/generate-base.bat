@echo off

echo Building Artifact GC base...
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\SteamMsgBase.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\SteamMsgGCSystem.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\SteamMsgGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\SteamMsgGCSDK.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"econ_shared_enums.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\SteamMsgGCEconSharedEnums.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\SteamMsgGCEcon.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing

echo Building Steamworks Unified Messages
..\..\Protogen\protogen -s:..\ -i:"steammessages_oauth.steamworkssdk.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\Steamworks\SteamMsgOAuthSteamworks.cs" -t:csharp -ns:"SteamKit2.Unified.Internal.Steamworks" -p:detectMissing

echo Building Artifact messages...
REM ..\..\Protogen\protogen -s:..\ -i:"network_connection.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\NetworkConnection.cs" -t:csharp -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing

..\..\Protogen\protogen -s:..\ -i:"dcg_gcmessages_common.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\MsgGCCommon.cs" -t:csharp -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dcg_gcmessages_client.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\MsgGCClient.cs" -t:csharp -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dcg_gcmessages_server.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Artifact\MsgGCServer.cs" -t:csharp -ns:"SteamKit2.GC.Artifact.Internal" -p:detectMissing
