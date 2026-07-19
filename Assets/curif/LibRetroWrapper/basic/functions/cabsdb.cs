using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

class CommandFunctionCABDBCOUNT : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABDBCOUNT(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBCOUNT";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue(0);

        int count = config.GameRegistry.CountCabinets();
        if (count < 0)
            throw new Exception("error access cabinetsDB folder");

        return new BasicValue((double)count);
    }
}

class CommandFunctionCABDBCOUNTINROOM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBCOUNTINROOM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBCOUNTINROOM";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue(0);

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        return new BasicValue(
            (double)config.GameRegistry.GetCabinetsCountInRoom(val.GetValueAsString())
            );
    }
}

class CommandFunctionCABDBGETNAME : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBGETNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBGETNAME";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue("");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val);

        return new BasicValue(
                config.GameRegistry.GetCabinetNameByPosition((int)val.GetValueAsNumber()), forceType: BasicValue.BasicValueType.String
            );
    }
}


// CABDBGETINFO(name$, "path") -> reads description.yaml for cabinet "name" and returns
// the value at the field path, e.g. CABDBGETINFO("pacman", "crt.type"),
// CABDBGETINFO("pacman", "year"), CABDBGETINFO("pacman", "parts[3].art.file")
// (list indices accept either "parts[3]" or "parts.3").
// Unlike most CABDB* functions, an invalid cabinet name or path does NOT stop the running
// AGEBasic program: the problem is logged to console and "" is returned instead.
class CommandFunctionCABDBGETINFO : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBGETINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBGETINFO";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - cabinet name");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - property path, e.g. \"crt.type\"");

        string cabName = vals[0].GetString();
        string path = vals[1].GetString();

        try
        {
            string cabPath = Path.Combine(ConfigManager.CabinetsDB, cabName);
            CabinetInformation info = CabinetInformation.fromYaml(cabPath);
            if (info == null)
                throw new Exception($"cabinet '{cabName}' not found or its description.yaml couldn't be parsed");

            object value = CabinetInfoReflection.Resolve(info, path);
            return CabinetInfoReflection.ToBasicValue(value, path);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[{CmdToken}] cab:'{cabName}' path:'{path}' ", e);
            return new BasicValue("", forceType: BasicValue.BasicValueType.String);
        }
    }
}

// CABDBSETINFO(cabinetName, path, value): writes 'value' into cabinetName's description.yaml
// at the given dotted/bracketed field path (same syntax as CABDBGETINFO). Missing intermediate
// objects/lists are auto-created; a list index equal to the list's current length appends.
// Rewrites the whole yaml file (comments/key order/unknown keys are not preserved) and does
// NOT touch the live 3D cabinet - pair with WORKSHOPRELOAD() to see the change.
// Fails soft: logs the problem and returns 0 instead of stopping the running AGEBasic program.
class CommandFunctionCABDBSETINFO : CommandFunctionExpressionListBase
{
    private static readonly YamlDotNet.Serialization.ISerializer writeSerializer =
        new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
            .Build();

    public CommandFunctionCABDBSETINFO(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBSETINFO";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - cabinet name");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - property path, e.g. \"crt.type\"");

        string cabName = vals[0].GetString();
        string path = vals[1].GetString();
        string cabPath = Path.Combine(ConfigManager.CabinetsDB, cabName);

        try
        {
            string yamlPath = Path.Combine(cabPath, "description.yaml");
            // Raw deserialize (no Validate(), no cache): fromYaml()/Validate() derives rom = roms[0]
            // and other computed state we must not write back to disk.
            CabinetInformation info = YamlUtils.Parse<CabinetInformation>(yamlPath);

            CabinetInfoReflection.Set(info, path, vals[2]);

            File.WriteAllText(yamlPath, writeSerializer.Serialize(info));
            ConfigManager.CabinetInformationCache.Remove(Path.GetFullPath(cabPath));

            return BasicValue.True;
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[{CmdToken}] cab:'{cabName}' path:'{path}' ", e);
            return BasicValue.False;
        }
    }
}

static class CabinetInfoReflection
{
    const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

    public static object Resolve(object root, string path)
    {
        // normalize bracket indices ("parts[3]") to dotted indices ("parts.3")
        string normalizedPath = System.Text.RegularExpressions.Regex.Replace(path, @"\[(\d+)\]", ".$1");

