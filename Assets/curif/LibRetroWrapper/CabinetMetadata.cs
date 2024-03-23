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
        return Hashes.ContainsKey(hashedFile) ? Hashes[hashedFile] : null;
    }

    public static CabinetMetadata fromName(string cabName)
    {
        CabinetMetadata metadata = fromYaml(ConfigManager.CabinetsDB + "/" + cabName);
        return metadata != null ? metadata : init(cabName);
    }

    private static CabinetMetadata init(string cabName)
    {
        CabinetMetadata metadata = new CabinetMetadata();
        Dictionary<string, string> hashes = new Dictionary<string, string>();
        metadata.Hashes = hashes;

        string cabPath = ConfigManager.CabinetsDB + "/" + cabName;
        string[] files = Directory.GetFiles(cabPath, "*.glb");
        foreach (string file in files)
        {
            string hash = computeHash(file);
            string glbFile = Path.GetFileName(file);
            ConfigManager.WriteConsole($"[CabinetMetadata.init]: {glbFile}:{hash}");
            hashes.Add(glbFile.ToLower(), hash);
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
