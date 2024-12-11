using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;

public class CabinetDebugConsole : MonoBehaviour
{

    [Tooltip("AGEBasic engine, will find one if not set.")]
    public basicAGE AGEBasic;

    [Tooltip("System skin.")]
    public string system_skin = "c64";

    [Tooltip("Repeat every secs.")]
    public int runEverySecs = 2;

    [Tooltip("Program to execute.")]
    public string AGEBasicFileName = "debug.bas";

    public FileMonitor fileMonitor;

    public ScreenGenerator scr;

    private ShaderScreenBase shader;
    private Renderer display;

    private bool mustRun = false;
    
    public bool BASFileFound = false;
    string debugbas;
    private bool isListenerAdded = false;

    // Start is called before the first frame update
    void Start()
    {
        ConfigManager.WriteConsole("[CabinetDebugConsole] start");

        if (AGEBasic == null)
            AGEBasic = GetComponent<basicAGE>();
        if (scr  == null)
            scr = GetComponent<ScreenGenerator>();
        if (fileMonitor == null)
            fileMonitor = GetComponent<FileMonitor>();

        debugbas = Path.Combine(ConfigManager.AGEBasicDir, AGEBasicFileName);

        //bas file
        string destination = Path.Combine(ConfigManager.AGEBasicDir, AGEBasicFileName);
        ConfigManager.WriteConsole($"[CabinetDebugConsole.run] AGEBasic program: {destination}");

        initScreen();
        addListener();

        if (!File.Exists(destination))
        {
            ConfigManager.WriteConsole($"[CabinetDebugConsole.run] {destination} not found, copying: {destination}");
            string source = Path.Combine(Application.streamingAssetsPath, AGEBasicFileName);
            StartCoroutine(CopyFile(source, destination, OnFileCopyComplete));
        }
        else
        {
            StartCoroutine(run());
        }

    }

