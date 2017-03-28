@echo off

echo Building CSGO GC base...
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgBase.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCSystem.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCSDK.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCEcon.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"engine_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\SteamMsgGCEngine.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing

echo Building CSGO messages...
..\..\Protogen\protogen -s:..\ -i:"cstrike15_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\MsgGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.CSGO.Internal" -p:detectMissing
