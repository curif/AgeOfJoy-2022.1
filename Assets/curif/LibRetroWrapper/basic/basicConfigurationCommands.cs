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

    public float SleepTime;

    public Dictionary<string, BasicValueList> basicValueLists = new();

    // Determines how many lines of BASIC code execute per frame.
    // Default is 1 for legacy compatibility (approx 72 LPS on Quest).
    // Can be increased via SETCPU command inside the script.
    public double cpuPercentage = 1;

    //cabinet events
    public List<Event> events;

    public LightGunTarget lightGunTarget;

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
