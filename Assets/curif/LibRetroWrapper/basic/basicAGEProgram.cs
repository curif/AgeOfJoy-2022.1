using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class AGEProgram
{
    public string Name;
    Dictionary<int, ICommandBase> lines = new();
    int nextLineToExecute = -1;
    int lastLineNumberExecuted = -1;
    int lastLineNumberParsed = -1;
    BasicVars vars = new();
    BasicValue lineNumber = new();
    BasicValue gosubstack = new("");

    public int LastLineNumberParsed { get { return lastLineNumberParsed; } }
    public int LastLineNumberExecuted { get { return lastLineNumberExecuted; } }
    public BasicVars Vars { get { return vars; } }

    private KeyValuePair<int, ICommandBase> getNext()
    {
        KeyValuePair<int, ICommandBase> nextLine =
                    lines.FirstOrDefault(kvp => kvp.Key >= nextLineToExecute);
        return nextLine;
    }

    public void PrepareToRun()
    {
        this.nextLineToExecute = -1;
        vars.SetValue("_linenumber", lineNumber); //on excecution
        vars.SetValue("_gosubstack", gosubstack); //gosub stack lines
    }

    public BasicValue GetVarValue(string varName)
    {
        return vars.GetValue(varName);
    }

    public bool runNextLine()
    {
        KeyValuePair<int, ICommandBase> cmd = getNext();
        if (cmd.Key == 0)
            return false;

        ConfigManager.WriteConsole($"EXEC LINE #[{cmd.Key}] Instruction: {cmd.Value.CmdToken}");

        lineNumber.SetValue((double)cmd.Key);
        lastLineNumberExecuted = cmd.Key;
        BasicValue ret = cmd.Value.Execute(vars);

        if (ret != null)
        {
            double newLineNumber = ret.GetValueAsNumber();
            if (newLineNumber == double.MaxValue)
                return false;
            int newLine = (int)newLineNumber;
            if (!lines.ContainsKey(newLine))
                throw new Exception($"Line number not found: {newLineNumber}");
            if (cmd.Value.CmdToken == "RETURN")
                newLine++; //next instruction
            nextLineToExecute = newLine;
            ConfigManager.WriteConsole($"[AGEProgram.runNextLine] jump to line >= {nextLineToExecute}");
        
        }
        else
            nextLineToExecute = lastLineNumberExecuted + 1;
        
        return true;
    }

    public List<string> ParseString(string input)
    {
        List<string> result = new List<string>();

        string pattern = "\"(.*?)\"|(\\S+)";
        MatchCollection matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            string parsedElement = match.Value;
            /*if (parsedElement.StartsWith("\"") && parsedElement.EndsWith("\""))
            {
                // Remove leading and trailing quotes
                parsedElement = parsedElement.Trim('"');
            }*/
            result.Add(parsedElement);
        }

        return result;
    }

    public string Log()
    {
        string str = $"PROGRAM: {Name}\n";
        str += $"Last line parsed: {lastLineNumberParsed} executed: {nextLineToExecute}\n";
        str += $"vars: \n";
        str += vars.ToString() + "\n";
        return str;
    }


    public void Parse(string filePath, ConfigurationCommands config)
    {
        Dictionary<int, ICommandBase> lines = new();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                List<string> parsedString = ParseString(line);
                TokenConsumer tokens = new(parsedString);
                ConfigManager.WriteConsole($"[basicAGEProgram.Parse]  {tokens.ToString()}");

                if (tokens.Remains() >= 1)
                {
                    if (int.TryParse(tokens.Token, out int lineNumber))
                    {
                        if (lineNumber <= 0)
                            throw new Exception($"Line number <= 0 is not allowed, in file: {filePath}");

                        if (lines.ContainsKey(lineNumber))
                            throw new Exception($"Duplicated line number {lineNumber}, in file: {filePath}");

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