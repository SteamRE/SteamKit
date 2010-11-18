using System;
using System.Collections.Generic;

public class MultiMap<K, V>
{
    Dictionary<K, List<V>> _dictionary = new Dictionary<K, List<V>>();

    public void Add(K key, V value)
    {
        List<V> list;
        if (this._dictionary.TryGetValue(key, out list))
        {
            list.Add(value);
        }
        else
        {
            list = new List<V>();
            list.Add(value);
            this._dictionary[key] = list;
        }
    }

    public IEnumerable<K> Keys
    {
        get
        {
            return this._dictionary.Keys;
        }
    }

    public List<V> this[K key]
    {
        get
        {
            List<V> list;
            if (this._dictionary.TryGetValue(key, out list))
            {
                return list;
            }
            else
            {
                return new List<V>();
            }
        }
    }
}