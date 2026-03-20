
using System;
using System.Collections.Generic;

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

class CommandFunctionONTIMER : CommandFunctionSingleExpressionBase
{
    public CommandFunctionONTIMER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONTIMER";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue delay = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(delay, $"{cmdToken} - delay must be a number (seconds)");

        List<object> list = new List<object> { "CONFIG-EVENT", "on-timer", delay.GetValueAsNumber() };
        return new BasicValue(list);
    }
}

class CommandFunctionONCONTROL : CommandFunctionExpressionListBase
{
    public CommandFunctionONCONTROL(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONCONTROL";
        MinCantParamsRequired = 2; // ControlID, Type, [Port]
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);
        if (args.Length < 2)
            throw new Exception($"{cmdToken} - expected ControlID, Type (pressed/held/released), and optional Port");

        string controlId = args[0].GetString();
        string type = args[1].GetString().ToLower();
        int port = 0;
        if (args.Length > 2)
            port = (int)args[2].GetValueAsNumber();

        string eventId;
        switch (type)
        {
            case "pressed": eventId = "on-control-active-pressed"; break;
            case "held": eventId = "on-control-active-held"; break;
            case "released": eventId = "on-control-active-released"; break;
            default: throw new Exception($"{cmdToken} - unknown control event type: {type}. Use 'pressed', 'held', or 'released'.");
        }

        List<object> list = new List<object> { "CONFIG-EVENT", eventId, controlId, (double)port };
        return new BasicValue(list);
    }
}

class CommandFunctionONTOUCH : CommandFunctionSingleExpressionBase
{
    public CommandFunctionONTOUCH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONTOUCH";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue part = expr.Execute(vars);
        FunctionHelper.ExpectedString(part, $"{cmdToken} - part name must be a string");

        List<object> list = new List<object> { "CONFIG-EVENT", "on-touch-start", part.GetString() };
        return new BasicValue(list);
    }
}

class CommandFunctionONGRAB : CommandFunctionSingleExpressionBase
{
    public CommandFunctionONGRAB(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONGRAB";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue part = expr.Execute(vars);
        FunctionHelper.ExpectedString(part, $"{cmdToken} - part name must be a string");

        List<object> list = new List<object> { "CONFIG-EVENT", "on-grab-start", part.GetString() };
        return new BasicValue(list);
    }
}

class CommandFunctionONCOLLISION : CommandFunctionExpressionListBase
{
    public CommandFunctionONCOLLISION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONCOLLISION";
        MinCantParamsRequired = 1;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);

        List<object> list = new List<object> { "CONFIG-EVENT", "on-collision-start", args[0].GetString() };
        for (int i = 1; i < args.Length; i++)
            list.Add(args[i].GetString());

        return new BasicValue(list);
    }
}

class CommandFunctionONSPRITECOLLISION : CommandFunctionExpressionListBase
{
    public CommandFunctionONSPRITECOLLISION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONSPRITECOLLISION";
        MinCantParamsRequired = 2;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(args[0], $"{cmdToken} - first sprite name must be a string");
        FunctionHelper.ExpectedString(args[1], $"{cmdToken} - second sprite name must be a string");

        List<object> list = new List<object> { "CONFIG-EVENT", "on-sprite-collision-start", args[0].GetString(), args[1].GetString() };
        return new BasicValue(list);
    }
}

class CommandFunctionONSPRITECOLLISIONEND : CommandFunctionExpressionListBase
{
    public CommandFunctionONSPRITECOLLISIONEND(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ONSPRITECOLLISIONEND";
        MinCantParamsRequired = 2;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] args = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(args[0], $"{cmdToken} - first sprite name must be a string");
        FunctionHelper.ExpectedString(args[1], $"{cmdToken} - second sprite name must be a string");

        List<object> list = new List<object> { "CONFIG-EVENT", "on-sprite-collision-end", args[0].GetString(), args[1].GetString() };
        return new BasicValue(list);
    }
}
