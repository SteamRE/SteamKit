@echo off

echo Steam Messages Base
..\..\Protogen\protogen -i:"steammessages_base.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgBase.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
echo.
echo.

echo Encrypted App Ticket
..\..\Protogen\protogen -i:"encrypted_app_ticket.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgAppTicket.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
echo.
echo.

echo Steam Messages ClientServer
..\..\Protogen\protogen -i:"steammessages_clientserver.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServer.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
..\..\Protogen\protogen -i:"steammessages_clientserver_2.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServer2.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
..\..\Protogen\protogen -i:"steammessages_clientserver_friends.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServerFriends.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
..\..\Protogen\protogen -i:"steammessages_clientserver_login.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgClientServerLogin.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
..\..\Protogen\protogen -i:"steammessages_sitelicenseclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\SteamMsgSiteLicenseClient.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing

echo.
echo.

echo Content Manifest
..\..\Protogen\protogen -i:"content_manifest.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\ContentManifest.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Internal" -p:detectMissing
echo.
echo.

echo Unified Messages
..\..\Protogen\protogen -s:..\ -i:"steammessages_unified_base.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgUnifiedBase.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing

..\..\Protogen\protogen -s:..\ -i:"steammessages_broadcast.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgBroadcast.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_cloud.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgCloud.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_credentials.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgCredentials.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_datapublisher.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgDataPublisher.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_depotbuilder.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgDepotBuilder.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_deviceauth.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgDeviceAuth.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_econ.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgEcon.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_gamenotifications.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgGameNotifications.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_gameservers.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgGameServers.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_linkfilter.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgLinkFilter.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_inventory.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgInventory.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_offline.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgOffline.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_parental.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgParental.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_partnerapps.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPartnerApps.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_physicalgoods.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPhysicalGoods.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_player.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPlayer.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_publishedfile.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgPublishedFile.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_secrets.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgSecrets.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_site_license.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgSiteLicense.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_twofactor.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgTwoFactor.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing-p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_useraccount.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgUserAccount.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing
..\..\Protogen\protogen -s:..\ -i:"steammessages_video.steamclient.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\Generated\Unified\SteamMsgVideo.cs" -t:csharp -p:lightFramework=true -ns:"SteamKit2.Unified.Internal" -p:detectMissing

pause