------------------------------------------------------------------------------
v 3.3.1			August 21 2025
------------------------------------------------------------------------------

* Deprecated `IsSteamDeck` and added `GamingDeviceType` to `LogOnDetails`
* Updated Steam protobufs.
* Updated dependencies.

------------------------------------------------------------------------------
v 3.3.0			June 9 2025
------------------------------------------------------------------------------

* `Configuration.HttpClientFactory` will now be used for websocket CM connections.
  * Added `HttpClientPurpose` enum to allow returning different HTTP clients for different purposes.
* Post client disconnection callback before cancelling async jobs.
  * Related to the `AsyncJob` change in the previous release.
* Added some Deadlock protobufs.
* Updated Steam protocol version.
* Updated Steam protobufs.


------------------------------------------------------------------------------
v 3.2.0			May 9 2025
------------------------------------------------------------------------------

* `AsyncJob` will now instantly fail if not connected to Steam.
  * This is technically a breaking change if you relied on this behaviour.
* Added support for zstd compressed depot chunks.
  * Added a dependency on `ZstdSharp.Port`.
* Added an `GetAuthTicketForWebApi`
* Added an argument to pass extra args to `WebAPI.CallProtobufAsync<TResponse, TRequest>()`
* Updated Steam protobufs.


------------------------------------------------------------------------------
v 3.1.0			April 21 2025
------------------------------------------------------------------------------

* Added `SteamApps.PICSGetPrivateBeta`.
* Fixed `PersonaStateCallback.StatusFlags`
* Fixed parsing old v4 depot manifests.
* `DepotChunk.Process` now validates the magic bytes of the compression.
* Updated Steam EMsg.
* Updated dependencies.


------------------------------------------------------------------------------
v 3.0.2			February 3 2025
------------------------------------------------------------------------------

* Fixed handling on tasks to reduce chance of deadlocking sync-over-async consumers.
* Fixed a crash due to library resolution errors when running under .NET 9.0.1 on macOS.
* Updated Steam protobufs.
* Updated dependencies.


------------------------------------------------------------------------------
v 3.0.1			December 29 2024
------------------------------------------------------------------------------

* Added protected `PostResponseMsg` and `PostNotificationMsg` to `UnifiedService` to allow using user-provided services.
* Added `DetectLancacheServerAsync` and `UseLancacheServer` to `CDN.Client` to allow downloading via LanCache servers.
* Updated Steam protobufs.


------------------------------------------------------------------------------
v 3.0.0			November 7 2024
------------------------------------------------------------------------------

* Added a dependency on `System.IO.Hashing`.
* Added `SteamKit2.WebUI.Internal` protobufs.
* Added `ChatMode`, `UIMode`, and `IsSteamDeck` to `LogOnDetails`.
* Added `DepotManifest.Serialize`.
* Added `SteamClient.WaitForCallbackAsync` and `CallbackManager.RunWaitCallbackAsync`.
* Added `cdnAuthToken` parameter to `CDN.Client` method for country specific servers that still require it.
* Added `SteamAuthTicket` handler.
* Added `WebAPI.AsyncInterface.CallAsync` overload that does not require specifying `HttpMethod.Get`.
* Added WebSocket as a default enabled protocol, switched to using GetCMListForConnect API.
* Added support for parsing binary keyvalues that have an alternate end byte.
* `SmartCMServerList` will now attempt to refresh itself over the WebAPI if it was last refreshed over 7 days ago.
* Updated Steam enums and protobufs.
* Various performance and memory optimizations.
* Linux machines will now fetch MAC address for the machine id.

BREAKING CHANGES
* SteamKit now targets .NET 8.
* `SteamUnifiedMessages` are now reflection-free with a new API.
  * See updated `013_UnifiedMessages` sample for new usage.
  * Requests are now generated functions like so: `UnifiedMessages.CreateService<Player>().GetGameBadgeLevels( req );`
  * If you subscribed to `ServiceMethodResponse`, use `CallbackManager.SubscribeServiceResponse` instead.
  * If you subscribed to `ServiceMethodNotification`, `CallbackManager.SubscribeServiceNotification` instead.
  * Response messages are now typed under `Body` property, calling `GetDeserializedResponse` was removed.
  * For incoming messages to be processed and decoded, the service must be registered with `CreateService` first,
    which is done for you by using the new subscribe functions on the callback manager.
* `SteamClient` callback queue is now backed by `BufferBlock`:
  * `FreeLastCallback` and `GetAllCallbacks` have been removed.
  * Calling `GetCallback` and `WaitForCallback` now always dequeues a callback, there is no more peek and "freeLast".
  * `CallbackManager.RunCallbacks` now returns bool indicating whether a callback was handled.
* `DepotManifest.ChunkData.Checksum` is now a `uint` instead of `byte[4]`.
* `DepotManifest.SaveToFile` now returns void.
* `CDN.Client.DownloadDepotChunkAsync` now requires a mandatory destination buffer.
  * Returns the number of written bytes to the destination.
  * You can rent a buffer like `ArrayPool<byte>.Shared.Rent((int)chunk.UncompressedLength)`
  * `DepotChunk` is now a static class that only contains a `Process` method.
* Moved `SteamApps.GetCDNAuthToken` to `SteamContent.GetCDNAuthToken` due to a Steam change.
* `IServerListProvider` has a new property `LastServerListRefresh` which should return an UTC DateTime
  last time the server list was refreshed.
* Removed obsolete methods and enum values.
* Removed Artifact and Underlords generated protobufs.
* Removed `SteamTrading` handler.
* Removed `RSACrypto` class.
* Removed all methods from `CryptoHelper` except for `SymmetricDecrypt`.
* Removed `ICallbackMsg` interface, simply use `CallbackMsg` instead.
* Removed `CMListCallback` as it was removed by Steam.


