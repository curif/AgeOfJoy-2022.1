/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;
using System.Linq;
using System.Reflection;

public class ConfigInformationBase
{
  public virtual bool IsValid()
  {
    return true;
  }
}
public class ConfigInformation
{
  public NPC npc;
  public Audio audio;

  public class NPC : ConfigInformationBase
  {
    private string[] validStatus = new string[] {"enabled", "static", "disabled"};
    public string status;

    public override bool IsValid()
    {
      return validStatus.Contains(status);
    }
  }

  //background audio
  public class Background : ConfigInformationBase
  { 
    [YamlMember(Alias = "volume-percent", ApplyNamingConventions = false)]
    public uint? volume;
    public bool? muted;

    public override bool IsValid()
    {
      return volume <= 100 && volume >= 0;
    }
  }

  //global audio config
  public class Audio : ConfigInformationBase
  {
    public Background background;
    [YamlMember(Alias = "in-game-background", ApplyNamingConventions = false)]
    public Background inGameBackground;

    public override bool IsValid()
    {
      if (background != null && !background.IsValid())
        return false;
      if (inGameBackground != null && !inGameBackground.IsValid())
        return false;

      return true;
    }
  }

  // defaults ===================================================
  public static Background BackgroundInGameDefault()
  {
    Background bg = new();
    bg.volume = 20;
    bg.muted = false;
    return bg;
  }
  public static Background BackgroundDefault()
  {
    Background bg = new();
    bg.volume = 70;
    bg.muted = false;
    return bg;
  }
  public static ConfigInformation newDefault()
  {
    ConfigInformation configuration = new();
    configuration.audio = new();
    configuration.audio.background = BackgroundDefault();
    configuration.audio.inGameBackground = BackgroundInGameDefault();

    configuration.npc = new();
    configuration.npc.status = "enabled";
    return configuration;
  }
  // =============================================================

  public static ConfigInformation fromYaml(string yamlPath)
  {
    ConfigManager.WriteConsole($"[ConfigInformation]: {yamlPath}");

    if (!File.Exists(yamlPath))
    {
      ConfigManager.WriteConsole($"[ConfigInformation]: ERROR YAML file ({yamlPath}) doesn't exists ");
      return null;
    }

    try
    {
      var input = File.OpenText(yamlPath);

      var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .IgnoreUnmatchedProperties()
          .Build();

      var configInfo = deserializer.Deserialize<ConfigInformation>(input);
      if (configInfo == null)
        throw new IOException("Deserialization error");

      configInfo.validate();

      return configInfo;
    }
    catch (Exception e)
    {
      ConfigManager.WriteConsole($"[ConfigInformation]:ERROR reading configuration YAML file {yamlPath} - {e}");
      return null;
    }
    return null;
  }

  public bool toYaml(string yamlPath)
  {
    try
    {
      var serializer = new SerializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();
      var yaml = serializer.Serialize(this);
      File.WriteAllText(yamlPath, yaml);
    }
    catch (Exception e)
    {
      ConfigManager.WriteConsole($"[ConfigInformation]:ERROR configuration YAML file in configuration subdir {yamlPath} - {e}");
      return false;
    }
    return true;
  } 

  public override string ToString()
  {
    string ret = "Configuration \n";
    ret += "Audio \n";
    ret += "----- \n";
    ret += $" \t background: {audio?.background?.volume}\n";
    ret += "NPCs \n";
    ret += "---- \n";
    ret += $" \t status: {npc?.status}\n";
    ret += " \n";
    ret += " \n";
    ret += " \n";
    ret += " \n";
    ret += " \n";
    return ret;
  }

  private void validate()
  {
    if (npc != null)
      if (!npc.IsValid())
        throw new Exception("Invalid NPC configuration");
    
    if (audio != null)
      if (!audio.IsValid())
        throw new Exception("Invalid audio settings");
  }

  private static T change<T>(T obj1, T obj2) where T: class,new()
  {
    if (obj1 == null && obj2 != null) return obj2;
    if (obj1 != null && obj2 == null) return obj1;
    return new T(); //and change internal properties 
  }
  private static T returnsNotNullOrSecond<T>(T obj1, T obj2)
  {
    if (obj1 == null && obj2 != null) return obj2;
    if (obj1 != null && obj2 == null) return obj1;
    return obj2;
  }
  public static ConfigInformation Merge(ConfigInformation ci1, ConfigInformation ci2)
  {
    if (ci1 == null && ci2 != null) return ci2;
    if (ci1 != null && ci2 == null) return ci1;

    ConfigInformation ret = new();

    ret.audio = change<Audio>(ci1.audio, ci2.audio);
    if (ret.audio != ci2.audio)
    {
      ret.audio.background = change<Background>(ci1.audio?.background, ci2.audio?.background);
      if (ret.audio.background != ci2.audio?.background)
      {
        ret.audio.background.volume = returnsNotNullOrSecond<uint?>(ci1.audio.background?.volume, ci2.audio.background?.volume);
      }
    }

    ret.npc = change<NPC>(ci1.npc, ci2.npc);
    if (ret.npc != ci2.npc)
    {
      ret.npc.status = returnsNotNullOrSecond(ci1.npc?.status, ci2.npc?.status);
    }

    return ret;
  }
}

