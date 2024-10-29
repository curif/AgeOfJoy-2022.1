
using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;

class CommandFunctionGETFILES : CommandFunctionExpressionListBase
{
    public CommandFunctionGETFILES(ConfigurationCommands config) : base(config)
    {
        cmdToken = "GETFILES";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 3);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - file path");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - separator");
        FunctionHelper.ExpectedNumber(vals[2], " - order");

        string path = FunctionHelper.FileTraversalFree(vals[0].GetValueAsString(), ConfigManager.BaseDir);
        string separator = vals[1].GetValueAsString();
        int orderType = (int)vals[2].GetValueAsNumber();

        string[] files;

        try
        {
            // Get files from the specified path based on order type
            switch (orderType)
            {
                case 0: // Alphabetic order
                    files = Directory.GetFiles(path).OrderBy(f => f).ToArray();
                    break;
                case 1: // Random order
                    files = Directory.GetFiles(path).OrderBy(f => Guid.NewGuid()).ToArray();
                    break;
                case 2: // Creation date from old to new
                    files = Directory.GetFiles(path).OrderBy(f => new FileInfo(f).CreationTime).ToArray();
                    break;
                case 3: // Creation date from new to old
                    files = Directory.GetFiles(path).OrderByDescending(f => new FileInfo(f).CreationTime).ToArray();
                    break;
                default:
                    // Invalid order type, return an empty string or handle the error accordingly
                    return new BasicValue("");
            }

            // Extract file names without the path
            string result = string.Join(separator, files.Select(f => Path.GetFileName(f)));

            return new BasicValue(result);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that may occur (e.g., invalid path)
            AGEBasicDebug.WriteConsole($"Error: {ex.Message}");
            return new BasicValue("");
        }
    }
}

class CommandFunctionFILEEXISTS : CommandFunctionSingleExpressionBase
{
    public CommandFunctionFILEEXISTS(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEEXISTS";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNonEmptyString(val, " - file path");
        string filePath = FunctionHelper.FileTraversalFree(val.GetValueAsString(), ConfigManager.BaseDir);
        bool fileExists = File.Exists(filePath);

        return new BasicValue(fileExists ? 1 : 0);
    }
}

class CommandFunctionCOMBINEPATH : CommandFunctionExpressionListBase
{
    public CommandFunctionCOMBINEPATH(ConfigurationCommands config) : base(config)
    {
        cmdToken = "COMBINEPATH";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - file path 1");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - file path 2");

        string path1 = vals[0].GetValueAsString();
        string path2 = vals[1].GetValueAsString();

        string combinedPath = FunctionHelper.FileTraversalFree(Path.Combine(path1, path2), ConfigManager.BaseDir);
        return new BasicValue(combinedPath);
    }
}

class PathFunctionsBase : CommandFunctionNoExpressionBase
{
    public PathFunctionsBase(ConfigurationCommands config, string token) : base(config)
    {
        cmdToken = token;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        switch (cmdToken)
        {
            case "CONFIGPATH":
                return new BasicValue(ConfigManager.ConfigDir);
            case "DEBUGPATH":
                return new BasicValue(ConfigManager.DebugDir);
            case "AGEBASICPATH":
                return new BasicValue(ConfigManager.AGEBasicDir);
            case "CABINETSDBPATH":
                return new BasicValue(ConfigManager.CabinetsDB);
            case "CABINETSPATH":
                return new BasicValue(ConfigManager.Cabinets);
            case "CABINETPATH":
                {
                    if (string.IsNullOrEmpty(config.Cabinet?.FilesPath))
                        throw new Exception("CabinetPath() allowed only in an AGEBasic program running in a cabinet.");

                    return new BasicValue(config.Cabinet.FilesPath);
                }
                return new BasicValue(ConfigManager.Cabinets);
            case "ROOTPATH":
                return new BasicValue(ConfigManager.BaseDir);
            case "MUSICPATH":
                return new BasicValue(ConfigManager.MusicDir);
            default:
                return new BasicValue("");
        }
    }
}

class CommandFunctionCONFIGPATH : PathFunctionsBase
{
    public CommandFunctionCONFIGPATH(ConfigurationCommands config) : base(config, "CONFIGPATH") { }
}

