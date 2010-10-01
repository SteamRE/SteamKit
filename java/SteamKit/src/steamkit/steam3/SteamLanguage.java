package steamkit.steam3;
import java.nio.*;
import steamkit.steam3.SteamMessages.*;
import steamkit.util.MsgUtil;
import com.google.protobuf.InvalidProtocolBufferException;

public final class SteamLanguage {
	public interface ISteamSerializable
	{
		public ByteBuffer serialize();
		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException;
	}
	public interface ISteamSerializableHeader extends ISteamSerializable
	{
		public void SetEMsg( EMsg msg );
	}
	public interface ISteamSerializableMessage extends ISteamSerializable
	{
		public EMsg GetEMsg();
	}

	public enum EMsg
	{
		Invalid(0),
		Multi(1),
		GenericReply(100),
		DestJobFailed(113),
		Alert(115),
		SCIDRequest(120),
		SCIDResponse(121),
		JobHeartbeat(123),
		Subscribe(126),
		k_EMRouteMessage(127),
		RemoteSysID(128),
		AMCreateAccountResponse(129),
		WGRequest(130),
		WGResponse(131),
		KeepAlive(132),
		WebAPIJobRequest(133),
		WebAPIJobResponse(134),
		ClientSessionStart(135),
		ClientSessionEnd(136),
		ClientSessionUpdateAuthTicket(137),
		Stats(138),
		Ping(139),
		PingResponse(140),
		AssignSysID(200),
		Exit(201),
		DirRequest(202),
		DirResponse(203),
		ZipRequest(204),
		ZipResponse(205),
		UpdateRecordResponse(215),
		UpdateCreditCardRequest(221),
		UpdateUserBanResponse(225),
		PrepareToExit(226),
		ContentDescriptionUpdate(227),
		TestResetServer(228),
		UniverseChanged(229),
		Heartbeat(300),
		ShellFailed(301),
		ExitShells(307),
		ExitShell(308),
		GracefulExitShell(309),
		NotifyWatchdog(314),
		LicenseProcessingComplete(316),
		SetTestFlag(317),
		QueuedEmailsComplete(318),
		GMReportPHPError(319),
		GMDRMSync(320),
		PhysicalBoxInventory(321),
		UpdateConfigFile(322),
		AISRefreshContentDescription(401),
		AISRequestContentDescription(402),
		AISUpdateAppInfo(403),
		AISUpdatePackageInfo(404),
		AISGetPackageChangeNumber(405),
		AISGetPackageChangeNumberResponse(406),
		AISAppInfoTableChanged(407),
		AISUpdatePackageInfoResponse(408),
		AISCreateMarketingMessage(409),
		AISCreateMarketingMessageResponse(410),
		AISGetMarketingMessage(411),
		AISGetMarketingMessageResponse(412),
		AISUpdateMarketingMessage(413),
		AISUpdateMarketingMessageResponse(414),
		AISRequestMarketingMessageUpdate(415),
		AISDeleteMarketingMessage(416),
		AISGetMarketingTreatments(419),
		AISGetMarketingTreatmentsResponse(420),
		AISRequestMarketingTreatmentUpdate(421),
		AMUpdateUserBanRequest(504),
		AMAddLicense(505),
		AMBeginProcessingLicenses(507),
		AMSendSystemIMToUser(508),
		AMExtendLicense(509),
		AMAddMinutesToLicense(510),
		AMCancelLicense(511),
		AMInitPurchase(512),
		AMPurchaseResponse(513),
		AMGetFinalPrice(514),
		AMGetFinalPriceResponse(515),
		AMGetLegacyGameKey(516),
		AMGetLegacyGameKeyResponse(517),
		AMFindHungTransactions(518),
		AMSetAccountTrustedRequest(519),
		AMCompletePurchase(521),
		AMCancelPurchase(522),
		AMNewChallenge(523),
		AMFixPendingPurchase(526),
		AMIsUserBanned(527),
		AMRegisterKey(528),
		AMLoadActivationCodes(529),
		AMLoadActivationCodesResponse(530),
		AMLookupKeyResponse(531),
		AMLookupKey(532),
		AMChatCleanup(533),
		AMClanCleanup(534),
		AMFixPendingRefund(535),
		AMReverseChargeback(536),
		AMReverseChargebackResponse(537),
		AMClanCleanupList(538),
		AllowUserToPlayQuery(550),
		AllowUserToPlayResponse(551),
		AMVerfiyUser(552),
		AMClientNotPlaying(553),
		AMClientRequestFriendship(554),
		AMRelayPublishStatus(555),
		AMResetCommunityContent(556),
		CAMPrimePersonaStateCache(557),
		AMAllowUserContentQuery(558),
		AMAllowUserContentResponse(559),
		AMInitPurchaseResponse(560),
		AMRevokePurchaseResponse(561),
		AMLockProfile(562),
		AMRefreshGuestPasses(563),
		AMInviteUserToClan(564),
		AMAcknowledgeClanInvite(565),
		AMGrantGuestPasses(566),
		AMClanDataUpdated(567),
		AMReloadAccount(568),
		AMClientChatMsgRelay(569),
		AMChatMulti(570),
		AMClientChatInviteRelay(571),
		AMChatInvite(572),
		AMClientJoinChatRelay(573),
		AMClientChatMemberInfoRelay(574),
		AMPublishChatMemberInfo(575),
		AMClientAcceptFriendInvite(576),
		AMChatEnter(577),
		AMClientPublishRemovalFromSource(578),
		AMChatActionResult(579),
		AMFindAccounts(580),
		AMFindAccountsResponse(581),
		AMSetAccountFlags(584),
		AMCreateClan(586),
		AMCreateClanResponse(587),
		AMGetClanDetails(588),
		AMGetClanDetailsResponse(589),
		AMSetPersonaName(590),
		AMSetAvatar(591),
		AMAuthenticateUser(592),
		AMAuthenticateUserResponse(593),
		AMGetAccountFriendsCount(594),
		AMGetAccountFriendsCountResponse(595),
		AMP2PIntroducerMessage(596),
		ClientChatAction(597),
		AMClientChatActionRelay(598),
		ReqChallenge(600),
		VACResponse(601),
		ReqChallengeTest(602),
		VSInitDB(603),
		VSMarkCheat(604),
		VSAddCheat(605),
		VSPurgeCodeModDB(606),
		VSGetChallengeResults(607),
		VSChallengeResultText(608),
		VSReportLingerer(609),
		VSRequestManagedChallenge(610),
		DRMBuildBlobRequest(628),
		DRMBuildBlobResponse(629),
		DRMResolveGuidRequest(630),
		DRMResolveGuidResponse(631),
		DRMVariabilityReport(633),
		DRMVariabilityReportResponse(634),
		DRMStabilityReport(635),
		DRMStabilityReportResponse(636),
		DRMDetailsReportRequest(637),
		DRMDetailsReportResponse(638),
		DRMProcessFile(639),
		DRMAdminUpdate(640),
		DRMAdminUpdateResponse(641),
		DRMSync(642),
		DRMSyncResposne(643),
		DRMProcessFileResponse(644),
		CSManifestUpdate(651),
		CSUserContentRequest(652),
		ClientLogOn_Deprecated(701),
		ClientAnonLogOn_Deprecated(702),
		ClientHeartBeat(703),
		ClientVACResponse(704),
		ClientLogOff(706),
		ClientNoUDPConnectivity(707),
		ClientInformOfCreateAccount(708),
		ClientAckVACBan(709),
		ClientConnectionStats(710),
		ClientInitPurchase(711),
		ClientPingResponse(712),
		ClientAddFriend(713),
		ClientRemoveFriend(714),
		ClientGamesPlayedNoDataBlob(715),
		ClientChangeStatus(716),
		ClientVacStatusResponse(717),
		ClientFriendMsg(718),
		ClientGetFinalPrice(722),
		ClientSystemIM(726),
		ClientSystemIMAck(727),
		ClientGetLicenses(728),
		ClientCancelLicense(729),
		ClientGetLegacyGameKey(730),
		ClientContentServerLogOn_Deprecated(731),
		ClientAckVACBan2(732),
		ClientCompletePurchase(733),
		ClientCancelPurchase(734),
		ClientAckMessageByGID(735),
		ClientGetPurchaseReceipts(736),
		ClientAckPurchaseReceipt(737),
		ClientSendGuestPass(739),
		ClientAckGuestPass(740),
		ClientRedeemGuestPass(741),
		ClientGamesPlayed(742),
		ClientRegisterKey(743),
		ClientInviteUserToClan(744),
		ClientAcknowledgeClanInvite(745),
		ClientPurchaseWithMachineID(746),
		ClientAppUsageEvent(747),
		ClientGetGiftTargetList(748),
		ClientGetGiftTargetListResponse(749),
		ClientLogOnResponse(751),
		ClientVACChallenge(753),
		ClientSetHeartbeatRate(755),
		ClientNotLoggedOnDeprecated(756),
		ClientLoggedOff(757),
		GSApprove(758),
		GSDeny(759),
		GSKick(760),
		ClientCreateAcctResponse(761),
		ClientVACBanStatus(762),
		ClientPurchaseResponse(763),
		ClientPing(764),
		ClientNOP(765),
		ClientPersonaState(766),
		ClientFriendsList(767),
		ClientAccountInfo(768),
		ClientAddFriendResponse(769),
		ClientVacStatusQuery(770),
		ClientNewsUpdate(771),
		ClientGameConnectDeny(773),
		GSStatusReply(774),
		ClientGetFinalPriceResponse(775),
		ClientGameConnectTokens(779),
		ClientLicenseList(780),
		ClientCancelLicenseResponse(781),
		ClientVACBanStatus2(782),
		ClientCMList(783),
		ClientEncryptPct(784),
		ClientGetLegacyGameKeyResponse(785),
		CSUserContentApprove(787),
		CSUserContentDeny(788),
		ClientInitPurchaseResponse(789),
		ClientAddFriend2(791),
		ClientAddFriendResponse2(792),
		ClientInviteFriend(793),
		ClientInviteFriendResponse(794),
		ClientSendGuestPassResponse(795),
		ClientAckGuestPassResponse(796),
		ClientRedeemGuestPassResponse(797),
		ClientUpdateGuestPassesList(798),
		ClientChatMsg(799),
		ClientChatInvite(800),
		ClientJoinChat(801),
		ClientChatMemberInfo(802),
		ClientLogOnWithCredentials_Deprecated(803),
		ClientPasswordChange(804),
		ClientPasswordChangeResponse(805),
		ClientChatEnter(807),
		ClientFriendRemovedFromSource(808),
		ClientCreateChat(809),
		ClientCreateChatResponse(810),
		ClientUpdateChatMetadata(811),
		ClientP2PIntroducerMessage(813),
		ClientChatActionResult(814),
		ClientRequestFriendData(815),
		ClientOneTimeWGAuthPassword(816),
		ClientGetUserStats(818),
		ClientGetUserStatsResponse(819),
		ClientStoreUserStats(820),
		ClientStoreUserStatsResponse(821),
		ClientClanState(822),
		ClientServiceModule(830),
		ClientServiceCall(831),
		ClientServiceCallResponse(832),
		ClientNatTraversalStatEvent(839),
		ClientAppInfoRequest(840),
		ClientAppInfoResponse(841),
		ClientSteamUsageEvent(842),
		ClientEmailChange(843),
		ClientPersonalQAChange(844),
		ClientCheckPassword(845),
		ClientResetPassword(846),
		ClientCheckPasswordResponse(848),
		ClientResetPasswordResponse(849),
		ClientSessionToken(850),
		ClientDRMProblemReport(851),
		ClientSetIgnoreFriend(855),
		ClientSetIgnoreFriendResponse(856),
		ClientGetAppOwnershipTicket(857),
		ClientGetAppOwnershipTicketResponse(858),
		ClientGetLobbyListResponse(860),
		ClientGetLobbyMetadata(861),
		ClientGetLobbyMetadataResponse(862),
		ClientVTTCert(863),
		ClientAppInfoRequestOld(864),
		ClientAppInfoResponseOld(865),
		ClientAppInfoUpdate(866),
		ClientAppInfoChanges(867),
		ClientServerList(880),
		ClientGetFriendsLobbies(888),
		ClientGetFriendsLobbiesResponse(889),
		ClientGetLobbyList(890),
		ClientEmailChangeResponse(891),
		ClientSecretQAChangeResponse(892),
		ClientPasswordChange2(893),
		ClientEmailChange2(894),
		ClientPersonalQAChange2(895),
		ClientDRMBlobRequest(896),
		ClientDRMBlobResponse(897),
		ClientLookupKey(898),
		ClientLookupKeyResponse(899),
		GSDisconnectNotice(901),
		GSStatus(903),
		GSUserPlaying(905),
		GSStatus2(906),
		GSStatusUpdate_Unused(907),
		GSServerType(908),
		GSPlayerList(909),
		GSGetUserAchievementStatus(910),
		GSGetUserAchievementStatusResponse(911),
		GSGetPlayStats(918),
		GSGetPlayStatsResponse(919),
		GSGetUserGroupStatus(920),
		AMGetUserGroupStatus(921),
		AMGetUserGroupStatusResponse(922),
		GSGetUserGroupStatusResponse(923),
		GSGetReputation(936),
		GSGetReputationResponse(937),
		AdminCmd(1000),
		AdminCmdResponse(1004),
		AdminLogListenRequest(1005),
		AdminLogEvent(1006),
		LogSearchRequest(1007),
		LogSearchResponse(1008),
		LogSearchCancel(1009),
		UniverseData(1010),
		RequestStatHistory(1014),
		StatHistory(1015),
		AdminPwLogon(1017),
		AdminPwLogonResponse(1018),
		AdminSpew(1019),
		AdminConsoleTitle(1020),
		AdminGCSpew(1023),
		AdminGCCommand(1024),
		FBSReqVersion(1100),
		FBSVersionInfo(1101),
		FBSForceRefresh(1102),
		FBSForceBounce(1103),
		FBSDeployPackage(1104),
		FBSDeployResponse(1105),
		FBSUpdateBootstrapper(1106),
		FBSSetState(1107),
		FBSApplyOSUpdates(1108),
		FBSRunCMDScript(1109),
		FBSRebootBox(1110),
		FBSSetBigBrotherMode(1111),
		FBSMinidumpServer(1112),
		FBSSetShellCount(1113),
		FBSDeployHotFixPackage(1114),
		FBSDeployHotFixResponse(1115),
		FBSUpdateTargetConfigFile(1118),
		FileXferRequest(1200),
		FileXferResponse(1201),
		FileXferData(1202),
		FileXferEnd(1203),
		FileXferDataAck(1204),
		ChannelAuthChallenge(1300),
		ChannelAuthResponse(1301),
		ChannelAuthResult(1302),
		ChannelEncryptRequest(1303),
		ChannelEncryptResponse(1304),
		ChannelEncryptResult(1305),
		BSPurchaseStart(1401),
		BSPurchaseResponse(1402),
		BSSettleStart(1404),
		BSSettleComplete(1406),
		BSBannedRequest(1407),
		BSInitPayPalTxn(1408),
		BSInitPayPalTxnResponse(1409),
		BSGetPayPalUserInfo(1410),
		BSGetPayPalUserInfoResponse(1411),
		BSRefundTxn(1413),
		BSRefundTxnResponse(1414),
		BSGetEvents(1415),
		BSChaseRFRRequest(1416),
		BSPaymentInstrBan(1417),
		BSPaymentInstrBanResponse(1418),
		BSProcessGCReports(1419),
		BSProcessPPReports(1420),
		BSInitGCBankXferTxn(1421),
		BSInitGCBankXferTxnResponse(1422),
		BSQueryGCBankXferTxn(1423),
		BSQueryGCBankXferTxnResponse(1424),
		BSCommitGCTxn(1425),
		BSQueryGCOrderStatus(1426),
		BSQueryGCOrderStatusResponse(1427),
		BSQueryCBOrderStatus(1428),
		BSQueryCBOrderStatusResponse(1429),
		BSRunRedFlagReport(1430),
		BSQueryPaymentInstUsage(1431),
		BSQueryPaymentInstResponse(1432),
		BSQueryTxnExtendedInfo(1433),
		BSQueryTxnExtendedInfoResponse(1434),
		BSUpdateConversionRates(1435),
		BSProcessUSBankReports(1436),
		ATSStartStressTest(1501),
		ATSStopStressTest(1502),
		ATSRunFailServerTest(1503),
		ATSUFSPerfTestTask(1504),
		ATSUFSPerfTestResponse(1505),
		ATSCycleTCM(1506),
		ATSInitDRMSStressTest(1507),
		ATSCallTest(1508),
		ATSCallTestReply(1509),
		ATSStartExternalStress(1510),
		ATSExternalStressJobStart(1511),
		ATSExternalStressJobQueued(1512),
		ATSExternalStressJobRunning(1513),
		ATSExternalStressJobStopped(1514),
		ATSExternalStressJobStopAll(1515),
		ATSExternalStressActionResult(1516),
		ATSStarted(1517),
		ATSCSPerfTestTask(1518),
		ATSCSPerfTestResponse(1519),
		DPSetPublishingState(1601),
		DPGamePlayedStats(1602),
		DPUniquePlayersStat(1603),
		DPVacInfractionStats(1605),
		DPVacBanStats(1606),
		DPCoplayStats(1607),
		DPNatTraversalStats(1608),
		DPSteamUsageEvent(1609),
		DPVacCertBanStats(1610),
		DPVacCafeBanStats(1611),
		DPCloudStats(1612),
		DPAchievementStats(1613),
		DPAccountCreationStats(1614),
		DPGetPlayerCount(1615),
		DPGetPlayerCountResponse(1616),
		CMSetAllowState(1701),
		CMSpewAllowState(1702),
		CMAppInfoResponse(1703),
		DSSNewFile(1801),
		DSSSynchList(1803),
		DSSSynchListResponse(1804),
		DSSSynchSubscribe(1805),
		DSSSynchUnsubscribe(1806),
		EPMStartProcess(1901),
		EPMStopProcess(1902),
		EPMRestartProcess(1903),
		AMInternalAuthComplete(2000),
		AMInternalRemoveAMSession(2001),
		GCSendClient(2200),
		AMRelayToGC(2201),
		GCUpdatePlayedState(2202),
		GCCmdRevive(2203),
		GCCmdBounce(2204),
		GCCmdForceBounce(2205),
		GCCmdDown(2206),
		GCCmdDeploy(2207),
		GCCmdDeployResponse(2208),
		GCCmdSwitch(2209),
		AMRefreshSessions(2210),
		GCUpdateGSState(2211),
		GCAchievementAwarded(2212),
		GCSystemMessage(2213),
		GCValidateSession(2214),
		GCValidateSessionResponse(2215),
		GCCmdStatus(2216),
		GCRegisterWebInterfaces(2217),
		GCGetAccountDetails(2218),
		GCInterAppMessage(2219),
		P2PIntroducerMessage(2502),
		SMBuildUGSTables(2901),
		SMExpensiveReport(2902),
		SMHourlyReport(2903),
		SMFishingReport(2904),
		SMPartitionRenames(2905),
		FailServer(3000),
		JobHeartbeatTest(3001),
		JobHeartbeatTestResponse(3002),
		FTSGetBrowseCounts(3101),
		FTSGetBrowseCountsResponse(3102),
		FTSBrowseClans(3103),
		FTSBrowseClansResponse(3104),
		FTSSearchClansByLocation(3105),
		FTSSearchClansByLocationResponse(3106),
		FTSSearchPlayersByLocation(3107),
		FTSSearchPlayersByLocationResponse(3108),
		FTSClanDeleted(3109),
		FTSSearch(3110),
		FTSSearchResponse(3111),
		FTSSearchStatus(3112),
		FTSSearchStatusResponse(3113),
		FTSGetGSPlayStats(3114),
		FTSGetGSPlayStatsResponse(3115),
		FTSGetGSPlayStatsForServer(3116),
		FTSGetGSPlayStatsForServerResponse(3117),
		CCSGetComments(3151),
		CCSGetCommentsResponse(3152),
		CCSAddComment(3153),
		CCSAddCommentResponse(3154),
		CCSDeleteComment(3155),
		CCSDeleteCommentResponse(3156),
		CCSPreloadComments(3157),
		CCSNotifyCommentCount(3158),
		CCSGetCommentsForNews(3159),
		CCSGetCommentsForNewsResponse(3160),
		CCSDeleteAllComments(3161),
		CCSDeleteAllCommentsResponse(3162),
		LBSSetScore(3201),
		LBSSetScoreResponse(3202),
		LBSFindOrCreateLB(3203),
		LBSFindOrCreateLBResponse(3204),
		LBSGetLBEntries(3205),
		LBSGetLBEntriesResponse(3206),
		LBSGetLBList(3207),
		LBSGetLBListResponse(3208),
		LBSSetLBDetails(3209),
		LBSDeleteLB(3210),
		LBSDeleteLBEntry(3211),
		LBSResetLB(3212),
		OGSBeginSession(3401),
		OGSBeginSessionResponse(3402),
		OGSEndSession(3403),
		OGSEndSessionResponse(3404),
		OGSWriteRow(3405),
		AMCreateChat(4001),
		AMCreateChatResponse(4002),
		AMUpdateChatMetadata(4003),
		AMPublishChatMetadata(4004),
		AMSetProfileURL(4005),
		AMGetAccountEmailAddress(4006),
		AMGetAccountEmailAddressResponse(4007),
		AMRequestFriendData(4008),
		AMRouteToClients(4009),
		AMLeaveClan(4010),
		AMClanPermissions(4011),
		AMClanPermissionsResponse(4012),
		AMCreateClanEvent(4013),
		AMCreateClanEventResponse(4014),
		AMUpdateClanEvent(4015),
		AMUpdateClanEventResponse(4016),
		AMGetClanEvents(4017),
		AMGetClanEventsResponse(4018),
		AMDeleteClanEvent(4019),
		AMDeleteClanEventResponse(4020),
		AMSetClanPermissionSettings(4021),
		AMSetClanPermissionSettingsResponse(4022),
		AMGetClanPermissionSettings(4023),
		AMGetClanPermissionSettingsResponse(4024),
		AMPublishChatRoomInfo(4025),
		ClientChatRoomInfo(4026),
		AMCreateClanAnnouncement(4027),
		AMCreateClanAnnouncementResponse(4028),
		AMUpdateClanAnnouncement(4029),
		AMUpdateClanAnnouncementResponse(4030),
		AMGetClanAnnouncementsCount(4031),
		AMGetClanAnnouncementsCountResponse(4032),
		AMGetClanAnnouncements(4033),
		AMGetClanAnnouncementsResponse(4034),
		AMDeleteClanAnnouncement(4035),
		AMDeleteClanAnnouncementResponse(4036),
		AMGetSingleClanAnnouncement(4037),
		AMGetSingleClanAnnouncementResponse(4038),
		AMGetClanHistory(4039),
		AMGetClanHistoryResponse(4040),
		AMGetClanPermissionBits(4041),
		AMGetClanPermissionBitsResponse(4042),
		AMSetClanPermissionBits(4043),
		AMSetClanPermissionBitsResponse(4044),
		AMSessionInfoRequest(4045),
		AMSessionInfoResponse(4046),
		AMValidateWGToken(4047),
		AMGetSingleClanEvent(4048),
		AMGetSingleClanEventResponse(4049),
		AMGetClanRank(4050),
		AMGetClanRankResponse(4051),
		AMSetClanRank(4052),
		AMSetClanRankResponse(4053),
		AMGetClanPOTW(4054),
		AMGetClanPOTWResponse(4055),
		AMSetClanPOTW(4056),
		AMRequestChatMetadata(4058),
		AMDumpUser(4059),
		AMKickUserFromClan(4060),
		AMAddFounderToClan(4061),
		AMValidateWGTokenResponse(4062),
		AMSetCommunityState(4063),
		AMSetAccountDetails(4064),
		AMGetChatBanList(4065),
		AMGetChatBanListResponse(4066),
		AMUnBanFromChat(4067),
		AMSetClanDetails(4068),
		AMGetAccountLinks(4069),
		AMGetAccountLinksResponse(4070),
		AMSetAccountLinks(4071),
		AMSetAccountLinksResponse(4072),
		AMGetUserGameStats(4073),
		AMGetUserGameStatsResponse(4074),
		AMCheckClanMembership(4075),
		AMGetClanMembers(4076),
		AMGetClanMembersResponse(4077),
		AMJoinPublicClan(4078),
		AMNotifyChatOfClanChange(4079),
		AMResubmitPurchase(4080),
		AMAddFriend(4081),
		AMAddFriendResponse(4082),
		AMRemoveFriend(4083),
		AMCancelEasyCollect(4086),
		AMCancelEasyCollectResponse(4087),
		AMGetClanMembershipList(4088),
		AMGetClanMembershipListResponse(4089),
		AMClansInCommon(4090),
		AMClansInCommonResponse(4091),
		AMIsValidAccountID(4092),
		AMConvertClan(4093),
		AMGetGiftTargetListRelay(4094),
		AMWipeFriendsList(4095),
		AMSetIgnored(4096),
		AMClansInCommonCountResponse(4097),
		AMFriendsList(4098),
		AMFriendsListResponse(4099),
		AMFriendsInCommon(4100),
		AMFriendsInCommonResponse(4101),
		AMFriendsInCommonCountResponse(4102),
		AMClansInCommonCount(4103),
		AMChallengeVerdict(4104),
		AMChallengeNotification(4105),
		AMFindGSByIP(4106),
		AMFoundGSByIP(4107),
		AMGiftRevoked(4108),
		AMCreateAccountRecord(4109),
		AMUserClanList(4110),
		AMUserClanListResponse(4111),
		AMGetAccountDetails2(4112),
		AMGetAccountDetailsResponse2(4113),
		AMSetCommunityProfileSettings(4114),
		AMSetCommunityProfileSettingsResponse(4115),
		AMGetCommunityPrivacyState(4116),
		AMGetCommunityPrivacyStateResponse(4117),
		AMCheckClanInviteRateLimiting(4118),
		AMGetUserAchievementStatus(4119),
		AMGetIgnored(4120),
		AMGetIgnoredResponse(4121),
		AMSetIgnoredResponse(4122),
		AMSetFriendRelationshipNone(4123),
		AMGetFriendRelationship(4124),
		AMGetFriendRelationshipResponse(4125),
		AMServiceModulesCache(4126),
		AMServiceModulesCall(4127),
		AMServiceModulesCallResponse(4128),
		AMGetCaptchaDataForIP(4129),
		AMGetCaptchaDataForIPResponse(4130),
		AMValidateCaptchaDataForIP(4131),
		AMValidateCaptchaDataForIPResponse(4132),
		AMTrackFailedAuthByIP(4133),
		AMGetCaptchaDataByGID(4134),
		AMGetCaptchaDataByGIDResponse(4135),
		AMGetLobbyList(4136),
		AMGetLobbyListResponse(4137),
		AMGetLobbyMetadata(4138),
		AMGetLobbyMetadataResponse(4139),
		AMAddFriendNews(4140),
		AMAddClanNews(4141),
		AMWriteNews(4142),
		AMFindClanUser(4143),
		AMFindClanUserResponse(4144),
		AMBanFromChat(4145),
		AMGetUserHistoryResponse(4146),
		AMGetUserNewsSubscriptions(4147),
		AMGetUserNewsSubscriptionsResponse(4148),
		AMSetUserNewsSubscriptions(4149),
		AMGetUserNews(4150),
		AMGetUserNewsResponse(4151),
		AMSendQueuedEmails(4152),
		AMSetLicenseFlags(4153),
		AMGetUserHistory(4154),
		AMDeleteUserNews(4155),
		AMAllowUserFilesRequest(4156),
		AMAllowUserFilesResponse(4157),
		AMGetAccountStatus(4158),
		AMGetAccountStatusResponse(4159),
		AMEditBanReason(4160),
		AMProbeClanMembershipList(4162),
		AMProbeClanMembershipListResponse(4163),
		AMRouteClientMsgToAM(4164),
		AMGetFriendsLobbies(4165),
		AMGetFriendsLobbiesResponse(4166),
		AMGetUserFriendNewsResponse(4172),
		AMGetUserFriendNews(4173),
		AMGetUserClansNewsResponse(4174),
		AMGetUserClansNews(4175),
		AMStoreInitPurchase(4176),
		AMStoreInitPurchaseResponse(4177),
		AMStoreGetFinalPrice(4178),
		AMStoreGetFinalPriceResponse(4179),
		AMStoreCompletePurchase(4180),
		AMStoreCancelPurchase(4181),
		AMStorePurchaseResponse(4182),
		AMCreateAccountRecordInSteam3(4183),
		AMGetPreviousCBAccount(4184),
		AMGetPreviousCBAccountResponse(4185),
		AMUpdateBillingAddress(4186),
		AMUpdateBillingAddressResponse(4187),
		AMGetBillingAddress(4188),
		AMGetBillingAddressResponse(4189),
		AMGetUserLicenseHistory(4190),
		AMGetUserLicenseHistoryResponse(4191),
		AMGetUserTransactionHistory(4192),
		AMGetUserTransactionHistoryResponse(4193),
		AMSupportChangePassword(4194),
		AMSupportChangeEmail(4195),
		AMSupportChangeSecretQA(4196),
		AMResetUserVerificationGSByIP(4197),
		AMUpdateGSPlayStats(4198),
		AMSupportEnableOrDisable(4199),
		AMGetComments(4200),
		AMGetCommentsResponse(4201),
		AMAddComment(4202),
		AMAddCommentResponse(4203),
		AMDeleteComment(4204),
		AMDeleteCommentResponse(4205),
		AMGetPurchaseStatus(4206),
		AMChatDetailsQuery(4207),
		AMChatDetailsResponse(4208),
		AMSupportIsAccountEnabled(4209),
		AMSupportIsAccountEnabledResponse(4210),
		AMGetUserStats(4211),
		AMSupportKickSession(4212),
		AMGSSearch(4213),
		MarketingMessageUpdate(4216),
		AMRouteFriendMsg(4219),
		AMTicketAuthRequestOrResponse(4220),
		AMVerifyDepotManagementRights(4222),
		AMVerifyDepotManagementRightsResponse(4223),
		AMAddFreeLicense(4224),
		AMGetUserFriendsMinutesPlayed(4225),
		AMGetUserFriendsMinutesPlayedResponse(4226),
		AMGetUserMinutesPlayed(4227),
		AMGetUserMinutesPlayedResponse(4228),
		AMRelayCurrentCoplayCount(4230),
		AMValidateEmailLink(4231),
		AMValidateEmailLinkResponse(4232),
		AMAddUsersToMarketingTreatment(4234),
		AMStoreUserStats(4236),
		AMGetUserGameplayInfo(4237),
		AMGetUserGameplayInfoResponse(4238),
		AMGetCardList(4239),
		AMGetCardListResponse(4240),
		AMDeleteStoredCard(4241),
		AMRevokeLegacyGameKeys(4242),
		AMGetWalletDetails(4244),
		AMGetWalletDetailsResponse(4245),
		AMDeleteStoredPaymentInfo(4246),
		AMGetStoredPaymentSummary(4247),
		AMGetStoredPaymentSummaryResponse(4248),
		AMGetWalletConversionRate(4249),
		AMGetWalletConversionRateResponse(4250),
		AMConvertWallet(4251),
		AMConvertWalletResponse(4252),
		AMRelayGetFriendsWhoPlayGame(4253),
		AMRelayGetFriendsWhoPlayGameResponse(4254),
		AMSetPreApproval(4255),
		AMSetPreApprovalResponse(4256),
		AMMarketingTreatmentUpdate(4257),
		AMCreateRefund(4258),
		AMCreateRefundResponse(4259),
		AMCreateChargeback(4260),
		AMCreateChargebackResponse(4261),
		AMCreateDispute(4262),
		AMCreateDisputeResponse(4263),
		AMClearDispute(4264),
		AMClearDisputeResponse(4265),
		AMSetDRMTestConfig(4268),
		AMGetUserCurrentGameInfo(4269),
		AMGetUserCurrentGameInfoResponse(4270),
		AMGetGSPlayerList(4271),
		AMGetGSPlayerListResponse(4272),
		AMUpdatePersonaStateCache(4275),
		AMGetGameMembers(4276),
		AMGetGameMembersResponse(4277),
		AMGetSteamIDForMicroTxn(4278),
		AMGetSteamIDForMicroTxnResponse(4279),
		AMAddPublisherUser(4280),
		AMRemovePublisherUser(4281),
		AMGetUserLicenseList(4282),
		AMGetUserLicenseListResponse(4283),
		AMReloadGameGroupPolicy(4284),
		AMAddFreeLicenseResponse(4285),
		AMVACStatusUpdate(4286),
		AMGetAccountDetails(4287),
		AMGetAccountDetailsResponse(4288),
		AMGetPlayerLinkDetails(4289),
		AMGetPlayerLinkDetailsResponse(4290),
		AMSubscribeToPersonaFeed(4291),
		AMGetUserVacBanList(4292),
		AMGetUserVacBanListResponse(4293),
		PSCreateShoppingCart(5001),
		PSCreateShoppingCartResponse(5002),
		PSIsValidShoppingCart(5003),
		PSIsValidShoppingCartResponse(5004),
		PSAddPackageToShoppingCart(5005),
		PSAddPackageToShoppingCartResponse(5006),
		PSRemoveLineItemFromShoppingCart(5007),
		PSRemoveLineItemFromShoppingCartResponse(5008),
		PSGetShoppingCartContents(5009),
		PSGetShoppingCartContentsResponse(5010),
		PSAddWalletCreditToShoppingCart(5011),
		PSAddWalletCreditToShoppingCartResponse(5012),
		ClientUFSUploadFileRequest(5202),
		ClientUFSUploadFileResponse(5203),
		ClientUFSUploadFileChunk(5204),
		ClientUFSUploadFileFinished(5205),
		ClientUFSGetFileListForApp(5206),
		ClientUFSGetFileListForAppResponse(5207),
		RouteClientMsgToUFS(5208),
		RouteUFSMsgToClient(5209),
		ClientUFSDownloadRequest(5210),
		ClientUFSDownloadResponse(5211),
		ClientUFSDownloadChunk(5212),
		ClientUFSLoginRequest(5213),
		ClientUFSLoginResponse(5214),
		UFSReloadPartitionInfo(5215),
		ClientUFSTransferHeartbeat(5216),
		UFSSynchronizeFile(5217),
		UFSSynchronizeFileResponse(5218),
		ClientUFSDeleteFileRequest(5219),
		ClientUFSDeleteFileResponse(5220),
		UFSDownloadRequest(5221),
		UFSDownloadResponse(5222),
		UFSDownloadChunk(5223),
		UFSDeleteFileRequest(5224),
		UFSDeleteFileResponse(5225),
		ClientRequestForgottenPasswordEmail(5401),
		ClientRequestForgottenPasswordEmailResponse(5402),
		ClientCreateAccountResponse(5403),
		ClientResetForgottenPassword(5404),
		ClientResetForgottenPasswordResponse(5405),
		ClientCreateAccount2(5406),
		ClientInformOfResetForgottenPassword(5407),
		ClientInformOfResetForgottenPasswordResponse(5408),
		ClientAnonUserLogOn_Deprecated(5409),
		ClientGamesPlayedWithDataBlob(5410),
		ClientUpdateUserGameInfo(5411),
		ClientFileToDownload(5412),
		ClientFileToDownloadResponse(5413),
		ClientLBSSetScore(5414),
		ClientLBSSetScoreResponse(5415),
		ClientLBSFindOrCreateLB(5416),
		ClientLBSFindOrCreateLBResponse(5417),
		ClientLBSGetLBEntries(5418),
		ClientLBSGetLBEntriesResponse(5419),
		ClientMarketingMessageUpdate(5420),
		ClientChatDeclined(5426),
		ClientFriendMsgIncoming(5427),
		ClientAuthList_Deprecated(5428),
		ClientTicketAuthComplete(5429),
		ClientIsLimitedAccount(5430),
		ClientRequestAuthList(5431),
		ClientAuthList(5432),
		ClientStat(5433),
		ClientP2PConnectionInfo(5434),
		ClientP2PConnectionFailInfo(5435),
		ClientGetNumberOfCurrentPlayers(5436),
		ClientGetNumberOfCurrentPlayersResponse(5437),
		ClientGetDepotDecryptionKey(5438),
		ClientGetDepotDecryptionKeyResponse(5439),
		GSPerformHardwareSurvey(5440),
		ClientEnableTestLicense(5443),
		ClientEnableTestLicenseResponse(5444),
		ClientDisableTestLicense(5445),
		ClientDisableTestLicenseResponse(5446),
		ClientRequestValidationMail(5448),
		ClientRequestValidationMailResponse(5449),
		ClientToGC(5452),
		ClientFromGC(5453),
		ClientRequestChangeMail(5454),
		ClientRequestChangeMailResponse(5455),
		ClientEmailAddrInfo(5456),
		ClientPasswordChange3(5457),
		ClientEmailChange3(5458),
		ClientPersonalQAChange3(5459),
		ClientResetForgottenPassword3(5460),
		ClientRequestForgottenPasswordEmail3(5461),
		ClientCreateAccount3(5462),
		ClientNewLoginKey(5463),
		ClientNewLoginKeyAccepted(5464),
		ClientLogOnWithHash_Deprecated(5465),
		ClientStoreUserStats2(5466),
		ClientStatsUpdated(5467),
		ClientActivateOEMLicense(5468),
		ClientRequestedClientStats(5480),
		ClientStat2Int32(5481),
		ClientStat2(5482),
		ClientVerifyPassword(5483),
		ClientVerifyPasswordResponse(5484),
		ClientDRMDownloadRequest(5485),
		ClientDRMDownloadResponse(5486),
		ClientDRMFinalResult(5487),
		ClientGetFriendsWhoPlayGame(5488),
		ClientGetFriendsWhoPlayGameResponse(5489),
		ClientOGSBeginSession(5490),
		ClientOGSBeginSessionResponse(5491),
		ClientOGSEndSession(5492),
		ClientOGSEndSessionResponse(5493),
		ClientOGSWriteRow(5494),
		ClientDRMTest(5495),
		ClientDRMTestResult(5496),
		ClientServerUnavailable(5500),
		ClientServersAvailable(5501),
		ClientRegisterAuthTicketWithCM(5502),
		ClientGCMsgFailed(5503),
		ClientMicroTxnAuthRequest(5504),
		ClientMicroTxnAuthorize(5505),
		ClientMicroTxnAuthorizeResponse(5506),
		ClientAppMinutesPlayedData(5507),
		ClientGetMicroTxnInfo(5508),
		ClientGetMicroTxnInfoResponse(5509),
		ClientMarketingMessageUpdate2(5510),
		ClientDeregisterWithServer(5511),
		ClientSubscribeToPersonaFeed(5512),
		ClientLogon(5514),
		ClientReportOverlayDetourFailure(5517),
		ClientRequestEncryptedAppTicket(5526),
		ClientRequestEncryptedAppTicketResponse(5527),
		ClientWalletInfoUpdate(5528),
		DFSGetFile(5601),
		DFSInstallLocalFile(5602),
		DFSConnection(5603),
		DFSConnectionReply(5604),
		ClientDFSAuthenticateRequest(5605),
		ClientDFSAuthenticateResponse(5606),
		ClientDFSEndSession(5607),
		DFSPurgeFile(5608),
		DFSRouteFile(5609),
		DFSGetFileFromServer(5610),
		DFSAcceptedResponse(5611),
		DFSRequestPingback(5612),
		DFSRecvTransmitFile(5613),
		DFSSendTransmitFile(5614),
		DFSRequestPingback2(5615),
		DFSResponsePingback2(5616),
		ClientDFSDownloadStatus(5617),
		ClientMDSLoginRequest(5801),
		ClientMDSLoginResponse(5802),
		ClientMDSUploadManifestRequest(5803),
		ClientMDSUploadManifestResponse(5804),
		ClientMDSTransmitManifestDataChunk(5805),
		ClientMDSHeartbeat(5806),
		ClientMDSUploadDepotChunks(5807),
		ClientMDSUploadDepotChunksResponse(5808),
		ClientMDSInitDepotBuildRequest(5809),
		ClientMDSInitDepotBuildResponse(5810),
		AMToMDSGetDepotDecryptionKey(5812),
		MDSToAMGetDepotDecryptionKeyResponse(5813),
		MDSGetVersionsForDepot(5814),
		MDSGetVersionsForDepotResponse(5815),
		MDSSetPublicVersionForDepot(5816),
		MDSSetPublicVersionForDepotResponse(5817),
		ClientMDSGetDepotManifest(5818),
		ClientMDSGetDepotManifestResponse(5819),
		ClientMDSGetDepotManifestChunk(5820),
		ClientMDSDownloadDepotChunksRequest(5823),
		ClientMDSDownloadDepotChunksAsync(5824),
		ClientMDSDownloadDepotChunksAck(5825),
		MDSContentServerStatsBroadcast(5826),
		MDSContentServerConfigRequest(5827),
		MDSContentServerConfig(5828),
		MDSGetDepotManifest(5829),
		MDSGetDepotManifestResponse(5830),
		MDSGetDepotManifestChunk(5831),
		MDSGetDepotChunk(5832),
		MDSGetDepotChunkResponse(5833),
		MDSGetDepotChunkChunk(5834),
		ClientCSLoginRequest(6201),
		ClientCSLoginResponse(6202),
		GMSGameServerReplicate(6401),
		ClientMMSCreateLobby(6601),
		ClientMMSCreateLobbyResponse(6602),
		ClientMMSJoinLobby(6603),
		ClientMMSJoinLobbyResponse(6604),
		ClientMMSLeaveLobby(6605),
		ClientMMSLeaveLobbyResponse(6606),
		ClientMMSGetLobbyList(6607),
		ClientMMSGetLobbyListResponse(6608),
		ClientMMSSetLobbyData(6609),
		ClientMMSSetLobbyDataResponse(6610),
		ClientMMSGetLobbyData(6611),
		ClientMMSLobbyData(6612),
		ClientMMSSendLobbyChatMsg(6613),
		ClientMMSLobbyChatMsg(6614),
		ClientMMSSetLobbyOwner(6615),
		ClientMMSSetLobbyOwnerResponse(6616),
		ClientMMSSetLobbyGameServer(6617),
		ClientMMSLobbyGameServerSet(6618),
		ClientMMSUserJoinedLobby(6619),
		ClientMMSUserLeftLobby(6620),
		ClientMMSInviteToLobby(6621),
		ClientUDSP2PSessionStarted(7001),
		ClientUDSP2PSessionEnded(7002),
		Max(7003);

