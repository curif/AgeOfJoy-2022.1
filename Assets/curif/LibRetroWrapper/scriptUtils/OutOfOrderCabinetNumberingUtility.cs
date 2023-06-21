#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class OutOfOrderCabinetNumberingUtility : Editor
{
    [MenuItem("Custom/Change Marquee Texture")]
    static void ChangeMarqueeTexture()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("No game object selected!");
            return;
        }

        // Loop over the selected object's children
        int childCount = selectedObject.transform.childCount;
        int maxChildren = Mathf.Min(childCount, 15); // Limit to a maximum of 15 children
        for (int i = 0; i < maxChildren; i++)
        {
            Transform child = selectedObject.transform.GetChild(i);
            GameObject cabinet = child.gameObject;

            // Find the marquee child object
            Transform marquee = cabinet.transform.Find("marquee");
            if (marquee != null)
            {
                // Get the texture name with leading zeros
                string textureName = "Cabinets/OutOfOrder/CabinetNumbers/OutOfOrderMarquee " + (i + 1).ToString("D2");

                // Load the new texture from Resources folder
                Texture2D newTexture = Resources.Load<Texture2D>(textureName);
                if (newTexture != null)
                {
                    // Get the material from the marquee object
                    Renderer marqueeRenderer = marquee.GetComponent<Renderer>();
                    if (marqueeRenderer != null)
                    {
                        Material marqueeMaterial = marqueeRenderer.sharedMaterial; // Use sharedMaterial instead of material
                        if (marqueeMaterial != null)
                        {
                            // Assign the new texture to the material's MainTexture property
                            marqueeMaterial.SetTexture("_MainTex", newTexture);
                        }
                    }
                }
                else {
                    Debug.LogError($"texture not found idx: {i} name: {textureName}");
                }
            }
            else {
                Debug.LogError($"marquee not found idx: {i}");
            }
        }

        // Mark scene as dirty to save changes
        EditorUtility.SetDirty(selectedObject);
    }
}

#endif
