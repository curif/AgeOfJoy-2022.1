using UnityEngine;

public class ForwardDirectionGizmo : MonoBehaviour
{
	public float arrowLength = 0.6f; // Adjust the length of the arrow
	public Color arrowColor = Color.red; // Set the desired color for the arrow

	private void OnDrawGizmosSelected() // Use OnDrawGizmos instead of OnDrawGizmosSelected
	{
		//if (!Application.isPlaying) // Only draw in Editor mode
		
			Gizmos.color = arrowColor;
			Vector3 forward = transform.forward * arrowLength;
			Gizmos.DrawLine(transform.position, transform.position + forward);

			// Optional: Draw an arrowhead for better visualization
			Gizmos.DrawRay(transform.position + forward, Vector3.up * 0.3f); // Adjust size of arrowhead
		
	}
}