    void addListener()
    {
        if (isListenerAdded) return;
        fileMonitor?.OnFileChanged.AddListener(OnFileChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        fileMonitor?.OnFileChanged.RemoveListener(OnFileChanged);
        isListenerAdded = false;
    }
    void OnEnable()
    {
        addListener();
    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        removeListener();
    }

    void OnFileChanged()
    {
        //A coroutine only works in main thread.
        //You have to inject the method of the coroutine you want to execute to the main thread.
        mustRun = true;
    }

    void initScreen()
    {
        display = GetComponent<Renderer>();

        Dictionary<string, string> shaderConfig = new Dictionary<string, string>();
        shader = ShaderScreen.Factory(display, 1, "crtlod", shaderConfig);
        scr.Init(system_skin);
        scr.ActivateShader(shader);
        scr.ClearBackground();
        scr.Clear();
        ShowInitialMessage();
        scr.DrawScreen();
    }
    /*
    void VerifyBasFile()
    {

        CopyBasFile();

        if (!File.Exists(debugbas))
        {
            BASFileNotFound = true;
        }
    }
    */

    void ShowNotFoundBasMessage()
    {
        scr.Clear();
        scr.PrintCentered(1, "------- ERROR ------", true);
        scr.ForegroundColorString = "red";
        scr.PrintCentered(3, $"{AGEBasicFileName} not found", true);
        scr.ResetForegroundColor();
        scr.Print("");
        scr.Print("To read the `test.log` file,");
        scr.Print("which contains the results of the");
        scr.Print("cabinet test, you'll need to copy a");
        scr.Print($"file named *{AGEBasicFileName}* into the");
        scr.Print("/AGEBasic folder.");
        scr.Print("Once it's in the right folder,");
        scr.Print($"the {AGEBasicFileName} program will");
        scr.Print("be executed automatically, allowing");
        scr.Print("the system to interpret and display");
        scr.Print("the log file's content without any");
        scr.Print("additional steps from you.");

        scr.DrawScreen();
    }

    void ShowInitialMessage()
    {
        scr.ForegroundColorString = "green";
        scr.PrintCentered(1, "TEST and DEBUG MODE", true);
        scr.ResetForegroundColor();

        scr.Print("");
        scr.Print("Copy a cabinet to test into the");
        scr.Print($"{ConfigManager.Cabinets} folder.");
        scr.Print("");
        scr.Print($"An AGEBasic program will show the test");
        scr.Print($"results in this computer (only errors are shown)");
        scr.Print("");
        scr.Print($"You can customize the program:");
        scr.Print($"*{AGEBasicFileName}* located in the");
        scr.Print($"/AGEBasic folder to show the results.");
        scr.Print("");
        scr.Print("The program will run again");
        scr.Print("automatically when you ");
        scr.Print("copy a new test.zip cabinet");
        scr.Print("into the /cabinets folder.");
        scr.Print("");
        scr.ForegroundColorString = "green";
        scr.Print("Waiting for a cabinet...");
        scr.ResetForegroundColor();
    }

    bool compile()
    {
        try
        {
            AGEBasic.ParseFile(debugbas);
        }
        catch (CompilationException ex)
        {
            ConfigManager.WriteConsoleException($"[CabinetDebugConsole] [{AGEBasicFileName}] compilation error", ex);
            scr.Clear();
            scr.PrintCentered(5, $"[{AGEBasicFileName}] compilation error:", true);
            scr.Print(0, 6, $"Line: {ex.LineNumber}");
            scr.Print(0, 7, ex.Message);
            scr.Print($"Exit the workshop, fix and replace the {AGEBasicFileName} program and start again.");
            scr.DrawScreen();
            return false;
        }
        catch (Exception ex)
        {
            ConfigManager.WriteConsoleException($"[ConfigurationController] [{AGEBasicFileName}]", ex);
            scr.Clear();
            scr.PrintCentered(5, "compilation error:", true);
            scr.Print(ex.Message);
            scr.Print($"Exit the workshop, fix and replace the {AGEBasicFileName} program and start again.");
            scr.DrawScreen();
            return false;
        }
        return true;
    }

    public IEnumerator CopyFile(string source, string destinationPath, Action<bool> onComplete)
    {

        ConfigManager.WriteConsole($"[CabinetDebugConsole.CopyFile] AGEBasic program: {source} -> {destinationPath}");

        UnityWebRequest request = UnityWebRequest.Get(source);
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            ConfigManager.WriteConsoleError($"[CabinetDebugConsole.CopyFile] Failed to load AGEBasic file: {request.error}");
            onComplete?.Invoke(false);
        }
        else
        {
            byte[] fileData = request.downloadHandler.data;
            File.WriteAllBytes(destinationPath, fileData);

            // Check if file exists and report success
            bool fileCopied = File.Exists(destinationPath);
            ConfigManager.WriteConsoleError($"[CabinetDebugConsole.CopyFile] copied? {fileCopied}");

            onComplete?.Invoke(fileCopied);
        }
    }

    //should run only if BAS file is present.
    IEnumerator run()
    {
        ConfigManager.WriteConsole("[CabinetDebugConsole.run] coroutine started.");
        if (!compile())
            yield return null;

        //listener on file changed
        fileMonitor.StartMonitor(ConfigManager.DebugDir, "test.log");
        fileMonitor.OnFileChanged.AddListener(OnFileChanged);
        
        //when the program ends.
        AGEBasic.OnProgramEnded.AddListener(OnProgramEnded);

        while (true)
        {
            if (mustRun && !AGEBasic.IsRunning(AGEBasicFileName))
                AGEBasic.Run(AGEBasicFileName, maxExecutionLinesAllowed: 0);
            yield return new WaitForSeconds(runEverySecs);
        }
    }

    private void OnFileCopyComplete(bool success)
    {
        BASFileFound = success;
        if (BASFileFound)
            StartCoroutine(run());
    }

    void OnProgramEnded(string program)
    {
        if (program == AGEBasicFileName)
            mustRun = false;
    }

}
