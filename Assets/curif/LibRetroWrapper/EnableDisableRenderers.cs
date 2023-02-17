using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableRenderers : MonoBehaviour
{
  [SerializeField]
  private bool isEnabled = true; // This is the private variable that will hold the value of the property

  public bool IsEnabled // This is the public property that will be shown in the editor, but can only be read
  {
      get { return isEnabled; }
  }

  public void SetRenderers(bool enableRenderers)
  {
    if (enableRenderers == isEnabled)
      return;

    isEnabled = enableRenderers;

    SetRenderersObj(gameObject, enableRenderers);
  }

  private void SetRenderersObj(GameObject obj, bool enableRenderers)
  {
    // Recursive function to disable all Renderer components of a GameObject and its children
    // Get the Renderer component of the GameObject, if it exists
    Renderer renderer = obj.GetComponent<Renderer>();

    // If a Renderer component was found, disable it
    if (renderer != null)
    {
        renderer.enabled = enableRenderers;
    }

    // Traverse all child GameObjects and disable their Renderer components
    foreach (Transform child in obj.transform)
    {
        SetRenderersObj(child.gameObject, enableRenderers);
    }
  }
}
