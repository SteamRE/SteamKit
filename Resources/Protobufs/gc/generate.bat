@echo off

..\..\Protogen\protogen -s:..\ -i:"gc.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\MsgBaseGC.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.GC.Internal" -p:detectMissing
