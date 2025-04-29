using System;
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
    private readonly object locker = new object();

    internal ResourceCache(string name, float maxSizeInMB = 512f)
    {
        this.maxSizeInMB = maxSizeInMB;
        this.currentSizeInMB = 0f;
        this.Name = name;
    }

    public void FreeResources()
    {
        lock (locker)
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
    }


    public V Get(K key)
    {
        if (key == null) return default;

        lock (locker)
        {
            if (cache.TryGetValue(key, out var value))
            {
                lruList.Remove(key);
                lruList.AddFirst(key);
                return value;
            }
            return default;
        }
    }

    //is the cache exeeded in case of add the element with sizeInMB mb?
    public bool CacheExceeded(float sizeInMB)
    {
        lock (locker)
        {
            return currentSizeInMB + sizeInMB > maxSizeInMB && lruList.Count > 0;
        }
    }

    public V Add(K key, V value, float sizeInMB, bool replaceIfExists = false)
    {
        if (key == null || value == null || sizeInMB < 0f) return value;

        lock (locker)
        {
            if (cache.ContainsKey(key))
            {
                if (replaceIfExists)
                {
                    DestroyIfUnityObject(cache[key]);

                    float oldSize = sizeMap[key];
                    currentSizeInMB -= oldSize;

                    lruList.Remove(key);

                    currentSizeInMB += sizeInMB;
                    lruList.AddFirst(key);
                    cache[key] = value;
                    sizeMap[key] = sizeInMB;

                    if (oldSize > sizeInMB)
                    {
                        makeSpaceFor(oldSize - sizeInMB);
                    }
                }
            }
            else
            {
                /* unnecesary message because this is usual*/
                // if (CacheExceeded(sizeInMB))
                //  ConfigManager.WriteConsole($"[ResourceCache] {this.Name} Exceeded adding key: {key} \n size: {sizeInMB}MB \n Actual: {currentSizeInMB}\n Max: {maxSizeInMB}MB");

                Status();

                makeSpaceFor(sizeInMB);

                cache.Add(key, value);
                sizeMap.Add(key, sizeInMB);
                lruList.AddFirst(key);
                currentSizeInMB += sizeInMB;
            }
        }

        return cache[key];
    }

    private void makeSpaceFor(float sizeInMB)
    {
        while (CacheExceeded(sizeInMB))
        {
            K lruKey = lruList.Last.Value;
            lruList.RemoveLast();
            DestroyIfUnityObject(cache[lruKey]);
            currentSizeInMB -= sizeMap[lruKey];
            ConfigManager.WriteConsole($"[ResourceCache] {this.Name} \n removed {lruKey} of size: {sizeMap[lruKey]}MB");
            cache.Remove(lruKey);
            sizeMap.Remove(lruKey);

        }
    }

    public void Status()
    {
        ConfigManager.WriteConsole($"[ResourceCache] {this.Name} \n size: {currentSizeInMB}MB \n Count: {lruList.Count}");
    }

    public void Remove(K key)
    {
        if (key == null) return;

        lock (locker)
        {
            if (cache.ContainsKey(key))
            {
                lruList.Remove(key);
                DestroyIfUnityObject(cache[key]);
                currentSizeInMB -= sizeMap[key];
                cache.Remove(key);
                sizeMap.Remove(key);
            }
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
        if (key == null) return false;
        lock (locker)
        {
            return cache.ContainsKey(key);
        }
    }
    private void DestroyIfUnityObject(V value)
    {
        if (value is UnityEngine.Object unityObj && unityObj != null)
        {
            if (!unityObj.Equals(null))
            {
                /*
                DestroyImmediate is dangerous in play mode and recommended only in Editor scripts.
                On Meta Quest, you want the safe version(Destroy), which schedules destruction properly for next frame.
                */
                ConfigManager.WriteConsole($"[ResourceCache] destroying {unityObj}");

                //UnityEngine.Object.DestroyImmediate(unityObj);
                UnityEngine.Object.Destroy(unityObj);
            }
        }
    }
    public void FreeHalfResources()
    {
        lock (locker)
        {
            if (lruList.Count <= 1 || currentSizeInMB <= 0f) return;

            float targetSizeToFree = currentSizeInMB / 2f;
            float freedSize = 0f;

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
}