		private int code;
		private EMsg( int c ) { code = c; }
		public int getCode() {
			return code;
		}
		public static EMsg lookup( int code ) {
			for ( EMsg x : values() ) {
				if( x.getCode() == code ) return x;
			}
			return Invalid;
		}
	}
	public enum EUniverse
	{
		Invalid(0),
		Public(1),
		Beta(2),
		Internal(3),
		Dev(4),
		RC(5),
		Max(6);

		private int code;
		private EUniverse( int c ) { code = c; }
		public int getCode() {
			return code;
		}
		public static EUniverse lookup( int code ) {
			for ( EUniverse x : values() ) {
				if( x.getCode() == code ) return x;
			}
			return Invalid;
		}
	}
	public enum EResult
	{
		Invalid(0),
		OK(1),
		Fail(2),
		NoConnection(3),
		InvalidPassword(5),
		LoggedInElsewhere(6),
		InvalidProtocolVer(7),
		InvalidParam(8),
		FileNotFound(9),
		Busy(10),
		InvalidState(11),
		InvalidName(12),
		InvalidEmail(13),
		DuplicateName(14),
		AccessDenied(15),
		Timeout(16),
		Banned(17),
		AccountNotFound(18),
		InvalidSteamID(19),
		ServiceUnavailable(20),
		NotLoggedOn(21),
		Pending(22),
		EncryptionFailure(23),
		InsufficientPrivilege(24),
		LimitExceeded(25),
		Revoked(26),
		Expired(27),
		AlreadyRedeemed(28),
		DuplicateRequest(29),
		AlreadyOwned(30),
		IPNotFound(31),
		PersistFailed(32),
		LockingFailed(33),
		LogonSessionReplaced(34),
		ConnectFailed(35),
		HandshakeFailed(36),
		IOFailure(37),
		RemoteDisconnect(38),
		ShoppingCartNotFound(39),
		Blocked(40),
		Ignored(41),
		NoMatch(42),
		AccountDisabled(43),
		ServiceReadOnly(44),
		AccountNotFeatured(45),
		AdministratorOK(46),
		ContentVersion(47),
		TryAnotherCM(48),
		PasswordRequiredToKickSession(49),
		AlreadyLoggedInElsewhere(50),
		Suspended(51),
		Cancelled(52),
		DataCorruption(53),
		DiskFull(54),
		RemoteCallFailed(55),
		Max(56);

