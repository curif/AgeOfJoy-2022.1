using UnityEngine;
using System.Collections.Generic;

public class RandomGuy : MonoBehaviour
{
    [System.Serializable]
    public class MaterialVariant
    {
        public GameObject targetObject;
        public List<Material> alternateMaterials = new List<Material>();
    }

    [Header("Customize RandomGuy Parts")]
    [SerializeField] private List<GameObject> hair = new List<GameObject>();
    [SerializeField] private List<GameObject> eyeglasses = new List<GameObject>();
    [SerializeField] private List<GameObject> eyebrows = new List<GameObject>();
    [SerializeField] private List<GameObject> facialHair = new List<GameObject>();

    [Header("Material Variants")]
    [SerializeField] private List<MaterialVariant> materialVariants = new List<MaterialVariant>();

    private void Start()
    {
        RandomizeCategory(hair);
        RandomizeCategory(eyeglasses);
        RandomizeCategory(eyebrows);
        RandomizeCategory(facialHair);
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

                if (shouldBeActive)
                {
                    TryApplyRandomMaterial(obj);
                }
            }
        }
    }

    private void TryApplyRandomMaterial(GameObject obj)
    {
        foreach (MaterialVariant variant in materialVariants)
        {
            if (variant.targetObject == obj && obj.TryGetComponent(out Renderer rend))
            {
                Material defaultMat = rend.sharedMaterial;
                List<Material> options = new List<Material> { defaultMat };
                options.AddRange(variant.alternateMaterials);

                int selected = UnityEngine.Random.Range(0, options.Count);
                rend.material = options[selected];
            }
        }
    }
}
