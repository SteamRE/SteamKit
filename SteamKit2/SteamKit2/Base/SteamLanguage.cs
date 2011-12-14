#pragma warning disable 1591
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamKit2
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
	public interface IGCSerializableHeader : ISteamSerializable
	{
		void SetEMsg( EGCMsg msg );
	}
	public interface IGCSerializableMessage : ISteamSerializable
	{
		EGCMsg GetEMsg();
	}

	public enum EMsg
	{
		Invalid = 0,
		Multi = 1,
		BaseGeneral = 100,
		GenericReply = 100,
		DestJobFailed = 113,
		Alert = 115,
		SCIDRequest = 120,
		SCIDResponse = 121,
		JobHeartbeat = 123,
		Subscribe = 126,
		RouteMessage = 127,
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
		StatsDeprecated = 138,
		Ping = 139,
		PingResponse = 140,
		Stats = 141,
		RequestFullStatsBlock = 142,
		BaseShell = 200,
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
		BaseGM = 300,
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
		TestInitDB = 323,
		BaseAIS = 400,
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
		AISTestAddPackage = 422,
		BaseAM = 500,
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
		AMGetLicenses = 539,
		AMGetLicensesResponse = 540,
		AllowUserToPlayQuery = 550,
		AllowUserToPlayResponse = 551,
		AMVerfiyUser = 552,
		AMClientNotPlaying = 553,
		ClientRequestFriendship = 554,
		AMRelayPublishStatus = 555,
		AMResetCommunityContent = 556,
		AMPrimePersonaStateCache = 557,
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
		BaseVS = 600,
		ReqChallenge = 600,
		VACResponse = 601,
		ReqChallengeTest = 602,
		VSMarkCheat = 604,
		VSAddCheat = 605,
		VSPurgeCodeModDB = 606,
		VSGetChallengeResults = 607,
		VSChallengeResultText = 608,
		VSReportLingerer = 609,
		VSRequestManagedChallenge = 610,
		BaseDRMS = 625,
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
		DRMSyncResponse = 643,
		DRMProcessFileResponse = 644,
		DRMEmptyGuidCache = 645,
		DRMEmptyGuidCacheResponse = 646,
		BaseCS = 650,
		CSUserContentRequest = 652,
		BaseClient = 700,
		ClientLogOn_Deprecated = 701,
		ClientAnonLogOn_Deprecated = 702,
		ClientHeartBeat = 703,
		ClientVACResponse = 704,
		ClientGamesPlayed_obsolete = 705,
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
		ClientGameConnect_obsolete = 719,
		ClientGamesPlayed2_obsolete = 720,
		ClientGameEnded_obsolete = 721,
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
		ClientGamesPlayed3_obsolete = 738,
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
		ClientFavoritesList = 786,
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
		ClientUpdateChatMetadata = 811,
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
		ClientGetLobbyListResponse = 860,
		ClientGetLobbyMetadata = 861,
		ClientGetLobbyMetadataResponse = 862,
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
		BaseGameServer = 900,
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
		AMGetUserGroupStatus = 921,
		AMGetUserGroupStatusResponse = 922,
		GSGetUserGroupStatusResponse = 923,
		GSGetReputation = 936,
		GSGetReputationResponse = 937,
		GSAssociateWithClan = 938,
		GSAssociateWithClanResponse = 939,
		GSComputeNewPlayerCompatibility = 940,
		GSComputeNewPlayerCompatibilityResponse = 941,
		BaseAdmin = 1000,
		AdminCmd = 1000,
		AdminCmdResponse = 1004,
		AdminLogListenRequest = 1005,
		AdminLogEvent = 1006,
		LogSearchRequest = 1007,
		LogSearchResponse = 1008,
		LogSearchCancel = 1009,
		UniverseData = 1010,
		RequestStatHistory = 1014,
		StatHistory = 1015,
		AdminPwLogon = 1017,
		AdminPwLogonResponse = 1018,
		AdminSpew = 1019,
		AdminConsoleTitle = 1020,
		AdminGCSpew = 1023,
		AdminGCCommand = 1024,
		AdminGCGetCommandList = 1025,
		AdminGCGetCommandListResponse = 1026,
		BaseFBS = 1100,
		FBSReqVersion = 1100,
		FBSVersionInfo = 1101,
		FBSForceRefresh = 1102,
		FBSForceBounce = 1103,
		FBSDeployPackage = 1104,
		FBSDeployResponse = 1105,
		FBSUpdateBootstrapper = 1106,
		FBSSetState = 1107,
		FBSApplyOSUpdates = 1108,
		FBSRunCMDScript = 1109,
		FBSRebootBox = 1110,
		FBSSetBigBrotherMode = 1111,
		FBSMinidumpServer = 1112,
		FBSSetShellCount = 1113,
		FBSDeployHotFixPackage = 1114,
		FBSDeployHotFixResponse = 1115,
		FBSDownloadHotFix = 1116,
		FBSDownloadHotFixResponse = 1117,
		FBSUpdateTargetConfigFile = 1118,
		FBSApplyAccountCred = 1119,
		FBSApplyAccountCredResponse = 1120,
		BaseFileXfer = 1200,
		FileXferRequest = 1200,
		FileXferResponse = 1201,
		FileXferData = 1202,
		FileXferEnd = 1203,
		FileXferDataAck = 1204,
		BaseChannelAuth = 1300,
		ChannelAuthChallenge = 1300,
		ChannelAuthResponse = 1301,
		ChannelAuthResult = 1302,
		ChannelEncryptRequest = 1303,
		ChannelEncryptResponse = 1304,
		ChannelEncryptResult = 1305,
		BaseBS = 1400,
		BSPurchaseStart = 1401,
		BSPurchaseResponse = 1402,
		BSSettleStart = 1404,
		BSSettleComplete = 1406,
		BSBannedRequest = 1407,
		BSInitPayPalTxn = 1408,
		BSInitPayPalTxnResponse = 1409,
		BSGetPayPalUserInfo = 1410,
		BSGetPayPalUserInfoResponse = 1411,
		BSRefundTxn = 1413,
		BSRefundTxnResponse = 1414,
		BSGetEvents = 1415,
		BSChaseRFRRequest = 1416,
		BSPaymentInstrBan = 1417,
		BSPaymentInstrBanResponse = 1418,
		BSProcessGCReports = 1419,
		BSProcessPPReports = 1420,
		BSInitGCBankXferTxn = 1421,
		BSInitGCBankXferTxnResponse = 1422,
		BSQueryGCBankXferTxn = 1423,
		BSQueryGCBankXferTxnResponse = 1424,
		BSCommitGCTxn = 1425,
		BSQueryGCOrderStatus = 1426,
		BSQueryGCOrderStatusResponse = 1427,
		BSQueryCBOrderStatus = 1428,
		BSQueryCBOrderStatusResponse = 1429,
		BSRunRedFlagReport = 1430,
		BSQueryPaymentInstUsage = 1431,
		BSQueryPaymentInstResponse = 1432,
		BSQueryTxnExtendedInfo = 1433,
		BSQueryTxnExtendedInfoResponse = 1434,
		BSUpdateConversionRates = 1435,
		BSProcessUSBankReports = 1436,
		BSPurchaseMicroTxn = 1437,
		BSPurchaseMicroTxnResponse = 1438,
		BSStartShippingJobs = 1439,
		BaseATS = 1500,
		ATSStartStressTest = 1501,
		ATSStopStressTest = 1502,
		ATSRunFailServerTest = 1503,
		ATSUFSPerfTestTask = 1504,
		ATSUFSPerfTestResponse = 1505,
		ATSCycleTCM = 1506,
		ATSInitDRMSStressTest = 1507,
		ATSCallTest = 1508,
		ATSCallTestReply = 1509,
		ATSStartExternalStress = 1510,
		ATSExternalStressJobStart = 1511,
		ATSExternalStressJobQueued = 1512,
		ATSExternalStressJobRunning = 1513,
		ATSExternalStressJobStopped = 1514,
		ATSExternalStressJobStopAll = 1515,
		ATSExternalStressActionResult = 1516,
		ATSStarted = 1517,
		ATSCSPerfTestTask = 1518,
		ATSCSPerfTestResponse = 1519,
		BaseDP = 1600,
		DPSetPublishingState = 1601,
		DPGamePlayedStats = 1602,
		DPUniquePlayersStat = 1603,
		DPVacInfractionStats = 1605,
		DPVacBanStats = 1606,
		DPCoplayStats = 1607,
		DPNatTraversalStats = 1608,
		DPSteamUsageEvent = 1609,
		DPVacCertBanStats = 1610,
		DPVacCafeBanStats = 1611,
		DPCloudStats = 1612,
		DPAchievementStats = 1613,
		DPAccountCreationStats = 1614,
		DPGetPlayerCount = 1615,
		DPGetPlayerCountResponse = 1616,
		DPGameServersPlayersStats = 1617,
		DPDownloadRateStatistics = 1618,
		BaseCM = 1700,
		CMSetAllowState = 1701,
		CMSpewAllowState = 1702,
		CMAppInfoResponse = 1703,
		BaseDSS = 1800,
		DSSNewFile = 1801,
		DSSCurrentFileList = 1802,
		DSSSynchList = 1803,
		DSSSynchListResponse = 1804,
		DSSSynchSubscribe = 1805,
		DSSSynchUnsubscribe = 1806,
		BaseEPM = 1900,
		EPMStartProcess = 1901,
		EPMStopProcess = 1902,
		EPMRestartProcess = 1903,
		BaseGC = 2200,
		GCSendClient = 2200,
		AMRelayToGC = 2201,
		GCUpdatePlayedState = 2202,
		GCCmdRevive = 2203,
		GCCmdBounce = 2204,
		GCCmdForceBounce = 2205,
		GCCmdDown = 2206,
		GCCmdDeploy = 2207,
		GCCmdDeployResponse = 2208,
		GCCmdSwitch = 2209,
		AMRefreshSessions = 2210,
		GCUpdateGSState = 2211,
		GCAchievementAwarded = 2212,
		GCSystemMessage = 2213,
		GCValidateSession = 2214,
		GCValidateSessionResponse = 2215,
		GCCmdStatus = 2216,
		GCRegisterWebInterfaces = 2217,
		GCGetAccountDetails = 2218,
		GCInterAppMessage = 2219,
		BaseP2P = 2500,
		P2PIntroducerMessage = 2502,
		BaseSM = 2900,
		SMExpensiveReport = 2902,
		SMHourlyReport = 2903,
		SMFishingReport = 2904,
		SMPartitionRenames = 2905,
		BaseTest = 3000,
		FailServer = 3000,
		JobHeartbeatTest = 3001,
		JobHeartbeatTestResponse = 3002,
		BaseFTSRange = 3100,
		FTSGetBrowseCounts = 3101,
		FTSGetBrowseCountsResponse = 3102,
		FTSBrowseClans = 3103,
		FTSBrowseClansResponse = 3104,
		FTSSearchClansByLocation = 3105,
		FTSSearchClansByLocationResponse = 3106,
		FTSSearchPlayersByLocation = 3107,
		FTSSearchPlayersByLocationResponse = 3108,
		FTSClanDeleted = 3109,
		FTSSearch = 3110,
		FTSSearchResponse = 3111,
		FTSSearchStatus = 3112,
		FTSSearchStatusResponse = 3113,
		FTSGetGSPlayStats = 3114,
		FTSGetGSPlayStatsResponse = 3115,
		FTSGetGSPlayStatsForServer = 3116,
		FTSGetGSPlayStatsForServerResponse = 3117,
		BaseCCSRange = 3150,
		CCSGetComments = 3151,
		CCSGetCommentsResponse = 3152,
		CCSAddComment = 3153,
		CCSAddCommentResponse = 3154,
		CCSDeleteComment = 3155,
		CCSDeleteCommentResponse = 3156,
		CCSPreloadComments = 3157,
		CCSNotifyCommentCount = 3158,
		CCSGetCommentsForNews = 3159,
		CCSGetCommentsForNewsResponse = 3160,
		CCSDeleteAllComments = 3161,
		CCSDeleteAllCommentsResponse = 3162,
		BaseLBSRange = 3200,
		LBSSetScore = 3201,
		LBSSetScoreResponse = 3202,
		LBSFindOrCreateLB = 3203,
		LBSFindOrCreateLBResponse = 3204,
		LBSGetLBEntries = 3205,
		LBSGetLBEntriesResponse = 3206,
		LBSGetLBList = 3207,
		LBSGetLBListResponse = 3208,
		LBSSetLBDetails = 3209,
		LBSDeleteLB = 3210,
		LBSDeleteLBEntry = 3211,
		LBSResetLB = 3212,
		BaseOGS = 3400,
		OGSBeginSession = 3401,
		OGSBeginSessionResponse = 3402,
		OGSEndSession = 3403,
		OGSEndSessionResponse = 3404,
		OGSWriteAppSessionRow = 3406,
		BaseAMRange2 = 4000,
		AMCreateChat = 4001,
		AMCreateChatResponse = 4002,
		AMUpdateChatMetadata = 4003,
		AMPublishChatMetadata = 4004,
		AMSetProfileURL = 4005,
		AMGetAccountEmailAddress = 4006,
		AMGetAccountEmailAddressResponse = 4007,
		AMRequestFriendData = 4008,
		AMRouteToClients = 4009,
		AMLeaveClan = 4010,
		AMClanPermissions = 4011,
		AMClanPermissionsResponse = 4012,
		AMCreateClanEvent = 4013,
		AMCreateClanEventResponse = 4014,
		AMUpdateClanEvent = 4015,
		AMUpdateClanEventResponse = 4016,
		AMGetClanEvents = 4017,
		AMGetClanEventsResponse = 4018,
		AMDeleteClanEvent = 4019,
		AMDeleteClanEventResponse = 4020,
		AMSetClanPermissionSettings = 4021,
		AMSetClanPermissionSettingsResponse = 4022,
		AMGetClanPermissionSettings = 4023,
		AMGetClanPermissionSettingsResponse = 4024,
		AMPublishChatRoomInfo = 4025,
		ClientChatRoomInfo = 4026,
		AMCreateClanAnnouncement = 4027,
		AMCreateClanAnnouncementResponse = 4028,
		AMUpdateClanAnnouncement = 4029,
		AMUpdateClanAnnouncementResponse = 4030,
		AMGetClanAnnouncementsCount = 4031,
		AMGetClanAnnouncementsCountResponse = 4032,
		AMGetClanAnnouncements = 4033,
		AMGetClanAnnouncementsResponse = 4034,
		AMDeleteClanAnnouncement = 4035,
		AMDeleteClanAnnouncementResponse = 4036,
		AMGetSingleClanAnnouncement = 4037,
		AMGetSingleClanAnnouncementResponse = 4038,
		AMGetClanHistory = 4039,
		AMGetClanHistoryResponse = 4040,
		AMGetClanPermissionBits = 4041,
		AMGetClanPermissionBitsResponse = 4042,
		AMSetClanPermissionBits = 4043,
		AMSetClanPermissionBitsResponse = 4044,
		AMSessionInfoRequest = 4045,
		AMSessionInfoResponse = 4046,
		AMValidateWGToken = 4047,
		AMGetSingleClanEvent = 4048,
		AMGetSingleClanEventResponse = 4049,
		AMGetClanRank = 4050,
		AMGetClanRankResponse = 4051,
		AMSetClanRank = 4052,
		AMSetClanRankResponse = 4053,
		AMGetClanPOTW = 4054,
		AMGetClanPOTWResponse = 4055,
		AMSetClanPOTW = 4056,
		AMSetClanPOTWResponse = 4057,
		AMRequestChatMetadata = 4058,
		AMDumpUser = 4059,
		AMKickUserFromClan = 4060,
		AMAddFounderToClan = 4061,
		AMValidateWGTokenResponse = 4062,
		AMSetCommunityState = 4063,
		AMSetAccountDetails = 4064,
		AMGetChatBanList = 4065,
		AMGetChatBanListResponse = 4066,
		AMUnBanFromChat = 4067,
		AMSetClanDetails = 4068,
		AMGetAccountLinks = 4069,
		AMGetAccountLinksResponse = 4070,
		AMSetAccountLinks = 4071,
		AMSetAccountLinksResponse = 4072,
		AMGetUserGameStats = 4073,
		AMGetUserGameStatsResponse = 4074,
		AMCheckClanMembership = 4075,
		AMGetClanMembers = 4076,
		AMGetClanMembersResponse = 4077,
		AMJoinPublicClan = 4078,
		AMNotifyChatOfClanChange = 4079,
		AMResubmitPurchase = 4080,
		AMAddFriend = 4081,
		AMAddFriendResponse = 4082,
		AMRemoveFriend = 4083,
		AMDumpClan = 4084,
		AMChangeClanOwner = 4085,
		AMCancelEasyCollect = 4086,
		AMCancelEasyCollectResponse = 4087,
		AMGetClanMembershipList = 4088,
		AMGetClanMembershipListResponse = 4089,
		AMClansInCommon = 4090,
		AMClansInCommonResponse = 4091,
		AMIsValidAccountID = 4092,
		AMConvertClan = 4093,
		AMGetGiftTargetListRelay = 4094,
		AMWipeFriendsList = 4095,
		AMSetIgnored = 4096,
		AMClansInCommonCountResponse = 4097,
		AMFriendsList = 4098,
		AMFriendsListResponse = 4099,
		AMFriendsInCommon = 4100,
		AMFriendsInCommonResponse = 4101,
		AMFriendsInCommonCountResponse = 4102,
		AMClansInCommonCount = 4103,
		AMChallengeVerdict = 4104,
		AMChallengeNotification = 4105,
		AMFindGSByIP = 4106,
		AMFoundGSByIP = 4107,
		AMGiftRevoked = 4108,
		AMCreateAccountRecord = 4109,
		AMUserClanList = 4110,
		AMUserClanListResponse = 4111,
		AMGetAccountDetails2 = 4112,
		AMGetAccountDetailsResponse2 = 4113,
		AMSetCommunityProfileSettings = 4114,
		AMSetCommunityProfileSettingsResponse = 4115,
		AMGetCommunityPrivacyState = 4116,
		AMGetCommunityPrivacyStateResponse = 4117,
		AMCheckClanInviteRateLimiting = 4118,
		AMGetUserAchievementStatus = 4119,
		AMGetIgnored = 4120,
		AMGetIgnoredResponse = 4121,
		AMSetIgnoredResponse = 4122,
		AMSetFriendRelationshipNone = 4123,
		AMGetFriendRelationship = 4124,
		AMGetFriendRelationshipResponse = 4125,
		AMServiceModulesCache = 4126,
		AMServiceModulesCall = 4127,
		AMServiceModulesCallResponse = 4128,
		AMGetCaptchaDataForIP = 4129,
		AMGetCaptchaDataForIPResponse = 4130,
		AMValidateCaptchaDataForIP = 4131,
		AMValidateCaptchaDataForIPResponse = 4132,
		AMTrackFailedAuthByIP = 4133,
		AMGetCaptchaDataByGID = 4134,
		AMGetCaptchaDataByGIDResponse = 4135,
		AMGetLobbyList = 4136,
		AMGetLobbyListResponse = 4137,
		AMGetLobbyMetadata = 4138,
		AMGetLobbyMetadataResponse = 4139,
		AMAddFriendNews = 4140,
		AMAddClanNews = 4141,
		AMWriteNews = 4142,
		AMFindClanUser = 4143,
		AMFindClanUserResponse = 4144,
		AMBanFromChat = 4145,
		AMGetUserHistoryResponse = 4146,
		AMGetUserNewsSubscriptions = 4147,
		AMGetUserNewsSubscriptionsResponse = 4148,
		AMSetUserNewsSubscriptions = 4149,
		AMGetUserNews = 4150,
		AMGetUserNewsResponse = 4151,
		AMSendQueuedEmails = 4152,
		AMSetLicenseFlags = 4153,
		AMGetUserHistory = 4154,
		AMDeleteUserNews = 4155,
		AMAllowUserFilesRequest = 4156,
		AMAllowUserFilesResponse = 4157,
		AMGetAccountStatus = 4158,
		AMGetAccountStatusResponse = 4159,
		AMEditBanReason = 4160,
		AMProbeClanMembershipList = 4162,
		AMProbeClanMembershipListResponse = 4163,
		AMGetFriendsLobbies = 4165,
		AMGetFriendsLobbiesResponse = 4166,
		AMGetUserFriendNewsResponse = 4172,
		AMGetUserFriendNews = 4173,
		AMGetUserClansNewsResponse = 4174,
		AMGetUserClansNews = 4175,
		AMStoreInitPurchase = 4176,
		AMStoreInitPurchaseResponse = 4177,
		AMStoreGetFinalPrice = 4178,
		AMStoreGetFinalPriceResponse = 4179,
		AMStoreCompletePurchase = 4180,
		AMStoreCancelPurchase = 4181,
		AMStorePurchaseResponse = 4182,
		AMCreateAccountRecordInSteam3 = 4183,
		AMGetPreviousCBAccount = 4184,
		AMGetPreviousCBAccountResponse = 4185,
		AMUpdateBillingAddress = 4186,
		AMUpdateBillingAddressResponse = 4187,
		AMGetBillingAddress = 4188,
		AMGetBillingAddressResponse = 4189,
		AMGetUserLicenseHistory = 4190,
		AMGetUserLicenseHistoryResponse = 4191,
		AMSupportChangePassword = 4194,
		AMSupportChangeEmail = 4195,
		AMSupportChangeSecretQA = 4196,
		AMResetUserVerificationGSByIP = 4197,
		AMUpdateGSPlayStats = 4198,
		AMSupportEnableOrDisable = 4199,
		AMGetComments = 4200,
		AMGetCommentsResponse = 4201,
		AMAddComment = 4202,
		AMAddCommentResponse = 4203,
		AMDeleteComment = 4204,
		AMDeleteCommentResponse = 4205,
		AMGetPurchaseStatus = 4206,
		AMSupportIsAccountEnabled = 4209,
		AMSupportIsAccountEnabledResponse = 4210,
		AMGetUserStats = 4211,
		AMSupportKickSession = 4212,
		AMGSSearch = 4213,
		MarketingMessageUpdate = 4216,
		AMRouteFriendMsg = 4219,
		AMTicketAuthRequestOrResponse = 4220,
		AMVerifyDepotManagementRights = 4222,
		AMVerifyDepotManagementRightsResponse = 4223,
		AMAddFreeLicense = 4224,
		AMGetUserFriendsMinutesPlayed = 4225,
		AMGetUserFriendsMinutesPlayedResponse = 4226,
		AMGetUserMinutesPlayed = 4227,
		AMGetUserMinutesPlayedResponse = 4228,
		AMValidateEmailLink = 4231,
		AMValidateEmailLinkResponse = 4232,
		AMAddUsersToMarketingTreatment = 4234,
		AMStoreUserStats = 4236,
		AMGetUserGameplayInfo = 4237,
		AMGetUserGameplayInfoResponse = 4238,
		AMGetCardList = 4239,
		AMGetCardListResponse = 4240,
		AMDeleteStoredCard = 4241,
		AMRevokeLegacyGameKeys = 4242,
		AMGetWalletDetails = 4244,
		AMGetWalletDetailsResponse = 4245,
		AMDeleteStoredPaymentInfo = 4246,
		AMGetStoredPaymentSummary = 4247,
		AMGetStoredPaymentSummaryResponse = 4248,
		AMGetWalletConversionRate = 4249,
		AMGetWalletConversionRateResponse = 4250,
		AMConvertWallet = 4251,
		AMConvertWalletResponse = 4252,
		AMRelayGetFriendsWhoPlayGame = 4253,
		AMRelayGetFriendsWhoPlayGameResponse = 4254,
		AMSetPreApproval = 4255,
		AMSetPreApprovalResponse = 4256,
		AMMarketingTreatmentUpdate = 4257,
		AMCreateRefund = 4258,
		AMCreateRefundResponse = 4259,
		AMCreateChargeback = 4260,
		AMCreateChargebackResponse = 4261,
		AMCreateDispute = 4262,
		AMCreateDisputeResponse = 4263,
		AMClearDispute = 4264,
		AMClearDisputeResponse = 4265,
		AMSetDRMTestConfig = 4268,
		AMGetUserCurrentGameInfo = 4269,
		AMGetUserCurrentGameInfoResponse = 4270,
		AMGetGSPlayerList = 4271,
		AMGetGSPlayerListResponse = 4272,
		AMUpdatePersonaStateCache = 4275,
		AMGetGameMembers = 4276,
		AMGetGameMembersResponse = 4277,
		AMGetSteamIDForMicroTxn = 4278,
		AMGetSteamIDForMicroTxnResponse = 4279,
		AMAddPublisherUser = 4280,
		AMRemovePublisherUser = 4281,
		AMGetUserLicenseList = 4282,
		AMGetUserLicenseListResponse = 4283,
		AMReloadGameGroupPolicy = 4284,
		AMAddFreeLicenseResponse = 4285,
		AMVACStatusUpdate = 4286,
		AMGetAccountDetails = 4287,
		AMGetAccountDetailsResponse = 4288,
		AMGetPlayerLinkDetails = 4289,
		AMGetPlayerLinkDetailsResponse = 4290,
		AMSubscribeToPersonaFeed = 4291,
		AMGetUserVacBanList = 4292,
		AMGetUserVacBanListResponse = 4293,
		AMGetAccountFlagsForWGSpoofing = 4294,
		AMGetAccountFlagsForWGSpoofingResponse = 4295,
		AMGetFriendsWishlistInfo = 4296,
		AMGetFriendsWishlistInfoResponse = 4297,
		AMGetClanOfficers = 4298,
		AMGetClanOfficersResponse = 4299,
		AMNameChange = 4300,
		AMGetNameHistory = 4301,
		AMGetNameHistoryResponse = 4302,
		AMUpdateProviderStatus = 4305,
		AMClearPersonaMetadataBlob = 4306,
		AMSupportRemoveAccountSecurity = 4307,
		AMIsAccountInCaptchaGracePeriod = 4308,
		AMIsAccountInCaptchaGracePeriodResponse = 4309,
		AMAccountPS3Unlink = 4310,
		AMAccountPS3UnlinkResponse = 4311,
		AMStoreUserStatsResponse = 4312,
		AMGameServerUpdate = 4331,
		AMGameServerRemove = 4332,
		BasePSRange = 5000,
		PSCreateShoppingCart = 5001,
		PSCreateShoppingCartResponse = 5002,
		PSIsValidShoppingCart = 5003,
		PSIsValidShoppingCartResponse = 5004,
		PSAddPackageToShoppingCart = 5005,
		PSAddPackageToShoppingCartResponse = 5006,
		PSRemoveLineItemFromShoppingCart = 5007,
		PSRemoveLineItemFromShoppingCartResponse = 5008,
		PSGetShoppingCartContents = 5009,
		PSGetShoppingCartContentsResponse = 5010,
		PSAddWalletCreditToShoppingCart = 5011,
		PSAddWalletCreditToShoppingCartResponse = 5012,
		BaseUFSRange = 5200,
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
		UFSReloadPartitionInfo = 5215,
		ClientUFSTransferHeartbeat = 5216,
		UFSSynchronizeFile = 5217,
		UFSSynchronizeFileResponse = 5218,
		ClientUFSDeleteFileRequest = 5219,
		ClientUFSDeleteFileResponse = 5220,
		UFSDownloadRequest = 5221,
		UFSDownloadResponse = 5222,
		UFSDownloadChunk = 5223,
		ClientUFSGetUGCDetails = 5226,
		ClientUFSGetUGCDetailsResponse = 5227,
		UFSUpdateFileFlags = 5228,
		UFSUpdateFileFlagsResponse = 5229,
		ClientUFSGetSingleFileInfo = 5230,
		ClientUFSGetSingleFileInfoResponse = 5231,
		ClientUFSShareFile = 5232,
		ClientUFSShareFileResponse = 5233,
		UFSReloadAccount = 5234,
		UFSReloadAccountResponse = 5235,
		UFSUpdateRecordBatched = 5236,
		UFSUpdateRecordBatchedResponse = 5237,
		UFSMigrateFile = 5238,
		UFSMigrateFileResponse = 5239,
		UFSGetUGCURLs = 5240,
		UFSGetUGCURLsResponse = 5241,
		BaseClient2 = 5400,
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
		ClientRequestAuthList = 5431,
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
		BaseDFS = 5600,
		DFSGetFile = 5601,
		DFSInstallLocalFile = 5602,
		DFSConnection = 5603,
		DFSConnectionReply = 5604,
		ClientDFSAuthenticateRequest = 5605,
		ClientDFSAuthenticateResponse = 5606,
		ClientDFSEndSession = 5607,
		DFSPurgeFile = 5608,
		DFSRouteFile = 5609,
		DFSGetFileFromServer = 5610,
		DFSAcceptedResponse = 5611,
		DFSRequestPingback = 5612,
		DFSRecvTransmitFile = 5613,
		DFSSendTransmitFile = 5614,
		DFSRequestPingback2 = 5615,
		DFSResponsePingback2 = 5616,
		ClientDFSDownloadStatus = 5617,
		BaseMDS = 5800,
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
		AMToMDSGetDepotDecryptionKey = 5812,
		MDSToAMGetDepotDecryptionKeyResponse = 5813,
		MDSGetVersionsForDepot = 5814,
		MDSGetVersionsForDepotResponse = 5815,
		MDSSetPublicVersionForDepot = 5816,
		MDSSetPublicVersionForDepotResponse = 5817,
		ClientMDSGetDepotManifest = 5818,
		ClientMDSGetDepotManifestResponse = 5819,
		ClientMDSGetDepotManifestChunk = 5820,
		ClientMDSDownloadDepotChunksRequest = 5823,
		MDSDownloadDepotChunksAsync = 5824,
		MDSDownloadDepotChunksAck = 5825,
		MDSContentServerStatsBroadcast = 5826,
		MDSContentServerConfigRequest = 5827,
		MDSContentServerConfig = 5828,
		MDSGetDepotManifest = 5829,
		MDSGetDepotManifestResponse = 5830,
		MDSGetDepotManifestChunk = 5831,
		MDSGetDepotChunk = 5832,
		MDSGetDepotChunkResponse = 5833,
		MDSGetDepotChunkChunk = 5834,
		MDSUpdateContentServerConfig = 5835,
		MDSGetServerListForUser = 5836,
		MDSGetServerListForUserResponse = 5837,
		ClientMDSRegisterAppBuild = 5838,
		ClientMDSRegisterAppBuildResponse = 5839,
		ClientMDSSetAppBuildLive = 5840,
		ClientMDSSetAppBuildLiveResponse = 5841,
		CSBase = 6200,
		GMSBase = 6400,
		GMSGameServerReplicate = 6401,
		ClientGMSServerQuery = 6403,
		GMSClientServerQueryResponse = 6404,
		MMSBase = 6600,
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
		NonStdMsgBase = 6800,
		NonStdMsgMemcached = 6801,
		NonStdMsgHTTPServer = 6802,
		NonStdMsgHTTPClient = 6803,
		NonStdMsgWGResponse = 6804,
		NonStdMsgPHPSimulator = 6805,
		NonStdMsgChase = 6806,
		NonStdMsgDFSTransfer = 6807,
		NonStdMsgTests = 6808,
		UDSBase = 7000,
		ClientUDSP2PSessionStarted = 7001,
		ClientUDSP2PSessionEnded = 7002,
		UDSRenderUserAuth = 7003,
		UDSRenderUserAuthResponse = 7004,
		ClientUDSInviteToGame = 7005,
		UDSFindSession = 7006,
		UDSFindSessionResponse = 7007,
		KGSBase = 7200,
		KGSAllocateKeyRange = 7201,
		KGSAllocateKeyRangeResponse = 7202,
		KGSGenerateKeys = 7203,
		KGSGenerateKeysResponse = 7204,
		UCMBase = 7300,
		ClientUCMAddScreenshot = 7301,
		ClientUCMAddScreenshotResponse = 7302,
		UCMValidateObjectExists = 7303,
		UCMValidateObjectExistsResponse = 7304,
		UCMResetCommunityContent = 7307,
		UCMResetCommunityContentResponse = 7308,
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
		FSBase = 7500,
		ClientRichPresenceUpload = 7501,
		ClientRichPresenceRequest = 7502,
		ClientRichPresenceInfo = 7503,
		FSRichPresenceRequest = 7504,
		FSRichPresenceResponse = 7505,
		DRMRange2 = 7600,
		CEGVersionSetEnableDisableRequest = 7600,
		CEGVersionSetEnableDisableResponse = 7601,
		CEGPropStatusDRMSRequest = 7602,
		CEGPropStatusDRMSResponse = 7603,
		CEGWhackFailureReportRequest = 7604,
		CEGWhackFailureReportResponse = 7605,
		EconBase = 7700,
		EconTrading_InitiateTradeRequest = 7701,
		EconTrading_InitiateTradeProposed = 7702,
		EconTrading_InitiateTradeResponse = 7703,
		EconTrading_InitiateTradeResult = 7704,
		EconTrading_StartSession = 7705,
		EconTrading_CancelTradeRequest = 7706,
		RMRange = 7800,
		RMTestVerisignOTP = 7800,
		RMTestVerisignOTPResponse = 7801,
		RMDeleteMemcachedKeys = 7803,
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
		ParentalControlRestricted = 69,
		FacebookQueryError = 70,
		ExpiredLoginAuthCode = 71,
		IPLoginRestrictionFailed = 72,
		Max = 73,
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
	public enum EAppInfoSection
	{
		AppInfoSectionUnknown = 0,
		AppInfoSectionAll = 1,
		AppInfoSectionCommon = 2,
		AppInfoSectionExtended = 3,
		AppInfoSectionConfig = 4,
		AppInfoSectionStats = 5,
		AppInfoSectionInstall = 6,
		AppInfoSectionDepots = 7,
		AppInfoSectionVac = 8,
		AppInfoSectionDrm = 9,
		AppInfoSectionUfs = 10,
		AppInfoSectionOgg = 11,
		AppInfoSectionItems = 12,
		AppInfoSectionPolicies = 13,
		AppInfoSectionSysreqs = 14,
		Max = 15,
	}
	public enum EContentDownloadSourceType
	{
		ContentDownloadSourceTypeInvalid = 0,
		ContentDownloadSourceTypeCS = 1,
		ContentDownloadSourceTypeCDN = 2,
		ContentDownloadSourceTypeLCS = 3,
		ContentDownloadSourceTypeProxy = 4,
		Max = 5,
	}
	public enum EOSType
	{
		OSTypeUnknown = -1,
		OSTypeMacos = -102,
		OSTypeMacos104 = -101,
		OSTypeMacos105 = -100,
		OSTypeMacos1058 = -99,
		OSTypeMacos106 = -95,
		OSTypeMacos1063 = -94,
		OSTypeMacos1064_slgu = -93,
		OSTypeMacos1067 = -92,
		OSTypeMacos107 = -90,
		OSTypeLinux = -203,
		OSTypeLinux22 = -202,
		OSTypeLinux24 = -201,
		OSTypeLinux26 = -200,
		OSTypeWindows = 0,
		OSTypeWin311 = 1,
		OSTypeWin95 = 2,
		OSTypeWin98 = 3,
		OSTypeWinME = 4,
		OSTypeWinNT = 5,
		OSTypeWin200 = 6,
		OSTypeWinXP = 7,
		OSTypeWin2003 = 8,
		OSTypeWinVista = 9,
		OSTypeWin7 = 10,
		OSTypeWin2008 = 11,
		Max = 12,
	}
	public enum EServerType
	{
		ServerTypeInvalid = -1,
		ServerTypeShell = 0,
		ServerTypeGM = 1,
		ServerTypeBUM = 2,
		ServerTypeAM = 3,
		ServerTypeBS = 4,
		ServerTypeVS = 5,
		ServerTypeATS = 6,
		ServerTypeCM = 7,
		ServerTypeFBS = 8,
		ServerTypeFG = 9,
		ServerTypeSS = 10,
		ServerTypeDRMS = 11,
		ServerTypeHubOBSOLETE = 12,
		ServerTypeConsole = 13,
		ServerTypeASBOBSOLETE = 14,
		ServerTypeClient = 15,
		ServerTypeBootstrapOBSOLETE = 16,
		ServerTypeDP = 17,
		ServerTypeWG = 18,
		ServerTypeSM = 19,
		ServerTypeUFS = 21,
		ServerTypeUtil = 23,
		ServerTypeDSS = 24,
		ServerTypeP2PRelayOBSOLETE = 25,
		ServerTypeAppInformation = 26,
		ServerTypeSpare = 27,
		ServerTypeFTS = 28,
		ServerTypeEPM = 29,
		ServerTypePS = 30,
		ServerTypeIS = 31,
		ServerTypeCCS = 32,
		ServerTypeDFS = 33,
		ServerTypeLBS = 34,
		ServerTypeMDS = 35,
		ServerTypeCS = 36,
		ServerTypeGC = 37,
		ServerTypeNS = 38,
		ServerTypeOGS = 39,
		ServerTypeWebAPI = 40,
		ServerTypeUDS = 41,
		ServerTypeMMS = 42,
		ServerTypeGMS = 43,
		ServerTypeKGS = 44,
		ServerTypeUCM = 45,
		ServerTypeRM = 46,
		ServerTypeFS = 47,
		ServerTypeEcon = 48,
		ServerTypeBackpack = 49,
		Max = 50,
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
		public long TargetJobID { get; set; }
		// Static size: 8
		public long SourceJobID { get; set; }

		public MsgHdr()
		{
			Msg = EMsg.Invalid;
			TargetJobID = -1;
			SourceJobID = -1;
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
		public SteamKit2.CMsgProtoBufHeader ProtoHeader { get; set; }

		public MsgHdrProtoBuf()
		{
			Msg = EMsg.Invalid;
			HeaderLength = 0;
			ProtoHeader = new SteamKit2.CMsgProtoBufHeader();
		}

		public void Serialize(Stream stream)
		{
			MemoryStream msProtoHeader = new MemoryStream();
			ProtoBuf.Serializer.Serialize<SteamKit2.CMsgProtoBufHeader>(msProtoHeader, ProtoHeader);
			HeaderLength = (int)msProtoHeader.Length;
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)MsgUtil.MakeMsg( Msg, true ) );
			bw.Write( HeaderLength );
			bw.Write( msProtoHeader.ToArray() );

			msProtoHeader.Close();
		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Msg = (EMsg)MsgUtil.GetMsg( (uint)br.ReadInt32() );
			HeaderLength = br.ReadInt32();
			using( MemoryStream msProtoHeader = new MemoryStream( br.ReadBytes( HeaderLength ) ) )
				ProtoHeader = ProtoBuf.Serializer.Deserialize<SteamKit2.CMsgProtoBufHeader>( msProtoHeader );
		}
	}

	public enum EGCMsg
	{
		Invalid = 0,
		GenericReply = 10,
		SOMsg_Create = 21,
		SOMsg_Update = 22,
		SOMsg_Destroy = 23,
		SOMsg_CacheSubscribed = 24,
		SOMsg_CacheUnsubscribed = 25,
		SOMsg_UpdateMultiple = 26,
		AchievementAwarded = 51,
		ConCommand = 52,
		StartPlaying = 53,
		StopPlaying = 54,
		StartGameserver = 55,
		StopGameserver = 56,
		WGRequest = 57,
		WGResponse = 58,
		GetUserGameStatsSchema = 59,
		GetUserGameStatsSchemaResponse = 60,
		GetUserStatsDEPRECATED = 61,
		GetUserStatsResponse = 62,
		AppInfoUpdated = 63,
		ValidateSession = 64,
		ValidateSessionResponse = 65,
		LookupAccountFromInput = 66,
		SendHTTPRequest = 67,
		SendHTTPRequestResponse = 68,
		PreTestSetup = 69,
		RecordSupportAction = 70,
		GetAccountDetails = 71,
		SendInterAppMessage = 72,
		ReceiveInterAppMessage = 73,
		FindAccounts = 74,
		PostAlert = 75,
		GetLicenses = 76,
		GetUserStats = 77,
		GetCommands = 78,
		GetCommandsResponse = 79,
		AddFreeLicense = 80,
		AddFreeLicenseResponse = 81,
		WebAPIRegisterInterfaces = 101,
		WebAPIJobRequest = 102,
		WebAPIRegistrationRequested = 103,
		MemCachedGet = 200,
		MemCachedGetResponse = 201,
		MemCachedSet = 202,
		MemCachedDelete = 203,
		SetItemPosition = 1001,
		Craft = 1002,
		CraftResponse = 1003,
		Delete = 1004,
		VerifyCacheSubscription = 1005,
		NameItem = 1006,
		DecodeItem = 1007,
		DecodeItemResponse = 1008,
		PaintItem = 1009,
		PaintItemResponse = 1010,
		GoldenWrenchBroadcast = 1011,
		MOTDRequest = 1012,
		MOTDRequestResponse = 1013,
		AddItemToSocket = 1014,
		AddItemToSocketResponse = 1015,
		AddSocketToBaseItem = 1016,
		AddSocketToItem = 1017,
		AddSocketToItemResponse = 1018,
		NameBaseItem = 1019,
		NameBaseItemResponse = 1020,
		RemoveSocketItem = 1021,
		RemoveSocketItemResponse = 1022,
		CustomizeItemTexture = 1023,
		CustomizeItemTextureResponse = 1024,
		UseItemRequest = 1025,
		UseItemResponse = 1026,
		GiftedItems = 1027,
		SpawnItem = 1028,
		RespawnPostLoadoutChange = 1029,
		RemoveItemName = 1030,
		RemoveItemPaint = 1031,
		GiftWrapItem = 1032,
		GiftWrapItemResponse = 1033,
		DeliverGift = 1034,
		DeliverGiftResponseGiver = 1035,
		DeliverGiftResponseReceiver = 1036,
		UnwrapGiftRequest = 1037,
		UnwrapGiftResponse = 1038,
		SetItemStyle = 1039,
		UsedClaimCodeItem = 1040,
		SortItems = 1041,
		RevolvingLootList = 1042,
		LookupAccount = 1043,
		LookupAccountResponse = 1044,
		LookupAccountName = 1045,
		LookupAccountNameResponse = 1046,
		StartupCheck = 1047,
		StartupCheckResponse = 1048,
		UpdateItemSchema = 1049,
		RequestInventoryRefresh = 1050,
		Trading_InitiateTradeRequest = 1501,
		Trading_InitiateTradeResponse = 1502,
		Trading_StartSession = 1503,
		Trading_SetItem = 1504,
		Trading_RemoveItem = 1505,
		Trading_UpdateTradeInfo = 1506,
		Trading_SetReadiness = 1507,
		Trading_ReadinessResponse = 1508,
		Trading_SessionClosed = 1509,
		Trading_CancelSession = 1510,
		Trading_TradeChatMsg = 1511,
		Trading_ConfirmOffer = 1512,
		Trading_TradeTypingChatMsg = 1513,
		ServerBrowser_FavoriteServer = 1601,
		ServerBrowser_BlacklistServer = 1602,
		Dev_NewItemRequest = 2001,
		Dev_NewItemRequestResponse = 2002,
		StoreGetUserData = 2500,
		StoreGetUserDataResponse = 2501,
		StorePurchaseInit = 2502,
		StorePurchaseInitResponse = 2503,
		StorePurchaseFinalize = 2504,
		StorePurchaseFinalizeResponse = 2505,
		StorePurchaseCancel = 2506,
		StorePurchaseCancelResponse = 2507,
		StorePurchaseQueryTxn = 2508,
		StorePurchaseQueryTxnResponse = 2509,
		SystemMessage = 3001,
		ReportWarKill = 5001,
		VoteKickBanPlayer = 5018,
		VoteKickBanPlayerResult = 5019,
		KickPlayer = 5020,
		StartedTraining = 5021,
		FreeTrial_ChooseMostHelpfulFriend = 5022,
		RequestTF2Friends = 5023,
		RequestTF2FriendsResponse = 5024,
		Replay_UploadedToYouTube = 5025,
		Replay_SubmitContestEntry = 5026,
		Replay_SubmitContestEntryResponse = 5027,
		Coaching_AddToCoaches = 5200,
		Coaching_AddToCoachesResponse = 5201,
		Coaching_RemoveFromCoaches = 5202,
		Coaching_RemoveFromCoachesResponse = 5203,
		Coaching_FindCoach = 5204,
		Coaching_FindCoachResponse = 5205,
		Coaching_AskCoach = 5206,
		Coaching_AskCoachResponse = 5207,
		Coaching_CoachJoinGame = 5208,
		Coaching_CoachJoining = 5209,
		Coaching_CoachJoined = 5210,
		Coaching_LikeCurrentCoach = 5211,
		Coaching_RemoveCurrentCoach = 5212,
		Coaching_AlreadyRatedCoach = 5213,
		Duel_Request = 5500,
		Duel_Response = 5501,
		Duel_Results = 5502,
		Duel_Status = 5503,
		Halloween_ReservedItem = 5600,
		Halloween_GrantItem = 5601,
		Halloween_GrantItemResponse = 5604,
		Halloween_Cheat_QueryResponse = 5605,
		Halloween_ItemClaimed = 5606,
		GameServer_LevelInfo = 5700,
		GameServer_AuthChallenge = 5701,
		GameServer_AuthChallengeResponse = 5702,
		GameServer_CreateIdentity = 5703,
		GameServer_CreateIdentityResponse = 5704,
		GameServer_List = 5705,
		GameServer_ListResponse = 5706,
		GameServer_AuthResult = 5707,
		GameServer_ResetIdentity = 5708,
		GameServer_ResetIdentityResponse = 5709,
		QP_ScoreServers = 5800,
		QP_ScoreServersResponse = 5701,
		PickupItemEligibility_Query = 6000,
		Dev_GrantWarKill = 6001,
		IncrementKillCountAttribute = 6100,
		IncrementKillCountResponse = 6101,
		Max = 6102,
	}
	[StructLayout( LayoutKind.Sequential )]
	public class MsgGCHdrProtoBuf : IGCSerializableHeader
	{
		public void SetEMsg( EGCMsg msg ) { this.Msg = msg; }

		// Static size: 4
		public EGCMsg Msg { get; set; }
		// Static size: 4
		public int HeaderLength { get; set; }
		// Static size: 0
		public SteamKit2.GC.CMsgProtoBufHeader ProtoHeader { get; set; }

		public MsgGCHdrProtoBuf()
		{
			Msg = EGCMsg.Invalid;
			HeaderLength = 0;
			ProtoHeader = new SteamKit2.GC.CMsgProtoBufHeader();
		}

		public void Serialize(Stream stream)
		{
			MemoryStream msProtoHeader = new MemoryStream();
			ProtoBuf.Serializer.Serialize<SteamKit2.GC.CMsgProtoBufHeader>(msProtoHeader, ProtoHeader);
			HeaderLength = (int)msProtoHeader.Length;
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)MsgUtil.MakeGCMsg( Msg, true ) );
			bw.Write( HeaderLength );
			bw.Write( msProtoHeader.ToArray() );

			msProtoHeader.Close();
		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Msg = (EGCMsg)MsgUtil.GetGCMsg( (uint)br.ReadInt32() );
			HeaderLength = br.ReadInt32();
			using( MemoryStream msProtoHeader = new MemoryStream( br.ReadBytes( HeaderLength ) ) )
				ProtoHeader = ProtoBuf.Serializer.Deserialize<SteamKit2.GC.CMsgProtoBufHeader>( msProtoHeader );
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	public class MsgGCHdr : IGCSerializableHeader
	{
		public void SetEMsg( EGCMsg msg ) { }

		// Static size: 2
		public ushort HeaderVersion { get; set; }
		// Static size: 8
		public long TargetJobID { get; set; }
		// Static size: 8
		public long SourceJobID { get; set; }

		public MsgGCHdr()
		{
			HeaderVersion = 1;
			TargetJobID = -1;
			SourceJobID = -1;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( HeaderVersion );
			bw.Write( TargetJobID );
			bw.Write( SourceJobID );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			HeaderVersion = br.ReadUInt16();
			TargetJobID = br.ReadInt64();
			SourceJobID = br.ReadInt64();
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

	public class MsgMulti : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.Multi; }

		// Static size: 0
		public CMsgMulti Proto { get; set; }

		public MsgMulti()
		{
			Proto = new CMsgMulti();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgMulti>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgMulti>( stream );
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

	public class MsgClientHeartBeat : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientHeartBeat; }

		// Static size: 0
		public CMsgClientHeartBeat Proto { get; set; }

		public MsgClientHeartBeat()
		{
			Proto = new CMsgClientHeartBeat();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientHeartBeat>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientHeartBeat>( stream );
		}
	}

	public class MsgClientLogon : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogon; }

		public static readonly uint ObfuscationMask = 0xBAADF00D;
		public static readonly uint CurrentProtocol = 65573;
		// Static size: 0
		public CMsgClientLogon Proto { get; set; }

		public MsgClientLogon()
		{
			Proto = new CMsgClientLogon();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientLogon>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogon>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientLogOff>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogOff>( stream );
		}
	}

	public class MsgClientLogOnResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogOnResponse; }

		// Static size: 0
		public CMsgClientLogonResponse Proto { get; set; }

		public MsgClientLogOnResponse()
		{
			Proto = new CMsgClientLogonResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientLogonResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogonResponse>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgGSServerType>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgGSServerType>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgGSStatusReply>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgGSStatusReply>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientRegisterAuthTicketWithCM>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRegisterAuthTicketWithCM>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientGetAppOwnershipTicket>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetAppOwnershipTicket>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientGetAppOwnershipTicketResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetAppOwnershipTicketResponse>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAuthList>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAuthList>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientRequestFriendData>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRequestFriendData>( stream );
		}
	}

	public class MsgClientChangeStatus : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChangeStatus; }

		// Static size: 0
		public CMsgClientChangeStatus Proto { get; set; }

		public MsgClientChangeStatus()
		{
			Proto = new CMsgClientChangeStatus();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientChangeStatus>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientChangeStatus>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientPersonaState>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientPersonaState>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientSessionToken>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientSessionToken>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientGameConnectTokens>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGameConnectTokens>( stream );
		}
	}

	public class MsgClientGamesPlayedWithDataBlob : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGamesPlayedWithDataBlob; }

		// Static size: 0
		public CMsgClientGamesPlayed Proto { get; set; }

		public MsgClientGamesPlayedWithDataBlob()
		{
			Proto = new CMsgClientGamesPlayed();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientGamesPlayed>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGamesPlayed>( stream );
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

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientFriendsList>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientFriendsList>( stream );
		}
	}

	public class MsgClientFriendMsg : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendMsg; }

		// Static size: 0
		public CMsgClientFriendMsg Proto { get; set; }

		public MsgClientFriendMsg()
		{
			Proto = new CMsgClientFriendMsg();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientFriendMsg>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientFriendMsg>( stream );
		}
	}

	public class MsgClientFriendMsgIncoming : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendMsgIncoming; }

		// Static size: 0
		public CMsgClientFriendMsgIncoming Proto { get; set; }

		public MsgClientFriendMsgIncoming()
		{
			Proto = new CMsgClientFriendMsgIncoming();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientFriendMsgIncoming>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientFriendMsgIncoming>( stream );
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

	public class MsgClientAccountInfo : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAccountInfo; }

		// Static size: 0
		public CMsgClientAccountInfo Proto { get; set; }

		public MsgClientAccountInfo()
		{
			Proto = new CMsgClientAccountInfo();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAccountInfo>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAccountInfo>( stream );
		}
	}

	public class MsgClientLicenseList : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLicenseList; }

		// Static size: 0
		public CMsgClientLicenseList Proto { get; set; }

		public MsgClientLicenseList()
		{
			Proto = new CMsgClientLicenseList();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientLicenseList>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLicenseList>( stream );
		}
	}

	public class MsgClientAppInfoUpdate : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppInfoUpdate; }

		// Static size: 0
		public CMsgClientAppInfoUpdate Proto { get; set; }

		public MsgClientAppInfoUpdate()
		{
			Proto = new CMsgClientAppInfoUpdate();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAppInfoUpdate>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAppInfoUpdate>( stream );
		}
	}

	public class MsgClientAppInfoChanges : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppInfoChanges; }

		// Static size: 0
		public CMsgClientAppInfoChanges Proto { get; set; }

		public MsgClientAppInfoChanges()
		{
			Proto = new CMsgClientAppInfoChanges();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAppInfoChanges>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAppInfoChanges>( stream );
		}
	}

	public class MsgClientAppInfoRequest : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppInfoRequest; }

		// Static size: 0
		public CMsgClientAppInfoRequest Proto { get; set; }

		public MsgClientAppInfoRequest()
		{
			Proto = new CMsgClientAppInfoRequest();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAppInfoRequest>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAppInfoRequest>( stream );
		}
	}

	public class MsgClientAppInfoResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppInfoResponse; }

		// Static size: 0
		public CMsgClientAppInfoResponse Proto { get; set; }

		public MsgClientAppInfoResponse()
		{
			Proto = new CMsgClientAppInfoResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAppInfoResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAppInfoResponse>( stream );
		}
	}

	public class MsgClientAppMinutesPlayedData : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAppMinutesPlayedData; }

		// Static size: 0
		public CMsgClientAppMinutesPlayedData Proto { get; set; }

		public MsgClientAppMinutesPlayedData()
		{
			Proto = new CMsgClientAppMinutesPlayedData();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAppMinutesPlayedData>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAppMinutesPlayedData>( stream );
		}
	}

	public class MsgClientWalletInfoUpdate : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientWalletInfoUpdate; }

		// Static size: 0
		public CMsgClientWalletInfoUpdate Proto { get; set; }

		public MsgClientWalletInfoUpdate()
		{
			Proto = new CMsgClientWalletInfoUpdate();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientWalletInfoUpdate>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientWalletInfoUpdate>( stream );
		}
	}

	public class MsgClientCMList : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientCMList; }

		// Static size: 0
		public CMsgClientCMList Proto { get; set; }

		public MsgClientCMList()
		{
			Proto = new CMsgClientCMList();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientCMList>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientCMList>( stream );
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

		// Static size: 0
		public CMsgClientServerList Proto { get; set; }

		public MsgClientServerList()
		{
			Proto = new CMsgClientServerList();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientServerList>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientServerList>( stream );
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

	public class MsgClientAddFriend : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAddFriend; }

		// Static size: 0
		public CMsgClientAddFriend Proto { get; set; }

		public MsgClientAddFriend()
		{
			Proto = new CMsgClientAddFriend();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAddFriend>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAddFriend>( stream );
		}
	}

	public class MsgClientAddFriendResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAddFriendResponse; }

		// Static size: 0
		public CMsgClientAddFriendResponse Proto { get; set; }

		public MsgClientAddFriendResponse()
		{
			Proto = new CMsgClientAddFriendResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientAddFriendResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAddFriendResponse>( stream );
		}
	}

	public class MsgClientRemoveFriend : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRemoveFriend; }

		// Static size: 0
		public CMsgClientRemoveFriend Proto { get; set; }

		public MsgClientRemoveFriend()
		{
			Proto = new CMsgClientRemoveFriend();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientRemoveFriend>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRemoveFriend>( stream );
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

		// Static size: 0
		public CMsgAMGCClientRelay Proto { get; set; }

		public MsgClientFromGC()
		{
			Proto = new CMsgAMGCClientRelay();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgAMGCClientRelay>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgAMGCClientRelay>( stream );
		}
	}

	public class MsgClientToGC : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientToGC; }

		// Static size: 0
		public CMsgAMGCClientRelay Proto { get; set; }

		public MsgClientToGC()
		{
			Proto = new CMsgAMGCClientRelay();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgAMGCClientRelay>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgAMGCClientRelay>( stream );
		}
	}

	public class MsgClientUpdateMachineAuth : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientUpdateMachineAuth; }

		// Static size: 0
		public CMsgClientUpdateMachineAuth Proto { get; set; }

		public MsgClientUpdateMachineAuth()
		{
			Proto = new CMsgClientUpdateMachineAuth();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientUpdateMachineAuth>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientUpdateMachineAuth>( stream );
		}
	}

	public class MsgClientUpdateMachineAuthResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientUpdateMachineAuthResponse; }

		// Static size: 0
		public CMsgClientUpdateMachineAuthResponse Proto { get; set; }

		public MsgClientUpdateMachineAuthResponse()
		{
			Proto = new CMsgClientUpdateMachineAuthResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientUpdateMachineAuthResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientUpdateMachineAuthResponse>( stream );
		}
	}

	public class MsgClientLoggedOff : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLoggedOff; }

		// Static size: 0
		public CMsgClientLoggedOff Proto { get; set; }

		public MsgClientLoggedOff()
		{
			Proto = new CMsgClientLoggedOff();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientLoggedOff>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLoggedOff>( stream );
		}
	}

	public class MsgClientGetUserStats : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetUserStats; }

		// Static size: 0
		public CMsgClientGetUserStats Proto { get; set; }

		public MsgClientGetUserStats()
		{
			Proto = new CMsgClientGetUserStats();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientGetUserStats>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetUserStats>( stream );
		}
	}

	public class MsgClientGetUserStatsResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetUserStatsResponse; }

		// Static size: 0
		public CMsgClientGetUserStatsResponse Proto { get; set; }

		public MsgClientGetUserStatsResponse()
		{
			Proto = new CMsgClientGetUserStatsResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientGetUserStatsResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetUserStatsResponse>( stream );
		}
	}

	public class MsgClientStoreUserStats2 : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientStoreUserStats2; }

		// Static size: 0
		public CMsgClientStoreUserStats2 Proto { get; set; }

		public MsgClientStoreUserStats2()
		{
			Proto = new CMsgClientStoreUserStats2();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientStoreUserStats2>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientStoreUserStats2>( stream );
		}
	}

	public class MsgClientStoreUserStatsResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientStoreUserStatsResponse; }

		// Static size: 0
		public CMsgClientStoreUserStatsResponse Proto { get; set; }

		public MsgClientStoreUserStatsResponse()
		{
			Proto = new CMsgClientStoreUserStatsResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientStoreUserStatsResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientStoreUserStatsResponse>( stream );
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

	public class MsgClientTicketAuthComplete : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientTicketAuthComplete; }

		// Static size: 0
		public CMsgClientTicketAuthComplete Proto { get; set; }

		public MsgClientTicketAuthComplete()
		{
			Proto = new CMsgClientTicketAuthComplete();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientTicketAuthComplete>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientTicketAuthComplete>( stream );
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

	public class MsgClientRequestMachineAuth : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRequestMachineAuth; }

		// Static size: 0
		public CMsgClientRequestMachineAuth Proto { get; set; }

		public MsgClientRequestMachineAuth()
		{
			Proto = new CMsgClientRequestMachineAuth();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientRequestMachineAuth>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRequestMachineAuth>( stream );
		}
	}

	public class MsgClientRequestMachineAuthResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRequestMachineAuthResponse; }

		// Static size: 0
		public CMsgClientRequestMachineAuthResponse Proto { get; set; }

		public MsgClientRequestMachineAuthResponse()
		{
			Proto = new CMsgClientRequestMachineAuthResponse();
		}

		public void Serialize(Stream stream)
		{
			ProtoBuf.Serializer.Serialize<CMsgClientRequestMachineAuthResponse>(stream, Proto);
		}

		public void Deserialize( Stream stream )
		{
			Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRequestMachineAuthResponse>( stream );
		}
	}

	public class MsgClientGetDepotDecryptionKey : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetDepotDecryptionKey; }

		// Static size: 4
		public uint DepotID { get; set; }

		public MsgClientGetDepotDecryptionKey()
		{
			DepotID = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( DepotID );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			DepotID = br.ReadUInt32();
		}
	}

	public class MsgClientGetDepotDecryptionKeyResponse : ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetDepotDecryptionKeyResponse; }

		// Static size: 4
		public EResult Result { get; set; }
		// Static size: 4
		public uint DepotID { get; set; }
		// Static size: 32
		public byte[] DepotEncryptionKey { get; set; }

		public MsgClientGetDepotDecryptionKeyResponse()
		{
			Result = 0;
			DepotID = 0;
			DepotEncryptionKey = new byte[32];
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter bw = new BinaryWriter( stream );

			bw.Write( (int)Result );
			bw.Write( DepotID );
			bw.Write( DepotEncryptionKey );

		}

		public void Deserialize( Stream stream )
		{
			BinaryReader br = new BinaryReader( stream );

			Result = (EResult)br.ReadInt32();
			DepotID = br.ReadUInt32();
			DepotEncryptionKey = br.ReadBytes( 32 );
		}
	}

}
#pragma warning restore 1591
