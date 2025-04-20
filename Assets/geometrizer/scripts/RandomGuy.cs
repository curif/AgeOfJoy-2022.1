using UnityEngine;
using System.Collections.Generic;

public class RandomGuy : MonoBehaviour
{
    [Header("Customize RandomGuy Parts")]
    [SerializeField] private List<GameObject> hair = new List<GameObject>();
    [SerializeField] private List<GameObject> eyeglasses = new List<GameObject>();
    [SerializeField] private List<GameObject> eyebrows = new List<GameObject>();
    [SerializeField] private List<GameObject> facialHair = new List<GameObject>();

    private void Start()
    {
        RandomizeCategory(hair);
        RandomizeCategory(eyeglasses);
        RandomizeCategory(eyebrows);
        RandomizeCategory(facialHair);
    }

    private void RandomizeCategory(List<GameObject> list)
    {
        // Filter out nulls
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
}
