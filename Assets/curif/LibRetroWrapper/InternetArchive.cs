using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

// archive.org item (cabinet pack) reader. Rules: docs/sharing_cabinets_archive_org.md §9.
public enum IAStatus
{
    Loading,
    Ready,
    Processing,
    Dark,
    NotFound,
    Failed
}

public class IACabinet
{
    public string FileName;    // "galaga.zip"
    public string Name;        // "galaga"
    public long Size;
    public string Md5;
    public string Description; // "" if none
    public string Url;
}

public class IAItem
{
    public string Identifier;
    public IAStatus Status = IAStatus.Loading;
    public string Title = "";
    public string Description = "";
    public List<IACabinet> Cabinets = new List<IACabinet>();
    // storage servers that hold the item (d1, d2, server) and its folder (dir), used as fallback for downloads.
    public List<string> Servers = new List<string>();
    public string Dir = "";

    public IACabinet FindCabinet(string cab)
    {
        if (string.IsNullOrEmpty(cab))
            return null;
        string name = cab.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? cab.Substring(0, cab.Length - 4) : cab;
        return Cabinets.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}

public static class InternetArchive
{
    const string Host = "https://archive.org";

    // "ident", "https://archive.org/details/ident/...", ".../metadata/ident", ".../download/ident/file" -> "ident"
    public static string NormalizeIdentifier(string idOrUrl)
    {
        if (string.IsNullOrWhiteSpace(idOrUrl))
            return "";
        string s = idOrUrl.Trim();
        if (s.Contains("://") || s.StartsWith("archive.org", StringComparison.OrdinalIgnoreCase))
        {
            string noQuery = s.Split('?', '#')[0];
            string[] parts = noQuery.Split('/').Where(p => p.Length > 0).ToArray();
            int i = Array.FindIndex(parts, p => p == "details" || p == "metadata" || p == "download" || p == "embed");
            if (i < 0 || i + 1 >= parts.Length)
                return "";
            s = parts[i + 1];
        }
        return s.Trim('/').ToLowerInvariant();
    }

    // identifier and file name from an https://archive.org/download/<id>/<file> url. False if it is not one.
    public static bool TryParseDownloadUrl(string url, out string identifier, out string fileName)
    {
        identifier = fileName = "";
        if (string.IsNullOrEmpty(url))
            return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            return false;
        if (!uri.Host.Equals("archive.org", StringComparison.OrdinalIgnoreCase) &&
            !uri.Host.EndsWith(".archive.org", StringComparison.OrdinalIgnoreCase))
            return false;
        string[] parts = uri.AbsolutePath.Split('/').Where(p => p.Length > 0).ToArray();
        if (parts.Length < 3 || parts[0] != "download")
            return false;
        identifier = parts[1].ToLowerInvariant();
        fileName = Uri.UnescapeDataString(parts[parts.Length - 1]);
        return true;
    }

    // direct urls on the item's storage servers: https://<server><dir>/<file>
    public static IEnumerable<string> MirrorUrls(IAItem item, string file)
    {
        if (item == null || string.IsNullOrEmpty(item.Dir))
            yield break;
        foreach (string host in item.Servers)
            yield return $"https://{host}{item.Dir}/{Uri.EscapeDataString(file)}";
    }

    public static string MetadataUrl(string identifier) => $"{Host}/metadata/{Uri.EscapeDataString(identifier)}";

    public static string DownloadUrl(string identifier, string file) =>
        $"{Host}/download/{Uri.EscapeDataString(identifier)}/{Uri.EscapeDataString(file)}";

    // archive.org returns a single value as a string and repeated values as an array.
    static string AsText(JToken t)
    {
        if (t == null || t.Type == JTokenType.Null)
            return "";
        if (t is JArray arr)
            return string.Join("\n", arr.Select(AsText).Where(x => x.Length > 0));
        return t.ToString().Trim();
    }

    static JToken GetIgnoreCase(JObject obj, string key)
    {
        if (obj == null)
            return null;
        foreach (var p in obj.Properties())
            if (string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase))
                return p.Value;
        return null;
    }

    static long ParseLong(JToken t)
    {
        return long.TryParse(AsText(t), NumberStyles.Integer, CultureInfo.InvariantCulture, out long v) ? v : 0;
    }

    public static IAItem Parse(string identifier, string json)
    {
        var item = new IAItem { Identifier = identifier };

        JObject root;
        try
        {
            root = JObject.Parse(json);
        }
        catch (Exception)
        {
            item.Status = IAStatus.Failed;
            return item;
        }

        if (!root.HasValues)
        {
            item.Status = IAStatus.NotFound;
            return item;
        }

        var files = root["files"] as JArray;
        var meta = root["metadata"] as JObject;

        bool dark = root["is_dark"]?.Type == JTokenType.Boolean && root["is_dark"].Value<bool>();
        if (dark && (files == null || files.Count == 0) && meta == null)
        {
            item.Status = IAStatus.Dark;
            return item;
        }

        item.Dir = AsText(root["dir"]);
        foreach (string key in new[] { "d1", "d2", "server" })
        {
            string host = AsText(root[key]);
            if (host.Length > 0 && !item.Servers.Contains(host))
                item.Servers.Add(host);
        }

        item.Title = AsText(GetIgnoreCase(meta, "title"));
        item.Description = AsText(GetIgnoreCase(meta, "description"));

        var originals = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
        if (files != null)
        {
            foreach (var f in files.OfType<JObject>())
            {
                string name = AsText(f["name"]);
                if (name.Length == 0 || AsText(f["source"]) != "original")
                    continue;
                originals[name] = f;
            }
        }

        // archive.org's own files (_files.xml, _meta.xml, ...) and derivatives are never .zip originals.
        foreach (var kv in originals.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            string file = kv.Key;
            if (!file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                continue;
            if (file.IndexOf('/') >= 0) // only top-level cabinets
                continue;

            string baseName = file.Substring(0, file.Length - 4);
            JToken desc = GetIgnoreCase(meta, baseName) ?? GetIgnoreCase(meta, file);

            item.Cabinets.Add(new IACabinet
            {
                FileName = file,
                Name = baseName,
                Size = ParseLong(kv.Value["size"]),
                Md5 = AsText(kv.Value["md5"]).ToLowerInvariant(),
                Description = AsText(desc),
                Url = DownloadUrl(identifier, file)
            });
        }

        item.Status = root["pending_tasks"] != null ? IAStatus.Processing : IAStatus.Ready;
        return item;
    }
}
