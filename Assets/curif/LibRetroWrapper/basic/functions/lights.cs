using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

class CommandFunctionGETLIGHTS : CommandFunctionNoExpressionBase
{
    public CommandFunctionGETLIGHTS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETLIGHTS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        StringBuilder resultBuilder = new StringBuilder();

        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject lightObject in lightObjects)
        {
            LightManagerController lightController = lightObject.GetComponent<LightManagerController>();

            if (lightController != null && !string.IsNullOrEmpty(lightController.LightName))
            {
                if (resultBuilder.Length > 0)
                {
                    resultBuilder.Append("|");
                }

                resultBuilder.Append(lightController.LightName);
            }
        }

        string result = resultBuilder.ToString();
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] Result: {result}");

        return new BasicValue(result);
    }
}


class CommandFunctionGETLIGHTINTENSITY : CommandFunctionSingleExpressionBase
{
    public CommandFunctionGETLIGHTINTENSITY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETLIGHTINTENSITY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        // Retrieve the parameter "NAME"
        BasicValue nameValue = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(nameValue, "- light name");

        string lightName = nameValue.GetValueAsString();

        // Find all GameObjects with the "Light" tag
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");

        // Iterate through each GameObject to find the one with the matching LightName
        foreach (GameObject lightObject in lightObjects)
        {
            LightManagerController lightController = lightObject.GetComponent<LightManagerController>();

            if (lightController != null && lightController.LightName == lightName)
            {
                // Return the intensity as a BasicValue
                return new BasicValue((double)lightController.GetIntensity());
            }
        }

        return new BasicValue((double)0);
    }
}

class CommandFunctionSETLIGHTINTENSITY : CommandFunctionExpressionListBase
{
    public CommandFunctionSETLIGHTINTENSITY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SETLIGHTINTENSITY";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        // Retrieve the parameters "lightname" and "intensity"
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- light name");
        FunctionHelper.ExpectedNumber(vals[1], "- intensity");

        string lightName = vals[0].GetValueAsString();
        float intensity = (float)vals[1].GetValueAsNumber();

        // Find the correct GameObject with the specified LightName
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject lightObject in lightObjects)
        {
            LightManagerController lightController = lightObject.GetComponent<LightManagerController>();

            if (lightController != null && lightController.LightName == lightName)
            {
                // Set the intensity using the LightController method
                lightController.SetIntensity(intensity);

                ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] Intensity set for light '{lightName}' to {intensity}");

                return new BasicValue(intensity);
            }
        }

        return new BasicValue(0);
    }
}


class CommandFunctionLIGHTSCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionLIGHTSCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LIGHTSCOUNT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        // Find all GameObjects with the "Light" tag
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");

        // Count the lights with a LightController component
        int lightsCount = 0;

        foreach (GameObject lightObject in lightObjects)
        {
            if (lightObject.GetComponent<LightManagerController>() != null)
            {
                lightsCount++;
            }
        }

        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] Counted {lightsCount} lights with LightController.");

        return new BasicValue((double)lightsCount);
    }
}

class CommandFunctionSETLIGHTCOLOR : CommandFunctionExpressionListBase
{
    public CommandFunctionSETLIGHTCOLOR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SETLIGHTCOLOR";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 4); // Expecting 4 parameters: lightname, R, G, B
    }

    public override BasicValue Execute(BasicVars vars)
    {
        // Retrieve the parameters "lightname", "R", "G", and "B"
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], "- light name");
        FunctionHelper.ExpectedNumber(vals[1], "- R");
        FunctionHelper.ExpectedNumber(vals[2], "- G");
        FunctionHelper.ExpectedNumber(vals[3], "- B");

        string lightName = vals[0].GetValueAsString();
        float r = (float)vals[1].GetValueAsNumber();
        float g = (float)vals[2].GetValueAsNumber();
        float b = (float)vals[3].GetValueAsNumber();

        // Find the correct GameObject with the specified LightName
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject lightObject in lightObjects)
        {
            LightManagerController lightController = lightObject.GetComponent<LightManagerController>();

            if (lightController != null && lightController.LightName == lightName)
            {
                // Set the color using the LightController method
                lightController.SetColor(new Color(r, g, b));

                ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] Color set for light '{lightName}' to ({r}, {g}, {b})");

                return new BasicValue(1); // Indicate success
            }
        }

        return new BasicValue(0); // Indicate failure
    }
}