class CommandFunctionAGEBASICPATH : PathFunctionsBase
{
    public CommandFunctionAGEBASICPATH(ConfigurationCommands config) : base(config, "AGEBASICPATH") { }
}

class CommandFunctionCABINETSDBPATH : PathFunctionsBase
{
    public CommandFunctionCABINETSDBPATH(ConfigurationCommands config) : base(config, "CABINETSDBPATH") { }
}

class CommandFunctionCABINETSPATH : PathFunctionsBase
{
    public CommandFunctionCABINETSPATH(ConfigurationCommands config) : base(config, "CABINETSPATH") { }
}

class CommandFunctionCABINETPATH : PathFunctionsBase
{
    public CommandFunctionCABINETPATH(ConfigurationCommands config) : base(config, "CABINETPATH") { }
}

class CommandFunctionROOTPATH : PathFunctionsBase
{
    public CommandFunctionROOTPATH(ConfigurationCommands config) : base(config, "ROOTPATH") { }
}

class CommandFunctionMUSICPATH : PathFunctionsBase
{
    public CommandFunctionMUSICPATH(ConfigurationCommands config) : base(config, "MUSICPATH") { }
}


class CommandFunctionDEBUGPATH : PathFunctionsBase
{
    public CommandFunctionDEBUGPATH(ConfigurationCommands config) : base(config, "DEBUGPATH") { }
}

// AGEBasic file management

public class AGEBasicUserFile
{
    public FileStream file;
    public StreamReader reader;
    public StreamWriter writer;
}
class CommandFunctionFILEOPEN : CommandFunctionExpressionListBase
{
    public CommandFunctionFILEOPEN(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEOPEN";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // Parse expects two values: file path and mode ("R", "A", or "W")
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        // Execute the expression list to get file path and mode
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - file path");
        FunctionHelper.ExpectedNonEmptyString(vals[1], " - mode");

        string filePath = vals[0].GetValueAsString();
        string mode = vals[1].GetValueAsString().ToUpper(); // Mode should be case-insensitive

        // Validate mode
        if (mode != "R" && mode != "A" && mode != "W")
        {
            throw new Exception("Invalid mode. Must be 'R' (read), 'A' (append), or 'W' (write).");
        }

        // Check if the file exists for read mode
        if (mode == "R" && !File.Exists(filePath))
        {
            return new BasicValue(-1); // Return -1 if the file does not exist and the mode is "R"
        }

        // Find an empty (null) position in the filepointer array
        int fileNumber = -1;
        for (int i = 0; i < 256; i++)
        {
            if (config.filepointer[i] == null)
            {
                fileNumber = i;
                break;
            }
        }

        // If no empty position is found, throw an error
        if (fileNumber == -1)
        {
            return new BasicValue(-1); 
        }

        config.filepointer[fileNumber] = new();

        // Open the file based on the mode
        FileStream fileStream;
        if (mode == "R")
        {
            fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read); // Open for read
        }
        else if (mode == "A")
        {
            fileStream = File.Open(filePath, FileMode.Append, FileAccess.Write); // Open for append
        }
        else // mode == "W"
        {
            fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write); // Open for write (overwrite or create)
        }

        // Store the file stream in the filepointer array
        config.filepointer[fileNumber].file = fileStream;

        // Return the file number as BasicValue
        return new BasicValue(fileNumber);
    }
}

class CommandFunctionFILEREAD : CommandFunctionSingleExpressionBase
{
    public CommandFunctionFILEREAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEREAD";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        // Execute the expression to get the file number
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, " - file pointer");
        int fileNumber = val.GetInt();

        // Check if the file pointer position is valid
        if (fileNumber < 0 || fileNumber >= 256 || config.filepointer[fileNumber] == null)
        {
            return new BasicValue(-1); // Return -1 if the file is not open or invalid position
        }

        // Retrieve the FileStream at the given position
        FileStream fileStream = config.filepointer[fileNumber].file;

        // Check if the file stream is not open for reading
        if (!fileStream.CanRead)
        {
            throw new Exception("File stream is not open for reading.");
        }
        if (config.filepointer[fileNumber].reader == null)
            config.filepointer[fileNumber].reader = new StreamReader(fileStream);

        // Read the next line
        string line = config.filepointer[fileNumber].reader.ReadLine();

        // If there are no more lines, return -2
        if (line == null)
        {
            return new BasicValue("");
        }

        // Return the line as a BasicValue string
        return new BasicValue(line);
    }
}


