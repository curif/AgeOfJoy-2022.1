using System;
using UnityEngine;

// CABROOMPARTS* — same manipulations as CABPARTS*, but targeting any cabinet in the current room by
// position instead of only the cabinet whose own AGEBasic program is running (config.Cabinet).
static class CabRoomPartsHelper
{
    public static Cabinet ResolveCabinet(ConfigurationCommands config, int position)
    {
        if (config?.CabinetsController == null)
            throw new Exception("AGEBasic can't access the room's cabinets.");

        Cabinet cabinet = config.CabinetsController.GetLiveCabinetByPosition(position);
        if (cabinet == null)
            throw new Exception($"No cabinet loaded at position {position}.");

        return cabinet;
    }

    public static CabinetPart ResolvePart(Cabinet cabinet, BasicValue val)
    {
        return val.IsString() ? cabinet.GetPartController(val.GetString()) : cabinet.GetPartController(val.GetInt());
    }
}

class CommandFunctionCABROOMPARTSCOUNT : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABROOMPARTSCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, " - cabinet position");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)val.GetNumber());
        return new BasicValue((double)cabinet.PartsCount());
    }
}

class CommandFunctionCABROOMPARTSNAME : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSNAME";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNumber(vals[1], " - part index");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        return new BasicValue(cabinet.PartsName((int)vals[1].GetNumber()), forceType: BasicValue.BasicValueType.String);
    }
}

class CommandFunctionCABROOMPARTSENABLE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSENABLE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSENABLE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        part.Enable(vals[2].IsTrue());

        return new BasicValue(1);
    }
}

class CommandFunctionCABROOMPARTSGETCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSGETCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSGETCOORDINATE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - coordinate X, Y, Z");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        return new BasicValue(part.GetCoordinate(vals[2].GetString().ToUpper()));
    }
}

class CommandFunctionCABROOMPARTSSETCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETCOORDINATE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 4);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[3], " - coordinate value");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        Transform child = vals[1].IsString() ? cabinet.PartsTransform(vals[1].GetString()) : cabinet.PartsTransform(vals[1].GetInt());

        string coord = vals[2].GetString().ToUpper();
        Vector3 currentPosition = child.localPosition;

        switch (coord)
        {
            case "X":
                currentPosition.x = (float)vals[3].GetNumber();
                break;
            case "Y":
                currentPosition.y = (float)vals[3].GetNumber();
                break;
            case "Z":
                currentPosition.z = (float)vals[3].GetNumber();
                break;
            default:
                throw new Exception("coordinate should be X, Y or Z");
        }

        child.localPosition = currentPosition;
        return BasicValue.True;
    }
}

class CommandFunctionCABROOMPARTSGETGLOBALCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSGETGLOBALCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSGETGLOBALCOORDINATE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - coordinate X, Y, Z");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        Transform child = vals[1].IsString() ? cabinet.PartsTransform(vals[1].GetString()) : cabinet.PartsTransform(vals[1].GetInt());

        string coord = vals[2].GetString().ToUpper();
        float coordinateValue;
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
                throw new Exception("coordinate should be X, Y or Z");
        }

        return new BasicValue(coordinateValue);
    }
}

class CommandFunctionCABROOMPARTSSETGLOBALCOORDINATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETGLOBALCOORDINATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETGLOBALCOORDINATE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 4);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - coordinate");
        FunctionHelper.ExpectedNumber(vals[3], " - coordinate value");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        Transform child = vals[1].IsString() ? cabinet.PartsTransform(vals[1].GetString()) : cabinet.PartsTransform(vals[1].GetInt());

        string coord = vals[2].GetString().ToUpper();
        Vector3 currentPosition = child.position;

        switch (coord)
        {
            case "X":
                currentPosition.x = (float)vals[3].GetNumber();
                break;
            case "Y":
                currentPosition.y = (float)vals[3].GetNumber();
                break;
            case "Z":
                currentPosition.z = (float)vals[3].GetNumber();
                break;
            default:
                throw new Exception("coordinate should be X, Y or Z");
        }

        child.position = currentPosition;
        return BasicValue.True;
    }
}

class CommandFunctionCABROOMPARTSSETROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 4);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[3], " - angle");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        part.RotateLocalEulerAngleByAxisFromOrigin(vals[2].GetString().ToUpper(), (float)vals[3].GetNumber());

        return BasicValue.True;
    }
}

class CommandFunctionCABROOMPARTSROTATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSROTATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSROTATE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 4);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[3], " - angle");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        part.RotateLocalEulerAngleByAxis(vals[2].GetString().ToUpper(), (float)vals[3].GetNumber());

        return BasicValue.True;
    }
}

class CommandFunctionCABROOMPARTSGETROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSGETROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSGETROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - axis (X, Y, Z)");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        float rotationValue = part.GetLocalRotationByAxis(vals[2].GetString().ToUpper());

        return new BasicValue(rotationValue);
    }
}

