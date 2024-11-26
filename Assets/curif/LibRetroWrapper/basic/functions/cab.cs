using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static CabinetInformation;

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

        return new BasicValue(config.Cabinet.PartsName(partNum), forceType: BasicValue.BasicValueType.String);
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


class CommandFunctionCABPARTSLIST : CommandFunctionSingleExpressionBase
{

    public CommandFunctionCABPARTSLIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSLIST";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string partSeparator = val.GetString();
        string result = string.Empty;
        string sep = string.Empty;
        foreach (KeyValuePair<int, CabinetPart> entry in config.Cabinet.CabinetPartsControllersByIdx)
        {
            int idx = entry.Key;
            CabinetPart cp = entry.Value;
            result += sep + cp.GameObject.name;
            sep = partSeparator;
        }
        return new BasicValue(result);
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
        FunctionHelper.ExpectedAtLeast(vals, 1);
        // FunctionHelper.ExpectedNumber(vals[0], " - part number");
        // FunctionHelper.ExpectedNumber(vals[1], " - enabled true/false");

        if (vals[0].IsString())
            config.Cabinet.GetPartController(vals[0].GetString()).Enable(vals[1].IsTrue());
        else
            config.Cabinet.GetPartController(vals[0].GetInt()).Enable(vals[1].IsTrue());

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
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedAtLeast(vals, 1);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate X, Y, Z");

        if (vals[0].IsString())
            return new BasicValue(config.Cabinet.GetPartController(vals[0].GetString()).GetCoordinate(vals[1].GetString().ToUpper()));
        else
            return new BasicValue(config.Cabinet.GetPartController(vals[0].GetInt()).GetCoordinate(vals[1].GetString().ToUpper()));
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
        FunctionHelper.ExpectedAtLeast(vals, 2);
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

class CommandFunctionCABPARTSGETGLOBALCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSGETGLOBALCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSGETGLOBALCOORDINATE";
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
        FunctionHelper.ExpectedAtLeast(vals, 1);
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
                coordinateValue = child.position.x;
                break;
            case "Y":
                coordinateValue = child.position.y;
                break;
            case "Z":
                coordinateValue = child.position.z;
                break;
            default:
                throw new Exception("cabPartsGetGlobalCoordinate: coordinate should be X, Y or Z");
        }

        return new BasicValue(coordinateValue);
    }
}

class CommandFunctionCABPARTSSETGLOBALCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETGLOBALCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETGLOBALCOORDINATE";
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
        FunctionHelper.ExpectedAtLeast(vals, 2);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[2], " - coordinate value");

        Transform child;
        if (vals[0].IsString())
            child = config.Cabinet.PartsTransform(vals[0].GetString());
        else
            child = config.Cabinet.PartsTransform(vals[0].GetInt());

        string coord = vals[1].GetString().ToUpper();
        Vector3 currentPosition = child.position;

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

        child.position = currentPosition;
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
        FunctionHelper.ExpectedAtLeast(vals, 3);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        CabinetPart part;
        if (vals[0].IsString())
            part = config.Cabinet.GetPartController(vals[0].GetString());
        else
            part = config.Cabinet.GetPartController(vals[0].GetInt());
        string axis = vals[1].GetString().ToUpper();
        float rotationValue = (float)vals[2].GetNumber();

        part.RotateLocalEulerAngleByAxisFromOrigin(axis, rotationValue);

        return BasicValue.True;
    }
}

class CommandFunctionCABPARTSROTATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSROTATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSROTATE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3); // Assuming the syntax is like: partNum, axis(X/Y/Z), angle
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        CabinetPart part;
        if (vals[0].IsString())
            part = config.Cabinet.GetPartController(vals[0].GetString());
        else
            part = config.Cabinet.GetPartController(vals[0].GetInt());
        string axis = vals[1].GetString().ToUpper();
        float rotationValue = (float)vals[2].GetNumber();

        part.RotateLocalEulerAngleByAxis(axis, rotationValue);

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

        CabinetPart part;
        if (vals[0].IsString())
            part = config.Cabinet.GetPartController(vals[0].GetString());
        else
            part = config.Cabinet.GetPartController(vals[0].GetInt());
        string axis = vals[1].GetString().ToUpper();
        float rotationValue = part.GetLocalRotationByAxis(axis);

        return new BasicValue(rotationValue);
    }
}


