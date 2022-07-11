using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.SceneStreamer
{

	/// <summary>
	/// This trigger handler tells SceneStreamer about a neighboring scene. You'll generally
	/// add it to a trigger collider at the edge of a scene. When the player enters the edge,
	/// for example when entering the edge coming from a neighboring scene, the edge's scene
	/// is made the current scene.
	/// </summary>
	[AddComponentMenu("Scene Streamer/Scene Edge")]
	public class SceneEdge : MonoBehaviour
	{

		/// <summary>
		/// The current scene root.
		/// </summary>
		[Tooltip("The root GameObject of this scene")]
		public GameObject currentSceneRoot;

		/// <summary>
		/// The name of the next scene on the other side of the edge.
		/// </summary>
		[Tooltip("The name of the next scene on the other side of the edge")]
		public string nextSceneName;

		public List<string> acceptedTags = new List<string>() { "Player" };

		/// <summary>
		/// When the player enters this edge (for example coming in from a neighbor),
		/// makes sure to set the current scene to this edge's scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public void OnTriggerEnter(Collider other) 
		{
			CheckEdge(other.tag);
		}

		/// <summary>
		/// When the player enters this edge (for example coming in from a neighbor),
		/// makes sure to set the current scene to this edge's scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public void OnTriggerEnter2D(Collider2D other)
		{

			CheckEdge(other.tag);
		}

		private void CheckEdge(string otherTag)
		{
			if (acceptedTags == null || acceptedTags.Count == 0 || acceptedTags.Contains(otherTag))
			{
				SetCurrentScene();
			}
		}

		private void SetCurrentScene()
		{
			if (currentSceneRoot) SceneStreamer.SetCurrentScene(currentSceneRoot.name);
		}

	}

}