using System;
using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;


public class ConfigurationCommands
{
    public ConfigurationController ConfigurationController = null;
    public LibretroControlMap ControlMap = null;
    public ScreenGenerator ScreenGenerator = null;
    public SceneDatabase SceneDatabase = null;
    public Teleportation Teleportation = null;

    public Stack<int> Gosub = new Stack<int>();
    public int LineNumber; //in execution
    public bool stop; //the program should stop

    public int JumpTo; //line to jump in the next line run
    public int JumpNextTo; //jump to the next line of...

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
    public GameObject PlayerGameObject;
    public XROrigin PlayerOrigin;

    public MusicPlayer MusicPlayerQueue;

    public float SleepTime;
}
