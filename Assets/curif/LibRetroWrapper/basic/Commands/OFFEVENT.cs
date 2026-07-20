using System;

class CommandOFFEVENT : CommandExpressionListBase
{
    public CommandOFFEVENT(ConfigurationCommands config) : base(config)
    {
        this.cmdToken = "OFFEVENT";
        this.MinCantParamsRequired = 1;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken} #{config.LineNumber}] ");

        if (config.events == null)
            throw new Exception($"{CmdToken}: this command requires an initialized events list in ConfigurationCommands.");

        BasicValue[] values = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(values[0], $"- Event Name for {CmdToken}");
        string name = values[0].GetString();

        if (string.IsNullOrEmpty(name))
            throw new Exception($"{CmdToken}: event name can't be empty.");

        int removed = 0;
        for (int i = config.events.Count - 1; i >= 0; i--)
        {
            if (config.events[i].eventInformation.name == name)
            {
                try
                {
                    config.events[i].Dispose();
                }
                catch (Exception e)
                {
                    ConfigManager.WriteConsoleException($"[{CmdToken}] error disposing event:{name}", e);
                }
                config.events.RemoveAt(i);
                removed++;
            }
        }

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN #{config.LineNumber} {CmdToken}] removed {removed} event(s) named '{name}'");

        return null;
    }
}
