using UnityEngine;
using System;

[Serializable]
public class PlaceInformation
{
  public GameObject Place;
  public int MaxTimeSpentThere = 120;
  public int MinTimeSpentThere = 1;
  public float MinimalDistanceToReachObject = 1.5f;

  public PlaceInformation(GameObject place, int maxTimeSpentThere, int minTimeSpentThere, float minimalDistanceToReachObject)
  {
    this.Place = place;
    this.MaxTimeSpentThere = maxTimeSpentThere;
    this.MinTimeSpentThere = minTimeSpentThere;
    this.MinimalDistanceToReachObject = minimalDistanceToReachObject;
  }

  public DateTime getDateTimeUntilWait()
  {
    return DateTime.Now.AddSeconds(UnityEngine.Random.Range(MinTimeSpentThere, MaxTimeSpentThere));
  }
}
