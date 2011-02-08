/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamKit2
{
	public interface ISteamSerializable
	{
		byte[] Serialize();
		void Deserialize( MemoryStream ms );
	}
	public interface ISteamSerializableHeader : ISteamSerializable
	{
		void SetEMsg( EMsg msg );
	}
	public interface ISteamSerializableMessage : ISteamSerializable
	{
		EMsg GetEMsg();
	}

	public enum EMsg
	{
		Invalid = 0,
		Multi = 1,
		GenericReply = 100,
		DestJobFailed = 113,
		Alert = 115,
		SCIDRequest = 120,
		SCIDResponse = 121,
		JobHeartbeat = 123,
		Subscribe = 126,
		k_EMRouteMessage = 127,
		RemoteSysID = 128,
		AMCreateAccountResponse = 129,
		WGRequest = 130,
		WGResponse = 131,
		KeepAlive = 132,
		WebAPIJobRequest = 133,
		WebAPIJobResponse = 134,
		ClientSessionStart = 135,
		ClientSessionEnd = 136,
		ClientSessionUpdateAuthTicket = 137,
		Stats = 138,
		Ping = 139,
		PingResponse = 140,
		AssignSysID = 200,
		Exit = 201,
		DirRequest = 202,
		DirResponse = 203,
		ZipRequest = 204,
		ZipResponse = 205,
		UpdateRecordResponse = 215,
		UpdateCreditCardRequest = 221,
		UpdateUserBanResponse = 225,
		PrepareToExit = 226,
		ContentDescriptionUpdate = 227,
		TestResetServer = 228,
		UniverseChanged = 229,
		Heartbeat = 300,
		ShellFailed = 301,
		ExitShells = 307,
		ExitShell = 308,
		GracefulExitShell = 309,
		NotifyWatchdog = 314,
		LicenseProcessingComplete = 316,
		SetTestFlag = 317,
		QueuedEmailsComplete = 318,
		GMReportPHPError = 319,
		GMDRMSync = 320,
		PhysicalBoxInventory = 321,
		UpdateConfigFile = 322,
		AISRefreshContentDescription = 401,
		AISRequestContentDescription = 402,
		AISUpdateAppInfo = 403,
		AISUpdatePackageInfo = 404,
		AISGetPackageChangeNumber = 405,
		AISGetPackageChangeNumberResponse = 406,
		AISAppInfoTableChanged = 407,
		AISUpdatePackageInfoResponse = 408,
		AISCreateMarketingMessage = 409,
		AISCreateMarketingMessageResponse = 410,
		AISGetMarketingMessage = 411,
		AISGetMarketingMessageResponse = 412,
		AISUpdateMarketingMessage = 413,
		AISUpdateMarketingMessageResponse = 414,
		AISRequestMarketingMessageUpdate = 415,
		AISDeleteMarketingMessage = 416,
		AISGetMarketingTreatments = 419,
		AISGetMarketingTreatmentsResponse = 420,
		AISRequestMarketingTreatmentUpdate = 421,
		AMUpdateUserBanRequest = 504,
		AMAddLicense = 505,
		AMBeginProcessingLicenses = 507,
		AMSendSystemIMToUser = 508,
		AMExtendLicense = 509,
		AMAddMinutesToLicense = 510,
		AMCancelLicense = 511,
		AMInitPurchase = 512,
		AMPurchaseResponse = 513,
		AMGetFinalPrice = 514,
		AMGetFinalPriceResponse = 515,
		AMGetLegacyGameKey = 516,
		AMGetLegacyGameKeyResponse = 517,
		AMFindHungTransactions = 518,
		AMSetAccountTrustedRequest = 519,
		AMCompletePurchase = 521,
		AMCancelPurchase = 522,
		AMNewChallenge = 523,
		AMFixPendingPurchase = 526,
		AMIsUserBanned = 527,
		AMRegisterKey = 528,
		AMLoadActivationCodes = 529,
		AMLoadActivationCodesResponse = 530,
		AMLookupKeyResponse = 531,
		AMLookupKey = 532,
		AMChatCleanup = 533,
		AMClanCleanup = 534,
		AMFixPendingRefund = 535,
		AMReverseChargeback = 536,
		AMReverseChargebackResponse = 537,
		AMClanCleanupList = 538,
		AllowUserToPlayQuery = 550,
		AllowUserToPlayResponse = 551,
		AMVerfiyUser = 552,
		AMClientNotPlaying = 553,
		AMClientRequestFriendship = 554,
		AMRelayPublishStatus = 555,
		AMResetCommunityContent = 556,
		CAMPrimePersonaStateCache = 557,
		AMAllowUserContentQuery = 558,
		AMAllowUserContentResponse = 559,
		AMInitPurchaseResponse = 560,
		AMRevokePurchaseResponse = 561,
		AMLockProfile = 562,
		AMRefreshGuestPasses = 563,
		AMInviteUserToClan = 564,
		AMAcknowledgeClanInvite = 565,
		AMGrantGuestPasses = 566,
		AMClanDataUpdated = 567,
		AMReloadAccount = 568,
		AMClientChatMsgRelay = 569,
		AMChatMulti = 570,
		AMClientChatInviteRelay = 571,
		AMChatInvite = 572,
		AMClientJoinChatRelay = 573,
		AMClientChatMemberInfoRelay = 574,
		AMPublishChatMemberInfo = 575,
		AMClientAcceptFriendInvite = 576,
		AMChatEnter = 577,
		AMClientPublishRemovalFromSource = 578,
		AMChatActionResult = 579,
		AMFindAccounts = 580,
		AMFindAccountsResponse = 581,
		AMSetAccountFlags = 584,
		AMCreateClan = 586,
		AMCreateClanResponse = 587,
		AMGetClanDetails = 588,
		AMGetClanDetailsResponse = 589,
		AMSetPersonaName = 590,
		AMSetAvatar = 591,
		AMAuthenticateUser = 592,
		AMAuthenticateUserResponse = 593,
		AMGetAccountFriendsCount = 594,
		AMGetAccountFriendsCountResponse = 595,
		AMP2PIntroducerMessage = 596,
		ClientChatAction = 597,
		AMClientChatActionRelay = 598,
		ReqChallenge = 600,
		VACResponse = 601,
		ReqChallengeTest = 602,
		VSInitDB = 603,
		VSMarkCheat = 604,
		VSAddCheat = 605,
		VSPurgeCodeModDB = 606,
		VSGetChallengeResults = 607,
		VSChallengeResultText = 608,
		VSReportLingerer = 609,
		VSRequestManagedChallenge = 610,
		DRMBuildBlobRequest = 628,
		DRMBuildBlobResponse = 629,
		DRMResolveGuidRequest = 630,
		DRMResolveGuidResponse = 631,
		DRMVariabilityReport = 633,
		DRMVariabilityReportResponse = 634,
		DRMStabilityReport = 635,
		DRMStabilityReportResponse = 636,
		DRMDetailsReportRequest = 637,
		DRMDetailsReportResponse = 638,
		DRMProcessFile = 639,
		DRMAdminUpdate = 640,
		DRMAdminUpdateResponse = 641,
		DRMSync = 642,
		DRMSyncResposne = 643,
		DRMProcessFileResponse = 644,
		CSManifestUpdate = 651,
		CSUserContentRequest = 652,
		ClientLogOn_Deprecated = 701,
		ClientAnonLogOn_Deprecated = 702,
		ClientHeartBeat = 703,
		ClientVACResponse = 704,
		ClientLogOff = 706,
		ClientNoUDPConnectivity = 707,
		ClientInformOfCreateAccount = 708,
		ClientAckVACBan = 709,
		ClientConnectionStats = 710,
		ClientInitPurchase = 711,
		ClientPingResponse = 712,
		ClientAddFriend = 713,
		ClientRemoveFriend = 714,
		ClientGamesPlayedNoDataBlob = 715,
		ClientChangeStatus = 716,
		ClientVacStatusResponse = 717,
		ClientFriendMsg = 718,
		ClientGetFinalPrice = 722,
		ClientSystemIM = 726,
		ClientSystemIMAck = 727,
		ClientGetLicenses = 728,
		ClientCancelLicense = 729,
		ClientGetLegacyGameKey = 730,
		ClientContentServerLogOn_Deprecated = 731,
		ClientAckVACBan2 = 732,
		ClientCompletePurchase = 733,
		ClientCancelPurchase = 734,
		ClientAckMessageByGID = 735,
		ClientGetPurchaseReceipts = 736,
		ClientAckPurchaseReceipt = 737,
		ClientSendGuestPass = 739,
		ClientAckGuestPass = 740,
		ClientRedeemGuestPass = 741,
		ClientGamesPlayed = 742,
		ClientRegisterKey = 743,
		ClientInviteUserToClan = 744,
		ClientAcknowledgeClanInvite = 745,
		ClientPurchaseWithMachineID = 746,
		ClientAppUsageEvent = 747,
		ClientGetGiftTargetList = 748,
		ClientGetGiftTargetListResponse = 749,
		ClientLogOnResponse = 751,
		ClientVACChallenge = 753,
		ClientSetHeartbeatRate = 755,
		ClientNotLoggedOnDeprecated = 756,
		ClientLoggedOff = 757,
		GSApprove = 758,
		GSDeny = 759,
		GSKick = 760,
		ClientCreateAcctResponse = 761,
		ClientPurchaseResponse = 763,
		ClientPing = 764,
		ClientNOP = 765,
		ClientPersonaState = 766,
		ClientFriendsList = 767,
		ClientAccountInfo = 768,
		ClientAddFriendResponse = 769,
		ClientVacStatusQuery = 770,
		ClientNewsUpdate = 771,
		ClientGameConnectDeny = 773,
		GSStatusReply = 774,
		ClientGetFinalPriceResponse = 775,
		ClientGameConnectTokens = 779,
		ClientLicenseList = 780,
		ClientCancelLicenseResponse = 781,
		ClientVACBanStatus = 782,
		ClientCMList = 783,
		ClientEncryptPct = 784,
		ClientGetLegacyGameKeyResponse = 785,
		CSUserContentApprove = 787,
		CSUserContentDeny = 788,
		ClientInitPurchaseResponse = 789,
		ClientAddFriend2 = 791,
		ClientAddFriendResponse2 = 792,
		ClientInviteFriend = 793,
		ClientInviteFriendResponse = 794,
		ClientSendGuestPassResponse = 795,
		ClientAckGuestPassResponse = 796,
		ClientRedeemGuestPassResponse = 797,
		ClientUpdateGuestPassesList = 798,
		ClientChatMsg = 799,
		ClientChatInvite = 800,
		ClientJoinChat = 801,
		ClientChatMemberInfo = 802,
		ClientLogOnWithCredentials_Deprecated = 803,
		ClientPasswordChange = 804,
		ClientPasswordChangeResponse = 805,
		ClientChatEnter = 807,
		ClientFriendRemovedFromSource = 808,
		ClientCreateChat = 809,
		ClientCreateChatResponse = 810,
		ClientUpdateChatMetadata = 811,
		ClientP2PIntroducerMessage = 813,
		ClientChatActionResult = 814,
		ClientRequestFriendData = 815,
		ClientOneTimeWGAuthPassword = 816,
		ClientGetUserStats = 818,
		ClientGetUserStatsResponse = 819,
		ClientStoreUserStats = 820,
		ClientStoreUserStatsResponse = 821,
		ClientClanState = 822,
		ClientServiceModule = 830,
		ClientServiceCall = 831,
		ClientServiceCallResponse = 832,
		ClientNatTraversalStatEvent = 839,
		ClientAppInfoRequest = 840,
		ClientAppInfoResponse = 841,
		ClientSteamUsageEvent = 842,
		ClientEmailChange = 843,
		ClientPersonalQAChange = 844,
		ClientCheckPassword = 845,
		ClientResetPassword = 846,
		ClientCheckPasswordResponse = 848,
		ClientResetPasswordResponse = 849,
		ClientSessionToken = 850,
		ClientDRMProblemReport = 851,
		ClientSetIgnoreFriend = 855,
		ClientSetIgnoreFriendResponse = 856,
		ClientGetAppOwnershipTicket = 857,
		ClientGetAppOwnershipTicketResponse = 858,
		ClientGetLobbyListResponse = 860,
		ClientGetLobbyMetadata = 861,
		ClientGetLobbyMetadataResponse = 862,
		ClientVTTCert = 863,
		ClientAppInfoUpdate = 866,
		ClientAppInfoChanges = 867,
		ClientServerList = 880,
		ClientGetFriendsLobbies = 888,
		ClientGetFriendsLobbiesResponse = 889,
		ClientGetLobbyList = 890,
		ClientEmailChangeResponse = 891,
		ClientSecretQAChangeResponse = 892,
		ClientPasswordChange2 = 893,
		ClientEmailChange2 = 894,
		ClientPersonalQAChange2 = 895,
		ClientDRMBlobRequest = 896,
		ClientDRMBlobResponse = 897,
		ClientLookupKey = 898,
		ClientLookupKeyResponse = 899,
		GSDisconnectNotice = 901,
		GSStatus = 903,
		GSUserPlaying = 905,
		GSStatus2 = 906,
		GSStatusUpdate_Unused = 907,
		GSServerType = 908,
		GSPlayerList = 909,
		GSGetUserAchievementStatus = 910,
		GSGetUserAchievementStatusResponse = 911,
		GSGetPlayStats = 918,
		GSGetPlayStatsResponse = 919,
		GSGetUserGroupStatus = 920,
		GSGetUserGroupStatusResponse = 923,
		GSGetReputation = 936,
		GSGetReputationResponse = 937,
		ChannelEncryptRequest = 1303,
		ChannelEncryptResponse = 1304,
		ChannelEncryptResult = 1305,
		ClientChatRoomInfo = 4026,
		ClientUFSUploadFileRequest = 5202,
		ClientUFSUploadFileResponse = 5203,
		ClientUFSUploadFileChunk = 5204,
		ClientUFSUploadFileFinished = 5205,
		ClientUFSGetFileListForApp = 5206,
		ClientUFSGetFileListForAppResponse = 5207,
		ClientUFSDownloadRequest = 5210,
		ClientUFSDownloadResponse = 5211,
		ClientUFSDownloadChunk = 5212,
		ClientUFSLoginRequest = 5213,
		ClientUFSLoginResponse = 5214,
		ClientUFSTransferHeartbeat = 5216,
		ClientUFSDeleteFileRequest = 5219,
		ClientUFSDeleteFileResponse = 5220,
		ClientUFSGetUGCDetails = 5226,
		ClientUFSGetUGCDetailsResponse = 5227,
		ClientUFSGetSingleFileInfo = 5230,
		ClientUFSGetSingleFileInfoResponse = 5231,
		ClientUFSShareFile = 5232,
		ClientUFSShareFileResponse = 5233,
		ClientRequestForgottenPasswordEmail = 5401,
		ClientRequestForgottenPasswordEmailResponse = 5402,
		ClientCreateAccountResponse = 5403,
		ClientResetForgottenPassword = 5404,
		ClientResetForgottenPasswordResponse = 5405,
		ClientCreateAccount2 = 5406,
		ClientInformOfResetForgottenPassword = 5407,
		ClientInformOfResetForgottenPasswordResponse = 5408,
		ClientAnonUserLogOn_Deprecated = 5409,
		ClientGamesPlayedWithDataBlob = 5410,
		ClientUpdateUserGameInfo = 5411,
		ClientFileToDownload = 5412,
		ClientFileToDownloadResponse = 5413,
		ClientLBSSetScore = 5414,
		ClientLBSSetScoreResponse = 5415,
		ClientLBSFindOrCreateLB = 5416,
		ClientLBSFindOrCreateLBResponse = 5417,
		ClientLBSGetLBEntries = 5418,
		ClientLBSGetLBEntriesResponse = 5419,
		ClientMarketingMessageUpdate = 5420,
		ClientChatDeclined = 5426,
		ClientFriendMsgIncoming = 5427,
		ClientAuthList_Deprecated = 5428,
		ClientTicketAuthComplete = 5429,
		ClientIsLimitedAccount = 5430,
		ClientAuthList = 5432,
		ClientStat = 5433,
		ClientP2PConnectionInfo = 5434,
		ClientP2PConnectionFailInfo = 5435,
		ClientGetNumberOfCurrentPlayers = 5436,
		ClientGetNumberOfCurrentPlayersResponse = 5437,
		ClientGetDepotDecryptionKey = 5438,
		ClientGetDepotDecryptionKeyResponse = 5439,
		GSPerformHardwareSurvey = 5440,
		ClientEnableTestLicense = 5443,
		ClientEnableTestLicenseResponse = 5444,
		ClientDisableTestLicense = 5445,
		ClientDisableTestLicenseResponse = 5446,
		ClientRequestValidationMail = 5448,
		ClientRequestValidationMailResponse = 5449,
		ClientToGC = 5452,
		ClientFromGC = 5453,
		ClientRequestChangeMail = 5454,
		ClientRequestChangeMailResponse = 5455,
		ClientEmailAddrInfo = 5456,
		ClientPasswordChange3 = 5457,
		ClientEmailChange3 = 5458,
		ClientPersonalQAChange3 = 5459,
		ClientResetForgottenPassword3 = 5460,
		ClientRequestForgottenPasswordEmail3 = 5461,
		ClientCreateAccount3 = 5462,
		ClientNewLoginKey = 5463,
		ClientNewLoginKeyAccepted = 5464,
		ClientLogOnWithHash_Deprecated = 5465,
		ClientStoreUserStats2 = 5466,
		ClientStatsUpdated = 5467,
		ClientActivateOEMLicense = 5468,
		ClientRequestedClientStats = 5480,
		ClientStat2Int32 = 5481,
		ClientStat2 = 5482,
		ClientVerifyPassword = 5483,
		ClientVerifyPasswordResponse = 5484,
		ClientDRMDownloadRequest = 5485,
		ClientDRMDownloadResponse = 5486,
		ClientDRMFinalResult = 5487,
		ClientGetFriendsWhoPlayGame = 5488,
		ClientGetFriendsWhoPlayGameResponse = 5489,
		ClientOGSBeginSession = 5490,
		ClientOGSBeginSessionResponse = 5491,
		ClientOGSEndSession = 5492,
		ClientOGSEndSessionResponse = 5493,
		ClientOGSWriteRow = 5494,
		ClientDRMTest = 5495,
		ClientDRMTestResult = 5496,
		ClientServerUnavailable = 5500,
		ClientServersAvailable = 5501,
		ClientRegisterAuthTicketWithCM = 5502,
		ClientGCMsgFailed = 5503,
		ClientMicroTxnAuthRequest = 5504,
		ClientMicroTxnAuthorize = 5505,
		ClientMicroTxnAuthorizeResponse = 5506,
		ClientAppMinutesPlayedData = 5507,
		ClientGetMicroTxnInfo = 5508,
		ClientGetMicroTxnInfoResponse = 5509,
		ClientMarketingMessageUpdate2 = 5510,
		ClientDeregisterWithServer = 5511,
		ClientSubscribeToPersonaFeed = 5512,
		ClientLogon = 5514,
		ClientGetClientDetails = 5515,
		ClientGetClientDetailsResponse = 5516,
		ClientReportOverlayDetourFailure = 5517,
		ClientGetClientAppList = 5518,
		ClientGetClientAppListResponse = 5519,
		ClientInstallClientApp = 5520,
		ClientInstallClientAppResponse = 5521,
		ClientUninstallClientApp = 5522,
		ClientUninstallClientAppResponse = 5523,
		ClientSetClientAppUpdateState = 5524,
		ClientSetClientAppUpdateStateResponse = 5525,
		ClientRequestEncryptedAppTicket = 5526,
		ClientRequestEncryptedAppTicketResponse = 5527,
		ClientWalletInfoUpdate = 5528,
		ClientLBSSetUGC = 5529,
		ClientLBSSetUGCResponse = 5530,
		ClientAMGetClanOfficers = 5531,
		ClientAMGetClanOfficersResponse = 5532,
		ClientDFSAuthenticateRequest = 5605,
		ClientDFSAuthenticateResponse = 5606,
		ClientDFSEndSession = 5607,
		ClientDFSDownloadStatus = 5617,
		ClientMDSLoginRequest = 5801,
		ClientMDSLoginResponse = 5802,
		ClientMDSUploadManifestRequest = 5803,
		ClientMDSUploadManifestResponse = 5804,
		ClientMDSTransmitManifestDataChunk = 5805,
		ClientMDSHeartbeat = 5806,
		ClientMDSUploadDepotChunks = 5807,
		ClientMDSUploadDepotChunksResponse = 5808,
		ClientMDSInitDepotBuildRequest = 5809,
		ClientMDSInitDepotBuildResponse = 5810,
		ClientMDSGetDepotManifest = 5818,
		ClientMDSGetDepotManifestResponse = 5819,
		ClientMDSGetDepotManifestChunk = 5820,
		ClientMDSDownloadDepotChunksRequest = 5823,
		ClientMDSDownloadDepotChunksAsync = 5824,
		ClientMDSDownloadDepotChunksAck = 5825,
		ClientMMSCreateLobby = 6601,
		ClientMMSCreateLobbyResponse = 6602,
		ClientMMSJoinLobby = 6603,
		ClientMMSJoinLobbyResponse = 6604,
		ClientMMSLeaveLobby = 6605,
		ClientMMSLeaveLobbyResponse = 6606,
		ClientMMSGetLobbyList = 6607,
		ClientMMSGetLobbyListResponse = 6608,
		ClientMMSSetLobbyData = 6609,
		ClientMMSSetLobbyDataResponse = 6610,
		ClientMMSGetLobbyData = 6611,
		ClientMMSLobbyData = 6612,
		ClientMMSSendLobbyChatMsg = 6613,
		ClientMMSLobbyChatMsg = 6614,
		ClientMMSSetLobbyOwner = 6615,
		ClientMMSSetLobbyOwnerResponse = 6616,
		ClientMMSSetLobbyGameServer = 6617,
		ClientMMSLobbyGameServerSet = 6618,
		ClientMMSUserJoinedLobby = 6619,
		ClientMMSUserLeftLobby = 6620,
		ClientMMSInviteToLobby = 6621,
		ClientUDSP2PSessionStarted = 7001,
		ClientUDSP2PSessionEnded = 7002,
		Max = 7003,
	}
	public enum EResult
	{
		Invalid = 0,
		OK = 1,
		Fail = 2,
		NoConnection = 3,
		InvalidPassword = 5,
		LoggedInElsewhere = 6,
		InvalidProtocolVer = 7,
		InvalidParam = 8,
		FileNotFound = 9,
		Busy = 10,
		InvalidState = 11,
		InvalidName = 12,
		InvalidEmail = 13,
		DuplicateName = 14,
		AccessDenied = 15,
		Timeout = 16,
		Banned = 17,
		AccountNotFound = 18,
		InvalidSteamID = 19,
		ServiceUnavailable = 20,
		NotLoggedOn = 21,
		Pending = 22,
		EncryptionFailure = 23,
		InsufficientPrivilege = 24,
		LimitExceeded = 25,
		Revoked = 26,
		Expired = 27,
		AlreadyRedeemed = 28,
		DuplicateRequest = 29,
		AlreadyOwned = 30,
		IPNotFound = 31,
		PersistFailed = 32,
		LockingFailed = 33,
		LogonSessionReplaced = 34,
		ConnectFailed = 35,
		HandshakeFailed = 36,
		IOFailure = 37,
		RemoteDisconnect = 38,
		ShoppingCartNotFound = 39,
		Blocked = 40,
		Ignored = 41,
		NoMatch = 42,
		AccountDisabled = 43,
		ServiceReadOnly = 44,
		AccountNotFeatured = 45,
		AdministratorOK = 46,
		ContentVersion = 47,
		TryAnotherCM = 48,
		PasswordRequiredToKickSession = 49,
		AlreadyLoggedInElsewhere = 50,
		Suspended = 51,
		Cancelled = 52,
		DataCorruption = 53,
		DiskFull = 54,
		RemoteCallFailed = 55,
		Max = 56,
	}
	public enum EUniverse
	{
		Invalid = 0,
		Public = 1,
		Beta = 2,
		Internal = 3,
		Dev = 4,
		RC = 5,
		Max = 6,
	}
	public enum EChatEntryType
	{
		Invalid = 0,
		ChatMsg = 1,
		Typing = 2,
		InviteGame = 3,
		Emote = 4,
		LobbyGameStart = 5,
		LeftConversation = 6,
		Max = 7,
	}
	public enum EPersonaState
	{
		Offline = 0,
		Online = 1,
		Busy = 2,
		Away = 3,
		Snooze = 4,
		Max = 5,
	}
	public enum EAccountType
	{
		Invalid = 0,
		Individual = 1,
		Multiseat = 2,
		GameServer = 3,
		AnonGameServer = 4,
		Pending = 5,
		ContentServer = 6,
		Clan = 7,
		Chat = 8,
		P2PSuperSeeder = 9,
		AnonUser = 10,
		Max = 11,
	}
	public enum EFriendRelationship
	{
		None = 0,
		Blocked = 1,
		RequestRecipient = 2,
		Friend = 3,
		RequestInitiator = 4,
		Ignored = 5,
		IgnoredFriend = 6,
		Max = 7,
	}
	[Flags]
	public enum EAccountFlags
	{
		NormalUser = 0,
		PersonaNameSet = 1,
		Unbannable = 2,
		PasswordSet = 4,
		Support = 8,
		Admin = 16,
		Supervisor = 32,
		AppEditor = 64,
		HWIDSet = 128,
		PersonalQASet = 256,
		VacBeta = 512,
		Debug = 1024,
		Disabled = 2048,
		LimitedUser = 4096,
		LimitedUserForce = 8192,
		EmailValidated = 16384,
		MarketingTreatment = 32768,
		Max = 32769,
	}
	[Flags]
	public enum EFriendFlags
	{
		None = 0,
		Blocked = 1,
		FriendshipRequested = 2,
		Immediate = 4,
		ClanMember = 8,
		GameServer = 16,
		RequestingFriendship = 128,
		RequestingInfo = 256,
		Ignored = 512,
		IgnoredFriend = 1024,
		FlagAll = 65535,
		Max = 65536,
	}
	[Flags]
	public enum EClientPersonaStateFlag
	{
		Status = 1,
		PlayerName = 2,
		QueryPort = 4,
		SourceID = 8,
		Presence = 16,
		Metadata = 32,
		LastSeen = 64,
		ClanInfo = 128,
		GameExtraInfo = 256,
		GameDataBlob = 512,
		Max = 513,
	}
	public enum EAppUsageEvent
	{
		GameLaunch = 1,
		GameLaunchTrial = 2,
		Media = 3,
		PreloadStart = 4,
		PreloadFinish = 5,
		MarketingMessageView = 6,
		InGameAdViewed = 7,
		GameLaunchFreeWeekend = 8,
		Max = 9,
	}
	public enum EUdpPacketType
	{
		Invalid = 0,
		ChallengeReq = 1,
		Challenge = 2,
		Connect = 3,
		Accept = 4,
		Disconnect = 5,
		Data = 6,
		Datagram = 7,
		Max = 8,
	}
	public class UdpHeader : ISteamSerializable
	{
		public static readonly uint MAGIC = 0x31305356;
		// Static size: 4
		public uint Magic { get; set; }
		// Static size: 2
		public ushort PayloadSize { get; set; }
		// Static size: 1
		public EUdpPacketType PacketType { get; set; }
		// Static size: 1
		public byte Flags { get; set; }
		// Static size: 4
		public uint SourceConnID { get; set; }
		// Static size: 4
		public uint DestConnID { get; set; }
		// Static size: 4
		public uint SeqThis { get; set; }
		// Static size: 4
		public uint SeqAck { get; set; }
		// Static size: 4
		public uint PacketsInMsg { get; set; }
		// Static size: 4
		public uint MsgStartSeq { get; set; }
		// Static size: 4
		public uint MsgSize { get; set; }

		public UdpHeader()
		{
			Magic = UdpHeader.MAGIC;
			PayloadSize = 0;
			PacketType = EUdpPacketType.Invalid;
			Flags = 0;
			SourceConnID = 512;
			DestConnID = 0;
			SeqThis = 0;
			SeqAck = 0;
			PacketsInMsg = 0;
			MsgStartSeq = 0;
			MsgSize = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 36 );

			bb.Append( Magic );
			bb.Append( PayloadSize );
			bb.Append( (byte)PacketType );
			bb.Append( Flags );
			bb.Append( SourceConnID );
			bb.Append( DestConnID );
			bb.Append( SeqThis );
			bb.Append( SeqAck );
			bb.Append( PacketsInMsg );
			bb.Append( MsgStartSeq );
			bb.Append( MsgSize );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			Magic = br.ReadUInt32();
			PayloadSize = br.ReadUInt16();
			PacketType = (EUdpPacketType)br.ReadByte();
			Flags = br.ReadByte();
			SourceConnID = br.ReadUInt32();
			DestConnID = br.ReadUInt32();
			SeqThis = br.ReadUInt32();
			SeqAck = br.ReadUInt32();
			PacketsInMsg = br.ReadUInt32();
			MsgStartSeq = br.ReadUInt32();
			MsgSize = br.ReadUInt32();
		}
	}

	public class ChallengeData : ISteamSerializable
	{
		public static readonly uint CHALLENGE_MASK = 0xA426DF2B;
		// Static size: 4
		public uint ChallengeValue { get; set; }
		// Static size: 4
		public uint ServerLoad { get; set; }

		public ChallengeData()
		{
			ChallengeValue = 0;
			ServerLoad = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 8 );

			bb.Append( ChallengeValue );
			bb.Append( ServerLoad );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			ChallengeValue = br.ReadUInt32();
			ServerLoad = br.ReadUInt32();
		}
	}

	public class ConnectData : ISteamSerializable
	{
		public static readonly uint CHALLENGE_MASK = ChallengeData.CHALLENGE_MASK;
		// Static size: 4
		public uint ChallengeValue { get; set; }

		public ConnectData()
		{
			ChallengeValue = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 4 );

			bb.Append( ChallengeValue );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			ChallengeValue = br.ReadUInt32();
		}
	}

	public class Accept : ISteamSerializable
	{

		public Accept()
		{
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 0 );


			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
		}
	}

	public class Datagram : ISteamSerializable
	{

		public Datagram()
		{
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 0 );


			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
		}
	}

	public class Disconnect : ISteamSerializable
	{

		public Disconnect()
		{
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 0 );


			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	public class MsgHdr : ISteamSerializableHeader
	{
		public void SetEMsg( EMsg msg ) { this.Msg = msg; }

		// Static size: 4
		public EMsg Msg { get; set; }
		// Static size: 8
		public long TargetJobID { get; set; }
		// Static size: 8
		public long SourceJobID { get; set; }

		public MsgHdr()
		{
			Msg = EMsg.Invalid;
			TargetJobID = -1;
			SourceJobID = -1;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 20 );

			bb.Append( (int)Msg );
			bb.Append( TargetJobID );
			bb.Append( SourceJobID );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			Msg = (EMsg)br.ReadInt32();
			TargetJobID = br.ReadInt64();
			SourceJobID = br.ReadInt64();
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	public class ExtendedClientMsgHdr : ISteamSerializableHeader
	{
		public void SetEMsg( EMsg msg ) { this.Msg = msg; }

		// Static size: 4
		public EMsg Msg { get; set; }
		// Static size: 1
		public byte HeaderSize { get; set; }
		// Static size: 2
		public ushort HeaderVersion { get; set; }
		// Static size: 8
		public long TargetJobID { get; set; }
		// Static size: 8
		public long SourceJobID { get; set; }
		// Static size: 1
		public byte HeaderCanary { get; set; }
		// Static size: 8
		private ulong steamID;
		public SteamID SteamID { get { return new SteamID( steamID ); } set { steamID = value.ConvertToUint64(); } }
		// Static size: 4
		public int SessionID { get; set; }

		public ExtendedClientMsgHdr()
		{
			Msg = EMsg.Invalid;
			HeaderSize = 36;
			HeaderVersion = 2;
			TargetJobID = -1;
			SourceJobID = -1;
			HeaderCanary = 239;
			steamID = 0;
			SessionID = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 36 );

			bb.Append( (int)Msg );
			bb.Append( HeaderSize );
			bb.Append( HeaderVersion );
			bb.Append( TargetJobID );
			bb.Append( SourceJobID );
			bb.Append( HeaderCanary );
			bb.Append( steamID );
			bb.Append( SessionID );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			Msg = (EMsg)br.ReadInt32();
			HeaderSize = br.ReadByte();
			HeaderVersion = br.ReadUInt16();
			TargetJobID = br.ReadInt64();
			SourceJobID = br.ReadInt64();
			HeaderCanary = br.ReadByte();
			steamID = br.ReadUInt64();
			SessionID = br.ReadInt32();
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	public class MsgHdrProtoBuf : ISteamSerializableHeader
	{
		public void SetEMsg( EMsg msg ) { this.Msg = msg; }

		// Static size: 4
		public EMsg Msg { get; set; }
		// Static size: 4
		public int HeaderLength { get; set; }
		// Static size: 0
		public CMsgProtoBufHeader ProtoHeader { get; set; }

		public MsgHdrProtoBuf()
		{
			Msg = EMsg.Invalid;
			HeaderLength = 0;
			ProtoHeader = new CMsgProtoBufHeader();
		}

		public byte[] Serialize()
		{
			MemoryStream msProtoHeader = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgProtoBufHeader>(msProtoHeader, ProtoHeader);
			HeaderLength = (int)msProtoHeader.Length;
			ByteBuffer bb = new ByteBuffer( 8 + (int)msProtoHeader.Length );

			bb.Append( (int)MsgUtil.MakeMsg( Msg, true ) );
			bb.Append( HeaderLength );
			byte[] buff = msProtoHeader.ToArray();
			bb.Append( buff );

			msProtoHeader.Close();
			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			Msg = (EMsg)MsgUtil.GetMsg( (uint)br.ReadInt32() );
			HeaderLength = br.ReadInt32();
			using( MemoryStream msProtoHeader = new MemoryStream( br.ReadBytes( HeaderLength ) ) )
				ProtoHeader = ProtoBuf.Serializer.Deserialize<CMsgProtoBufHeader>( msProtoHeader );
		}
	}

	public class MsgChannelEncryptRequest : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ChannelEncryptRequest; }

		public static readonly uint PROTOCOL_VERSION = 1;
		// Static size: 4
		public uint ProtocolVersion { get; set; }
		// Static size: 4
		public EUniverse Universe { get; set; }

		public MsgChannelEncryptRequest()
		{
			ProtocolVersion = MsgChannelEncryptRequest.PROTOCOL_VERSION;
			Universe = EUniverse.Invalid;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 8 );

			bb.Append( ProtocolVersion );
			bb.Append( (int)Universe );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			ProtocolVersion = br.ReadUInt32();
			Universe = (EUniverse)br.ReadInt32();
		}
	}

	public class MsgChannelEncryptResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ChannelEncryptResponse; }

		// Static size: 4
		public uint ProtocolVersion { get; set; }
		// Static size: 4
		public uint KeySize { get; set; }

		public MsgChannelEncryptResponse()
		{
			ProtocolVersion = MsgChannelEncryptRequest.PROTOCOL_VERSION;
			KeySize = 128;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 8 );

			bb.Append( ProtocolVersion );
			bb.Append( KeySize );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			ProtocolVersion = br.ReadUInt32();
			KeySize = br.ReadUInt32();
		}
	}

	public class MsgChannelEncryptResult : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ChannelEncryptResult; }

		// Static size: 4
		public EResult Result { get; set; }

		public MsgChannelEncryptResult()
		{
			Result = EResult.Invalid;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 4 );

			bb.Append( (int)Result );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			Result = (EResult)br.ReadInt32();
		}
	}

	public class MsgMulti : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.Multi; }

		// Static size: 0
		public CMsgMulti Proto { get; set; }

		public MsgMulti()
		{
			Proto = new CMsgMulti();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgMulti>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgMulti>( ms );
		}
	}

	public class MsgClientNewLoginKey : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientNewLoginKey; }

		// Static size: 4
		public uint UniqueID { get; set; }
		// Static size: 20
		public byte[] LoginKey { get; set; }

		public MsgClientNewLoginKey()
		{
			UniqueID = 0;
			LoginKey = new byte[20];
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 24 );

			bb.Append( UniqueID );
			bb.Append( LoginKey );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			UniqueID = br.ReadUInt32();
			LoginKey = br.ReadBytes( 20 );
		}
	}

	public class MsgClientNewLoginKeyAccepted : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientNewLoginKeyAccepted; }

		// Static size: 4
		public uint UniqueID { get; set; }

		public MsgClientNewLoginKeyAccepted()
		{
			UniqueID = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 4 );

			bb.Append( UniqueID );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			UniqueID = br.ReadUInt32();
		}
	}

	public class MsgClientHeartBeat : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientHeartBeat; }

		// Static size: 0
		public CMsgClientHeartBeat Proto { get; set; }

		public MsgClientHeartBeat()
		{
			Proto = new CMsgClientHeartBeat();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientHeartBeat>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientHeartBeat>( ms );
		}
	}

	public class MsgClientLogon : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogon; }

		public static readonly uint ObfuscationMask = 0xBAADF00D;
		public static readonly uint CurrentProtocol = 65565;
		// Static size: 0
		public CMsgClientLogon Proto { get; set; }

		public MsgClientLogon()
		{
			Proto = new CMsgClientLogon();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientLogon>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogon>( ms );
		}
	}

	public class MsgClientLogOff : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogOff; }

		// Static size: 0
		public CMsgClientLogOff Proto { get; set; }

		public MsgClientLogOff()
		{
			Proto = new CMsgClientLogOff();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientLogOff>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogOff>( ms );
		}
	}

	public class MsgClientLogonResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogOnResponse; }

		// Static size: 0
		public CMsgClientLogonResponse Proto { get; set; }

		public MsgClientLogonResponse()
		{
			Proto = new CMsgClientLogonResponse();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientLogonResponse>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogonResponse>( ms );
		}
	}

	public class MsgGSServerType : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSServerType; }

		// Static size: 0
		public CMsgGSServerType Proto { get; set; }

		public MsgGSServerType()
		{
			Proto = new CMsgGSServerType();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgGSServerType>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgGSServerType>( ms );
		}
	}

	public class MsgGSStatusReply : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSStatusReply; }

		// Static size: 0
		public CMsgGSStatusReply Proto { get; set; }

		public MsgGSStatusReply()
		{
			Proto = new CMsgGSStatusReply();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgGSStatusReply>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgGSStatusReply>( ms );
		}
	}

	public class MsgClientRegisterAuthTicketWithCM : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRegisterAuthTicketWithCM; }

		// Static size: 0
		public CMsgClientRegisterAuthTicketWithCM Proto { get; set; }

		public MsgClientRegisterAuthTicketWithCM()
		{
			Proto = new CMsgClientRegisterAuthTicketWithCM();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientRegisterAuthTicketWithCM>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRegisterAuthTicketWithCM>( ms );
		}
	}

	public class MsgClientGetAppOwnershipTicket : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetAppOwnershipTicket; }

		// Static size: 0
		public CMsgClientGetAppOwnershipTicket Proto { get; set; }

		public MsgClientGetAppOwnershipTicket()
		{
			Proto = new CMsgClientGetAppOwnershipTicket();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientGetAppOwnershipTicket>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetAppOwnershipTicket>( ms );
		}
	}

	public class MsgClientGetAppOwnershipTicketResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetAppOwnershipTicketResponse; }

		// Static size: 0
		public CMsgClientGetAppOwnershipTicketResponse Proto { get; set; }

		public MsgClientGetAppOwnershipTicketResponse()
		{
			Proto = new CMsgClientGetAppOwnershipTicketResponse();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientGetAppOwnershipTicketResponse>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetAppOwnershipTicketResponse>( ms );
		}
	}

	public class MsgClientAuthList : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAuthList; }

		// Static size: 0
		public CMsgClientAuthList Proto { get; set; }

		public MsgClientAuthList()
		{
			Proto = new CMsgClientAuthList();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientAuthList>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAuthList>( ms );
		}
	}

	public class MsgClientRequestFriendData : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRequestFriendData; }

		// Static size: 0
		public CMsgClientRequestFriendData Proto { get; set; }

		public MsgClientRequestFriendData()
		{
			Proto = new CMsgClientRequestFriendData();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientRequestFriendData>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRequestFriendData>( ms );
		}
	}

	public class MsgClientChangeStatus : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChangeStatus; }

		// Static size: 1
		public byte PersonaState { get; set; }

		public MsgClientChangeStatus()
		{
			PersonaState = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 1 );

			bb.Append( PersonaState );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			PersonaState = br.ReadByte();
		}
	}

	public class MsgClientPersonaState : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientPersonaState; }

		// Static size: 0
		public CMsgClientPersonaState Proto { get; set; }

		public MsgClientPersonaState()
		{
			Proto = new CMsgClientPersonaState();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientPersonaState>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientPersonaState>( ms );
		}
	}

	public class MsgClientSessionToken : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientSessionToken; }

		// Static size: 0
		public CMsgClientSessionToken Proto { get; set; }

		public MsgClientSessionToken()
		{
			Proto = new CMsgClientSessionToken();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientSessionToken>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientSessionToken>( ms );
		}
	}

	public class MsgClientGameConnectTokens : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGameConnectTokens; }

		// Static size: 0
		public CMsgClientGameConnectTokens Proto { get; set; }

		public MsgClientGameConnectTokens()
		{
			Proto = new CMsgClientGameConnectTokens();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientGameConnectTokens>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGameConnectTokens>( ms );
		}
	}

	public class MsgClientGamesPlayed : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGamesPlayedWithDataBlob; }

		// Static size: 0
		public CMsgClientGamesPlayed Proto { get; set; }

		public MsgClientGamesPlayed()
		{
			Proto = new CMsgClientGamesPlayed();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientGamesPlayed>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGamesPlayed>( ms );
		}
	}

	public class MsgClientFriendsList : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendsList; }

		// Static size: 0
		public CMsgClientFriendsList Proto { get; set; }

		public MsgClientFriendsList()
		{
			Proto = new CMsgClientFriendsList();
		}

		public byte[] Serialize()
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<CMsgClientFriendsList>(msProto, Proto);
			return msProto.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientFriendsList>( ms );
		}
	}

	public class MsgClientFriendMsg : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendMsg; }

		// Static size: 8
		private ulong steamID;
		public SteamID SteamID { get { return new SteamID( steamID ); } set { steamID = value.ConvertToUint64(); } }
		// Static size: 4
		public EChatEntryType EntryType { get; set; }

		public MsgClientFriendMsg()
		{
			steamID = 0;
			EntryType = EChatEntryType.Invalid;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 12 );

			bb.Append( steamID );
			bb.Append( (int)EntryType );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			steamID = br.ReadUInt64();
			EntryType = (EChatEntryType)br.ReadInt32();
		}
	}

	public class MsgClientFriendMsgIncoming : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendMsgIncoming; }

		// Static size: 8
		private ulong steamID;
		public SteamID SteamID { get { return new SteamID( steamID ); } set { steamID = value.ConvertToUint64(); } }
		// Static size: 4
		public EChatEntryType EntryType { get; set; }
		// Static size: 4
		public int FromLimitedAccount { get; set; }
		// Static size: 4
		public int MessageSize { get; set; }

		public MsgClientFriendMsgIncoming()
		{
			steamID = 0;
			EntryType = EChatEntryType.Invalid;
			FromLimitedAccount = 0;
			MessageSize = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 20 );

			bb.Append( steamID );
			bb.Append( (int)EntryType );
			bb.Append( FromLimitedAccount );
			bb.Append( MessageSize );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			steamID = br.ReadUInt64();
			EntryType = (EChatEntryType)br.ReadInt32();
			FromLimitedAccount = br.ReadInt32();
			MessageSize = br.ReadInt32();
		}
	}

	public class MsgClientVACBanStatus : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientVACBanStatus; }

		// Static size: 4
		public uint NumBans { get; set; }

		public MsgClientVACBanStatus()
		{
			NumBans = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 4 );

			bb.Append( NumBans );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			NumBans = br.ReadUInt32();
		}
	}

	public class MsgClientAppUsageEvent : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppUsageEvent; }

		// Static size: 4
		public EAppUsageEvent AppUsageEvent { get; set; }
		// Static size: 8
		public ulong GameID { get; set; }
		// Static size: 1
		public byte Offline { get; set; }

		public MsgClientAppUsageEvent()
		{
			AppUsageEvent = 0;
			GameID = 0;
			Offline = 0;
		}

		public byte[] Serialize()
		{
			ByteBuffer bb = new ByteBuffer( 13 );

			bb.Append( (int)AppUsageEvent );
			bb.Append( GameID );
			bb.Append( Offline );

			return bb.ToArray();
		}

		public void Deserialize( MemoryStream ms )
		{
			BinaryReader br = new BinaryReader( ms );

			AppUsageEvent = (EAppUsageEvent)br.ReadInt32();
			GameID = br.ReadUInt64();
			Offline = br.ReadByte();
		}
	}

}
