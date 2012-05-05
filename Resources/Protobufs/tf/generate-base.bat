@echo off

rem we use tf2 as the GC message base since it's the most updated

echo Building GC base
..\..\Protogen\protogen -s:..\ -i:"steammessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgBase.cs" -t:csharp -ns:"SteamKit2.Internal.GC"
..\..\Protogen\protogen -s:..\ -i:"base_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgGC.cs" -t:csharp -ns:"SteamKit2.Internal.GC"
..\..\Protogen\protogen -s:..\ -i:"gcsdk_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\SteamMsgGCSDK.cs" -t:csharp -ns:"SteamKit2.Internal.GC"

echo Building TF2 GC messages
..\..\Protogen\protogen -s:..\ -i:"tf_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\TF2\MsgGC.cs" -t:csharp -ns:"SteamKit2.Internal.GC.TF2"