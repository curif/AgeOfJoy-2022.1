using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using C4Captain.Utils;

namespace C4Captain.Editor
{
	public sealed class Snaps : EditorWindow
	{
		public enum SnapType
		{
			None,
			MeshBounds,
			ColliderBounds,
			GroupBounds
		}

		public enum Center
		{
			Pivot,
			BoundsCenter
		}

		Center _center;
		Space _coordinateSpace;
		SnapType _snapType;

		[MenuItem ("Tools/Snaps/Open")]
		static void Open ()
		{
			GetWindow<Snaps> ("Snaps");
		}

		void OnGUI ()
		{
			//draw required fields
			_center = (Center)EditorGUILayout.EnumPopup ("Center", _center);
			_coordinateSpace = (Space)EditorGUILayout.EnumPopup ("Coordinate Space", _coordinateSpace);
			_snapType = (SnapType)EditorGUILayout.EnumPopup ("Snap Type", _snapType);

			//caching the default color
			Color defaultColor = GUI.backgroundColor;

			//draw snap buttons
			EditorGUILayout.BeginVertical ();
			{
				EditorGUILayout.BeginHorizontal ();
				{
					GUI.backgroundColor = Color.red;
					if (GUILayout.Button ("+X")) Snap (Vector3.right, _center, _coordinateSpace, _snapType);
					GUI.backgroundColor = Color.green;
					if (GUILayout.Button ("+Y")) Snap (Vector3.up, _center, _coordinateSpace, _snapType);
					GUI.backgroundColor = Color.blue;
					if (GUILayout.Button ("+Z")) Snap (Vector3.forward, _center, _coordinateSpace, _snapType);
				}
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				{
					GUI.backgroundColor = Color.red;
					if (GUILayout.Button ("-X")) Snap (Vector3.left, _center, _coordinateSpace, _snapType);
					GUI.backgroundColor = Color.green;
					if (GUILayout.Button ("-Y")) Snap (Vector3.down, _center, _coordinateSpace, _snapType);
					GUI.backgroundColor = Color.blue;
					if (GUILayout.Button ("-Z")) Snap (Vector3.back, _center, _coordinateSpace, _snapType);
					GUI.backgroundColor = defaultColor;
				}
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical ();
			GUI.enabled = false;
			EditorGUILayout.LabelField ("Selection count: " + Selection.objects.Length);
			GUI.enabled = true;
		}

		[Shortcut ("Tools/Snaps/Front", KeyCode.Keypad8, ShortcutModifiers.Alt)]
		static void Forward () => Snap (Vector3.forward);

		[Shortcut ("Tools/Snaps/Back", KeyCode.Keypad2, ShortcutModifiers.Alt)]
		static void Back () => Snap (Vector3.back);

		[Shortcut ("Tools/Snaps/Left", KeyCode.Keypad4, ShortcutModifiers.Alt)]
		static void Left () => Snap (Vector3.left);

		[Shortcut ("Tools/Snaps/Right", KeyCode.Keypad6, ShortcutModifiers.Alt)]
		static void Right () => Snap (Vector3.right);

		[Shortcut ("Tools/Snaps/Top", KeyCode.Keypad5, ShortcutModifiers.Alt)]
		static void Up () => Snap (Vector3.up);

		[Shortcut ("Tools/Snaps/Bottom", KeyCode.Keypad0, ShortcutModifiers.Alt)]
		static void Down () => Snap (Vector3.down);

		public static void Snap (Vector3 worldDir, Center center = Center.Pivot, Space space = Space.World, SnapType snapType = SnapType.None)
		{
			GameObject[] selected = Selection.gameObjects;
			int selectedLength = selected.Length;
			for (int i = 0; i < selectedLength; i++)
			{
				GameObject go = selected[i];

				//find the origin
				Vector3 origin = center == Center.Pivot ? go.transform.position : go.transform.GetBounds ().center;

				//find the direction
				Vector3 direction = space == Space.World ? worldDir : FindLocalDirection (worldDir, go);

				//casts multiple rays from origin to the desired direction
				//from which we will find the closest RaycastHit
				RaycastHit[] hits = Physics.RaycastAll (origin, direction);
				RaycastHit closestHit = new RaycastHit ();
				float closestDistance = Mathf.Infinity;

				//in case of GroupBounds we have to ignore childrens colliders
				//otherwise it will not work as intended
				if (snapType == SnapType.GroupBounds)
				{
					List<RaycastHit> tempHits = new List<RaycastHit> (hits);
					foreach (Transform tChild in go.GetComponentInChildren<Transform> ())
					{
						tempHits.RemoveAll (x => x.transform == tChild);
					}
					hits = tempHits.ToArray ();
				}

				//iterate through all the raycasts and find the closest RaycastHit
				int hitsLength = hits.Length;
				for (int j = 0; j < hitsLength; j++)
				{
					float distance = Vector3.Distance (origin, hits[j].point);

					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestHit = hits[j];
					}
				}

				switch (snapType)
				{
					default:
					case SnapType.None:
					Undo.RecordObject (go.transform, "Position Snapped");
					go.transform.position = closestHit.point;
					break;

					case SnapType.MeshBounds:
					if (go.TryGetComponent (out MeshRenderer meshRenderer))
					{
						Undo.RecordObject (go.transform, "Position Snapped");
						meshRenderer.transform.position += FindOffset (direction, closestHit.point, meshRenderer.bounds);
					}
					else
					{
						Debug.LogError ("Trying to snap with MeshBounds but no MeshRenderer found on : " + go.name, go);
					}
					break;

					case SnapType.ColliderBounds:
					if (go.TryGetComponent (out Collider collider))
					{
						Undo.RecordObject (go.transform, "Position Snapped");
						collider.transform.position += FindOffset (direction, closestHit.point, collider.bounds);
					}
					else
					{
						Debug.LogError ("Trying to snap with ColliderBounds but no Collider found on : " + go.name, go);
					}
					break;

					case SnapType.GroupBounds:
					Undo.RecordObject (go.transform, "Position Snapped");
					go.transform.position += FindOffset (direction, closestHit.point, go.transform.GetBounds ());
					break;
				}
			}
		}

		static Vector3 FindLocalDirection (Vector3 original, GameObject go)
		{
			if (original == Vector3.right) return go.transform.right;
			else if (original == Vector3.left) return go.transform.right * -1f;
			else if (original == Vector3.up) return go.transform.up;
			else if (original == Vector3.down) return go.transform.up * -1f;
			else if (original == Vector3.forward) return go.transform.forward;
			else if (original == Vector3.back) return go.transform.forward * -1f;
			else return Vector3.zero;
		}

		static Vector3 FindOffset (Vector3 original, Vector3 hitPoint, Bounds bound)
		{
			if (original == Vector3.right) return new Vector3 (hitPoint.x - bound.max.x, 0, 0);
			else if (original == Vector3.left) return new Vector3 (hitPoint.x - bound.min.x, 0, 0);
			else if (original == Vector3.up) return new Vector3 (0, hitPoint.y - bound.max.y, 0);
			else if (original == Vector3.down) return new Vector3 (0, hitPoint.y - bound.min.y, 0);
			else if (original == Vector3.forward) return new Vector3 (0, 0, hitPoint.z - bound.max.z);
			else if (original == Vector3.back) return new Vector3 (0, 0, hitPoint.z - bound.min.z);
			else return Vector3.zero;
		}
	}
}