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
}
