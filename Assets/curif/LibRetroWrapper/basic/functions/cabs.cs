using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

class CommandFunctionCABROOMCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABROOMCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.CabinetsController == null)
            return new BasicValue(0);

        return new BasicValue((double)config.CabinetsController.Count());
    }
}


class CommandFunctionCABROOMGETNAME : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABROOMGETNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMGETNAME";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config?.CabinetsController == null)
            return new BasicValue("");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        CabinetPosition cabPos = config.CabinetsController.GetCabinetByPosition((int)val.GetValueAsNumber());
        if (cabPos == null)
            return new BasicValue("");

        return new BasicValue(cabPos.CabinetDBName);
    }
}

class CommandFunctionCABROOMREPLACE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMREPLACE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMREPLACE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config?.CabinetsController == null || config?.ConfigurationController == null)
            return new BasicValue(0);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedString(vals[1], " - new cabinet name");

        string roomName = config?.ConfigurationController?.GetRoomName();
        if (string.IsNullOrEmpty(roomName))
            return new BasicValue(0); //fail

        string cabinetDBName = vals[1].GetValueAsString();
        if (!config.GameRegistry.CabinetExists(cabinetDBName))
            return new BasicValue(0); //fail

        bool result = config.CabinetsController.Replace((int)vals[0].GetValueAsNumber(),
                                                        roomName,
                                                        cabinetDBName);

        return new BasicValue(result ? 1 : 0);
    }
}

class CommandFunctionCABDBCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABDBCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue(0);

        int count = config.GameRegistry.CountCabinets();
        if (count < 0)
            throw new Exception("error access cabinetsDB folder");

        return new BasicValue((double)count);
    }
}

class CommandFunctionCABDBCOUNTINROOM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBCOUNTINROOM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBCOUNTINROOM";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue(0);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        return new BasicValue(
            (double)config.GameRegistry.GetCabinetsCountInRoom(val.GetValueAsString())
            );
    }
}

class CommandFunctionCABDBGETNAME : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBGETNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBGETNAME";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue(0);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        return new BasicValue(
                config.GameRegistry.GetCabinetNameByPosition((int)val.GetValueAsNumber())
            );
    }
}

/*
class CommandFunctionCABDBGET : CommandFunctionExpressionListBase, ICommandFunctionList
{
    public CommandFunctionCABDBGET(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBGET";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
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
                new BasicValue(-1)
        };

        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return ret;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");

        CabinetPosition cabpos = 
                config.GameRegistry.GetCabinetPositionInRoom(
                                        (int)vals[0].GetValueAsNumber(),
                                        (int)vals[1].GetValueAsNumber()
                                        );
        if (cabpos == null)
            return ret;

        ret[0].SetValue(cabpos.CabinetDBName);
        ret[1].SetValue(cabpos.Position);

        return ret;
    }
}
*/

class CommandFunctionCABDBDELETE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBDELETE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBDELETE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue ret = new BasicValue(0);

        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return ret;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");

        string room = vals[0].GetValueAsString();
        int position = (int)vals[1].GetValueAsNumber();
        CabinetPosition cabpos =
                config.GameRegistry.GetCabinetPositionInRoom(position, room);
        if (cabpos == null)
            return ret;

        config.GameRegistry.DeleteCabinetPositionInRoom(position, room);
        if (cabpos == null)
            return ret;

        return ret.SetValue(1);
    }
}
class CommandFunctionCABDBADD : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBADD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBADD";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue ret = new BasicValue(0);

        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return ret;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");
        FunctionHelper.ExpectedString(vals[2], " - new cabinet name");

        string room = vals[0].GetValueAsString();
        int position = (int)vals[1].GetValueAsNumber();
        CabinetPosition cabpos =
                config.GameRegistry.GetCabinetPositionInRoom(position, room);
        if (cabpos != null)
            return ret;

        cabpos = new();
        cabpos.CabinetDBName = vals[2].GetValueAsString();
        cabpos.Position = position;
        cabpos.Room = room;
        config.GameRegistry.Add(cabpos);

        return ret.SetValue(1);
    }
}

class CommandFunctionCABDBSAVE : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABDBSAVE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBSAVE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue ret = new BasicValue(0);

        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return ret;

        config.GameRegistry.Persist();
        return null;
    }
}

/*
class CommandFunctionCABDBREPLACE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBREPLACE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBREPLACE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config?.GameRegistry == null)
            return new BasicValue(0);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");
        FunctionHelper.ExpectedString(vals[2], " - new cabinet name");

        string roomName = vals[0].GetValueAsString();
        if (string.IsNullOrEmpty(roomName))
            return new BasicValue(0); //fail

        string cabinetDBName = vals[2].GetValueAsString();
        if (!config.GameRegistry.CabinetExists(cabinetDBName))
            return new BasicValue(0); //fail

        int position = (int)vals[1].GetValueAsNumber();

        CabinetPosition toAdd = new();
        toAdd.Room = roomName;
        toAdd.Position = position;
        toAdd.CabinetDBName = cabinetDBName;

        CabinetPosition toBeReplaced = config.GameRegistry.GetCabinetPositionInRoom(position, roomName);
        ConfigManager.WriteConsole($"[CommandFunctionCABDBREPLACE] [{toBeReplaced}] by [{toAdd}] ");
        config.GameRegistry.Replace(toBeReplaced, toAdd); //persists changes

        return new BasicValue(1);
    }
}
*/