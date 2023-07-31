using System;
using System.Collections.Generic;
using System.IO;


public class CommandExpression : ICommandBase
{
    public string CmdToken { get; } = "EXPR";
    public CommandType.Type Type { get; } = CommandType.Type.Expression;

    ConfigurationCommands config;
    HashSet<string> constantStoppers;

    private class Element
    {
        public BasicVar var;
        public BasicValue val;
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
            this.op = new BasicValue(op);
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
            this.val = val;
            this.type = CommandType.Type.Constant;
        }

        public BasicValue GetValue(BasicVars vars)
        {
            switch (type)
            {
                case CommandType.Type.Variable:
                    return vars.GetValue(var);
                case CommandType.Type.Constant:
                    return val;
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
                    tostring += " var: " + var;
                    break;
                case CommandType.Type.Constant:
                    tostring += " " + val.ToString();
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

    public CommandExpression(ConfigurationCommands config)
    {
        this.config = config;
        this.constantStoppers = new HashSet<string> { ")", ",", "'", "THEN", "TO", "STEP" };
    }

    public bool Parse(TokenConsumer tokens)
    {
        // Implementation specific to CommandImplementation class
        // expr operator expr
        ConfigManager.WriteConsole($"[CommandExpression.Parse] START PARSE {tokens.ToString()}");

        do
        {
            CommandType.Type tokenType = CommandType.TokenType(tokens);
            ConfigManager.WriteConsole($"[CommandExpression.Parse] token: {tokens.Token} type: {tokenType}");
            switch (tokenType)
            {
                case CommandType.Type.Variable:
                    elements.Add(new Element(new BasicVar(tokens.Token)));
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
                        throw new Exception($"Syntax error function not found in expression clause: {tokens.ToString()}");

                    ICommandList exprList = fnct as ICommandList;
                    if (exprList != null)
                        throw new Exception($" fnct list cant be part of an expression {tokens.Token}");

                    ConfigManager.WriteConsole($"[CommandExpression.Parse] FUNCTION parse -  {tokens.ToString()}");
                    fnct.Parse(tokens);
                    ConfigManager.WriteConsole($"[CommandExpression.Parse] FUNCTION parse END -  {tokens.ToString()}");
                    elements.Add(new Element(fnct));
                    break;
                case CommandType.Type.Expression:
                    ConfigManager.WriteConsole($"[CommandExpression.Parse] nested expression token: {tokens.Token}");
                    tokens++; //consumes (
                    CommandExpression expr = new(config);
                    expr.Parse(tokens);
                    if (tokens.Token != ")")
                        throw new Exception($"unbalanced parentesis: {tokens.ToString()}");
                    // tokens++; //consumes )
                    elements.Add(new Element(expr));
                    ConfigManager.WriteConsole($"[CommandExpression.Parse] expression parse end -  {tokens.ToString()}");
                    break;
                case CommandType.Type.Unknown:
                    throw new Exception($"invalid expression {tokens.ToString()}");
            }
        } while (tokens.Next() != null && !constantStoppers.Contains(tokens.Token.ToUpper()));

        ConfigManager.WriteConsole($"[CommandExpression.Parse] parser expression ended {tokens.ToString()}");
        return true;
    }

    public void ElementsLog()
    {
        string str = $"Elements in memory: {elements.Count}:\n";
        foreach (Element el in elements)
        {
            str += el.ToString() + "\n";
        }
        ConfigManager.WriteConsole($"[CommandExpression.ElementsLog] {str}\n");
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
        ConfigManager.WriteConsole($"[AGE BASIC {CmdToken}] [expression execution]");
        //ElementsLog();

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
