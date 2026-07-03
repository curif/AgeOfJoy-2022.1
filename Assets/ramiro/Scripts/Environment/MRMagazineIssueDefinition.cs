/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>Parsed <c>MR/Magazines/&lt;Issue&gt;/magazine.yaml</c> — schema v1.</summary>
[Serializable]
public class MRMagazineIssueDefinition
{
    public const int SupportedYamlVersion = 1;

    public const string DefaultFrontCover = "1.jpg";
    public const string DefaultInsideFrontCover = "2.jpg";
    public const string DefaultInsideBackCover = "75.jpg";
    public const string DefaultBackCover = "76.jpg";

    public int Version = 1;
    public string DisplayName;
    public MRMagazineCoversYaml Covers;

    public string IssueName { get; private set; }
    public string IssueDir { get; private set; }

    public string GetFrontCover() => FirstNonEmpty(Covers?.Front, DefaultFrontCover);
    public string GetInsideFrontCover() => FirstNonEmpty(Covers?.InsideFront, DefaultInsideFrontCover);
    public string GetInsideBackCover() => FirstNonEmpty(Covers?.InsideBack, DefaultInsideBackCover);
    public string GetBackCover() => FirstNonEmpty(Covers?.Back, DefaultBackCover);

    public string GetDisplayName() =>
        FirstNonEmpty(DisplayName, IssueName);

    public static bool TryLoad(string issueName, out MRMagazineIssueDefinition definition)
    {
        definition = null;
        string issueDir = MRPaths.GetMagazineIssueDir(issueName);
        if (string.IsNullOrEmpty(issueDir) || !Directory.Exists(issueDir))
            return false;

        return TryLoadFromDir(issueName, issueDir, out definition);
    }

    public static bool TryLoadFromDir(string issueName, string issueDir, out MRMagazineIssueDefinition definition)
    {
        definition = null;
        if (string.IsNullOrEmpty(issueDir) || !Directory.Exists(issueDir))
            return false;

        string yamlPath = Path.Combine(issueDir, MRPaths.MagazineYamlFileName);
        if (!File.Exists(yamlPath))
        {
            ConfigManager.WriteConsoleWarning(
                $"[MRMagazineIssueDefinition] {issueName}: {MRPaths.MagazineYamlFileName} not found in {issueDir}");
            return false;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            definition = deserializer.Deserialize<MRMagazineIssueDefinition>(File.ReadAllText(yamlPath));
            if (definition == null)
                return false;

            definition.IssueName = issueName;
            definition.IssueDir = issueDir;

            if (definition.Version <= 0)
                definition.Version = 1;

            if (definition.Version > SupportedYamlVersion)
            {
                ConfigManager.WriteConsoleWarning(
                    $"[MRMagazineIssueDefinition] {issueName}: version {definition.Version} > supported {SupportedYamlVersion}");
            }

            return true;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[MRMagazineIssueDefinition] failed to parse {yamlPath}", e);
            definition = null;
            return false;
        }
    }

    public void ApplyTo(Magazine magazine)
    {
        if (magazine == null)
            return;

        magazine.frontCoverImgName = GetFrontCover();
        magazine.insideFrontCoverImgName = GetInsideFrontCover();
        magazine.insideBackCoverImgName = GetInsideBackCover();
        magazine.backCoverImgName = GetBackCover();
    }

    static string FirstNonEmpty(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}

[Serializable]
public class MRMagazineCoversYaml
{
    public string Front;
    public string InsideFront;
    public string InsideBack;
    public string Back;
}