		private int code;
		private EResult( int c ) { code = c; }
		public int getCode() {
			return code;
		}
		public static EResult lookup( int code ) {
			for ( EResult x : values() ) {
				if( x.getCode() == code ) return x;
			}
			return Invalid;
		}
	}
	public enum EAccountType
	{
		Invalid(0),
		Individual(1),
		Multiseat(2),
		GameServer(3),
		AnonGameServer(4),
		Pending(5),
		ContentServer(6),
		Clan(7),
		Chat(8),
		P2PSuperSeeder(9),
		AnonUser(10),
		Max(11);

		private int code;
		private EAccountType( int c ) { code = c; }
		public int getCode() {
			return code;
		}
		public static EAccountType lookup( int code ) {
			for ( EAccountType x : values() ) {
				if( x.getCode() == code ) return x;
			}
			return Invalid;
		}
	}
	public enum EChatEntryType
	{
		Invalid(0),
		ChatMsg(1),
		Typing(2),
		InviteGame(3),
		Emote(4),
		LobbyGameStart(5),
		LeftConversation(6),
		Max(7);

		private int code;
		private EChatEntryType( int c ) { code = c; }
		public int getCode() {
			return code;
		}
		public static EChatEntryType lookup( int code ) {
			for ( EChatEntryType x : values() ) {
				if( x.getCode() == code ) return x;
			}
			return Invalid;
		}
	}
	public enum EUdpPacketType
	{
		Invalid(0),
		ChallengeReq(1),
		Challenge(2),
		Connect(3),
		Accept(4),
		Disconnect(5),
		Data(6),
		Datagram(7),
		Max(8);

