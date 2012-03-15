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
..\..\Protogen\protogen -i:"steammessages_clientserver.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServer.cs" -t:csharp -ns:"SteamKit2.Internal"
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

pause