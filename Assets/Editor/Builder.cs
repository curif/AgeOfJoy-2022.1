using UnityEditor;
using System;
using System.Linq;

class AndroidBuilder
{
    static void Build()
    {
        // Default scenes
        string[] defaultScenes = new[]
        {
            "Assets/Scenes/IntroGallery.unity",
            "Assets/Scenes/HallwayToPolybius.unity",
            "Assets/Scenes/HallwayToWorkshop.unity",
            "Assets/Scenes/PolybiusRoom.unity",
            "Assets/Scenes/HallwayToWorkshop.unity",
            "Assets/Scenes/workshop.unity",
            "Assets/Scenes/Room001.unity",
            "Assets/Scenes/Room002.unity",
            "Assets/Scenes/Room003.unity",
            "Assets/Scenes/Room004.unity"
        };

        // Get command-line arguments
        string[] args = Environment.GetCommandLineArgs();

        // Check for custom scenes
        string customScenesArg = args.FirstOrDefault(arg => arg.StartsWith("-customScenes"));
        string[] scenes = defaultScenes;

        if (customScenesArg != null)
        {
            string customScenes = customScenesArg.Split('=')[1];
            scenes = customScenes.Split(',');
        }

        string buildPath = "Build";
        if (args.FirstOrDefault(arg => arg.StartsWith("-outputPath")) != null)
        {
            string outputPathArg = args.FirstOrDefault(arg => arg.StartsWith("-outputPath"));
            buildPath = outputPathArg.Split('=')[1];
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = $"{buildPath}/AgeOfJoy.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}