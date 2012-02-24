using System;

namespace SteamKit2.Blob
{
    /// <summary>
    /// Represents the root fields available in the CDR blob.
    /// </summary>
    public static class CDRFields
    {
        /// <summary>
        /// The version number field.
        /// </summary>
        public const string eFieldVersionNum = "0"; // U16
        /// <summary>
        /// The applications record field.
        /// </summary>
        public const string eFieldApplicationsRecord = "1"; // CApplicationsRecord
        /// <summary>
        /// The subscriptions record field.
        /// </summary>
        public const string eFieldSubscriptionsRecord = "2"; // CSubscriptionsRecord
        /// <summary>
        /// The last changed time field.
        /// </summary>
        public const string eFieldLastChangedExistingAppOrSubscriptionTime = "3"; // U64
        /// <summary>
        /// The AppID to SubID record field.
        /// </summary>
        public const string eFieldIndexAppIdToSubscriptionIdsRecord = "4"; // CIndexAppIdToSubscriptionIdsRecord
        /// <summary>
        /// The public keys record field.
        /// </summary>
        public const string eFieldAllAppsPublicKeysRecord = "5"; // CAllAppsPublicKeysRecord
        /// <summary>
        /// The encrypted private keys record field.
        /// </summary>
        public const string eFieldAllAppsEncryptedPrivateKeysRecord = "6"; // CAllAppsEncryptedPrivateKeysRecord
    }

    /// <summary>
    /// Represents the root fields available in the applications record.
    /// </summary>
    public static class CDRAppRecordFields
    {
        /// <summary>
        /// The AppID field.
        /// </summary>
        public const string eFieldAppId = "1";  // U32
        /// <summary>
        /// The name field.
        /// </summary>
        public const string eFieldName = "2";  // String
        /// <summary>
        /// The install directory field.
        /// </summary>
        public const string eFieldInstallDirName = "3";  // String
        /// <summary>
        /// The minimum cache size field.
        /// </summary>
        public const string eFieldMinCacheFileSizeMB = "4";  // U32
        /// <summary>
        /// The maximum cache size field.
        /// </summary>
        public const string eFieldMaxCacheFileSizeMB = "5";  // U32
        /// <summary>
        /// The launch options record field.
        /// </summary>
        public const string eFieldLaunchOptionsRecord = "6";  // CApplicationLaunchOptionsRecord
        /// <summary>
        /// The icons record field.
        /// </summary>
        public const string eFieldIconsRecord = "7";  // CApplicationIconsRecord
        /// <summary>
        /// The "on first launch" field.
        /// </summary>
        public const string eFieldOnFirstLaunch = "8";  // I32
        /// <summary>
        /// The "is bandwidth greedy" field.
        /// </summary>
        public const string eFieldIsBandwidthGreedy = "9";  // Boolean (U8)
        /// <summary>
        /// The versions record field.
        /// </summary>
        public const string eFieldVersionsRecord = "10"; // CApplicationVersionsRecord
        /// <summary>
        /// The current version id field.
        /// </summary>
        public const string eFieldCurrentVersionId = "11"; // U32
        /// <summary>
        /// The filesystems record field.
        /// </summary>
        public const string eFieldFilesystemsRecord = "12"; // CApplicationFilesystemsRecord
        /// <summary>
        /// The trickle version field.
        /// </summary>
        public const string eFieldTrickleVersionId = "13"; // I32
        /// <summary>
        /// The user defined record field.
        /// </summary>
        public const string eFieldUserDefinedRecord = "14"; // CApplicationUserDefinedRecord
        /// <summary>
        /// The beta version password field.
        /// </summary>
        public const string eFieldBetaVersionPassword = "15"; // String
        /// <summary>
        /// The beta version id field.
        /// </summary>
        public const string eFieldBetaVersionId = "16"; // I32
        /// <summary>
        /// The legacy install directory field.
        /// </summary>
        public const string eFieldLegacyInstallDirName = "17"; // String
        /// <summary>
        /// The "skip mfp overwrite" field.
        /// </summary>
        public const string eFieldSkipMFPOverwrite = "18"; // Boolean (U8)
        /// <summary>
        /// The "use filesystem dvr" field.
        /// </summary>
        public const string eFieldUseFilesystemDvr = "19"; // Boolean (U8)
        /// <summary>
        /// The "manifest only app" field.
        /// </summary>
        public const string eFieldManifestOnlyApp = "20"; // Boolean (U8)
        /// <summary>
        /// The "AppID of manifest only app" field.
        /// </summary>
        public const string eFieldAppOfManifestOnlyCache = "21"; // U32
    }

