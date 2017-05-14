@echo off

echo Building Dota GC base...
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\SteamMsgBase.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\SteamMsgGCSystem.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\SteamMsgGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\SteamMsgGCSDK.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"econ_shared_enums.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\SteamMsgGCEconSharedEnums.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\SteamMsgGCEcon.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing

echo Building Steamworks Unified Messages
..\..\Protogen\protogen -s:..\ -i:"steammessages_oauth.steamworkssdk.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\Steamworks\SteamMsgOAuthSteamworks.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal.Steamworks" -p:detectMissing

echo Building Dota messages...
..\..\Protogen\protogen -s:..\ -i:"network_connection.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\NetworkConnection.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing

..\..\Protogen\protogen -s:..\ -i:"dota_client_enums.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgClientEnums.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_shared_enums.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgSharedEnums.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_common.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCCommon.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_common_match_management.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCCommonMatchMgmt.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_msgid.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCMsgId.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClient.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_chat.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientChat.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_fantasy.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientFantasy.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_guild.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientGuild.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_match_management.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientMatchMgmt.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_team.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientTeam.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_tournament.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientTournament.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client_watch.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClientWatch.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_server.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCServer.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"dota_match_metadata.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgMatchMetadata.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Dota.Internal" -p:detectMissing
