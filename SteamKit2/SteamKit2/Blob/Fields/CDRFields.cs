using System;

namespace SteamKit2
{
    public static class CDRFields
    {
        public const string eFieldVersionNum = "0"; // U16
        public const string eFieldApplicationsRecord = "1"; // CApplicationsRecord
        public const string eFieldSubscriptionsRecord = "2"; // CSubscriptionsRecord
        public const string eFieldLastChangedExistingAppOrSubscriptionTime = "3"; // U64
        public const string eFieldIndexAppIdToSubscriptionIdsRecord = "4"; // CIndexAppIdToSubscriptionIdsRecord
        public const string eFieldAllAppsPublicKeysRecord = "5"; // CAllAppsPublicKeysRecord
        public const string eFieldAllAppsEncryptedPrivateKeysRecord = "6"; // CAllAppsEncryptedPrivateKeysRecord
    }

    public static class CDRAppRecordFields
    {

        public const string eFieldAppId = "1";  // U32
        public const string eFieldName = "2";  // String
        public const string eFieldInstallDirName = "3";  // String
        public const string eFieldMinCacheFileSizeMB = "4";  // U32
        public const string eFieldMaxCacheFileSizeMB = "5";  // U32
        public const string eFieldLaunchOptionsRecord = "6";  // CApplicationLaunchOptionsRecord
        public const string eFieldIconsRecord = "7";  // CApplicationIconsRecord
        public const string eFieldOnFirstLaunch = "8";  // I32
        public const string eFieldIsBandwidthGreedy = "9";  // Boolean (U8)
        public const string eFieldVersionsRecord = "10"; // CApplicationVersionsRecord
        public const string eFieldCurrentVersionId = "11"; // U32
        public const string eFieldFilesystemsRecord = "12"; // CApplicationFilesystemsRecord
        public const string eFieldTrickleVersionId = "13"; // I32
        public const string eFieldUserDefinedRecord = "14"; // CApplicationUserDefinedRecord
        public const string eFieldBetaVersionPassword = "15"; // String
        public const string eFieldBetaVersionId = "16"; // I32
        public const string eFieldLegacyInstallDirName = "17"; // String
        public const string eFieldSkipMFPOverwrite = "18"; // Boolean (U8)
        public const string eFieldUseFilesystemDvr = "19"; // Boolean (U8)
        public const string eFieldManifestOnlyApp = "20"; // Boolean (U8)
        public const string eFieldAppOfManifestOnlyCache = "21"; // U32
    }
    public static class CDRAppVersionFields
    {
        public const string eFieldDescription = "1"; // String
        public const string eFieldVersionId = "2"; // U32
        public const string eFieldIsNotAvailable = "3"; // Boolean (U8)
        public const string eFieldLaunchOptionIdsRecord = "4"; // CApplicationLaunchOptionIdsRecord
        public const string eFieldDepotEncryptionKey = "5"; // String
        public const string eFieldIsEncryptionKeyAvailable = "6"; // Boolean (U8)
        public const string eFieldIsRebased = "7"; // Boolean (U8)
        public const string eFieldIsLongVersionRoll = "8"; // Boolean (U8)
    }


    public static class CDRAppLaunchOptionFields
    {
        public const string eFieldDescription = "1"; // String
        public const string eFieldCommandLine = "2"; // String
        public const string eFieldIconIndex = "3"; // I32
        public const string eFieldNoDesktopShortcut = "4"; // Boolean (U8)
        public const string eFieldNoStartMenuShortcut = "5"; // Boolean (U8)
        public const string eFieldLongRunningUnattended = "6"; // Boolean (U8)
        public const string eFieldPlatform = "7"; // String
    }

    public static class CDRAppFilesystemFields
    {
        public const string eFieldAppId = "1"; // U32
        public const string eFieldMountName = "2"; // String
        public const string eFieldIsOptional = "3"; // Boolean (U8)
        public const string eFieldPlatform = "4"; // String
    }


    public static class CDRSubRecordFields
    {
        public const string eFieldSubId = "1";  // U32
        public const string eFieldName = "2";  // String
        public const string eFieldBillingType = "3";  // ESubscriptionBillingType (U16)
        public const string eFieldCostInCents = "4";  // U32
        public const string eFieldPeriodInMinutes = "5";  // I32
        public const string eFieldAppIdsRecord = "6";  // CSubscriptionAppIdsRecord
        public const string eFieldOnSubscribeRunAppId = "7";  // I32
        public const string eFieldOnSubscribeRunLaunchOptionIndex = "8";  // I32
        public const string eFieldOptionalRateLimitRecord = "9";  // CRateLimitRecord
        public const string eFieldDiscountsRecord = "10"; // CSubscriptionDiscountsRecord
        public const string eFieldIsPreorder = "11"; // Boolean (U8)
        public const string eFieldRequiresShippingAddress = "12"; // Boolean (U8)
        public const string eFieldDomesticCostInCents = "13"; // U32
        public const string eFieldInternationalCostInCents = "14"; // U32
        public const string eFieldRequiredKeyType = "15"; // U32
        public const string eFieldIsCyberCafe = "16"; // Boolean (U8)
        public const string eFieldGameCode = "17"; // I32
        public const string eFieldGameCodeDescription = "18"; // String
        public const string eFieldIsDisabled = "19"; // Boolean (U8)
        public const string eFieldRequiresCD = "20"; // Boolean (U8)
        public const string eFieldTerritoryCode = "21"; // U32
        public const string eFieldIsSteam3Subscription = "22"; // Boolean (U8)
        public const string eFieldExtendedInfoRecord = "23"; // CSubscriptionExtendedInfoRecord
    }

    public static class CDRSubRateLimitFields
    {
        public const string eFieldLimit = "1"; // U32
        public const string eFieldPeriodInMinutes = "2"; // U32
    }


    public static class CDRSubDiscountFields
    {
        public const string eFieldName = "1"; // String
        public const string eFieldDiscountInCents = "2"; // U32
        public const string eFieldDiscountQualifiersRecord = "3"; // CSubscriptionDiscountQualifiersRecord
    }

    public static class CDRSubDiscountQualifierFields
    {
        public const string eFieldName = "1"; // String
        public const string eFieldSubscriptionRequired = "2"; // U32
        public const string eFieldIsDisqualifier = "3"; // Boolean (U8)
    }

}
