using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class AGEProgram
{
    private string name;
    Dictionary<int, ICommandBase> lines = new();
    int nextLineToExecute = -1;
    int lastLineNumberParsed = -1;
    BasicVars vars = new();
    public TokenConsumer tokens;

    //statis
    public int ContLinesExecuted;

    public int LastLineNumberParsed { get { return lastLineNumberParsed; } }
    public BasicVars Vars { get { return vars; } }

    public string Name { get => name; }

    ConfigurationCommands config;

    private KeyValuePair<int, ICommandBase> getNext()
    {
        KeyValuePair<int, ICommandBase> nextLine =
                    lines.FirstOrDefault(kvp => kvp.Key >= nextLineToExecute);
        return nextLine;
    }

    public AGEProgram(string name)
    {
        this.name = name;
    }

    public void PrepareToRun()
    {
        this.nextLineToExecute = -1;
        config.Gosub = new Stack<int>();
        config.LineNumber = config.JumpNextTo = config.JumpTo = ContLinesExecuted = 0;
        config.stop = false;
        vars = new();
    }

    public BasicValue GetVarValue(string varName)
    {
        return vars.GetValue(varName);
    }

    public bool runNextLine()
    {
        if (config.stop)
            return false;

        if (ContLinesExecuted > 10000)
            throw new Exception("program has reached the maximum execution lines available.");

        KeyValuePair<int, ICommandBase> cmd = getNext();
        if (cmd.Key == 0)
            return false;

        ConfigManager.WriteConsole($">> EXEC LINE #[{cmd.Key}] {cmd.Value.CmdToken}");

        config.LineNumber = cmd.Key;
        cmd.Value.Execute(vars);
        ContLinesExecuted++;

        if (config.stop)
            return false;

        if (config.JumpTo != 0)
        {
            if (!lines.ContainsKey(config.JumpTo))
                throw new Exception($"Line number not found: {config.JumpTo}");

            ConfigManager.WriteConsole($"[AGEProgram.runNextLine] jump to line = {config.JumpTo}");
            nextLineToExecute = config.JumpTo;
            config.JumpTo = 0;
        }
        else if (config.JumpNextTo != 0)
        {
            ConfigManager.WriteConsole($"[AGEProgram.runNextLine] jump to line >= {config.JumpNextTo}");
            nextLineToExecute = config.JumpNextTo + 1;
            config.JumpNextTo = 0;
        }
        else
            nextLineToExecute = cmd.Key + 1;

        return true;
    }

    /*public List<string> ParseString(string input)
    {
        List<string> result = new List<string>();

        string pattern = "\"(.*?)\"|(\\S+)";
        MatchCollection matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            string parsedElement = match.Value;

            result.Add(parsedElement);
        }

        return result;
    }
    */

    public string Log()
    {
        string str = $"PROGRAM: {Name}\n";
        str += $"Last line parsed: {lastLineNumberParsed} executed: {nextLineToExecute}\n";
        str += $"vars: \n";
        str += vars.ToString() + "\n";
        return str;
    }

    public string[] ParseLineOfCode(string codeLine)
    {
        string pattern = @"(?<Text>""[^""]*""|-?\d+(\.\d+)?|\w+|[,\(\)=/*+\-]|!=|<>|>=|<=|==|>|<|')";
        //        string pattern = @"(?<Text>""[^""]*""|\d+(\.\d+)?|\w+|[,\(\)=/*+\-]|!=|<>|>=|<=|>|<)|\s+";

        //string pattern = @"(?<Text>""[^""]*""|\d+(\.\d+)?|\w+|[,\(\)=/*+\-]|!=|<>|>=|<=|>|<|\s+)";

        //        string pattern = @"(?<Text>""[^""]*""|\d+(\.\d+)?|\w+|[,\(\)=/*+-]|!=|<>|>=|<=|>|<)";
        MatchCollection matches = Regex.Matches(codeLine, pattern);

        string[] tokens = new string[matches.Count];
        int index = 0;
        string token;
        foreach (Match match in matches)
        {
            token = match.Value.Trim();
            if (!string.IsNullOrEmpty(token))
            {
                // ConfigManager.WriteConsole($"[{match.Value}] {token.Length} chars");
                tokens[index] = token;
                index++;
            }
        }
        if (matches.Count == index)
            return tokens;

        return tokens.Take(index).ToArray<string>();
    }

    public void Parse(string filePath, ConfigurationCommands config)
    {
        Dictionary<int, ICommandBase> lines = new();
        this.config = config;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // List<string> parsed = ParseLine(line);
                // ConfigManager.WriteConsole($">>>{string.Join('.', parsed)}");
                // continue;

                string[] parsedString = ParseLineOfCode(line);
                tokens = new(parsedString);
                ConfigManager.WriteConsole($"[basicAGEProgram.Parse] >>>>  {tokens.ToString()}");
                //continue;

                if (tokens.Remains() >= 1)
                {
                    if (int.TryParse(tokens.Token, out int lineNumber))
                    {
                        if (lineNumber <= 0)
                            throw new Exception($"Line number <= 0 is not allowed, in file: {filePath}");
                        if (lineNumber <= lastLineNumberParsed)
                            throw new Exception($"Line numbers not in sequence, in file: {filePath}");
                        // if (lines.ContainsKey(lineNumber))
                        //     throw new Exception($"Duplicated line number {lineNumber}, in file: {filePath}");

                        ICommandBase cmd = Commands.GetNew(tokens.Next(), config);

                        if (cmd == null || cmd.Type != CommandType.Type.Command)
                            throw new Exception($"Syntax error command not found: {tokens.Token} line: {lineNumber} file: {filePath}");

                        lastLineNumberParsed = lineNumber;
                        cmd.Parse(++tokens);
                        lines[lineNumber] = cmd;
                    }
                    else
                    {
                        throw new Exception($"Invalid line number format: {tokens.Token} file: {filePath}");
                    }
                }
                else
                {
                    throw new Exception($"Invalid line format: {line} file: {filePath}");
                }
            }
        }

        // Sort a dictionary by line numbers
        this.lines = lines.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}