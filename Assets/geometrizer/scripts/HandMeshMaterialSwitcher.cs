using UnityEngine;

public class HandMeshMaterialSwitcher : MonoBehaviour
{
    public Material lightMaterial;
    public Material darkMaterial;

    [SerializeField, Tooltip("Drag the child object with the Renderer component here.")]
    private Renderer handRenderer;

    public void SetLightMaterial()
    {
        if (handRenderer != null)
        {
            Debug.Log("Setting light material");
            handRenderer.material = lightMaterial;
        }
        else
        {
            Debug.LogError("Hand renderer not assigned.");
        }
    }

    public void SetDarkMaterial()
    {
        if (handRenderer != null)
        {
            Debug.Log("Setting dark material");
            handRenderer.material = darkMaterial;
        }
        else
        {
            Debug.LogError("Hand renderer not assigned.");
        }
    }
}