        object current = root;
        foreach (string segment in normalizedPath.Split('.'))
        {
            if (string.IsNullOrEmpty(segment))
                throw new Exception($"invalid property path: '{path}'");

            if (current == null)
                return null;

            if (current is ICollection col && segment.Equals("count", StringComparison.OrdinalIgnoreCase))
            {
                current = col.Count;
                continue;
            }

            if (current is IList list && int.TryParse(segment, out int index))
            {
                if (index < 0 || index >= list.Count)
                    throw new Exception($"index {index} out of range in path '{path}'");
                current = list[index];
                continue;
            }

            Type type = current.GetType();
            FieldInfo field = type.GetField(segment, Flags);
            if (field != null)
            {
                current = field.GetValue(current);
                continue;
            }

            PropertyInfo prop = type.GetProperty(segment, Flags);
            if (prop != null)
            {
                current = prop.GetValue(current);
                continue;
            }

            throw new Exception($"'{segment}' not found on {type.Name} (path: '{path}')");
        }
        return current;
    }

    // Writes 'value' into 'root' at the dotted/bracketed path, auto-creating missing
    // intermediate objects/lists where possible (mirrors Resolve's path syntax).
    // An index equal to a list's current Count appends a new element.
    public static void Set(object root, string path, BasicValue value)
    {
        string normalizedPath = System.Text.RegularExpressions.Regex.Replace(path, @"\[(\d+)\]", ".$1");
        string[] segments = normalizedPath.Split('.');

        object current = root;
        for (int i = 0; i < segments.Length - 1; i++)
        {
            string segment = segments[i];
            if (string.IsNullOrEmpty(segment))
                throw new Exception($"invalid property path: '{path}'");

            if (current is IDictionary)
                throw new Exception($"dictionary paths are not supported (path: '{path}')");

            if (current is IList list)
            {
                if (!int.TryParse(segment, out int index))
                    throw new Exception($"'{segment}' is not a valid list index (path: '{path}')");

                Type elemType = ListElementType(list);
                if (index == list.Count)
                {
                    object created = CreateInstance(elemType, $"list element for path '{path}'");
                    list.Add(created);
                    current = created;
                }
                else if (index < 0 || index >= list.Count)
                {
                    throw new Exception($"index {index} out of range in path '{path}' " +
                        $"(list has {list.Count} item(s); use index {list.Count} to append)");
                }
                else
                {
                    current = list[index];
                }
                continue;
            }

            Type type = current.GetType();
            FieldInfo field = type.GetField(segment, Flags);
            PropertyInfo prop = field == null ? type.GetProperty(segment, Flags) : null;
            if (field == null && prop == null)
                throw new Exception($"'{segment}' not found on {type.Name} (path: '{path}')");

            Type memberType = field != null ? field.FieldType : prop.PropertyType;
            object memberValue = field != null ? field.GetValue(current) : prop.GetValue(current);

            if (memberValue == null)
            {
                memberValue = CreateInstance(memberType, $"'{segment}' (path: '{path}')");
                if (field != null)
                    field.SetValue(current, memberValue);
                else
                    prop.SetValue(current, memberValue);
            }

            current = memberValue;
        }

        string lastSegment = segments[segments.Length - 1];
        if (string.IsNullOrEmpty(lastSegment))
            throw new Exception($"invalid property path: '{path}'");

        if (current is IDictionary)
            throw new Exception($"dictionary paths are not supported (path: '{path}')");

        if (lastSegment.Equals("count", StringComparison.OrdinalIgnoreCase) && current is ICollection)
            throw new Exception($"'count' is read-only (path: '{path}')");

        if (current is IList lastList)
        {
            if (!int.TryParse(lastSegment, out int index))
                throw new Exception($"'{lastSegment}' is not a valid list index (path: '{path}')");

            Type elemType = ListElementType(lastList);
            object converted = ConvertBasicValue(value, elemType, path);

            if (index == lastList.Count)
                lastList.Add(converted);
            else if (index < 0 || index >= lastList.Count)
                throw new Exception($"index {index} out of range in path '{path}' " +
                    $"(list has {lastList.Count} item(s); use index {lastList.Count} to append)");
            else
                lastList[index] = converted;
            return;
        }

        Type ownerType = current.GetType();
        FieldInfo lastField = ownerType.GetField(lastSegment, Flags);
        PropertyInfo lastProp = lastField == null ? ownerType.GetProperty(lastSegment, Flags) : null;
        if (lastField == null && lastProp == null)
            throw new Exception($"'{lastSegment}' not found on {ownerType.Name} (path: '{path}')");

        Type targetType = lastField != null ? lastField.FieldType : lastProp.PropertyType;
        object convertedValue = ConvertBasicValue(value, targetType, path);

        if (lastField != null)
            lastField.SetValue(current, convertedValue);
        else if (lastProp.CanWrite)
            lastProp.SetValue(current, convertedValue);
        else
            throw new Exception($"'{lastSegment}' is read-only on {ownerType.Name} (path: '{path}')");
    }

