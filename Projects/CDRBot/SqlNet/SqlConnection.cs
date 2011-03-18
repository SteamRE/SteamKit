using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace SqlNet
{

    public class SqlException : Exception
    {
        public SqlException()
            : base()
        {
        }
        public SqlException( string message )
            : base( message )
        {
        }
    }

    class SqlCommandCache
    {
        public Dictionary<string, List<SqlInsertionData>> Insertions { get; private set; }

        public SqlCommandCache()
        {
            Insertions = new Dictionary<string, List<SqlInsertionData>>();
        }
    }

    class SqlInsertionData
    {
        public string ColumnList { get; set; }
        public List<string> ValuesList { get; set; }
    }

    public class Sql
    {
        MySqlConnection sqlConn;
        SqlCommandCache commandCache;


        public Sql()
        {
            this.sqlConn = new MySqlConnection();
            this.commandCache = new SqlCommandCache();
        }


        public void Connect( string host, string userName, string password, string db )
        {
            this.Disconnect();

            this.sqlConn.ConnectionString = string.Format( "Server = {0}; Database = {1}; User ID = {2}; Pwd = {3}; Pooling = true; Compress = true;", host, db, userName, password );
            this.sqlConn.Open();
        }

        public void Disconnect()
        {
            this.sqlConn.Close();
        }


        public void CreateTable( Type tbl, bool ifNotExists )
        {
            var tableInfo = tbl.GetAttribute<SqlTableAttribute>();

            if ( tableInfo == null )
                throw new SqlException( "Table does not have SqlTableAttribute!" );

            StringBuilder createBuilder = new StringBuilder();

            createBuilder.Append( "CREATE TABLE" );

            if ( ifNotExists )
                createBuilder.Append( " IF NOT EXISTS" );

            createBuilder.AppendFormat( " `{0}` ( ", tableInfo.TableName );

            List<string> columnList = new List<string>();
            foreach ( var propInfo in tbl.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var columnInfo = propInfo.GetAttribute<SqlColumnAttribute>();

                if ( columnInfo == null )
                    continue;

                if ( columnInfo.ColumnType == SqlColumnType.RelationalList )
                {
                    this.CreateTable( columnInfo.TableType, ifNotExists );
                    continue;
                }

                StringBuilder columnStr = new StringBuilder();

                // name
                columnStr.AppendFormat( "`{0}`", propInfo.Name );

                // type
                if ( columnInfo.ColumnType == SqlColumnType.DataList || columnInfo.ColumnType == SqlColumnType.DataDictionary )
                {
                    columnStr.Append( " TEXT" );
                }
                else
                {
                    columnStr.AppendFormat( " {0}", columnInfo.RawColumnType ?? GetSqlType( propInfo.PropertyType ) );
                }

                if ( columnInfo.IsNotNull )
                    columnStr.Append( " NOT NULL" );

                if ( columnInfo.IsAutoIncrement )
                    columnStr.Append( " AUTO_INCREMENT" );

                columnList.Add( columnStr.ToString() );

                if ( columnInfo.IsPrimaryKey )
                {
                    columnList.Add( string.Format(
                        "PRIMARY KEY ( `{0}` {1} )",
                        propInfo.Name,
                        columnInfo.PrimaryKeyLength > 0 ? string.Format( "( {0} )", columnInfo.PrimaryKeyLength ) : ""
                    ) );
                }
            }

            createBuilder.AppendFormat( "{0} )", string.Join( ", ", columnList.ToArray() ) );

            var cmd = this.sqlConn.CreateCommand();
            cmd.CommandText = createBuilder.ToString();

            cmd.ExecuteNonQuery();
        }

        public void FlushInsertions()
        {
            foreach ( var insertion in this.commandCache.Insertions )
            {
                StringBuilder insertBuilder = new StringBuilder();

                insertBuilder.AppendFormat( "LOCK TABLES {0} WRITE; INSERT INTO {1}({2})VALUES", insertion.Key, insertion.Key, insertion.Value[ 0 ].ColumnList );

                foreach ( var subInsertionList in insertion.Value )
                {
                    foreach ( var subInsertion in subInsertionList.ValuesList )
                    {
                        insertBuilder.AppendFormat( "{0},", subInsertion );
                    }
                }

                insertBuilder.Remove( insertBuilder.Length - 1, 1 );
                insertBuilder.AppendFormat( "; UNLOCK TABLES", insertion.Key );

                var cmd = this.sqlConn.CreateCommand();
                cmd.CommandText = insertBuilder.ToString();

                cmd.ExecuteNonQuery();
            }
        }

        public MySqlDataReader Select( string query )
        {
            var cmd = this.sqlConn.CreateCommand();
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if ( !reader.Read() )
            {
                reader.Close();
                return null;
            }

            return reader;
        }

		// When I wrote this, only God and I understood what I was doing
		// Now, God only knows
        public void Insert( object obj )
        {
            if ( obj == null )
                return;

            PrepRelations( null, obj );

            Type objType = obj.GetType();

            var tableInfo = objType.GetAttribute<SqlTableAttribute>();
            if ( tableInfo == null )
                throw new SqlException( "Row does not have an acompanying SqlTableAttribute!" );


            StringBuilder columnBuilder = new StringBuilder();
            foreach ( var propInfo in objType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var columnInfo = propInfo.GetAttribute<SqlColumnAttribute>();

                if ( columnInfo == null || columnInfo.IsAutoIncrement || columnInfo.ColumnType == SqlColumnType.RelationalList)
                    continue;

                columnBuilder.AppendFormat( "`{0}`,", propInfo.Name );
            }
            columnBuilder.Remove( columnBuilder.Length - 1, 1 );


            StringBuilder insertBuilder = new StringBuilder();
            foreach ( var propInfo in objType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var columnInfo = propInfo.GetAttribute<SqlColumnAttribute>();

                if ( columnInfo == null || columnInfo.IsAutoIncrement )
                    continue;

                object data = GetSqlData( propInfo, obj );

                if ( columnInfo.ColumnType == SqlColumnType.RelationalList )
                {
                    // data is a List<T>
                    IEnumerable dataList = ( IEnumerable )data;

                    foreach ( var dataObj in dataList )
                        this.Insert( dataObj );

                    continue;
                }
                else if ( columnInfo.ColumnType == SqlColumnType.DataList )
                {
                    IEnumerable dataList = ( IEnumerable )data;

                    StringBuilder listBuilder = new StringBuilder();

                    foreach ( var dataObj in dataList )
                        listBuilder.AppendFormat( "{0};", dataObj.ToString() );

                    insertBuilder.AppendFormat( "'{0}',", Sql.Escape( listBuilder.ToString() ) );
                    continue;
                }
                else if ( columnInfo.ColumnType == SqlColumnType.DataDictionary )
                {
                    IDictionary dataDict = ( IDictionary )data;
                    IDictionaryEnumerator dictEnum = dataDict.GetEnumerator();

                    StringBuilder dictBuilder = new StringBuilder();

                    while ( dictEnum.MoveNext() )
                        dictBuilder.AppendFormat( "{0}={1};", dictEnum.Key, dictEnum.Value );

                    insertBuilder.AppendFormat( "'{0}',", Sql.Escape( dictBuilder.ToString() ) );
                    continue;
                }

                insertBuilder.AppendFormat( "'{0}',", Sql.Escape( data.ToString() ) );
            }
            insertBuilder.Remove( insertBuilder.Length - 1, 1 );

            var insertion = new SqlInsertionData()
            {
                ColumnList = columnBuilder.ToString(),
                ValuesList = new List<string>(),
            };

            insertion.ValuesList.Add( "(" + insertBuilder.ToString() + ")" );

            var insertCache = commandCache.Insertions;

            if ( !insertCache.ContainsKey( tableInfo.TableName ) )
                insertCache.Add( tableInfo.TableName, new List<SqlInsertionData>() );

            insertCache[ tableInfo.TableName ].Add( insertion );
        }


        public static string Escape( string input )
        {
            return MySqlHelper.EscapeString( input );
        }


        static void PrepRelations( object rootObj, object obj )
        {
            Type objType = obj.GetType();

            foreach ( var propInfo in objType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var columnInfo = propInfo.GetAttribute<SqlColumnAttribute>();

                if ( columnInfo == null)
                    continue;

                object data = propInfo.GetValue( obj, null );

                if ( columnInfo.ColumnType == SqlColumnType.RelationalList )
                {
                    IEnumerable list = ( IEnumerable )data;

                    foreach ( var subData in list )
                        PrepRelations( obj, subData );

                    continue;
                }

                var relationInfo = propInfo.GetAttribute<SqlRelationAttribute>();

                if ( relationInfo == null || rootObj == null )
                    continue;

                object rootValue = GetColumnValue( rootObj, relationInfo.Column );
                propInfo.SetValue( obj, rootValue, null );
                
            }
        }

        static object GetSqlData( PropertyInfo propInfo, object obj )
        {
            object data = propInfo.GetValue( obj, null );
            Type type = propInfo.PropertyType;

            if ( data == null )
                return "";

            if ( type.IsEnum )
            {
                type = Enum.GetUnderlyingType( type );
                data = Convert.ChangeType( data, type );

                return data;
            }

            if ( propInfo.PropertyType == typeof( bool ) )
            {
                bool bData = ( bool )data;

                return bData ? 1 : 0;
            }

            return data;
        }

        static object GetColumnValue( object obj, string column )
        {
            var propInfo = obj.GetType().GetProperty( column, BindingFlags.Public | BindingFlags.Instance );
            return GetSqlData( propInfo, obj );
        }

        static string GetSqlType( Type type )
        {
            if ( type.IsEnum )
                type = Enum.GetUnderlyingType( type );

            if ( type == typeof( int ) )
                return "INT";
            else if ( type == typeof( uint ) )
                return "INT UNSIGNED";
            else if ( type == typeof( short ) )
                return "SMALLINT";
            else if ( type == typeof( ushort ) )
                return "SMALLINT UNSIGNED";
            else if ( type == typeof( ulong ) )
                return "BIGINT UNSIGNED";
            else if ( type == typeof( long ) )
                return "BIGINT";
            else if ( type == typeof( sbyte ) )
                return "TINYINT";
            else if ( type == typeof( byte ) )
                return "TINYINT UNSIGNED";
            else if ( type == typeof( char ) )
                return "TINYINT UNSIGNED";
            else if ( type == typeof( string ) )
                return "TEXT";
            else if ( type == typeof( bool ) )
                return "BOOL";


            throw new NotImplementedException( "GetSqlType missing type!" );
        }

    }
}