		private int code;
		private EUdpPacketType( int c ) { code = c; }
		public int getCode() {
			return code;
		}
		public static EUdpPacketType lookup( int code ) {
			for ( EUdpPacketType x : values() ) {
				if( x.getCode() == code ) return x;
			}
			return Invalid;
		}
	}
	public static class UdpHeader implements ISteamSerializable
	{
		public static final int MAGIC = 0x31305356;
		// Static size: 4
		private int magic;
		// Static size: 2
		private short payloadSize;
		// Static size: 1
		private EUdpPacketType packetType;
		// Static size: 1
		private byte flags;
		// Static size: 4
		private int sourceConnID;
		// Static size: 4
		private int destConnID;
		// Static size: 4
		private int seqThis;
		// Static size: 4
		private int seqAck;
		// Static size: 4
		private int packetsInMsg;
		// Static size: 4
		private int msgStartSeq;
		// Static size: 4
		private int msgSize;

		public int getMagic() { return magic; }
		public void setMagic( int value ) { magic = value; }
		public short getPayloadSize() { return payloadSize; }
		public void setPayloadSize( short value ) { payloadSize = value; }
		public EUdpPacketType getPacketType() { return packetType; }
		public void setPacketType( EUdpPacketType value ) { packetType = value; }
		public byte getFlags() { return flags; }
		public void setFlags( byte value ) { flags = value; }
		public int getSourceConnID() { return sourceConnID; }
		public void setSourceConnID( int value ) { sourceConnID = value; }
		public int getDestConnID() { return destConnID; }
		public void setDestConnID( int value ) { destConnID = value; }
		public int getSeqThis() { return seqThis; }
		public void setSeqThis( int value ) { seqThis = value; }
		public int getSeqAck() { return seqAck; }
		public void setSeqAck( int value ) { seqAck = value; }
		public int getPacketsInMsg() { return packetsInMsg; }
		public void setPacketsInMsg( int value ) { packetsInMsg = value; }
		public int getMsgStartSeq() { return msgStartSeq; }
		public void setMsgStartSeq( int value ) { msgStartSeq = value; }
		public int getMsgSize() { return msgSize; }
		public void setMsgSize( int value ) { msgSize = value; }

