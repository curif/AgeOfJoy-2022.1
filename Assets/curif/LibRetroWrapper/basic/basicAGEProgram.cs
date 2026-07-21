using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class AGEProgram
{
    private string name;
    SortedDictionary<double, ICommandBase> lines = new();
    
    // Fast execution arrays
    private double[] parsedLineNumbers;
    private ICommandBase[] parsedCommands;
    private int currentExecutionIndex = 0;

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
        // If a jump was requested (e.g. GOTO, GOSUB, NEXT)
        if (nextLineToExecute >= 0)
        {
            // Binary Search for the target line
            int index = Array.BinarySearch(parsedLineNumbers, nextLineToExecute);
            
            // If it wasn't found exactly, BinarySearch returns the bitwise complement 
            // of the next largest element (>=). We flip it to get the correct index.
            if (index < 0) 
            {
                index = ~index;
            }

            // If the index is past the end of the array, the jump went past the end of the program
            if (index >= parsedLineNumbers.Length)
            {
                return default;
            }

            // Set our execution pointer to the found index
            currentExecutionIndex = index;
            nextLineToExecute = -1; // Reset jump flag
        }

        // Standard execution: return current and move pointer forward
        if (currentExecutionIndex < parsedLineNumbers.Length)
        {
            double lineNo = parsedLineNumbers[currentExecutionIndex];
            ICommandBase cmd = parsedCommands[currentExecutionIndex];
            currentExecutionIndex++;
            return new KeyValuePair<double, ICommandBase>(lineNo, cmd);
        }

        // Return an empty KeyValuePair if reach the end.
        return default;
    }

    public AGEProgram(string name)
    {
        this.name = name;
        tracker = new();
    }

    // Called after parsing is complete to populate the fast execution arrays
    private void CompileExecutionArrays()
    {
        parsedLineNumbers = new double[lines.Count];
        parsedCommands = new ICommandBase[lines.Count];
        
        int i = 0;
        foreach (KeyValuePair<double, ICommandBase> kvp in lines)
        {
            parsedLineNumbers[i] = kvp.Key;
            parsedCommands[i] = kvp.Value;
            i++;
        }
    }

    public void PrepareProgramToRun(BasicVars pvars = null, int lineNumber = 0, int maxExecutionLinesAllowed = -1)
    {
        // Ensure arrays are built if they somehow weren't
        if (parsedLineNumbers == null || parsedLineNumbers.Length != lines.Count)
        {
            CompileExecutionArrays();
        }

        this.nextLineToExecute = lineNumber - 1; // trigger a jump to the starting line
        this.currentExecutionIndex = 0;
        this.maxExecutionLinesAllowed = maxExecutionLinesAllowed;

        config.Gosub = new Stack<double>();
        config.LineNumber = 0;
        config.JumpNextTo = 0;
        config.JumpTo = 0;
        config.stop = false;
        config.shutdown = false;
        if (pvars == null)
        {
            vars = new();
            // Initialize fast slots from the symbol table built during parse
            if (config.VarIdMap != null && config.VarIdMap.Count > 0)
                vars.InitFastSlots(config.VarIdMap);
        }
        else
        {
            vars = pvars;
            // pvars comes from a different program scope — fast slots are NOT initialized
            // because the IDs in this program's VarIdMap won't match pvars' layout.
        }
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

    public float GetLinesPerSecond()
    {
        return tracker?.GetAverageLinesPerSecond() ?? 0f;
    }


    public bool runNextLine()
    {
        if (config.stop || config.shutdown)
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
        if (config.stop || config.shutdown)
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

        string pattern = @"(?<Text>""[^""]*""|-?\d+(\.\d+)?|&[0-9A-Fa-f]+|\w+|[,\(\)=/*+\-\[\]]|:|!=|<>|>=|<=|==|>|<|'|&&|\|\|)";

//        string pattern = @"(?<Text>""[^""]*""|-?\d+(\.\d+)?|&[0-9A-Fa-f]+|\w+|[,\(\)=/*+\-]|:|!=|<>|>=|<=|==|>|<|'|&&|\|\|)";
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
    // Helper method to detect line numbers
    private bool TryParseLineNumberAndContent(string line, out int lineNumber, out string content)
    {
        lineNumber = -1;
        content = string.Empty;

        // We expect 'line' here to already be trimmed (no leading/trailing physical line whitespace)
        // and comments stripped. So, we only need to handle whitespace between number and command.

        int i = 0;
        // Find the end of the digit sequence (number part)
        while (i < line.Length && char.IsDigit(line[i]))
        {
            i++;
        }

        if (i == 0) // No digits found at the very beginning of the (already trimmed) line
        {
            return false;
        }

        string numberPart = line.Substring(0, i); // Number is at the very start
        if (int.TryParse(numberPart, out lineNumber))
        {
            // The rest of the line is the content. Trim it thoroughly.
            content = line.Substring(i).Trim();
            return true;
        }

        return false; // Should not be reached if char.IsDigit already succeeded, but for safety
    }
    public void Parse(string filePath, ConfigurationCommands config)
    {
        this.config = config;
        config.ageProgram = this;

        double currentLineNumber = -1;
        string currentSentence = "";

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            
            
            while ((line = reader.ReadLine()) != null)
            {

                // *** NEW: Trim the physical line immediately after reading ***
                line = line.Trim();
                try
                {
                    // After trimming and stripping comments, if the line is empty, skip it.
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue; // Ignore empty lines or lines that were only comments
                    }

                    // 1. Strip comments (everything after a single quote) immediately.
                    // This ensures comments don't interfere with line number detection or command content.
                    int commentIndex = line.IndexOf('\'');
                    if (commentIndex != -1)
                    {
                        line = line.Substring(0, commentIndex);
                    }

                    // 2. Attempt to parse a line number at the start of the physical line.
                    int parsedLineNumber;
                    string commandPartAfterNumber;
                    bool startsWithLineNumber = TryParseLineNumberAndContent(line,
                                                                            out parsedLineNumber,
                                                                            out commandPartAfterNumber);

                    if (startsWithLineNumber)
                    {
                        // This physical line starts with a line number, so it signifies a NEW logical program line.

                        // A. First, process any accumulated command from the PREVIOUS logical line.
                        if (!string.IsNullOrEmpty(currentSentence)) // If we have a pending logical line
                            ProcessCommand((int)currentLineNumber, currentSentence, config, filePath);

                        if (parsedLineNumber <= currentLineNumber)
                            throw new Exception($"Syntax Error: Line numbers must be in strictly ascending order and unique. Duplicate or out-of-order line number {parsedLineNumber}. File: '{filePath}'");

                        // C. Initialize for the new logical line.
                        currentLineNumber = parsedLineNumber;
                        if (string.IsNullOrEmpty(commandPartAfterNumber))
                        {
                            currentSentence = "";
                            continue;
                        }
                        currentSentence = commandPartAfterNumber;
                    }
                    else
                    {
                        // This physical line does NOT start with a line number. It's a continuation.

                        if (currentLineNumber == -1)
                        {
                            throw new Exception($"Syntax Error: First executable line must start with a line number. File: '{filePath}'");
                        }

                        // Append this line's content to the current accumulated command.
                        // Add a space as a separator, which is common for multi-line statements.
                        currentSentence += " " + line;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"[AGEBasicProgam.Parse] program: '{filePath}', error parsing line `{line}` exception: {ex}");
                }
            }
        }

        // After the loop, process the very last accumulated logical line in the file.
        if (!string.IsNullOrEmpty(currentSentence))
        {
            ProcessCommand((int)currentLineNumber, currentSentence, config, filePath);
        }
        // If the file was completely empty or only comments, currentLineNumber would still be -1.
        // Ensure we have a valid line number for the implicit END.
        if (currentLineNumber == -1)
        {
            currentLineNumber = 0; // Use 0 or 1 as a base if no lines were processed
        }
        // Force an END command at the very end of the program.
        ProcessCommand((int)currentLineNumber + 1, "END", config, filePath);
    }
    bool AllowedAtFirst(ICommandBase cmd) => (cmd != null && cmd.Type == CommandType.Type.Command /*|| cmd.Type == CommandType.Type.Function*/);
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
            throw new Exception($"[basicAGEProgram.ProcessCommand] Invalid line format: {command} line: {(int)lineNumber} file: {filePath}");

        if (tokens.Token == "REM")
        {
            lines[lineNumber] = null;
            return;
        }

        ICommandBase cmd = Commands.GetNew(tokens.Token, config);
        if (!AllowedAtFirst(cmd))
            throw new Exception($"[basicAGEProgram.ProcessCommand] Syntax error command or function not found: {tokens.Token} line: {(int)lineNumber} file: {filePath}");

        config.LineNumber = lineNumber; //config.LineNumber could be changed by a parser.
        lines[lineNumber] = cmd;
        cmd.Parse(++tokens);
        cmd.CheckConfigRequirements(config);

        //add next sentences in the same line if any.
        while (tokens.Token == ":")
        {
            tokens++;
            cmd = Commands.GetNew(tokens.Token, config);
            if (!AllowedAtFirst(cmd))
                throw new Exception($"[basicAGEProgram.ProcessCommand] Syntax error command or function not found: {tokens.Token}  line: {(int)lineNumber} file: {filePath}");

            config.LineNumber += MinJump;
            lines[config.LineNumber] = cmd;
            cmd.Parse(++tokens);
            cmd.CheckConfigRequirements(config);
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