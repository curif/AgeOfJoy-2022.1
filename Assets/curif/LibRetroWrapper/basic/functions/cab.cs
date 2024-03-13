using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class CommandFunctionCABPARTSCOUNT : CommandFunctionNoExpressionBase
{

    public CommandFunctionCABPARTSCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        return new BasicValue((double)config.Cabinet.PartsCount());
    }
}

class CommandFunctionCABPARTSNAME : CommandFunctionSingleExpressionBase
{

    public CommandFunctionCABPARTSNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSNAME";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        int partNum = (int)val.GetNumber();

        return new BasicValue(config.Cabinet.PartsName(partNum));
    }
}


class CommandFunctionCABPARTSPOSITION : CommandFunctionSingleExpressionBase
{

    public CommandFunctionCABPARTSPOSITION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSPOSITION";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string partName = val.GetString();
        return new BasicValue(config.Cabinet.PartsPosition(partName));
    }
}

class CommandFunctionCABPARTSENABLE : CommandFunctionExpressionListBase
{

    public CommandFunctionCABPARTSENABLE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSENABLE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        // FunctionHelper.ExpectedNumber(vals[0], " - part number");
        // FunctionHelper.ExpectedNumber(vals[1], " - enabled true/false");

        if (vals[0].IsString())
            config.Cabinet.EnablePart(vals[0].GetString(), vals[1].IsTrue());
        else
            config.Cabinet.EnablePart(vals[0].GetInt(), vals[1].IsTrue());

        return new BasicValue(1);
    }
}


class CommandFunctionCABPARTSGETCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSGETCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSGETCOORDINATE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
        {
            throw new Exception("AGEBasic can't access the Cabinet data.");
        }

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate X, Y, Z");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string coord = vals[1].GetString().ToUpper();

        float coordinateValue = 0f;
        switch (coord)
        {
            case "X":
                coordinateValue = child.localPosition.x;
                break;
            case "Y":
                coordinateValue = child.localPosition.y;
                break;
            case "Z":
                coordinateValue = child.localPosition.z;
                break;
            default:
                throw new Exception("cabPartsGetCoordinate: coordinate should be X, Y or Z");
        }

        return new BasicValue(coordinateValue);
    }
}


class CommandFunctionCABPARTSSETCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETCOORDINATE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
        {
            throw new Exception("AGEBasic can't access the Cabinet data.");
        }

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[2], " - coordinate value");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string coord = vals[1].GetString().ToUpper();
        Vector3 currentPosition = child.localPosition; // Use localPosition


        switch (coord)
        {
            case "X":
                currentPosition.x = (float)vals[2].GetNumber();
                break;
            case "Y":
                currentPosition.y = (float)vals[2].GetNumber();
                break;
            case "Z":
                currentPosition.z = (float)vals[2].GetNumber();
                break;
            default:
                throw new Exception("coordinate should be X, Y or Z");
        }


        child.localPosition = currentPosition; // Set localPosition
        return BasicValue.True;
    }
}

class CommandFunctionCABINSERTCOIN : CommandFunctionNoExpressionBase
{

    public CommandFunctionCABINSERTCOIN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABINSERTCOIN";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.CoinSlot == null)
            throw new Exception("Cabinet hasn't a coin slot.");

        config.CoinSlot.insertCoin();
        return BasicValue.True;
    }
}

class CommandFunctionCABPARTSSETROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETROTATION";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3); // Assuming the syntax is like: partNum, axis(X/Y/Z), angle
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
        {
            throw new Exception("AGEBasic can't access the Cabinet data.");
        }

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string axis = vals[1].GetString().ToUpper();
        float angle = (float)vals[2].GetNumber();

        // Calculate rotation relative to parent
        Vector3 localEuler = child.localEulerAngles;
        float newLocalAngle;
        switch (axis)
        {
            case "X":
                newLocalAngle = localEuler.x + angle;
                break;
            case "Y":
                newLocalAngle = localEuler.y + angle;
                break;
            case "Z":
                newLocalAngle = localEuler.z + angle;
                break;
            default:
                throw new Exception("cabPartsSetRotation: axis should be X, Y, or Z");
        }

        // Clamp the angle within 0-360 degrees
        newLocalAngle = Mathf.Repeat(newLocalAngle, 360f);

        // Set the new local rotation
        child.localRotation = Quaternion.Euler(newLocalAngle, localEuler.y, localEuler.z);

        return BasicValue.True;
    }
}

class CommandFunctionCABPARTSGETROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSGETROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSGETROTATION";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2); // Assuming the syntax is like: partNum, axis(X/Y/Z)
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string axis = vals[1].GetString().ToUpper();

        float rotationValue = 0f;
        switch (axis)
        {
            case "X":
                rotationValue = child.localEulerAngles.x;
                break;
            case "Y":
                rotationValue = child.localEulerAngles.y;
                break;
            case "Z":
                rotationValue = child.localEulerAngles.z;
                break;
            default:
                throw new Exception("cabPartsGetRotation: axis should be X, Y, or Z");
        }

        return new BasicValue(rotationValue);
    }
}


class CommandFunctionCABPARTSGETTRANSPARENCY : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABPARTSGETTRANSPARENCY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSGETTRANSPARENCY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        if (val.IsString())
            return new BasicValue(config.Cabinet.GetTransparencyPart(val.GetString()));

        return new BasicValue(config.Cabinet.GetTransparencyPart(val.GetInt()));

    }
}

class CommandFunctionCABPARTSSETTRANSPARENCY : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETTRANSPARENCY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETTRANSPARENCY";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2); // Assuming the syntax is like: partNum, axis(X/Y/Z)
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[1], " - percentaje from 0 to 100");

        int percentage = (int)Mathf.Clamp((int)vals[1].GetNumber(), 0, 100);

        if (vals[0].IsString())
            config.Cabinet.SetTransparencyPart(vals[0].GetString(), percentage);
        else
            config.Cabinet.SetTransparencyPart(vals[0].GetInt(), percentage);

        return new BasicValue(1);

    }
}

class CommandFunctionCABPARTSEMISSION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSEMISSION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSEMISSION";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2); // Assuming the syntax is like: partNum, axis(X/Y/Z)
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[1], " - true/false");


        if (vals[0].IsString())
            config.Cabinet.SetEmissionEnabledPart(vals[0].GetString(), vals[1].IsTrue());
        else
            config.Cabinet.SetEmissionEnabledPart(vals[0].GetInt(), vals[1].IsTrue());

        return new BasicValue(1);

    }
}


class CommandFunctionCABPARTSSETEMISSIONCOLOR : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETEMISSIONCOLOR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETEMISSIONCOLOR";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 4); // Assuming the syntax is like: partNum, axis(X/Y/Z)
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[1], "- R");
        FunctionHelper.ExpectedNumber(vals[2], "- G");
        FunctionHelper.ExpectedNumber(vals[3], "- B");

        float r = (float)vals[1].GetValueAsNumber();
        float g = (float)vals[2].GetValueAsNumber();
        float b = (float)vals[3].GetValueAsNumber();
        Color color = new Color(r, g, b);

        if (vals[0].IsString())
            config.Cabinet.SetEmissionColorPart(vals[0].GetString(), color);
        else
            config.Cabinet.SetEmissionColorPart(vals[0].GetInt(), color);

        return new BasicValue(1);

    }
}

class CommandFunctionCABPARTSSETCOLOR : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETCOLOR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETCOLOR";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 4); // Assuming the syntax is like: partNum, axis(X/Y/Z)
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[1], "- R");
        FunctionHelper.ExpectedNumber(vals[2], "- G");
        FunctionHelper.ExpectedNumber(vals[3], "- B");

        float r = (float)vals[1].GetValueAsNumber();
        float g = (float)vals[2].GetValueAsNumber();
        float b = (float)vals[3].GetValueAsNumber();
        Color color = new Color(r, g, b);

        if (vals[0].IsString())
            config.Cabinet.SetColorPart(vals[0].GetString(), color);
        else
            config.Cabinet.SetColorPart(vals[0].GetInt(), color);

        return new BasicValue(1);

    }
}

