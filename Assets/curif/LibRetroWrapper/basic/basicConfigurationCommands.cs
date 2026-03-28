using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;
using AOJ.Managers;


public class ConfigurationCommands
{
    public ConfigurationController ConfigurationController = null;
    public LibretroControlMap ControlMap = null;
    public ScreenGenerator ScreenGenerator = null;
    public SceneDatabase SceneDatabase = null;
    public Teleportation Teleportation = null;
    //public DownloadManager DownloadManager = null;
    public UserLightManager UserLightManager = null; //lazy load.
    public Stack<double> Gosub = new Stack<double>();
    public double LineNumber; //in execution or parsing
    public AGEProgram ageProgram = null;
    public bool stop; //the program should stop
    public bool shutdown; //the program should stop and clear all events

    public double JumpTo; //line to jump in the next line run
    public double JumpNextTo; //jump to the next line of...
    public int JumpNextToMultiCommandIndex; //jump to the sentence of the multicommand in the line of...

    public CabinetsController CabinetsController;
    public GameRegistry GameRegistry;
    public Dictionary<string, forToStorage> ForToNext = new();

    public bool DebugMode = false;

    //actual cabinet.
    public Cabinet Cabinet;

    public CoinSlotController CoinSlot;

    public MoviePosterController PostersController;

    public AudioMixer audioMixer;

    //convenience player component
    public PlayerController Player;
    public EventManager EventManager;

    public GameObject PlayerGameObject;
    public XROrigin PlayerOrigin;

    public MusicPlayer MusicPlayerQueue;

    public basicAGE ageBasic;

    // The AudioSource on the cabinet's GameObject (guaranteed by RequireComponent on screen/cabinet controllers).
    public AudioSource AudioSource;

    // SID music player — attached to the same GameObject as AudioSource.
    public SIDPlayer SIDPlayer;

    // Populated by AGEBasicScreenController only. Null on AGEBasicCabinetController cabinets.
    public GameVideoPlayer VideoPlayer;

    // The game/CRT shader. Used by VIDEOSTOP to restore the game shader after video playback.
    public ShaderScreenBase GameShader;

    public string RunSubProgramPath = null;
    public int RunSubProgramLine = -1;

    public float SleepTime;

    public string ProgramPath;
    public string ProgramName;

    public Dictionary<string, BasicValueList> basicValueLists = new();

    // Determines how many lines of BASIC code execute per frame.
    // Default is 1 for legacy compatibility (approx 72 LPS on Quest).
    // Can be increased via SETCPU command inside the script.
    public double cpuPercentage = 1;

    //cabinet events
    public List<Event> events;

    // Cheat address lookup built from a mamecheat XML file (Pugsy's Cheats).
    // Maps cheat description (case-insensitive) → (region, offset, byteSize).
    // Null when no cheat XML was found in the cabinet folder.
    public Dictionary<string, CheatAddress> CheatAddresses = null;

    public LightGunTarget lightGunTarget;

    // Symbol table populated during parse. Maps uppercase variable names to slot indices in BasicVars.
    public Dictionary<string, int> VarIdMap = new();

    // File pointer array with 256 positions, initialized to null
    public AGEBasicUserFile[] filepointer = new AGEBasicUserFile[256];

    // Track when a control was last reported as active by CONTROLACTIVE for debouncing
    public Dictionary<string, float> lastControlActiveTime = new();

    public void CloseFiles()
    {
        for (int i = 0; i < filepointer.Length; i++)
        {
            if (filepointer[i] != null)
            {
                filepointer[i].file.Close();
                filepointer[i].file.Dispose();
                filepointer[i].file = null;
            }
        }
    }
}
