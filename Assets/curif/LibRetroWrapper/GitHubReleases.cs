using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

// GitHub releases reader (public repos, no login). Used by the GH* AGEBasic functions (github.cs).
// Unauthenticated API limit: 60 requests per hour per IP.
public enum GHStatus
{
    Loading,
    Ready,
    NotFound,
    RateLimited,
    Failed
}

public class GHAsset
{
    public string Name;   // "AgeOfJoy.apk"
    public long Size;
    public string Url;    // browser_download_url, no login needed
    public string Sha256 = ""; // lowercase hex, "" if github didn't publish a digest
}

public class GHRelease
{
    public string Tag;
    public string Name = "";
    public string Date = ""; // published_at, ISO 8601 UTC
    public bool Prerelease;
    public string Notes = "";
    public List<GHAsset> Assets = new List<GHAsset>();

    public GHAsset FindAsset(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        return Assets.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    // first *.apk of the release, null if none.
    public GHAsset Apk => Assets.FirstOrDefault(a => a.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase));
}

public class GHProject
{
    public string Project; // "owner/repo"
    public GHStatus Status = GHStatus.Loading;
    public List<GHRelease> Releases = new List<GHRelease>(); // newest first

    public GHRelease FindRelease(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return null;
        return Releases.FirstOrDefault(r => string.Equals(r.Tag, tag, StringComparison.OrdinalIgnoreCase));
    }
}

public static class GitHubReleases
{
    public const int DefaultCount = 10;
    public const int MaxCount = 100; // github's per_page limit

    public static readonly Dictionary<string, string> ApiHeaders = new Dictionary<string, string>
    {
        { "Accept", "application/vnd.github+json" },
        { "X-GitHub-Api-Version", "2022-11-28" }
    };

    static readonly Regex NamePart = new Regex(@"^[A-Za-z0-9._-]+$");

    // "owner/repo", "github.com/owner/repo", "https://github.com/owner/repo/releases/..." -> "owner/repo". "" if invalid.
    public static string NormalizeProject(string projectOrUrl)
    {
        if (string.IsNullOrWhiteSpace(projectOrUrl))
            return "";
        string s = projectOrUrl.Trim().Split('?', '#')[0];
        int scheme = s.IndexOf("://", StringComparison.Ordinal);
        if (scheme >= 0)
            s = s.Substring(scheme + 3);
        string[] parts = s.Split('/').Where(p => p.Length > 0).ToArray();
        if (parts.Length > 0 && parts[0].EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
            parts = parts.Skip(1).ToArray();
        if (parts.Length < 2)
            return "";
        string owner = parts[0];
        string repo = parts[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? parts[1].Substring(0, parts[1].Length - 4) : parts[1];
        if (!NamePart.IsMatch(owner) || !NamePart.IsMatch(repo))
            return "";
        return owner + "/" + repo;
    }

    public static string ReleasesUrl(string project, int count) =>
        $"https://api.github.com/repos/{project}/releases?per_page={count}";

    public static string RepoName(string project) => project.Substring(project.IndexOf('/') + 1);

    static string AsText(JToken t) => t == null || t.Type == JTokenType.Null ? "" : t.ToString().Trim();

    public static GHProject Parse(string project, string json)
    {
        var result = new GHProject { Project = project };
        try
        {
            if (!(JToken.Parse(json) is JArray releases))
            {
                result.Status = GHStatus.Failed;
                return result;
            }
            foreach (var r in releases.OfType<JObject>())
            {
                if (r["draft"]?.Type == JTokenType.Boolean && (bool)r["draft"])
                    continue;
                string tag = AsText(r["tag_name"]);
                if (tag.Length == 0)
                    continue;
                var release = new GHRelease
                {
                    Tag = tag,
                    Name = AsText(r["name"]),
                    Date = r["published_at"]?.Type == JTokenType.Date
                        ? ((DateTime)r["published_at"]).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
                        : AsText(r["published_at"]),
                    Prerelease = r["prerelease"]?.Type == JTokenType.Boolean && (bool)r["prerelease"],
                    Notes = AsText(r["body"])
                };
                if (r["assets"] is JArray assets)
                {
                    foreach (var a in assets.OfType<JObject>())
                    {
                        string digest = AsText(a["digest"]);
                        release.Assets.Add(new GHAsset
                        {
                            Name = AsText(a["name"]),
                            Size = long.TryParse(AsText(a["size"]), NumberStyles.Integer, CultureInfo.InvariantCulture, out long size) ? size : 0,
                            Url = AsText(a["browser_download_url"]),
                            Sha256 = digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) ? digest.Substring(7).ToLowerInvariant() : ""
                        });
                    }
                }
                result.Releases.Add(release);
            }
            // ISO 8601 UTC sorts as text.
            result.Releases = result.Releases.OrderByDescending(r => r.Date, StringComparer.Ordinal).ToList();
            result.Status = GHStatus.Ready;
        }
        catch (Exception)
        {
            result.Status = GHStatus.Failed;
        }
        return result;
    }

    // "v0.5.0-rc31" and "0.5.0-rc31" are the same version.
    public static bool SameVersion(string tag, string version) =>
        string.Equals(tag.TrimStart('v', 'V'), (version ?? "").TrimStart('v', 'V'), StringComparison.OrdinalIgnoreCase);
}