		public UdpHeader()
		{
			magic = UdpHeader.MAGIC;
			payloadSize = 0;
			packetType = EUdpPacketType.Invalid;
			flags = 0;
			sourceConnID = 512;
			destConnID = 0;
			seqThis = 0;
			seqAck = 0;
			packetsInMsg = 0;
			msgStartSeq = 0;
			msgSize = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 36 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( magic );
			buffer.putShort( payloadSize );
			buffer.put( (byte)packetType.getCode() );
			buffer.put( flags );
			buffer.putInt( sourceConnID );
			buffer.putInt( destConnID );
			buffer.putInt( seqThis );
			buffer.putInt( seqAck );
			buffer.putInt( packetsInMsg );
			buffer.putInt( msgStartSeq );
			buffer.putInt( msgSize );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			magic = buffer.getInt();
			payloadSize = buffer.getShort();
			packetType = (EUdpPacketType)EUdpPacketType.lookup( buffer.get() );
			flags = buffer.get();
			sourceConnID = buffer.getInt();
			destConnID = buffer.getInt();
			seqThis = buffer.getInt();
			seqAck = buffer.getInt();
			packetsInMsg = buffer.getInt();
			msgStartSeq = buffer.getInt();
			msgSize = buffer.getInt();
		}
	}

	public static class ChallengeData implements ISteamSerializable
	{
		public static final int CHALLENGE_MASK = 0xA426DF2B;
		// Static size: 4
		private int challengeValue;
		// Static size: 4
		private int serverLoad;

