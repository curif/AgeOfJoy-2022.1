using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class AGEProgram
{
    private string name;
    SortedDictionary<double, ICommandBase> lines = new();
    private IEnumerator<KeyValuePair<double, ICommandBase>> enumerator;

    double nextLineToExecute = -1;
    int lastLineNumberParsed = -1;
    BasicVars vars = new();
    public TokenConsumer tokens;

    public static double MinJump = 0.000001;

    //execution

    public int LastLineNumberParsed { get { return lastLineNumberParsed; } }
    public BasicVars Vars { get { return vars; } }

    public string Name { get => name; }

    ConfigurationCommands config;
    CodeExecutionTracker tracker;

    private KeyValuePair<double, ICommandBase> getNext()
    {
        if (nextLineToExecute >= 0)
        {
            IEnumerator<KeyValuePair<double, ICommandBase>> etor = lines.GetEnumerator();
            while (etor.MoveNext())
            {
                if (etor.Current.Key >= nextLineToExecute)
                {
                    nextLineToExecute = -1; // Reset the flag
                    enumerator = etor;
                    return etor.Current;
                }
            }
            return default;
        }

        if (enumerator == null) enumerator = lines.GetEnumerator();

        if (enumerator.MoveNext())
            return enumerator.Current;
        
        // Return an empty KeyValuePair if reach the end.
        return default;
    }

    public AGEProgram(string name)
    {
        this.name = name;
        tracker = new();
    }

    public void PrepareToRun(BasicVars pvars = null, int lineNumber = 0)
    {
        this.nextLineToExecute = lineNumber - 1;
        this.enumerator = null;

        config.Gosub = new Stack<double>();
        config.LineNumber = 0;
        config.JumpNextTo = 0;
        config.JumpTo = 0;
        config.stop = false;
        if (pvars == null)
            vars = new();
        else
            vars = pvars;
        tracker.Reset();
    }

    public BasicValue GetVarValue(string varName)
    {
        return vars.GetValue(varName);
    }

    private int maxExecutionLinesAllowed = 10000;
    public int MaxExecutionLinesAllowed
    {
        get { return maxExecutionLinesAllowed; }
        set { maxExecutionLinesAllowed = value; }
    }

    public int ContLinesExecuted
    {
        get { return tracker.GetTotalLinesExecuted(); }
    }


    public bool runNextLine()
    {
        if (config.stop)
        {
            ConfigManager.WriteConsole($"[AGEProgram.runNextLine] {name} stopped by config.stop");
            return false;
        }

        if (maxExecutionLinesAllowed > 0 && tracker.GetTotalLinesExecuted() > maxExecutionLinesAllowed)
        {
            ConfigManager.WriteConsole($"[AGEProgram.runNextLine] {name} executed lines {ContLinesExecuted} > {maxExecutionLinesAllowed}");
            throw new Exception("program has reached the maximum execution lines available.");
        }

        KeyValuePair<double, ICommandBase> cmd = getNext();
        if (cmd.Key == 0.0) //default
            return false;

        if (cmd.Value == null) // empty or REM line.
            return true;

        // ConfigManager.WriteConsole($">> EXEC LINE #[{cmd.Key}] {cmd.Value.CmdToken}");
        config.LineNumber = cmd.Key;

        cmd.Value.Execute(vars);
        if (config.stop)
        {
            ConfigManager.WriteConsole($"[AGEProgram.runNextLine] {name} stopped by config.stop after exec line");
            return false;
        }

        if (tracker == null)
            tracker = new();
        tracker.ExecuteLine();

        if (config.JumpTo != 0) //exactly
        {
            if (!lines.ContainsKey(config.JumpTo))
                throw new Exception($"Line number not found: {config.JumpTo}");

            // ConfigManager.WriteConsole($"[AGEProgram.runNextLine] jump to line = {config.JumpTo}");
            nextLineToExecute = config.JumpTo;
            config.JumpTo = 0;
            return true;
        }
        
        if (config.JumpNextTo != 0) //next one.
        {
            // ConfigManager.WriteConsole($"[AGEProgram.runNextLine] jump to line >= {config.JumpNextTo}");
            nextLineToExecute = config.JumpNextTo + MinJump;
            config.JumpNextTo = 0;
            return true;
        }

        return true; 
    }

    public string Log()
    {
        string str = $"PROGRAM: {Name}\n";
        str += $"Last line parsed: #{lastLineNumberParsed}\n";
        str += $"Next line to execute: > #{nextLineToExecute}\n";
        str += $"Executed lines counter: {ContLinesExecuted}\n";
        str += $"Lines per second: {tracker.GetAverageLinesPerSecond()}\n";

        str += $"VARS: ----------------\n";
        str += vars.ToString() + "\n";
        str += $"----------------\n";
        return str;
    }

    public string[] ParseLineOfCode(string codeLine)
    {
        /*
         * This pattern is designed to match various elements of AGEBasic code. Here's what each part does:

            (?<Text>...): This creates a named capture group called "Text" that will contain the matched text.
            ""[^""]*"": Matches a string literal enclosed in double quotes.
            -?\d+(\.\d+)?: Matches integer or decimal numbers, optionally negative.
            &[0-9A-Fa-f]+: Matches hexadecimal numbers starting with '&'.
            \w+: Matches word characters (letters, digits, or underscores), typically used for identifiers or keywords.
            [,\(\)=/*+\-]: Matches common operators and punctuation.
            !=|<>|>=|<=|==|>|<: Matches comparison operators.
            ': Matches a single quote, often used to start comments in BASIC.

            The | character separates these different patterns, allowing the regex to match any of these elements.
        */
        string pattern = @"(?<Text>""[^""]*""|-?\d+(\.\d+)?|&[0-9A-Fa-f]+|\w+|[,\(\)=/*+\-]|:|!=|<>|>=|<=|==|>|<|'|&&|\|\|)";
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
        this.config = config;
        config.ageProgram = this;
        string currentCommand = null;
        int currentLineNumber = 0; //users line numbers must be an INT

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Check if the line starts with a line number
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex != -1 && int.TryParse(line.Substring(0, spaceIndex), out int lineNumber))
                {
                    if (lineNumber <= 0)
                        throw new Exception($"Line number <= 0 is not allowed, in file: {filePath}");

                    // Process the previous command if there is one
                    if (currentCommand != null)
                        ProcessCommand(currentLineNumber, currentCommand, config, filePath);

                    lastLineNumberParsed = currentLineNumber;

                    if (lineNumber <= currentLineNumber)
                        throw new Exception($"Line numbers not in sequence, in file: {filePath}");

                    // Start a new command
                    currentLineNumber = lineNumber;
                    //removes comments, line numbers and spaces
                    currentCommand = line.Split('\'')[0].Substring(spaceIndex + 1).Trim();
                }
                else
                {
                    // This is a continuation of the current command
                    if (currentCommand != null)
                        currentCommand += " " + line.Split('\'')[0].Trim(); //removes comments, line numbers and spaces
                    else
                        throw new Exception($"Invalid line format or misplaced continuation line: {line} file: {filePath}");
                }
            }

            // Process the last command in the file
            if (currentCommand != null)
                ProcessCommand(currentLineNumber, currentCommand, config, filePath);
        }

        //force an END:
        ProcessCommand(currentLineNumber + 1, "END", config, filePath);

    }

    private void ProcessCommand(int lineNumber, string command, ConfigurationCommands config, string filePath)
    {
        lastLineNumberParsed = lineNumber;

        if (string.IsNullOrEmpty(command))
        {
            lines[lineNumber] = null;
            return;
        }

        string[] parsedString = ParseLineOfCode(command);
        tokens = new(parsedString);
        AGEBasicDebug.WriteConsole($"[basicAGEProgram.ProcessCommand] >>>> line: {lineNumber}  {tokens.ToString()}");

        if (tokens.Count() < 1)
            throw new Exception($"Invalid line format: {command} line: {(int)lineNumber} file: {filePath}");

        if (tokens.Token == "REM")
        {
            lines[lineNumber] = null;
            return;
        }

        ICommandBase cmd = Commands.GetNew(tokens.Token, config);
        if (cmd == null || cmd.Type != CommandType.Type.Command)
            throw new Exception($"Syntax error command not found: {tokens.Token} line: {(int)lineNumber} file: {filePath}");

        config.LineNumber = lineNumber; //config.LineNumber could be changed by a parser.
        lines[lineNumber] = cmd;
        cmd.Parse(++tokens);

        //add next sentences in the same line if any.
        while (tokens.Token == ":")
        {
            tokens++;
            cmd = Commands.GetNew(tokens.Token, config);
            if (cmd == null || cmd.Type != CommandType.Type.Command)
                throw new Exception($"Syntax error command not found: {tokens.Token}  line: {(int)lineNumber} file: {filePath}");

            config.LineNumber += MinJump;
            lines[config.LineNumber] = cmd;
            cmd.Parse(++tokens);
        }
    }

    public void AddCommand(double lineNo, ICommandBase command)
    {
        if (lines.ContainsKey(lineNo))
            throw new Exception($"Line number ({lineNo}) already exists.");
        lines.Add(lineNo, command);
    }

    /*
    public static void ProcessMultiCommands(TokenConsumer tokens, MultiCommand multiCommand, 
                                            ConfigurationCommands config)
    {
        while (tokens.Token == ":")
        {
            tokens++;
            ICommandBase mcmd = Commands.GetNew(tokens.Token, config);

            if (mcmd == null || mcmd.Type != CommandType.Type.Command)
                throw new Exception($"Syntax error command not found: {tokens.Token}");

            mcmd.Parse(++tokens);

            multiCommand.Add(mcmd);
        }
    }
    */
    public class CodeExecutionTracker
    {
        private int totalLinesExecuted;
        private readonly System.Diagnostics.Stopwatch stopwatch;
        private float averageLinesPerSecond;

        public CodeExecutionTracker()
        {
            totalLinesExecuted = 0;
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }

        public void ExecuteLine()
        {
            // Increment the count of total lines executed
            totalLinesExecuted++;

            // Update the average lines per second
            float elapsedTime = stopwatch.ElapsedMilliseconds / 1000.0f; // convert milliseconds to seconds
            if (elapsedTime > 0)  // Ensure no division by zero
            {
                averageLinesPerSecond = totalLinesExecuted / elapsedTime;
            }
        }

        public float GetAverageLinesPerSecond()
        {
            return averageLinesPerSecond;
        }

        public int GetTotalLinesExecuted()
        {
            return totalLinesExecuted;
        }

        public void Reset()
        {
            stopwatch.Reset();
            stopwatch.Start();
            totalLinesExecuted = 0;
        }
    }
}