using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

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

        config.MusicPlayerQueue.AddMusic(ConfigManager.MusicDir + "/" + filePath);        

        return new BasicValue(1);
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

        foreach(string file in paths.Split(new[] { separator }, StringSplitOptions.None))
        {
            string filePath = FunctionHelper.FileTraversalFree(
                                                                Path.Combine(ConfigManager.MusicDir, file),
                                                                ConfigManager.MusicDir
                                                                );

            config.MusicPlayerQueue.AddMusic(filePath);
        }

        return new BasicValue(1);
    }
}