------------------------------------------------------------------------------
v 2.5.0			November 6 2023
------------------------------------------------------------------------------
* Added `SteamApps.GetLegacyGameKey`.
* Added `SteamUser.PlayingSessionStateCallback`.
* Added ability to serialize depot manifests.
* Added support for unauthenticated service methods.
* Added `LogOnDetails.MachineName`.
* Added support for new Steam authentication system.
* Improved TCP connection reliability.
* Update Steam enums and protobufs.
* Added `BalanceDelayed` and `LongBalanceDelayed` in `WalletInfoCallback`
* Deprecated `WebAPIUserNonce`, `RequestWebAPIUserNonce`, `SendMachineAuthResponse`, `UpdateMachineAuthCallback`

Bug Fixes
* Fixed nullability annotations on `PersonaStateCallback`.
* Fixed sending service method notifications.
* Fixed `machine_id` on Windows to be consistent with the Steam client.
* Fixed SteamLanguageParser to generate BinaryWriter/Reader that gets disposed.
* Fixed async jobs to use high precision timer for timeouts instead of wall clock..


------------------------------------------------------------------------------
v 2.4.0			December 4 2021
------------------------------------------------------------------------------
* Updated protobuf-net dependency to v3.0.
* Added a MemoryServerListProvider implementation.
* Added `CallProtobufAsync<T>` method to WebAPI to deserialize response as Protobuf instead of KeyValues.
* Added `SteamChinaOnly` flag to CDN server objects.
* Added new APIs to allow consumers to provide their own machine info.
* Added `SteamUser.VanityUrlChangedCallback`.
* Added `SteamApps.PurchaseResponseCallback`.
* Added `SteamApps.RedeemGuestPassResponseCallback`.
* Added new `SteamContent` handler.
* Added the ability to set `ClientMsgProtoBuf<T>.Body`.
* SteamKit2 now ships a net6.0 assembly as well as a netstandard2.0 assembly.
* Enabled strong name signing on the SteamKit2 assembly.
* Changed thread names to not get truncated on Linux.
* Changed message handler exception handling to log the full `Exception` object rather than just the message.
* Changed the implementation of `SteamUnifiedMessages` to use the newer message protocol under the hood.
* Removed some DebugLog spam from WebSocket connections.
* Updated C# nullability annotations.
* Updated Steam enums and protobufs.

Bug Fixes
* Fixed a race in `CMClient.Send`.
* Fixed a race in `AsyncJob` registration.
* Fixed an issue opening files in `IsolatedStorageServerListProvider`.
* Fixed throwing an exception when calling WebAPI twice with the same arguments.
* Fixed WebAPI mutating the supplied arguments dictionary.
* Fixed cryptographic errors in .NET 6 previews.
* Fixed `IDebugNetworkListener` not being given encryption handshake messages.
* Fixed a possible unhandled exception when opening a TCP connection.
* Fixed the `UnobservedTaskException` event being triggered on .NET 6 if a TCP connection times out.
* Fixed WebSocket connections constructing an incorrect IPv6 URI.
* Fixed `DisconnectedCallback.UserInitiated` being true when a connection was terminated due to an internal error.
* Fixed `SmartCMServerList` getting fixated on the first CM server when all servers are marked as bad.

BREAKING CHANGES
* `SteamWorkshop.EnumerateUserPublishedFiles` and its associated callback has been removed.
* `SteamWorkshop.EnumerateUserSubscribedFiles` and its associated callback has been removed.
* `SteamApps.PICSRequest` has been changed from a class to a struct, and `only_public` has been removed.
* A non-final optional parameter has been removed from `SteamApps.PICSGetProductInfo`. For any caller that supplied
* three unnamed arguments, this is a source-breaking change as the value for `onlyPublic` will now be passed to the
* method parameter `metaDataOnly`. Please audit your code for any calls to `PICSGetProductInfo` as the compiler will
* not warn you about this change.
* Changed `SteamApps.GetPICSProductInfo` signature to now use `PICSRequest` objects.
* Removed some old non-Protobuf messages.
* Updated nullability to match latest BCL annotations and to fix .NET 6 SDK analysis warnings.
* `SteamClient.AddHandler` will now throw an `ArgumentNullException` if the handler is null, rather than crashing on a `NullReferenceException`.
* WebAPI will now throw an `ArgumentException` if the `method` parameter is `null`, rather than crashing on a `NullReferenceException`.
* CDN timeouts options are now properties instead of fields.
* `CDNClient` has been heavily refactored and is now `SteamKit2.CDN.Client`.
* `ClientMsgProtobuf` can now only be constructed from a `PacketClientMsgProtobuf`.
* Removed `IClientMsg.Deserialize(...)` and implementations.
* Removed `ServiceMethodResponse.ResponseRaw`.
* `CDN.Server.AllowedAppIds` is now not-nullable. Check for empty instead.


------------------------------------------------------------------------------
v 2.3.0			July 05, 2020 
------------------------------------------------------------------------------
* Added `SteamMatchmaking` to manage matchmaking lobbies.
* Added `ParentalSettings` to `LoggedOnCallback`.
* Added annotations for C#8 Nullable Reference Types.
* Added initial support for an IPv6-aware Steam network.
* Added `SteamUser.EmailAddrInfoCallback`.
* Added `DepotManifest.Deserialize()`.
* Added new `Licence` fields to `SteamApps` callbacks.
* Added `CMClient.CurrentEndPoint` to expose currently connected remote server address.
* Added full update fields to `PICSChangesCallback`.
* `SteamUser.LogOn` will now use the Cell ID specified in `SteamConfiguration` as a fallback.
* SteamKit will now try skip a CM that responds to a logon attempt with `TryAnotherCM` or `ServiceUnavailable`.
* Removed debug messages for each packet send/recieve event. This can be accomplished through `NetHookNetworkListener` or a customer `IDebugNetworkListener` instead.
* Log messages now uniquely identify the related `SteamClient` object, where relevant.
* Fixed incorrect Cell ID parameter in `ContentServerDirectoryService`.
* Fixed a possible exception when calling `SteamApps` functions with duplicated inputs
* Fixed Web API exceptions not including the numeric HTTP response code.
* Fixed a couple possible crashes.
* Updated Steam enums and protobufs.

