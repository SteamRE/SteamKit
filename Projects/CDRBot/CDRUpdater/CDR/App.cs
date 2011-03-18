using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SqlNet;

namespace CDRUpdater
{
    [SqlTable( "AppLaunchOptionList" )]
    class AppLaunchOption : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string RelHash { get; set; }

        [SqlRelation( Column = "AppID" )]
        [SqlColumn( IsNotNull = true )]
        public uint RelAppID { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppLaunchOptionFields.eFieldDescription )]
        public string Description { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppLaunchOptionFields.eFieldCommandLine )]
        public string CommandLine { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppLaunchOptionFields.eFieldIconIndex )]
        public int IconIndex { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppLaunchOptionFields.eFieldNoDesktopShortcut )]
        public bool NoDesktopShortcut { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppLaunchOptionFields.eFieldNoStartMenuShortcut )]
        public bool NoStartMenuShortcut { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppLaunchOptionFields.eFieldLongRunningUnattended )]
        public bool LongRunningUnattended { get; set; }
    }

    [SqlTable( "AppVersionList" )]
    class AppVersion : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string RelHash { get; set; }

        [SqlColumn( IsNotNull = true )]
        [SqlRelation( Column = "AppID" )]
        public uint RelAppID { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldDescription )]
        public string Description { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldVersionId )]
        public uint VersionID { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldIsNotAvailable )]
        public bool IsNotAvailable { get; set; }


        [SqlColumn( IsNotNull = true, ColumnType = SqlColumnType.DataList )]
        [BlobField( Field = CDRAppVersionFields.eFieldLaunchOptionIdsRecord, FieldType = FieldType.TypeList, KeyType = typeof( int ) )]
        public BlobList<int> LaunchOptionIDs { get; private set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldDepotEncryptionKey )]
        public string DepotEncryptionKey { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldIsEncryptionKeyAvailable )]
        public bool IsEncryptionKeyAvailable { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldIsRebased )]
        public bool IsRebased { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppVersionFields.eFieldIsLongVersionRoll )]
        public bool IsLongVersionRoll { get; set; }

        public AppVersion()
        {
            this.LaunchOptionIDs = new BlobList<int>();
        }
    }

    [SqlTable( "AppFilesystemList" )]
    class AppFilesystem : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string RelHash { get; set; }

        [SqlRelation( Column = "AppID" )]
        [SqlColumn( IsNotNull = true )]
        public uint RelAppID { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppFilesystemFields.eFieldAppId )]
        public uint AppID { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppFilesystemFields.eFieldMountName )]
        public string MountName { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppFilesystemFields.eFieldIsOptional )]
        public bool IsOptional { get; set; }
    }

    [SqlTable( "AppList" )]
    class App : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string Hash { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldAppId )]
        public uint AppID { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldName )]
        public string Name { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldInstallDirName )]
        public string InstallDirName { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldMinCacheFileSizeMB )]
        public uint MinCacheFileSizeMB { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldMaxCacheFileSizeMB )]
        public uint MaxCacheFileSizeMB { get; set; }


        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( AppLaunchOption ) )]
        [BlobField( Field = CDRAppRecordFields.eFieldLaunchOptionsRecord, FieldType = FieldType.BlobList, ValueType = typeof( AppLaunchOption ) )]
        public BlobList<AppLaunchOption> LaunchOptions { get; private set; }


        // todo: icons record


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldOnFirstLaunch )]
        public int OnFirstLaunch { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldIsBandwidthGreedy )]
        public bool IsBandwidthGreedy { get; set; }


        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( AppVersion ) )]
        [BlobField( Field = CDRAppRecordFields.eFieldVersionsRecord, FieldType = FieldType.BlobList, ValueType = typeof( AppVersion ) )]
        public BlobList<AppVersion> Versions { get; private set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldCurrentVersionId )]
        public uint CurrentVersionID { get; set; }


        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( AppFilesystem ) )]
        [BlobField( Field = CDRAppRecordFields.eFieldFilesystemsRecord, FieldType = FieldType.BlobList, ValueType = typeof( AppFilesystem ) )]
        public BlobList<AppFilesystem> Filesystems { get; private set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldTrickleVersionId )]
        public int TrickleVersionID { get; set; }


        [SqlColumn( IsNotNull = true, ColumnType = SqlColumnType.DataDictionary )]
        [BlobField( Field = CDRAppRecordFields.eFieldUserDefinedRecord, FieldType = FieldType.TypeDictionary, KeyType = typeof( string ), ValueType = typeof( string ) )]
        public BlobDictionary<string, string> UserDefined { get; private set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldBetaVersionPassword )]
        public string BetaVersionPassword { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldBetaVersionId )]
        public int BetaVersionID { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldLegacyInstallDirName )]
        public string LegacyInstallDirName { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldSkipMFPOverwrite )]
        public bool SkipMFPOverwrite { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldUseFilesystemDvr )]
        public bool UseFilesystemDvr { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldManifestOnlyApp )]
        public bool ManifestOnlyApp { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRAppRecordFields.eFieldAppOfManifestOnlyCache )]
        public uint AppOfManifestOnlyCache { get; set; }


        public App()
        {
            this.LaunchOptions = new BlobList<AppLaunchOption>();
            this.UserDefined = new BlobDictionary<string, string>();
            this.Versions = new BlobList<AppVersion>();
            this.Filesystems = new BlobList<AppFilesystem>();
        }
    }
}