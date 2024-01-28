using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEngine.Audio;


class CommandFunctionAudioMixerGetVolBase : CommandFunctionNoExpressionBase
{
    protected string volumeParam;

    public CommandFunctionAudioMixerGetVolBase(ConfigurationCommands config, 
                                            string token, 
                                            string volPar) 
            : base(config)
    {
        cmdToken = token;
        //check the Spatializer mixer exposed parameters in Unity editor audio mixer.
        volumeParam = volPar + "Volume"; 
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

class CommandFunctionAUDIOGAMEGETVOLUME : CommandFunctionAudioMixerGetVolBase
{
    public CommandFunctionAUDIOGAMEGETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOGAMEGETVOLUME", "Game")
    {
    }

}
class CommandFunctionAUDIOMUSICGETVOLUME : CommandFunctionAudioMixerGetVolBase
{
    public CommandFunctionAUDIOMUSICGETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOMUSICGETVOLUME", "Music")
    {
    }
}

class CommandFunctionAUDIOAMBIENCEGETVOLUME : CommandFunctionAudioMixerGetVolBase
{
    public CommandFunctionAUDIOAMBIENCEGETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOAMBIENCEGETVOLUME", "Ambience")
    {
    }
}


class CommandFunctionAudioMixerSetVolBase : CommandFunctionSingleExpressionBase
{
    protected string volumeParam;

    public CommandFunctionAudioMixerSetVolBase(ConfigurationCommands config, 
                                            string token, 
                                            string volPar) 
            : base(config)
    {
        cmdToken = token;
        //check the Spatializer mixer exposed parameters in Unity editor audio mixer.
        volumeParam = volPar + "Volume"; 
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config.audioMixer == null)
            return new BasicValue(0);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, "Volume");

        float vol = (float)val.GetValueAsNumber(); 
        config.audioMixer.SetFloat(volumeParam, vol);
       
        return new BasicValue(1);
    }
}

class CommandFunctionAUDIOGAMESETVOLUME : CommandFunctionAudioMixerSetVolBase
{
    public CommandFunctionAUDIOGAMESETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOGAMEGETVOLUME", "Game")
    {
    }
}

class CommandFunctionAUDIOMUSICSETVOLUME : CommandFunctionAudioMixerSetVolBase
{
    public CommandFunctionAUDIOMUSICSETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOMUSICSETVOLUME", "Music")
    {
    }
}

class CommandFunctionAUDIOAMBIENCESETVOLUME : CommandFunctionAudioMixerSetVolBase
{
    public CommandFunctionAUDIOAMBIENCESETVOLUME(ConfigurationCommands config) : 
        base(config, "AUDIOAMBIENCESETVOLUME", "Ambience")
    {
    }
}