BREAKING CHANGES
* Generated protobuf classes have changed slightly due to updating to a newer version of protobuf-net.
    * Attributes vary slightly, particularly with regards to the `Name` property on `ProtoContractAttribute`, `ProtoMemberAttribute`, and `ProtoEnumAttribute`.
	* For any given property, e.g. a property named `Foo`, the corresponding property `FooSpecified` has been removed. Use `ShouldSerializeFoo()` instead.
	* Protobuf classes are no longer marked with the `[Serializable]` attribute.
* The `appid` parameter in `SteamApps.GetDepotDecryptionKey` is no longer optional.
* `CDNClient` no longer supports CS servers. Some public methods have had their signatures changed to accommodate this.
* `WebAPI` no longer logs exceptions. Callers that want this information logged should log it themselves.
* `SteamWorkshop.EnumeratePublishedFiles` has been removed. Use the unified API `IPublishedFile.QueryFiles` instead.
* All classes in the `SteamKit2.Internal.Unified` namespace have moved to `SteamKit2.Internal`.


------------------------------------------------------------------------------
v 2.2.0 	 	Jun 27, 2019
------------------------------------------------------------------------------
* Added an overload of `SteamDirectory.LoadAsync` that accepts a maximum number of servers to return.
* Added `ServerRecord.TryCreateSocketServer` to try parse a record from a string.
* Added support for initializing a `GameID` for mods and shortcuts.
* Added response details for some failed WebAPI responses. WebAPI can now throw a `WebAPIRequestException` with further details.
* Added customization options to SteamKit's underlying HTTP stack.
* Added `WalletInfoCallback.Balance64` for large Steam Wallet balances.
* Added more details to `DepotManifest`.
* Added SteamKit version to default HTTP user agent.
* Added overloads for `CDNClient.DownloadManifestAsync` and `CDNClient.DownloadDepotChunkAsync` for advanced consumers that perform their own server and key management.
* Added `ContentServerDirectoryService` for the discovery of ContentServers.
* The dynamic interface for WebAPI can now take a single un-named argument of `IDictionary<string, object>` instead of having to pass each argument as a named parameter. 
* Fixed concurrency issues with UDP connections.
* Fixed final fallback connection to Steam when server list is unavailable.
* Fixed thread safety issues in message deserialization.
* Fixed a crash on logon when network adapter information is not available from the underlying runtime.
* Fixed downloaded content silently ignoring a length mismatch. An exception will now be thrown in this case.
* Fixed WebSocket CM servers not being marked as bad by the server list, and thus not being ignored on subsequent connection attempts.
* Updated Steam protocol version.
* Updated Steam enums and protobufs.

BREAKING CHANGES
* WebAPI now takes a arguments dictionary as `Dictionary<string, object>`, rather than `Dictionary<string, string>`.
	* Arguments of type `byte[]` are converted to their Base-64 representation.
	* Arguments of value `null` are treated as empty strings.
	* All other arguments are converted to `string` by calling `object.ToString()` and encoded to a URL safe representation.


------------------------------------------------------------------------------
v 2.1.0			Jun 13, 2018
------------------------------------------------------------------------------
* Added `SimpleConsoleDebugListener`.
* Added helper methods to convert between Clan (Group) SteamIDs and their corresponding Chat SteamIDs.
* Added SourceLink to enable consumers to step through SteamKit's source code when debugging.
  * This requires Visual Studio v15.3 or higher.
* Fixed several possible crashes.
* Fixed NuGet package description.
* Fixed `UFSClient` sending an extra `DisconnectedCallback` when connecting.
* Updated Steam enums and protobufs.

BREAKING CHANGES
* `SteamFriends.SetPersonaName()` and `SteamFriends.SetPersonaState()` are no longer Job-based.


------------------------------------------------------------------------------
v 2.0.0			Dec 17, 2017
------------------------------------------------------------------------------
* SteamKit 2.0 now targets .NET Standard 2.0. This means it now requires .NET Framework 4.6.1 or higher, .NET Core 2.0 or higher, or any other .NET Standard 2.0-compatible runtime.
* Added support for WebSocket client connections.
	* Server List treats WebSocket, TCP and UDP independently, even for the same server.
	* If a server is not reachable on TCP, SteamKit will still attempt UDP and vice-versa.
* Added `SteamConfiguration`, which replaces the previous assortment of individual configuration properties and parameters.
	* If the `SteamConfiguration` permits both TCP and UDP, both can now be used (depending on server ranking).
	* See the Breaking Changes section below for further details.
* Added `ArgumentNullException` to the public API surface when passing null into methods that would have previously triggered a `NullReferenceException`.
* Added HTTPS support to `CDNClient`.
* Updated Protobuf message classes to expose a property indicating if any wire value was specified or not, and a method to clear the value.
* Updated game-related GC messages and protobufs.
* Updated Steam enums and protobufs.
* Fixed no callback being fired when calling `SteamUserStats.GetLeaderboardEntries` .
* Fixed a crash if we encountered a message that's too small to actually contain a message.
* Fixed user's name being changed to `[unassigned]` if `SteamFriends.SetPersonaState()` is called too early.
* Fixed order of handlers being non-deterministic.
* Fixed exception when using `SteamUnifiedMessages.UnifiedService<>.SendMessage`.
* Fixed a potential deadlock when awaiting an `AsyncJob`.
* Fixed TCP connections not being correctly flagged in the server list.
* Fixed TCP disconnections not being correctly flagged as user-initiated.

