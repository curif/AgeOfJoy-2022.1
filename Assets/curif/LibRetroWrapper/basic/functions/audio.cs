using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEngine.Audio;


class CommandFunctionAudioMixerBase : CommandFunctionNoExpressionBase
{
    public enum audioMixerType
    {
        Ambience = 0,
        Game = 1,
        Music = 2
    }

    protected audioMixerType type;
    protected string volumeParam;

    public CommandFunctionAudioMixerBase(ConfigurationCommands config, 
                                            string token, 
                                            CommandFunctionAudioMixerBase.audioMixerType typeparam) 
            : base(config)
    {
        cmdToken = token;
        type = typeparam;
        if (type == audioMixerType.Ambience)
        {
            volumeParam = "AmbienceVolume";
        }
        else if (type == audioMixerType.Music)
        {
            volumeParam = "MusicVolume";
        }
        else if (type == audioMixerType.Game)
        {
            volumeParam = "GameVolume";
        }
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config.audioMixer == null)
            return new BasicValue(0);    

        float vol; 
        config.audioMixer.GetFloat(volumeParam, out vol);
       
        return new BasicValue(vol);
    }
}

class CommandFunctionAUDIOGAMEGETVOLUME : CommandFunctionAudioMixerBase
{
    public CommandFunctionAUDIOGAMEGETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOGAMEGETVOLUME", audioMixerType.Game)
    {
    }

}
class CommandFunctionAUDIOMUSICGETVOLUME : CommandFunctionAudioMixerBase
{
    public CommandFunctionAUDIOMUSICGETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOMUSICGETVOLUME", audioMixerType.Music)
    {
    }
}

class CommandFunctionAUDIOAMBIENCEGETVOLUME : CommandFunctionAudioMixerBase
{
    public CommandFunctionAUDIOAMBIENCEGETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOAMBIENCEGETVOLUME", audioMixerType.Ambience)
    {
    }
}
