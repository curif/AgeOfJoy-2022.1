using System;
using UnityEngine;

class CommandFunctionCABPARTSAUDIOVOLUME: CommandFunctionExpressionListBase
{

    public CommandFunctionCABPARTSAUDIOVOLUME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIOVOLUME";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[1], "- volume should be a number");

        GameObject go;
        if (vals[0].IsString())
            go = config.Cabinet.Parts(vals[0].GetString());
        else
            go = config.Cabinet.Parts(vals[0].GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to set audio volume.");
        
        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.SetVolume((float)vals[1].GetNumber());
        return new BasicValue(1);
    }
}

class CommandFunctionCABPARTSAUDIODISTANCE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSAUDIODISTANCE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIODISTANCE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[1], "- min distance should be a number");
        FunctionHelper.ExpectedNumber(vals[2], "- max distance should be a number");

        GameObject go;
        if (vals[0].IsString())
            go = config.Cabinet.Parts(vals[0].GetString());
        else
            go = config.Cabinet.Parts(vals[0].GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to set audio distance.");

        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.SetMinMaxDistance((float)vals[1].GetNumber(), (float)vals[2].GetNumber());
        return new BasicValue(1);
    }
}

class CommandFunctionCABPARTSAUDIOFILE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSAUDIOFILE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIOFILE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[1], "- file path should be a string");

        GameObject go;
        if (vals[0].IsString())
            go = config.Cabinet.Parts(vals[0].GetString());
        else
            go = config.Cabinet.Parts(vals[0].GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to set audio file.");

        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.AssignAudioClip(vals[1].GetString());
        return new BasicValue(1);
    }
}

class CommandFunctionCABPARTSAUDIOLOOP : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSAUDIOLOOP(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIOLOOP";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);

        GameObject go;
        if (vals[0].IsString())
            go = config.Cabinet.Parts(vals[0].GetString());
        else
            go = config.Cabinet.Parts(vals[0].GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to set audio loop.");

        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.SetLoop(vals[1].GetBoolean());
        return new BasicValue(1);
    }
}

class CommandFunctionCABPARTSAUDIOPLAY : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABPARTSAUDIOPLAY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIOPLAY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        GameObject go;
        if (val.IsString())
            go = config.Cabinet.Parts(val.GetString());
        else
            go = config.Cabinet.Parts(val.GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to play audio.");

        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.PlayAudio();
        return new BasicValue(1);
    }
}

class CommandFunctionCABPARTSAUDIOSTOP : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABPARTSAUDIOSTOP(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIOSTOP";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        GameObject go;
        if (val.IsString())
            go = config.Cabinet.Parts(val.GetString());
        else
            go = config.Cabinet.Parts(val.GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to stop audio.");

        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.StopAudio();
        return new BasicValue(1);
    }
}

class CommandFunctionCABPARTSAUDIOPAUSE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABPARTSAUDIOPAUSE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTAUDIOPAUSE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        GameObject go;
        if (val.IsString())
            go = config.Cabinet.Parts(val.GetString());
        else
            go = config.Cabinet.Parts(val.GetInt());

        if (go == null)
            throw new Exception("AGEBasic part name or number is wrong to pause audio.");

        CabinetPartAudioController controller = go.GetComponent<CabinetPartAudioController>();
        if (controller == null)
            throw new Exception($"AGEBasic part {go.name} isn't declared as 'audio' in description.yaml");

        controller.PauseAudio();
        return new BasicValue(1);
    }
}