BREAKING CHANGES

* Removed all methods and properties that were marked as `[Obsolete]`.
* The `SteamClient` constructor now accepts a `SteamConfiguration` object, which is a container for various configuration settings.
	* If you were using the `SteamClient` constructor to specify a specific protocol type (TCP or UDP), use `SteamConfiguration` instead.
	* If you were using various properties to modify timeouts etc., use `SteamConfiguration` instead.
	* If you do not provide a `SteamConfiguration` when constructing a `SteamClient`, the server list will be private to that `SteamClient` instance.
	* If you create multiple clients from a single configuration, the server list will be shared among those clients.
	* `SteamClient.ConnectedUniverse` is now `SteamClient.Universe`. This is now set from the configuration, and is no longer `EUniverse.Invalid` when not connected.
	* `SteamClient.ConnectionTimeout` is now read-only. Setting this property is now performed on `SteamConfiguration`.
* Removed `SteamClient.ConnectedCallback.EResult`, as it could only ever be `EResult.OK`.
* Servers are now represented by `ServerRecord` objects, not IP addresses.
	* `CMListCallback` now returns a collection of `ServerRecord` objects, not IP addresses.
	* `CMListCallback` now also includes WebSocket servers.
	* `SteamClient.Connect` now optionally accepts a `ServerRecord` instead of optionally accepting an `IPEndPoint`. You can create a `ServerRecord` for a particular protocol type or set of protocol types.
	* `IServerListProvider` now deals with `ServerRecord`s instead of `IPEndPoint`s.
* Disabling server list fetching from the directory is now done via `SteamConfiguration` instead of `SmartCMServerList`.
* `SteamDirectory` helper methods now accept a `SteamConfiguration` rather than just a cellid.
* `SteamFriends.GetPersonaName()`, `SteamFriends.GetFriendPersonaName` and `SteamFriends.GetClanName` can all now return null if the value is unknown.
* `WebAPI` used to throw a `WebException` on non-success status code, or other failure. It now throws `HttpRequestException`.
* `CDNClient` and `WebAPI` now expose `Task`-based asynchronous methods. This replaces the previous synchronous methods.
* `SteamID.ToString()` now prints a Steam3 string by default. For the older Steam2 `STEAM_X:Y:Z` format, use `SteamID.Render()`.
* The default argument of `SteamID.Render(bool)` has been changed to render Steam3 by default instead of Steam2.\
* Async job continuations are no longer invoked on the `CallbackMgr` thread.


------------------------------------------------------------------------------
v 1.8.3			Mar 28, 2017
------------------------------------------------------------------------------
* Obsoleted `TradeProposedCallback.OtherName`.

This is the final release to support .NET Framework 4.5.


------------------------------------------------------------------------------
v 1.8.2			Mar 23, 2017
------------------------------------------------------------------------------
* Added support for Binary KeyValues field type 10 (`Int64`)
* Obsoleted `SteamApps.GetAppInfo`, `SteamApps.GetPackageInfo`, and `SteamApps.GetAppChanges`. Use the PICS equivalents instead.
* Updated game-related GC messages and protobufs.