    private static Type ListElementType(IList list)
    {
        Type listType = list.GetType();
        Type[] args = listType.IsGenericType ? listType.GetGenericArguments() : null;
        if (args == null || args.Length != 1)
            throw new Exception($"cannot determine element type of {listType.Name}");
        return args[0];
    }

    private static object CreateInstance(Type type, string context)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch (MissingMethodException)
        {
            throw new Exception($"cannot auto-create {type.Name} for {context} - assign a leaf value directly");
        }
    }

    private static object ConvertBasicValue(BasicValue v, Type targetType, string path)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType);
        Type effectiveType = underlying ?? targetType;

        if (effectiveType == typeof(string))
            return v.GetString();
        if (effectiveType == typeof(bool))
            return v.IsString() ? v.GetString().Equals("true", StringComparison.OrdinalIgnoreCase) : v.GetValueAsNumber() != 0;
        if (effectiveType == typeof(double))
            return v.GetValueAsNumber();
        if (effectiveType == typeof(float))
            return (float)v.GetValueAsNumber();
        if (effectiveType == typeof(int))
            return (int)v.GetValueAsNumber();
        if (effectiveType == typeof(uint))
            return (uint)v.GetValueAsNumber();

        throw new Exception($"path '{path}' resolves to a {targetType.Name}, not a settable value - " +
            $"append a field name, e.g. '{path}.<field>'");
    }

    public static BasicValue ToBasicValue(object value, string path)
    {
        switch (value)
        {
            case null:
                return new BasicValue("", forceType: BasicValue.BasicValueType.String);
            case bool b:
                return new BasicValue(b);
            case int i:
                return new BasicValue((double)i);
            case uint ui:
                return new BasicValue((double)ui);
            case float f:
                return new BasicValue((double)f);
            case double d:
                return new BasicValue(d);
            case string s:
                return new BasicValue(s, forceType: BasicValue.BasicValueType.String);
            case IList list:
                throw new Exception($"path '{path}' resolves to a list of {list.Count} item(s), not a value - " +
                    $"append an index, e.g. '{path}[0]' or '{path}.0'");
            default:
                throw new Exception($"path '{path}' resolves to a {value.GetType().Name} object, not a value - " +
                    $"append a field name, e.g. '{path}.<field>'");
        }
    }
}

class CommandFunctionCABDBSEARCH : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBSEARCH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBSEARCH";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue("");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - cab name (part of)");
        FunctionHelper.ExpectedString(vals[1], " - separator");

        return new BasicValue(
                string.Join(vals[1].GetValueAsString(), 
                            config.GameRegistry.GetAllPrefixMatches(vals[0].GetValueAsString())), 
                forceType: BasicValue.BasicValueType.String
            );
    }
}



class CommandFunctionCABDBSEARCHARRAY : CommandFunctionSingleExpressionBase
{
    public CommandFunctionCABDBSEARCHARRAY(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBSEARCHARRAY";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue("");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - cab name (part of)");
        return new BasicValue(config.GameRegistry.GetAllPrefixMatches(val.GetString()).ToArray());
    }
}


/*
class CommandFunctionCABDBGET : CommandFunctionExpressionListBase, ICommandFunctionList
{
    public CommandFunctionCABDBGET(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBGET";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        throw new Exception("Bad function implementation, should return a list");
    }

    public BasicValue[] ExecuteList(BasicVars vars)
    {
        BasicValue[] ret = new BasicValue[2]
        {
                new BasicValue(""),
                new BasicValue(-1)
        };

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return ret;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");

        CabinetPosition cabpos = 
                config.GameRegistry.GetCabinetPositionInRoom(
                                        (int)vals[0].GetValueAsNumber(),
                                        (int)vals[1].GetValueAsNumber()
                                        );
        if (cabpos == null)
            return ret;

        ret[0].SetValue(cabpos.CabinetDBName);
        ret[1].SetValue(cabpos.Position);

        return ret;
    }
}
*/

