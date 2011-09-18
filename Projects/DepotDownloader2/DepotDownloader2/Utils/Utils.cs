/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepotDownloader2
{
    static class DictionaryExtensions
    {
        public static bool TryGetValue<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key, out TValue value, IEqualityComparer<TKey> comp )
        {
            value = default( TValue );

            foreach ( var kvp in dict )
            {
                if ( comp.Equals( kvp.Key, key ) )
                {
                    value = kvp.Value;
                    return true;
                }
            }

            return false;
        }
    }

    static class LinqExtensions
    {
        public static IEnumerable<TSource> Except<TSource>( this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer )
        {
            return first.Except( second, new LambdaComparer<TSource>( comparer ) );
        }
    }

    class LambdaComparer<T> : IEqualityComparer<T>
    {
        Func<T, T, bool> equalFunc;
        Func<T, int> hashFunc;

        public LambdaComparer( Func<T, T, bool> comparerFunc, Func<T, int> hashCodeFunc = null )
        {
            equalFunc = comparerFunc;
            hashFunc = hashCodeFunc;

            if ( hashFunc == null )
                hashFunc = obj => obj.GetHashCode();
        }

        public bool Equals( T x, T y )
        {
            return equalFunc( x, y );
        }

        public int GetHashCode( T obj )
        {
            return obj.GetHashCode();
        }
    }
}
