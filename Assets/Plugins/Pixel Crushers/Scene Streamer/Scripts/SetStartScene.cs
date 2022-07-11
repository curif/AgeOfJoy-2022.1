using UnityEngine;
using System.Collections;

namespace PixelCrushers.SceneStreamer
{

	/// <summary>
	/// Sets the current scene at Start().
	/// </summary>
	[AddComponentMenu("Scene Streamer/Set Start Scene")]
	public class SetStartScene : MonoBehaviour 
	{

		/// <summary>
		/// The name of the scene to load at Start.
		/// </summary>
		[Tooltip("Load this scene at start")]
		public string startSceneName = "Scene 1";

		public void Start() 
		{
			SceneStreamer.SetCurrentScene(startSceneName);
			Destroy(this);
		}

	}

}
