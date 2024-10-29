using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class CommandFunctionLEN : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLEN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LEN";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        double ret = val.GetValueAsString().Length;
        return new BasicValue(ret);
    }
}
class CommandFunctionUCASE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionUCASE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "UCASE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string ret = val.GetValueAsString().ToUpper();
        return new BasicValue(ret);
    }
}


class CommandFunctionLCASE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLCASE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LCASE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string ret = val.GetValueAsString().ToLower();
        return new BasicValue(ret);
    }
}

class CommandFunctionRTRIM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionRTRIM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "RTRIM";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string ret = val.GetValueAsString().TrimEnd();

        return new BasicValue(ret);
    }
}

class CommandFunctionLTRIM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionLTRIM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "LTRIM";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);

        string ret = val.GetValueAsString().TrimStart();

        return new BasicValue(ret);
    }
}

class CommandFunctionTRIM : CommandFunctionSingleExpressionBase
{
    public CommandFunctionTRIM(ConfigurationCommands config) : base(config)
    {
        cmdToken = "TRIM";
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val);
        string ret = val.GetValueAsString().Trim();

        return new BasicValue(ret);
    }
}

class CommandFunctionSUBSTR : CommandFunctionExpressionListBase
{
    public CommandFunctionSUBSTR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "SUBSTR";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedNumber(vals[1], " - start");
        FunctionHelper.ExpectedNumber(vals[2], " - length");

        string input = vals[0].GetValueAsString();
        int startIndex = (int)vals[1].GetValueAsNumber();
        int length = (int)vals[2].GetValueAsNumber();

        if (length < 0)
            length = 0;

        // Adjust length if it extends beyond the end of the string
        if (startIndex + length > input.Length)
        {
            length = input.Length - startIndex;
        }

        string ret = input.Substring(startIndex, length);

        return new BasicValue(ret);
    }
}

class CommandFunctionSTR : CommandFunctionSingleExpressionBase
{
    public CommandFunctionSTR(ConfigurationCommands config) : base(config)
    {
        cmdToken = "STR";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");
        BasicValue val = expr.Execute(vars);
        BasicValue ret = new BasicValue(val);
        ret.CastTo(BasicValue.BasicValueType.String);
        return ret;
    }
}

class CommandFunctionGETMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionGETMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedNumber(vals[1], " - index");
        FunctionHelper.ExpectedString(vals[2], " - separator");

        string input = vals[0].GetValueAsString();
        int memberIndex = (int)vals[1].GetValueAsNumber();
        char separator = vals[2].GetValueAsString()[0];

        int currentIndex = 0;
        int start = 0;

        for (int i = 0; i <= input.Length; i++)
        {
            // Check if the current character is the separator or if we've reached the end of the string
            if (i == input.Length || input[i] == separator)
            {
                if (currentIndex == memberIndex)
                {
                    // Use a span to avoid string allocation
                    var retSpan = input.AsSpan(start, i - start);
                    return new BasicValue(new string(retSpan));
                }

                // Update start position after the separator for the next part
                start = i + 1;
                currentIndex++;
            }
        }

        // If we exit the loop without having returned, the index was out of bounds
        return new BasicValue(""); // Return an empty string or handle the error accordingly
    }
}

class CommandFunctionCOUNTMEMBERS : CommandFunctionExpressionListBase
{
    public CommandFunctionCOUNTMEMBERS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "COUNTMEMBERS";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }
    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - separator");

        string input = vals[0].GetValueAsString();
        char separator = vals[1].GetValueAsString()[0];

        int count = 0;
        bool inSegment = false;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == separator)
            {
                // End of a segment
                if (inSegment)
                {
                    count++;
                    inSegment = false;
                }
            }
            else
            {
                // Inside a segment
                inSegment = true;
            }
        }

        // Account for the last segment if it didn't end with a separator
        if (inSegment) count++;

        return new BasicValue(count);
    }
}

class CommandFunctionISMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionISMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ISMEMBER";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - string");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        char separator = vals[2].GetValueAsString()[0];

        int start = 0;

        for (int i = 0; i <= separatedList.Length; i++)
        {
            if (i == separatedList.Length || separatedList[i] == separator)
            {
                // Get the current segment as a span to avoid allocation
                var segmentSpan = separatedList.AsSpan(start, i - start);

                // Check if the segment matches the member
                if (segmentSpan.SequenceEqual(member.AsSpan()))
                {
                    return new BasicValue(true);
                }

                // Move start to the next character after the separator
                start = i + 1;
            }
        }

        // If no match is found, return false
        return new BasicValue(false);
    }
}

class CommandFunctionINDEXMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionINDEXMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "INDEXMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }


    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - char");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        char separator = vals[2].GetValueAsString()[0];

        int start = 0;
        int index = 0;

        for (int i = 0; i <= separatedList.Length; i++)
        {
            if (i == separatedList.Length || separatedList[i] == separator)
            {
                var segmentSpan = separatedList.AsSpan(start, i - start);

                if (segmentSpan.SequenceEqual(member.AsSpan()))
                {
                    return new BasicValue(index);
                }

                start = i + 1;
                index++;
            }
        }

        // Return -1 if the member is not found in the list
        return new BasicValue(-1);
    }
}

class CommandFunctionREMOVEMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionREMOVEMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "REMOVEMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - string");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        char separator = vals[2].GetValueAsString()[0];

        var result = new StringBuilder();
        int start = 0;

        for (int i = 0; i <= separatedList.Length; i++)
        {
            if (i == separatedList.Length || separatedList[i] == separator)
            {
                var segmentSpan = separatedList.AsSpan(start, i - start);

                if (!segmentSpan.SequenceEqual(member.AsSpan()))
                {
                    if (result.Length > 0)
                    {
                        result.Append(separator);
                    }
                    result.Append(segmentSpan);
                }

                start = i + 1;
            }
        }

        return new BasicValue(result.ToString());
    }
}


class CommandFunctionADDMEMBER : CommandFunctionExpressionListBase
{
    public CommandFunctionADDMEMBER(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ADDMEMBER";
    }
    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedString(vals[0]);
        FunctionHelper.ExpectedString(vals[1], " - list");
        FunctionHelper.ExpectedString(vals[2], " - separator");

        string separatedList = vals[0].GetValueAsString();
        string member = vals[1].GetValueAsString();
        char separator = vals[2].GetValueAsString()[0];

        if (string.IsNullOrEmpty(separatedList))
        {
            return new BasicValue(member);
        }

        // Use a StringBuilder to minimize temporary allocations
        var result = new StringBuilder(separatedList.Length + member.Length + 1);
        result.Append(separatedList).Append(separator).Append(member);

        return new BasicValue(result.ToString());
    }
}

class CommandFunctionSTRINGMATCH : CommandFunctionExpressionListBase
{
    public CommandFunctionSTRINGMATCH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "STRINGMATCH";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // Parse expects two values: the string and the pattern
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        // Execute the expressions to get the string and pattern
        BasicValue[] vals = exprs.ExecuteList(vars);
        string inputString = vals[0].GetValueAsString();
        string pattern = vals[1].GetValueAsString();

        // Check if the pattern is in the string
        bool isMatch = inputString.Contains(pattern);

        // Return 1 if the pattern is found, 0 otherwise
        return new BasicValue(isMatch ? 1 : 0);
    }
}

