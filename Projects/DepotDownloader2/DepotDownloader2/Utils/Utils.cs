/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;

namespace DepotDownloader2
{
    class NodeComparer : IEqualityComparer<Steam2Manifest.Node>
    {
        public bool Equals( Steam2Manifest.Node x, Steam2Manifest.Node y )
        {
            return string.Equals( x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase );
        }

        public int GetHashCode( Steam2Manifest.Node obj )
        {
            if ( Object.ReferenceEquals( obj, null ) || obj.FullName == null )
                return 0;

            return obj.FullName.GetHashCode();
        }
    }

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

    static class Utils
    {
        public static T RepeatAction<T>( int numRepeats, Func<T> func )
        {
            int timesDone = 0;

            do
            {
                var obj = func();

                if ( obj != null )
                    return obj;

                timesDone++;
            }
            while ( timesDone < numRepeats );

            return default( T );
        }
    }
}
