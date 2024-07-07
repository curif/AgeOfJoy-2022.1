using System.Collections.Generic;

public class ResourceCache<K, V>
{
    private Dictionary<K, V> cache = new Dictionary<K, V>();

    public V Get(K key)
    {
        return key != null && cache.ContainsKey(key) ? cache[key] : default;
    }

    public void Add(K key, V value)
    {
        if (key != null && value != null)
        {
            if (!cache.ContainsKey(key))
            {
                cache.Add(key, value);
            }
            else
            {
                cache[key] = value;
            }
        }
    }

    public void Remove(K key)
    {
        if (key != null && cache.ContainsKey(key))
        {
            cache.Remove(key);
        }
    }

    public void Clear()
    {
        cache.Clear();
    }

    public bool ContainsKey(K key)
    {
        return key != null && cache.ContainsKey(key);
    }
}
