using System.Collections.Generic;

public interface IResourceCache
{
    void FreeResources();
}

public class ResourceCacheManager
{
    public static List<IResourceCache> caches = new List<IResourceCache>();
    public static ResourceCache<K, V> Create<K, V>()
    {
        ResourceCache<K, V> cache = new ResourceCache<K, V>();
        caches.Add(cache);
        return cache;
    }

    public static void FreeResources()
    {
        foreach (IResourceCache cache in caches)
        {
            cache.FreeResources();
        }
    }
}

public class ResourceCache<K, V> : IResourceCache
{
    private Dictionary<K, V> cache = new Dictionary<K, V>();
    
    internal ResourceCache() { }

    public void FreeResources()
    {
        // Super aggressive ! TODO fine tune this
        cache.Clear();
    }

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
