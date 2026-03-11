using System;

public class CommandRUN : ICommandBase
{
    public string CmdToken { get; } = "RUN";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    private ConfigurationCommands config;
    private CommandExpression fileExpr;
    private CommandExpression lineExpr;
    private bool hasLineParameter = false;

    public CommandRUN(ConfigurationCommands config)
    {
        this.config = config;
        fileExpr = new CommandExpression(config);
        lineExpr = new CommandExpression(config);
    }

    public bool Parse(TokenConsumer tokens)
    {
        // Parse the file name expression (e.g., "myprogram.bas" or variable)
        fileExpr.Parse(tokens);

        // check if optional LINE keyword exists
        if (tokens.Token != null && tokens.Token.ToUpper() == "LINE")
        {
            tokens++; // Consume "LINE"
            hasLineParameter = true;
            lineExpr.Parse(tokens);
        }

        return true;
    }

    public BasicValue Execute(BasicVars vars)
    {
        BasicValue fileVal = fileExpr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(fileVal, "RUN command requires a valid file name.");
        
        int targetLine = 0; // Default execution starts at 0 (beginning)
        if (hasLineParameter)
        {
            BasicValue lineVal = lineExpr.Execute(vars);
            FunctionHelper.ExpectedNumber(lineVal, "RUN LINE requires a numeric line number.");
            targetLine = (int)lineVal.GetNumber();
        }

        //paths are relative to calling program:
        string path = fileVal.GetString();
        /*if (!System.IO.Path.IsPathRooted(path) && !string.IsNullOrEmpty(config.ProgramPath))
        {
            path = System.IO.Path.Combine(config.ProgramPath, path);
        }*/

        // Signal the engine to load and run the sub-program
        config.RunSubProgramPath = path;
        config.RunSubProgramLine = targetLine;

        return null;
    }

    public void CheckConfigRequirements(ConfigurationCommands config)
    {
    }
}