class CommandFunctionCABDBDELETE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBDELETE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBDELETE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return BasicValue.False;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");

        string room = vals[0].GetValueAsString();
        int position = (int)vals[1].GetValueAsNumber();
        CabinetPosition cabpos = config.GameRegistry.DeleteCabinetPositionInRoom(position, room);
        if (cabpos == null)
            throw new Exception($"{CmdToken}: {room} pos:{position} not found ");

        return BasicValue.True;
    }
}

class CommandFunctionCABDBADD : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBADD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBADD";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3);
    }
    public override BasicValue Execute(BasicVars vars)
    {

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return BasicValue.False;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");
        FunctionHelper.ExpectedString(vals[2], " - cabinet name");

        string room = vals[0].GetValueAsString();
        int position = (int)vals[1].GetValueAsNumber();

        CabinetPosition cabpos = new();
        cabpos.CabinetDBName = vals[2].GetValueAsString();
        cabpos.Position = position;
        cabpos.Room = room;
        config.GameRegistry.Add(cabpos); //throws when repeated

        return BasicValue.True;
    }
}

class CommandFunctionCABDBASSIGN : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBASSIGN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBASSIGN";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 3);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return BasicValue.False;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");
        
        //1942 issue.
        // FunctionHelper.ExpectedString(vals[2], " - new cabinet name");
        
        config.GameRegistry.AssignOrAddCabinet(vals[0].GetString(), 
                                                (int)vals[1].GetNumber(), 
                                                vals[2].CastTo(BasicValue.BasicValueType.String).GetString());

        return BasicValue.True;
    }
}

class CommandFunctionCABDBGETASSIGNED : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBGETASSIGNED(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CommandFunctionCABDBGETASSIGNED";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return new BasicValue("");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");

        string room = vals[0].GetString();
        int position = (int)vals[1].GetNumber();

        CabinetPosition cabPos = config.GameRegistry.GetCabinetPositionInRoom(position, room);
        if (cabPos == null)
            return new BasicValue("");

        return new BasicValue(cabPos.CabinetDBName, forceType: BasicValue.BasicValueType.String);
    }
}


class CommandFunctionCABDBSAVE : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABDBSAVE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBSAVE";
    }

    public override BasicValue Execute(BasicVars vars)
    {

        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return BasicValue.False;

        config.GameRegistry.Persist();
        return BasicValue.True;
    }
}

class CommandFunctionCABDBRELOAD : CommandFunctionNoExpressionBase
{
    public CommandFunctionCABDBRELOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBRELOAD";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] ");
        if (config?.GameRegistry == null)
            return BasicValue.False;

        config.GameRegistry.Recover();

        if (config.CabinetsController != null)
            _ = config.CabinetsController.SyncRoomFromRegistry();

        return BasicValue.True;
    }
}

/*
class CommandFunctionCABDBREPLACE : CommandFunctionExpressionListBase
{
    public CommandFunctionCABDBREPLACE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "CABDBREPLACE";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        if (config?.GameRegistry == null)
            return new BasicValue(0);

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0], " - room name");
        FunctionHelper.ExpectedNumber(vals[1], " - cabinet position");
        FunctionHelper.ExpectedString(vals[2], " - new cabinet name");

        string roomName = vals[0].GetValueAsString();
        if (string.IsNullOrEmpty(roomName))
            return new BasicValue(0); //fail

        string cabinetDBName = vals[2].GetValueAsString();
        if (!config.GameRegistry.CabinetExists(cabinetDBName))
            return new BasicValue(0); //fail

        int position = (int)vals[1].GetValueAsNumber();

        CabinetPosition toAdd = new();
        toAdd.Room = roomName;
        toAdd.Position = position;
        toAdd.CabinetDBName = cabinetDBName;

        CabinetPosition toBeReplaced = config.GameRegistry.GetCabinetPositionInRoom(position, roomName);
        AGEBasicDebug.WriteConsole($"[CommandFunctionCABDBREPLACE] [{toBeReplaced}] by [{toAdd}] ");
        config.GameRegistry.Replace(toBeReplaced, toAdd); //persists changes

        return new BasicValue(1);
    }
}
*/