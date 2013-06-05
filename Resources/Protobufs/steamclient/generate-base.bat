@echo off

echo Steam Messages Base
..\..\Protogen\protogen -i:"steammessages_base.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgBase.cs" -t:csharp -ns:"SteamKit2.Internal"
echo.
echo.

echo Encrypted App Ticket
..\..\Protogen\protogen -i:"encrypted_app_ticket.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgAppTicket.cs" -t:csharp -ns:"SteamKit2.Internal"
echo.
echo.

echo Steam Messages ClientServer
..\..\sed\sed "s/string serialized/bytes serialized/g" steammessages_clientserver.proto > steammessages_clientserver_asbytes.proto
..\..\Protogen\protogen -i:"steammessages_clientserver_asbytes.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServer.cs" -t:csharp -ns:"SteamKit2.Internal"
del steammessages_clientserver_asbytes.proto
echo.
echo.

echo Content Manifest
..\..\Protogen\protogen -i:"content_manifest.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\ContentManifest.cs" -t:csharp -ns:"SteamKit2.Internal"
echo.
echo.

echo IClient Objects
..\..\Protogen\protogen -i:"iclient_objects.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\IClientObjects.cs" -t:csharp -ns:"SteamKit2.Internal"
echo.
echo.

echo Unified Messages
..\..\Protogen\protogen -s:..\ -i:"steammessages_unified_base.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgUnifiedBase.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"

..\..\Protogen\protogen -s:..\ -i:"steammessages_cloud.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgCloud.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_credentials.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgCredentials.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_player.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPlayer.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"

pause