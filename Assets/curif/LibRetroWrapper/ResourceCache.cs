using System.Collections.Generic;
using UnityEngine;

public interface IResourceCache
{
    void FreeResources();
}

public class ResourceCacheManager
{
    public static List<IResourceCache> caches = new List<IResourceCache>();
    public static ResourceCache<K, V> Create<K, V>(string name, float maxSizeInMB = 512f) // Default to 512MB
    {
        ResourceCache<K, V> cache = new ResourceCache<K, V>(name, maxSizeInMB);
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
    private readonly float maxSizeInMB; // Maximum size in megabytes
    private float currentSizeInMB; // Current total size in megabytes
    private Dictionary<K, V> cache = new Dictionary<K, V>();
    private Dictionary<K, float> sizeMap = new Dictionary<K, float>(); // Tracks size of each item
    private LinkedList<K> lruList = new LinkedList<K>(); // Tracks usage order
    string Name;

    internal ResourceCache(string name, float maxSizeInMB = 512f)
    {
        this.maxSizeInMB = maxSizeInMB;
        this.currentSizeInMB = 0f;
        this.Name = name;
    }

    public void FreeResources()
    {
        foreach (var pair in cache)
        {
            DestroyIfUnityObject(pair.Value);
        }
        cache.Clear();
        sizeMap.Clear();
        lruList.Clear();
        currentSizeInMB = 0f;
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

    //is the cache exeeded in case of add the element with sizeInMB mb?
    public bool CacheExceeded(float sizeInMB)
    {
        return currentSizeInMB + sizeInMB > maxSizeInMB && lruList.Count > 0;
    }

    public void Add(K key, V value, float sizeInMB)
    {
        if (key == null || value == null || sizeInMB < 0f) return; // Enforce valid size

        if (cache.ContainsKey(key))
        {
            // Update existing entry
            float oldSize = sizeMap[key];
            currentSizeInMB -= oldSize;
            currentSizeInMB += sizeInMB;

            lruList.Remove(key);
            lruList.AddFirst(key);
            cache[key] = value;
            sizeMap[key] = sizeInMB;
        }
        else
        {
            if (CacheExceeded(sizeInMB))
                ConfigManager.WriteConsoleWarning($"Cache {this.Name} Exceeded adding {sizeInMB}MB Actual: {currentSizeInMB} Max: {maxSizeInMB} <<<<<<<<");

            // Evict items if necessary
            while (CacheExceeded(sizeInMB))
            {
                K lruKey = lruList.Last.Value;
                lruList.RemoveLast();
                DestroyIfUnityObject(cache[lruKey]);
                currentSizeInMB -= sizeMap[lruKey];
                cache.Remove(lruKey);
                sizeMap.Remove(lruKey);
            }

            // Add new entry
            cache.Add(key, value);
            sizeMap.Add(key, sizeInMB);
            lruList.AddFirst(key);
            currentSizeInMB += sizeInMB;
        }
    }

    public void Remove(K key)
    {
        if (key != null && cache.ContainsKey(key))
        {
            lruList.Remove(key);
            DestroyIfUnityObject(cache[key]);
            currentSizeInMB -= sizeMap[key];
            cache.Remove(key);
            sizeMap.Remove(key);
        }
    }

    public void Clear()
    {
        foreach (var pair in cache)
        {
            DestroyIfUnityObject(pair.Value);
        }
        cache.Clear();
        sizeMap.Clear();
        lruList.Clear();
        currentSizeInMB = 0f;
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
    public void FreeHalfResources()
    {
        if (lruList.Count <= 1 || currentSizeInMB <= 0f) // No need to process if empty or no size
            return;

        float targetSizeToFree = currentSizeInMB / 2f; // Target amount of MBs to free
        float freedSize = 0f;

        // Remove from the end (least recently used) until we've freed half the size
        while (freedSize < targetSizeToFree && lruList.Count > 0)
        {
            K lruKey = lruList.Last.Value;
            float itemSize = sizeMap[lruKey];

            lruList.RemoveLast();
            DestroyIfUnityObject(cache[lruKey]);
            cache.Remove(lruKey);
            sizeMap.Remove(lruKey);

            currentSizeInMB -= itemSize;
            freedSize += itemSize;
        }
    }
}