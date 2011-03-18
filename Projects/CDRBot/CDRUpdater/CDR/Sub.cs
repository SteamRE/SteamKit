using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SqlNet;

namespace CDRUpdater
{
    // todo: this is temporary, should move it into steamkit eventually!
    enum ESubscriptionBillingType : ushort
    {
        NoCost = 0,
        BillOnceOnly = 1,
        BillMonthly = 2,
        ProofOfPrepurchaseOnly = 3,
        GuestPass = 4,
        HardwarePromo = 5,
        Gift = 6,
        AutoGrant = 7,
    }

    [SqlTable( "SubRateLimitList" )]
    class SubRateLimit : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string RelHash { get; set; }

        [SqlRelation( Column = "SubID" )]
        [SqlColumn( IsNotNull = true )]
        public uint RelSubID { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRateLimitFields.eFieldLimit ) ]
        public uint Limit { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRateLimitFields.eFieldPeriodInMinutes )]
        public uint PeriodInMinutes { get; set; }
    }

    [SqlTable( "SubDiscountList" )]
    class SubDiscount : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string RelHash { get; set; }

        [SqlRelation( Column = "SubID" )]
        [SqlColumn( IsNotNull = true )]
        public uint RelSubID { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubDiscountFields.eFieldName )]
        public string Name { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubDiscountFields.eFieldDiscountInCents )]
        public uint DiscountInCents { get; set; }


        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( SubDiscountQualifier ) )]
        [BlobField( Field = CDRSubDiscountFields.eFieldDiscountQualifiersRecord, FieldType = FieldType.BlobList, ValueType = typeof( SubDiscountQualifier ) )]
        public List<SubDiscountQualifier> DiscountQualifiers { get; private set; }


        public SubDiscount()
        {
            this.DiscountQualifiers = new List<SubDiscountQualifier>();
        }
    }

    [SqlTable( "SubDiscountQualifierList" )]
    class SubDiscountQualifier : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubDiscountQualifierFields.eFieldName )]
        public string Name { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubDiscountQualifierFields.eFieldSubscriptionRequired )]
        public uint SubscriptionRequired { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubDiscountQualifierFields.eFieldIsDisqualifier )]
        public bool IsDisqualifier { get; set; }
    }

    [SqlTable( "SubList" )]
    class Sub : IBlob
    {
        [SqlColumn( IsNotNull = true, IsPrimaryKey = true, IsAutoIncrement = true )]
        public ulong ID { get; set; }


        #region Relational Columns
        [SqlRelation( Column = "Hash" )]
        [SqlColumn( IsNotNull = true )]
        public string Hash { get; set; }
        #endregion


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldSubId )]
        public uint SubID { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldName )]
        public string Name { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldBillingType )]
        public ESubscriptionBillingType BillingType { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldCostInCents )]
        public uint CostInCents { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldPeriodInMinutes )]
        public int PeriodInMinutes { get; set; }


        [SqlColumn( IsNotNull = true, ColumnType = SqlColumnType.DataList )]
        [BlobField( Field = CDRSubRecordFields.eFieldAppIdsRecord, FieldType = FieldType.TypeList, KeyType = typeof( uint ) )]
        public BlobList<uint> AppIDs { get; private set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldOnSubscribeRunAppId )]
        public int OnSubscribeRunAppID { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldOnSubscribeRunLaunchOptionIndex )]
        public int OnSubscribeRunLaunchOptionIndex { get; set; }


        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( SubRateLimit ) )]
        [BlobField( Field = CDRSubRecordFields.eFieldOptionalRateLimitRecord, FieldType = FieldType.BlobList, ValueType = typeof( SubRateLimit ) )]
        public BlobList<SubRateLimit> RateLimits { get; private set; }


        [SqlColumn( ColumnType = SqlColumnType.RelationalList, TableType = typeof( SubDiscount ) )]
        [BlobField( Field = CDRSubRecordFields.eFieldDiscountsRecord, FieldType = FieldType.BlobList, ValueType = typeof( SubDiscount ) )]
        public BlobList<SubDiscount> Discounts { get; private set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldIsPreorder )]
        public bool IsPreorder { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldRequiresShippingAddress )]
        public bool RequiresShippingAddress { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldDomesticCostInCents )]
        public uint DomesticCostInCents { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldInternationalCostInCents )]
        public uint InternationalCostInCents { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldRequiredKeyType )]
        public uint RequiredKeyType { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldIsCyberCafe )]
        public bool IsCyberCafe { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldGameCode )]
        public int GameCode { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldGameCodeDescription )]
        public string GameCodeDescription { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldIsDisabled )]
        public bool IsDisabled { get; set; }

        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldRequiresCD )]
        public bool RequiresCD { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldTerritoryCode )]
        public uint TerritoryCode { get; set; }


        [SqlColumn( IsNotNull = true )]
        [BlobField( Field = CDRSubRecordFields.eFieldIsSteam3Subscription )]
        public bool IsSteam3Subscription { get; set; }


        [SqlColumn( IsNotNull = true, ColumnType = SqlColumnType.DataDictionary )]
        [BlobField( Field = CDRSubRecordFields.eFieldExtendedInfoRecord, FieldType = FieldType.TypeDictionary, KeyType = typeof( string ), ValueType = typeof( string ) )]
        public BlobDictionary<string, string> ExtendedInfo { get; private set; }


        public Sub()
        {
            this.AppIDs = new BlobList<uint>();
            this.RateLimits = new BlobList<SubRateLimit>();
            this.Discounts = new BlobList<SubDiscount>();
            this.ExtendedInfo = new BlobDictionary<string, string>();
        }
    }
}