using UnityEngine;
using System;

[Serializable]
public class PlaceInformation
{
  public GameObject Place;
  public int MaxTimeSpentThere = 120;
  public int MinTimeSpentThere = 1;
  public float MinimalDistanceToReachObject = 1.5f;
  [SerializeField]
  public PlaceType Type;

  public enum PlaceType
  {
    Generic=0, VendingMachine, ArcadeMachine, BoyPlay
  };


  public PlaceInformation(GameObject place, int maxTimeSpentThere, int minTimeSpentThere,
                          float minimalDistanceToReachObject, PlaceType type = PlaceType.Generic)
  {
    this.Place = place;
    this.MaxTimeSpentThere = maxTimeSpentThere;
    this.MinTimeSpentThere = minTimeSpentThere;
    this.MinimalDistanceToReachObject = minimalDistanceToReachObject;
    this.Type = type;
  }

  public DateTime getWaitingDateTime()
  {
    return DateTime.Now.AddSeconds(UnityEngine.Random.Range(MinTimeSpentThere, MaxTimeSpentThere));
  }
}
