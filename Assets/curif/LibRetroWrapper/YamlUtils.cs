using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlUtils
{

    private static readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    private static string ReadFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            ConfigManager.WriteConsole($"[YamlUtils]: {filePath} does not exist");
            return null;
        }
        using StreamReader input = File.OpenText(filePath);
        return input.ReadToEnd();
    }

    public static T Parse<T>(string yamlPath)
    {
        T result = ParseOptional<T>(yamlPath);
        return result == null ? throw new IOException() : result;
    }

    public static T ParseOptional<T>(string yamlPath)
    {
        string content = ReadFile(yamlPath);
        return content == null ? default : deserializer.Deserialize<T>(content);
    }

}