		public int getChallengeValue() { return challengeValue; }
		public void setChallengeValue( int value ) { challengeValue = value; }
		public int getServerLoad() { return serverLoad; }
		public void setServerLoad( int value ) { serverLoad = value; }

		public ChallengeData()
		{
			challengeValue = 0;
			serverLoad = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 8 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( challengeValue );
			buffer.putInt( serverLoad );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			challengeValue = buffer.getInt();
			serverLoad = buffer.getInt();
		}
	}

	public static class ConnectData implements ISteamSerializable
	{
		public static final int CHALLENGE_MASK = ChallengeData.CHALLENGE_MASK;
		// Static size: 4
		private int challengeValue;

		public int getChallengeValue() { return challengeValue; }
		public void setChallengeValue( int value ) { challengeValue = value; }

		public ConnectData()
		{
			challengeValue = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 4 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( challengeValue );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			challengeValue = buffer.getInt();
		}
	}

	public static class Accept implements ISteamSerializable
	{


		public Accept()
		{
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 0 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );



			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
		}
	}

	public static class Datagram implements ISteamSerializable
	{


		public Datagram()
		{
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 0 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );



			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
		}
	}

	public static class Disconnect implements ISteamSerializable
	{


		public Disconnect()
		{
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 0 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );



			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
		}
	}

	public static class MsgHdr implements ISteamSerializableHeader
	{
		public void SetEMsg( EMsg msg ) { this.msg = msg; }

		// Static size: 4
		private EMsg msg;
		// Static size: 8
		private long targetJobID;
		// Static size: 8
		private long sourceJobID;

		public EMsg getMsg() { return msg; }
		public void setMsg( EMsg value ) { msg = value; }
		public long getTargetJobID() { return targetJobID; }
		public void setTargetJobID( long value ) { targetJobID = value; }
		public long getSourceJobID() { return sourceJobID; }
		public void setSourceJobID( long value ) { sourceJobID = value; }

		public MsgHdr()
		{
			msg = EMsg.Invalid;
			targetJobID = -1;
			sourceJobID = -1;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 20 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( (int)msg.getCode() );
			buffer.putLong( targetJobID );
			buffer.putLong( sourceJobID );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			msg = (EMsg)EMsg.lookup( buffer.getInt() );
			targetJobID = buffer.getLong();
			sourceJobID = buffer.getLong();
		}
	}

	public static class ExtendedClientMsgHdr implements ISteamSerializableHeader
	{
		public void SetEMsg( EMsg msg ) { this.msg = msg; }

		// Static size: 4
		private EMsg msg;
		// Static size: 1
		private byte headerSize;
		// Static size: 2
		private short headerVersion;
		// Static size: 8
		private long targetJobID;
		// Static size: 8
		private long sourceJobID;
		// Static size: 1
		private byte headerCanary;
		// Static size: 8
		private long steamID;
		// Static size: 4
		private int sessionID;

		public EMsg getMsg() { return msg; }
		public void setMsg( EMsg value ) { msg = value; }
		public byte getHeaderSize() { return headerSize; }
		public void setHeaderSize( byte value ) { headerSize = value; }
		public short getHeaderVersion() { return headerVersion; }
		public void setHeaderVersion( short value ) { headerVersion = value; }
		public long getTargetJobID() { return targetJobID; }
		public void setTargetJobID( long value ) { targetJobID = value; }
		public long getSourceJobID() { return sourceJobID; }
		public void setSourceJobID( long value ) { sourceJobID = value; }
		public byte getHeaderCanary() { return headerCanary; }
		public void setHeaderCanary( byte value ) { headerCanary = value; }
		public long getSteamID() { return steamID; }
		public void setSteamID( long value ) { steamID = value; }
		public int getSessionID() { return sessionID; }
		public void setSessionID( int value ) { sessionID = value; }

		public ExtendedClientMsgHdr()
		{
			msg = EMsg.Invalid;
			headerSize = (byte)36;
			headerVersion = 2;
			targetJobID = -1;
			sourceJobID = -1;
			headerCanary = (byte)239;
			steamID = 0;
			sessionID = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 36 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( (int)msg.getCode() );
			buffer.put( headerSize );
			buffer.putShort( headerVersion );
			buffer.putLong( targetJobID );
			buffer.putLong( sourceJobID );
			buffer.put( headerCanary );
			buffer.putLong( steamID );
			buffer.putInt( sessionID );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			msg = (EMsg)EMsg.lookup( buffer.getInt() );
			headerSize = buffer.get();
			headerVersion = buffer.getShort();
			targetJobID = buffer.getLong();
			sourceJobID = buffer.getLong();
			headerCanary = buffer.get();
			steamID = buffer.getLong();
			sessionID = buffer.getInt();
		}
	}

	public static class MsgHdrProtoBuf implements ISteamSerializableHeader
	{
		public void SetEMsg( EMsg msg ) { this.msg = msg; }

		// Static size: 4
		private EMsg msg;
		// Static size: 4
		private int headerLength;
		// Static size: 0
		private CMsgProtoBufHeader protoHeader;

		public EMsg getMsg() { return msg; }
		public void setMsg( EMsg value ) { msg = value; }
		public int getHeaderLength() { return headerLength; }
		public void setHeaderLength( int value ) { headerLength = value; }
		public CMsgProtoBufHeader getProtoHeader() { return protoHeader; }
		public void setProtoHeader( CMsgProtoBufHeader value ) { protoHeader = value; }

		public MsgHdrProtoBuf()
		{
			msg = EMsg.Invalid;
			headerLength = 0;
			protoHeader = CMsgProtoBufHeader.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProtoHeader = protoHeader.toByteString().asReadOnlyByteBuffer();
			headerLength = bufProtoHeader.limit();
			ByteBuffer buffer = ByteBuffer.allocate( 8 + bufProtoHeader.limit() );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( (int)MsgUtil.MakeMsg( msg.getCode(), true ) );
			buffer.putInt( headerLength );
			buffer.put( bufProtoHeader );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			msg = (EMsg)MsgUtil.GetMsg( EMsg.lookup( buffer.getInt() ) );
			headerLength = buffer.getInt();
			byte[] bufProtoHeader = new byte[ headerLength ];
			buffer.get( bufProtoHeader );
			protoHeader = CMsgProtoBufHeader.parseFrom( bufProtoHeader );
		}
	}

	public static class MsgChannelEncryptRequest implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ChannelEncryptRequest; }

		public static final int PROTOCOL_VERSION = 1;
		// Static size: 4
		private int protocolVersion;
		// Static size: 4
		private EUniverse universe;

		public int getProtocolVersion() { return protocolVersion; }
		public void setProtocolVersion( int value ) { protocolVersion = value; }
		public EUniverse getUniverse() { return universe; }
		public void setUniverse( EUniverse value ) { universe = value; }

		public MsgChannelEncryptRequest()
		{
			protocolVersion = MsgChannelEncryptRequest.PROTOCOL_VERSION;
			universe = EUniverse.Invalid;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 8 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( protocolVersion );
			buffer.putInt( (int)universe.getCode() );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			protocolVersion = buffer.getInt();
			universe = (EUniverse)EUniverse.lookup( buffer.getInt() );
		}
	}

	public static class MsgChannelEncryptResponse implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ChannelEncryptResponse; }

		// Static size: 4
		private int protocolVersion;
		// Static size: 4
		private int keySize;

		public int getProtocolVersion() { return protocolVersion; }
		public void setProtocolVersion( int value ) { protocolVersion = value; }
		public int getKeySize() { return keySize; }
		public void setKeySize( int value ) { keySize = value; }

		public MsgChannelEncryptResponse()
		{
			protocolVersion = MsgChannelEncryptRequest.PROTOCOL_VERSION;
			keySize = 128;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 8 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( protocolVersion );
			buffer.putInt( keySize );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			protocolVersion = buffer.getInt();
			keySize = buffer.getInt();
		}
	}

	public static class MsgChannelEncryptResult implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ChannelEncryptResult; }

		// Static size: 4
		private EResult result;

		public EResult getResult() { return result; }
		public void setResult( EResult value ) { result = value; }

		public MsgChannelEncryptResult()
		{
			result = EResult.Invalid;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 4 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( (int)result.getCode() );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			result = (EResult)EResult.lookup( buffer.getInt() );
		}
	}

	public static class MsgMulti implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.Multi; }

		// Static size: 0
		private CMsgMulti proto;

		public CMsgMulti getProto() { return proto; }
		public void setProto( CMsgMulti value ) { proto = value; }

		public MsgMulti()
		{
			proto = CMsgMulti.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgMulti.parseFrom( bufProto );
		}
	}

	public static class MsgClientNewLoginKey implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientNewLoginKey; }

		// Static size: 4
		private int uniqueID;
		// Static size: 20
		private byte[] loginKey;

		public int getUniqueID() { return uniqueID; }
		public void setUniqueID( int value ) { uniqueID = value; }
		public byte[] getLoginKey() { return loginKey; }
		public void setLoginKey( byte[] value ) { loginKey = value; }

		public MsgClientNewLoginKey()
		{
			uniqueID = 0;
			loginKey = new byte[20];
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 24 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( uniqueID );
			buffer.put( loginKey );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			uniqueID = buffer.getInt();
			buffer.get( loginKey );
		}
	}

	public static class MsgClientNewLoginKeyAccepted implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientNewLoginKeyAccepted; }

		// Static size: 4
		private int uniqueID;

		public int getUniqueID() { return uniqueID; }
		public void setUniqueID( int value ) { uniqueID = value; }

		public MsgClientNewLoginKeyAccepted()
		{
			uniqueID = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 4 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putInt( uniqueID );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			uniqueID = buffer.getInt();
		}
	}

	public static class MsgClientHeartBeat implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientHeartBeat; }

		// Static size: 0
		private CMsgClientHeartBeat proto;

		public CMsgClientHeartBeat getProto() { return proto; }
		public void setProto( CMsgClientHeartBeat value ) { proto = value; }

		public MsgClientHeartBeat()
		{
			proto = CMsgClientHeartBeat.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientHeartBeat.parseFrom( bufProto );
		}
	}

	public static class MsgClientLogon implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogon; }

		public static final int ObfuscationMask = 0xBAADF00D;
		public static final int CurrentProtocol = 65565;
		// Static size: 0
		private CMsgClientLogon proto;

		public CMsgClientLogon getProto() { return proto; }
		public void setProto( CMsgClientLogon value ) { proto = value; }

		public MsgClientLogon()
		{
			proto = CMsgClientLogon.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientLogon.parseFrom( bufProto );
		}
	}

	public static class MsgClientLogonResponse implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientLogOnResponse; }

		// Static size: 0
		private CMsgClientLogonResponse proto;

		public CMsgClientLogonResponse getProto() { return proto; }
		public void setProto( CMsgClientLogonResponse value ) { proto = value; }

		public MsgClientLogonResponse()
		{
			proto = CMsgClientLogonResponse.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientLogonResponse.parseFrom( bufProto );
		}
	}

	public static class MsgGSServerType implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSServerType; }

		// Static size: 0
		private CMsgGSServerType proto;

		public CMsgGSServerType getProto() { return proto; }
		public void setProto( CMsgGSServerType value ) { proto = value; }

		public MsgGSServerType()
		{
			proto = CMsgGSServerType.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgGSServerType.parseFrom( bufProto );
		}
	}

	public static class MsgGSStatusReply implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.GSStatusReply; }

		// Static size: 0
		private CMsgGSStatusReply proto;

		public CMsgGSStatusReply getProto() { return proto; }
		public void setProto( CMsgGSStatusReply value ) { proto = value; }

		public MsgGSStatusReply()
		{
			proto = CMsgGSStatusReply.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgGSStatusReply.parseFrom( bufProto );
		}
	}

	public static class MsgClientRegisterAuthTicketWithCM implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRegisterAuthTicketWithCM; }

		// Static size: 0
		private CMsgClientRegisterAuthTicketWithCM proto;

		public CMsgClientRegisterAuthTicketWithCM getProto() { return proto; }
		public void setProto( CMsgClientRegisterAuthTicketWithCM value ) { proto = value; }

		public MsgClientRegisterAuthTicketWithCM()
		{
			proto = CMsgClientRegisterAuthTicketWithCM.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientRegisterAuthTicketWithCM.parseFrom( bufProto );
		}
	}

	public static class MsgClientGetAppOwnershipTicket implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetAppOwnershipTicket; }

		// Static size: 0
		private CMsgClientGetAppOwnershipTicket proto;

		public CMsgClientGetAppOwnershipTicket getProto() { return proto; }
		public void setProto( CMsgClientGetAppOwnershipTicket value ) { proto = value; }

		public MsgClientGetAppOwnershipTicket()
		{
			proto = CMsgClientGetAppOwnershipTicket.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientGetAppOwnershipTicket.parseFrom( bufProto );
		}
	}

	public static class MsgClientGetAppOwnershipTicketResponse implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientGetAppOwnershipTicketResponse; }

		// Static size: 0
		private CMsgClientGetAppOwnershipTicketResponse proto;

		public CMsgClientGetAppOwnershipTicketResponse getProto() { return proto; }
		public void setProto( CMsgClientGetAppOwnershipTicketResponse value ) { proto = value; }

		public MsgClientGetAppOwnershipTicketResponse()
		{
			proto = CMsgClientGetAppOwnershipTicketResponse.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientGetAppOwnershipTicketResponse.parseFrom( bufProto );
		}
	}

	public static class MsgClientAuthList implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientAuthList; }

		// Static size: 0
		private CMsgClientAuthList proto;

		public CMsgClientAuthList getProto() { return proto; }
		public void setProto( CMsgClientAuthList value ) { proto = value; }

		public MsgClientAuthList()
		{
			proto = CMsgClientAuthList.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientAuthList.parseFrom( bufProto );
		}
	}

	public static class MsgClientRequestFriendData implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientRequestFriendData; }

		// Static size: 0
		private CMsgClientRequestFriendData proto;

		public CMsgClientRequestFriendData getProto() { return proto; }
		public void setProto( CMsgClientRequestFriendData value ) { proto = value; }

		public MsgClientRequestFriendData()
		{
			proto = CMsgClientRequestFriendData.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientRequestFriendData.parseFrom( bufProto );
		}
	}

	public static class MsgClientChangeStatus implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientChangeStatus; }

		// Static size: 1
		private byte personaState;

		public byte getPersonaState() { return personaState; }
		public void setPersonaState( byte value ) { personaState = value; }

		public MsgClientChangeStatus()
		{
			personaState = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 1 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.put( personaState );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			personaState = buffer.get();
		}
	}

	public static class MsgClientPersonaState implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientPersonaState; }

		// Static size: 0
		private CMsgClientPersonaState proto;

		public CMsgClientPersonaState getProto() { return proto; }
		public void setProto( CMsgClientPersonaState value ) { proto = value; }

		public MsgClientPersonaState()
		{
			proto = CMsgClientPersonaState.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientPersonaState.parseFrom( bufProto );
		}
	}

	public static class MsgClientSessionToken implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientSessionToken; }

		// Static size: 0
		private CMsgClientSessionToken proto;

		public CMsgClientSessionToken getProto() { return proto; }
		public void setProto( CMsgClientSessionToken value ) { proto = value; }

		public MsgClientSessionToken()
		{
			proto = CMsgClientSessionToken.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientSessionToken.parseFrom( bufProto );
		}
	}

	public static class MsgClientFriendsList implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendsList; }

		// Static size: 0
		private CMsgClientFriendsList proto;

		public CMsgClientFriendsList getProto() { return proto; }
		public void setProto( CMsgClientFriendsList value ) { proto = value; }

		public MsgClientFriendsList()
		{
			proto = CMsgClientFriendsList.getDefaultInstance();
		}

		public ByteBuffer serialize()
		{
			ByteBuffer bufProto = proto.toByteString().asReadOnlyByteBuffer();
			return bufProto;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			byte[] bufProto = new byte[ buffer.limit() - buffer.position() ];
			buffer.get( bufProto );
			proto = CMsgClientFriendsList.parseFrom( bufProto );
		}
	}

	public static class MsgClientFriendMsgIncoming implements ISteamSerializableMessage
	{
		public EMsg GetEMsg() { return EMsg.ClientFriendMsgIncoming; }

		// Static size: 8
		private long steamID;
		// Static size: 4
		private EChatEntryType entryType;
		// Static size: 4
		private int messageSize;

		public long getSteamID() { return steamID; }
		public void setSteamID( long value ) { steamID = value; }
		public EChatEntryType getEntryType() { return entryType; }
		public void setEntryType( EChatEntryType value ) { entryType = value; }
		public int getMessageSize() { return messageSize; }
		public void setMessageSize( int value ) { messageSize = value; }

		public MsgClientFriendMsgIncoming()
		{
			steamID = 0;
			entryType = EChatEntryType.Invalid;
			messageSize = 0;
		}

		public ByteBuffer serialize()
		{
			ByteBuffer buffer = ByteBuffer.allocate( 16 );
			buffer.order( ByteOrder.LITTLE_ENDIAN );

			buffer.putLong( steamID );
			buffer.putInt( (int)entryType.getCode() );
			buffer.putInt( messageSize );


			buffer.flip();
			return buffer;
		}

		public void deserialize( ByteBuffer buffer ) throws InvalidProtocolBufferException
		{
			steamID = buffer.getLong();
			entryType = (EChatEntryType)EChatEntryType.lookup( buffer.getInt() );
			messageSize = buffer.getInt();
		}
	}

}
