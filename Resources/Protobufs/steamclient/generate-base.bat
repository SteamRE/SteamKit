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
..\..\Protogen\protogen -i:"steammessages_clientserver_2.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServer2.cs" -t:csharp -ns:"SteamKit2.Internal"
del steammessages_clientserver_asbytes.proto
echo.
echo.

echo Content Manifest
..\..\Protogen\protogen -i:"content_manifest.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\ContentManifest.cs" -t:csharp -ns:"SteamKit2.Internal"
echo.
echo.

echo Unified Messages
..\..\Protogen\protogen -s:..\ -i:"steammessages_unified_base.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgUnifiedBase.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"

..\..\Protogen\protogen -s:..\ -i:"steammessages_broadcast.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgBroadcast.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_cloud.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgCloud.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_credentials.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgCredentials.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_depotbuilder.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgDepotBuilder.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_deviceauth.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgDeviceAuth.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_econ.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgEcon.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_gamenotifications.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgGameNotifications.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_gameservers.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgGameServers.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_linkfilter.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgLinkFilter.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_offline.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgOffline.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_parental.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgParental.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_partnerapps.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPartnerApps.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_player.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPlayer.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_publishedfile.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPublishedFile.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_secrets.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgSecrets.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_twofactor.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgTwoFactor.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"
..\..\Protogen\protogen -s:..\ -i:"steammessages_video.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgVideo.cs" -t:csharp -ns:"SteamKit2.Unified.Internal"

pause