
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);

        string path = vals[0].GetValueAsString();
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
            ConfigManager.WriteConsole($"Error: {ex.Message}");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        
        BasicValue val = expr.Execute(vars);
        string filePath = val.GetValueAsString();

        bool fileExists = File.Exists(filePath);

        return new BasicValue(fileExists? 1:0);
    }
}
