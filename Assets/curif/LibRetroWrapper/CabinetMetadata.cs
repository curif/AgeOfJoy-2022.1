/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;

public class CabinetMetadata
{
    [YamlMember(Alias = "hashes", ApplyNamingConventions = false)]
    public Dictionary<string, string> Hashes { get; private set; }

    // ISO-8601 UTC last-write time of each GLB file at the time the hash was computed.
    // Cheap staleness check: if the timestamp matches, skip re-hashing.
    [YamlMember(Alias = "modified_at", ApplyNamingConventions = false)]
    public Dictionary<string, string> ModifiedAt { get; private set; }

    // Actual in-memory size (MB) of each loaded GLB, keyed by the file's MD5 hash.
    // Keyed by hash so a GLB upgrade automatically invalidates the entry.
    [YamlMember(Alias = "sizes_mb", ApplyNamingConventions = false)]
    public Dictionary<string, float> SizesMb { get; private set; }

    private static IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
    private static ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    public CabinetMetadata() { }

    public string getHash(string file)
    {
        var hashedFile = file.ToLower();
        return Hashes != null && Hashes.ContainsKey(hashedFile) ? Hashes[hashedFile] : null;
    }

    public string getModifiedAt(string filename)
    {
        if (ModifiedAt == null) return null;
        string key = filename.ToLower();
        return ModifiedAt.ContainsKey(key) ? ModifiedAt[key] : null;
    }

    public void setModifiedAt(string filename, DateTime utc)
    {
        if (ModifiedAt == null) ModifiedAt = new Dictionary<string, string>();
        ModifiedAt[filename.ToLower()] = utc.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    public float getSize(string hash)
    {
        if (SizesMb == null || !SizesMb.ContainsKey(hash)) return -1f;
        return SizesMb[hash];
    }

    public void setSize(string hash, float mb)
    {
        if (SizesMb == null) SizesMb = new Dictionary<string, float>();
        SizesMb[hash] = mb;
    }

    public void save(string cabPath)
    {
        toYaml(cabPath, this);
    }

    // Verifies the stored hash is still valid by comparing file modification timestamps.
    // Only re-hashes when the file has actually changed, keeping MD5 computation rare.
    // Returns true if the hash was valid (timestamps matched), false if it was refreshed.
    public bool verifyAndRefreshHash(string cabPath, string filename)
    {
        string lowerFilename = filename.ToLower();
        string filePath = Path.Combine(cabPath, filename);

        DateTime currentModTime = File.GetLastWriteTimeUtc(filePath);
        string currentModTimeStr = currentModTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string storedModTime = getModifiedAt(lowerFilename);

        if (storedModTime != null && currentModTimeStr == storedModTime)
            return true; // Timestamps match — hash presumed valid

        // File changed or no stored timestamp — re-compute MD5
        string oldHash = getHash(lowerFilename);
        string newHash = computeHash(filePath);

        if (Hashes == null) Hashes = new Dictionary<string, string>();
        Hashes[lowerFilename] = newHash;
        setModifiedAt(lowerFilename, currentModTime);

        // Clear stale size entry so it will be recalculated on next load
        if (oldHash != null && oldHash != newHash)
            SizesMb?.Remove(oldHash);

        save(cabPath);
        ConfigManager.WriteConsole($"[CabinetMetadata.verifyAndRefreshHash] {filename}: hash refreshed (old:{oldHash} new:{newHash})");
        return false;
    }

    public static CabinetMetadata fromName(string cabName)
    {
        CabinetMetadata metadata = fromYaml(ConfigManager.CabinetsDB + "/" + cabName);
        return metadata != null ? metadata : init(cabName);
    }

    private static CabinetMetadata init(string cabName)
    {
        CabinetMetadata metadata = new CabinetMetadata();
        metadata.Hashes = new Dictionary<string, string>();
        metadata.ModifiedAt = new Dictionary<string, string>();
        metadata.SizesMb = new Dictionary<string, float>();

        string cabPath = ConfigManager.CabinetsDB + "/" + cabName;
        string[] files = Directory.GetFiles(cabPath, "*.glb");
        foreach (string file in files)
        {
            string hash = computeHash(file);
            string glbFile = Path.GetFileName(file).ToLower();
            DateTime modifiedAt = File.GetLastWriteTimeUtc(file);
            ConfigManager.WriteConsole($"[CabinetMetadata.init]: {glbFile}:{hash}");
            metadata.Hashes.Add(glbFile, hash);
            metadata.ModifiedAt.Add(glbFile, modifiedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        toYaml(cabPath, metadata);

        return metadata;
    }

    private static void toYaml(string cabPath, CabinetMetadata metadata)
    {
        string yamlPath = Path.Combine(cabPath, "metadata.yaml");
        ConfigManager.WriteConsole($"[CabinetMetadata]: save to Yaml: {yamlPath}");
        string yaml = serializer.Serialize(metadata);
        File.WriteAllText(yamlPath, yaml);
    }

    private static CabinetMetadata fromYaml(string cabPath)
    {
        string yamlPath = Path.Combine(cabPath, "metadata.yaml");
        ConfigManager.WriteConsole($"[CabinetMetadata]: load from Yaml: {yamlPath}");
        if (!File.Exists(yamlPath))
        {
            return null;
        }
        
        string yaml = yamlFileToString(yamlPath);
        return parseYaml(yamlPath, yaml);
    }

    private static string yamlFileToString(string yamlPath)
    {
        string yaml;
        try
        {
            StreamReader input = File.OpenText(yamlPath);
            yaml = input.ReadToEnd();
            input.Close();
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetMetadata.fromYaml] YAML file {yamlPath} ", e);
            return null;
        }

        return yaml;
    }

    private static CabinetMetadata parseYaml(string yamlPath, string yaml)
    {
        try
        {
            //ConfigManager.WriteConsole($"[CabinetInformation]: {yamlPath} \n {yaml}");
            var cabMetadata = deserializer.Deserialize<CabinetMetadata>(yaml);
            if (cabMetadata == null)
                throw new IOException();
            return cabMetadata;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[CabinetMetadata.parseYaml] Metadata YAML file in cabinet {yamlPath} ", e);
            return null;
        }
    }

    private static string computeHash(string file)
    {
        byte[] fileBytes = File.ReadAllBytes(file);
        return computeMD5(fileBytes);
    }

    private static string computeMD5(byte[] inputBytes)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
