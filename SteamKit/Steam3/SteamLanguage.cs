using System;
using System.IO;

namespace SteamKit
{
    public interface ISteamSerializable
    {
        MemoryStream serialize();
        void deserialize(MemoryStream ms);
    }
    public interface ISteamSerializableHeader : ISteamSerializable
    {
        void SetEMsg(EMsg msg);
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
        ClientVACBanStatus = 762,
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
        ClientVACBanStatus2 = 782,
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
        ClientAppInfoRequestOld = 864,
        ClientAppInfoResponseOld = 865,
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
        AMGetUserGroupStatus = 921,
        AMGetUserGroupStatusResponse = 922,
        GSGetUserGroupStatusResponse = 923,
        GSGetReputation = 936,
        GSGetReputationResponse = 937,
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
        FBSUpdateTargetConfigFile = 1118,
        FileXferRequest = 1200,
        FileXferResponse = 1201,
        FileXferData = 1202,
        FileXferEnd = 1203,
        FileXferDataAck = 1204,
        ChannelAuthChallenge = 1300,
        ChannelAuthResponse = 1301,
        ChannelAuthResult = 1302,
        ChannelEncryptRequest = 1303,
        ChannelEncryptResponse = 1304,
        ChannelEncryptResult = 1305,
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
        CMSetAllowState = 1701,
        CMSpewAllowState = 1702,
        CMAppInfoResponse = 1703,
        DSSNewFile = 1801,
        DSSSynchList = 1803,
        DSSSynchListResponse = 1804,
        DSSSynchSubscribe = 1805,
        DSSSynchUnsubscribe = 1806,
        EPMStartProcess = 1901,
        EPMStopProcess = 1902,
        EPMRestartProcess = 1903,
        AMInternalAuthComplete = 2000,
        AMInternalRemoveAMSession = 2001,
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
        P2PIntroducerMessage = 2502,
        SMBuildUGSTables = 2901,
        SMExpensiveReport = 2902,
        SMHourlyReport = 2903,
        SMFishingReport = 2904,
        SMPartitionRenames = 2905,
        FailServer = 3000,
        JobHeartbeatTest = 3001,
        JobHeartbeatTestResponse = 3002,
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
        OGSBeginSession = 3401,
        OGSBeginSessionResponse = 3402,
        OGSEndSession = 3403,
        OGSEndSessionResponse = 3404,
        OGSWriteRow = 3405,
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
        AMRouteClientMsgToAM = 4164,
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
        AMGetUserTransactionHistory = 4192,
        AMGetUserTransactionHistoryResponse = 4193,
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
        AMChatDetailsQuery = 4207,
        AMChatDetailsResponse = 4208,
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
        AMRelayCurrentCoplayCount = 4230,
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
        ClientUFSUploadFileRequest = 5202,
        ClientUFSUploadFileResponse = 5203,
        ClientUFSUploadFileChunk = 5204,
        ClientUFSUploadFileFinished = 5205,
        ClientUFSGetFileListForApp = 5206,
        ClientUFSGetFileListForAppResponse = 5207,
        RouteClientMsgToUFS = 5208,
        RouteUFSMsgToClient = 5209,
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
        UFSDeleteFileRequest = 5224,
        UFSDeleteFileResponse = 5225,
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
        ClientReportOverlayDetourFailure = 5517,
        ClientRequestEncryptedAppTicket = 5526,
        ClientRequestEncryptedAppTicketResponse = 5527,
        ClientWalletInfoUpdate = 5528,
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
        ClientMDSDownloadDepotChunksAsync = 5824,
        ClientMDSDownloadDepotChunksAck = 5825,
        MDSContentServerStatsBroadcast = 5826,
        MDSContentServerConfigRequest = 5827,
        MDSContentServerConfig = 5828,
        MDSGetDepotManifest = 5829,
        MDSGetDepotManifestResponse = 5830,
        MDSGetDepotManifestChunk = 5831,
        MDSGetDepotChunk = 5832,
        MDSGetDepotChunkResponse = 5833,
        MDSGetDepotChunkChunk = 5834,
        ClientCSLoginRequest = 6201,
        ClientCSLoginResponse = 6202,
        GMSGameServerReplicate = 6401,
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(36);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(Magic);
            writer.Write(PayloadSize);
            writer.Write((byte)PacketType);
            writer.Write(Flags);
            writer.Write(SourceConnID);
            writer.Write(DestConnID);
            writer.Write(SeqThis);
            writer.Write(SeqAck);
            writer.Write(PacketsInMsg);
            writer.Write(MsgStartSeq);
            writer.Write(MsgSize);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            Magic = reader.ReadUInt32();
            PayloadSize = reader.ReadUInt16();
            PacketType = (EUdpPacketType)reader.ReadByte();
            Flags = reader.ReadByte();
            SourceConnID = reader.ReadUInt32();
            DestConnID = reader.ReadUInt32();
            SeqThis = reader.ReadUInt32();
            SeqAck = reader.ReadUInt32();
            PacketsInMsg = reader.ReadUInt32();
            MsgStartSeq = reader.ReadUInt32();
            MsgSize = reader.ReadUInt32();
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(8);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(ChallengeValue);
            writer.Write(ServerLoad);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            ChallengeValue = reader.ReadUInt32();
            ServerLoad = reader.ReadUInt32();
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(4);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(ChallengeValue);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            ChallengeValue = reader.ReadUInt32();
        }
    }

    public class MsgHdr : ISteamSerializableHeader
    {
        public void SetEMsg(EMsg msg) { this.Msg = msg; }

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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(20);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write((int)Msg);
            writer.Write(TargetJobID);
            writer.Write(SourceJobID);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            Msg = (EMsg)reader.ReadUInt32();
            TargetJobID = reader.ReadInt64();
            SourceJobID = reader.ReadInt64();
        }
    }

    public class ExtendedClientMsgHdr : ISteamSerializableHeader
    {
        public void SetEMsg(EMsg msg) { this.Msg = msg; }

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
        public SteamID SteamID { get { return new SteamID(steamID); } set { steamID = value.ConvertToUint64(); } }
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(36);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write((int)Msg);
            writer.Write(HeaderSize);
            writer.Write(HeaderVersion);
            writer.Write(TargetJobID);
            writer.Write(SourceJobID);
            writer.Write(HeaderCanary);
            writer.Write(steamID);
            writer.Write(SessionID);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            Msg = (EMsg)reader.ReadUInt32();
            HeaderSize = reader.ReadByte();
            HeaderVersion = reader.ReadUInt16();
            TargetJobID = reader.ReadInt64();
            SourceJobID = reader.ReadInt64();
            HeaderCanary = reader.ReadByte();
            steamID = reader.ReadUInt64();
            SessionID = reader.ReadInt32();
        }
    }

    public class MsgHdrProtoBuf : ISteamSerializableHeader
    {
        public void SetEMsg(EMsg msg) { this.Msg = msg; }

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

        public MemoryStream serialize()
        {
            MemoryStream msProtoHeader = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgProtoBufHeader>(msProtoHeader, ProtoHeader);
            HeaderLength = (int)msProtoHeader.Length;
            msProtoHeader.Seek(0, SeekOrigin.Begin);
            MemoryStream msBuffer = new MemoryStream(8 + (int)msProtoHeader.Length);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write((int)MsgUtil.MakeMsg(Msg, true));
            writer.Write(HeaderLength);
            msProtoHeader.CopyTo(msBuffer);

            msProtoHeader.Close();

            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            Msg = (EMsg)MsgUtil.GetMsg(reader.ReadUInt32());
            HeaderLength = reader.ReadInt32();
            using (MemoryStream msProtoHeader = new MemoryStream(reader.ReadBytes(HeaderLength)))
                ProtoHeader = ProtoBuf.Serializer.Deserialize<CMsgProtoBufHeader>(msProtoHeader);
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(8);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(ProtocolVersion);
            writer.Write((int)Universe);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            ProtocolVersion = reader.ReadUInt32();
            Universe = (EUniverse)reader.ReadUInt32();
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(8);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(ProtocolVersion);
            writer.Write(KeySize);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            ProtocolVersion = reader.ReadUInt32();
            KeySize = reader.ReadUInt32();
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(4);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write((int)Result);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            Result = (EResult)reader.ReadUInt32();
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgMulti>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgMulti>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(24);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(UniqueID);
            writer.Write(LoginKey);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            UniqueID = reader.ReadUInt32();
            LoginKey = reader.ReadBytes(20);
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(4);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(UniqueID);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            UniqueID = reader.ReadUInt32();
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientHeartBeat>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientHeartBeat>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientLogon>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogon>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientLogonResponse>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientLogonResponse>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgGSServerType>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgGSServerType>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgGSStatusReply>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgGSStatusReply>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientRegisterAuthTicketWithCM>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRegisterAuthTicketWithCM>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientGetAppOwnershipTicket>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetAppOwnershipTicket>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientGetAppOwnershipTicketResponse>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientGetAppOwnershipTicketResponse>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientAuthList>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientAuthList>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientRequestFriendData>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientRequestFriendData>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(1);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(PersonaState);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            PersonaState = reader.ReadByte();
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientPersonaState>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientPersonaState>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientSessionToken>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientSessionToken>(ms);
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

        public MemoryStream serialize()
        {
            MemoryStream msProto = new MemoryStream();
            ProtoBuf.Serializer.Serialize<CMsgClientFriendsList>(msProto, Proto);
            msProto.Seek(0, SeekOrigin.Begin);
            return msProto;
        }

        public void deserialize(MemoryStream ms)
        {
            Proto = ProtoBuf.Serializer.Deserialize<CMsgClientFriendsList>(ms);
        }
    }

    public class MsgClientFriendMsgIncoming : ISteamSerializableMessage
    {
        public EMsg GetEMsg() { return EMsg.ClientFriendMsgIncoming; }

        // Static size: 8
        private ulong steamID;
        public SteamID SteamID { get { return new SteamID(steamID); } set { steamID = value.ConvertToUint64(); } }
        // Static size: 4
        public EChatEntryType EntryType { get; set; }
        // Static size: 4
        public int MessageSize { get; set; }

        public MsgClientFriendMsgIncoming()
        {
            steamID = 0;
            EntryType = 0;
            MessageSize = 0;
        }

        public MemoryStream serialize()
        {
            MemoryStream msBuffer = new MemoryStream(16);

            BinaryWriter writer = new BinaryWriter(msBuffer);

            writer.Write(steamID);
            writer.Write((int)EntryType);
            writer.Write(MessageSize);


            msBuffer.Seek(0, SeekOrigin.Begin);
            return msBuffer;
        }

        public void deserialize(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);

            steamID = reader.ReadUInt64();
            EntryType = (EChatEntryType)reader.ReadUInt32();
            MessageSize = reader.ReadInt32();
        }
    }

}
