@echo off

rem the base GC messages are generated from the tf2 protobufs

echo Building CSGO messages...
..\..\Protogen\protogen -s:..\ -i:"cstrike15_gcmessages.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\GC\CSGO\MsgGC.cs" -t:csharp -ns:"SteamKit2.GC.CSGO.Internal"