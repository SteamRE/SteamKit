using System;

namespace BlobLib
{
    public class CDRFields
    {
        public enum EFieldType : int
        {
            eFieldAppId = 1,  // U32
            eFieldName = 2,  // String
            eFieldInstallDirName = 3,  // String
            eFieldMinCacheFileSizeMB = 4,  // U32
            eFieldMaxCacheFileSizeMB = 5,  // U32
            eFieldLaunchOptionsRecord = 6,  // CApplicationLaunchOptionsRecord
            eFieldIconsRecord = 7,  // CApplicationIconsRecord
            eFieldOnFirstLaunch = 8,  // I32
            eFieldIsBandwidthGreedy = 9,  // Boolean (U8)
            eFieldVersionsRecord = 10, // CApplicationVersionsRecord
            eFieldCurrentVersionId = 11, // U32
            eFieldFilesystemsRecord = 12, // CApplicationFilesystemsRecord
            eFieldTrickleVersionId = 13, // I32
            eFieldUserDefinedRecord = 14, // CApplicationUserDefinedRecord
            eFieldBetaVersionPassword = 15, // String
            eFieldBetaVersionId = 16, // I32
            eFieldLegacyInstallDirName = 17, // String
            eFieldSkipMFPOverwrite = 18, // Boolean (U8)
            eFieldUseFilesystemDvr = 19, // Boolean (U8)
            eFieldManifestOnlyApp = 20, // Boolean (U8)
            eFieldAppOfManifestOnlyCache = 21, // U32
        };
    }
}
