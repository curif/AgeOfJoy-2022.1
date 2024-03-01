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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        // FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate X, Y or Z");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string coord = vals[1].GetString().ToUpper();
        if (coord == "X")
            return new BasicValue(child.position.x);
        else if (coord == "Y")
            return new BasicValue(child.position.y);
        else if (coord == "Z")
            return new BasicValue(child.position.z);
        else if (coord == "H")
            return new BasicValue(child.localPosition.y);

        throw new Exception("cabPartsGetCoordinate: coordinate should be X, Y, Z or H");

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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        // FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[2], " - coordinate value");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        Vector3 newPosition;
        string coord = vals[1].GetString().ToUpper();
        if (coord == "H")
        {
            newPosition = new Vector3(child.localPosition.x,
                                        child.localPosition.y,
                                        child.localPosition.z);
            newPosition.y = (float)vals[2].GetNumber();
            child.localPosition = newPosition;
            return BasicValue.True;
        }

        newPosition = new Vector3(child.position.x,
                                                    child.position.y,
                                                    child.position.z);
        if (coord == "X")
            newPosition.x = (float)vals[2].GetNumber();
        else if (coord == "Y")
            newPosition.y = (float)vals[2].GetNumber();
        else if (coord == "Z")
            newPosition.z = (float)vals[2].GetNumber();
        else
            throw new Exception("coordinate should be X, Y, Z or H");

        child.position = newPosition;
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        // FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string axis = vals[1].GetString().ToUpper();
        float angle = (float)vals[2].GetNumber();

        Quaternion newRotation = Quaternion.identity;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception("cabPartsSetRotation: axis should be X, Y, or Z");
        }

        child.rotation = newRotation;
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        // FunctionHelper.ExpectedNumber(vals[0], " - part number");
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
                rotationValue = child.rotation.eulerAngles.x;
                break;
            case "Y":
                rotationValue = child.rotation.eulerAngles.y;
                break;
            case "Z":
                rotationValue = child.rotation.eulerAngles.z;
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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
        ConfigManager.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
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

