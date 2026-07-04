using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceCache
{
    void FreeResources();
    void Status();
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
        ConfigManager.WriteConsole("[ResourceCacheManager] ==== FreeResources triggered ====");
        LogAllCacheStatus("BEFORE");

        FreeResources();

        System.GC.Collect();
        yield return null;
        AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
        while (!unloadOp.isDone)
            yield return null;

        LogAllCacheStatus("AFTER");
        ConfigManager.WriteConsole("[ResourceCacheManager] Memory reclaimed successfully.");
    }

    public static void LogAllCacheStatus(string label)
    {
        ConfigManager.WriteConsole($"[ResourceCacheManager] ---- cache status ({label}) ----");
        foreach (IResourceCache cache in caches)
            cache.Status();
    }
}

public class ResourceCache<K, V> : IResourceCache
{
    private readonly float maxSizeInMB; // Maximum size in megabytes
    private float currentSizeInMB; // Current total size in megabytes
    private Dictionary<K, V> cache = new Dictionary<K, V>();
    private Dictionary<K, float> sizeMap = new Dictionary<K, float>(); // Tracks size of each item
    private LinkedList<K> lruList = new LinkedList<K>(); // Tracks usage order
    private Dictionary<K, int> pinCount = new Dictionary<K, int>(); // Ref-counts entries currently in active use
    string Name;
    private readonly object locker = new object();

    internal ResourceCache(string name, float maxSizeInMB = 512f)
    {
        this.maxSizeInMB = maxSizeInMB;
        this.currentSizeInMB = 0f;
        this.Name = name;
    }

    public string CacheName => Name;
    public float CurrentSizeInMB => currentSizeInMB;
    public float MaxSizeInMB => maxSizeInMB;
    public int Count => lruList.Count;

