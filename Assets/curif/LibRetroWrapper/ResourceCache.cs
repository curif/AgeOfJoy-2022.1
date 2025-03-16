using System.Collections.Generic;
using UnityEngine;

public interface IResourceCache
{
    void FreeResources();
}

public class ResourceCacheManager
{
    public static List<IResourceCache> caches = new List<IResourceCache>();
    public static ResourceCache<K, V> Create<K, V>(int capacity = 100) // Added default capacity
    {
        ResourceCache<K, V> cache = new ResourceCache<K, V>(capacity);
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
    private readonly int capacity;
    private Dictionary<K, V> cache = new Dictionary<K, V>();
    private LinkedList<K> lruList = new LinkedList<K>(); // Tracks usage order

    internal ResourceCache(int capacity = 100) // Default capacity of 100
    {
        this.capacity = capacity;
    }

    public void FreeResources()
    {
        foreach (var pair in cache)
        {
            DestroyIfUnityObject(pair.Value);
        }
        cache.Clear();
        lruList.Clear();
    }

    public V Get(K key)
    {
        if (key != null && cache.ContainsKey(key))
        {
            // Move to front (most recently used)
            lruList.Remove(key);
            lruList.AddFirst(key);
            return cache[key];
        }
        return default;
    }

    public void Add(K key, V value)
    {
        if (key == null || value == null) return;

        if (cache.ContainsKey(key))
        {
            // Update existing entry
            lruList.Remove(key);
            lruList.AddFirst(key);
            cache[key] = value; // Overwrite without immediate destruction
        }
        else
        {
            // Add new entry
            if (cache.Count >= capacity)
            {
                // Remove least recently used item
                K lruKey = lruList.Last.Value;
                lruList.RemoveLast();
                DestroyIfUnityObject(cache[lruKey]);
                cache.Remove(lruKey);
            }
            cache.Add(key, value);
            lruList.AddFirst(key);
        }
    }

    public void Remove(K key)
    {
        if (key != null && cache.ContainsKey(key))
        {
            lruList.Remove(key);
            DestroyIfUnityObject(cache[key]);
            cache.Remove(key);
        }
    }

    public void Clear()
    {
        foreach (var pair in cache)
        {
            DestroyIfUnityObject(pair.Value);
        }
        cache.Clear();
        lruList.Clear();
    }

    public bool ContainsKey(K key)
    {
        return key != null && cache.ContainsKey(key);
    }

    private void DestroyIfUnityObject(V value)
    {
        if (value is Object unityObj && unityObj != null)
        {
            if (!unityObj.Equals(null))
            {
                Object.Destroy(unityObj);
            }
        }
    }
}