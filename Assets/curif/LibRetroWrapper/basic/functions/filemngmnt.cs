
using System;
using System.IO;
using System.Linq;

class CommandFunctionGETFILES : CommandFunctionExpressionListBase
{
    public CommandFunctionGETFILES(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETFILES";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - file path");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - separator");
        FunctionHelper.ExpectedNumber(vals[2], " - order");

        string path = FunctionHelper.FileTraversalFree(vals[0].GetValueAsString(), ConfigManager.BaseDir);
        string separator = vals[1].GetValueAsString();
        int orderType = (int)vals[2].GetValueAsNumber();

        string[] files;

        try
        {
            // Get files from the specified path based on order type
            switch (orderType)
            {
                case 0: // Alphabetic order
                    files = Directory.GetFiles(path).OrderBy(f => f).ToArray();
                    break;
                case 1: // Random order
                    files = Directory.GetFiles(path).OrderBy(f => Guid.NewGuid()).ToArray();
                    break;
                case 2: // Creation date from old to new
                    files = Directory.GetFiles(path).OrderBy(f => new FileInfo(f).CreationTime).ToArray();
                    break;
                case 3: // Creation date from new to old
                    files = Directory.GetFiles(path).OrderByDescending(f => new FileInfo(f).CreationTime).ToArray();
                    break;
                default:
                    // Invalid order type, return an empty string or handle the error accordingly
                    return new BasicValue("");
            }

            // Extract file names without the path
            string result = string.Join(separator, files.Select(f => Path.GetFileName(f)));

            return new BasicValue(result);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that may occur (e.g., invalid path)
            AGEBasicDebug.WriteConsole($"Error: {ex.Message}");
            return new BasicValue("");
        }
    }
}

class CommandFunctionFILEEXISTS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionFILEEXISTS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEEXISTS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, " - file path");
        string filePath = FunctionHelper.FileTraversalFree(val.GetValueAsString(), ConfigManager.BaseDir);
        bool fileExists = File.Exists(filePath);

        return new BasicValue(fileExists ? 1 : 0);
    }
}

class CommandFunctionCOMBINEPATH : CommandFunctionExpressionListBase
{
    public CommandFunctionCOMBINEPATH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "COMBINEPATH";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - file path 1");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - file path 2");

        string path1 = vals[0].GetValueAsString();
        string path2 = vals[1].GetValueAsString();

        string combinedPath = FunctionHelper.FileTraversalFree(Path.Combine(path1, path2), ConfigManager.BaseDir);
        return new BasicValue(combinedPath);
    }
}

class PathFunctionsBase : CommandFunctionNoExpressionBase
{
    public PathFunctionsBase(ConfigurationCommands config, string token) : base(config)
    {
        cmdToken = token;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        switch (cmdToken)
        {
            case "CONFIGPATH":
                return new BasicValue(ConfigManager.ConfigDir);
            case "AGEBASICPATH":
                return new BasicValue(ConfigManager.AGEBasicDir);
            case "CABINETSDBPATH":
                return new BasicValue(ConfigManager.CabinetsDB);
            case "CABINETSPATH":
                return new BasicValue(ConfigManager.Cabinets);
            case "ROOTPATH":
                return new BasicValue(ConfigManager.BaseDir);
            case "MUSICPATH":
                return new BasicValue(ConfigManager.MusicDir);
            default:
                return new BasicValue("");
        }
    }
}

class CommandFunctionCONFIGPATH : PathFunctionsBase
{
    public CommandFunctionCONFIGPATH(ConfigurationCommands config) : base(config, "CONFIGPATH") { }
}

class CommandFunctionAGEBASICPATH : PathFunctionsBase
{
    public CommandFunctionAGEBASICPATH(ConfigurationCommands config) : base(config, "AGEBASICPATH") { }
}

class CommandFunctionCABINETSDBPATH : PathFunctionsBase
{
    public CommandFunctionCABINETSDBPATH(ConfigurationCommands config) : base(config, "CABINETSDBPATH") { }
}

class CommandFunctionCABINETSPATH : PathFunctionsBase
{
    public CommandFunctionCABINETSPATH(ConfigurationCommands config) : base(config, "CABINETSPATH") { }
}

class CommandFunctionROOTPATH : PathFunctionsBase
{
    public CommandFunctionROOTPATH(ConfigurationCommands config) : base(config, "ROOTPATH") { }
}

class CommandFunctionMUSICPATH : PathFunctionsBase
{
    public CommandFunctionMUSICPATH(ConfigurationCommands config) : base(config, "MUSICPATH") { }
}
