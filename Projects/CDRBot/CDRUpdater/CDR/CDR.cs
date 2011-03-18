using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SqlNet;

namespace CDRUpdater
{
    [SqlTable( TableName = "CDRList" )]
    class CDR : IBlob
    {
        [SqlColumn( IsPrimaryKey = true, PrimaryKeyLength = 20, IsNotNull = true )]
        public string Hash { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRFields.eFieldVersionNum )]
        public ushort VersionNum { get; set; }


        [SqlColumn( IsNotNull = true )]
        public int NumApps
        {
            get { return Apps.Count; }
        }

        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( App ) )]
        [BlobField( Field = CDRFields.eFieldApplicationsRecord, FieldType = FieldType.BlobList, ValueType = typeof( App ) )]
        public BlobList<App> Apps { get; private set; }


        [SqlColumn( IsNotNull = true )]
        public int NumSubs
        {
            get { return Subs.Count; }
        }

        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( Sub ) )]
        [BlobField( Field = CDRFields.eFieldSubscriptionsRecord, FieldType = FieldType.BlobList, ValueType = typeof( Sub ) )]
        public BlobList<Sub> Subs { get; private set; }


        [SqlColumn( IsNotNull = true )]
        public uint Date
        {
            get { return LastChangedExistingAppOrSubscriptionTime.ToUnixTime(); }
        }

        [BlobField( Field = CDRFields.eFieldLastChangedExistingAppOrSubscriptionTime )]
        public MicroTime LastChangedExistingAppOrSubscriptionTime { get; set; }


        [BlobField( Field = CDRFields.eFieldAllAppsPublicKeysRecord, FieldType = FieldType.TypeDictionary, KeyType = typeof( uint ), ValueType = typeof( byte[] ) )]
        public BlobDictionary<uint, byte[]> AllAppsPublicKeys { get; private set; }



        public CDR()
        {
            this.Apps = new BlobList<App>();
            this.Subs = new BlobList<Sub>();
            this.AllAppsPublicKeys = new BlobDictionary<uint, byte[]>();
        }
    }
}