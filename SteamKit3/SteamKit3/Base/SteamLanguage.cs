#pragma warning disable 1591
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamKit3
{
	public interface ISteamSerializable
	{
		void Serialize(Stream stream);
		void Deserialize( Stream stream );
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
		ChannelEncryptResult = 1305,
		RemoteSysID = 128,
		ClientChatAction = 597,
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
		ClientAddFriend = 791,
		ClientAddFriendResponse = 792,
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
		ClientPasswordChangeResponse = 805,
		ClientChatEnter = 807,
		ClientFriendRemovedFromSource = 808,
		ClientCreateChat = 809,
		ClientCreateChatResponse = 810,
		ClientP2PIntroducerMessage = 813,
		ClientChatActionResult = 814,
		ClientRequestFriendData = 815,
		ClientGetUserStats = 818,
		ClientGetUserStatsResponse = 819,
		ClientStoreUserStats = 820,
		ClientStoreUserStatsResponse = 821,
		ClientClanState = 822,
		ClientServiceModule = 830,
		ClientServiceCall = 831,
		ClientServiceCallResponse = 832,
		ClientPackageInfoRequest = 833,
		ClientPackageInfoResponse = 834,
		ClientNatTraversalStatEvent = 839,
		ClientAppInfoRequest = 840,
		ClientAppInfoResponse = 841,
		ClientSteamUsageEvent = 842,
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
		ClientVTTCert = 863,
		ClientAppInfoUpdate = 866,
		ClientAppInfoChanges = 867,
		ClientServerList = 880,
		ClientEmailChangeResponse = 891,
		ClientSecretQAChangeResponse = 892,
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
		GSAssociateWithClan = 938,
		GSAssociateWithClanResponse = 939,
		GSComputeNewPlayerCompatibility = 940,
		GSComputeNewPlayerCompatibilityResponse = 941,
		FileXferRequest = 1200,
		FileXferResponse = 1201,
		FileXferData = 1202,
		FileXferEnd = 1203,
		FileXferDataAck = 1204,
		ChannelEncryptRequest = 1303,
		ChannelEncryptResponse = 1304,
		ClientChatRoomInfo = 4026,
		AMGameServerUpdate = 4331,
		AMGameServerRemove = 4332,
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
		ClientCheckFileSignature = 5533,
		ClientCheckFileSignatureResponse = 5534,
		ClientFriendProfileInfo = 5535,
		ClientFriendProfileInfoResponse = 5536,
		ClientUpdateMachineAuth = 5537,
		ClientUpdateMachineAuthResponse = 5538,
		ClientReadMachineAuth = 5539,
		ClientReadMachineAuthResponse = 5540,
		ClientRequestMachineAuth = 5541,
		ClientRequestMachineAuthResponse = 5542,
		ClientScreenshotsChanged = 5543,
		ClientEmailChange4 = 5544,
		ClientEmailChangeResponse4 = 5545,
		ClientGetCDNAuthToken = 5546,
		ClientGetCDNAuthTokenResponse = 5547,
		ClientDownloadRateStatistics = 5548,
		ClientRequestAccountData = 5549,
		ClientRequestAccountDataResponse = 5550,
		ClientResetForgottenPassword4 = 5551,
		ClientHideFriend = 5552,
		ClientFriendsGroupsList = 5553,
		ClientGetClanActivityCounts = 5554,
		ClientGetClanActivityCountsResponse = 5555,
		ClientOGSReportString = 5556,
		ClientOGSReportBug = 5557,
		ClientSentLogs = 5558,
		ClientLogonGameServer = 5559,
		AMClientCreateFriendsGroup = 5560,
		AMClientCreateFriendsGroupResponse = 5561,
		AMClientDeleteFriendsGroup = 5562,
		AMClientDeleteFriendsGroupResponse = 5563,
		AMClientRenameFriendsGroup = 5564,
		AMClientRenameFriendsGroupResponse = 5565,
		AMClientAddFriendToGroup = 5566,
		AMClientAddFriendToGroupResponse = 5567,
		AMClientRemoveFriendFromGroup = 5568,
		AMClientRemoveFriendFromGroupResponse = 5569,
		ClientAMGetPersonaNameHistory = 5570,
		ClientAMGetPersonaNameHistoryResponse = 5571,
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
		ClientMDSRegisterAppBuild = 5838,
		ClientMDSRegisterAppBuildResponse = 5839,
		ClientMDSSetAppBuildLive = 5840,
		ClientMDSSetAppBuildLiveResponse = 5841,
		ClientGMSServerQuery = 6403,
		GMSClientServerQueryResponse = 6404,
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
		ClientMMSSetLobbyLinked = 6624,
		NonStdMsgMemcached = 6801,
		NonStdMsgHTTPServer = 6802,
		NonStdMsgHTTPClient = 6803,
		NonStdMsgWGResponse = 6804,
		NonStdMsgPHPSimulator = 6805,
		NonStdMsgChase = 6806,
		NonStdMsgDFSTransfer = 6807,
		NonStdMsgTests = 6808,
		ClientUDSP2PSessionStarted = 7001,
		ClientUDSP2PSessionEnded = 7002,
		ClientUDSInviteToGame = 7005,
		ClientUCMAddScreenshot = 7301,
		ClientUCMAddScreenshotResponse = 7302,
		ClientUCMDeleteScreenshot = 7309,
		ClientUCMDeleteScreenshotResponse = 7310,
		ClientUCMPublishFile = 7311,
		ClientUCMPublishFileResponse = 7312,
		ClientUCMGetPublishedFileDetails = 7313,
		ClientUCMGetPublishedFileDetailsResponse = 7314,
		ClientUCMDeletePublishedFile = 7315,
		ClientUCMDeletePublishedFileResponse = 7316,
		ClientUCMEnumerateUserPublishedFiles = 7317,
		ClientUCMEnumerateUserPublishedFilesResponse = 7318,
		ClientUCMSubscribePublishedFile = 7319,
		ClientUCMSubscribePublishedFileResponse = 7320,
		ClientUCMEnumerateUserSubscribedFiles = 7321,
		ClientUCMEnumerateUserSubscribedFilesResponse = 7322,
		ClientUCMUnsubscribePublishedFile = 7323,
		ClientUCMUnsubscribePublishedFileResponse = 7324,
		ClientUCMUpdatePublishedFile = 7325,
		ClientUCMUpdatePublishedFileResponse = 7326,
		ClientRichPresenceUpload = 7501,
		ClientRichPresenceRequest = 7502,
		ClientRichPresenceInfo = 7503,
		EconTrading_InitiateTradeRequest = 7701,
		EconTrading_InitiateTradeProposed = 7702,
		EconTrading_InitiateTradeResponse = 7703,
		EconTrading_InitiateTradeResult = 7704,
		EconTrading_StartSession = 7705,
		EconTrading_CancelTradeRequest = 7706,
		ClientUGSGetGlobalStats = 7901,
		ClientUGSGetGlobalStatsResponse = 7902,
		Max = 7903,
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
		PasswordNotSet = 56,
		PSNAccountNotLinked = 57,
		InvalidPSNTicket = 58,
		PSNAccountAlreadyLinked = 59,
		RemoteFileConflict = 60,
		IllegalPassword = 61,
		SameAsPreviousValue = 62,
		AccountLogonDenied = 63,
		CannotUseOldPassword = 64,
		InvalidLoginAuthCode = 65,
		AccountLogonDeniedNoMailSent = 66,
		HardwareNotCapableOfIPT = 67,
		IPTInitError = 68,
		Max = 69,
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
		ConsoleUser = 9,
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
	[Flags]
	public enum ELicenseFlags
	{
		Renew = 0x01,
		RenewalFailed = 0x02,
		Pending = 0x04,
		Expired = 0x08,
		CancelledByUser = 0x10,
		CancelledByAdmin = 0x20,
		Max = 33,
	}
	public enum ELicenseType
	{
		NoLicense = 0,
		SinglePurchase = 1,
		SinglePurchaseLimitedUse = 2,
		RecurringCharge = 3,
		RecurringChargeLimitedUse = 4,
		RecurringChargeLimitedUseWithOverages = 5,
		Max = 6,
	}
	public enum EPaymentMethod
	{
		None = 0,
		ActivationCode = 1,
		CreditCard = 2,
		PayPal = 4,
		GuestPass = 8,
		HardwarePromo = 16,
		ClickAndBuy = 32,
		AutoGrant = 64,
		Wallet = 128,
		OEMTicket = 256,
		Split = 512,
		Max = 513,
	}
	public enum EIntroducerRouting
	{
		FileShare = 0,
		P2PVoiceChat = 1,
		P2PNetworking = 2,
		Max = 3,
	}
	[Flags]
	public enum EServerFlags
	{
		None = 0,
		Active = 1,
		Secure = 2,
		Dedicated = 4,
		Linux = 8,
		Pssworded = 16,
		Private = 32,
		Max = 33,
	}
	public enum EDenyReason
	{
		InvalidVersion = 1,
		Generic = 2,
		NotLoggedOn = 3,
		NoLicense = 4,
		Cheater = 5,
		LoggedInElseWhere = 6,
		UnknownText = 7,
		IncompatibleAnticheat = 8,
		MemoryCorruption = 9,
		IncompatibleSoftware = 10,
		SteamConnectionLost = 11,
		SteamConnectionError = 12,
		SteamResponseTimedOut = 13,
		SteamValidationStalled = 14,
		SteamOwnerLeftGuestUser = 15,
		Max = 16,
	}
	public enum EClanRank
	{
		None = 0,
		Owner = 1,
		Officer = 2,
		Member = 3,
		Max = 4,
	}
	public enum EClanRelationship
	{
		None = 0,
		Blocked = 1,
		Invited = 2,
		Member = 3,
		Kicked = 4,
		Max = 5,
	}
	public enum EAuthSessionResponse
	{
		OK = 0,
		UserNotConnectedToSteam = 1,
		NoLicenseOrExpired = 2,
		VACBanned = 3,
		LoggedInElseWhere = 4,
		VACCheckTimedOut = 5,
		AuthTicketCanceled = 6,
		AuthTicketInvalidAlreadyUsed = 7,
		AuthTicketInvalid = 8,
		Max = 9,
	}
	public enum EChatRoomEnterResponse
	{
		Success = 1,
		DoesntExist = 2,
		NotAllowed = 3,
		Full = 4,
		Error = 5,
		Banned = 6,
		Limited = 7,
		ClanDisabled = 8,
		CommunityBan = 9,
		Max = 10,
	}
	public enum EChatRoomType
	{
		Friend = 1,
		MUC = 2,
		Lobby = 3,
		Max = 4,
	}
	public enum EChatInfoType
	{
		StateChange = 1,
		InfoUpdate = 2,
		MemberLimitChange = 3,
		Max = 4,
	}
	public enum EChatAction
	{
		InviteChat = 1,
		Kick = 2,
		Ban = 3,
		UnBan = 4,
		StartVoiceSpeak = 5,
		EndVoiceSpeak = 6,
		LockChat = 7,
		UnlockChat = 8,
		CloseChat = 9,
		SetJoinable = 10,
		SetUnjoinable = 11,
		SetOwner = 12,
		SetInvisibleToFriends = 13,
		SetVisibleToFriends = 14,
		SetModerated = 15,
		SetUnmoderated = 16,
		Max = 17,
	}
	public enum EChatActionResult
	{
		Success = 1,
		Error = 2,
		NotPermitted = 3,
		NotAllowedOnClanMember = 4,
		NotAllowedOnBannedUser = 5,
		NotAllowedOnChatOwner = 6,
		NotAllowedOnSelf = 7,
		ChatDoesntExist = 8,
		ChatFull = 9,
		VoiceSlotsFull = 10,
		Max = 11,
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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( Magic );
			bw.Write( PayloadSize );
			bw.Write( (byte)PacketType );
			bw.Write( Flags );
			bw.Write( SourceConnID );
			bw.Write( DestConnID );
			bw.Write( SeqThis );
			bw.Write( SeqAck );
			bw.Write( PacketsInMsg );
			bw.Write( MsgStartSeq );
			bw.Write( MsgSize );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( ChallengeValue );
			bw.Write( ServerLoad );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( ChallengeValue );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			ChallengeValue = br.ReadUInt32();
		}
	}

	public class Accept : ISteamSerializable
	{

		public Accept()
		{
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );


		}

		public void Deserialize( Stream stream )
		{
		}
	}

	public class Datagram : ISteamSerializable
	{

		public Datagram()
		{
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );


		}

		public void Deserialize( Stream stream )
		{
		}
	}

	public class Disconnect : ISteamSerializable
	{

		public Disconnect()
		{
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );


		}

		public void Deserialize( Stream stream )
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
		public ulong TargetJobID { get; set; }
		// Static size: 8
		public ulong SourceJobID { get; set; }

		public MsgHdr()
		{
			Msg = EMsg.Invalid;
			TargetJobID = ulong.MaxValue;
			SourceJobID = ulong.MaxValue;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Msg );
			bw.Write( TargetJobID );
			bw.Write( SourceJobID );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Msg = (EMsg)br.ReadInt32();
			TargetJobID = br.ReadUInt64();
			SourceJobID = br.ReadUInt64();
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
		public ulong TargetJobID { get; set; }
		// Static size: 8
		public ulong SourceJobID { get; set; }
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
			TargetJobID = ulong.MaxValue;
			SourceJobID = ulong.MaxValue;
			HeaderCanary = 239;
			steamID = 0;
			SessionID = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Msg );
			bw.Write( HeaderSize );
			bw.Write( HeaderVersion );
			bw.Write( TargetJobID );
			bw.Write( SourceJobID );
			bw.Write( HeaderCanary );
			bw.Write( steamID );
			bw.Write( SessionID );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Msg = (EMsg)br.ReadInt32();
			HeaderSize = br.ReadByte();
			HeaderVersion = br.ReadUInt16();
			TargetJobID = br.ReadUInt64();
			SourceJobID = br.ReadUInt64();
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
		public SteamKit3.CMsgProtoBufHeader Proto { get; set; }

		public MsgHdrProtoBuf()
		{
			Msg = EMsg.Invalid;
			HeaderLength = 0;
			Proto = new SteamKit3.CMsgProtoBufHeader();
		}

		public void Serialize(Stream stream)
		{
			MemoryStream msProto = new MemoryStream();
			ProtoBuf.Serializer.Serialize<SteamKit3.CMsgProtoBufHeader>(msProto, Proto);
			HeaderLength = (int)msProto.Length;
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)MsgUtil.MakeMsg( Msg, true ) );
			bw.Write( HeaderLength );
			bw.Write( msProto.ToArray() );

			msProto.Close();
		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Msg = (EMsg)MsgUtil.GetMsg( (uint)br.ReadInt32() );
			HeaderLength = br.ReadInt32();
			using( MemoryStream msProto = new MemoryStream( br.ReadBytes( HeaderLength ) ) )
				Proto = ProtoBuf.Serializer.Deserialize<SteamKit3.CMsgProtoBufHeader>( msProto );
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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( ProtocolVersion );
			bw.Write( (int)Universe );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( ProtocolVersion );
			bw.Write( KeySize );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( UniqueID );
			bw.Write( LoginKey );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( UniqueID );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			UniqueID = br.ReadUInt32();
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

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( NumBans );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			NumBans = br.ReadUInt32();
		}
	}

	public class MsgClientAppUsageEvent : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppUsageEvent; }

		// Static size: 4
		public EAppUsageEvent AppUsageEvent { get; set; }
		// Static size: 8
		private ulong gameID;
		public GameID GameID { get { return new GameID( gameID ); } set { gameID = value.ToUint64(); } }
		// Static size: 2
		public ushort Offline { get; set; }

		public MsgClientAppUsageEvent()
		{
			AppUsageEvent = 0;
			gameID = 0;
			Offline = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)AppUsageEvent );
			bw.Write( gameID );
			bw.Write( Offline );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			AppUsageEvent = (EAppUsageEvent)br.ReadInt32();
			gameID = br.ReadUInt64();
			Offline = br.ReadUInt16();
		}
	}

	public class MsgClientEmailAddrInfo : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientEmailAddrInfo; }

		// Static size: 4
		public uint PasswordStrength { get; set; }
		// Static size: 4
		public uint FlagsAccountSecurityPolicy { get; set; }
		// Static size: 1
		private byte validated;
		public bool Validated { get { return ( validated == 1 ); } set { validated = ( byte )( value ? 1 : 0 ); } }

		public MsgClientEmailAddrInfo()
		{
			PasswordStrength = 0;
			FlagsAccountSecurityPolicy = 0;
			validated = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( PasswordStrength );
			bw.Write( FlagsAccountSecurityPolicy );
			bw.Write( validated );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			PasswordStrength = br.ReadUInt32();
			FlagsAccountSecurityPolicy = br.ReadUInt32();
			validated = br.ReadByte();
		}
	}

	public class MsgClientUpdateGuestPassesList : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientUpdateGuestPassesList; }

		// Static size: 4
		public EResult Result { get; set; }
		// Static size: 4
		public int CountGuestPassesToGive { get; set; }
		// Static size: 4
		public int CountGuestPassesToRedeem { get; set; }

		public MsgClientUpdateGuestPassesList()
		{
			Result = 0;
			CountGuestPassesToGive = 0;
			CountGuestPassesToRedeem = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );
			bw.Write( CountGuestPassesToGive );
			bw.Write( CountGuestPassesToRedeem );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
			CountGuestPassesToGive = br.ReadInt32();
			CountGuestPassesToRedeem = br.ReadInt32();
		}
	}

	public class MsgClientServerList : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientServerList; }

		// Static size: 4
		public int CountServers { get; set; }

		public MsgClientServerList()
		{
			CountServers = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( CountServers );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			CountServers = br.ReadInt32();
		}
	}

	public class MsgClientRequestedClientStats : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRequestedClientStats; }

		// Static size: 4
		public int CountStats { get; set; }

		public MsgClientRequestedClientStats()
		{
			CountStats = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( CountStats );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			CountStats = br.ReadInt32();
		}
	}

	public class MsgClientP2PIntroducerMessage : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientP2PIntroducerMessage; }

		// Static size: 8
		private ulong steamID;
		public SteamID SteamID { get { return new SteamID( steamID ); } set { steamID = value.ConvertToUint64(); } }
		// Static size: 4
		public EIntroducerRouting RoutingType { get; set; }
		// Static size: 1450
		public byte[] Data { get; set; }
		// Static size: 4
		public uint DataLen { get; set; }

		public MsgClientP2PIntroducerMessage()
		{
			steamID = 0;
			RoutingType = 0;
			Data = new byte[1450];
			DataLen = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamID );
			bw.Write( (int)RoutingType );
			bw.Write( Data );
			bw.Write( DataLen );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamID = br.ReadUInt64();
			RoutingType = (EIntroducerRouting)br.ReadInt32();
			Data = br.ReadBytes( 1450 );
			DataLen = br.ReadUInt32();
		}
	}

	public class MsgClientFromGC : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFromGC; }

		// Static size: 4
		public uint AppId { get; set; }
		// Static size: 4
		public uint GCEMsg { get; set; }

		public MsgClientFromGC()
		{
			AppId = 0;
			GCEMsg = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( AppId );
			bw.Write( GCEMsg );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			AppId = br.ReadUInt32();
			GCEMsg = br.ReadUInt32();
		}
	}

	public class MsgClientToGC : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientToGC; }

		// Static size: 4
		public uint AppId { get; set; }
		// Static size: 4
		public uint GCEMsg { get; set; }

		public MsgClientToGC()
		{
			AppId = 0;
			GCEMsg = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( AppId );
			bw.Write( GCEMsg );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			AppId = br.ReadUInt32();
			GCEMsg = br.ReadUInt32();
		}
	}

	public class MsgClientCreateAccountResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientCreateAccountResponse; }

		// Static size: 4
		public EResult Result { get; set; }

		public MsgClientCreateAccountResponse()
		{
			Result = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
		}
	}

	public class MsgClientCreateAccount3 : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientCreateAccount3; }


		public MsgClientCreateAccount3()
		{
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );


		}

		public void Deserialize( Stream stream )
		{
		}
	}

	public class MsgClientOGSBeginSession : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientOGSBeginSession; }

		// Static size: 1
		public byte AccountType { get; set; }
		// Static size: 8
		private ulong accountId;
		public SteamID AccountId { get { return new SteamID( accountId ); } set { accountId = value.ConvertToUint64(); } }
		// Static size: 4
		public uint AppId { get; set; }
		// Static size: 4
		public uint TimeStarted { get; set; }

		public MsgClientOGSBeginSession()
		{
			AccountType = 0;
			accountId = 0;
			AppId = 0;
			TimeStarted = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( AccountType );
			bw.Write( accountId );
			bw.Write( AppId );
			bw.Write( TimeStarted );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			AccountType = br.ReadByte();
			accountId = br.ReadUInt64();
			AppId = br.ReadUInt32();
			TimeStarted = br.ReadUInt32();
		}
	}

	public class MsgClientOGSBeginSessionResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientOGSBeginSessionResponse; }

		// Static size: 4
		public EResult Result { get; set; }
		// Static size: 1
		private byte collectingAny;
		public bool CollectingAny { get { return ( collectingAny == 1 ); } set { collectingAny = ( byte )( value ? 1 : 0 ); } }
		// Static size: 1
		private byte collectingDetails;
		public bool CollectingDetails { get { return ( collectingDetails == 1 ); } set { collectingDetails = ( byte )( value ? 1 : 0 ); } }
		// Static size: 8
		public ulong SessionId { get; set; }

		public MsgClientOGSBeginSessionResponse()
		{
			Result = 0;
			collectingAny = 0;
			collectingDetails = 0;
			SessionId = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );
			bw.Write( collectingAny );
			bw.Write( collectingDetails );
			bw.Write( SessionId );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
			collectingAny = br.ReadByte();
			collectingDetails = br.ReadByte();
			SessionId = br.ReadUInt64();
		}
	}

	public class MsgClientOGSEndSession : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientOGSEndSession; }

		// Static size: 8
		public ulong SessionId { get; set; }
		// Static size: 4
		public uint TimeEnded { get; set; }
		// Static size: 4
		public int ReasonCode { get; set; }
		// Static size: 4
		public int CountAttributes { get; set; }

		public MsgClientOGSEndSession()
		{
			SessionId = 0;
			TimeEnded = 0;
			ReasonCode = 0;
			CountAttributes = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( SessionId );
			bw.Write( TimeEnded );
			bw.Write( ReasonCode );
			bw.Write( CountAttributes );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			SessionId = br.ReadUInt64();
			TimeEnded = br.ReadUInt32();
			ReasonCode = br.ReadInt32();
			CountAttributes = br.ReadInt32();
		}
	}

	public class MsgClientOGSEndSessionResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientOGSEndSessionResponse; }

		// Static size: 4
		public EResult Result { get; set; }

		public MsgClientOGSEndSessionResponse()
		{
			Result = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
		}
	}

	public class MsgClientOGSWriteRow : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientOGSWriteRow; }

		// Static size: 8
		public ulong SessionId { get; set; }
		// Static size: 4
		public int CountAttributes { get; set; }

		public MsgClientOGSWriteRow()
		{
			SessionId = 0;
			CountAttributes = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( SessionId );
			bw.Write( CountAttributes );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			SessionId = br.ReadUInt64();
			CountAttributes = br.ReadInt32();
		}
	}

	public class MsgClientGetFriendsWhoPlayGame : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetFriendsWhoPlayGame; }

		// Static size: 8
		private ulong gameId;
		public GameID GameId { get { return new GameID( gameId ); } set { gameId = value.ToUint64(); } }

		public MsgClientGetFriendsWhoPlayGame()
		{
			gameId = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( gameId );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			gameId = br.ReadUInt64();
		}
	}

	public class MsgClientGetFriendsWhoPlayGameResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetFriendsWhoPlayGameResponse; }

		// Static size: 4
		public EResult Result { get; set; }
		// Static size: 8
		private ulong gameId;
		public GameID GameId { get { return new GameID( gameId ); } set { gameId = value.ToUint64(); } }
		// Static size: 4
		public uint CountFriends { get; set; }

		public MsgClientGetFriendsWhoPlayGameResponse()
		{
			Result = 0;
			gameId = 0;
			CountFriends = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );
			bw.Write( gameId );
			bw.Write( CountFriends );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
			gameId = br.ReadUInt64();
			CountFriends = br.ReadUInt32();
		}
	}

	public class MsgGSPerformHardwareSurvey : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSPerformHardwareSurvey; }

		// Static size: 4
		public uint Flags { get; set; }

		public MsgGSPerformHardwareSurvey()
		{
			Flags = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( Flags );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Flags = br.ReadUInt32();
		}
	}

	public class MsgGSGetPlayStatsResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSGetPlayStatsResponse; }

		// Static size: 4
		public EResult Result { get; set; }
		// Static size: 4
		public int Rank { get; set; }
		// Static size: 4
		public uint LifetimeConnects { get; set; }
		// Static size: 4
		public uint LifetimeMinutesPlayed { get; set; }

		public MsgGSGetPlayStatsResponse()
		{
			Result = 0;
			Rank = 0;
			LifetimeConnects = 0;
			LifetimeMinutesPlayed = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );
			bw.Write( Rank );
			bw.Write( LifetimeConnects );
			bw.Write( LifetimeMinutesPlayed );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
			Rank = br.ReadInt32();
			LifetimeConnects = br.ReadUInt32();
			LifetimeMinutesPlayed = br.ReadUInt32();
		}
	}

	public class MsgGSGetReputationResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSGetReputationResponse; }

		// Static size: 4
		public EResult Result { get; set; }
		// Static size: 4
		public uint ReputationScore { get; set; }
		// Static size: 1
		private byte banned;
		public bool Banned { get { return ( banned == 1 ); } set { banned = ( byte )( value ? 1 : 0 ); } }
		// Static size: 4
		public uint BannedIp { get; set; }
		// Static size: 2
		public ushort BannedPort { get; set; }
		// Static size: 8
		public ulong BannedGameId { get; set; }
		// Static size: 4
		public uint TimeBanExpires { get; set; }

		public MsgGSGetReputationResponse()
		{
			Result = 0;
			ReputationScore = 0;
			banned = 0;
			BannedIp = 0;
			BannedPort = 0;
			BannedGameId = 0;
			TimeBanExpires = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );
			bw.Write( ReputationScore );
			bw.Write( banned );
			bw.Write( BannedIp );
			bw.Write( BannedPort );
			bw.Write( BannedGameId );
			bw.Write( TimeBanExpires );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
			ReputationScore = br.ReadUInt32();
			banned = br.ReadByte();
			BannedIp = br.ReadUInt32();
			BannedPort = br.ReadUInt16();
			BannedGameId = br.ReadUInt64();
			TimeBanExpires = br.ReadUInt32();
		}
	}

	public class MsgGSDeny : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSDeny; }

		// Static size: 8
		private ulong steamId;
		public SteamID SteamId { get { return new SteamID( steamId ); } set { steamId = value.ConvertToUint64(); } }
		// Static size: 4
		public EDenyReason DenyReason { get; set; }

		public MsgGSDeny()
		{
			steamId = 0;
			DenyReason = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamId );
			bw.Write( (int)DenyReason );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamId = br.ReadUInt64();
			DenyReason = (EDenyReason)br.ReadInt32();
		}
	}

	public class MsgGSApprove : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSApprove; }

		// Static size: 8
		private ulong steamId;
		public SteamID SteamId { get { return new SteamID( steamId ); } set { steamId = value.ConvertToUint64(); } }

		public MsgGSApprove()
		{
			steamId = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamId );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamId = br.ReadUInt64();
		}
	}

	public class MsgGSKick : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSKick; }

		// Static size: 8
		private ulong steamId;
		public SteamID SteamId { get { return new SteamID( steamId ); } set { steamId = value.ConvertToUint64(); } }
		// Static size: 4
		public EDenyReason DenyReason { get; set; }
		// Static size: 4
		public int WaitTilMapChange { get; set; }

		public MsgGSKick()
		{
			steamId = 0;
			DenyReason = 0;
			WaitTilMapChange = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamId );
			bw.Write( (int)DenyReason );
			bw.Write( WaitTilMapChange );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamId = br.ReadUInt64();
			DenyReason = (EDenyReason)br.ReadInt32();
			WaitTilMapChange = br.ReadInt32();
		}
	}

	public class MsgGSGetUserGroupStatus : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSGetUserGroupStatus; }

		// Static size: 8
		private ulong steamIdUser;
		public SteamID SteamIdUser { get { return new SteamID( steamIdUser ); } set { steamIdUser = value.ConvertToUint64(); } }
		// Static size: 8
		private ulong steamIdGroup;
		public SteamID SteamIdGroup { get { return new SteamID( steamIdGroup ); } set { steamIdGroup = value.ConvertToUint64(); } }

		public MsgGSGetUserGroupStatus()
		{
			steamIdUser = 0;
			steamIdGroup = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdUser );
			bw.Write( steamIdGroup );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdUser = br.ReadUInt64();
			steamIdGroup = br.ReadUInt64();
		}
	}

	public class MsgGSGetUserGroupStatusResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSGetUserGroupStatusResponse; }

		// Static size: 8
		private ulong steamIdUser;
		public SteamID SteamIdUser { get { return new SteamID( steamIdUser ); } set { steamIdUser = value.ConvertToUint64(); } }
		// Static size: 8
		private ulong steamIdGroup;
		public SteamID SteamIdGroup { get { return new SteamID( steamIdGroup ); } set { steamIdGroup = value.ConvertToUint64(); } }
		// Static size: 4
		public EClanRelationship ClanRelationship { get; set; }
		// Static size: 4
		public EClanRank ClanRank { get; set; }

		public MsgGSGetUserGroupStatusResponse()
		{
			steamIdUser = 0;
			steamIdGroup = 0;
			ClanRelationship = 0;
			ClanRank = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdUser );
			bw.Write( steamIdGroup );
			bw.Write( (int)ClanRelationship );
			bw.Write( (int)ClanRank );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdUser = br.ReadUInt64();
			steamIdGroup = br.ReadUInt64();
			ClanRelationship = (EClanRelationship)br.ReadInt32();
			ClanRank = (EClanRank)br.ReadInt32();
		}
	}

	public class MsgClientJoinChat : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientJoinChat; }

		// Static size: 8
		private ulong steamIdChat;
		public SteamID SteamIdChat { get { return new SteamID( steamIdChat ); } set { steamIdChat = value.ConvertToUint64(); } }
		// Static size: 1
		private byte isVoiceSpeaker;
		public bool IsVoiceSpeaker { get { return ( isVoiceSpeaker == 1 ); } set { isVoiceSpeaker = ( byte )( value ? 1 : 0 ); } }

		public MsgClientJoinChat()
		{
			steamIdChat = 0;
			isVoiceSpeaker = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdChat );
			bw.Write( isVoiceSpeaker );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdChat = br.ReadUInt64();
			isVoiceSpeaker = br.ReadByte();
		}
	}

	public class MsgClientChatEnter : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChatEnter; }

		// Static size: 8
		private ulong steamIdChat;
		public SteamID SteamIdChat { get { return new SteamID( steamIdChat ); } set { steamIdChat = value.ConvertToUint64(); } }
		// Static size: 8
		private ulong steamIdFriend;
		public SteamID SteamIdFriend { get { return new SteamID( steamIdFriend ); } set { steamIdFriend = value.ConvertToUint64(); } }
		// Static size: 4
		public EChatRoomType ChatRoomType { get; set; }
		// Static size: 8
		private ulong steamIdOwner;
		public SteamID SteamIdOwner { get { return new SteamID( steamIdOwner ); } set { steamIdOwner = value.ConvertToUint64(); } }
		// Static size: 8
		private ulong steamIdClan;
		public SteamID SteamIdClan { get { return new SteamID( steamIdClan ); } set { steamIdClan = value.ConvertToUint64(); } }
		// Static size: 1
		public byte ChatFlags { get; set; }
		// Static size: 4
		public EChatRoomEnterResponse EnterResponse { get; set; }

		public MsgClientChatEnter()
		{
			steamIdChat = 0;
			steamIdFriend = 0;
			ChatRoomType = 0;
			steamIdOwner = 0;
			steamIdClan = 0;
			ChatFlags = 0;
			EnterResponse = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdChat );
			bw.Write( steamIdFriend );
			bw.Write( (int)ChatRoomType );
			bw.Write( steamIdOwner );
			bw.Write( steamIdClan );
			bw.Write( ChatFlags );
			bw.Write( (int)EnterResponse );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdChat = br.ReadUInt64();
			steamIdFriend = br.ReadUInt64();
			ChatRoomType = (EChatRoomType)br.ReadInt32();
			steamIdOwner = br.ReadUInt64();
			steamIdClan = br.ReadUInt64();
			ChatFlags = br.ReadByte();
			EnterResponse = (EChatRoomEnterResponse)br.ReadInt32();
		}
	}

	public class MsgClientChatMsg : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChatMsg; }

		// Static size: 8
		private ulong steamIdChatter;
		public SteamID SteamIdChatter { get { return new SteamID( steamIdChatter ); } set { steamIdChatter = value.ConvertToUint64(); } }
		// Static size: 8
		private ulong steamIdChatRoom;
		public SteamID SteamIdChatRoom { get { return new SteamID( steamIdChatRoom ); } set { steamIdChatRoom = value.ConvertToUint64(); } }
		// Static size: 4
		public EChatEntryType ChatMsgType { get; set; }

		public MsgClientChatMsg()
		{
			steamIdChatter = 0;
			steamIdChatRoom = 0;
			ChatMsgType = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdChatter );
			bw.Write( steamIdChatRoom );
			bw.Write( (int)ChatMsgType );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdChatter = br.ReadUInt64();
			steamIdChatRoom = br.ReadUInt64();
			ChatMsgType = (EChatEntryType)br.ReadInt32();
		}
	}

	public class MsgClientChatMemberInfo : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChatMemberInfo; }

		// Static size: 8
		private ulong steamIdChat;
		public SteamID SteamIdChat { get { return new SteamID( steamIdChat ); } set { steamIdChat = value.ConvertToUint64(); } }
		// Static size: 4
		public EChatInfoType Type { get; set; }

		public MsgClientChatMemberInfo()
		{
			steamIdChat = 0;
			Type = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdChat );
			bw.Write( (int)Type );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdChat = br.ReadUInt64();
			Type = (EChatInfoType)br.ReadInt32();
		}
	}

	public class MsgClientChatActionResult : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChatActionResult; }

		// Static size: 8
		private ulong steamIdChat;
		public SteamID SteamIdChat { get { return new SteamID( steamIdChat ); } set { steamIdChat = value.ConvertToUint64(); } }
		// Static size: 8
		private ulong steamIdUserActedOn;
		public SteamID SteamIdUserActedOn { get { return new SteamID( steamIdUserActedOn ); } set { steamIdUserActedOn = value.ConvertToUint64(); } }
		// Static size: 4
		public EChatAction ChatAction { get; set; }
		// Static size: 4
		public EChatActionResult ActionResult { get; set; }

		public MsgClientChatActionResult()
		{
			steamIdChat = 0;
			steamIdUserActedOn = 0;
			ChatAction = 0;
			ActionResult = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( steamIdChat );
			bw.Write( steamIdUserActedOn );
			bw.Write( (int)ChatAction );
			bw.Write( (int)ActionResult );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			steamIdChat = br.ReadUInt64();
			steamIdUserActedOn = br.ReadUInt64();
			ChatAction = (EChatAction)br.ReadInt32();
			ActionResult = (EChatActionResult)br.ReadInt32();
		}
	}

}
#pragma warning restore 1591
