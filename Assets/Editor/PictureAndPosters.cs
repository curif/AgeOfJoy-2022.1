using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Siccity.GLTFUtility.GLTFMaterial;

public class TextureToYamlGenerator : EditorWindow
{
    [MenuItem("Custom/Generate Picture and Poster yaml")]
    private static void ShowWindow()
    {
        GetWindow<TextureToYamlGenerator>("Register pictures and posters");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate YAML from pictures and posters Resources", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Picture YAML"))
        {
            GenerateYaml("Assets/Resources/Decoration/Pictures");
        }

        if (GUILayout.Button("Generate Poster YAML"))
        {
            GenerateYaml("Assets/Resources/Decoration/MoviePoster/Pictures/1");
            GenerateYaml("Assets/Resources/Decoration/MoviePoster/Pictures/18");
            GenerateYaml("Assets/Resources/Decoration/MoviePoster/Pictures/80");
            GenerateYaml("Assets/Resources/Decoration/MoviePoster/Pictures/90");
        }

    }

    private void GenerateYaml(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        List<PictureAndFrameTextureInfo> textureInfos = new List<PictureAndFrameTextureInfo>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            if (texture != null)
            {
                PictureAndFrameTextureInfo info = new PictureAndFrameTextureInfo
                {
                    Name = texture.name,
                    Width = texture.width,
                    Height = texture.height,
                    Format = texture.format.ToString()
                };

                textureInfos.Add(info);
            }
        }

        string yamlContent = SerializeToYaml(textureInfos);
        string outputPath = Path.Combine(folderPath, "contents.yaml");
        File.WriteAllText(outputPath, yamlContent);

        AssetDatabase.Refresh();
        Debug.Log($"YAML file generated at: {outputPath}");
    }

    private string SerializeToYaml(List<PictureAndFrameTextureInfo> textureInfos)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(new { textures = textureInfos });
    }
}