    /// <summary>
    /// Represents the root fields available in the application version record.
    /// </summary>
    public static class CDRAppVersionFields
    {
        /// <summary>
        /// The description field.
        /// </summary>
        public const string eFieldDescription = "1"; // String
        /// <summary>
        /// The version id field.
        /// </summary>
        public const string eFieldVersionId = "2"; // U32
        /// <summary>
        /// The "is not available" field.
        /// </summary>
        public const string eFieldIsNotAvailable = "3"; // Boolean (U8)
        /// <summary>
        /// The launch option ids record field.
        /// </summary>
        public const string eFieldLaunchOptionIdsRecord = "4"; // CApplicationLaunchOptionIdsRecord
        /// <summary>
        /// The depot encryption key field.
        /// </summary>
        public const string eFieldDepotEncryptionKey = "5"; // String
        /// <summary>
        /// The "is encryption key available" field.
        /// </summary>
        public const string eFieldIsEncryptionKeyAvailable = "6"; // Boolean (U8)
        /// <summary>
        /// The "is rebased" field.
        /// </summary>
        public const string eFieldIsRebased = "7"; // Boolean (U8)
        /// <summary>
        /// The "is long version roll" field.
        /// </summary>
        public const string eFieldIsLongVersionRoll = "8"; // Boolean (U8)
    }

    /// <summary>
    /// Represents the root fields available in the launch option record.
    /// </summary>
    public static class CDRAppLaunchOptionFields
    {
        /// <summary>
        /// The description field.
        /// </summary>
        public const string eFieldDescription = "1"; // String
        /// <summary>
        /// The command line field.
        /// </summary>
        public const string eFieldCommandLine = "2"; // String
        /// <summary>
        /// The icon index field.
        /// </summary>
        public const string eFieldIconIndex = "3"; // I32
        /// <summary>
        /// The "no desktop shortcut" field.
        /// </summary>
        public const string eFieldNoDesktopShortcut = "4"; // Boolean (U8)
        /// <summary>
        /// The "no start menu shortcut" field.
        /// </summary>
        public const string eFieldNoStartMenuShortcut = "5"; // Boolean (U8)
        /// <summary>
        /// The "long running unattended" field.
        /// </summary>
        public const string eFieldLongRunningUnattended = "6"; // Boolean (U8)
        /// <summary>
        /// The platform field.
        /// </summary>
        public const string eFieldPlatform = "7"; // String
    }

    /// <summary>
    /// Represents the root fields available in the filesystems record.
    /// </summary>
    public static class CDRAppFilesystemFields
    {
        /// <summary>
        /// The AppID field.
        /// </summary>
        public const string eFieldAppId = "1"; // U32
        /// <summary>
        /// The mount name field.
        /// </summary>
        public const string eFieldMountName = "2"; // String
        /// <summary>
        /// The "is optional" field.
        /// </summary>
        public const string eFieldIsOptional = "3"; // Boolean (U8)
        /// <summary>
        /// The platform field.
        /// </summary>
        public const string eFieldPlatform = "4"; // String
    }

