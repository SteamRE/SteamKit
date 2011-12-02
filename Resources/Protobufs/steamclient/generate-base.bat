..\..\Protogen\protogen -i:"steammessages_base.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\SteamMsgBase.cs" -t:csharp -ns:"SteamKit2"
..\..\Protogen\protogen -i:"encrypted_app_ticket.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\SteamMsgAppTicket.cs" -t:csharp -ns:"SteamKit2"
..\..\Protogen\protogen -i:"steammessages_clientserver.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\SteamMsgClientServer.cs" -t:csharp -ns:"SteamKit2"
..\..\Protogen\protogen -i:"content_manifest.proto" -o:"..\..\..\SteamKit2\SteamKit2\Base\ContentManifest.cs" -t:csharp -ns:"SteamKit2"