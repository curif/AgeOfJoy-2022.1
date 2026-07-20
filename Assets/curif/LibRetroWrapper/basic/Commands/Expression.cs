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
        ")", ",", "'", ":", "THEN", "ELSE", "TO", "STEP", "]", "LINE", "GOTO", "NAME"
        };

    // Shared evaluation stack used by ALL expressions to prevent GC allocation.
    // Safe because AGEBasic execution is single-threaded.
    private static readonly Stack<BasicValue> sharedEvalStack = new Stack<BasicValue>(64);

    private class Element
    {
        public BasicVar var;
        public int varId = -1;

        public BasicValue constantValue;
        public CommandExpression expr;
        public ICommandBase func;
        public BasicValue op;
        public CommandType.Type type;

        public Element(BasicVar var)
        {
            this.var = var;
            this.varId = var.VarId;
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
            BasicValue v;
            if (varId >= 0 && vars.HasFastSlots)
                v = vars.GetFast(varId);
            else
                v = vars[var.Name];

            if (!v.IsArray())
                return BasicValuePool.GetCopy(v);

            //if there is not solicited index in array, returns a copy of the actual array.
            //it could be slow if the array is big.
            if (var.IndexExpressions == null || var.IndexExpressions.Count == 0)
                return BasicValuePool.GetCopy(v);

            BasicValue[] indexes = var.IndexExpressions.ExecuteList(vars);
            return BasicValuePool.GetCopy(v[indexes]);
        }
        public BasicValue GetValue(BasicVars vars)
        {
            switch (type)
            {
                case CommandType.Type.Variable:
                    return GetVarValue(vars);
                case CommandType.Type.Constant:
                    return BasicValuePool.GetCopy(constantValue);
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
    private List<Element> rpnElements = new(); // Stores the compiled Reverse Polish Notation
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

        CompileToRPN(); // Convert infix notation to RPN once during parse

        AGEBasicDebug.WriteConsole($"[CommandExpression.Parse] parser expression ended {tokens.ToString()}");
        return true;
    }

    private void CompileToRPN()
    {
        Stack<Element> operatorStack = new Stack<Element>();
        rpnElements.Clear();

        foreach (Element element in elements)
        {
            if (element.type == CommandType.Type.Operation)
            {
                while (operatorStack.Count > 0 &&
                    BasicValue.PrecedenceIsLess(element.op, operatorStack.Peek().op))
                {
                    rpnElements.Add(operatorStack.Pop());
                }
                operatorStack.Push(element);
            }
            else
            {
                // Operands (variables, constants, functions, sub-expressions) go straight to output
                rpnElements.Add(element);
            }
        }

        while (operatorStack.Count > 0)
        {
            rpnElements.Add(operatorStack.Pop());
        }
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
        //accelerator for single-element expressions
        if (rpnElements.Count == 1)
        {
            if (rpnElements[0].type == CommandType.Type.Constant)
                return BasicValuePool.GetCopy(rpnElements[0].constantValue);
            else if (rpnElements[0].type == CommandType.Type.Variable)
                return rpnElements[0].GetVarValue(vars);
            else if (rpnElements[0].type == CommandType.Type.Function)
                return rpnElements[0].func.Execute(vars);
            else if (rpnElements[0].type == CommandType.Type.Expression)
                return rpnElements[0].expr.Execute(vars);
        }

        // Snapshot stack size to safely restore it later (handles nested expression calls)
        int initialStackCount = sharedEvalStack.Count;

        try
        {
            for (int i = 0; i < rpnElements.Count; i++)
            {
                Element element = rpnElements[i];

                if (element.type == CommandType.Type.Operation)
                {
                    if (sharedEvalStack.Count - initialStackCount < 2)
                        throw new Exception($"Malformed expression {CmdToken} - Not enough operands for operator {element.op}");

                    BasicValue right = sharedEvalStack.Pop();
                    BasicValue left = sharedEvalStack.Pop();
                    BasicValue val = left.Operate(right, element.op); // mutates left, returns left
                    BasicValuePool.Return(right);  // right is consumed — recycle it
                    sharedEvalStack.Push(val);
                }
                else
                {
                    sharedEvalStack.Push(element.GetValue(vars));
                }
            }

            if (sharedEvalStack.Count - initialStackCount != 1)
                throw new Exception($"Malformed expression {CmdToken} (END) - Stack unbalanced.");

            return sharedEvalStack.Pop();
        }
        finally
        {
            // Guaranteed cleanup: if an exception happens (e.g. divide by zero),
            // return any garbage left on the stack by this expression execution to the pool.
            while (sharedEvalStack.Count > initialStackCount)
                BasicValuePool.Return(sharedEvalStack.Pop());
        }
    }

}
