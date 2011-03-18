using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CDRUpdater
{
    interface IBlob
    {
    }

    interface IBlobList : IEnumerable
    {
        void Add( object obj );
        object[] ToArray();
    }

    interface IBlobDictionary : IEnumerable<KeyValuePair<object, object>>
    {
        void Add( object key, object value );
    }

    class BlobList<T> : List<T>, IBlobList
    {
        public void Add( object obj )
        {
            base.Add( ( T )obj );
        }

        public new IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        public new object[] ToArray()
        {
            return base.ConvertAll<object>( ( val ) => { return ( object )val; } ).ToArray();
        }
    }

    class BlobDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IBlobDictionary
    {
        public void Add( object key, object value )
        {
            base.Add( ( TKey )key, ( TValue )value );
        }


        public new IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            Dictionary<object, object> obj = new Dictionary<object, object>();

            var enumer = base.GetEnumerator();
            while ( enumer.MoveNext() )
            {
                var kvp = enumer.Current;
                obj.Add( kvp.Key, kvp.Value );
            }

            return obj.GetEnumerator();
        }

    }
}
