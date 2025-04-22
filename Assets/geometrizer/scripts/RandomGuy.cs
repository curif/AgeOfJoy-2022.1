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

    private void Start()
    {
        RandomizeCategory(hair);
        RandomizeCategory(eyeglasses);
        RandomizeCategory(eyebrows);
        RandomizeCategory(facialHair);

        ApplyMaterialSwaps();
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
}
