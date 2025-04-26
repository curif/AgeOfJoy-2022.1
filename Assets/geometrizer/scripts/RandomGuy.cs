using UnityEngine;
using System.Collections.Generic;

public class RandomGuy : MonoBehaviour
{
    [System.Serializable]
    public class MaterialSwap
    {
        public GameObject targetObject;
        public List<Material> alternateMaterials = new List<Material>();
    }

    [Header("Customize RandomGuy Parts")]
    [SerializeField] private List<GameObject> hair = new List<GameObject>();
    [SerializeField] private List<GameObject> eyeglasses = new List<GameObject>();
    [SerializeField] private List<GameObject> eyebrows = new List<GameObject>();
    [SerializeField] private List<GameObject> facialHair = new List<GameObject>();

    [Header("Optional Material Swaps")]
    [SerializeField] private List<MaterialSwap> materialSwaps = new List<MaterialSwap>();

    [Header("Random Hair Colors")]
    [SerializeField] private List<Color> randomHairColors = new List<Color>();

    [Header("Random Scale")]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1f;

    private void Start()
    {
        RandomizeCategory(hair);
        RandomizeCategory(eyeglasses);
        RandomizeCategory(eyebrows);
        RandomizeCategory(facialHair);

        ApplyMaterialSwaps();
        ApplyHairColorIfNeeded();
        ApplyRandomScale();
    }

    private void RandomizeCategory(List<GameObject> list)
    {
        List<GameObject> validObjects = list.FindAll(obj => obj != null);
        int count = validObjects.Count;
        int indexToShow = (count > 0) ? UnityEngine.Random.Range(0, count + 1) : -1;

        foreach (GameObject obj in list)
        {
            if (obj != null)
            {
                bool shouldBeActive = (validObjects.IndexOf(obj) == indexToShow);
                obj.SetActive(shouldBeActive);
            }
        }
    }

    private void ApplyMaterialSwaps()
    {
        foreach (MaterialSwap swap in materialSwaps)
        {
            if (swap.targetObject == null || !swap.targetObject.activeInHierarchy)
                continue;

            Renderer renderer = swap.targetObject.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Material originalMaterial = renderer.sharedMaterial;
            List<Material> options = new List<Material> { originalMaterial };
            options.AddRange(swap.alternateMaterials.FindAll(mat => mat != null));

            int choice = UnityEngine.Random.Range(0, options.Count);
            renderer.material = options[choice];
        }
    }

    private void ApplyHairColorIfNeeded()
    {
        List<GameObject> activeHair = hair.FindAll(h => h != null && h.activeInHierarchy);
        List<GameObject> activeFacialHair = facialHair.FindAll(f => f != null && f.activeInHierarchy);
        if ((activeHair.Count > 0 || activeFacialHair.Count > 0) && randomHairColors.Count > 0)
        {
            Color chosenColor = randomHairColors[UnityEngine.Random.Range(0, randomHairColors.Count)];
            foreach (GameObject obj in activeHair)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                    renderer.material.color = chosenColor;
            }
            foreach (GameObject obj in activeFacialHair)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                    renderer.material.color = chosenColor;
            }
        }
    }

    private void ApplyRandomScale()
    {
        if (minScale > maxScale)
        {
            float temp = minScale;
            minScale = maxScale;
            maxScale = temp;
        }
        float randomValue = UnityEngine.Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * randomValue;
    }
}
