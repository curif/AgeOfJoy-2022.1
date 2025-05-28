using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using static Siccity.GLTFUtility.GLTFAccessor.Sparse;


public class CommandExpression : ICommandBase
{
    public string CmdToken { get; } = "EXPR";
    public CommandType.Type Type { get; } = CommandType.Type.Expression;

    ConfigurationCommands config;
    private static readonly HashSet<string> constantStoppers =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) // Or CurrentCultureIgnoreCase, etc.
        {
        ")", ",", "'", ":", "THEN", "ELSE", "TO", "STEP", "]"
        };
    private class Element
    {
        public BasicVar var;

        public BasicValue constantValue;
        public CommandExpression expr;
        public ICommandBase func;
        public BasicValue op;
        public CommandType.Type type;

        public Element(BasicVar var)
        {
            this.var = var;
            this.type = CommandType.Type.Variable;
        }
        public Element(string op)
        {
            this.op = new BasicValue(op, forceType: BasicValue.BasicValueType.String);
            this.type = CommandType.Type.Operation;
        }
        public Element(CommandExpression expr)
        {
            this.expr = expr;
            this.type = CommandType.Type.Expression;
        }
        public Element(ICommandBase func)
        {
            this.func = func;
            this.type = CommandType.Type.Function;
        }
        public Element(BasicValue val)
        {
            this.constantValue = val;
            this.type = CommandType.Type.Constant;
        }

        public BasicValue GetVarValue(BasicVars vars)
        {
            BasicValue v = vars[var.Name];
            if (!v.IsArray())
                return new(v);

            //if there is not solicited index in array, returns a copy of the actual array.
            //it could be slow if the array is big.
            if (var.IndexExpressions == null || var.IndexExpressions.Count == 0)
                return new(v);

            BasicValue[] indexes = var.IndexExpressions.ExecuteList(vars);
            return new(v[indexes]);
        }
        public BasicValue GetValue(BasicVars vars)
        {
            switch (type)
            {
                case CommandType.Type.Variable:
                    return GetVarValue(vars);
                case CommandType.Type.Constant:
                    return constantValue;
                case CommandType.Type.Operation:
                    return op;
                case CommandType.Type.Expression:
                    return expr.Execute(vars);
                case CommandType.Type.Function:
                    return func.Execute(vars);
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            string tostring = "";
            switch (type)
            {
                case CommandType.Type.Variable:
                    tostring += var;
                    break;
                case CommandType.Type.Constant:
                    tostring += " " + constantValue.ToString();
                    break;
                case CommandType.Type.Operation:
                    tostring += " " + op.ToString();
                    break;
                case CommandType.Type.Expression:
                    tostring += " (" + expr.ToString() + ")";
                    break;
                case CommandType.Type.Function:
                    tostring += func.ToString();
                    break;
            }
            return tostring;
        }
    }

    private List<Element> elements = new();
    public int Count { get { return elements.Count; } }

    public BasicVar GetVariable(int position)
    {
        if (elements[position].type != CommandType.Type.Variable)
            throw new ArgumentException($"Element {position} in expression requires a variable");
        return elements[position].var;
    }

    public CommandExpression(ConfigurationCommands config)
    {
        this.config = config;
    }

    public bool Parse(TokenConsumer tokens)
    {
        // Implementation specific to CommandImplementation class
        // expr operator expr
        AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] START PARSE {tokens.ToString()}");

        do
        {
            CommandType.Type tokenType = CommandType.TokenType(tokens);
            AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] token: {tokens.Token} type: {tokenType}");
            switch (tokenType)
            {
                case CommandType.Type.Variable:
                    elements.Add(new Element(BasicVar.Parse(tokens, config)));
                    break;
                case CommandType.Type.Constant:
                    elements.Add(new Element(new BasicValue(tokens.Token)));
                    break;
                case CommandType.Type.Operation:
                    elements.Add(new Element(tokens.Token));
                    break;
                case CommandType.Type.Function:
                    ICommandBase fnct = Commands.GetNew(tokens.Token, config);
                    if (fnct == null)
                        throw new Exception($"Syntax error function not found in expression clause: {tokens}");

                    ICommandList exprList = fnct as ICommandList;
                    if (exprList != null)
                        throw new Exception($" fnct list cant be part of an expression {tokens.Token}");

                    AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] FUNCTION parse -  {tokens}");
                    fnct.Parse(tokens);
                    AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] FUNCTION parse END -  {tokens}");
                    elements.Add(new Element(fnct));
                    break;
                case CommandType.Type.Expression:
                    AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] nested expression token: {tokens.Token}");
                    tokens++; //consumes (
                    CommandExpression expr = new(config);
                    expr.Parse(tokens);
                    if (tokens.Token != ")")
                        throw new Exception($"unbalanced parentesis or function don't recognized: {tokens}");
                    // tokens++; //consumes )
                    elements.Add(new Element(expr));
                    AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] expression parse end -  {tokens}");
                    break;
                case CommandType.Type.Unknown:
                    throw new Exception($"invalid expression {tokens}");
            }
        } while (tokens.Next() != null && !CommandExpression.constantStoppers.Contains(tokens.Token));

        AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] parser expression ended {tokens.ToString()}");
        return true;
    }

    public void ElementsLog()
    {
        string str = $"Elements in memory: {elements.Count}:\n";
        foreach (Element el in elements)
        {
            str += el.ToString() + "\n";
        }
        AGEBasicDebug.WriteConsole($"[CommandExpression.ElementsLog] {str}\n");
    }
    public override string ToString()
    {
        string str = "";
        foreach (Element el in elements)
        {
            str += el.ToString();
        }
        return str;
    }

    public BasicValue Execute(BasicVars vars)
    {
        // AGEBasicDebug.WriteConsole($"[AGE BASIC {CmdToken}] [expression execution]");
        //ElementsLog();

        //accelerator
        if (elements.Count == 1)
            if (elements[0].type == CommandType.Type.Constant)
                return new(elements[0].constantValue);
            else if (elements[0].type == CommandType.Type.Variable)
            {
                return elements[0].GetVarValue(vars);
            }
            else if (elements[0].type == CommandType.Type.Function)
                return elements[0].func.Execute(vars);

        Stack<BasicValue> operands = new Stack<BasicValue>();
        Stack<BasicValue> operators = new Stack<BasicValue>();

        foreach (Element element in elements)
        {
            if (element.type == CommandType.Type.Operation)
            {
                while (operators.Count > 0 &&
                    BasicValue.PrecedenceIsLess(element.op, operators.Peek()))
                {
                    BasicValue right = operands.Pop();
                    BasicValue left = operands.Pop();
                    BasicValue val = left.Operate(right, operators.Pop());
                    operands.Push(val);
                }

                operators.Push(element.op);
            }
            else
            {
                operands.Push(element.GetValue(vars));
            }
        }

        while (operators.Count > 0)
        {
            BasicValue right, left, val, op;
            try
            {
                right = operands.Pop();
                left = operands.Pop();
                op = operators.Pop();
                val = left.Operate(right, op);
            }
            catch (Exception e)
            {
                throw new Exception($"Malformed expression {CmdToken} - [{e.Message}]");
            }
            operands.Push(val);
        }

        //end
        BasicValue valRet;
        try
        {
            valRet = operands.Pop();
        }
        catch
        {
            throw new Exception($"Malformed expression {CmdToken} (END)");
        }
        return valRet;
    }

}
