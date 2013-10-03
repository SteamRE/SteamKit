@echo off

rem we use dota as the GC message base since it's the most updated

echo Building GC base
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgBase.cs" -t:csharp -ns:"SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgGCSystem.cs" -t:csharp -ns:"SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgGC.cs" -t:csharp -ns:"SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgGCSDK.cs" -t:csharp -ns:"SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgGCEcon.cs" -t:csharp -ns:"SteamKit2.GC.Internal"

echo Building Dota messages...
..\..\Protogen\protogen -s:..\ -i:"matchmaker_common.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MatchmakerCommon.cs" -t:csharp -ns:"SteamKit2.GC.Dota.Internal"

:: dota messages reference some types from the gc base, so we need to import the reference for it
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_common.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCCommon.cs" -t:csharp -ns:"SteamKit2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_client.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCClient.cs" -t:csharp -ns:"SteamKit2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages_server.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGCServer.cs" -t:csharp -ns:"SteamKit2.GC.Dota.Internal" -p:import="SteamKit2.GC.Internal"