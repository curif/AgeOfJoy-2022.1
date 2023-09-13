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
    public LocomotionConfiguration locomotion;
    public Player player;

    /** don't. create an empty object. Merge didn't works if both are loaded.
    public ConfigInformation() {
      audio = new Audio();
      audio.background = BackgroundDefault();
      audio.inGameBackground = BackgroundInGameDefault();

      npc = new NPC();
      npc.status = "enabled";
    }
    */

    public class NPC : ConfigInformationBase
    {
        public static string[] validStatus = new string[] { "enabled", "static", "disabled" };
        public string status;

        public override bool IsValid()
        {
            return status == null || validStatus.Contains(status);
        }
    }

    public class Player : ConfigInformationBase
    {
        public static Dictionary<string, float> heightPlayers = new Dictionary<string, float>
        {
            {"Pac-man", 1f}, // 1 is a kid
            {"Sonic", 1.05f}, // 0.05 step
            {"Pikachu", 1.1f}, // 0.05 step
            {"Mario", 1.15f}, // 0.05 step
            {"Luigi", 1.2f}, // 0.05 step
            {"Final Fantasy", 1.25f}, // 0.05 step
            {"Megaman", 1.3f}, // aprox 1.7m
            {"Street Fighter", 1.35f}, // 0.05 step
            {"Donkey Kong", 1.4f}, // 0.05 step
            {"Mega Boss", 1.45f}, // aprox 1.8m
            {"NBA Jam", 1.5f}, // 0.05 step
        };

        public float height;
        public static bool IsValidHeight(float height)
        {
            return height <= 1.5f && height >= 1f;

        }
        public override bool IsValid()
        {
            return IsValidHeight(height);
        }
        static string FindNearestKey(float value)
        {
            if (!IsValidHeight(value))
                return "";

            var nearestKey = heightPlayers.OrderBy(pair => Math.Abs(pair.Value - value)).First().Key;
            return nearestKey;
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

    public class LocomotionConfiguration : ConfigInformationBase
    {
        //enable/disable "Teleport Interactor" gameobject
        [YamlMember(Alias = "teleport-enabled", ApplyNamingConventions = false)]
        public bool? teleportEnabled;
        //in player.DynamicMoveProvider.moveSpeed
        [YamlMember(Alias = "speed", ApplyNamingConventions = false)]
        public int? moveSpeed;
        [YamlMember(Alias = "turn-speed", ApplyNamingConventions = false)]
        public int? turnSpeed;
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
        Background bg = new Background();
        bg.volume = 20;
        bg.muted = false;
        return bg;
    }
    public static Background BackgroundDefault()
    {
        Background bg = new Background();
        bg.volume = 70;
        bg.muted = false;
        return bg;
    }
    public static Player PlayerDefault()
    {
        Player player = new();
        player.height = 1.35f;
        return player;
    }

    public static ConfigInformation newDefault()
    {
        return new ConfigInformation();
    }
    // =============================================================

    public static ConfigInformation fromYaml(string yamlPath)
    {
        ConfigManager.WriteConsole($"[ConfigInformation]: {yamlPath}");

        if (!File.Exists(yamlPath))
        {
            ConfigManager.WriteConsoleError($"[ConfigInformation] YAML file ({yamlPath}) doesn't exists ");
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
            ConfigManager.WriteConsoleError($"[ConfigInformation] reading configuration YAML file {yamlPath} - {e}");
            return null;
        }
    }

    public bool ToYaml(string yamlPath)
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
            ConfigManager.WriteConsoleError($"[ConfigInformation] configuration YAML file in configuration subdir {yamlPath} - {e}");
            return false;
        }
        return true;
    }

    public override string ToString()
    {
        string ret = "Configuration \n";
        ret += "Audio \n";
        ret += $" \t background: {audio?.background?.volume}\n";
        ret += "NPCs \n";
        ret += $" \t status: {npc?.status}\n";
        ret += "Player \n";
        ret += $" \t height: {player?.height}\n";
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
        if (player != null)
            if (!player.IsValid())
                throw new Exception("Invalid player settings");
    }

    // private static T returnNotNullOrNew<T>(T obj1, T obj2) where T : class, new()
    // {
    //     if (obj1 == null && obj2 != null) return obj2;
    //     if (obj1 != null && obj2 == null) return obj1;
    //     return new T(); //and change internal properties 
    // }
    // private static T returnsNotNullOrSecond<T>(T obj1, T obj2)
    // {
    //     if (obj1 == null && obj2 != null) return obj2;
    //     if (obj1 != null && obj2 == null) return obj1;
    //     return obj2;
    // }
    // public static ConfigInformation Merge(ConfigInformation ci1, ConfigInformation ci2)
    // {
    //     if (ci1 == null && ci2 != null) return ci2;
    //     if (ci1 != null && ci2 == null) return ci1;

    //     ConfigInformation ret = new();

    //     ret.audio = returnNotNullOrNew<Audio>(ci1.audio, ci2.audio);
    //     if (ret.audio != ci2.audio)
    //     {
    //         ret.audio.background = returnNotNullOrNew<Background>(ci1.audio?.background, ci2.audio?.background);
    //         if (ret.audio.background != ci2.audio?.background)
    //             ret.audio.background = returnsNotNullOrSecond<Background>(ci1.audio?.background, ci2.audio?.background);
    //         if (ret.audio.inGameBackground != ci2.audio?.inGameBackground)
    //             ret.audio.inGameBackground = returnsNotNullOrSecond<Background>(ci1.audio?.inGameBackground, ci2.audio?.inGameBackground);
    //     }

    //     ret.npc = returnNotNullOrNew<NPC>(ci1.npc, ci2.npc);
    //     if (ret.npc != ci2.npc)
    //     {
    //         ret.npc = returnsNotNullOrSecond(ci1.npc, ci2.npc);
    //     }

    //     return ret;
    // }

    public static ConfigInformation.Background GetNewMergedBackground(ConfigInformation.Background bkg1, ConfigInformation.Background bkg2, ConfigInformation.Background ret)
    {
        if (bkg2 != null)
        {
            ret.muted = bkg2.muted;
            ret.volume = bkg2.volume;
        }
        else if (bkg1 != null)
        {
            ret.muted = bkg1.muted;
            ret.volume = bkg1.volume;
        }
        return ret;
    }

    public static ConfigInformation.Background GetNewMergedBackground(ConfigInformation ci1, ConfigInformation ci2)
    {
        return GetNewMergedBackground(ci1?.audio?.background, ci2?.audio?.background, ConfigInformation.BackgroundDefault());
    }
    public static ConfigInformation.Background GetNewMergedInGameBackground(ConfigInformation ci1, ConfigInformation ci2)
    {
        return GetNewMergedBackground(ci1?.audio?.inGameBackground, ci2?.audio?.inGameBackground, ConfigInformation.BackgroundInGameDefault());
    }

    // merge 1 with 2, 2 data will prevail.
    public static ConfigInformation Merge(ConfigInformation ci1, ConfigInformation ci2)
    {
        ConfigInformation ret = new();
        if (ci1?.audio != null || ci2?.audio != null)
        {
            ret.audio = new();
            ret.audio.background = GetNewMergedBackground(ci1, ci2);
            ret.audio.inGameBackground = GetNewMergedInGameBackground(ci1, ci2);
        }
        if (ci1?.npc != null || ci2?.npc != null)
        {
            ret.npc = new();
            ret.npc.status = ci2?.npc?.status != null ? ci2.npc.status : ci1?.npc?.status;
        }
        if (ci1?.player != null || ci2?.player != null)
        {
            ret.player = new();
            ret.player.height = ci2?.player != null ? ci2.player.height : ci1.player.height;
        }

        return ret;

    }
}

