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

        return new BasicValue((double)config.Cabinet.transform.childCount);
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
        // Check if the parent has at least 3 children
        if (config.Cabinet.transform.childCount < partNum + 1)
            return new BasicValue("");

        Transform child = config.Cabinet.transform.GetChild(partNum);
        return new BasicValue(child.name);
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
        Transform child = config.Cabinet.transform.Find(partName);
        return new BasicValue(child == null ? -1 : child.GetSiblingIndex());
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
        FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNumber(vals[1], " - enabled true/false");

        int partNum = (int)vals[0].GetNumber();
        // Check if the parent has at least N children
        if (config.Cabinet.transform.childCount < partNum + 1)
            return new BasicValue(-1);

        Transform child = config.Cabinet.transform.GetChild(partNum);
        child.gameObject.SetActive(vals[1].GetNumber() != 0);

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
        FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate X, Y or Z");

        int partNum = (int)vals[0].GetNumber();
        // Check if the parent has at least N children
        if (config.Cabinet.transform.childCount < partNum + 1)
            throw new Exception("cabPartsGetCoordinate: invalid part number");

        Transform child = config.Cabinet.transform.GetChild(partNum);
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
        FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[2], " - coordinate value");

        int partNum = (int)vals[0].GetNumber();
        // Check if the parent has at least N children
        if (config.Cabinet.transform.childCount < partNum + 1)
            throw new Exception("cabPartsSetCoordinate: invalid part number");

        Transform child = config.Cabinet.transform.GetChild(partNum);
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
        FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        int partNum = (int)vals[0].GetNumber();
        // Check if the parent has at least N children
        if (config.Cabinet.transform.childCount < partNum + 1)
            throw new Exception("cabPartsSetRotation: invalid part number");

        Transform child = config.Cabinet.transform.GetChild(partNum);
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
        FunctionHelper.ExpectedNumber(vals[0], " - part number");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");

        int partNum = (int)vals[0].GetNumber();
        // Check if the parent has at least N children
        if (config.Cabinet.transform.childCount < partNum + 1)
            throw new Exception("cabPartsGetRotation: invalid part number");

        Transform child = config.Cabinet.transform.GetChild(partNum);
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