    // Frees every UNPINNED entry (e.g. in response to the OS-level low-memory
    // callback). Pinned entries (textures still bound to a live, on-screen
    // material) are intentionally left untouched - destroying those is exactly
    // what caused cabinets to go dark under memory pressure.
    public void FreeResources()
    {
        lock (locker)
        {
            int totalCountBefore = lruList.Count;
            float totalSizeBefore = currentSizeInMB;
            int freedCount = 0;
            int pinnedSkipped = 0;

            LinkedListNode<K> node = lruList.Last;
            while (node != null)
            {
                LinkedListNode<K> prev = node.Previous;
                K key = node.Value;

                if (IsPinned(key))
                {
                    pinnedSkipped++;
                }
                else
                {
                    DestroyIfUnityObject(cache[key]);
                    currentSizeInMB -= sizeMap[key];
                    cache.Remove(key);
                    sizeMap.Remove(key);
                    lruList.Remove(node);
                    freedCount++;
                }

                node = prev;
            }

            ConfigManager.WriteConsole($"[ResourceCacheManager] {Name}: FreeResources freed {freedCount}/{totalCountBefore} entries ({totalSizeBefore - currentSizeInMB:F2}MB freed). Kept {pinnedSkipped} pinned entries in use ({currentSizeInMB:F2}MB remaining).");
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

    // Marks an entry as actively in use, protecting it from LRU eviction. Ref-counted:
    // callers must pair every Pin() with an Unpin() when they stop using the entry.
    public void Pin(K key)
    {
        if (key == null) return;

        lock (locker)
        {
            if (!cache.ContainsKey(key)) return;
            pinCount.TryGetValue(key, out int c);
            pinCount[key] = c + 1;
            ConfigManager.WriteConsole($"[ResourceCacheManager] {Name}: Pin({key}) -> refcount {c + 1}");
        }
    }

    public void Unpin(K key)
    {
        if (key == null) return;

        lock (locker)
        {
            if (!pinCount.TryGetValue(key, out int c)) return;
            if (c <= 1)
            {
                pinCount.Remove(key);
                ConfigManager.WriteConsole($"[ResourceCacheManager] {Name}: Unpin({key}) -> refcount 0 (unpinned)");
            }
            else
            {
                pinCount[key] = c - 1;
                ConfigManager.WriteConsole($"[ResourceCacheManager] {Name}: Unpin({key}) -> refcount {c - 1}");
            }
        }
    }

    private bool IsPinned(K key) => pinCount.TryGetValue(key, out int c) && c > 0;
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
                        // NOTE: this does not guard existingValue against being destroyed while
                        // pinned (see step 1 above) - no current caller passes replaceIfExists:
                        // true, so this is a known gap rather than an active bug.
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
        bool loggedAllPinned = false;

        // 1. Loop and Evict
        while (CacheExceeded(sizeInMB))
        {
            // Walk from least-recently-used towards most-recently-used, skipping
            // any key that is currently pinned (i.e. still actively displayed).
            LinkedListNode<K> victim = lruList.Last;
            while (victim != null && IsPinned(victim.Value))
                victim = victim.Previous;

            if (victim == null)
            {
                // Every entry is pinned - nothing can be evicted right now.
                // Better to run over budget than to destroy a texture in active use.
                if (!loggedAllPinned)
                {
                    ConfigManager.WriteConsoleWarning($"[ResourceCacheManager] {Name}: cannot free space, all {lruList.Count} entries are pinned. Over budget: {currentSizeInMB}MB / {maxSizeInMB}MB.");
                    loggedAllPinned = true;
                }
                break;
            }

            K lruKey = victim.Value;
            lruList.Remove(victim);

            V value = cache[lruKey];
            DestroyIfUnityObject(value);

            currentSizeInMB -= sizeMap[lruKey];
            cache.Remove(lruKey);
            sizeMap.Remove(lruKey);
            pinCount.Remove(lruKey);
            evictedAny = true;
        }

        // 2. Performance Tip: Only log once, not inside the while loop
        if (evictedAny)
        {
            // Log once at the end to prevent string allocation spam
            ConfigManager.WriteConsole($"[ResourceCacheManager] Trimmed cache for {sizeInMB}MB. New Total: {currentSizeInMB}MB");
        }
    }

    public void Status()
    {
        lock (locker)
        {
            float pct = maxSizeInMB > 0f ? (currentSizeInMB / maxSizeInMB * 100f) : 0f;
            ConfigManager.WriteConsole($"[ResourceCacheManager] {this.Name} | size: {currentSizeInMB:F2}MB / {maxSizeInMB:F2}MB ({pct:F1}%) | count: {lruList.Count} | pinned: {pinCount.Count}");
        }
    }

    // Explicit invalidation (e.g. artwork file changed on disk). Unlike automatic
    // eviction, this refuses to destroy a pinned entry - it is still bound to a
    // live material and destroying it would leave that material dark.
    public void Remove(K key)
    {
        if (key == null) return;

        lock (locker)
        {
            if (!cache.ContainsKey(key)) return;

            if (IsPinned(key))
            {
                ConfigManager.WriteConsoleWarning($"[ResourceCacheManager] {Name}: Remove({key}) skipped, entry is pinned (refcount {pinCount[key]}) and still in use.");
                return;
            }

            lruList.Remove(key);
            DestroyIfUnityObject(cache[key]);
            currentSizeInMB -= sizeMap[key];
            cache.Remove(key);
            sizeMap.Remove(key);
            pinCount.Remove(key);
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
            pinCount.Clear();
            currentSizeInMB = 0f;
        }

        ConfigManager.WriteConsole($"[ResourceCacheManager] {Name} cleared.");
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
            bool loggedAllPinned = false;

            while (freedSize < targetSizeToFree && lruList.Count > 0)
            {
                LinkedListNode<K> victim = lruList.Last;
                while (victim != null && IsPinned(victim.Value))
                    victim = victim.Previous;

                if (victim == null)
                {
                    if (!loggedAllPinned)
                    {
                        ConfigManager.WriteConsoleWarning($"[ResourceCacheManager] {Name}: FreeHalfResources cannot free more, remaining entries are pinned.");
                        loggedAllPinned = true;
                    }
                    break;
                }

                K lruKey = victim.Value;
                float itemSize = sizeMap[lruKey];

                lruList.Remove(victim);
                DestroyIfUnityObject(cache[lruKey]);
                cache.Remove(lruKey);
                sizeMap.Remove(lruKey);
                pinCount.Remove(lruKey);

                currentSizeInMB -= itemSize;
                freedSize += itemSize;
            }
        }
    }
}