------------------------------------------------------------------------------
v 1.8.1			Feb 22, 2017
------------------------------------------------------------------------------
* Added support for using CS servers that have (CDN) `usetokenauth` specified.
* Added support for newer branch passwords with `SteamApps.CheckAppBetaPassword` and `CryptoHelper.SymmetricDecryptECB`.
* Added `LastSeen` to the default info flags used by `SteamFriends.RequestFriendInfo`. (pr #313)
* Tell Steam that we support the `RateLimitExceeded` logon response. (pr #307)
* Fixed timeouts not being set for sending/receiving when using TCP. (pr #317)
* Fixed more possible crashes when querying WMI on Windows.
* Fixed concurrent calls to Disconnect possibly blocking connectLock indefinitely.
* Fixed not escaping backslashes and newlines when serializing KeyValues to text. (bug #334)
* Fixed KeyValues float parsing in cultures where comma is used as decimal separator. (bug #355)
* Updated `SteamApps.GetCDNAuthToken` to populate `depot_id`.
* Updated Steam enums and protobufs. (pr #323) (pr #326) (pr #327) (pr #328) (pr #329) (pr #330) (pr #361)
* Updated game-related GC messages and protobufs.


------------------------------------------------------------------------------
v 1.8.0			Jul 8, 2016
------------------------------------------------------------------------------
* Added `CallbackManager.RunWaitAllCallbacks` (pr #292)
* Added `KeyValue.AsUnsignedByte`. (pr #270)
* Added `KeyValue.AsUnsignedInteger`. (pr #255)
* Added `KeyValue.AsUnsignedShort`. (pr #270)
* Added `SteamUserStats.GetNumberOfCurrentPlayers(GameID)`. (pr #234)
* Added the ability to persist the server list to Isolated Storage. (pr #293)
* Added the ability to persist the server list to a file. (pr #293)
* Added support for fetching server list from the Steam Directory API. (pr #293)
* Fixed a crash on Windows if WMI is unavailable.
* Fixed a memory leak when reconnecting to Steam with the same `SteamClient` instance (pr #292)
* Updated `SteamUserStats.GetNumberOfCurrentPlayers` to use messages that Steam continues to respond to. (pr #234)
* Updated Steam enums and protobufs. (pr #271, pr #274, pr #296)
* Updated game-related GC messages and protobufs.
* Removed the hardcoded list of Steam server addresses. (pr #293)

BREAKING CHANGES
* `SmartCMServerList` APIs have changed to accomodate new server management behaviour.


------------------------------------------------------------------------------
v 1.7.0			Dec 21, 2015
------------------------------------------------------------------------------
* Added awaitable API for job-based messages. APIs which returned a `JobID` now return an `AsyncJob<>`, which can be used to asynchronously await for results. (pr #170)
* Added `SteamApps.PICSGetAccessTokens` overload with singular parameters. (pr #190)
* Added `SteamFriends.RequestMessageHistory` and `SteamFriends.RequestOfflineMessages` (pr #193)
* Added the ability to connect to Developer instances of Steam (`EUniverse.Dev`). If anyone at Valve is using this internally, hi!
* Added the ability to set a `LoginID` in `SteamUser.LogOnDetails` so that multiple instances can connect from the same host concurrenctly. (pr #217)
* Added `SteamClient.DebugNetworkListener` API to intercept and log raw messages. (pr #204)
* Added the ability to dump messages in NetHook2 format for debugging purposes. (pr #204)
* Upgraded the encryption protocol used to communicate with the Steam servers.
* Implemented protection against man-in-the-middle attacks. (pr #214)
* Server List will now maintain ordering from Steam, increasing the chances of a successful and geographically local connection. (pr #218)
* After calling `SteamUser.LogOff` or `SteamGameServer.LogOff`, `SteamClient.DisconnectedCallback.UserInitiated` will be `true`. (pr #205)
* Fixed a crash when parsing a Steam ID of the format '[i:1:234]'.
* Fixed a crash when logging on in an environment where the hard disk has no serial ID, such as Hyper-V.
* Fixed a bug when parsing a KeyValue file that contains a `/` followed by a newline. (pr #187)
* Updated Steam enums and protobufs.
* Updated game-related GC messages and protobufs.

BREAKING CHANGES
* SteamKit2 now requires .NET 4.5 or equivalent (Mono 3.0), or higher.
* Removed obsoleted `ICallbackMsg` extension methods `IsType<>` and `Handle<>`. (pr #221)
* Game Coordinator base messages are now generated per-game, instead of relying on Dota 2. GC messages should use the base messages for their game, which is separated by namespace. (pr #180)
* Cell IDs are now consistently `uint`s within `SteamDirectory`.


------------------------------------------------------------------------------
v 1.6.5			Oct 17, 2015
------------------------------------------------------------------------------
* Added inventory service unified protobufs.
* Added the ability to specify the client's prefered Cell ID in `LogOnDetails.CellID`. (pr #148)
* `KeyValue` objects can now be serialized (both text and binary) to streams with `SaveToStream`.
* Fixed an issue with `CDNClient` session initialization involving sessionid values. 
* Added setter for `KeyValue`'s indexer operator.
* Added `ELeaderboardDisplayType` and various leaderboard retrieval functions to `SteamUserStats`. (pr #153)
* Implemented machine id support for logon for when the Steam servers inevitably require it. (pr #152)
* Fixed case where logging on with a different account could lead to an anonymous logon instead. (bug #160)
* `SteamFriends.SetPersonaName` now supports `JobID`s and has a new associated callback: `PersonaChangeCallback`
* Updated game-related GC messages and protobufs.


------------------------------------------------------------------------------
v 1.6.4			Aug 03, 2015
------------------------------------------------------------------------------
* Added smarter server selection logic.
* Added ability to load initial server list from Steam Directory Web API. See `SteamDirectory.Initialize`.
* Added ability to persist internal server list. See Sample 7 for details.
* Added `SteamFriends.InviteUserToChat`.
* Added support in `SteamUser` for passwordless login with a login key.
* Added `NumChatMembers`, `ChatRoomName` and `ChatMembers` to `ChatEnterCallback`.
* Added new API for callback subscriptions, `CallbackManager.Subscribe`.
* Added `SteamApps.RequestFreeLicense` to request Free On-Demand licences.
* Exposed `ClientOSType` and `ClientLanguage` when logging in as a specific or as an anonymous user.
* Fixed `KeyValue` binary deserialization returning a dummy parent node containing the actually deserialized `KeyValue`. You must change to the new `Try`-prefixed methods to adopt the fixed behavior.
* Updated Steam enums and protobufs.
* Updated game-related GC messages and protobufs.

DEPRECATIONS
* `ICallbackMsg.IsType<>` and `ICallbackMsg.Handle<>` are deprecated and will be removed soon in a future version of SteamKit. Please use `CallbackManager.Subscribe` instead.
* `Callback<T>` is deprecated and will be removed in a future version of SteamKit. Please use `CallbackManager.Subscribe` instead.
* `KeyValue.ReadAsBinary` and `KeyValue.LoadAsBinary` are deprecated and will be removed in a future version of SteamKit. Use the `Try`-prefixed methods as outlined above.


------------------------------------------------------------------------------
v 1.6.3			Jun 20, 2015
------------------------------------------------------------------------------

* Added support for parsing older representations of Steam3 Steam IDs such as those from Counter-Strike: Global Offensive, i.e. `[M:1:123(456)]`.
* Steam IDs parsed from Steam3 string representations will now have the correct instance ID set.
* KeyValues can now be serialized to binary, however all values will be serialized as the string type.
* Improved reliability of TCP connections to the CM and UFS servers.
* Added `UserInitiated` property to `SteamClient.DisconnectedCallback` and `UFSClient.DisconnectedCallback` to indicate whether a disconnect was caused by the user, or by another source (Steam servers, bad network connection).
* Updated Steam protobufs.
* Updated game-related GC messages and protobufs.


------------------------------------------------------------------------------
v 1.6.2			Dec 16, 2014
------------------------------------------------------------------------------

*	Fixed a crash when receiving a `ServiceMethod` message.
*	Fixed `ServiceMethodCallback.RpcName` having a leading '.' character (e.g. '.MethodName' instead of 'MethodName).
*	Fixed web responses in `CDNClient` not being closed, which could lead to running out of system resources.
*	Added error handling for `ClientMsgHandler`. Any unhandled exceptions will be logged to `DebugLog` and trigger `SteamClient` to disconnect.
*	Updated `EMsg` list.
*	Updated Steam protobufs.
*	Updated game-related GC messages and protobufs.


------------------------------------------------------------------------------
v 1.6.1			Nov 30, 2014
------------------------------------------------------------------------------

*	Added support for VZip when decompressing depot chunks.
*	Improved thread safety and error handling inside `TcpConnection`.
*	Added `DownloadDepotChunk` overload for consumers who insist on connecting to particular CDNs.
* 	Updated `EResult` with the new field `NotModified`.
*	Updated `EMsg` list.
*	Updated `EOSType`.
	*	The short names for Windows versions (e.g. `Win8` instead of `Windows8`) are preferred.
	*	Addded `MacOS1010` for OS X 10.10 'Yosemite'
*	Removed various long-obsolete values from enums where the value was renamed.
*	Removed `EUniverse.RC`.
*	Updated game related GC messages and protobufs.


------------------------------------------------------------------------------
v 1.6.0			Oct 11, 2014
------------------------------------------------------------------------------

*	Updated EOSType for newer Linux and Windows versions.
*	A LoggedOnCallback with EResult.NoConnection is now posted when attempting to logon without being
	connected to the remote Steam server.
*	Fixed anonymous gameserver logon.
*	CDNClient.Server's constructor now accepts a DnsEndPoint.
*	Updated EResult with the following new fields: AccountLogonDeniedNeedTwoFactorCode, ItemDeleted,
	AccountLoginDeniedThrottle, TwoFactorCodeMismatch
*	Added public utility class for working with DateTime and unix epochs: DateUtils
*	Added GetSingleFileInfo, ShareFile and related callbacks for dealing with Steam cloud files with the
	SteamCloud handler.
*	Fixed a potential crash when failing to properly deserialize network messages.
*	Updated EMsg list.
*	Refactored the internals of tcp connections to Steam servers to be more resiliant and threadsafe.
*	CallbackMsg.Handle will now return a boolean indiciating that the passed in callback matches the
	generic type parameter.
*	Added support for logging into accounts with two-factor auth enabled. See the
	SteamUser.LogOnDetails.TwoFactorCode field.
*	Updated the bootstrap list of Steam CM servers that SteamKit will initially attempt to connect to.
*	Added SteamFriends.FriendMsgEchoCallback for echoed messages sent to other logged in client
	instances.
*	Updated game related GC messages and protobufs.

BREAKING CHANGES
*	JobCallback API has been merged with Callback. For help with transitioning code, please see the following
	wiki notes: https://github.com/SteamRE/SteamKit/wiki/JobCallback-Transition.
*	UFSClient.UploadFileResponseCallback.JobID has been renamed to RemoteJobID in order to not conflict with
	CallbackMsg's new JobID member.
*	UFSClient.UploadDetails.JobID has been renamed to RemoteJobID.
*	CDNClient has been refactored to support multiple authdepot calls for a single instance of the client
	and to support CDN servers.
*	The following EResult fields have been renamed:
		PSNAccountNotLinked -> ExternalAccountUnlinked
		InvalidPSNTicket -> PSNTicketInvalid
		PSNAccountAlreadyLinked -> ExternalAccountAlreadyLinked


------------------------------------------------------------------------------
v 1.5.1			Mar 15, 2014
------------------------------------------------------------------------------

*	Added a parameterless public constructor to DepotManifest.ChunkData to support serialization.
*	SteamWorkshop.RequestPublishedFileDetails has been obsoleted and is no longer supported. This functionality will be 
	dropped in a future SteamKit release. See the the PublishedFile WebAPI service for a functional replacement.
*	Added the request and response messages for the PublishedFile service.
*	Fixed an unhandled exception when requesting metadata-only PICS product info.
*	Exposed the following additional fields in the LoggedOnCallback: VanityURL, NumLoginFailuresToMigrate, NumDisconnectsToMigrate.
*	Exposed the HTTP url details for PICS product info, see: PICSProductInfoCallback.PICSProductInfo.HttpUri and UseHttp.
*	Added EEconTradeResponse.InitiatorPasswordResetProbation and InitiatorNewDeviceCooldown.
*	Fixed SteamGameServer.LogOn and LogOnAnonymous sending the wrong message.
*	Added support for token authentication for game server logon.
*	Added the request and response messages for the GameServers service.
*	Added the ability to specify server type for game servers, see: SteamGameServer.SendStatus.
*	Exposed a few more fields on TradeResultCallback: NumDaysSteamGuardRequired, NumDaysNewDeviceCooldown,
	DefaultNumDaysPasswordResetProbation, NumDaysPasswordResetProbation.
*	Fixed being unable to download depot manifests.
*	Added SteamID.SetFromSteam3String.
*	Obsoleted SteamApps.SendGuestPass. This functionality will be dropped in a future SteamKit release.
*	Updated EResult with the following new fields: UnexpectedError, Disabled, InvalidCEGSubmission, RestrictedDevice.
*	Updated EMsg list.
*	Updated game related GC messages.

BREAKING CHANGES
*	Fixed ServiceMethodResponse.RpcName containing a leading '.'.


------------------------------------------------------------------------------
v 1.5.0			Oct 26, 2013
------------------------------------------------------------------------------

*	Added DebugLog.ClearListeners().
*	Added WebAPI.AsyncInterface, a .NET TPL'd version of WebAPI.Interface.
*	Added SteamClient.ServerListCallback.
*	Added SteamUser.WebAPIUserNonceCallback, and a method to request it: SteamUser.RequestWebAPIUserNonce().
*	Added SteamUser.MarketingMessageCallback.
*	Added a new member to CMClient: CellID. This is the Steam server's recommended CellID.
*	Added the ability to specify AccountID in SteamUser.LogOnDetails.
*	Added a helper API to SteamUnifiedMessages for service messages.
*	Fixed issue where CallbackManager was not triggering for JobCallback<T>.
*	Fixed unhandled protobuf-net exception when (de)serializing messages with enums that are out of date.
*	Fixed a bug where all WebAPI.Interface requests would instantly timeout.
*	Fixed Manifest.HashFileName and Manifest.HashContent being swapped.
*	Updated Emsg list.
*	Updated game related GC messages.
*	Updated the following enums: EResult, EChatEntryType, EAccountFlags, EClanPermission, EFriendFlags, EOSType, EServerType,
	EBillingType, EChatMemberStateChange, EDepotFileFlag, EEconTradeResponse.
*	The following members of EChatRoomEnterResponse have been obsoleted: NoRankingDataLobby, NoRankingDataUser, RankOutOfRange.
*	EOSType.Win7 has been obsoleted and renamed to EOSType.Windows7.
*	EEconTradeResponse.InitiatorAlreadyTrading has been obsoleted and renamed to EEconTradeResponse.AlreadyTrading.
*	EEconTradeResponse.Error has been obsoleted and renamed to EEconTradeResponse.AlreadyHasTradeRequest.
*	EEconTradeResponse.Timeout has been obsoleted and renamed to EEconTradeResponse.NoResponse.
*	EChatEntryType.Emote has been obsoleted. Emotes are no longer supported by Steam.
*	SteamFriends.ProfileInfoCallback.RecentPlaytime has been obsoleted. This data is no longer sent by the Steam servers.
*	Updated to latest protobuf-net.

BREAKING CHANGES
*	SteamUser.LoggedOnCallback.Steam2Ticket is now exposed as a byte array, rather than a Steam2Ticket object.
*	The SteamKit2.Blob namespace and all related classes have been removed.
*	Support for Steam2 servers and the various classes within SteamKit have been removed.
*	CDNClient has been heavily refactored to be more developer friendly.
*	All DateTimes in callbacks are now DateTimeKind.Utc.


------------------------------------------------------------------------------
v 1.4.1			Jul 15, 2013
------------------------------------------------------------------------------

*	Added the ability to manipulate UFS (Steam cloud) files with UFSClient.
*	Added SteamScreenshots handler for interacting with user screenshots.
*	Added an optional parameter to SteamID.Render() to render SteamIDs to their Steam3 representations.
*	Added the ability to specify the timeout of WebAPI requests with Interface.Timeout.
*	The RSACrypto and KeyDictionary utility classes are now accessible to consumers.
*	Updated EMsg list.
*	Updated game related GC messages.


------------------------------------------------------------------------------
v 1.4.0			Jun 08, 2013
------------------------------------------------------------------------------

*	KeyValues now correctly writes out strings in UTF8.
*	Fixed an exception that could occur with an invalid string passed to SteamID's constructor.
*	Added SteamFriends.ClanStateCallback.
*	Added EPersonaStateFlag. This value is now exposed in SteamFriends.PersonaStateCallback.
*	Added MsgClientCreateChat and MsgClientCreateChatResponse messages.
*	Added GlobalID base class for globally unique values (such as JobIDs, UGCHandles) in Steam.
*	Updated EMsg list.
*	Updated game related GC messages.
*	Added initial support for the Steam Cloud file system with UFSClient. This feature should be considered unstable and may
	have breaking changes in the future.

BREAKING CHANGES
*	STATIC_CALLBACKS builds of SteamKit have now been completely removed.
*	Message classes for unified messages have moved namespaces from SteamKit2.Steamworks to SteamKit2.Unified.Internal.


------------------------------------------------------------------------------
v 1.3.1			Mar 10, 2013
------------------------------------------------------------------------------

*	Fixed issue where the avatar hash of a clan was always null.
*	Introduced better handling of networking related cryptographic exceptions.
*	Updated EMsg list.
*	Exposed SteamClient.JobCallback<T> for external consumers.
*	STATIC_CALLBACK builds of SteamKit and related code has been obsoleted and will be removed in the next version.
*	Implemented GameID.ToString().
*	Implemented game pass sending and recieving with SteamApps.SendGuestPass(), SteamApps.GuestPassListCallback, and
	SteamApps.SendGuestPassCallback.
*	Implemented requesting Steam community profile info with SteamFriends.RequestProfileInfo(), and SteamFriends.ProfileInfoCallback
*	CMClient now exposes a ConnectionTimeout field to control the timeout when connecting to Steam. The default timeout is 5 seconds.
*	Updated the internal list of CM servers to help alleviate some issues with connecting to dead servers.
*	Implemented SteamClient.CMListCallback to retrieve the current list of CM servers.
*	Implemented initial support for unified messages through the SteamUnifiedMessages handler.

BREAKING CHANGES
*	CMClient.Connect has been refactored significantly. It is no longer possible to use unencrypted connections. The Connect function
	now accepts an IPEndPoint to allow consumers to specify which Steam server they wish to connect to. Along with this,
	CMClient.Servers is now exposed as a collection of IPEndPoints, instead of IPAddresses.
*	SteamApps.PackageInfoCallback now exposes the immediate child KeyValue for the data, to be more consistent with
	SteamApps.AppInfoCallback.


------------------------------------------------------------------------------
v 1.3.0			Jan 16, 2013
------------------------------------------------------------------------------

*	Fixed case where friend and chat messages were incorrectly trimming the last character.
*	Steam2 ServerClient now exposes a IsConnected property.
*	Steam2 ContentServerClient can now optionally not perform a server handshake when opening a storage session.
*	Added various enums: EClanPermission, EMarketingMessageFlags, ENewsUpdateType, ESystemIMType, EChatFlags,
	ERemoteStoragePlatform, EDRMBlobDownloadType, EDRMBlobDownloadErrorDetail, EClientStat, EClientStatAggregateMethod,
	ELeaderboardDataRequest, ELeaderboardSortMethod, ELeaderboardUploadScoreMethod, and EChatPermission.
*	Fixed case where SteamKit was throwing an unhandled exception during Steam3 tcp connection teardown.
*	Added PICS support to the SteamApps handler: PICSGetAccessTokens, PICSGetChangesSince, and PICSGetProductInfo.
*	Added anonymous download support to CDNClient.
*	Updated the following enums: EMsg, EUniverse, EChatEntryType, EPersonaState, EFriendRelationship, EFriendFlags,
	EClientPersonaStateFlag, ELicenseFlags, ELicenseType, EPaymentMethod, EIntroducerRouting, EClanRank, EClanRelationship,
	EAppInfoSection, EContentDownloadSourceType, EOSType, EServerType, ECurrencyCode, EDepotFileFlag, EEconTradeResponse,
	ESystemIMType, ERemoteStoragePlatform, and EResult.
*	Exposed the following properties in SteamUser.LoggedOnCallback: CellIDPingThreshold, UsePICS, WebAPIUserNonce, and 
	IPCountryCode.
*	Fixed case where SteamKit was incorrectly handling certain logoff messages during Steam server unavailability.
*	Fixed potential crash in Steam2 ContentServerClient when opening a storage session.
*	Updated to latest protobuf-net.

BREAKING CHANGES
*	DepotManifest.ChunkData.CRC is now named DepotManifest.ChunkData.Checksum.


------------------------------------------------------------------------------
v 1.2.2			Nov 11, 2012
------------------------------------------------------------------------------

*	Fixed critical issue that occured while serializing protobuf messages.


------------------------------------------------------------------------------
v 1.2.1			Nov 11, 2012
------------------------------------------------------------------------------

*	Added EPersonaState.LookingToTrade and EPersonaState.LookingToPlay.
*	Added SteamFriends.UnbanChatMember.
*	Removed GeneralDSClient.GetAuthServerList as Steam2 auth servers no longer exist.
*	Removed dependency on Classless.Hasher.
*	Updated to latest protobuf-net.


------------------------------------------------------------------------------
v 1.2.0			Nov 04, 2012
------------------------------------------------------------------------------

*	Fixed issue where LoginKeyCallback was being passed incorrect data.
*	Fixed ClientGCMsg PacketMessage constructor.
*	WebAPI list and array parameters are now accepted and flattened to x[n]=y format.
*	Fixed KeyValue issue when multiple duplicate children exist.
*	Updated protobuf definitions for internal message classes to their latest definitions.
*	Updated EMsgs.
*	Fixed critical MsgMulti handling.
*	Added EEconTradeResponse.
*	Added SteamTrading client message handler.
*	Modified Steam3 TCP socket shutdown to play well with Mono.
*	Modified CMClient.Connect method to be properly async.
*	Implemented friend blocking/unblocking with SteamFriends.IgnoreFriend and SteamFriends.IgnoreFriendCallback.
*	Fixed gameserver logon.
*	Local user is now given the persona name [unassigned] before SteamUser.AccountInfoCallback comes in.
*	Updated SteamKit2's bootstrap CM list, this should reduce how often SK2 will connect to an offline/dead server.
*	Steam2 ServerClient's now expose a ConnectionTimeout member.

BREAKING CHANGES
*	Dota GC EMsgs are now longer located in SteamKit2.GC.Dota.EGCMsg, they are now in SteamKit2.Gc.Dota.Internal.EDOTAGCMsg.
*	Base GC EMsgs are now longer located in SyteamKit2.GC.EGCMsgBase, they are now in multiple enums in the SteamKit2.GC.Internal namespace:
	EGCBaseMsg, EGCSystemMsg, EGCSharedMsg, ESOMsg, EGCItemMsg
*	SteamApps.AppInfoCallback now exposes the immediate child KeyValue for every Section, instead of an empty root parent.


------------------------------------------------------------------------------
v 1.1.0			May 14, 2012
------------------------------------------------------------------------------

*	Added SteamWorkshop for enumerating and requesting details of published workshop files.
*	Large overhaul of SteamGameCoordinator to support the sending and receiving of GC messages.
*	Added SteamFriends ChatInviteCallback.
*	Added SteamFriends KickChatMember and BanChatMember.
*	Fixed invalid handling of PackageInfoCallback response.
*	Updated protobuf definitions for internal message classes to their latest definitions.

BREAKING CHANGES
*	Consumers of SteamClient.JobCallback<T> will have to change their handler functions to take a "JobID" parameter instead of a "ulong".
	These are functionally equivalent, and JobIDs can be implicitly casted to and from ulongs.


------------------------------------------------------------------------------
v 1.0.0			Feb 26, 2012
------------------------------------------------------------------------------

*	Initial release.
