using UnityEngine;

namespace PixelCrushers.SceneStreamer
{

	/// <summary>
	/// Add this to the scene's root object. It lists the scene's neighbors.
	/// SceneStreamer uses it to determine which neighbors to load and unload.
	/// If the scene's root object doesn't have this component, SceneStreamer 
	/// will generate it automatically at load time, which takes a little time.
	/// </summary>
	[AddComponentMenu("Scene Streamer/Neighboring Scenes")]
	public class NeighboringScenes : MonoBehaviour
	{

		/// <summary>
		/// The scene's neighbors.
		/// </summary>
		[Tooltip("This scene's neighbors")]
		public string[] sceneNames;

	}

}