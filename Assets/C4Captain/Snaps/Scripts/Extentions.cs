using UnityEngine;

namespace C4Captain.Utils
{
	public static class Extentions
	{
		/// <summary>
		/// Get bounds of a single gameobject or a group of gameobjects with MeshRenderer attached or any type of collider attached
		/// </summary>
		/// <param name="original">The parent object</param>
		/// <param name="expand">optional param to expand the size of the bounds if you want to</param>
		/// <returns></returns>
		public static Bounds GetBounds (this Component original, float expand = 0f)
		{
			Vector3 groupVectors = Vector3.zero;
			Vector3 groupCenter = Vector3.zero;

			Transform[] transforms = original.GetComponentsInChildren<Transform> ();

			foreach (Transform t in transforms)
			{
				groupVectors += t.position;
			}
			groupCenter = groupVectors / transforms.Length;

			Bounds bounds = new Bounds (groupCenter, Vector3.zero);

			foreach (Transform t in transforms)
			{
				if (t.TryGetComponent (out MeshRenderer meshRenderer)) bounds.Encapsulate (meshRenderer.bounds);
				else if (t.TryGetComponent (out Collider collider)) bounds.Encapsulate (collider.bounds);
			}
			bounds.Expand (expand);
			return bounds;
		}
	}
}