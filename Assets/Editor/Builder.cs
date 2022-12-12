using UnityEditor;
class AndroidBuilder
{
  static void Build()
  {
    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    buildPlayerOptions.scenes = new[] { "Assets/Scenes/IntroGallery.unity", "Assets/Scenes/HallwayToPolybius.unity",
      "Assets/Scenes/HallwayToWorkshop.unity", "Assets/Scenes/PolybiusRoom.unity", "Assets/Scenes/HallwayToWorkshop.unity",
      "Assets/Scenes/workshop.unity",
      "Assets/Scenes/Room001.unity",
      "Assets/Scenes/Room002.unity",
      "Assets/Scenes/Room003.unity",
      "Assets/Scenes/Room004.unity",
    };
    buildPlayerOptions.locationPathName = "/home/fabio.curi/desarr/AgeOfJoy-2022.1/AgeOfJoy.apk";
    buildPlayerOptions.target = BuildTarget.Android;
    buildPlayerOptions.options = BuildOptions.None;
    BuildPipeline.BuildPlayer(buildPlayerOptions);
  }
}