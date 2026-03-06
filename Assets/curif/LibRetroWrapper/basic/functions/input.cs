using System;

class CommandFunctionCONTROLACTIVE : CommandFunctionExpressionListBase
{
    public CommandFunctionCONTROLACTIVE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CONTROLACTIVE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 1); //only the id is required
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config?.ControlMap == null)
            throw new Exception("no control map assigned.");
        
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- Control ID");

        string mameControl = vals[0].GetValueAsString();
        int port = 0;
        if (FunctionHelper.GetValsCount(vals) > 1)
            port = vals[1].GetInt();

        string debounceKey = $"{mameControl}_{port}";

        AGEBasicDebug.WriteConsole($"[ControlActive Execute] {mameControl} {port} params count:{vals.Length}");
        
        if (config.ControlMap.Active(mameControl, port) == 0)
        {
            // If the button is released, clear the debounce timer so the next press is instant
            if (config.lastControlActiveTime.ContainsKey(debounceKey))
                config.lastControlActiveTime.Remove(debounceKey);

            return BasicValue.False;
        }

        // The button is physically pressed. Check the debounce timer.
        float currentTime = UnityEngine.Time.time;
        if (config.lastControlActiveTime.TryGetValue(debounceKey, out float lastTime))
        {
            // 0.25 seconds debounce delay (acts like keyboard repeat delay)
            if (currentTime - lastTime < 0.25f)
            {
                return BasicValue.False; // Suppress the input, it's bouncing
            }
        }

        // Register the active time and return true
        config.lastControlActiveTime[debounceKey] = currentTime;
        return BasicValue.True;
    }
}

class CommandFunctionCONTROLRUMBLE : CommandFunctionExpressionListBase
{
    public CommandFunctionCONTROLRUMBLE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CONTROLRUMBLE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        if (config?.ControlMap == null)
            throw new Exception("no controller assigned.");
        
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- Control");
        FunctionHelper.ExpectedNumber(vals[1], "- amplitude");
        FunctionHelper.ExpectedNumber(vals[2], "- duration");

        string mameControl = vals[0].GetString();
        float amplitude = (float)vals[1].GetNumber();
        float duration = (float)vals[2].GetNumber();

        AGEBasicDebug.WriteConsole($"[CommandFunctionCONTROLRUMBLE] {mameControl} amp: {amplitude} dur: {duration}");
        if (config.ControlMap.SendHapticImpulse(mameControl, amplitude, duration))
            return BasicValue.True;

        return BasicValue.False;
    }
}
