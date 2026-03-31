using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceCache
{
    void FreeResources();
}

public class ResourceCacheManager
{
    private static int mainThreadId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

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

    public static IEnumerator FreeResourcesAsync()
    {
        FreeResources();
        System.GC.Collect();
        yield return null;
        AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
        while (!unloadOp.isDone)
            yield return null;
        ConfigManager.WriteConsole("[ResourceCacheManager] Memory reclaimed successfully.");
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
            if (cache.TryGetValue(key, out V existingValue))
            {
                if (replaceIfExists)
                {
                    // 1. Clean up the old Unity Object immediately
                    DestroyIfUnityObject(existingValue);

                    float oldSize = sizeMap[key];
                    currentSizeInMB -= oldSize;

                    // 2. Update with new data
                    currentSizeInMB += sizeInMB;
                    cache[key] = value;
                    sizeMap[key] = sizeInMB;

                    lruList.Remove(key);
                    lruList.AddFirst(key);

                    // 3. CORRECT LOGIC: If we grew, we might need to shrink
                    if (sizeInMB > oldSize)
                    {
                        // Pass 0 because currentSizeInMB is already updated
                        makeSpaceFor(0);
                    }
                }
                return cache[key];
            }
            else
            {
                // NEW ITEM
                // 1. Make space BEFORE adding to prevent a "Peak" that hits 5GB
                makeSpaceFor(sizeInMB);

                // 2. Add new item
                cache.Add(key, value);
                sizeMap.Add(key, sizeInMB);
                lruList.AddFirst(key);
                currentSizeInMB += sizeInMB;

                // 3. Optimized Logging: Only log if actually adding 
                // (Move Status() calls here or make them optional)
                return value;
            }
        }
    }

    private void makeSpaceFor(float sizeInMB)
    {
        bool evictedAny = false;

        // 1. Loop and Evict
        while (CacheExceeded(sizeInMB))
        {
            K lruKey = lruList.Last.Value;
            lruList.RemoveLast();

            V value = cache[lruKey];
            DestroyIfUnityObject(value);

            currentSizeInMB -= sizeMap[lruKey];
            cache.Remove(lruKey);
            sizeMap.Remove(lruKey);
            evictedAny = true;
        }

        // 2. Performance Tip: Only log once, not inside the while loop
        if (evictedAny)
        {
            // Log once at the end to prevent string allocation spam
            ConfigManager.WriteConsole($"[Cache] Trimmed cache for {sizeInMB}MB. New Total: {currentSizeInMB}MB");
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

    /*
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
    */
    public void Clear()
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

        ConfigManager.WriteConsole($"[Cache] {Name} cleared.");
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
            if (ResourceCacheManager.IsMainThread)
            {
                UnityEngine.Object.DestroyImmediate(unityObj, true);
            }
            else
            {
                // If we are on a background thread, we CANNOT use DestroyImmediate.
                // We use Destroy, which Unity internally queues for the next main-thread frame.
                UnityEngine.Object.Destroy(unityObj);

                // Optional: Log a warning because Destroy is slower than DestroyImmediate
                // and might cause a tiny memory peak.
                // ConfigManager.WriteConsoleWarning("[Cache] Background thread destruction detected.");
            }
        }
    }

    /*
    private void DestroyIfUnityObject(V value)
    {
        if (value is UnityEngine.Object unityObj && unityObj != null)
        {
            if (!unityObj.Equals(null))
            {
                // DestroyImmediate is dangerous in play mode and recommended only in Editor scripts.
                // On Meta Quest, you want the safe version(Destroy), which schedules destruction properly for next frame.
                ConfigManager.WriteConsole($"[ResourceCache] destroying {unityObj}");

                //UnityEngine.Object.DestroyImmediate(unityObj);
                UnityEngine.Object.Destroy(unityObj);
            }
        }
    }
    */
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