using System;
using System.IO;

class CommandFunctionMUSICPLAY : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICPLAY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICPLAY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        config.MusicPlayerQueue.Play();

        return new BasicValue(1);
    }
}

class CommandFunctionMUSICADD : CommandFunctionSingleExpressionBase
{
    public CommandFunctionMUSICADD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICADD";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "file path");
        string filePath = val.GetValueAsString();

        config.MusicPlayerQueue.AddMusic(Path.Combine(ConfigManager.MusicDir, filePath));        

        return new BasicValue(1);
    }
}


class CommandFunctionMUSICREMOVE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionMUSICREMOVE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICREMOVE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "file path");
        string filePath = Path.Combine(ConfigManager.MusicDir, val.GetValueAsString());
        if (config.MusicPlayerQueue.IsInQueue(filePath))
        {
            config.MusicPlayerQueue.RemoveMusic(filePath);
            return new BasicValue(1);
        }

        return new BasicValue(0);
    }
}

class CommandFunctionMUSICEXIST : CommandFunctionSingleExpressionBase
{
    public CommandFunctionMUSICEXIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICEXIST";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, "file path");
        string filePath = val.GetValueAsString();

        return new BasicValue(config.MusicPlayerQueue.IsInQueue(Path.Combine(ConfigManager.MusicDir, filePath)));
    }
}


class CommandFunctionMUSICCLEAR : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICCLEAR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICCLEAR";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        config.MusicPlayerQueue.ClearQueue();        

        return new BasicValue(1);
    }
}

class CommandFunctionMUSICLOOP : CommandFunctionSingleExpressionBase
{
    public CommandFunctionMUSICLOOP(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICLOOP";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");


        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, "Boolean - loop active or not");

        config.MusicPlayerQueue.Loop = val.IsTrue();
        return new BasicValue(1);
    }
}


class CommandFunctionMUSICLOOPSTATUS : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICLOOPSTATUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICLOOPSTATUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        return new BasicValue(config.MusicPlayerQueue.Loop);
    }
}


class CommandFunctionMUSICADDLIST : CommandFunctionExpressionListBase
{
    public CommandFunctionMUSICADDLIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICADDLIST";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");


        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "list");
        FunctionHelper.ExpectedNonEmptyString(vals[1], "separator");
        string paths = vals[0].GetValueAsString();
        string separator = vals[1].GetValueAsString();

        config.MusicPlayerQueue.ClearQueue();

        foreach (string file in paths.Split(new[] { separator }, StringSplitOptions.None))
        {
            if (string.IsNullOrEmpty(file))
                continue;

            string filePath = FunctionHelper.FileTraversalFree(Path.Combine(ConfigManager.MusicDir, file),
                                                                ConfigManager.MusicDir);

            config.MusicPlayerQueue.AddMusic(filePath);
        }

        return new BasicValue(1);
    }
}

class CommandFunctionMUSICNEXT : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICNEXT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICNEXT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        config.MusicPlayerQueue.Next();        

        return new BasicValue(1);
    }
}

class CommandFunctionMUSICPREVIOUS : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICPREVIOUS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICPREVIOUS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        config.MusicPlayerQueue.Previous();        

        return new BasicValue(1);
    }
}


class CommandFunctionMUSICRESET : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICRESET(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICRESET";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        config.MusicPlayerQueue.ResetQueue();

        return new BasicValue(1);
    }
}

class CommandFunctionMUSICCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionMUSICCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICCOUNT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");

        return new BasicValue(config.MusicPlayerQueue.CountQueue());
    }
}

class CommandFunctionMUSICGETLIST: CommandFunctionSingleExpressionBase
{
    public CommandFunctionMUSICGETLIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "MUSICGETLIST";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config.MusicPlayerQueue == null)
            throw new Exception("Music player doesn't exists");


        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, "string - list separator");

        return new BasicValue(config.MusicPlayerQueue.Separated(val.GetString()));
    }
}