class CommandFunctionFILECLOSE : CommandFunctionSingleExpressionBase
{
    public CommandFunctionFILECLOSE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILECLOSE";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        // Execute the expression to get the file number
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, " - file pointer");
        int fileNumber = val.GetInt();

        // Check if the file pointer position is valid
        if (fileNumber < 0 || fileNumber >= 256 || config.filepointer[fileNumber] == null)
        {
            throw new Exception("File is not open.");
        }

        // Close the file at the given position
        FileStream fileStream = config.filepointer[fileNumber].file;
        fileStream.Close();

        // Set the file pointer to null to free the position
        config.filepointer[fileNumber] = null;

        // Return 0 to indicate success
        return new BasicValue(0);
    }
}

class CommandFunctionFILEWRITE : CommandFunctionExpressionListBase
{
    public CommandFunctionFILEWRITE(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEWRITE";
    }

    public override bool Parse(TokenConsumer tokens)
    {
        // Parse expects two values: file number and string to write
        return base.Parse(tokens, 2);
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{exprs}] ");

        // Execute the expression list to get the file number and string to write
        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNumber(vals[0], " - file pointer");
        FunctionHelper.ExpectedString(vals[1], " - string to write");

        int fileNumber = vals[0].GetInt();
        string contentToWrite = vals[1].GetValueAsString();

        // Check if the file pointer position is valid and the file is open
        if (fileNumber < 0 || fileNumber >= 256 || config.filepointer[fileNumber] == null)
        {
            throw new Exception("File is not open.");
        }

        // Retrieve the FileStream at the given position
        FileStream fileStream = config.filepointer[fileNumber].file;

        // Check if the file was opened in write or append mode
        if (fileStream.CanWrite == false)
        {
            throw new Exception("File is not writable.");
        }

        StreamWriter writer;
        // Use StreamWriter to write the content to the file
        if (config.filepointer[fileNumber].writer == null)
        {
            writer = new StreamWriter(fileStream);
            config.filepointer[fileNumber].writer = writer;
        }
        else
        {
           writer = config.filepointer[fileNumber].writer;
        }

        writer.WriteLine(contentToWrite);
        writer.Flush(); // Ensure the data is written to the file

        // Return 0 to indicate success
        return new BasicValue(0);
    }
}

class CommandFunctionFILEEOF : CommandFunctionSingleExpressionBase
{
    public CommandFunctionFILEEOF(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEEOF";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        // Execute the expression to get the file number
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedNumber(val, " - file pointer");
        int fileNumber = val.GetInt();

        // Check if the file pointer position is valid
        if (fileNumber < 0 || fileNumber >= 256 || config.filepointer[fileNumber] == null)
        {
            return new BasicValue(-1); // Return -1 if the file is not open or invalid position
        }

        bool isEOF;
        StreamReader reader = config.filepointer[fileNumber].reader;
        if (reader != null)
        {
            isEOF = reader.EndOfStream;
        }
        else
        {
            FileStream fileStream = config.filepointer[fileNumber].file;
            isEOF = fileStream.Position == fileStream.Length;
        }

        // Return 1 if at EOF, otherwise return 0
        return new BasicValue(isEOF);
    }
}
class CommandFunctionFILEEXIST : CommandFunctionSingleExpressionBase
{
    public CommandFunctionFILEEXIST(ConfigurationCommands config) : base(config)
    {
        cmdToken = "FILEEXIST";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}] [{expr}] ");

        // Execute the expression to get the file path
        BasicValue val = expr.Execute(vars);
        FunctionHelper.ExpectedString(val, " - file path");

        string filePath = val.GetValueAsString();

        // Check if the file path is valid
        if (string.IsNullOrEmpty(filePath))
        {
            return new BasicValue(0); // Return 0 if the file path is invalid or empty
        }

        // Check if the file exists at the specified path
        bool fileExists = File.Exists(filePath);

        // Return 1 if the file exists, otherwise return 0
        return new BasicValue(fileExists ? 1 : 0);
    }
}
