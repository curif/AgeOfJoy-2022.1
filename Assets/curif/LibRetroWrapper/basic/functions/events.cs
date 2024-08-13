
using System;

class CommandFunctionEVENTTRIGGER : CommandFunctionSingleExpressionBase
{

    public CommandFunctionEVENTTRIGGER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "EVENTTRIGGER";
    }


    public Event GetEventByName(string name)
    {
        foreach (Event evt in config.events)
            if (evt.eventInformation.name == name)
                return evt;
        return null;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - event name should be a string");

        Event evt = GetEventByName(val.GetString());
        if (evt == null)
            throw new Exception($"{CmdToken}: AGEBasic cabinet event {val.GetString()} not found.");

        if (evt.GetType() != typeof(OnCustom))
            throw new Exception($"{CmdToken}: AGEBasic cabinet event {val.GetString()} isn't a custom event. Only custom events could be triggered.");

        OnCustom cstm = (OnCustom)evt;
        cstm.ForceTrigger();

        return null;
    }
}
