@echo off

echo Building CSGO GC base...
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgBase.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCSystem.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGC.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCSDK.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCEcon.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"

echo Building CSGO messages...
..\..\Protogen\protogen -s:..\ -i:"cstrike15_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\MsgGC.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"