    /// <summary>
    /// Represents the root fields available in the subscription record.
    /// </summary>
    public static class CDRSubRecordFields
    {
        /// <summary>
        /// The SubID field.
        /// </summary>
        public const string eFieldSubId = "1";  // U32
        /// <summary>
        /// The name field.
        /// </summary>
        public const string eFieldName = "2";  // String
        /// <summary>
        /// The billing type field.
        /// </summary>
        public const string eFieldBillingType = "3";  // ESubscriptionBillingType (U16)
        /// <summary>
        /// The cost in cents field.
        /// </summary>
        public const string eFieldCostInCents = "4";  // U32
        /// <summary>
        /// The period in minutes field.
        /// </summary>
        public const string eFieldPeriodInMinutes = "5";  // I32
        /// <summary>
        /// The AppIDs record field.
        /// </summary>
        public const string eFieldAppIdsRecord = "6";  // CSubscriptionAppIdsRecord
        /// <summary>
        /// The "on subscribe run appid" field.
        /// </summary>
        public const string eFieldOnSubscribeRunAppId = "7";  // I32
        /// <summary>
        /// The "on subscribe run launch option index" field.
        /// </summary>
        public const string eFieldOnSubscribeRunLaunchOptionIndex = "8";  // I32
        /// <summary>
        /// The optional rate limit record field.
        /// </summary>
        public const string eFieldOptionalRateLimitRecord = "9";  // CRateLimitRecord
        /// <summary>
        /// The discounts record field.
        /// </summary>
        public const string eFieldDiscountsRecord = "10"; // CSubscriptionDiscountsRecord
        /// <summary>
        /// The "is preorder" field.
        /// </summary>
        public const string eFieldIsPreorder = "11"; // Boolean (U8)
        /// <summary>
        /// The "requires shipping address" field.
        /// </summary>
        public const string eFieldRequiresShippingAddress = "12"; // Boolean (U8)
        /// <summary>
        /// The domestic cost in cents field.
        /// </summary>
        public const string eFieldDomesticCostInCents = "13"; // U32
        /// <summary>
        /// The international cost in cents field.
        /// </summary>
        public const string eFieldInternationalCostInCents = "14"; // U32
        /// <summary>
        /// The required key type field.
        /// </summary>
        public const string eFieldRequiredKeyType = "15"; // U32
        /// <summary>
        /// The "is cyber cafe" field.
        /// </summary>
        public const string eFieldIsCyberCafe = "16"; // Boolean (U8)
        /// <summary>
        /// The game code field.
        /// </summary>
        public const string eFieldGameCode = "17"; // I32
        /// <summary>
        /// The game code description field.
        /// </summary>
        public const string eFieldGameCodeDescription = "18"; // String
        /// <summary>
        /// The "is disabled" field.
        /// </summary>
        public const string eFieldIsDisabled = "19"; // Boolean (U8)
        /// <summary>
        /// The "requires CD" field.
        /// </summary>
        public const string eFieldRequiresCD = "20"; // Boolean (U8)
        /// <summary>
        /// The territory code field.
        /// </summary>
        public const string eFieldTerritoryCode = "21"; // U32
        /// <summary>
        /// The "is steam3 subscription" field.
        /// </summary>
        public const string eFieldIsSteam3Subscription = "22"; // Boolean (U8)
        /// <summary>
        /// The extended info record field.
        /// </summary>
        public const string eFieldExtendedInfoRecord = "23"; // CSubscriptionExtendedInfoRecord
    }

    /// <summary>
    /// Represents the root fields available in the subscription rate limit record.
    /// </summary>
    public static class CDRSubRateLimitFields
    {
        /// <summary>
        /// The limit field.
        /// </summary>
        public const string eFieldLimit = "1"; // U32
        /// <summary>
        /// The period in minutes field.
        /// </summary>
        public const string eFieldPeriodInMinutes = "2"; // U32
    }

    /// <summary>
    /// Represents the root fields available in the subscription discount record.
    /// </summary>
    public static class CDRSubDiscountFields
    {
        /// <summary>
        /// The name field.
        /// </summary>
        public const string eFieldName = "1"; // String
        /// <summary>
        /// The discount in cents field.
        /// </summary>
        public const string eFieldDiscountInCents = "2"; // U32
        /// <summary>
        /// The discount qualifiers record field.
        /// </summary>
        public const string eFieldDiscountQualifiersRecord = "3"; // CSubscriptionDiscountQualifiersRecord
    }

    /// <summary>
    /// Represents the root fields available in the subscription discount qualifier record.
    /// </summary>
    public static class CDRSubDiscountQualifierFields
    {
        /// <summary>
        /// The name field.
        /// </summary>
        public const string eFieldName = "1"; // String
        /// <summary>
        /// The "subscription required" field.
        /// </summary>
        public const string eFieldSubscriptionRequired = "2"; // U32
        /// <summary>
        /// The "is disqualifier" field.
        /// </summary>
        public const string eFieldIsDisqualifier = "3"; // Boolean (U8)
    }

}
