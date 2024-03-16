using System;
using System.IO;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;

class CommandFunctionPLAYERGETHEIGHT : CommandFunctionNoExpressionBase
{
    public CommandFunctionPLAYERGETHEIGHT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PLAYERGETHEIGHT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.Player == null)
            return new BasicValue(0);

        return new BasicValue((double)config.Player.CameraYOffset);
    }
}


class CommandFunctionPLAYERSETHEIGHT : CommandFunctionSingleExpressionBase
{
    public CommandFunctionPLAYERSETHEIGHT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PLAYERSETHEIGHT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        if (config?.Player == null)
            return new BasicValue(0);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, "Player height");
        float playerHeight = (float)val.GetValueAsNumber();

        config.Player.CameraYOffset = playerHeight;

        return new BasicValue(1);
    }

}


class CommandFunctionPLAYERGETCOORDINATE : CommandFunctionSingleExpressionBase
{

    public CommandFunctionPLAYERGETCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PLAYERGETCOORDINATE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.PlayerGameObject == null)
            throw new Exception("AGEBasic can't access the Player data.");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, " - coordinate X or Z");

        Transform playerTransform = config.PlayerGameObject.transform;
        string coord = val.GetString().ToUpper();
        if (coord == "X")
            return new BasicValue(playerTransform.position.x);
        else if (coord == "Z")
            return new BasicValue(playerTransform.position.z);

        throw new Exception("coordinate should be X or Z");

    }
}


class CommandFunctionPLAYERSETCOORDINATE : CommandFunctionExpressionListBase
{

    public CommandFunctionPLAYERSETCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PLAYERSETCOORDINATE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.PlayerOrigin == null)
            throw new Exception("AGEBasic can't access the Player data.");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[1], " - coordinate value");

        string coordType = vals[0].GetString().ToUpper();
        // Transform playerTransform = config.PlayerGameObject.transform;
        Vector3 newPosition = new Vector3(config.PlayerOrigin.Origin.transform.position.x,
                                            config.PlayerOrigin.Origin.transform.position.y,
                                            config.PlayerOrigin.Origin.transform.position.z);
        if (coordType == "X")
            newPosition.x = (float)vals[1].GetNumber();
        else if (coordType == "Z")
            newPosition.z = (float)vals[1].GetNumber();
        else
            throw new Exception("coordinate should be X, Z");

        // config.PlayerGameObject.transform.position = newPosition;
        config.PlayerOrigin.Origin.transform.position = newPosition;
        return BasicValue.True;
    }
}


class CommandFunctionPLAYERLOOKAT : CommandFunctionSingleExpressionBase
{

    public CommandFunctionPLAYERLOOKAT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "PLAYERLOOKAT";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.PlayerOrigin == null)
            throw new Exception("AGEBasic can't access the Player data.");
        if (config?.Cabinet == null)
            throw new Exception("AGEBasic can't access the Cabinet data.");

        BasicValue val = expr.Execute(vars);

        Transform objectToLookAt;
        if (val.IsString())
            objectToLookAt = config.Cabinet.PartsTransform(val.GetString());
        else
            objectToLookAt = config.Cabinet.PartsTransform(val.GetInt());

        if (objectToLookAt == null)
            throw new Exception("part not found in cabinet.");

        // Calculate the direction from the player's head to the object
        Vector3 directionToLookAt = objectToLookAt.position - config.PlayerOrigin.Origin.transform.position;

        // Ignore changes in the y-axis
        directionToLookAt.y = 0;

        // Rotate the player's head towards the calculated direction
        Quaternion newRotation = Quaternion.LookRotation(directionToLookAt);
        config.PlayerOrigin.Origin.transform.rotation = newRotation;

        // config.PlayerOrigin.Origin.transform.LookAt(objectToLookAt.transform);

        return BasicValue.True;
    }
}
