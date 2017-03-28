@echo off

echo Building TF2 GC base...
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\SteamMsgBase.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.TF2.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsystemmsgs.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\SteamMsgGCSystem.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.TF2.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\SteamMsgGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.TF2.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\SteamMsgGCSDK.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.TF2.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"econ_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\SteamMsgGCEcon.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.TF2.Internal" -p:detectMissing

echo Building TF2 GC messages
..\..\Protogen\protogen -s:..\ -i:"tf_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\MsgGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.TF2.Internal" -p:detectMissing
