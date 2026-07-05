/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Magazine issue metadata from ordered page images in the issue folder.
/// Optional <c>magazine.yaml</c> overrides inferred covers when present.
/// </summary>
[Serializable]
public class MRMagazineIssueDefinition
{
    public const int SupportedYamlVersion = 1;

    public int Version = 1;
    public string DisplayName;
    public MRMagazineCoversYaml Covers;

    public string IssueName { get; private set; }
    public string IssueDir { get; private set; }

    public string GetFrontCover() => Covers?.Front;
    public string GetInsideFrontCover() => Covers?.InsideFront;
    public string GetInsideBackCover() => Covers?.InsideBack;
    public string GetBackCover() => Covers?.Back;

    public string GetDisplayName() => IssueName;

    /// <summary>Loads yaml when present, otherwise infers covers from sorted page images.</summary>
    public static bool TryResolve(string issueName, out MRMagazineIssueDefinition definition)
    {
        definition = null;
        string issueDir = MRPaths.GetMagazineIssueDir(issueName);
        if (string.IsNullOrEmpty(issueDir) || !Directory.Exists(issueDir))
            return false;

        return TryResolveFromDir(issueName, issueDir, out definition);
    }

    /// <summary>Legacy alias — prefer <see cref="TryResolve"/>.</summary>
    public static bool TryLoad(string issueName, out MRMagazineIssueDefinition definition) =>
        TryResolve(issueName, out definition);

    /// <summary>Legacy alias — prefer <see cref="TryResolveFromDir"/>.</summary>
    public static bool TryLoadFromDir(string issueName, string issueDir, out MRMagazineIssueDefinition definition) =>
        TryResolveFromDir(issueName, issueDir, out definition);

    public static bool TryResolveFromDir(string issueName, string issueDir, out MRMagazineIssueDefinition definition)
    {
        definition = null;
        if (string.IsNullOrEmpty(issueDir) || !Directory.Exists(issueDir))
            return false;

        string yamlPath = Path.Combine(issueDir, MRPaths.MagazineYamlFileName);
        if (File.Exists(yamlPath) && TryParseYamlFile(issueName, issueDir, yamlPath, out definition))
            return true;

        List<string> pages = MRMagazineCatalog.CollectOrderedPageFileNames(issueDir);
        if (pages.Count == 0)
            return false;

        if (!MRMagazineCatalog.TryInferCoverFileNames(
                pages,
                out string front,
                out string insideFront,
                out string insideBack,
                out string back))
        {
            return false;
        }

        definition = CreateFromCovers(issueName, issueDir, front, insideFront, insideBack, back);
        return true;
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

    static bool TryParseYamlFile(
        string issueName,
        string issueDir,
        string yamlPath,
        out MRMagazineIssueDefinition definition)
    {
        definition = null;

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

            if (definition.Covers == null)
            {
                List<string> pages = MRMagazineCatalog.CollectOrderedPageFileNames(issueDir);
                if (pages.Count == 0
                    || !MRMagazineCatalog.TryInferCoverFileNames(
                        pages,
                        out string front,
                        out string insideFront,
                        out string insideBack,
                        out string back))
                {
                    return false;
                }

                definition.Covers = new MRMagazineCoversYaml
                {
                    Front = front,
                    InsideFront = insideFront,
                    InsideBack = insideBack,
                    Back = back
                };
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

    static MRMagazineIssueDefinition CreateFromCovers(
        string issueName,
        string issueDir,
        string front,
        string insideFront,
        string insideBack,
        string back)
    {
        return new MRMagazineIssueDefinition
        {
            Version = 1,
            IssueName = issueName,
            IssueDir = issueDir,
            Covers = new MRMagazineCoversYaml
            {
                Front = front,
                InsideFront = insideFront,
                InsideBack = insideBack,
                Back = back
            }
        };
    }
}

[Serializable]
public class MRMagazineCoversYaml
{
    public string Front;
    public string InsideFront;
    public string InsideBack;
    public string Back;
}
