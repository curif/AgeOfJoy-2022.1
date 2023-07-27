using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

class CommandFunctionROOMNAME : CommandFunctionNoExpressionBase
{
    public CommandFunctionROOMNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMNAME";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        if (config == null)
            throw new Exception($"AGEBasic not configured. can't access the room name, the program should be excecuted as part of a configuration controller.");
        if (config.ConfigurationController == null)
            throw new Exception($"AGEBasic no .ConfigurationController. can't access the room name, the program should be excecuted as part of a configuration controller.");
        string roomName = config?.ConfigurationController?.GetRoomName();
        if (roomName == null)
            throw new Exception($"AGEBasic can't access the room name, the program should be excecuted as part of a configuration controller.");

        return new BasicValue(roomName);
    }
}

class CommandFunctionROOMCOUNT : CommandFunctionNoExpressionBase
{

    public CommandFunctionROOMCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMCOUNT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        double count = 0;
        if (config?.SceneDatabase != null)
            count = (double)config.SceneDatabase.Scenes.Length;
        return new BasicValue(count);
    }
}

class CommandFunctionROOMGETNAME : CommandFunctionSingleExpressionBase
{

    public CommandFunctionROOMGETNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMGETNAME";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        string name = "";

        if (config?.SceneDatabase != null)
        {
            BasicValue val = expr.Execute(vars);
            if (!val.IsNumber())
                throw new Exception($"{CmdToken} parameter should be a number");

            int idx = (int)val.GetValueAsNumber();
            if (idx < 0 || idx > 999)
                throw new Exception($"Invalid room number: {idx}");

            //name = string.Format("Room{0:000}", number);
            // if (!config.SceneDatabase.Exists(name))
            SceneDocument scene = config.SceneDatabase.ByIdx(idx);
            if (scene == null)
                name = "";
            else
                name = scene.SceneName;
        }

        return new BasicValue(name);
    }

}

class CommandFunctionROOMGETDESC : CommandFunctionSingleExpressionBase
{

    public CommandFunctionROOMGETDESC(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMGETDESC";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        string name, desc = "";
        SceneDocument scene;

        if (config?.SceneDatabase != null)
        {
            BasicValue val = expr.Execute(vars);
            if (val.IsNumber())
            {
                int idx = (int)val.GetValueAsNumber();
                if (idx < 0 || idx > 999)
                    return new BasicValue("");

                scene = config.SceneDatabase.ByIdx(idx);
            }
            else
            {
                name = val.GetValueAsString();
                scene = config.SceneDatabase.FindByName(name);
            }
            if (scene != null)
                desc = scene.Description;
        }
        return new BasicValue(desc);
    }
}

class CommandFunctionROOMGET : CommandFunctionSingleExpressionBase, ICommandFunctionList
{
    public int MaxAllowed { get; } = 2;

    public CommandFunctionROOMGET(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMGET";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        throw new Exception("Bad function implementation, should return a list");
    }

    public BasicValue[] ExecuteList(BasicVars vars)
    {
        BasicValue[] ret = new BasicValue[2]
        {
                new BasicValue(""),
                new BasicValue("")
        };

        if (config?.SceneDatabase != null)
        {
            BasicValue val = expr.Execute(vars);
            if (!val.IsNumber())
                throw new Exception($"{CmdToken} parameter should be a number");

            int idx = (int)val.GetValueAsNumber();
            if (idx < 0 || idx > 999)
                throw new Exception($"Invalid room number: {idx}");

            SceneDocument scene = config.SceneDatabase.ByIdx(idx);
            if (scene != null)
            {
                ret[0].SetValue(scene.SceneName);
                ret[1].SetValue(scene.Description);
            }
        }

        return ret;
    }
}


class CommandFunctionROOMTELEPORT : CommandFunctionSingleExpressionBase
{

    public CommandFunctionROOMTELEPORT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMTELEPORT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config?.Teleportation == null)
            throw new Exception("teleportation capability not available");
        
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - room name");
        
        string scene = val.GetString();
        SceneDocument toScene = config.SceneDatabase.FindByName(scene);
        if (toScene == null)
            throw new Exception($"unknown room to teleport: {scene} ");

        config.Teleportation.Teleport(toScene);

        return null;
    }
}