class CommandFunctionCABPARTSSETGLOBALROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSSETGLOBALROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSSETGLOBALROTATION";
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

        CabinetPart part;
        if (vals[0].IsString())
            part = config.Cabinet.GetPartController(vals[0].GetString());
        else
            part = config.Cabinet.GetPartController(vals[0].GetInt());
        string axis = vals[1].GetString().ToUpper();
        float rotationValue = (float)vals[2].GetNumber();

        part.RotateWorldEulerAngleByAxis(axis, rotationValue);

        return BasicValue.True;
    }
}

class CommandFunctionCABPARTSGETGLOBALROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABPARTSGETGLOBALROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSGETGLOBALROTATION";
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

        CabinetPart part;
        if (vals[0].IsString())
            part = config.Cabinet.GetPartController(vals[0].GetString());
        else
            part = config.Cabinet.GetPartController(vals[0].GetInt());
        string axis = vals[1].GetString().ToUpper();
        float rotationValue = part.GetWorldRotationByAxis(axis);

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
            return new BasicValue(config.Cabinet.GetPartController(val.GetString()).GetTransparency());
        
        return new BasicValue(config.Cabinet.GetPartController(val.GetInt()).GetTransparency());

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
            config.Cabinet.GetPartController(vals[0].GetString()).SetTransparency(ref percentage);
        else 
            config.Cabinet.GetPartController(vals[0].GetInt()).SetTransparency(ref percentage);
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
            config.Cabinet.GetPartController(vals[0].GetString()).ActivateEmission(vals[1].IsTrue());
        else
            config.Cabinet.GetPartController(vals[0].GetInt()).ActivateEmission(vals[1].IsTrue());
        
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
        Color color = ColorConverter.ConvertToColor(vals[1], vals[2], vals[3]);

        if (vals[0].IsString())
            config.Cabinet.GetPartController(vals[0].GetString()).SetEmissionColor(color);
        else
            config.Cabinet.GetPartController(vals[0].GetInt()).SetEmissionColor(color);
       
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

        Color color = ColorConverter.ConvertToColor(vals[1], vals[2], vals[3]);

        if (vals[0].IsString())
            config.Cabinet.GetPartController(vals[0].GetString()).SetColor(color);
        else
            config.Cabinet.GetPartController(vals[0].GetInt()).SetColor(color);

        return new BasicValue(1);

    }
}

// PHYSICAL ----------------------

class CommandFunctionCABPARTSPHYACTIVATEGRAVITY : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABPARTSPHYACTIVATEGRAVITY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSPHYACTIVATEGRAVITY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        if (val.IsString())
            config.Cabinet.PhyActivateGravity(val.GetString());
        else
            config.Cabinet.PhyActivateGravity(val.GetInt());

        return null;
    }
}

class CommandFunctionCABPARTSPHYDEACTIVATEGRAVITY : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABPARTSPHYDEACTIVATEGRAVITY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABPARTSPHYDEACTIVATEGRAVITY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        if (val.IsString())
            config.Cabinet.PhyDeactivateGravity(val.GetString());
        else
            config.Cabinet.PhyDeactivateGravity(val.GetInt());

        return null;
    }
}

public static class ColorConverter
{
    public static Color ConvertToColor(BasicValue bvr, BasicValue bvg, BasicValue bvb)
    {
        FunctionHelper.ExpectedNumber(bvr, "- R");
        FunctionHelper.ExpectedNumber(bvg, "- G");
        FunctionHelper.ExpectedNumber(bvb, "- B");

        byte r = (byte)bvr.GetNumber();
        byte g = (byte)bvg.GetNumber();
        byte b = (byte)bvb.GetNumber();

        // Ensure the input values are within the valid range
        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
            throw new ArgumentException("RGB values must be between 0 and 255");

        return new Color32(r, g, b, 255);
    }
}