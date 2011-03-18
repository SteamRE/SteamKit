using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlNet
{

    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public sealed class SqlTableAttribute : Attribute
    {

        public string TableName { get; set; }

        public SqlTableAttribute()
        {
        }
        public SqlTableAttribute( string table )
        {
            this.TableName = table;
        }

    }

    [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
    public sealed class SqlRelationAttribute : Attribute
    {
        public string Column { get; set; }

        public SqlRelationAttribute()
        {
        }
    }

    public enum SqlColumnType
    {
        Data,
        DataList,
        DataDictionary,
        RelationalList,
    }

    [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
    public sealed class SqlColumnAttribute : Attribute
    {

        public bool IsPrimaryKey { get; set; }
        public int PrimaryKeyLength { get; set; }

        public bool IsAutoIncrement { get; set; }
        public bool IsNotNull { get; set; }

        public string RawColumnType { get; set; }

        //public bool IsList { get; set; }
        public SqlColumnType ColumnType { get; set; }
        public Type TableType { get; set; }


        public SqlColumnAttribute()
        {
        }

    }

}
