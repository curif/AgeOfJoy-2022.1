using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CacheStatusMonitor))]
public class CacheStatusMonitorEditor : Editor
{
    private string _status = "Press Refresh to query caches.";

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Refresh Cache Status"))
            _status = BuildStatus();

        EditorGUILayout.HelpBox(_status, MessageType.Info);
    }

    private string BuildStatus()
    {
        var sb = new System.Text.StringBuilder();

        AppendCache(sb, ConfigManager.CabinetCache);
        AppendCacheCounter(sb, ConfigManager.CabinetInformationCache);

        if (CabinetTextureCache.CachedTextures != null)
            AppendCache(sb, CabinetTextureCache.CachedTextures);
        else
            sb.AppendLine("TextureCache: not initialised yet");

        return sb.ToString();
    }

    private void AppendCache<K, V>(System.Text.StringBuilder sb, ResourceCache<K, V> cache)
    {
        if (cache == null) { sb.AppendLine("(null cache)"); return; }
        float pct = cache.MaxSizeInMB > 0f ? cache.CurrentSizeInMB / cache.MaxSizeInMB * 100f : 0f;
        sb.AppendLine($"{cache.CacheName}");
        sb.AppendLine($"  items : {cache.Count}");
        sb.AppendLine($"  used  : {cache.CurrentSizeInMB:F1} / {cache.MaxSizeInMB:F0} MB  ({pct:F0}%)");
    }

    // For caches that use a unit counter instead of real MB (each item costs 1 unit).
    private void AppendCacheCounter<K, V>(System.Text.StringBuilder sb, ResourceCache<K, V> cache)
    {
        if (cache == null) { sb.AppendLine("(null cache)"); return; }
        float pct = cache.MaxSizeInMB > 0f ? cache.CurrentSizeInMB / cache.MaxSizeInMB * 100f : 0f;
        sb.AppendLine($"{cache.CacheName}  (counter — not MB)");
        sb.AppendLine($"  items : {cache.Count}");
        sb.AppendLine($"  used  : {cache.CurrentSizeInMB:F0} / {cache.MaxSizeInMB:F0} units  ({pct:F0}%)");
    }
}
