/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;

namespace SteamKit2
{
    public static class CDRFields
    {
        public const int eFieldVersionNum = 0; // U16
        public const int eFieldApplicationsRecord = 1; // CApplicationsRecord
        public const int eFieldSubscriptionsRecord = 2; // CSubscriptionsRecord
        public const int eFieldLastChangedExistingAppOrSubscriptionTime = 3; // U64
        public const int eFieldIndexAppIdToSubscriptionIdsRecord = 4; // CIndexAppIdToSubscriptionIdsRecord
        public const int eFieldAllAppsPublicKeysRecord = 5; // CAllAppsPublicKeysRecord
        public const int eFieldAllAppsEncryptedPrivateKeysRecord = 6; // CAllAppsEncryptedPrivateKeysRecord
    }

    public static class CDRAppRecordFields
    {

        public const int eFieldAppId = 1;  // U32
        public const int eFieldName = 2;  // String
        public const int eFieldInstallDirName = 3;  // String
        public const int eFieldMinCacheFileSizeMB = 4;  // U32
        public const int eFieldMaxCacheFileSizeMB = 5;  // U32
        public const int eFieldLaunchOptionsRecord = 6;  // CApplicationLaunchOptionsRecord
        public const int eFieldIconsRecord = 7;  // CApplicationIconsRecord
        public const int eFieldOnFirstLaunch = 8;  // I32
        public const int eFieldIsBandwidthGreedy = 9;  // Boolean (U8)
        public const int eFieldVersionsRecord = 10; // CApplicationVersionsRecord
        public const int eFieldCurrentVersionId = 11; // U32
        public const int eFieldFilesystemsRecord = 12; // CApplicationFilesystemsRecord
        public const int eFieldTrickleVersionId = 13; // I32
        public const int eFieldUserDefinedRecord = 14; // CApplicationUserDefinedRecord
        public const int eFieldBetaVersionPassword = 15; // String
        public const int eFieldBetaVersionId = 16; // I32
        public const int eFieldLegacyInstallDirName = 17; // String
        public const int eFieldSkipMFPOverwrite = 18; // Boolean (U8)
        public const int eFieldUseFilesystemDvr = 19; // Boolean (U8)
        public const int eFieldManifestOnlyApp = 20; // Boolean (U8)
        public const int eFieldAppOfManifestOnlyCache = 21; // U32
    }


    public static class CDRSubRecordFields
    {
        public const int eFieldSubId = 1;  // U32
        public const int eFieldName = 2;  // String
        public const int eFieldBillingType = 3;  // ESubscriptionBillingType (U16)
        public const int eFieldCostInCents = 4;  // U32
        public const int eFieldPeriodInMinutes = 5;  // I32
        public const int eFieldAppIdsRecord = 6;  // CSubscriptionAppIdsRecord
        public const int eFieldOnSubscribeRunAppId = 7;  // I32
        public const int eFieldOnSubscribeRunLaunchOptionIndex = 8;  // I32
        public const int eFieldOptionalRateLimitRecord = 9;  // CRateLimitRecord
        public const int eFieldDiscountsRecord = 10; // CSubscriptionDiscountsRecord
        public const int eFieldIsPreorder = 11; // Boolean (U8)
        public const int eFieldRequiresShippingAddress = 12; // Boolean (U8)
        public const int eFieldDomesticCostInCents = 13; // U32
        public const int eFieldInternationalCostInCents = 14; // U32
        public const int eFieldRequiredKeyType = 15; // U32
        public const int eFieldIsCyberCafe = 16; // Boolean (U8)
        public const int eFieldGameCode = 17; // I32
        public const int eFieldGameCodeDescription = 18; // String
        public const int eFieldIsDisabled = 19; // Boolean (U8)
        public const int eFieldRequiresCD = 20; // Boolean (U8)
        public const int eFieldTerritoryCode = 21; // U32
        public const int eFieldIsSteam3Subscription = 22; // Boolean (U8)
        public const int eFieldExtendedInfoRecord = 23; // CSubscriptionExtendedInfoRecord
    }
}
