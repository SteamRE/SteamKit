using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDRUpdater
{

    enum FieldType
    {
        Type,

        BlobList, // list of blobs

        TypeList, // list of types
        TypeDictionary, // list of key values
    }

    [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
    sealed class BlobFieldAttribute : Attribute
    {
        public int Field { get; set; }

        public FieldType FieldType { get; set; }

        public Type KeyType { get; set; }
        public Type ValueType { get; set; }

        public BlobFieldAttribute()
        {
            this.FieldType = FieldType.Type;

            this.ValueType = null;
            this.KeyType = null;
        }
        public BlobFieldAttribute( int field )
            : this()
        {
            this.Field = field;
        }
    }

    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    sealed class BlobAttribute : Attribute
    {
    }
}
