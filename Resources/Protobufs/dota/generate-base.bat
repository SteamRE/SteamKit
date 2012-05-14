@echo off

rem the base GC messages are generated from the tf2 protobufs

echo Building Dota messages...
..\..\Protogen\protogen -s:..\ -i:"matchmaker_common.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MatchmakerCommon.cs" -t:csharp -ns:"SteamKit2.GC.Dota.Internal"
..\..\Protogen\protogen -s:..\ -i:"dota_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\Dota\MsgGC.cs" -t:csharp -ns:"SteamKit2.GC.Dota.Internal"