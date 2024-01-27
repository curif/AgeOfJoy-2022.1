using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;


public class ConfigurationCommands
{
    public ConfigurationController ConfigurationController = null;
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
    public GameObject Cabinet;

    public CoinSlotController CoinSlot;

    public MoviePosterController PostersController;

    public AudioMixer audioMixerAmbience; // Drag your Audio Mixer asset here in the Unity Editor
    public AudioMixer audioMixerGame; // Drag your Audio Mixer asset here in the Unity Editor

}