class CommandFunctionCABROOMPARTSSETGLOBALROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETGLOBALROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETGLOBALROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 4);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[3], " - angle");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        part.RotateWorldEulerAngleByAxis(vals[2].GetString().ToUpper(), (float)vals[3].GetNumber());

        return BasicValue.True;
    }
}

class CommandFunctionCABROOMPARTSGETGLOBALROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSGETGLOBALROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSGETGLOBALROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[2], " - axis (X, Y, Z)");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        float rotationValue = part.GetWorldRotationByAxis(vals[2].GetString().ToUpper());

        return new BasicValue(rotationValue);
    }
}

// CABROOMSETROTATION(position, axis, angle) — rotate the whole cabinet at that room position
// (root gameObject, not a part) to an absolute local angle, measured from its placement rotation.
class CommandFunctionCABROOMSETROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMSETROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMSETROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        cabinet.RotateLocalEulerAngleByAxisFromOrigin(vals[1].GetString().ToUpper(), (float)vals[2].GetNumber());

        return BasicValue.True;
    }
}

// CABROOMROTATE(position, axis, angle) — rotate the whole cabinet at that room position relative
// to its current local rotation.
class CommandFunctionCABROOMROTATE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMROTATE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMROTATE";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        cabinet.RotateLocalEulerAngleByAxis(vals[1].GetString().ToUpper(), (float)vals[2].GetNumber());

        return BasicValue.True;
    }
}

// CABROOMGETROTATION(position, axis) — local rotation delta (degrees) of the whole cabinet since placement.
class CommandFunctionCABROOMGETROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMGETROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMGETROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        float rotationValue = cabinet.GetLocalRotationByAxis(vals[1].GetString().ToUpper());

        return new BasicValue(rotationValue);
    }
}

// CABROOMSETGLOBALROTATION(position, axis, angle) — rotate the whole cabinet at that room position
// to an absolute world-space angle.
class CommandFunctionCABROOMSETGLOBALROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMSETGLOBALROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMSETGLOBALROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");
        FunctionHelper.ExpectedNumber(vals[2], " - angle");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        cabinet.RotateWorldEulerAngleByAxis(vals[1].GetString().ToUpper(), (float)vals[2].GetNumber());

        return BasicValue.True;
    }
}

// CABROOMGETGLOBALROTATION(position, axis) — world-space rotation delta (degrees) of the whole cabinet since placement.
class CommandFunctionCABROOMGETGLOBALROTATION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMGETGLOBALROTATION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMGETGLOBALROTATION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - axis (X, Y, Z)");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        float rotationValue = cabinet.GetWorldRotationByAxis(vals[1].GetString().ToUpper());

        return new BasicValue(rotationValue);
    }
}

class CommandFunctionCABROOMPARTSGETTRANSPARENCY : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSGETTRANSPARENCY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSGETTRANSPARENCY";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 2);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        return new BasicValue(part.GetTransparency());
    }
}

class CommandFunctionCABROOMPARTSSETTRANSPARENCY : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETTRANSPARENCY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETTRANSPARENCY";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNumber(vals[2], " - percentaje from 0 to 100");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);

        int percentage = (int)Mathf.Clamp((int)vals[2].GetNumber(), 0, 100);
        part.SetTransparency(ref percentage);

        return new BasicValue(1);
    }
}

class CommandFunctionCABROOMPARTSSETCOLOR : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETCOLOR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETCOLOR";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 5);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNumber(vals[2], "- R");
        FunctionHelper.ExpectedNumber(vals[3], "- G");
        FunctionHelper.ExpectedNumber(vals[4], "- B");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);

        Color color = ColorConverter.ConvertToColor(vals[2], vals[3], vals[4]);
        part.SetColor(color);

        return new BasicValue(1);
    }
}

class CommandFunctionCABROOMPARTSEMISSION : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSEMISSION(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSEMISSION";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 3);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");
        FunctionHelper.ExpectedNumber(vals[2], " - true/false");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);
        part.ActivateEmission(vals[2].IsTrue());

        return new BasicValue(1);
    }
}

class CommandFunctionCABROOMPARTSSETEMISSIONCOLOR : CommandFunctionExpressionListBase
{
    public CommandFunctionCABROOMPARTSSETEMISSIONCOLOR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABROOMPARTSSETEMISSIONCOLOR";
    }
    public override bool Parse(TokenConsumer tokens) => Parse(tokens, 5);

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - cabinet position");

        Cabinet cabinet = CabRoomPartsHelper.ResolveCabinet(config, (int)vals[0].GetNumber());
        CabinetPart part = CabRoomPartsHelper.ResolvePart(cabinet, vals[1]);

        Color color = ColorConverter.ConvertToColor(vals[2], vals[3], vals[4]);
        part.SetEmissionColor(color);

        return new BasicValue(1);
    }
}
